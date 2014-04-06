namespace TestShot
{
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.IO;
	using Newtonsoft.Json;
	using TestPipe.Core.Enums;

	public class TestRecordingManager
	{
		public static TestRecording Create(TestRecording recording, string recordingVersion)
		{
			recording.DateCreated = DateTime.Now;
			recording.Id = Guid.NewGuid();
			recording = TestRecordingManager.ResetTestRecording(recording, recordingVersion);
			recording = TestRecordingManager.Save(recording);
			return recording;
		}

		//TODO: Move to seperate interfaced serialization class
		public static TestRecording JsonDeserializeRecording(string json)
		{
			TestRecording recording = JsonConvert.DeserializeObject<TestRecording>(json);
			return recording;
		}

		//TODO: Move to seprate interfaced file manager class
		public static TestRecording Save(TestRecording recording)
		{
			recording.DateSaved = DateTime.Now;
			string fileContent = TestRecordingManager.JsonSerializeRecording(recording);
			Common.SaveToFile(recording.VersionPath, Constants.RecordFileName, fileContent);
			return recording;
		}

		// Root\AppName\TestRecordingId\BrowserType\
		public static TestRecording SetRecordingRootPath(TestRecording recording)
		{
			string rootPath = ConfigurationManager.AppSettings["TestShot.rootpath"];
			string path = Common.GetCleanPathName(Path.Combine(rootPath, recording.AppName, recording.Id.ToString(), recording.BrowserType.ToString()));
			recording.Directory = path;
			return recording;
		}

		//Get an existing Version of Scenario for the Recording
		public TestRecording Load(TestRecording recording, string recordingVersion)
		{
			recording = TestRecordingManager.LoadRecording(recording, recordingVersion);
			recording = TestRecordingManager.ValidateTestRecording(recording);
			return recording;
		}

		//TODO: Move to seperate interfaced serialization class
		private static string JsonSerializeRecording(TestRecording recording)
		{
			string json = JsonConvert.SerializeObject(recording, Formatting.Indented);
			return json;
		}

		//TODO: Move to seperate interfaced file manager class
		private static TestRecording LoadRecording(TestRecording recording, string recordingVersion)
		{
			recording = TestRecordingManager.SetRecordingVersion(recording, recordingVersion);
			recording = TestRecordingManager.SetRecordingFullPath(recording);

			string fileContent = string.Empty;

			try
			{
				fileContent = Common.GetFileContent(recording.FilePath);
				recording.FileContent = fileContent;
				recording = TestRecordingManager.JsonDeserializeRecording(fileContent);
			}
			catch
			{
				throw new ApplicationException(string.Format("Test Recording failed to load from {0}.", recording.FilePath));
			}

			return recording;
		}

		public static TestRecording ResetTestRecording(TestRecording recording, string recordingVersion)
		{
			recording = TestRecordingManager.SetRecordingVersion(recording, recordingVersion);
			recording = TestRecordingManager.SetRecordingPaths(recording);
			return recording;
		}

		// Root\AppName\TestRecordingId\BrowserType\TestRecordingVersion\recording.json
		private static TestRecording SetRecordingFullPath(TestRecording recording)
		{
			if (string.IsNullOrWhiteSpace(recording.VersionPath))
			{
				recording = TestRecordingManager.SetRecordingVersionPath(recording);
			}

			string path = Common.GetCleanPathName(Path.Combine(recording.VersionPath, Constants.RecordFileName));
			recording.FilePath = path;
			return recording;
		}

		private static TestRecording SetRecordingPaths(TestRecording recording)
		{
			if (string.IsNullOrWhiteSpace(recording.VersionPath))
			{
				recording = TestRecordingManager.SetRecordingFullPath(recording);
			}
			return recording;
		}

		private static TestRecording SetRecordingVersion(TestRecording recording, string recordingVersion)
		{
			recording.RecordingVersion = string.IsNullOrWhiteSpace(recordingVersion) ? "1" : recordingVersion;
			return recording;
		}

		// Root\AppName\TestRecordingId\BrowserType\TestRecordingVersion\
		private static TestRecording SetRecordingVersionPath(TestRecording recording)
		{
			if (string.IsNullOrWhiteSpace(recording.Directory))
			{
				recording = TestRecordingManager.SetRecordingRootPath(recording);
			}

			string path = Common.GetCleanPathName(Path.Combine(recording.Directory, "ver" + recording.RecordingVersion));
			recording.VersionPath = path;
			return recording;
		}

		private static TestRecording ValidateTestRecording(TestRecording recording)
		{
			IList<string> errorLog = new List<string>();

			if (recording == null)
			{
				throw new ArgumentNullException("Recording is null.");
			}

			if (string.IsNullOrWhiteSpace(recording.EnvironmentUrl))
			{
				errorLog.Add("EnvironmentUrl is empty.");
			}

			if (string.IsNullOrWhiteSpace(recording.TestVirtualUrl))
			{
				errorLog.Add("TestVirtualUrl is empty.");
			}

			if (string.IsNullOrWhiteSpace(recording.TestUrl))
			{
				errorLog.Add("TestUrl is empty.");
			}

			if (string.IsNullOrWhiteSpace(recording.AppName))
			{
				errorLog.Add("Recording AppName is empty.");
			}

			if (recording.Id == Guid.Empty)
			{
				errorLog.Add("Recording Id is empty.");
			}

			if (recording.BrowserType == BrowserTypeEnum.None)
			{
				errorLog.Add("Recording BrowserType is set to None, must be a valid type.");
			}

			if (errorLog.Count > 0)
			{
				string errors = Common.GetErrors(errorLog, "\r\n");
				throw new ApplicationException(string.Format("Recording for {0}: {1}", recording.Name, errors));
			}

			return recording;
		}

		private TestScenario MakeNote(TestScenario scenario, string note)
		{
			TestStep step = new TestStep { Action = Action.Note, Timestamp = DateTime.Now.Ticks, Note = note };
			scenario.Steps.Add(step);
			return scenario;
		}
	}
}