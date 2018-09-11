using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace DeployLib
{
	public interface IProjectEnumerator
	{
		bool IsProjectDirectory(DirectoryInfo directory);
		List<DirectoryInfo> GetProjectDirectories(DirectoryInfo directory);
	}

	public class ProjectEnumerator : IProjectEnumerator
	{
		private List<DirectoryInfo> CrawlDirectories(DirectoryInfo directory)
		{
			var result = new List<DirectoryInfo>();

			var subDirectories = directory.GetDirectories();
			if (!subDirectories.HasContent())
				return result;

			foreach (var sub in subDirectories)
				result.AddRange(CrawlDirectories(sub));

			if (IsProjectDirectory(directory))
			{
				result.Add(directory);
				return result;
			}

			return result;
		}

		public List<DirectoryInfo> GetProjectDirectories(DirectoryInfo directory)
		{
			var result =
				CrawlDirectories(directory)
					.OrderBy(x => x.Name)
					.ToList();

			return result;
		}

		public bool IsProjectDirectory(DirectoryInfo directory)
		{
			var files = directory.GetFiles();
			var hasDeploymentFile = files?.Any(x => x.Extension.EqualsIgnoreCase(ConfigHelper._DeploymentFileName)) == true;
			var isWebProject = files?.Any(x => x.Extension.EqualsIgnoreCase(".asax")) == true;
			return hasDeploymentFile && isWebProject;
		}
	}
}
