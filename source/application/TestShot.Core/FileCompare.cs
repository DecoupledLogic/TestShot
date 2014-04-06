namespace TestShot
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	public class FileCompare
	{
		public string OriginalFilePath { set; get; }
		public string CompareFilePath { get; set; }
		public string DiffFilePath { get; set; }
	}
}
