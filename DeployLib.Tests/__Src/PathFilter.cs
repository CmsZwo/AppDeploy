using System.IO;
using System.Collections.Generic;

using Xunit;

namespace DeployLib.Tests
{
	public class PathFilterTests
	{
		[Fact]
		public void Directory_File_Combination_Should_Apply()
		{
			var PathFilter = new PathFilter();

			var file = new FileInfo("/bin/test.csproj");
			var files = new[] { file };

			var filter = new FilterConfig
			{
				Entries = new List<string>
				{
					"bin/*.csproj"
				}
			};

			var result = PathFilter.Filter(filter, files);
			Assert.Empty(result);
		}

		[Fact]
		public void Directory_Filter_Should_Apply()
		{
			var PathFilter = new PathFilter();

			var directory = new DirectoryInfo("/Static/Test/");
			var directories = new[] { directory };

			var filter = new FilterConfig
			{
				Entries = new List<string>
				{
					"Static/"
				}
			};

			var result = PathFilter.Filter(filter, directories);
			Assert.Empty(result);
		}
	}
}
