namespace TestShot.Console
{
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Drawing;
	using TestShot;

	internal class Program
	{
		private static void Main(string[] args)
		{
			bool loadFile = true;

			Console.WriteLine("Loading Recording.");
			TestRecording recording = ConsoleManager.GetRecording(loadFile, args);

			Console.WriteLine("Creating Scenario.");
			TestScenarioManager scenarioManager = new TestScenarioManager();
			TestScenario scenario = scenarioManager.Create(recording, "1", false);

			Console.WriteLine("Opening Recorder.");
			TestRecorder recorder = new TestRecorder();

			Console.WriteLine("Starting Recorder.");
			scenario = recorder.Record(scenario);
			PrintErrors(scenario);
			Console.WriteLine("Recorder Started.");

			Console.WriteLine("Begin Recording.");
			Console.WriteLine("Type q to quit, l to take a screenshot and mark a live playback point till next screenshot, and anything else to take a normal screenshot. If you quit the console any other way than typing q, it may leave the recording in an unfinished state and may leave hidden applications open until you restart.");

			scenario = ConsoleManager.MonitorConsole(recorder, scenario);

			PrintErrors(scenario);

			Console.WriteLine(string.Format("Recording finished and saved at {0}.", scenario.FilePath));
		}

		private static void PrintErrors(TestScenario scenario)
		{
			if (scenario.ErrorLog.Count > 0)
			{
				foreach (string error in scenario.ErrorLog)
				{
					Console.WriteLine(error);
				}

				scenario.ErrorLog.Clear();
			}
		}
	}
}