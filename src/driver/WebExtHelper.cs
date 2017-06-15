using Newtonsoft.Json;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.Extensions;
using webdiff.http;

namespace webdiff.driver
{
	internal static class WebExtHelper
	{
		public static HttpResponse GetHttpResponse(this RemoteWebDriver driver)
		{
			var response = driver.ExecuteJavaScript<string>(GetHttpFromSessionStorageJs);
			return response == null ? null : JsonConvert.DeserializeObject<HttpResponse>(response);
		}

		private const string GetHttpFromSessionStorageJs = "return sessionStorage.getItem('webdiff.http');";
	}
}