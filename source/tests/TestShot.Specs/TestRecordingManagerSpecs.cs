namespace TestShot.Specs
{
	using System;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using TestPipe.Core.Enums;

	[TestClass]
	[DeploymentItem(@"Data\record.json")]
	public class TestRecordingManagerSpecs
	{
		[TestCategory("Integration")]
		[TestMethod]
		public void JsonDeserializeRecordingReturnsTestRecording()
		{
			string json = Common.GetFileContent("record.json");
			
			TestRecording recording = TestRecordingManager.JsonDeserializeRecording(json);

			Assert.IsNotNull(recording);
		}

		[TestCategory("Integration")]
		[TestMethod]
		public void JsonDeserializeRecordingReturnsTestRecordingBrowserType()
		{
			string json = Common.GetFileContent("record.json");

			TestRecording recording = TestRecordingManager.JsonDeserializeRecording(json);

			Assert.AreEqual(BrowserTypeEnum.FireFox, recording.BrowserType);
		}
	}
}
