namespace DeployLib.Ftp
{
	public interface IFtpFactory
	{
		IFtpClient Client(TargetFtpConfig targetFtp);
		IFtpCleanupHelper CleanupHelper(TargetFtpConfig targetFtp);
	}

	public class FtpFactory : IFtpFactory
	{
		public IFtpClient Client(TargetFtpConfig targetFtp)
		{
			var result = new FtpClient(targetFtp);
			Container.Shared.Inject(result);
			return result;
		}

		public IFtpCleanupHelper CleanupHelper(TargetFtpConfig targetFtp)
		{
			var result = new FtpCleanupHelper(targetFtp);
			Container.Shared.Inject(result);
			return result;
		}
	}
}
