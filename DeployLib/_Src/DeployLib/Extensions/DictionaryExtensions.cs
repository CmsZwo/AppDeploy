using System.Collections.Generic;

namespace DeployLib
{
	public static class DictionaryExtensions
	{
		public static T GetIfHasKey<T>(this IDictionary<string, T> instance, string key)
		{
			if (key == null)
				return default(T);

			if (!instance.ContainsKey(key))
				return default(T);

			return instance[key];
		}
	}
}
