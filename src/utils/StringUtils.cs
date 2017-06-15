using System;

namespace webdiff.utils
{
	public static class StringUtils
	{
		public static bool RemovePrefix(ref string value, string prefix)
		{
			if(!value.StartsWith(prefix, StringComparison.Ordinal))
				return false;
			value = value.Substring(prefix.Length);
			return true;
		}
	}
}