namespace TestShot.Specs
{
	using System;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using TestPipe.Core.Enums;

	[TestClass]
	[DeploymentItem(@"Data\scenario.json")]
	public class TestScenarioManagerSpecs
	{
		[TestCategory("Integration")]
		[TestMethod]
		public void JsonDeserializeScenarioReturnsTestScenario()
		{
			string json = Common.GetFileContent("scenario.json");

			TestScenario recording = TestScenarioManager.JsonDeserializeScenario(json);

			Assert.IsNotNull(recording);
		}

		[TestCategory("Integration")]
		[TestMethod]
		public void JsonDeserializeScenarioReturnsTestScenarioBrowserType()
		{
			string json = Common.GetFileContent("scenario.json");

			TestScenario recording = TestScenarioManager.JsonDeserializeScenario(json);

			Assert.AreEqual(BrowserTypeEnum.FireFox, recording.BrowserType);
		}
	}
}
