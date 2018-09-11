using System.Collections.Generic;

namespace DeployLib
{
	public class TargetConfig
	{
		public string Name { get; set; }
		public List<FtpConfig> Ftp { get; set; }
		public FilterConfig Exclude { get; set; }
		public FilterConfig Pick { get; set; }
		public string Directory { get; set; }
		public bool CleanupDisable { get; set; }
		public string OK { get; set; }
	}
}
