using System;
using System.Drawing;

namespace webdiff.img
{
	public static class ColorHelper
	{
		public static Color Parse(string color)
		{
			if(string.IsNullOrEmpty(color))
				throw new ArgumentException("Empty color value", nameof(color));

			if(color[0] != '#')
				return Color.FromName(color);

			if(color.Length == 4)
				return Color.FromArgb(
					byte.MaxValue,
					Convert.ToInt32(new string(color[1], 2), 16),
					Convert.ToInt32(new string(color[2], 2), 16),
					Convert.ToInt32(new string(color[3], 2), 16));

			if(color.Length == 7)
				return Color.FromArgb(
					byte.MaxValue,
					Convert.ToInt32(color.Substring(1, 2), 16),
					Convert.ToInt32(color.Substring(3, 2), 16),
					Convert.ToInt32(color.Substring(5, 2), 16));

			if(color.Length == 9)
				return Color.FromArgb(
					Convert.ToInt32(color.Substring(1, 2), 16),
					Convert.ToInt32(color.Substring(3, 2), 16),
					Convert.ToInt32(color.Substring(5, 2), 16),
					Convert.ToInt32(color.Substring(7, 2), 16));

			throw new ArgumentException($"Invalid color value '{color}'", nameof(color));
		}

		public static Color BlendWith(this Color color1, Color color2)
		{
			var c1 = color1.ToArgb();
			var c2 = color2.ToArgb();
			return Color.FromArgb(((c1 >> 1) & RemHighestBits) + ((c2 >> 1) & RemHighestBits) + ((((c1 & LowestBits) + (c2 & LowestBits)) >> 1) & LowestBits));
		}

		public static int CompareTo(this Color color1, Color color2)
		{
			return Math.Max(Math.Max(Math.Abs(color1.A - color2.A), Math.Abs(color1.R - color2.R)), Math.Max(Math.Abs(color1.G - color2.G), Math.Abs(color1.B - color2.B)));
		}

		private const int RemHighestBits = 0x7f7f7f7f;
		private const int LowestBits = 0x01010101;
	}
}