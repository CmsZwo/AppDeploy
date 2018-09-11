using System;
using System.Collections;

namespace DeployLib
{
	public static class ICollectionExtensions
	{
		public static bool HasContent(this ICollection instance)
			=> instance?.Count > 0 == true;
	}
}
