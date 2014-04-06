namespace TestShot
{
	using System;
	using System.Collections.Generic;
	using OpenQA.Selenium;

	public interface ITestRecorder
	{
		IList<TestStep> Poll(TestScenario scenario);

		TestScenario Record(TestScenario scenario);

		TestScenario Stop(TestScenario scenario);
	}
}