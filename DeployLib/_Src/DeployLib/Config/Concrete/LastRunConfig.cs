using System;
using System.Collections.Generic;

namespace DeployLib
{
	public class LastRunConfig
	{
		public DateTime Dt { get; set; }
		public List<string> Files { get; set; }
	}
}
