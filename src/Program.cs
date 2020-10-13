using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using NDesk.Options;
using Newtonsoft.Json;
using webdiff.driver;
using webdiff.img;
using webdiff.utils;
using Cookie = OpenQA.Selenium.Cookie;

namespace webdiff
{
	internal static class Program
	{
		private enum Error
		{
			None = 0,
			InvalidArgs = 1,
			CestLaVie = 2
		}

		private static int Main(string[] args)
		{
			Error error = 0;

			var output = ".";
			var profile = "profile.toml";
			var template = "template.html";
			var showHelpAndExit = false;
			var useAbsoluteLinks = false;

			var options = new OptionSet
			{
				{"o|output=", $"Reports output directory\n(default: '{output}')", v => output = v},
				{"p|profile=", $"Profile TOML file with current settings\n(default: '{profile}')", v => profile = v},
				{"t|template=", $"HTML report template file\n(default: '{template}')", v => template = v},
				{"l|full-links", "Use absolute links instead of relative", v => useAbsoluteLinks = v != null},
				{"h|help", "Show this message", v => showHelpAndExit = v != null}
			};

			List<string> free = null;
			try
			{
				free = options.Parse(args);
			}
			catch(OptionException e)
			{
				Console.Error.WriteLine("Option '{0}' value is invalid: {1}", e.OptionName, e.Message);
				Console.Error.WriteLine();
				error = Error.InvalidArgs;
			}

			if(!PathHelper.FileExistsWithOptionalExt(ref profile, ".toml"))
			{
				Console.Error.WriteLine($"Profile '{profile}' not found");
				Console.Error.WriteLine();
				error = Error.InvalidArgs;
			}

			if(!PathHelper.FileExistsWithOptionalExt(ref template, ".html"))
			{
				Console.Error.WriteLine($"Template '{template}' not found");
				Console.Error.WriteLine();
				error = Error.InvalidArgs;
			}

			Settings settings = null;
			try
			{
				settings = Settings.Read(profile.RelativeToBaseDirectory());
			}
			catch(Exception e)
			{
				Console.Error.WriteLine($"Failed to read profile '{profile}': {e.Message}");
				Console.Error.WriteLine();
				error = Error.InvalidArgs;
			}


			Uri svcLeft = null;
			Uri svcRight = null;
			if (!useAbsoluteLinks)
			{
				try
				{
					if (free?.Count >= 2)
					{
						svcLeft = new Uri(free[0]);
						svcRight = new Uri(free[1]);
					}
				}
				catch (Exception e)
				{
					Console.Error.WriteLine(e.Message);
					Console.Error.WriteLine();
					error = Error.InvalidArgs;
				}
			}

			var input = free?.Skip(useAbsoluteLinks ? 0 : 2).FirstOrDefault();
			if(input != null && !File.Exists(input.RelativeToBaseDirectory()))
			{
				Console.Error.WriteLine("Input file with URLs not found");
				Console.Error.WriteLine();
				error = Error.InvalidArgs;
			}

			if(showHelpAndExit || error != 0 || settings == null || free == null)
			{
				Console.WriteLine("Usage: webdiff [OPTIONS] URL1 URL2 [FILE]");
				Console.WriteLine("Usage: webdiff -l [OPTIONS] [FILE]");
				Console.WriteLine("Options:");
				options.WriteOptionDescriptions(Console.Out);
				Console.WriteLine();
				Console.WriteLine("Examples:");
				Console.WriteLine("  webdiff http://prod.example.com http://test.example.com < URLs.txt");
				Console.WriteLine("  webdiff -p profile.toml -t template.html -o data http://prod.example.com http://test.example.com URLs.txt");
				Console.WriteLine();
				return (int)error;
			}

			Cookie[] cookies = null;
			try
			{
				if(settings.Driver.Cookies != null)
					cookies = CookieParser.Parse(settings.Driver.Cookies.RelativeToBaseDirectory());
			}
			catch(Exception e)
			{
				Console.Error.WriteLine($"Failed to parse cookies file: {e.Message}");
				return (int)Error.InvalidArgs;
			}

			var started = DateTime.Now;

			try
			{
				ResultsPath = Path.Combine(output, started.ToString("yyyyMMdd-HHmmss")).RelativeToBaseDirectory();
				ResultsImgPath = Path.Combine(ResultsPath, ImgRelativePath);

				Directory.CreateDirectory(ResultsPath);
				Directory.CreateDirectory(ResultsImgPath);
			}
			catch(Exception e)
			{
				Console.Error.WriteLine($"Failed to create output directory '{ResultsPath}': {e.Message}");
				return (int)Error.CestLaVie;
			}

			DriveContainer[] drivers = null;

			try
			{
				if (useAbsoluteLinks)
					drivers = Enumerable.Range(0, 2)
						.AsParallel().AsOrdered()
						.Select(_ => DriveContainer.Create(settings))
						.ToArray();
				else
					drivers = new[] {svcLeft, svcRight}
						.AsParallel().AsOrdered()
						.Select(uri => DriveContainer.Create(settings, uri))
						.ToArray();

				if(drivers.Any(driver => !driver.IsInitialized))
					return (int)Error.CestLaVie;

				drivers.ForEach(driver => driver.Cookies = cookies);

				var results = new Results
				{
					Started = started,
					LeftBase = svcLeft,
					RightBase = svcRight,
					Profile = profile,
					Diffs = new List<Diff>()
				};

				int errors =
					(input == null ? Console.In.ReadLines() : File.ReadLines(input.RelativeToBaseDirectory()))
					.Select(line => line.Trim())
					.Where(line => line != string.Empty)
					.Where(line => !line.StartsWith("#", StringComparison.Ordinal))
					.Select((line, idx) => Cmp(settings, drivers, line, idx, results, useAbsoluteLinks))
					.Count(res => !res);

				results.Ended = DateTime.Now;
				results.Elapsed = results.Ended - results.Started;

				File.Copy(template.RelativeToBaseDirectory(), Path.Combine(ResultsPath, HtmlReportFilename));
				File.WriteAllText(Path.Combine(ResultsPath, JsResultsFilename), $"{JsRenderFunctionName}({JsonConvert.SerializeObject(results, Formatting.Indented, new SizeConverter(), new RectangleConverter())});{Environment.NewLine}");

				Console.WriteLine();

				var result = errors == 0;
				WriteResult(result, result ? "ALL SAME" : $"DIFFERS ({errors})");

				return (int)Error.None;
			}
			catch(Exception e)
			{
				Console.Error.WriteLine(e);
				return (int)Error.CestLaVie;
			}
			finally
			{
				drivers.AsParallel().ForAll(driver => driver?.Dispose());
			}
		}

		//TODO: Errors handling and refactoring
		private static bool Cmp(Settings settings, DriveContainer[] drivers, string test, int idx, Results results, bool useAbsoluteLinks)
		{
			var isScript = StringUtils.RemovePrefix(ref test, "EXEC ");

			var urls = !isScript && useAbsoluteLinks
				? test.Split(new []{'\t'}, 2)
				: Enumerable.Repeat(test, 2);

			var pages = drivers.Zip(urls, (driver, url) => (driver, url))
				.AsParallel().AsOrdered()
				.Select(tuple => tuple.driver.ProcessRequest(tuple.url, isScript))
				.ToArray();

			var pageLeft = pages[0];
			var pageRight = pages[1];
			Bitmap imgLeft = pageLeft.Bmp, imgRight = pageRight.Bmp, diff;

			int pixels;
			var result = ImageDiff.CompareImages(settings, imgLeft, imgRight, out diff);
			var areSame = (pixels = result.Unmatched) <= settings.Compare.PixelsThreshold || result.Map == null || result.Map.Count == 0;

			var match = 1.0 - (double)pixels / diff.Width / diff.Height;

			WriteResult(areSame, areSame ? "Same: " : "Diff: ", test);
			WriteInfo($"      Match {match:P1} ({pixels} / {diff.Width * diff.Height} pixels)");

			string leftName = null, rightName = null, diffName = null;

			var name = $"{idx:0000}-" + (useAbsoluteLinks ? "different" : test.Trim('/').ToSafeFilename());
			if(!areSame)
			{
				leftName = name + "-left.png";
				imgLeft.Save(Path.Combine(ResultsImgPath, leftName), ImageFormat.Png);

				rightName = name + "-right.png";
				imgRight.Save(Path.Combine(ResultsImgPath, rightName), ImageFormat.Png);

				diffName = name + "-diff.png";
				diff.Save(Path.Combine(ResultsImgPath, diffName), ImageFormat.Png);
			}

			results.TotalCount++;
			if(areSame) results.SameCount++;
			else results.DiffCount++;

			results.Diffs.Add(new Diff
			{
				Relative = useAbsoluteLinks ? "different" : test,
				AreSame = areSame,
				UnmatchedPixels = result.Unmatched,
				TotalPixels = diff.Width * diff.Height,
				Match = match,
				Left = new Page
				{
					Url = pageLeft.RequestUri,
					Response = pageLeft.Http,
					Img = new Img
					{
						Filename = leftName,
						Src = leftName == null ? null : Path.Combine(ImgRelativePath, WebUtility.UrlEncode(leftName)),
						Size = pageLeft.Bmp.Size
					}
				},
				Right = new Page
				{
					Url = pageRight.RequestUri,
					Response = pageRight.Http,
					Img = new Img
					{
						Filename = rightName,
						Src = rightName == null ? null : Path.Combine(ImgRelativePath, WebUtility.UrlEncode(rightName)),
						Size = pageRight.Bmp.Size
					}
				},
				DiffImg = new Img
				{
					Filename = diffName,
					Src = diffName == null ? null : Path.Combine(ImgRelativePath, WebUtility.UrlEncode(diffName)),
					Size = diff.Size
				},
				DiffMap = result.Map
			});

			return areSame;
		}

		private static void WriteResult(bool success, string prefix, string text = null)
		{
			lock(Console.Out)
			{
				Console.ForegroundColor = success ? ConsoleColor.Green : ConsoleColor.Red;
				Console.Write(prefix);
				Console.ResetColor();
				Console.WriteLine(text);
			}
		}

		private static void WriteInfo(string info)
		{
			lock(Console.Out)
			{
				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.WriteLine(info);
				Console.ResetColor();
			}
		}

		private const string ImgRelativePath = "img";

		private const string HtmlReportFilename = "index.html";
		private const string JsRenderFunctionName = "render";
		private const string JsResultsFilename = "results.js";

		private static string ResultsPath;
		private static string ResultsImgPath;
	}
}