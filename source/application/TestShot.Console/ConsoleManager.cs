namespace TestShot.Console
{
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Drawing;
	using TestPipe.Core;
	using TestPipe.Runner;
	using TestShot;
	using Action = TestShot.Action;

	public static class ConsoleManager
	{
		public static TestRecording GetRecording(bool loadFile, string[] args)
		{
			TestRecording recording = new TestRecording();

			if (loadFile)
			{
				recording.AppName = GetUserAppName();
				recording.EnvironmentUrl = GetEnvironmentUrl();
				recording.TestVirtualUrl = GetUserPageUrl();

				string browserSelection = GetUserBrowser();
				string browser = ConsoleManager.GetBrowserName(browserSelection);
				recording.BrowserType = RunnerBase.GetBrowserType(browser);

				recording.Name = GetUserScenario();

				if (recording.Id == Guid.Empty)
				{
					//Create new recording
					recording = TestRecordingManager.Create(recording, "1");
					return recording;
				}

				recording = TestRecordingManager.SetRecordingRootPath(recording);
				recording.RecordingVersion = "1";

				if (!Common.PathHasVersion(recording.Directory))
				{
					Console.WriteLine("No current versions found.");
					Console.WriteLine("Do you want to create a new one. Y or N");
					string createNew = Console.ReadLine();
					if (createNew.ToLower() == "n")
					{
						Console.WriteLine("Recording Cancelled.");
						return recording;
					}

					//Create new recording
					recording = TestRecordingManager.Create(recording, "1");
					return recording;
				}
				else
				{
					Console.WriteLine("Overwrite previous version");
					string overwrite = Console.ReadLine();
					bool overwriteRecording = false;
					bool.TryParse(overwrite, out overwriteRecording);
					string overwriteVersion = string.Empty;
					string recordingVersion = "1";

					if (overwriteRecording)
					{
						overwriteVersion = ConsoleManager.GetOverwriteVersion(recording.Directory);
						IList<string> versions = Common.GetVersionsFromDirectoryNames(recording.Directory);

						bool validOverwrite = false;

						while (!validOverwrite)
						{
							if (!versions.Contains(overwriteVersion))
							{
								Console.WriteLine("You entered an invalid version.");
								overwriteVersion = ConsoleManager.GetOverwriteVersion(recording.Directory).ToLower();
							}

							validOverwrite = true;
						}

						recordingVersion = overwriteVersion;
					}

					TestRecordingManager manager = new TestRecordingManager();
					recording = manager.Load(recording, recordingVersion);
					return recording;
				}
			}

			recording = GetTestRecordingFromArgs(args);
			return recording;
		}

		public static TestScenario MonitorConsole(TestRecorder recorder, TestScenario scenario)
		{
			bool done = false;
			
			//Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelHandler);
			while (!done)
			{
				ConsoleKeyInfo cki = Console.ReadKey(true);

				switch (cki.Key)
				{
					case ConsoleKey.Q:
						{
							Console.WriteLine("Recording Cancelled.");
							recorder.Stop(scenario);
							done = true;
							return scenario;
						}
					case ConsoleKey.L:
						{
							Console.WriteLine("Begin Live Playback.");
							
							//step.LivePlayback = true;
							break;
						}
					default:
						{
							Console.WriteLine("Taking Screenshot.");
							TestStep step = new TestStep { Action = Action.Screenshot, Timestamp = DateTime.Now.Ticks };
							Common.TakeScreenshot(scenario);
							scenario.Steps.Add(step);
							break;
						}
				}
			}

			return scenario;
		}

		private static string GetBrowserName(string abbreviation)
		{
			switch (abbreviation)
			{
				case "f":
					return "FireFox";

				case "i":
					return "IE";
			}
			return string.Empty;
		}

		private static string GetOverwriteVersion(string recordingPath)
		{
			Console.WriteLine("Select version to overwrite.");
			string version = Common.GetVersionsCsvFromDirectoryNames(recordingPath);
			Console.WriteLine(version);
			return Console.ReadLine();
		}

		private static TestRecording GetTestRecordingFromArgs(string[] args)
		{
			TestRecording recording = new TestRecording();

			if (args.Length < 11)
			{
				throw new Exception("You must provide no arguments or the first 9 command line arguments in order to properly start a recording");
			}

			recording.EnvironmentUrl = args[0];
			recording.AppName = args[1];
			recording.AppVersion = args[2];
			recording.BrowserType = RunnerBase.GetBrowserType(args[3]);
			recording.Name = args[4];
			bool openDummy = true;
			bool.TryParse(args[5], out openDummy);
			recording.OpenDummy = openDummy;
			int x;
			int.TryParse(args[6], out x);
			int y;
			int.TryParse(args[7], out y);
			Size size = x > 0 && y > 0 ? new Size(x, y) : Constants.DefaultSize;
			recording.ScreenSize = size;
			recording.TestVirtualUrl = args[8];
			recording.FilePath = args[9];
			return recording;
		}

		private static string GetUserAppName()
		{
			Console.WriteLine("Enter Application Name");
			return Console.ReadLine();
		}

		private static string GetUserBrowser()
		{
			Console.WriteLine("Select browser: f = Firefox, i = Internet Explorer");
			return Console.ReadLine();
		}

		private static string GetEnvironmentUrl()
		{
			Console.WriteLine("Enter Environment Root Url");
			return Console.ReadLine();
		}

		private static string GetUserPageUrl()
		{
			Console.WriteLine("Enter Page Virtual Url");
			return Console.ReadLine();
		}

		private static string GetUserScenario()
		{
			Console.WriteLine("Enter scenario name.");
			return Console.ReadLine();
		}

		//TODO: Figure out how to handle Cancel Key Press
		private static void CancelHandler(object sender, ConsoleCancelEventArgs args)
		{
			//ui.Cancel(scenario);
		}
	}
}