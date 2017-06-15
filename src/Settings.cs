using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Nett;
using webdiff.img;

namespace webdiff
{
	internal class Settings
	{
		public static Settings Read(string filename)
		{
			var config = TomlConfig.Create(cfg => cfg.ConfigureType<Color>(type => type.WithConversionFor<TomlString>(convert => convert.FromToml(val => ColorHelper.Parse(val.Value)))));
			return filename == null ? new Settings() : Toml.ReadFile<Settings>(filename, config);
		}

		public DriverSettings Driver { get; set; } = new DriverSettings();
		public MobileSettings Mobile { get; set; } = new MobileSettings();
		public WindowSettings Window { get; set; } = new WindowSettings();
		public ScriptSettings Script { get; set; } = new ScriptSettings();
		public WaitSettings WaitUntil { get; set; } = new WaitSettings();
		public VisualSettings Visual { get; set; } = new VisualSettings();
		public CompareSettings Compare { get; set; } = new CompareSettings();
	}

	internal class DriverSettings
	{
		public Browser Browser { get; set; } = Browser.Chrome;

		public string BrowserBinaryPath { get; set; } = null;
		public string[] CmdArgs { get; set; } = null;
		public string[] Extensions { get; set; } = null;
		public Dictionary<string, object> ProfilePrefs { get; set; } = null;
		public Dictionary<string, object> Capabilities { get; set; } = null;

		public string Proxy { get; set; } = null;
		public string Cookies { get; set; } = null;
	}

	internal class MobileSettings
	{
		public bool Enable { get; set; } = false;
		public string DeviceName { get; set; } = "Nexus 5X";

		public int Width { get; set; } = 360;
		public int Height { get; set; } = 640;

		public double PixelRatio { get; set; } = 3.0;
		public bool EnableTouchEvents { get; set; } = true;
	}

	internal class WindowSettings
	{
		public bool Maximize { get; set; } = true;

		public int Width { get; set; } = 1024;
		public int Height { get; set; } = 768;
	}

	internal class ScriptSettings
	{
		public string OnLoad { get; set; } = null;
	}

	internal class WaitSettings
	{
		public TimeSpan Elapsed { get; set; } = TimeSpan.Zero;

		public string Exists { get; set; } = null;
		public string NotExists { get; set; } = null;

		public string AnyVisible { get; set; } = null;
		public string NoVisibles { get; set; } = null;

		public string TitleIs { get; set; } = null;
		public string TitleNotIs { get; set; } = null;

		public string JsCondition { get; set; } = null;
	}

	internal class CompareSettings
	{
		public bool VScroll { get; set; } = true;

		public int ColorThreshold { get; set; } = 0;
		public int PixelsThreshold { get; set; } = 0;
		public int DiffSideThreshold { get; set; } = 0;
	}

	internal class VisualSettings
	{
		public bool Border { get; set; } = true;

		public Color BorderColor { get; set; } = Color.FromArgb(0x7f, Color.Red);
		public float BorderWidth { get; set; } = 2.0f;

		public int BorderPadding { get; set; } = 4;
		public int BorderSpacing { get; set; } = 10;

		public HatchStyle FillStyle { get; set; } = HatchStyle.OutlinedDiamond;
		public Color FillBackColor { get; set; } = Color.FromArgb(0x3f, Color.Red);
		public Color FillForeColor { get; set; } = Color.FromArgb(0x1f, Color.Red);

		public HatchStyle OverflowFillStyle { get; set; } = HatchStyle.DiagonalCross;
		public Color OverflowFillBackColor { get; set; } = Color.FromArgb(0x3f, Color.Blue);
		public Color OverflowFillForeColor { get; set; } = Color.FromArgb(0x1f, Color.Blue);
	}

	internal enum Browser
	{
		Unknown = 0,
		Chrome,
		Firefox,
		Opera,
		Safari,
		IE,
		Edge
	}
}