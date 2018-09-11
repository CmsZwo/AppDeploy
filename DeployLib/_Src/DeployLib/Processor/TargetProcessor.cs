using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Unity.Attributes;

using DeployLib.Ftp;

namespace DeployLib
{
	public interface ITargetProcessor : ICommandProcessor<TargetCommand>
	{
		Task ApplyCurrentStateAsync(TargetCommand command);
		Task ResetState(TargetCommand command);
	}

	public class TargetProcessor : CommandProcessor<TargetCommand>, ITargetProcessor
	{
		#region Inject

		[Dependency]
		public IRootHelper IRootHelper { get; set; }

		[Dependency]
		public IOutputConsole IOutputConsole { get; set; }

		[Dependency]
		public IConfigHelper IConfigHelper { get; set; }

		[Dependency]
		public IFtpFactory IFtpFactory { get; set; }

		[Dependency]
		public IOkProcessor IOkProcessor { get; set; }

		[Dependency]
		public IFileEnumerator IFileEnumerator { get; set; }

		#endregion

		#region Tools .lastrun

		private const string _LastRunFileName = ".lastrun";

		private async Task<LastRunConfig> ReadLastRunAsync(IFtpClient ftpClient)
		{
			var json = await ftpClient.DownloadStringAsync(_LastRunFileName);
			var result = json.ToObjectByJson<LastRunConfig>();
			return result;
		}

		private async Task WriteLastRunAsync(IFtpClient ftpClient, List<FileInfo> files)
		{
			var root = IRootHelper.GetRoot();

			var lastRun = new LastRunConfig
			{
				Dt = DateTime.Now,

				Files =
					files
						.Select(x => root.GetRealtive(x))
						.ToList()
			};

			var json = lastRun.ToJsonWithTypeInformation();

			await ftpClient.UploadStringAsync(_LastRunFileName, json);
		}

		private bool AppliesToModified(FileInfo file, DateTime? modifiedSince)
		{
			file.Refresh();

			var lastModified = file.LastWriteTime;
			if (file.CreationTime > lastModified)
				lastModified = file.CreationTime;

			if (lastModified > modifiedSince)
				return true;

			return false;
		}

		#endregion

		#region Tools new files

		private List<FileInfo> GetNewFiles(LastRunConfig lastRun, List<FileInfo> files)
		{
			var result = new List<FileInfo>();

			var root = IRootHelper.GetRoot();
			var lastFiles = lastRun.Files.ToHashSet();

			foreach (var file in files)
			{
				var relative = root.GetRealtive(file);

				if (lastFiles.Contains(relative))
					continue;

				result.Add(file);
			}

			return result;
		}

		private List<FileInfo> GetFiles(TargetConfig target)
			=> IFileEnumerator.GetFiles(target);

		private TargetFtpConfig CreateTargetFtp(TargetCommand command, FtpConfig ftp)
			=>
			new TargetFtpConfig
			{
				Host = ftp.Host,
				Port = ftp.Port,
				User = ftp.User,
				Password = ftp.Password,

				Name = ftp.Name,
				Directory = command.Target.Directory
			};

		#endregion

		public override async Task Process(TargetCommand command)
		{
			foreach (var ftp in command.Target.Ftp)
			{
				var targetFtp = CreateTargetFtp(command, ftp);

				using (var ftpClient = IFtpFactory.Client(targetFtp))
				{
					var lastRun = await ReadLastRunAsync(ftpClient);
					var modifiedSince = lastRun?.Dt;

					var localFiles = GetFiles(command.Target);
					var filesToUpload = localFiles;

					if (modifiedSince.HasValue)
						filesToUpload =
							localFiles
								.Where(x => AppliesToModified(x, modifiedSince))
								.ToList();

					if (lastRun != null)
					{
						var remoteFilesHash =
							lastRun
								.Files
								.ToHashSet();

						var root = IRootHelper.GetRoot();

						foreach (var file in localFiles)
						{
							var remotePath = root.GetRealtive(file);
							if (remoteFilesHash.Contains(remotePath))
								continue;

							if (!filesToUpload.Any(x => x.FullName == file.FullName))
								filesToUpload.Add(file);
						}
					}

					IOutputConsole.WriteLine($"Target [{command.Value}/{targetFtp.Name}]");

					if (!filesToUpload.HasContent())
					{
						PrintNoFiles();
					}
					else
					{
						IOutputConsole.WriteLine($"found {filesToUpload.Count} files");
						await UploadFilesAsync(targetFtp, filesToUpload);
					}

					if (!command.Target.CleanupDisable)
						await CleanupAsync(targetFtp, lastRun, localFiles);

					await UploadLastRunAsync(ftpClient, localFiles);

					PrintFinished();

					if (command.Target.OK.HasContent())
						await IOkProcessor.Process(new OkCommand { Url = command.Target.OK });
				}
			}
		}

		private async Task UploadLastRunAsync(IFtpClient ftpClient, List<FileInfo> files)
		{
			IOutputConsole.ClearLine();
			IOutputConsole.Write("uploading .LastRun");
			await WriteLastRunAsync(ftpClient, files);
		}

		private void PrintNoFiles()
		{
			IOutputConsole.WriteLine("no files to upload");
			IOutputConsole.WriteLine("");
		}

		private void PrintFinished()
		{
			IOutputConsole.ClearLine();
			IOutputConsole.WriteLine("finished");
			IOutputConsole.WriteLine("");
		}

		private async Task<List<FileInfo>> UploadFilesAsync(TargetFtpConfig targetFtp, List<FileInfo> files)
		{
			var result = new List<FileInfo>();

			var worker = new FtpWorker(targetFtp);
			Container.Shared.Inject(worker);
			worker.Enqueue(files.ToArray());

			var root = IRootHelper.GetRoot();

			worker.FileWillUpload += (sender, a) =>
			{
				lock (this)
				{
					IOutputConsole.ClearLine();
					IOutputConsole.Write($"uploading {a.FilesLeft} {root.GetRealtive(a.File)}");
				}
			};

			worker.FileDidUpload += (sender, a) =>
			{
				lock (result)
				{
					result.Add(a.File);
				}
			};

			worker.FileError += (sender, a) =>
			{
				lock (this)
				{
					IOutputConsole.ClearLine();
					IOutputConsole.WriteLine($"error: {root.GetRealtive(a.File)}, [{a.Message}]");
				}
			};

			await worker.UploadAsync();
			return result;
		}

		public async Task ApplyCurrentStateAsync(TargetCommand command)
		{
			var files = GetFiles(command.Target);

			foreach (var ftp in command.Target.Ftp)
			{
				var targetFtp = CreateTargetFtp(command, ftp);

				using (var ftpClient = IFtpFactory.Client(targetFtp))
				{
					await UploadLastRunAsync(ftpClient, files);
					IOutputConsole.ClearLine();
					IOutputConsole.WriteLine($"Apply current state [{targetFtp.Name}] OK.");
					IOutputConsole.WriteLine("");
				}
			}
		}

		public async Task ResetState(TargetCommand command)
		{
			foreach (var ftp in command.Target.Ftp)
			{
				var targetFtp = CreateTargetFtp(command, ftp);

				using (var ftpClient = IFtpFactory.Client(targetFtp))
				{
					await ftpClient.DeleteAsync(_LastRunFileName);
					IOutputConsole.ClearLine();
					IOutputConsole.WriteLine($"Reset state [{targetFtp.Name}] OK.");
					IOutputConsole.WriteLine("");
				}
			}
		}

		private async Task CleanupAsync(TargetFtpConfig targetFtp, LastRunConfig lastRun, List<FileInfo> files)
		{
			if (lastRun == null)
				return;

			using (var helper = IFtpFactory.CleanupHelper(targetFtp))
			{
				IOutputConsole.ClearLine();
				IOutputConsole.Write("cleanup...");

				helper.DidCleanup += (sender, remoteFile) =>
				{
					IOutputConsole.ClearLine();
					IOutputConsole.Write($"cleanup [{remoteFile}]");
				};

				helper.Error += (sender, remoteFile) =>
				{
					IOutputConsole.ClearLine();
					IOutputConsole.Write($"error [{remoteFile}]");
				};

				await helper.DeleteNotContainedFiles(lastRun, files);
			}
		}
	}
}
