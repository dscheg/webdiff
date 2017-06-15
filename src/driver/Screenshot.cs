using System;
using System.Drawing;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.Extensions;
using webdiff.img;

namespace webdiff.driver
{
	internal static class Screenshot
	{
		public static Bitmap GetVertScrollScreenshot(this RemoteWebDriver driver, Settings settings)
		{
			var bmp = BitmapHelper.FromByteArray(driver.GetScreenshot().AsByteArray);
			if(!settings.Compare.VScroll)
				return bmp;

			var innerHeight = checked((int)driver.ExecuteJavaScript<long>(InnerHeightJs));
			var scrollHeight = checked((int)driver.ExecuteJavaScript<long>(BodyScrollHeightJs));
			if(scrollHeight <= innerHeight)
				return bmp;

			var composite = new Bitmap(bmp.Width, scrollHeight);
			using(var g = Graphics.FromImage(composite))
			{
				g.DrawImage(bmp, 0, 0);
				for(int i = 1; i <= scrollHeight / innerHeight; i++)
				{
					driver.ExecuteScript(ScrollByInnerHeightJs);
					bmp = BitmapHelper.FromByteArray(driver.GetScreenshot().AsByteArray);
					int top = innerHeight * i, height = Math.Min(scrollHeight - top, bmp.Height);
					g.DrawImage(bmp, 0, top, new Rectangle(0, bmp.Height - height, bmp.Width, height), GraphicsUnit.Pixel);
				}
			}

			return composite;
		}

		private const string InnerHeightJs = "return window.innerHeight;";
		private const string BodyScrollHeightJs = "return document.body.scrollHeight;";
		private const string ScrollByInnerHeightJs = "window.scrollBy(0,window.innerHeight);";
	}
}