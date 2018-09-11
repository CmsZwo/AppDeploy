using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;

namespace DeployLib
{
	public interface IRootHelper
	{
		DirectoryInfo GetWorking();

		DirectoryInfo GetRoot();
		void SetRoot(DirectoryInfo directory);
	}

	public class RootHelper : IRootHelper
	{
		private DirectoryInfo _WorkingDirectory;
		private DirectoryInfo _Root;

		private DirectoryInfo GetWorkingDirectory()
			=>
			Debugger.IsAttached
				? new DirectoryInfo("C:\\www\\")
				: new DirectoryInfo(Environment.CurrentDirectory);

		public DirectoryInfo GetWorking()
		{
			if (_WorkingDirectory == null)
				_WorkingDirectory = GetWorkingDirectory();

			return _WorkingDirectory;
		}

		public DirectoryInfo GetRoot()
		{
			if (_Root == null)
			{
				var workingDirectory = GetWorkingDirectory();
				_Root = workingDirectory;

				while (true)
				{
					var found =
						_Root
							.GetFiles()
							?.Any(x => x.Name.ToLower() == ConfigHelper._DeploymentFileName.ToLower())
							== true;

					if (found)
						break;

					if (_Root.Parent == null)
					{
						_Root = workingDirectory;
						break;
					}

					_Root = _Root.Parent;
				}
			}

			return _Root;
		}

		public void SetRoot(DirectoryInfo directory)
		{
			_Root = directory;
		}
	}
}
