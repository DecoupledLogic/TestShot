namespace TestShot
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	public class SuiteManager
	{
		public static IList<string> GetTestRecordingPathsForSuite(string suite)
		{
			//TODO: Deserialize suite file to list
			//Suite file contains the path of all recording files that should be ran for the suite
			return new List<string>();
		}

		public static IList<TestRecording> GetTestRecordings(string suite)
		{
			IList<string> paths = SuiteManager.GetTestRecordingPathsForSuite(suite);

			if (paths.Count() > 1)
			{
				throw new ApplicationException(string.Format("No TestRecording files found for the suite {0}.", suite));
			}

			IList<TestRecording> recordings = new List<TestRecording>();

			foreach (string path in paths)
			{
				string fileContent = string.Empty;

				try
				{
					fileContent = Common.GetFileContent(path);
				}
				catch
				{
					throw new ApplicationException(string.Format("Could not read TestRecording for at {0}", path));
				}

				TestRecording recording = TestRecordingManager.JsonDeserializeRecording(fileContent);

				recordings.Add(recording);
			}

			return recordings;
		}
	}
}
