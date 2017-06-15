using System;
using System.Drawing;
using System.IO;

namespace webdiff.img
{
	internal static class BitmapHelper
	{
		public static Bitmap FromByteArray(byte[] array) => Image.FromStream(new MemoryStream(array)) as Bitmap ?? throw new ArgumentException("Byte array isn't valid bitmap", nameof(array));
	}
}