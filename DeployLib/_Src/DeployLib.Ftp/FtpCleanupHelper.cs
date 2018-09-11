using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using FluentFTP;

namespace DeployLib.Ftp
{
	public interface IFtpCleanupHelper : IDisposable
	{
		event EventHandler<string> DidCleanup;
		event EventHandler<string> Error;

		Task DeleteNotContainedFiles(LastRunConfig lastrun, List<FileInfo> files);
		Task CleanupRemoteAsync();
	}

	public class FtpCleanupHelper : FluentFtpHolder, IFtpCleanupHelper
	{
		public event EventHandler<string> DidCleanup;
		public event EventHandler<string> Error;

		public FtpCleanupHelper(TargetFtpConfig targetFtp)
			: base(targetFtp)
		{
		}

		public async Task DeleteNotContainedFiles(LastRunConfig lastRun, List<FileInfo> files)
		{
			var root = IRootHelper.GetRoot();

			var currentFiles =
				files
					.Select(x => root.GetRealtive(x))
					.ToList();

			var remoteFilesToRemove = lastRun.Files.Except(currentFiles);

			var ftpClient = GetFtpClient();

			foreach (var remoteFile in remoteFilesToRemove)
			{
				try
				{
					var remoteUrl = GetRemoteUrl(remoteFile);
					await ftpClient.DeleteFileAsync(remoteUrl);
					DidCleanup?.Invoke(this, remoteFile);
				}
				catch (FtpException)
				{
					Error?.Invoke(this, remoteFile);
					continue;
				}
			}
		}

		public async Task CleanupRemoteAsync()
		{
			var ftpClient = GetFtpClient();
			var root = GetRemoteUrl("/");
			var remoteItems = await ftpClient.GetListingAsync(root);
			foreach (var item in remoteItems)
			{
				if (item.Type == FtpFileSystemObjectType.File)
					await ftpClient.DeleteFileAsync(item.FullName);

				else if (item.Type == FtpFileSystemObjectType.Directory)
					await ftpClient.DeleteDirectoryAsync(item.FullName, FtpListOption.Recursive);
			}
		}
	}
}
