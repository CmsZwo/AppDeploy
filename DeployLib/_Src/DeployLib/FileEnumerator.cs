using System.IO;
using System.Linq;
using System.Collections.Generic;

using Unity.Attributes;

namespace DeployLib
{
	public interface IFileEnumerator
	{
		List<FileInfo> GetFiles(TargetConfig target);
	}

	public class FileEnumerator : IFileEnumerator
	{
		[Dependency]
		public IRootHelper IRootHelper { get; set; }

		[Dependency]
		public IPathFilter IPathFilter { get; set; }

		private List<FileInfo> CrawlFiles(DirectoryInfo directory, FilterConfig filter)
		{
			var files = directory.GetFiles();
			return IPathFilter.Filter(filter, files);
		}

		private List<FileInfo> CrawlDirectories(FilterConfig filter)
		{
			var root = IRootHelper.GetRoot();
			return CrawlDirectories(root, filter);
		}

		private List<FileInfo> CrawlDirectories(DirectoryInfo directory, FilterConfig filter)
		{
			var subDirectories =
				IPathFilter
					.Filter(
						filter,
						directory.GetDirectories()
					);

			var result = CrawlFiles(directory, filter);

			if (subDirectories.Count == 0)
				return result;

			foreach (var sub in subDirectories)
				result.AddRange(CrawlDirectories(sub, filter));

			return result;
		}

		private List<FileInfo> Pick(DirectoryInfo directory)
		{
			var result = new List<FileInfo>();

			result.AddRange(directory.GetFiles());

			var subDirectories = directory.GetDirectories();
			if (!subDirectories.HasContent())
				return result;

			foreach (var sub in subDirectories)
				result.AddRange(Pick(sub));

			return result;
		}

		private List<FileInfo> Pick(FilterConfig filter)
		{
			if (filter.Entries.Any(x => x.Contains("*")))
				throw new ConfigException($"Filter for {nameof(Pick)} must not use wildcard [*].");

			var root = IRootHelper.GetRoot();

			var result = new List<FileInfo>();

			foreach (var item in filter.Entries)
			{
				var isFile = !item.EndsWith("/");
				var absolutePath = root.GetAbsoulte(item);

				if (isFile)
				{
					var file = new FileInfo(absolutePath);
					result.Add(file);
					continue;
				}

				var directory = new DirectoryInfo(absolutePath);
				result.AddRange(Pick(directory));
			}

			return result;
		}

		public List<FileInfo> GetFiles(TargetConfig target)
		{
			var result = new List<FileInfo>();

			if (target.Exclude != null)
				result.AddRange(CrawlDirectories(target.Exclude));

			if (target.Pick != null)
				result.AddRange(Pick(target.Pick));

			return result;
		}
	}
}
