using System;
using System.Linq;
using System.Collections.Generic;

using Unity.Attributes;

namespace DeployLib
{
	public class Input
	{
		public ConsoleKey? Key { get; set; }
		public int? Number { get; set; }
	}

	public interface IInputHelper
	{
		Input ReadInput(int maxNumber, IEnumerable<ConsoleKey> acceptedKeys = null);
	}

	public class InputHelper : IInputHelper
	{
		[Dependency]
		public IOutputConsole IOutputConsole { get; set; }

		private ConsoleKeyInfo ReadKey(IEnumerable<ConsoleKey> acceptedKeys = null)
		{
			while (true)
			{
				var key = IOutputConsole.ReadKey();

				if (acceptedKeys == null)
					return key;

				var isAcceptedKey
					= acceptedKeys.Any(x => key.Key == x) == true;

				if (isAcceptedKey)
					return key;
			}
		}

		public Input ReadInput(int maxNumber, IEnumerable<ConsoleKey> acceptedKeys = null)
		{
			var numberBuffer = "";

			while (true)
			{
				var keyInfo = ReadKey(acceptedKeys);

				if (keyInfo.Key == ConsoleKey.Escape)
				{
					IOutputConsole.ClearLine();

					if (numberBuffer == "")
						return new Input
						{
							Key = keyInfo.Key
						};

					numberBuffer = "";
					continue;
				}

				var isNumber
					= int.TryParse(keyInfo.KeyChar.ToString(), out var lastDigit);

				if (!isNumber)
				{
					if (numberBuffer == "" && keyInfo.Key != ConsoleKey.Backspace)
					{
						IOutputConsole.ClearLine();

						return new Input
						{
							Key = keyInfo.Key
						};
					}

					if (keyInfo.Key == ConsoleKey.Enter)
						return new Input
						{
							Number = int.Parse(numberBuffer)
						};

					if (keyInfo.Key == ConsoleKey.Backspace)
					{
					if (numberBuffer.Length < 2)
							numberBuffer = "";
						else
							numberBuffer = numberBuffer.Substring(0, numberBuffer.Length - 1);
					}

					IOutputConsole.ClearKey();
					continue;
				}

				if (lastDigit == 0 && numberBuffer == "")
				{
					IOutputConsole.ClearKey();
					continue;
				}

				numberBuffer += lastDigit.ToString();
				var number = int.Parse(numberBuffer);

				if (number > maxNumber)
				{
					numberBuffer = numberBuffer.Substring(0, numberBuffer.Length - 1);
					IOutputConsole.ClearKey();
					continue;
				}

				var maxDigits = maxNumber.ToString().Length;
				if (numberBuffer.Length == maxDigits)
				{
					IOutputConsole.ClearLine();

					return new Input
					{
						Number = number
					};
				}
			}
		}

	}
}
