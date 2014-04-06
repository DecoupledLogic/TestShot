namespace TestShot
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Configuration;
	using System.IO;
	using System.Linq;
	using System.Threading;
	using AutoMapper;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Converters;
	using TestPipe.Common;
	using TestPipe.Core;
	using TestPipe.Core.Browser;

	public class TestRecorder : ITestRecorder
	{
		private static readonly string EagleEyeInjectedMessage = "EagleEye Injected";
		private static readonly string EagleEyeNotFoundMessage = "EagleEye not found.";
		private static readonly string StopRecordingMessage = "Scenario End.";
		private static readonly string TestPageNotOpenMessage = "Test Page was not opened.";
		private static readonly string TestScenarioNullMessage = "TestScenario is null.";

		public TestRecorder()
		{
		}

		public IList<TestStep> MergeSteps(IList<TestStep> screenshotSteps, IList<TestStep> browserSteps)
		{
			foreach (TestStep step in browserSteps)
			{
				screenshotSteps.Add(step);
			}

			screenshotSteps = screenshotSteps.ToList().OrderBy(x => x.Timestamp).ToList();
			TestStep lastScreenshot = screenshotSteps.LastOrDefault(x => x.Action == Action.Screenshot);

			List<TestStep> results = screenshotSteps.Where(x => x.Timestamp <= lastScreenshot.Timestamp).ToList();

			return results;
		}

		public IList<TestStep> Poll(TestScenario scenario)
		{
			if (scenario == null)
			{
				throw new ArgumentNullException(TestRecorder.TestScenarioNullMessage);
			}

			object value = scenario.Browser.ExecuteScript("return window._getTestShotEvents();");

			if (value == null)
			{
				return null;
			}

			IList<TestStep> steps = this.DeserializeSteps(value);
			return steps;
		}

		public TestScenario Record(TestScenario scenario)
		{
			if (scenario == null)
			{
				throw new ArgumentNullException(TestRecorder.TestScenarioNullMessage);
			}

			scenario.ScenarioStart = DateTime.Now;
			Logger log = new Logger();
			scenario.Browser = BrowserFactory.Create(scenario.BrowserType, log);

			try
			{
				this.OpenDummyDriver(scenario, log);
				scenario.Browser.Open(scenario.TestUrl, 2);

				if (scenario.Browser.HasUrl(scenario.TestUrl))
				{
					scenario.ScenarioLog.Add(string.Format("Test Page Opened: {0}", scenario.TestUrl));
				}
				else
				{
					throw new Exception(TestRecorder.TestPageNotOpenMessage);
				}

				this.InjectEagleEye(scenario);

				//If EagleEye was successfully injected then the last log message will state that it was injected.
				if (!scenario.ScenarioLog.LastOrDefault().Contains(TestRecorder.EagleEyeInjectedMessage))
				{
					throw new Exception(TestRecorder.EagleEyeNotFoundMessage);
				}

				return scenario;
			}
			catch (Exception ex)
			{
				scenario.ErrorLog = this.LogException("Exception caught while trying to start recording.", scenario.ErrorLog, ex);
				scenario.ScenarioEnd = DateTime.Now;
				scenario.RunTime = Common.GetDateDiffInMilliseconds(scenario.ScenarioEnd.Value, scenario.ScenarioStart);
				this.Quit(scenario);
				return scenario;
			}
		}

		public TestScenario Stop(TestScenario scenario)
		{
			if (scenario == null)
			{
				throw new ArgumentNullException(TestRecorder.TestScenarioNullMessage);
			}

			if (scenario.ScenarioLog.LastOrDefault().Count() > 0 && scenario.ScenarioLog.LastOrDefault().Contains(TestRecorder.StopRecordingMessage))
			{
				this.Quit(scenario);
				return scenario;
			}

			try
			{
				IList<TestStep> browserSteps = this.GetEagleEyeResults(scenario);
				IList<TestStep> browserAndUiSteps = this.MergeSteps(scenario.Steps, browserSteps);
				browserAndUiSteps = this.CleanSteps(browserAndUiSteps);
				scenario.Steps = browserAndUiSteps;
				scenario.ScenarioEnd = DateTime.Now;
				scenario.RunTime = Common.GetDateDiffInMilliseconds(scenario.ScenarioEnd.Value, scenario.ScenarioStart);
				scenario.ScenarioLog.Add(TestRecorder.StopRecordingMessage);
				scenario = this.SaveScenario(scenario);
			}
			finally
			{
				this.Quit(scenario);
			}

			return scenario;
		}

		private IList<TestStep> CleanSteps(IList<TestStep> browserAndScreenshotRecording)
		{
			// TODO: warn if page switched (can't get events)
			browserAndScreenshotRecording = this.InsertPauseSteps(browserAndScreenshotRecording);
			int i = 0;
			foreach (TestStep step in browserAndScreenshotRecording)
			{
				//Not sure if I should delete these
				// no need for these keys anymore
				if (step.LivePlayback)
				{
					browserAndScreenshotRecording.RemoveAt(i);
				}
				//Not sure if I should delete these
				//delete event.timeStamp;
				i++;
			}

			return browserAndScreenshotRecording;
		}

		private IList<TestStep> DeserializeSteps(object value)
		{
			if (!(value is ReadOnlyCollection<object>))
			{
				return new List<TestStep>();
			}

			ReadOnlyCollection<object> list = value as ReadOnlyCollection<object>;

			if (list.Count < 1)
			{
				return new List<TestStep>();
			}

			IList<TestStep> steps = new List<TestStep>();
			Mapper.CreateMap<object, TestStep>();

			foreach (object item in list)
			{
				string itemJson = JsonConvert.SerializeObject(item);
				TestStep step = JsonConvert.DeserializeObject<TestStep>(itemJson, new JavaScriptDateTimeConverter());
				steps.Add(step);
			}

			//string json = value.ToString();

			//IList<TestStep> steps = JsonConvert.DeserializeObject<List<TestStep>>(json);

			return steps;
		}

		private bool EagleEyeInjected(TestScenario scenario, int timeoutMilliseconds = 0)
		{
			bool injected = this.Poll(scenario) != null;

			if (injected)
			{
				return true;
			}

			Thread.Sleep(timeoutMilliseconds);
			return this.Poll(scenario) != null;
		}

		private IList<TestStep> GetEagleEyeResults(TestScenario scenario)
		{
			object value = scenario.Browser.ExecuteScript("return window._getTestShotEvents();");
			IList<TestStep> steps = this.DeserializeSteps(value);

			if (steps.Count < 1)
			{
				throw new Exception("No steps returned by BigBroher.");
			}

			return steps;
		}

		private TestScenario InjectEagleEye(TestScenario scenario)
		{
			scenario.ScenarioLog.Add("Injecting EagleEye");
			string path = Path.Combine(ConfigurationManager.AppSettings["TestShot.rootpath"], "EagleEye.js");
			FileInfo file = new FileInfo(path);

			if (file.Exists == false)
			{
				throw new FileNotFoundException();
			}

			string script = string.Empty;

			using (StreamReader streamReader = file.OpenText())
			{
				script = streamReader.ReadToEnd();
			}

			scenario.Browser.ExecuteScript(script);

			if (!this.EagleEyeInjected(scenario, 2000))
			{
				if (!this.EagleEyeInjected(scenario))
				{
					scenario.ErrorLog.Add(TestRecorder.EagleEyeNotFoundMessage);
					return scenario;
				}
			}

			scenario.ScenarioLog.Add(TestRecorder.EagleEyeInjectedMessage);
			return scenario;
		}

		private IList<TestStep> InsertPauseSteps(IList<TestStep> recording)
		{
			var previousScreenshotIsLivePlayback = false;
			IList<TestStep> newSteps = new List<TestStep>();
			int i = 0;

			foreach (TestStep step in recording)
			{
				newSteps.Add(step);

				if (step.Action == Action.Screenshot)
				{
					previousScreenshotIsLivePlayback = step.LivePlayback;
				}

				if (!previousScreenshotIsLivePlayback || i == recording.Count() - 1)
				{
					i++;
					continue;
				}

				long timestamp = step.Timestamp;
				TestStep newStep = new TestStep { Action = Action.Pause, Timestamp = timestamp };
				newSteps.Add(step);
			}

			recording = newSteps;
			return recording;
		}

		private IList<string> LogException(string message, IList<string> log, Exception ex)
		{
			log.Add(message);
			log.Add("Message: " + ex.Message);
			log.Add("Stack Trace: " + ex.StackTrace);
			log.Add("Target Site: " + ex.TargetSite);
			if (ex.InnerException != null)
			{
				log.Add("Inner Exception Message: " + ex.InnerException.Message);
			}
			return log;
		}

		private void OpenDummyDriver(TestScenario scenario, Logger log)
		{
			if (scenario.OpenDummy)
			{
				scenario.DummyBrowser = BrowserFactory.Create(scenario.BrowserType, log);
				// make this as unobstructive as possible
				scenario.DummyBrowser.SetWindowSize(new System.Drawing.Size(1, 1));
				scenario.DummyBrowser.SetWindowPosition(new System.Drawing.Point(9999, 9999));
				scenario.ScenarioLog.Add("Dummy Driver Opened");
			}
		}

		private bool OpenTestPage(string pageUrl, TestScenario scenario)
		{
			scenario.Browser.Open(pageUrl, 2);

			if (scenario.Browser.HasUrl(pageUrl))
			{
				scenario.ScenarioLog.Add(string.Format("Test Page Opened: {0}", pageUrl));
				return true;
			}

			return false;
		}

		private void Quit(TestScenario scenario)
		{
			if (scenario.Browser != null)
			{
				scenario.Browser.Quit();
			}

			if (scenario.DummyBrowser != null)
			{
				scenario.DummyBrowser.Quit();
			}
		}

		private TestScenario SaveScenario(TestScenario scenario)
		{
			string scenarioString = this.SerializeScenario(scenario);
			TestScenarioManager manager = new TestScenarioManager();
			manager.Save(scenario);
			return scenario;
		}

		private string SerializeScenario(TestScenario scenario)
		{
			string results = string.Empty;
			results = JsonConvert.SerializeObject(scenario, Formatting.Indented);
			return results;
		}
	}
}