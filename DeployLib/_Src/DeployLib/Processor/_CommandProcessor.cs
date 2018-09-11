using System.Threading.Tasks;

namespace DeployLib
{
	public interface ICommandProcessor<T>
		where T : BatchCommand
	{
		Task Process(T command);
	}

	public abstract class CommandProcessor<T> : ICommandProcessor<T>
		where T : BatchCommand
	{
		public abstract Task Process(T command);
	}
}
