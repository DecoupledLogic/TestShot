namespace TestShot
{
	using System;
	using System.Collections.Generic;
	using Newtonsoft.Json;
	using TestPipe.Core.Interfaces;

	public class TestScenario : TestScenarioBase
	{
		public TestScenario()
		{
			this.ErrorLog = new List<string>();
			this.Steps = new List<TestStep>();
			this.ScenarioStart = DateTime.Now;
			this.ScenarioLog = new List<string>();
			this.ScenarioLog.Add("Scenario Created: " + this.ScenarioStart.ToString());
		}

		[JsonIgnore]
		public IBrowser Browser { get; set; }

		[JsonIgnore]
		public IBrowser DummyBrowser { get; set; }

		public IList<string> ErrorLog { get; set; }

		//This scenario recording is the default when no specific scenario is chosen
		public bool IsBaseline { get; set; }

		public bool OverrideScreenshots { get; set; }

		public long RunTime { get; set; }

		public DateTime? ScenarioEnd { get; set; }

		public IList<string> ScenarioLog { get; set; }

		public DateTime ScenarioStart { get; set; }

		public string ScenarioVersion { get; set; }

		public int Screenshots { get; set; }

		public int ScreenshotsBaseline { get; set; }

		public string ScreenshotsPath { get; set; }

		public IList<TestStep> Steps { get; set; }

		public Guid TestRecordingId { get; set; }
	}
}