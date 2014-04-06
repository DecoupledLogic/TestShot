namespace TestShot
{
	using System;
	using System.Collections.Generic;
	using System.Drawing.Imaging;
	using System.IO;
	using System.Linq;
	using System.Text;
	using OpenQA.Selenium;

	public class Common
	{
		public static int GetDateDiffInMilliseconds(DateTime date1, DateTime date2)
		{
			return 0;
		}

		public static string GetFileContent(string path)
		{
			string fileContent = string.Empty;

			FileInfo file = new FileInfo(path);

			if (file.Exists == false)
			{
				throw new FileNotFoundException();
			}

			using (StreamReader streamReader = file.OpenText())
			{
				fileContent = streamReader.ReadToEnd();
			}

			return fileContent;
		}

		public static void SaveToFile(string directoryName, string fileName, string content)
		{
			string fullFilename = string.Empty;

			try
			{
				Common.CreateDirectory(directoryName);

				fullFilename = Path.Combine(directoryName, fileName);

				using (StreamWriter sw = new StreamWriter(File.Open(fullFilename, FileMode.Create, FileAccess.Write)))
				{
					sw.Write(content);
				}
			}
			catch (Exception)
			{
				if (!string.IsNullOrWhiteSpace(fullFilename))
				{
					if (File.Exists(fullFilename))
					{
						File.Delete(fullFilename);
					}
				}

				throw;
			}
		}

		public static void CreateDirectory(string path)
		{ 
			if (!System.IO.Directory.Exists(path))
				{
					System.IO.Directory.CreateDirectory(path);
				}
		}

		public static string GetErrors(IList<string> errors, string seperator)
		{
			StringBuilder result = new StringBuilder();

			foreach (string error in errors)
			{
				result.Append(error);
				result.Append(seperator);
			}

			return result.ToString();
		}

		public static string GetCleanPathName(string name)
		{
			name = name
				.Replace('/', '%')
				.Replace('?', '$')
				.Replace(" ", "+");

			return name;
		}

		public static FileCompare TakeScreenshot(TestScenario scenario)
		{
			scenario.Screenshots++;

			//Default is temp name
			string comparefileName = string.Format("temp_{0}_{1:yyyy-MM-dd_hh-mm-ss-tt}.png", scenario.Screenshots.ToString(), DateTime.Now);
			string compareFilePath = Path.Combine(scenario.ScreenshotsPath, comparefileName);
			scenario.Browser.TakeScreenshot(compareFilePath, ImageFormat.Png);

			string originalFileName = string.Format("{0}.png", scenario.Screenshots.ToString());
			string originalFilePath = Path.Combine(scenario.ScreenshotsPath, originalFileName);

			string diffFileName = string.Format("{0}.png", Constants.DiffPngName);
			string diffFilePath = Path.Combine(scenario.ScreenshotsPath, diffFileName);
				
			if (scenario.OverrideScreenshots)
			{
				scenario.Browser.TakeScreenshot(originalFilePath, ImageFormat.Png);
			}
			
			FileCompare fileCompare = new FileCompare();
			fileCompare.OriginalFilePath = originalFilePath;
			fileCompare.CompareFilePath = compareFilePath;
			fileCompare.DiffFilePath = diffFilePath;
						
			return fileCompare;
		}

		public static string GetVersionsCsvFromDirectoryNames(string path)
		{
			IList<string> versions = Common.GetVersionsFromDirectoryNames(path);
			StringBuilder version = new StringBuilder();

			foreach (string ver in versions)
			{
				if (version.Length > 0)
				{
					version.Append(", ");
				}

				version.Append(ver);
			}

			return version.ToString();
		}

		public static IList<string> GetVersionsFromDirectoryNames(string path)
		{
			IList<string> versionNames = new List<string>();
			DirectoryInfo info = new DirectoryInfo(path);
			DirectoryInfo[] versions = info.GetDirectories("ver*", SearchOption.TopDirectoryOnly);

			if (versions == null || versions.Length < 1)
			{
				return versionNames;
			}

			versions = versions.OrderByDescending(x => x.Name).ToArray();

			foreach (DirectoryInfo dir in versions)
			{
				versionNames.Add(dir.Name.Replace("ver", ""));
			}

			return versionNames;
		}

		public static string GetHighestVersionFromDirectoryNames(string path)
		{
			string highestVersionName = Common.GetVersionsFromDirectoryNames(path).FirstOrDefault();
			return highestVersionName;
		}

		public static bool PathHasVersion(string path)
		{
			IList<string> versions = Common.GetVersionsFromDirectoryNames(path);
			bool hasVersion = versions.Count() > 0;
			return hasVersion;
		}
	}
}