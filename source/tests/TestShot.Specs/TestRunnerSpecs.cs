namespace TestShot.Specs
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using AutoMapper;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using TestPipe.Common;
	using TestPipe.Core;
	using TestPipe.Core.Browser;
	using TestPipe.Core.Control;
	using TestPipe.Core.Enums;
	using TestPipe.Core.Interfaces;

	[TestClass]
	public class TestRunnerSpecs
	{
		private TestRunner sut;

		[TestInitialize]
		public void TestSetup()
		{
			sut = new TestRunner();
		}

		[TestMethod]
		public void TestMapping()
		{
			TestRecording recording = new TestRecording();
			recording.BrowserType = BrowserTypeEnum.FireFox;
			recording.Directory = "testdirectory";
			recording.FileContent = "testcontent";
			recording.FilePath = "testfilepath";
			recording.IsSkipped = true;
			recording.Name = "testname";
			recording.Tags = new List<string>() { "testtag" };
			recording.VersionPath = "testversionpath";
			recording.AppName = "testapp";
			recording.AppVersion = "testappversion";

			Mapper.CreateMap<TestRecording, TestScenario>();
			TestScenario scenario = Mapper.Map<TestScenario>(recording);

			Assert.IsTrue(scenario.BrowserType == BrowserTypeEnum.FireFox);
			Assert.IsTrue(scenario.Directory == "testdirectory");
			Assert.IsTrue(scenario.FileContent == "testcontent");
			Assert.IsTrue(scenario.FilePath == "testfilepath");
			Assert.IsTrue(scenario.IsSkipped == true);
			Assert.IsTrue(scenario.Name == "testname");
			Assert.IsTrue(scenario.Tags.Contains("testtag"));
			Assert.IsTrue(scenario.VersionPath == "testversionpath");
		}

		[TestMethod]
		public void TestRunnerClick()
		{
			IBrowser browser = BrowserFactory.Create(BrowserTypeEnum.FireFox, new Logger());
			browser.Open("http://localhost/testpipe.testsite/js_events_test.html", 2);
			ISelect selector = new Select(FindByEnum.Id, "seeMe");
			BaseControl button = new BaseControl(browser, selector);
			button.Click();

			sut.Click(browser, 45, 20);
			object webElement = browser.ExecuteScript("return window._getTestShotEvents();");
			browser.Quit();
			ReadOnlyCollection<object> list = webElement as ReadOnlyCollection<object>;

			Assert.IsTrue(list.Count == 1);
		}

		[TestMethod]
		public void TestRunnerRun()
		{
			TestRecording recording = TestHelper.GetTestRecording();

			TestScenario scenario = TestHelper.GetTestScenario(recording);

			sut.Run(scenario);


		}
	}
}