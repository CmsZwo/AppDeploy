using System;
using System.Linq;
using System.Collections.Generic;

namespace DeployLib
{
	public class StringDictionary : Dictionary<string, string>
	{
		public StringDictionary()
			: base(StringComparer.OrdinalIgnoreCase) { }
	}

	public class StringDictionary<T> : Dictionary<string, T>
	{
		public StringDictionary()
			: base(StringComparer.OrdinalIgnoreCase) { }

		public Dictionary<string, T> GetKeySorted()
			=>
			Keys
				.OrderBy(x => x)
				.ToDictionary(x => x, x => this[x]);
	}
}
