using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using DeployLib;

namespace AppDeploy
{
	class Program
	{
		static void Main(string[] args)
			=> MainAsync(args).GetAwaiter().GetResult();

		static int GetIndex(int inputNumber)
			=> inputNumber - 1;

		static async Task ServeBatchMenu(Config config)
		{
			var console = Container.Shared.Get<IOutputConsole>();
			var inputHelper = Container.Shared.Get<IInputHelper>();

			var batchOption = BatchOption.None;

			void printMenu()
			{
				console.Clear();
				console.WriteLine("Choose batch to process:");

				console.WriteLine("");
				console.WriteLine("[esc] to quit");

				var batchIndex = 0;

				if (batchOption == BatchOption.None)
				{
					console.WriteLine($"[a] apply current state");
					console.WriteLine($"[r] reset state");
				}
				else if (batchOption == BatchOption.ApplyCurrentState)
				{
					console.WriteLine("[apply current state]");
				}
				else if (batchOption == BatchOption.ResetState)
				{
					console.WriteLine("[reset state]");
				}

				console.WriteLine("");

				foreach (var p in config.Batch.GetKeySorted())
					console.WriteLine($"{batchIndex++ + 1}. {p.Key}");
			}

			while (true)
			{
				printMenu();

				while(true)
				{
					var input = inputHelper.ReadInput(
						config.Batch.Count,
						new[] { ConsoleKey.Escape, ConsoleKey.A, ConsoleKey.R }
					);

					if (input.Key == ConsoleKey.Escape)
					{
						Pause();
						break;
					}

					if (input.Key == ConsoleKey.A)
					{
						batchOption = BatchOption.ApplyCurrentState;
						continue;
					}

					else if (input.Key == ConsoleKey.R)
					{
						batchOption = BatchOption.ResetState;
						continue;
					}

					if (!input.Number.HasValue)
						continue;

					var batchIndex = GetIndex(input.Number.Value);
					var batch = config.Batch.GetKeySorted().ElementAt(batchIndex).Value;
					console.WriteLine("");

					await ProcessBatchAsync(batch, batchOption);
					batchOption = BatchOption.None;
					Pause();
				}
			}
		}

		static void ServeProjectMenu()
		{
			var console = Container.Shared.Get<IOutputConsole>();
			var inputHelper = Container.Shared.Get<IInputHelper>();
			var rootHelper = Container.Shared.Get<IRootHelper>();
			var projectEnumerator = Container.Shared.Get<IProjectEnumerator>();

			if (projectEnumerator.IsProjectDirectory(rootHelper.GetRoot()))
				return;

			var projectDirectories =
				projectEnumerator
					.GetProjectDirectories(rootHelper.GetRoot());

			void printMenu()
			{
				console.Clear();
				console.WriteLine("Choose project:");

				console.WriteLine("");
				console.WriteLine("[esc] to quit");
				console.WriteLine("");

				var batchIndex = 0;
				var digits = projectDirectories.Count.ToString().Length;
				var indexFormat = new string('0', digits);

				foreach (var directory in projectDirectories)
				{
					var indexString = (batchIndex++ + 1).ToString(indexFormat);

					var label = directory.Name;
					if (projectDirectories.Count(x => x.Name == directory.Name) > 1)
						label += $"/{directory.Parent.Name}";

					console.WriteLine($"{indexString}. {label}");
				}
			}

			while (true)
			{
				printMenu();

				var input = inputHelper.ReadInput(projectDirectories.Count);

				if (input.Key == ConsoleKey.Escape)
				{
					Pause();
					break;
				}

				var index = GetIndex(input.Number.Value);
				var root = projectDirectories[index];
				rootHelper.SetRoot(root);
				console.WriteLine("");
				return;
			}
		}

		static async Task MainAsync(string[] args)
		{
			try
			{
				ServeProjectMenu();

				var config = GetConfig();

				if (args.FirstOrDefault() != null)
				{
					var batch = GetBatch(args, config);
					await ProcessBatchAsync(batch, BatchOption.None);
				}
				else
				{
					await ServeBatchMenu(config);
				}
			}
			catch (Exception ex)
			{
				var console = Container.Shared.Get<IOutputConsole>();
				console.ClearLine();
				console.WriteLine($"{ex.GetType().Name}: {ex.Message}");
				Pause();
			}
		}

		private static void Pause()
		{
			Console.WriteLine("");
			Console.WriteLine("[press any key]");
			Console.ReadLine();
		}

		private static Config GetConfig()
		{
			var configHelper = Container.Shared.Get<IConfigHelper>();
			var config = configHelper.GetConfig();

			if (!config.Batch.HasContent())
				throw new ConfigException("No batch configured.");

			return config;
		}

		private static BatchConfig GetBatch(string[] args, Config config)
		{
			var batch = config.Batch.GetIfHasKey(args[0]);

			if (batch == null)
				throw new ConfigException($"Batch [{args[0]}] not found.");
			return batch;
		}

		private static async Task ProcessBatchAsync(BatchConfig batch, BatchOption option)
		{
			var batchProcessor = Container.Shared.Get<IBatchProcessor>();

			Console.WriteLine($"Processing [{batch.Name}]");
			Console.WriteLine("");

			await batchProcessor.Process(batch, option);
		}
	}
}
