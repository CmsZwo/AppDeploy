using System;
using System.IO;
using System.Threading.Tasks;

using FluentFTP;

namespace DeployLib.Ftp
{
	public interface IFtpClient : IDisposable
	{
		Task UploadAsync(FileInfo file);

		Task DeleteAsync(string relativePath);
		Task<string> DownloadStringAsync(string relativePath);
		Task UploadStringAsync(string relativePath, string content);
	}

	public class FtpClient : FluentFtpHolder, IFtpClient
	{
		public FtpClient(TargetFtpConfig targetFtp)
			: base(targetFtp)
		{
		}

		public async Task UploadAsync(FileInfo file)
		{
			var remoteTargetFile = GetRemoteTargetFile(file.FullName);
			var ftpUrl = GetRemoteUrl(remoteTargetFile);

			try
			{
				await
					GetFtpClient()
						.UploadFileAsync(
							file.FullName,
							ftpUrl
						);
			}
			catch (FtpException)
			{
				lock (Container.Shared)
				{
					GetFtpClient()
						.UploadFile(
							file.FullName,
							ftpUrl,
							createRemoteDir: true
						);
				}
			}
		}

		public async Task<string> DownloadStringAsync(string relativePath)
		{
			var ftpUrl = GetRemoteUrl(relativePath);

			try
			{
				using (var stream = await GetFtpClient().OpenReadAsync(ftpUrl))
				using (var reader = new StreamReader(stream))
				{
					return await reader.ReadToEndAsync();
				}
			}
			catch (FtpCommandException)
			{
				return null;
			}
		}

		public async Task UploadStringAsync(string relativePath, string content)
		{
			var ftpUrl = GetRemoteUrl(relativePath);

			using (var stream = await GetFtpClient().OpenWriteAsync(ftpUrl, FtpDataType.ASCII))
			using (var writer = new StreamWriter(stream))
			{
				await writer.WriteAsync(content);
			}
		}

		public async Task DeleteAsync(string relativePath)
		{
			var ftpUrl = GetRemoteUrl(relativePath);
			await GetFtpClient().DeleteFileAsync(ftpUrl);
		}
	}
}
