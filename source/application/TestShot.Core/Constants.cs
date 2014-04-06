namespace TestShot
{
	using System;
	using System.Drawing;

	public static class Constants
	{
		public static readonly Color DefaultDiffColor = Color.Red;

		// optimal default screen size. 1200 is bootstrap's definition of 'large screen' and 795 is a
		// mba 13inch's available height for firefox window in Selenium. The actual height of the
		// chromeless viewport should be 689
		public static readonly Size DefaultSize = new Size(1200, 795);
		public static readonly string DiffPngName = "diff.png";
		public static readonly string ScenarioFileName = "scenario.json";
		public static readonly string RecordFileName = "record.json";
		public static readonly string ScreenshotsFolderName = "screenshots";
	}
}