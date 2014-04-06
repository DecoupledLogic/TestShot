namespace TestShot
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;

	public class TestRecording : TestScenarioBase
	{
		public TestRecording()
		{
			this.OpenDummy = false;
			this.ScreenSize = Constants.DefaultSize;
		}

		public string AppName { get; set; }

		public string AppVersion { get; set; }

		public string BaselineScenarioVersion { get; set; }

		public string BrowserVersion { get; set; }

		public DeviceType DeviceType { get; set; }

		public OsType OsType { get; set; }

		public string OsVersion { get; set; }

		public string RecordingVersion { get; set; }

		public Size ScreenSize { get; set; }
	}
}