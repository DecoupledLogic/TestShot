namespace TestShot
{
	using System;

	public class TestRun
	{
		public TestRun()
		{
			this.ScreenSize = "1024x768";
			this.SleepFactor = 1.0;
			this.Browser = "firefox";
			this.DiffColor = "0,255,0";
		}

		public bool AutoRecord { get; set; }

		public string Browser { get; set; }

		public string DiffColor { get; set; }

		public string FileName { get; set; }

		public string Local { get; set; }

		public string PostData { get; set; }

		public bool Record { get; set; }

		public string Remote { get; set; }

		public bool Rerecord { get; set; }

		public bool SaveDiff { get; set; }

		public string ScreenSize { get; set; }

		public double SleepFactor { get; set; }

		public string TestName { get; set; }

		public string Url { get; set; }
	}
}