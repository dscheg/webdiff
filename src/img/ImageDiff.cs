using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.CompilerServices;

namespace webdiff.img
{
	internal static class ImageDiff
	{
		public static (int Unmatched, List<Rectangle> Map) CompareImages(Settings settings, Bitmap bmp1, Bitmap bmp2, out Bitmap diff)
		{
			var minWidth = Math.Min(bmp1.Width, bmp2.Width);
			var maxWidth = Math.Max(bmp1.Width, bmp2.Width);

			var minHeight = Math.Min(bmp1.Height, bmp2.Height);
			var maxHeight = Math.Max(bmp1.Height, bmp2.Height);

			diff = new Bitmap(bmp1.Height > bmp2.Height ? bmp1 : bmp2, maxWidth, maxHeight);

			int unmatched = 0;
			List<Rectangle> map = null;
			var matrix = new bool[maxWidth, maxHeight];

			int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;

			var compare = settings.Compare;

			using(var ddiff = new DirectBitmap(diff))
			using(var dbmp1 = new DirectBitmap(bmp1))
			using(var dbmp2 = new DirectBitmap(bmp2))
			for(int y = 0; y < minHeight; y++)
			for(int x = 0; x < minWidth; x++)
			{
				var color1 = dbmp1.GetPixel(x, y);
				var color2 = dbmp2.GetPixel(x, y);
				if(color1 != color2 && (compare.ColorThreshold == 0 || color1.CompareTo(color2) > compare.ColorThreshold))
				{
					ddiff.SetPixel(x, y, color1.BlendWith(color2));
					minX = Math.Min(minX, x);
					minY = Math.Min(minY, y);
					maxX = Math.Max(maxX, x);
					maxY = Math.Max(minY, y);
					matrix[x, y] = true;
					unmatched++;
				}
			}

			var heightOverflow = maxHeight - minHeight;
			var widthOverflow = maxWidth - minWidth;

			if(heightOverflow > 0)
				unmatched += heightOverflow * maxWidth;

			if(widthOverflow > 0)
				unmatched += widthOverflow * minHeight;

			if(unmatched > 0)
			{
				map = new List<Rectangle>();

				using(var g = Graphics.FromImage(diff))
				{
					if(diff.Width < maxWidth)
					{
						var src = bmp1.Width > bmp2.Width ? bmp1 : bmp2;
						var rect = new Rectangle(diff.Width, 0, src.Width - diff.Width, src.Height);
						g.DrawImage(src, rect, rect, GraphicsUnit.Pixel);
					}

					var visual = settings.Visual;

					using(var brush = new HatchBrush(visual.OverflowFillStyle, visual.OverflowFillForeColor, visual.OverflowFillBackColor))
					{
						if(widthOverflow > 0)
						{
							var rect = new Rectangle(minWidth, 0, widthOverflow, maxHeight);
							g.FillRectangle(brush, rect);
							map.Add(rect);
						}

						if(heightOverflow > 0)
						{
							var rect = new Rectangle(0, minHeight, maxWidth, heightOverflow);
							g.FillRectangle(brush, rect);
							map.Add(rect);
						}
					}

					using(var pen = new Pen(visual.BorderColor, visual.BorderWidth))
					using(var brush = new HatchBrush(visual.FillStyle, visual.FillForeColor, visual.FillBackColor))
					foreach(var rect in new Rectangle(minX, minY, maxX - minX, maxY - minY).Split(matrix, visual.BorderSpacing))
					{
						if(rect.Width <= compare.DiffSideThreshold || rect.Height <= compare.DiffSideThreshold)
							continue;

						map.Add(rect);

						var frame = new Rectangle(
							Math.Max(0, rect.X - visual.BorderPadding),
							Math.Max(0, rect.Y - visual.BorderPadding),
							Math.Min(diff.Width - rect.X, rect.Width + 2 * visual.BorderPadding),
							Math.Min(diff.Height - rect.Y, rect.Height + 2 * visual.BorderPadding));

						if(visual.Border)
							g.DrawRectangle(pen, frame);

						g.FillRectangle(brush, frame);
					}
				}
			}

			return (unmatched, map);
		}

		private static IEnumerable<Rectangle> Split(this Rectangle rect, bool[,] diff, int spacing)
		{
			var split = rect.SplitByX(diff, spacing).SelectMany(r => r.SplitByY(diff, spacing)).ToArray();
			return split.Length == 1 ? split : split.AsParallel().SelectMany(r => r.Split(diff, spacing)).ToArray();
		}

		private static IEnumerable<Rectangle> SplitByX(this Rectangle rect, bool[,] diff, int spacing)
		{
			var gap = 0;
			for(int x = rect.Left; x <= rect.Right; x++)
			{
				if(rect.ScanCol(x, diff))
					gap = 0;
				else if(++gap >= spacing)
				{
					yield return new Rectangle(rect.X, rect.Y, x - rect.X, rect.Height).Contract(diff);
					yield return new Rectangle(x, rect.Y, rect.Width - (x - rect.X), rect.Height).Contract(diff);
					yield break;
				}
			}
			yield return rect;
		}

		private static IEnumerable<Rectangle> SplitByY(this Rectangle rect, bool[,] diff, int spacing)
		{
			var gap = 0;
			for(int y = rect.Top; y <= rect.Bottom; y++)
			{
				if(rect.ScanRow(y, diff))
					gap = 0;
				else if(++gap >= spacing)
				{
					yield return new Rectangle(rect.X, rect.Y, rect.Width, y - rect.Y).Contract(diff);
					yield return new Rectangle(rect.X, y, rect.Width, rect.Height - (y - rect.Y)).Contract(diff);
					yield break;
				}
			}
			yield return rect;
		}

		private static Rectangle Contract(this Rectangle rect, bool[,] diff)
		{
			int top, bottom, left, right;
			for(top = rect.Top; top <= rect.Bottom && !rect.ScanRow(top, diff); top++) ;
			if(top != rect.Top)
				rect = new Rectangle(rect.X, top, rect.Width, rect.Height - (top - rect.Y));
			for(bottom = rect.Bottom; bottom >= rect.Top && !rect.ScanRow(bottom, diff); bottom--) ;
			if(bottom != rect.Bottom)
				rect = new Rectangle(rect.X, rect.Y, rect.Width, bottom - rect.Y);
			for(left = rect.Left; left <= rect.Right && !rect.ScanCol(left, diff); left++) ;
			if(left != rect.X)
				rect = new Rectangle(left, rect.Y, rect.Width - (left - rect.X), rect.Height);
			for(right = rect.Right; right >= rect.Left && !rect.ScanCol(right, diff); right--) ;
			if(right != rect.Right)
				rect = new Rectangle(rect.X, rect.Y, right - rect.X, rect.Height);
			return rect;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool ScanRow(this Rectangle rect, int y, bool[,] diff)
		{
			for(int x = rect.Left; x <= rect.Right; x++)
				if(diff[x, y]) return true;
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool ScanCol(this Rectangle rect, int x, bool[,] diff)
		{
			for(int y = rect.Top; y <= rect.Bottom; y++)
				if(diff[x, y]) return true;
			return false;
		}
	}
}