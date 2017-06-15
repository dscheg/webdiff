using System;
using System.Collections.Generic;
using System.IO;

namespace webdiff.utils
{
	internal static class PathHelper
	{
		public static string RelativeToBaseDirectory(this string path)
		{
			return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
		}

		public static string ToSafeFilename(this string path, char replacement = '_', int maxlen = 128)
		{
			if(string.IsNullOrEmpty(path))
				return path;

			var chars = path.ToCharArray(0, maxlen > 0 && path.Length > maxlen ? maxlen : path.Length);
			for(int i = 0; i < chars.Length; i++)
			{
				if(InvalidFileNameChars.Contains(chars[i]))
					chars[i] = replacement;
			}

			return new string(chars);
		}

		public static bool FileExistsWithOptionalExt(ref string filepath, string ext)
		{
			var path = filepath.RelativeToBaseDirectory();
			if(File.Exists(path))
				return true;
			if(File.Exists(Path.Combine(path, ext)))
			{
				filepath += ext;
				return true;
			}
			return false;
		}

		private static readonly HashSet<char> InvalidFileNameChars = new HashSet<char>(Path.GetInvalidFileNameChars());
	}
}