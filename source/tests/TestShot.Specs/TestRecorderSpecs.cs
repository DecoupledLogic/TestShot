namespace TestShot.Specs
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Converters;
	using TestPipe.Core.Control;
	using TestPipe.Core.Enums;
	using TestPipe.Core.Interfaces;
	using Action = TestShot.Action;

	[TestClass]
	public class TestRecorderSpecs
	{
		private TestRecorder sut;
		private TestRecording recording;
		private TestScenario scenario;

		[TestInitialize]
		public void Setup()
		{
			this.recording = this.GetTestRecording();
			this.scenario = this.GetTestScenario(this.recording);
			this.sut = new TestRecorder();
		}

		[TestMethod]
		public void RecordStartsScenarioRecording()
		{
			this.sut.Record(this.scenario);
			this.scenario.Browser.Quit();
			Assert.IsTrue(this.scenario.ErrorLog.Count < 1);
		}

		[TestMethod]
		public void StopSavesScenarioRecording()
		{
			this.sut.Record(this.scenario);

			BaseControl control = new BaseControl(this.scenario.Browser, "seeMe");
			control.Click();
			
			TestStep step = new TestStep() { Action = Action.Screenshot, Timestamp = DateTime.Now.Ticks };
			this.scenario.Steps.Add(step);

			this.sut.Stop(this.scenario);

			Assert.IsTrue(this.scenario.Steps.Single(x => x.Action == Action.Click).Action == Action.Click);
			Assert.IsTrue(this.scenario.Steps.Single(x => x.Action == Action.Screenshot).Action == Action.Screenshot);
		}

		[TestMethod]
		public void MergeStepsJoinsAndSortsListsRemovesOneInvalidStep()
		{ 
			//Simulate Screenshots
			IList<TestStep> stepsA = new List<TestStep>();
			
			TestStep stepA = new TestStep() { Action = Action.Screenshot, Timestamp = DateTime.Parse("2/25/2014 5:00 PM").Ticks, X = 5, Y = 5 };
			stepsA.Add(stepA);
			
			stepA = new TestStep() { Action = Action.Screenshot, Timestamp = DateTime.Parse("2/25/2014 5:02 PM").Ticks, X = 10, Y = 10 };
			stepsA.Add(stepA);
			
			//Simulate Browser Steps
			IList<TestStep> stepsB = new List<TestStep>();
			
			TestStep stepB = new TestStep() { Action = Action.Click, Timestamp = DateTime.Parse("2/25/2014 5:01 PM").Ticks, X = 6, Y = 6 };
			stepsB.Add(stepB);
			
			//Simulate Browser Step after last screenshot, any steps after last screen shots should not be saved
			stepB = new TestStep() { Action = Action.Click, Timestamp = DateTime.Parse("2/25/2014 5:03 PM").Ticks, X = 12, Y = 12 };
			stepsB.Add(stepB);

			IList<TestStep> merged = this.sut.MergeSteps(stepsA, stepsB);

			Assert.IsTrue(merged.Single(x => x.Action == Action.Click).Action == Action.Click);
			Assert.IsTrue(merged.Where(x => x.Action == Action.Screenshot).Count() == 2);
		}

		[TestMethod]
		public void JsonDeserializeStep()
		{
			string json = "{\"Action\":\"Scroll\",\"Timestamp\":1393364062113,\"X\":0,\"Y\":166}";
			TestStep steps = JsonConvert.DeserializeObject<TestStep>(json, new IsoDateTimeConverter());

			Assert.IsNotNull(steps);
		}

		private TestRecording GetTestRecording()
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

		private TestScenario GetTestScenario(TestRecording recording)
		{
			TestScenarioManager manager = new TestScenarioManager();
			TestScenario scenario = manager.Create(recording, "1", false);
			return scenario;
		}
	}
}
