namespace TestShot
{
	using System;
	using System.Collections.Generic;
	using System.Drawing.Imaging;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using Newtonsoft.Json;
	using TestPipe.Core.Interfaces;

	public class TestRunner
	{
		// TODO: handle friggin select menu click, can't right now bc browsers
		public void Click(IBrowser browser, int posX, int posY, string element = "")
		{
			string posString = string.Format("({0}, {1})", posX.ToString(), posY.ToString());
			Console.WriteLine(string.Format("  Clicking {0}", posString));

			StringBuilder script = new StringBuilder();
			script.Append("var el = document.elementFromPoint" + posString + ";");
			script.Append("if ((el.tagName === \"TEXTAREA\" || el.tagName === \"INPUT\") && document.caretPositionFromPoint) {");
			script.Append("var range = document.caretPositionFromPoint" + posString + ";");
			script.Append("var offset = range.offset;");
			script.Append("document.elementFromPoint" + posString + ".setSelectionRange(offset, offset);");
			script.Append("}");
			script.Append("return document.elementFromPoint" + posString + ";");
			object browserElement = browser.ExecuteScript(script.ToString());
			
			if (browserElement is IElement)
			{
				IElement webElement = browserElement as IElement;
				webElement.Click();
			}
		}

		public void Keypress(IBrowser browser, string key)
		{
			Console.WriteLine("  Typing " + key);

			//IJavaScriptExecutor js = driver as IJavaScriptExecutor;
			//object activeElement = js.ExecuteScript("return document.activeElement;");

			IElement activeElement = browser.ActiveElement();

			if (activeElement == null)
			{
				return;
			}

			// refer to `bigBrother.js`. The special keys are the arrow keys, stored
			// like 'ARROW_LEFT', By chance, the webdriver's `Key` object stores these
			// keys
			//if (key.Length > 1)
			//{
			//	key = specialKeys[key];
			//}
			activeElement.SendKeys(key);
		}

		public void Run(TestScenario scenario)
		{
			var currentStepIndex = 0;
			IBrowser browser = scenario.Browser;
			var steps = scenario.Steps;
			var overrideScreenshots = scenario.OverrideScreenshots;
			var recordPath = scenario.FilePath;
			browser.Open(scenario.TestVirtualUrl, 2);

			foreach (TestStep step in steps)
			{
				if (currentStepIndex == steps.Count() - 1)
				{
					string result = this.TakeScreenshot(scenario);
					Console.WriteLine(result);
					IImageProcessor imageProcessor = new ImageProcessor();
					imageProcessor.RemoveDanglingImages(recordPath, scenario.Screenshots + 1);
					return;
				}
				else
				{
					switch (step.Action)
					{
						case Action.Click:
							this.Click(browser, step.X, step.Y);
							break;

						case Action.Keypress:
							this.Keypress(browser, step.Key);
							break;

						case Action.Screenshot:
							this.TakeScreenshot(scenario);
							break;

						case Action.Pause:
							Console.WriteLine(string.Format("  Pause for {0} ms.", step.PauseMilliseconds));
							Thread.Sleep(step.PauseMilliseconds);
							break;

						case Action.Scroll:
							// this is really just to provide a visual cue during replay. Selenium records the whole page anyways we should technically set a delay here, but OSX' smooth scrolling would look really bad, adding the delay that Selenium has already
							this.Scroll(browser, step.X, step.Y);
							break;
					}
				}

				currentStepIndex++;
			}
		}

		public void Scroll(IBrowser browser, int posX, int posY)
		{
			string posString = string.Format("({0}, {1})", posX.ToString(), posY.ToString());
			Console.WriteLine("  Scrolling to " + posString);

			string script = string.Format("window.scrollTo({0},{1});", posX, posY);
			browser.ExecuteScript(script.ToString());
		}

		public string TakeScreenshot(TestScenario scenario)
		{
			Console.WriteLine("  Taking screenshot " + scenario.Screenshots + 1);

			FileCompare fileCompare = Common.TakeScreenshot(scenario);
			
			IImageProcessor imageProcessor = new ImageProcessor();
			BitmapDiff diff = imageProcessor.CompareAndSaveDiffOnMismatch(fileCompare);

			//TODO: Pass in Test Name
			string testName = string.Empty;

			if (!diff.AreSame)
			{
				return string.Format("FAIL: {0} - New screenshot looks different. The diff image is saved for you to examine at {1}.", testName, diff.DiffPath);
			}

			return string.Format("PASS: {0}", testName);
		}
	}
}