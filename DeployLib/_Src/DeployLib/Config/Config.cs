namespace DeployLib
{
	public class Config
	{
		public StringDictionary<FtpConfig> Ftp { get; }
			= new StringDictionary<FtpConfig>();

		public StringDictionary<FilterConfig> Exclude { get; }
			= new StringDictionary<FilterConfig>();

		public StringDictionary<FilterConfig> Pick { get; }
			= new StringDictionary<FilterConfig>();

		public StringDictionary<TargetConfig> Target { get; }
			= new StringDictionary<TargetConfig>();

		public StringDictionary<BatchConfig> Batch { get; }
			= new StringDictionary<BatchConfig>();
	}
}
