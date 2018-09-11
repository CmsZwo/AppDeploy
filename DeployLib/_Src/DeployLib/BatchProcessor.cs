using System.Threading.Tasks;

using Unity.Attributes;

using DeployLib.Ftp;

namespace DeployLib
{
	public enum BatchOption
	{
		None = 0,
		ApplyCurrentState = 1,
		ResetState = 2
	};

	public interface IBatchProcessor
	{
		Task Process(BatchConfig batch, BatchOption option);
	}

	public class BatchProcessor : IBatchProcessor
	{
		[Dependency]
		public IOkProcessor IOkProcessor { get; set; }

		[Dependency]
		public ITargetProcessor ITargetProcessor { get; set; }

		[Dependency]
		public IFtpFactory IFtpFactory { get; set; }

		[Dependency]
		public IOutputConsole IOutputConsole { get; set; }

		public async Task Process(BatchConfig batch, BatchOption option)
		{
			foreach (var command in batch.Commands)
			{
				if (command is TargetCommand targetCommand)
				{
					if (option == BatchOption.ApplyCurrentState)
						await ITargetProcessor.ApplyCurrentStateAsync(targetCommand);

					else if (option == BatchOption.ResetState)
						await ITargetProcessor.ResetState(targetCommand);

					else
						await ITargetProcessor.Process(targetCommand);
				}

				else if (command is OkCommand)
					await IOkProcessor.Process(command as OkCommand);
			}
		}
	}
}
