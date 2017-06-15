using System;
using System.Collections.Generic;
using System.IO;

namespace webdiff.utils
{
	internal static class LinqHelper
	{
		public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
		{
			foreach(var item in items)
				action(item);
		}

		public static IEnumerable<string> ReadLines(this TextReader reader)
		{
			string line;
			while((line = reader.ReadLine()) != null)
				yield return line;
		}
	}
}