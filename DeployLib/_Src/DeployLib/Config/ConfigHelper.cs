using System.IO;
using System.Collections.Generic;

using Unity.Attributes;

namespace DeployLib
{
	public interface IConfigHelper
	{
		Config GetConfig();
	}

	public class ConfigHelper : IConfigHelper
	{
		[Dependency]
		public IRootHelper IRootHelper { get; set; }

		public const string _DeploymentFileName = ".deployment";

		private string ReadWorkingConfig()
		{
			var directory = IRootHelper.GetWorking();
			var configFile = new FileInfo(directory.FullName + "\\" + _DeploymentFileName);

			if (!configFile.Exists)
				return null;

			return File.ReadAllText(configFile.FullName);
		}

		private string ReadRootConfig()
		{
			var directory = IRootHelper.GetRoot();
			var configFile = new FileInfo(directory.FullName + "\\" + _DeploymentFileName);

			if (!configFile.Exists)
				return null;

			return File.ReadAllText(configFile.FullName);
		}

		public Config GetConfig()
		{
			var configContent =
				ReadWorkingConfig()
				+ "\n"
				+ ReadRootConfig();

			var parser = new ConfigParser();
			var result = parser.Parse(configContent);

			foreach (var target in result.Target)
			{
				result.Batch[target.Key] = new BatchConfig
				{
					Name = target.Key,
					Commands = new List<BatchCommand>
					{
						new TargetCommand
						{
							Name = "target",
							Value = target.Key,
							Target = target.Value
						}
					}
				};
			}

			return result;
		}
	}
}
