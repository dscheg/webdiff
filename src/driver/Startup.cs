using System;
using System.Drawing;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Opera;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Safari;
using webdiff.utils;

namespace webdiff.driver
{
	internal static class Startup
	{
		public static RemoteWebDriver StartNewDriver(DriverSettings settings, MobileSettings mobile)
		{
			switch(settings.Browser)
			{
				case Browser.Chrome:   return StartChromeDriver(settings, mobile);
				case Browser.Firefox:  return StartFirefoxDriver(settings);
				//case Browser.Opera:    return StartOperaDriver(settings);
				//case Browser.Safari:   return StartSafariDriver(settings);
				//case Browser.IE:       return StartIeDriver(settings);
				//case Browser.Edge:     return StartEdgeDriver(settings);
				default:               throw new Exception("Unsupported browser");
			}
		}

		public static RemoteWebDriver SetWindowSettings(this RemoteWebDriver driver, WindowSettings settings)
		{
			if(settings.Maximize)
				driver.Manage().Window.Maximize();
			else
				driver.Manage().Window.Size = new Size(settings.Width, settings.Height);
			return driver;
		}

		private static RemoteWebDriver StartChromeDriver(DriverSettings settings, MobileSettings mobile)
		{
			var options = new ChromeOptions();
			settings.Capabilities?.ForEach(cap => options.AddAdditionalCapability(cap.Key, cap.Value));
			if(!string.IsNullOrEmpty(settings.BrowserBinaryPath))
				options.BinaryLocation = settings.BrowserBinaryPath;
			if(!string.IsNullOrEmpty(settings.Proxy))
				options.Proxy = new Proxy {HttpProxy = settings.Proxy};
			if(settings.BrowserBinaryPath != null)
				options.BinaryLocation = settings.BrowserBinaryPath;
			if(settings.CmdArgs != null)
				options.AddArguments(settings.CmdArgs);
			if(settings.Extensions != null)
				options.AddExtensions(settings.Extensions);
			settings.ProfilePrefs?.ForEach(pref => options.AddUserProfilePreference(pref.Key, pref.Value));
			if(mobile != null && mobile.Enable)
			{
				if(!string.IsNullOrEmpty(mobile.DeviceName))
					options.EnableMobileEmulation(mobile.DeviceName);
				else
				{
					var deviceSettings = new ChromeMobileEmulationDeviceSettings
					{
						EnableTouchEvents = mobile.EnableTouchEvents,
						Width = mobile.Width,
						Height = mobile.Height,
						PixelRatio = mobile.PixelRatio
					};
					options.EnableMobileEmulation(deviceSettings);
				}
			}
			return new ChromeDriver(options);
		}

		private static RemoteWebDriver StartFirefoxDriver(DriverSettings settings)
		{
			var profile = new FirefoxProfile(null, true);
			if(!string.IsNullOrEmpty(settings.Proxy))
				profile.SetProxyPreferences(new Proxy {HttpProxy = settings.Proxy});
			settings.Extensions?.ForEach(ext => profile.AddExtension(ext));
			var options = new FirefoxOptions {Profile = profile, UseLegacyImplementation = false};
			settings.Capabilities?.ForEach(cap => options.AddAdditionalCapability(cap.Key, cap.Value));
			if(!string.IsNullOrEmpty(settings.BrowserBinaryPath))
				options.BrowserExecutableLocation = settings.BrowserBinaryPath;
			if(settings.CmdArgs != null)
				options.AddArguments(settings.CmdArgs);
			settings.ProfilePrefs?.ForEach(pref => options.SetPreference(pref.Key, (dynamic)pref.Value));
			return new FirefoxDriver(options);
		}

		private static RemoteWebDriver StartOperaDriver(DriverSettings settings)
		{
			var options = new OperaOptions();
			settings.Capabilities?.ForEach(cap => options.AddAdditionalCapability(cap.Key, cap.Value));
			if(!string.IsNullOrEmpty(settings.BrowserBinaryPath))
				options.BinaryLocation = settings.BrowserBinaryPath;
			if(!string.IsNullOrEmpty(settings.Proxy))
				options.Proxy = new Proxy {HttpProxy = settings.Proxy};
			if(settings.BrowserBinaryPath != null)
				options.BinaryLocation = settings.BrowserBinaryPath;
			if(settings.CmdArgs != null)
				options.AddArguments(settings.CmdArgs);
			if(settings.Extensions != null)
				options.AddExtensions(settings.Extensions);
			return new OperaDriver(options);
		}

		private static RemoteWebDriver StartSafariDriver(DriverSettings settings)
		{
			var options = new SafariOptions();
			settings.Capabilities?.ForEach(cap => options.AddAdditionalCapability(cap.Key, cap.Value));
			return new SafariDriver(options);
		}

		private static RemoteWebDriver StartIeDriver(DriverSettings settings)
		{
			var options = new InternetExplorerOptions();
			settings.Capabilities?.ForEach(cap => options.AddAdditionalCapability(cap.Key, cap.Value));
			if(settings.CmdArgs != null)
				options.BrowserCommandLineArguments = string.Join(" ", settings.CmdArgs);
			return new InternetExplorerDriver(options);
		}

		private static RemoteWebDriver StartEdgeDriver(DriverSettings settings)
		{
			var options = new EdgeOptions();
			settings.Capabilities?.ForEach(cap => options.AddAdditionalCapability(cap.Key, cap.Value));
			return new EdgeDriver(options);
		}
	}
}