using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DeployLib
{
	public interface IPathFilter
	{
		List<FileInfo> Filter(FilterConfig config, IEnumerable<FileInfo> files);
		List<DirectoryInfo> Filter(FilterConfig config, IEnumerable<DirectoryInfo> directories);
	}

	public class PathFilter : IPathFilter
	{
		#region Tools

		private string ParseRegexFilter(string filter)
		{
			var result = Regex.Escape(filter);
			result = result.Replace("\\*", "(.*)");
			return result;
		}

		private bool IsMatch(string value, string filter, bool matchFile = false)
		{
			filter = ParseRegexFilter(filter);

			if (matchFile)
				filter = $"^{filter}$";

			var result = Regex.IsMatch(value, filter, RegexOptions.IgnoreCase);
			return result;
		}

		private bool IsMatch(DirectoryInfo directory, string filter)
		{
			var path = directory.FullName.Replace("\\", "/").ToLower() + "/";
			return IsMatch(path, filter);
		}

		private bool AppliesFile(string filter, FileInfo file)
		{
			if (filter.EndsWith("/"))
				return false;

			var hasPath = filter.Contains("/");
			if (hasPath)
			{
				var fileBeginIndex = filter.LastIndexOf('/') + 1;

				var pathFilter = filter.Substring(0, fileBeginIndex);
				filter = filter.Substring(fileBeginIndex);

				if (!IsMatch(file.Directory, pathFilter))
					return false;
			}

			var hasWildcard = filter.Contains("*");

			if (!hasWildcard)
				return file.Name.ToLower() == filter.ToLower();

			return IsMatch(file.Name, filter, matchFile: true);
		}

		private bool AppliesFile(FilterConfig config, FileInfo file)
		{
			var result = config.Entries.Any(x => AppliesFile(x, file));
			return result;
		}

		private bool AppliesDirectory(string filter, DirectoryInfo directory)
		{
			if (!filter.EndsWith("/"))
				return false;

			var hasWildcard = filter.Contains("*");

			if (!hasWildcard)
				return IsMatch(directory, filter);

			return IsMatch(directory.FullName, filter);
		}

		private bool AppliesDirectory(FilterConfig config, DirectoryInfo directory)
			=> config.Entries.Any(x => AppliesDirectory(x, directory));

		private List<T> Filter<T>(
			FilterConfig config,
			IEnumerable<T> items,
			Func<FilterConfig, T, bool> func
			)
		{
			var result = new List<T>();

			foreach (var item in items)
			{
				if (func(config, item))
					continue;

				result.Add(item);
			}

			return result;
		}

		#endregion

		public List<FileInfo> Filter(FilterConfig config, IEnumerable<FileInfo> files)
			=> Filter(config, files, AppliesFile);

		public List<DirectoryInfo> Filter(FilterConfig config, IEnumerable<DirectoryInfo> directories)
			=> Filter(config, directories, AppliesDirectory);
	}
}
