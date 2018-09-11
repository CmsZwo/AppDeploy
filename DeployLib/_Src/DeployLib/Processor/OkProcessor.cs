using System;
using System.Net;
using System.Threading.Tasks;

using Unity.Attributes;

namespace DeployLib
{
	public interface IOkProcessor : ICommandProcessor<OkCommand> { }

	public class OkProcessor : CommandProcessor<OkCommand>, IOkProcessor
	{
		[Dependency]
		public IOutputConsole IOutputConsole { get; set; }

		[Dependency]
		public ICurlHelper ICurlHelper { get; set; }

		public override async Task Process(OkCommand command)
		{
			IOutputConsole.Write($"Waiting for [{command.Url}]...");

			var start = DateTime.Now;

			while (true)
			{
				var status = await ICurlHelper.Curl(command.Url);

				if (status == HttpStatusCode.OK)
				{
					var ts = DateTime.Now - start;

					IOutputConsole.WriteLine($" {(int)ts.TotalSeconds} sec. OK");
					IOutputConsole.WriteLine("");
					return;
				}
			}
		}
	}
}
