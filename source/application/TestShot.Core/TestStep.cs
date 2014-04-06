namespace TestShot
{
	using System;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Converters;

	public class TestStep
	{
		[JsonConverter(typeof(StringEnumConverter))]
		public Action Action { get; set; }

		public string Key { get; set; }

		public bool LivePlayback { get; set; }

		public string Note { get; set; }

		public int PauseMilliseconds { get; set; }

		public long Timestamp { get; set; }

		[JsonIgnore]
		public DateTime Date { get { return new DateTime(this.Timestamp); } }

		public int X { get; set; }

		public int Y { get; set; }
	}
}