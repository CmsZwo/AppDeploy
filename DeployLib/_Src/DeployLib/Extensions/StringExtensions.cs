using System;

using Newtonsoft.Json;

namespace DeployLib
{
	public static class StringExtensions
	{
		public static bool HasContent(this string instance)
			=> instance != null && instance != "" && instance.Trim() != "";

		public static bool EqualsIgnoreCase(this string instance, string compare)
			=> string.Equals(instance, compare, StringComparison.OrdinalIgnoreCase);

		public static bool StartsWithIgnoreCase(this string instance, string compare)
			=> instance.StartsWith(compare, StringComparison.OrdinalIgnoreCase);

		private static readonly JsonSerializerSettings _TypeInformationSettings
			= new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

		public static string ToJsonWithTypeInformation(this object instance)
			=> JsonConvert.SerializeObject(instance, Formatting.None, _TypeInformationSettings);

		private static JsonSerializerSettings _JsonSerializerSettings = null;
		private static JsonSerializerSettings GetJsonSerializerSettings()
		{
			if (_JsonSerializerSettings != null) return _JsonSerializerSettings;

			_JsonSerializerSettings = new JsonSerializerSettings
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Error,
				DateTimeZoneHandling = DateTimeZoneHandling.Local,
				ObjectCreationHandling = ObjectCreationHandling.Replace,
				NullValueHandling = NullValueHandling.Include,
				TypeNameHandling = TypeNameHandling.Auto,
				MissingMemberHandling = MissingMemberHandling.Ignore
			};

			return _JsonSerializerSettings;
		}

		public static T ToObjectByJson<T>(this string instance)
		{
			if (string.IsNullOrWhiteSpace(instance) || instance == "[]")
				return default(T);

			var settings = GetJsonSerializerSettings();
			var result = JsonConvert.DeserializeObject<T>(instance, settings);
			return result;
		}
	}
}
