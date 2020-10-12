using System;
using System.Drawing;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using webdiff.http;
using webdiff.utils;

namespace webdiff.driver
{
	internal class DriveContainer : IDisposable
	{
		private readonly Settings settings;
		private RemoteWebDriver driver;

		protected DriveContainer(Settings settings, RemoteWebDriver driver)
		{
			this.settings = settings;
			this.driver = driver;
		}

		public static DriveContainer Create(Settings settings, Uri baseUri)
		{
			RemoteWebDriver driver = null;
			try
			{
				driver = Startup.StartNewDriver(settings.Driver, settings.Mobile).SetWindowSettings(settings.Window);
				Console.Error.WriteLine("Started driver, {0}", driver.Capabilities);
				if (baseUri != null)
					driver.Url = baseUri.ToString();
			}
			catch(Exception e)
			{
				Console.Error.WriteLine($"Failed to start driver: {e.Message}");
			}
			return new DriveContainerWithBase(settings, driver, baseUri);
		}

		public static DriveContainer Create(Settings settings)
		{
			RemoteWebDriver driver = null;
			try
			{
				driver = Startup.StartNewDriver(settings.Driver, settings.Mobile).SetWindowSettings(settings.Window);
				Console.Error.WriteLine("Started driver, {0}", driver.Capabilities);
			}
			catch(Exception e)
			{
				Console.Error.WriteLine($"Failed to start driver: {e.Message}");
			}
			return new DriveContainer(settings, driver);
		}

		public void Dispose()
		{
			driver?.Dispose();
			driver = null;
		}

		public bool IsInitialized => driver != null;

		public Cookie[] Cookies
		{
			set => value?.ForEach(cookie => driver.Manage().Cookies.AddCookie(cookie));
		}

		public (Uri RequestUri, HttpResponse Http, Bitmap Bmp, string ResultUrl) ProcessRequest(string request, bool isScript)
		{
			Uri requestUri = null;
			if (isScript)
				driver.ExecuteScript(request);
			else
				requestUri = GoToUrl(request);

			if (settings.Script.OnLoad != null)
				driver.ExecuteScript(settings.Script.OnLoad);

			driver.Wait(settings.WaitUntil);

			return (RequestUri: requestUri, Http: driver.GetHttpResponse(), Bmp: driver.GetVertScrollScreenshot(settings), ResultUrl: driver.Url);

		}

		protected virtual Uri GoToUrl(string url) => GoToUrl(new Uri(url));

		protected virtual Uri GoToUrl(Uri uri)
		{
			driver.Navigate().GoToUrl(uri);
			return uri;
		}
	}

	internal class DriveContainerWithBase : DriveContainer
	{
		private readonly Uri baseUri;

		public DriveContainerWithBase(Settings settings, RemoteWebDriver driver, Uri baseUri) : base(settings, driver)
		{
			this.baseUri = baseUri;
		}

		protected override Uri GoToUrl(string url) => base.GoToUrl(new Uri(baseUri, url));
	}
}