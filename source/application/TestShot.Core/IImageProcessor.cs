namespace TestShot
{
	using System;

	public interface IImageProcessor
	{
		BitmapDiff CompareAndSaveDiffOnMismatch(FileCompare fileCompare);

		BitmapDiff DiffImage(System.Drawing.Bitmap image1, System.Drawing.Bitmap image2, System.Drawing.Color? diffColor = null);

		BitmapDiff DiffImageFromPaths(string image1, string image2, System.Drawing.Color? diffColor = null);

		System.Drawing.Bitmap GetBitmapFromFile(string filePath);

		System.Drawing.Image MergeImages(System.Drawing.Image background, System.Drawing.Image foreground);

		void RemoveDanglingImages(string taskPath, int index);
	}
}