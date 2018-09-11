using System.IO;

namespace DeployLib
{
	public static class DirectoryInfoExtensions
	{
		public static string GetRealtive(this DirectoryInfo instance, FileInfo file)
			=>
			file
				.FullName
				.Substring(instance.FullName.Length)
				.Replace("\\", "/");

		public static string GetAbsoulte(this DirectoryInfo instance, string relativePath)
			=>
			instance.FullName + "\\" + relativePath.Replace("/", "\\");
	}
}
