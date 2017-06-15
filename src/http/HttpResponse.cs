using Newtonsoft.Json;

namespace webdiff.http
{
	[JsonObject(MemberSerialization.OptIn)]
	internal class HttpResponse
	{
		[JsonProperty("requestId")]        public string RequestId;
		[JsonProperty("url")]              public string Url;
		[JsonProperty("method")]           public string Method;
		[JsonProperty("frameId")]          public int FrameId;
		[JsonProperty("parentFrameId")]    public string ParentFrameId;
		[JsonProperty("tabId")]            public string TabId;
		[JsonProperty("type")]             public ResourceType Type;
		[JsonProperty("timeStamp")]        public double TimeStamp;
		[JsonProperty("statusLine")]       public string StatusLine;
		[JsonProperty("responseHeaders")]  public HttpHeader[] ResponseHeaders;
		[JsonProperty("statusCode")]       public int StatusCode;
	}

	[JsonObject(MemberSerialization.OptIn)]
	internal class HttpHeader
	{
		[JsonProperty("name")]             public string Name;
		[JsonProperty("value")]            public string Value;
		[JsonProperty("binaryValue")]      public byte[] BinaryValue;
	}

	internal enum ResourceType
	{
		Other = 0,
		Main_Frame,
		Sub_Frame,
		Stylesheet,
		Script,
		Image,
		Font,
		Object,
		Xmlhttprequest,
		Ping,
		Csp_Report,
		Media,
		Websocket
	}
}