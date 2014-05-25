namespace TestShot
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using AutoMapper;
	using Newtonsoft.Json;

	public class TestScenarioManager
	{
		public static List<Action> ActionsAllowed
		{
			get
			{
				List<Action> valid = new List<Action>();
				valid.Add(Action.Click);
				valid.Add(Action.Keypress);
				valid.Add(Action.Pause);
				valid.Add(Action.Screenshot);
				valid.Add(Action.Scroll);
				valid.Add(Action.Note);
				return valid;
			}
		}

		//TODO: Move to seperate interfaced serialization class
		public static TestScenario JsonDeserializeScenario(string json)
		{
			TestScenario scenario = JsonConvert.DeserializeObject<TestScenario>(json);
			return scenario;
		}

		//TODO: Move to seperate interfaced serialization class
		public static string JsonSerializeScenario(TestScenario scenario)
		{
			string json = JsonConvert.SerializeObject(scenario, Formatting.Indented);
			return json;
		}

		public TestScenario Create(TestRecording recording, string scenarioVersion, bool validate)
		{
			TestScenario scenario = GetScenarioFromTestRecording(recording);
			scenario.DateCreated = DateTime.Now;
			scenario.Id = Guid.NewGuid();
			scenario.TestRecordingId = recording.Id;
			scenario = TestScenarioManager.ResetScenario(recording, scenario, scenarioVersion);
			scenario.OverrideScreenshots = true;

			if (!validate)
			{
				return scenario;
			}

			scenario = TestScenarioManager.ValidateScenario(scenario);
			return scenario;
		}

		//Get an existing Version of Scenario for the Recording
		public TestScenario Load(TestRecording recording, string scenarioVersion)
		{
			TestScenario scenario = TestScenarioManager.LoadScenario(recording, scenarioVersion);
			scenario = TestScenarioManager.ValidateScenario(scenario);
			return scenario;
		}

		public TestScenario Save(TestScenario scenario)
		{
			scenario.DateSaved = DateTime.Now;
			string fileContent = TestScenarioManager.JsonSerializeScenario(scenario);
			Common.SaveToFile(scenario.VersionPath, Constants.ScenarioFileName, fileContent);
			return scenario;
		}

		private static TestScenario GetScenarioFromTestRecording(TestRecording recording)
		{
			Mapper.CreateMap<TestRecording, TestScenario>();
			TestScenario scenario = Mapper.Map<TestScenario>(recording);
			return scenario;
		}

		private static bool IsValidAction(Action action)
		{
			return TestScenarioManager.ActionsAllowed.Contains(action);
		}

		//TODO: Move to seperate interfaced file manager class
		private static TestScenario LoadScenario(TestRecording recording, string scenarioVersion)
		{
			TestScenario scenario = new TestScenario();
			scenario = TestScenarioManager.SetScenarioVersion(scenario, scenarioVersion);
			scenario = TestScenarioManager.SetScenarioFullPath(recording, scenario);

			string fileContent = string.Empty;

			try
			{
				fileContent = Common.GetFileContent(scenario.FilePath);
				scenario.FileContent = fileContent;
				scenario = new TestScenario();
				scenario = TestScenarioManager.JsonDeserializeScenario(fileContent);
			}
			catch
			{
				throw new ApplicationException(string.Format("Scenario failed to load from {0}.", scenario.FilePath));
			}

			return scenario;
		}

		private static TestScenario ResetScenario(TestRecording recording, TestScenario scenario, string scenarioVersion)
		{
			scenario = TestScenarioManager.ResetScreenShotCounter(scenario);
			scenario = TestScenarioManager.SetScenarioVersion(scenario, scenarioVersion);
			scenario = TestScenarioManager.SetScenarioPaths(recording, scenario);
			return scenario;
		}

		private static TestScenario ResetScreenShotCounter(TestScenario scenario)
		{
			scenario.ScreenshotsBaseline = scenario.Screenshots;
			scenario.Screenshots = 0;
			return scenario;
		}

		// Root\AppName\TestRecordingId\BrowserType\TestRecordingVersion\ScenarioVersion\scenario.json
		private static TestScenario SetScenarioFullPath(TestRecording recording, TestScenario scenario)
		{
			if (string.IsNullOrWhiteSpace(scenario.VersionPath))
			{
				scenario = TestScenarioManager.SetScenarioVersionPath(recording, scenario);
			}

			string path = Common.GetCleanPathName(Path.Combine(scenario.VersionPath, Constants.ScenarioFileName));
			scenario.FilePath = path;
			return scenario;
		}

		private static TestScenario SetScenarioPaths(TestRecording recording, TestScenario scenario)
		{
			TestScenarioManager.SetScenarioRootPath(recording, scenario);
			TestScenarioManager.SetScenarioVersionPath(recording, scenario);
			TestScenarioManager.SetScenarioFullPath(recording, scenario);

			string path = Common.GetCleanPathName(Path.Combine(scenario.VersionPath, Constants.ScreenshotsFolderName));
			scenario.ScreenshotsPath = path;
			//Create the path if it doesn't exist
			Common.CreateDirectory(path);
			return scenario;
		}

		// Root\AppName\TestRecordingId\BrowserType\TestRecordingVersion\
		private static TestScenario SetScenarioRootPath(TestRecording recording, TestScenario scenario)
		{
			string path = recording.VersionPath;
			scenario.Directory = path;
			return scenario;
		}

		private static TestScenario SetScenarioVersion(TestScenario scenario, string scenarioVersion)
		{
			scenario.ScenarioVersion = string.IsNullOrWhiteSpace(scenarioVersion) ? "1" : scenarioVersion;
			return scenario;
		}

		// Root\AppName\TestRecordingId\BrowserType\TestRecordingVersion\ScenarioVersion\
		private static TestScenario SetScenarioVersionPath(TestRecording recording, TestScenario scenario)
		{
			if (string.IsNullOrWhiteSpace(scenario.Directory))
			{
				scenario = TestScenarioManager.SetScenarioRootPath(recording, scenario);
			}

			string path = Common.GetCleanPathName(Path.Combine(scenario.Directory, "ver" + scenario.ScenarioVersion));
			scenario.VersionPath = path;
			return scenario;
		}

		private static TestScenario ValidateScenario(TestScenario scenario)
		{
			if (scenario == null)
			{
				throw new ArgumentNullException("Scenario is null.");
			}

			if (scenario.Steps == null)
			{
				throw new ApplicationException("Scenario Steps is null.");
			}

			if (scenario.Steps.Count < 1)
			{
				throw new ApplicationException("Scenario Steps is Empty.");
			}

			foreach (TestStep step in scenario.Steps)
			{
				var isValid = TestScenarioManager.IsValidAction(step.Action);

				if (!isValid)
				{
					scenario.ErrorLog.Add(string.Format("Unrecognized Action {0}.", step.Action));
				}
			}

			if (scenario.Steps.Last().Action != Action.Screenshot)
			{
				scenario.ErrorLog.Add("The last recorded item should have been a screenshot.");
			}

			if (scenario.ErrorLog.Count > 0)
			{
				string errors = Common.GetErrors(scenario.ErrorLog, "\r\n");
				throw new ApplicationException(string.Format("Scenario for {0}: {1}", scenario.Name, errors));
			}

			return scenario;
		}

		private string GetErrors(IList<string> errors, string seperator)
		{
			StringBuilder result = new StringBuilder();

			foreach (string error in errors)
			{
				result.Append(error);
				result.Append(seperator);
			}

			return result.ToString();
		}
	}
}