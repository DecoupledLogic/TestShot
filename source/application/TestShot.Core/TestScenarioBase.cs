namespace TestShot
{
	using System;
	using System.Collections.Generic;
	using OpenQA.Selenium;
	using TestPipe.Core.Enums;

	public class TestScenarioBase : TestEntity
	{
		public TestScenarioBase()
		{
		}

		public BrowserTypeEnum BrowserType { get; set; }

		public string Directory { get; set; }

		public string EnvironmentUrl { get; set; }

		public string FileContent { get; set; }

		public string FilePath { get; set; }

		public bool IsSecure { get; set; }

		public bool IsSkipped { get; set; }

		public string Name { get; set; }

		public bool OpenDummy { get; set; }

		public IList<string> Tags { get; set; }

		public string VersionPath { get; set; }

		public DateTime DateSaved { get; set; }

		public string TestUrl 
		{
			get 
			{
				this.EnvironmentUrl = this.EnvironmentUrl.Replace("http://", "").Replace("https://", "");

				if (this.IsSecure)
				{
					this.EnvironmentUrl = "https://" + this.EnvironmentUrl;
				}
				else
				{
					this.EnvironmentUrl = "http://" + this.EnvironmentUrl;
				}

				if (!EnvironmentUrl.EndsWith("/"))
				{
					this.EnvironmentUrl = this.EnvironmentUrl + "/";
				}

				if (this.TestVirtualUrl.StartsWith("/"))
				{
					this.TestVirtualUrl = this.TestVirtualUrl.Remove(0, 1); 
				}

				return this.EnvironmentUrl + this.TestVirtualUrl;
			} 
		}

		public string TestVirtualUrl { get; set; }
	}
}