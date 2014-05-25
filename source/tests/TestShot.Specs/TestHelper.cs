namespace TestShot.Specs
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using TestPipe.Core.Control;
	using TestPipe.Core.Enums;
	using TestPipe.Core.Interfaces;
	using Action = TestShot.Action;

	public class TestHelper
	{
		public static TestRecording GetTestRecording()
		{
			TestRecording recording = new TestRecording();
			recording.AppName = "Test App";
			recording.AppVersion = "1.0.0";
			recording.BrowserType = BrowserTypeEnum.FireFox;
			recording.Name = "Test Scenario";
			recording.OpenDummy = true;
			recording.ScreenSize = Constants.DefaultSize;
			recording.EnvironmentUrl = "localhost/testpipe.testsite/";
			recording.TestVirtualUrl = "testshot.html";
			recording.Id = Guid.NewGuid();
			recording = TestRecordingManager.ResetTestRecording(recording, "1");
			return recording;
		}

		public static TestScenario GetTestScenario(TestRecording recording)
		{
			TestScenarioManager manager = new TestScenarioManager();
			TestScenario scenario = manager.Create(recording, "1", false);
			return scenario;
		}
	}
}
