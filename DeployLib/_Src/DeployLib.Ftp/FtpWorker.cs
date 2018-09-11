using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Unity.Attributes;

namespace DeployLib.Ftp
{
	public interface IFtpWorker
	{
		event EventHandler<FtpEventArgs> FileWillUpload;
		event EventHandler<FtpEventArgs> FileDidUpload;

		void Enqueue(FileInfo file);
		void Enqueue(FileInfo[] files);

		Task Upload(FileInfo file);

		Task UploadAsync();
	}

	public class FtpEventArgs
	{
		public FileInfo File { get; set; }
		public int FilesLeft { get; set; }
		public string Message { get; set; }
	}

	public class FtpWorker : IFtpWorker
	{
		[Dependency]
		public IFtpFactory IFtpFactory { get; set; }

		private IFtpClient CreateFtpClient()
			=> IFtpFactory.Client(_TargetFtp);

		public event EventHandler<FtpEventArgs> FileWillUpload;
		public event EventHandler<FtpEventArgs> FileDidUpload;
		public event EventHandler<FtpEventArgs> FileError;

		private readonly TargetFtpConfig _TargetFtp;

		private readonly ConcurrentQueue<FileInfo> _Queue
			= new ConcurrentQueue<FileInfo>();

		private async Task UploadFiles()
		{
			using (var ftpClient = CreateFtpClient())
			{
				while (_Queue.TryDequeue(out var file))
				{
					var args = new FtpEventArgs
					{
						File = file,
						FilesLeft = _Queue.Count + 1
					};

					try
					{
						FileWillUpload?.Invoke(this, args);
						await ftpClient.UploadAsync(file);
						FileDidUpload?.Invoke(this, args);
					}
					catch (Exception ex)
					{
						args.Message = ex.InnerException?.Message;
						FileError?.Invoke(this, args);
					}
				}
			}
		}

		private const int _ConcurrentUploads = 4;

		public async Task Upload(FileInfo file)
		{
			using (var ftpClient = CreateFtpClient())
			{
				await CreateFtpClient().UploadAsync(file);
			}
		}

		public FtpWorker(TargetFtpConfig targetFtp)
		{
			_TargetFtp = targetFtp;
		}

		public void Enqueue(FileInfo file)
			=> _Queue.Enqueue(file);

		public void Enqueue(FileInfo[] files)
		{
			if (files.Length == 0)
				return;

			foreach (var file in files)
				_Queue.Enqueue(file);
		}

		public Task UploadAsync()
		{
			var tasks = new List<Task>();

			for (int i = 0; i < _ConcurrentUploads; i++)
				tasks.Add(Task.Run(async () => await UploadFiles()));

			return Task.WhenAll(tasks);
		}
	}
}
