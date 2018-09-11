using System;
using System.Net;

using Unity.Attributes;

namespace DeployLib.Ftp
{
	public abstract class FluentFtpHolder : IDisposable
	{
		[Dependency]
		public IRootHelper IRootHelper { get; set; }

		private readonly TargetFtpConfig _TargetFtp;

		private FluentFTP.FtpClient _FtpClient;
		protected FluentFTP.FtpClient GetFtpClient()
		{
			if (_FtpClient == null)
			{
				_FtpClient = new FluentFTP.FtpClient
				{
					Host = _TargetFtp.Host,
					Port = _TargetFtp.Port,
					Credentials = new NetworkCredential(_TargetFtp.User, _TargetFtp.Password),

					RetryAttempts = 3,
					SocketKeepAlive = true
				};
			}

			return _FtpClient;
		}

		protected string GetRemoteTargetFile(string localSourcePath)
			=>
				localSourcePath
					.Substring(IRootHelper.GetRoot().FullName.Length)
					.Replace("\\", "/");

		protected string GetRemoteUrl(string path)
		{
			var result =
				path.StartsWith("/")
					? path
					: "/" + path;

			if (!_TargetFtp.Directory.HasContent())
				return result;

			result = "/" + _TargetFtp.Directory + result;
			result = result.Replace("//", "/");
			return result;
		}

		public void Dispose()
		{
			if (_FtpClient == null)
				return;

			_FtpClient.Dispose();
		}

		public FluentFtpHolder(TargetFtpConfig targetFtp)
		{
			_TargetFtp = targetFtp;
		}
	}
}
