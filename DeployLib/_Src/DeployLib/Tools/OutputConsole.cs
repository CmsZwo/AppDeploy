using System;

namespace DeployLib
{
	public interface IOutputConsole
	{
		void Write(string message);
		void WriteLine(string message);
		void WriteUnderline(string message);

		void Clear();
		void ClearLine();
	}

	public class OutputConsole : IOutputConsole
	{
		private int _MaxConsoleMessageLength
			=> Console.WindowWidth - 1;

		private string GetLineSafe(string message)
		{
			if (message.Length <= _MaxConsoleMessageLength)
				return message;

			return message.Substring(0, _MaxConsoleMessageLength - 3) + "...";
		}

		public void Write(string message)
			=> Console.Write(GetLineSafe(message));

		public void WriteLine(string message)
			=> Console.WriteLine(GetLineSafe(message));

		public void Clear()
			=> Console.Clear();

		public void ClearLine()
		{
			Console.Write("\r");
			Console.Write(new string(' ', _MaxConsoleMessageLength));
			Console.Write("\r");
		}

		public void WriteUnderline(string message)
			=> Console.WriteLine(new string('-', GetLineSafe(message).Length));
	}
}
