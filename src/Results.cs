using System;
using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using webdiff.http;

namespace webdiff
{
	[JsonObject(MemberSerialization.OptIn)]
	internal class Results
	{
		[JsonProperty("started")]      public DateTime Started;
		[JsonProperty("ended")]        public DateTime Ended;
		[JsonProperty("elapsed")]      public TimeSpan Elapsed;

		[JsonProperty("profile")]      public string Profile;

		[JsonProperty("leftBase")]     public Uri LeftBase;
		[JsonProperty("rightBase")]    public Uri RightBase;

		[JsonProperty("diffs")]        public List<Diff> Diffs;

		[JsonProperty("sameCount")]    public int SameCount;
		[JsonProperty("diffCount")]    public int DiffCount;
		[JsonProperty("totalCount")]   public int TotalCount;
	}

	[JsonObject(MemberSerialization.OptIn)]
	internal class Diff
	{
		[JsonProperty("relative")]     public string Relative;

		[JsonProperty("left")]         public Page Left;
		[JsonProperty("right")]        public Page Right;

		[JsonProperty("areSame")]      public bool AreSame;
		[JsonProperty("unmatchedPxs")] public int UnmatchedPixels;
		[JsonProperty("totalPxs")]     public int TotalPixels;
		[JsonProperty("match")]        public double Match;

		[JsonProperty("diffImg")]      public Img DiffImg;
		[JsonProperty("diffMap")]      public List<Rectangle> DiffMap;
	}

	[JsonObject(MemberSerialization.OptIn)]
	internal class Page
	{
		[JsonProperty("url")]          public Uri Url;
		[JsonProperty("response")]     public HttpResponse Response;
		[JsonProperty("img")]          public Img Img;
	}

	[JsonObject(MemberSerialization.OptIn)]
	internal class Img
	{
		[JsonProperty("filename")]     public string Filename;
		[JsonProperty("src")]          public string Src;
		[JsonProperty("size")]         public Size Size;
	}

	internal class SizeConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var rectangle = (Size)value;
			JObject.FromObject(new {width = rectangle.Width, height = rectangle.Height}).WriteTo(writer);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
			=> throw new NotImplementedException();

		public override bool CanConvert(Type objectType) => typeof(Size).IsAssignableFrom(objectType);
	}

	internal class RectangleConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var rectangle = (Rectangle)value;
			JObject.FromObject(new {x = rectangle.X, y = rectangle.Y, width = rectangle.Width, height = rectangle.Height}).WriteTo(writer);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
			=> throw new NotImplementedException();

		public override bool CanConvert(Type objectType) => typeof(Rectangle).IsAssignableFrom(objectType);
	}
}