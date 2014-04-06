namespace TestShot
{
	using System;
	using System.Drawing;

	public class BitmapDiff
	{
		public bool AreSame { get; set; }

		public Bitmap Diff { get; set; }

		public string DiffPath { get; set; }
	}
}