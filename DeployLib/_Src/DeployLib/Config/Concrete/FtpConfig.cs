namespace DeployLib
{
	public class FtpConfig
	{
		public string Name { get; set; }

		public string Host { get; set; }
		public int Port { get; set; }
			= 21;

		public string User { get; set; }
		public string Password { get; set; }
	}
}
