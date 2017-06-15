using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;

namespace webdiff.img
{
	public class DirectBitmap : IDisposable
	{
		public unsafe DirectBitmap(Bitmap bmp)
		{
			this.bmp = bmp;
			data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
			ptr = (int*)data.Scan0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe Color GetPixel(int x, int y)
		{
			if(x < 0 || y < 0 || x >= data.Width || y >= data.Height)
				throw new ArgumentOutOfRangeException();
			return Color.FromArgb(ptr[y * data.Width + x]);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe void SetPixel(int x, int y, Color color)
		{
			if(x < 0|| y < 0 || x >= data.Width || y >= data.Height)
				throw new ArgumentOutOfRangeException();
			ptr[y * data.Width + x] = color.ToArgb();
		}

		public void Dispose()
		{
			bmp.UnlockBits(data);
		}

		private readonly Bitmap bmp;
		private readonly BitmapData data;
		private readonly unsafe int* ptr;
	}
}