using System.Collections.Generic;

namespace DeployLib
{
	public class BatchConfig
	{
		public string Name { get; set; }

		public List<BatchCommand> Commands { get; set; }
			= new List<BatchCommand>();
	}

	public abstract class BatchCommand
	{
		public string Name { get; set; }
		public string Value { get; set; }
	}

	public class TargetCommand : BatchCommand
	{
		public TargetConfig Target { get; set; }
	}

	public class OkCommand : BatchCommand
	{
		public string Url { get; set; }
	}
}
