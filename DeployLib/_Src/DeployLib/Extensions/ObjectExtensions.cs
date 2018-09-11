using Newtonsoft.Json;

namespace DeployLib
{
	public static class ObjectExtensions
	{
		public static string ToJson(this object instance)
			=> JsonConvert.SerializeObject(instance);
	}
}
