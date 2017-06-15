using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;

namespace webdiff.driver
{
	internal static class WaitHelper
	{
		public static void Wait(this RemoteWebDriver driver, WaitSettings wait)
		{
			if(wait == null)
				return;

			if(wait.Elapsed != TimeSpan.Zero)
				Thread.Sleep(wait.Elapsed);

			new WebDriverWait(driver, Timeout) {PollingInterval = PollingInterval}.Until(d =>
				(wait.Exists == null || d.ExistsCondition(wait.Exists)) &&
				(wait.NotExists == null || d.NotExistsCondition(wait.NotExists)) &&
				(wait.AnyVisible == null || d.AnyVisibleCondition(wait.AnyVisible)) &&
				(wait.NoVisibles == null || d.NoVisiblesCondition(wait.NoVisibles)) &&
				(wait.TitleIs == null || d.TitleIsCondition(wait.TitleIs)) &&
				(wait.TitleNotIs == null || d.TitleNotIsCondition(wait.TitleNotIs)) &&
				(wait.JsCondition == null || d.JsCondition(wait.JsCondition)));
		}

		private static bool ExistsCondition(this IWebDriver driver, string selector)
			=> driver.FindElements(By.CssSelector(selector)).Any();

		private static bool NotExistsCondition(this IWebDriver driver, string selector)
			=> !driver.ExistsCondition(selector);

		private static bool AnyVisibleCondition(this IWebDriver driver, string selector)
			=> driver.FindElements(By.CssSelector(selector)).Any(IsDisplayed);

		private static bool NoVisiblesCondition(this IWebDriver driver, string selector)
			=> driver.FindElements(By.CssSelector(selector)).All(el => !el.IsDisplayed());

		private static bool TitleIsCondition(this IWebDriver driver, string title)
		{
			var match = Regex.Match(title, "/(?<regex>.*?)/(?<opt>i)?", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
			var options = match.Groups["opt"].Success ? RegexOptions.IgnoreCase : RegexOptions.None;
			return match.Success ? Regex.IsMatch(title, match.Groups["regex"].Value, RegexOptions.CultureInvariant | options) : driver.Title == title;
		}

		private static bool TitleNotIsCondition(this IWebDriver driver, string title)
			=> !driver.TitleIsCondition(title);

		private static bool JsCondition(this IWebDriver driver, string js)
			=> driver.ExecuteJavaScript<bool>($"return !!({js});");

		private static bool IsDisplayed(this IWebElement element)
		{
			try { return element.Displayed; }
			catch(StaleElementReferenceException) { return false; }
		}

		private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(60);
		private static readonly TimeSpan PollingInterval = TimeSpan.FromMilliseconds(100);
	}
}