using System;
using System.IO;
using System.Linq;
using System.Reflection;
using OpenQA.Selenium;

namespace webdiff.utils
{
	internal static class CookieParser
	{
		public static Cookie[] Parse(string filepath)
		{
			return File.ReadLines(filepath)
				.Select(line => line.Trim())
				.Where(line => line != string.Empty)
				.Select(ParseCookie)
				.Select(ToSeleniumCookie)
				.ToArray();
		}

		private static Cookie ToSeleniumCookie(this System.Net.Cookie cookie) =>
			new Cookie(cookie.Name, cookie.Value, cookie.Domain, cookie.Path, cookie.Expires != DateTime.MinValue ? (DateTime?)cookie.Expires : null);

		private static System.Net.Cookie ParseCookie(string cookie)
		{
			var type = typeof(System.Net.Cookie).Assembly.GetType(CookieParserTypeName);
			if(type == null)
				throw new Exception($"Invalid runtime: type '{CookieParserTypeName}' not found");
			var ctor = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault();
			if(ctor == null)
				throw new Exception($"Invalid runtime: type '{CookieParserTypeName}' has no .ctor(string)");
			var obj = ctor.Invoke(new object[] {cookie});
			var method = type.GetMethod("Get", BindingFlags.NonPublic | BindingFlags.Instance);
			if(method == null)
				throw new Exception($"Invalid runtime: type '{CookieParserTypeName}' has no method Get()");
			return (System.Net.Cookie)method.Invoke(obj, null);
		}

		private const string CookieParserTypeName = "System.Net.CookieParser";
	}
}