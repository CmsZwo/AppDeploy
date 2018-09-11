using System;
using System.Linq;
using System.Collections.Generic;

namespace DeployLib
{
	public interface IConfigParser
	{
		Config Parse(string config);
	}

	public class ConfigParser : IConfigParser
	{
		#region Tools Lines

		private Queue<string> GetLines(string raw)
		{
			var result = new Queue<string>();

			if (!raw.HasContent())
				return result;

			var split =
				raw
					.Split(
						new[] { '\n' },
						StringSplitOptions.RemoveEmptyEntries
					);

			var linesWithContent =
				split
					.Where(x => x.HasContent())
					.ToList();

			foreach (var line in linesWithContent)
			{
				if (!line.Contains("#"))
				{
					result.Enqueue(line);
					continue;
				}

				var clean = line.Substring(0, line.IndexOf("#"));
				result.Enqueue(clean);
			}

			return result;
		}

		private KeyValuePair<string, string> ParsePair(string line)
		{
			var split =
				line
					.Split(new[] { ':' })
					.ToList();

			var key = split[0].Trim();

			var value = null as string;
			if (split.Count > 0)
				value = string.Join(':', split.Skip(1).ToArray()).Trim();

			return new KeyValuePair<string, string>(key, value);
		}

		private StringDictionary ParseDictonary(List<string> lines)
		{
			var result = new StringDictionary();

			if (lines.Count == 0)
				return result;

			foreach (var line in lines)
			{
				var pair = ParsePair(line);
				result[pair.Key] = pair.Value;
			}

			return result;
		}

		private List<string> ParseToken(List<string> lines)
		{
			var result = new List<string>();

			if (lines.Count == 0)
				return result;

			foreach (var line in lines)
			{
				var token =
					line
						.Split(new[] { ',' })
						.Select(x => x.Trim());

				result.AddRange(token);
			}

			return result;
		}

		#endregion

		#region Tools Config

		private FtpConfig ParseFtp(KeyValuePair<string, string> command, List<string> parameters)
		{
			var options = ParseDictonary(parameters);

			var result = new FtpConfig
			{
				Name = command.Value,
				Host = options.GetIfHasKey("Host"),
				User = options.GetIfHasKey("User"),
				Password = options.GetIfHasKey("Password")
			};

			return result;
		}

		private FilterConfig ParseFiles(KeyValuePair<string, string> command, List<string> parameters)
		{
			var token = ParseToken(parameters);

			var result = new FilterConfig
			{
				Entries = token
			};

			return result;
		}

		private FilterConfig GetFilter(string filterNames, StringDictionary<FilterConfig> filters)
		{
			var result = new FilterConfig
			{
				Entries = new List<string>()
			};

			if (!filterNames.HasContent())
				return result;

			var names = filterNames.Split(new[] { ',' });
			foreach (var name in names)
			{
				var key = name.Trim();
				var filter = filters.GetIfHasKey(key);

				if (filter == null)
					throw new ConfigException($"Unknown exclude or pick [{key}].");

				result.Entries.AddRange(filter.Entries);
			}

			return result;
		}

		private List<FtpConfig> GetFtp(string filterNames, StringDictionary<FtpConfig> ftpConfigs)
		{
			var result = new List<FtpConfig>();

			var names = filterNames.Split(new[] { ',' });
			foreach (var name in names)
			{
				var key = name.Trim();
				var ftp = ftpConfigs.GetIfHasKey(key);

				if (ftp == null)
					throw new ConfigException($"Unknown ftp [{key}].");

				result.Add(ftp);
			}

			return result;
		}

		private TargetConfig ParseTarget(KeyValuePair<string, string> command, List<string> parameters, Config config)
		{
			var options = ParseDictonary(parameters);

			var result = new TargetConfig
			{
				Name = command.Value,
				Ftp = GetFtp(options.GetIfHasKey("ftp"), config.Ftp),
				Exclude = GetFilter(options.GetIfHasKey("exclude"), config.Exclude),
				Pick = GetFilter(options.GetIfHasKey("pick"), config.Pick),
				Directory = options.GetIfHasKey("Directory"),
				CleanupDisable = options.GetIfHasKey("cleanup-disable").EqualsIgnoreCase("true"),
				OK = options.GetIfHasKey("ok")
			};

			if (result.Ftp == null)
				throw new ConfigException($"{nameof(ParseTarget)}: {nameof(result.Ftp)} [{command.Value}] not found.");

			return result;
		}

		private BatchConfig ParseBatch(KeyValuePair<string, string> command, List<string> parameters, Config config)
		{
			var rawCommands =
				parameters
					.Select(x => ParsePair(x))
					.ToList();

			var result = new BatchConfig
			{
				Name = command.Value
			};

			foreach (var pair in rawCommands)
			{
				if (pair.Key.EqualsIgnoreCase("target"))
				{
					var targetCommand = new TargetCommand
					{
						Name = pair.Key,
						Value = pair.Value,
						Target = config.Target.GetIfHasKey(pair.Value)
					};

					if (targetCommand.Target == null)
						throw new ConfigException($"{nameof(ParseBatch)}: {nameof(targetCommand.Target)} [{pair.Value}] not found.");

					result.Commands.Add(targetCommand);
				}
				else if (pair.Key.EqualsIgnoreCase("ok"))
				{
					var okCommand = new OkCommand
					{
						Name = pair.Key,
						Value = pair.Value,
						Url = pair.Value
					};

					result.Commands.Add(okCommand);
				}
				else
				{
					throw new ConfigException($"Unknown batch command [{pair.Key}].");
				}
			}

			return result;
		}

		#endregion

		#region IConfigParser

		public Config Parse(string config)
		{
			var result = new Config();

			var lines = GetLines(config);

			while (true)
			{
				if (lines.Count == 0)
					break;

				var commandLine = lines.Dequeue();
				var command = ParsePair(commandLine);

				var parameters = new List<string>();
				while (true)
				{
					if (lines.Count == 0)
						break;

					var parameter = lines.Peek();
					if (!parameter.StartsWith('\t') && !parameter.StartsWith(' '))
						break;

					parameters.Add(parameter.Trim());
					lines.Dequeue();
				}

				if (!command.Value.HasContent())
					throw new ConfigException($"Command [{command.Key}] has no value.");

				if (command.Key.StartsWith("ftp"))
					result.Ftp[command.Value] = ParseFtp(command, parameters);

				else if (command.Key.StartsWith("exclude"))
					result.Exclude[command.Value] = ParseFiles(command, parameters);

				else if (command.Key.StartsWith("pick"))
					result.Pick[command.Value] = ParseFiles(command, parameters);

				else if (command.Key.StartsWith("target"))
					result.Target[command.Value] = ParseTarget(command, parameters, result);

				else if (command.Key.StartsWith("batch"))
					result.Batch[command.Value] = ParseBatch(command, parameters, result);

				else
					throw new ConfigException($"Unknown command [{command.Key}].");
			}

			return result;
		}

		#endregion
	}

	public class ConfigException : Exception
	{
		public ConfigException(string message)
			: base(message)
		{ }
	}
}
