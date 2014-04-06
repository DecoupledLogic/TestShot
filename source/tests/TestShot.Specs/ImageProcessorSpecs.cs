namespace TestShot.Specs
{
	using System;
	using System.Diagnostics;
	using System.Drawing;
	using System.Drawing.Imaging;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	[DeploymentItem(@"Data\testimage1.png")]
	[DeploymentItem(@"Data\testimage2.png")]
	public class ImageProcessorSpecs : IDisposable
	{
		private Color color;
		private Bitmap image1;
		private Bitmap image2;
		private ImageProcessor sut;

		[TestMethod]
		public void DiffImageSmokeTest()
		{
			this.CreateImages();

			Stopwatch sw = new Stopwatch();
			sw.Start();
			this.sut = new ImageProcessor();
			BitmapDiff diff = this.sut.DiffImage(this.image1, this.image2);
			sw.Stop();

			diff.Diff.Save("C:\\ImageProcessortest-diff.png", ImageFormat.Png);

			Console.WriteLine(sw.ElapsedMilliseconds.ToString());
		}

		[TestMethod]
		public void DiffSavedPngFiles()
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();
			BitmapDiff diff = this.sut.DiffImage(this.image1, this.image2);
			sw.Stop();

			diff.Diff.Save("C:\\ImageProcessortestimage-diff.png", ImageFormat.Png);

			Console.WriteLine(sw.ElapsedMilliseconds.ToString());
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		[TestMethod]
		public void MergeImagesWithPngFiles()
		{
			BitmapDiff diff = this.sut.DiffImage(this.image1, this.image2);

			Stopwatch sw = new Stopwatch();
			sw.Start();
			Image merge = this.sut.MergeImages(this.image1, diff.Diff);
			sw.Stop();

			merge.Save("C:\\ImageProcessortestimage-merge.png", ImageFormat.Png);

			Console.WriteLine(sw.ElapsedMilliseconds.ToString());
		}

		[TestMethod]
		public void MergeImagesWithPngFilesAndColorOverlay()
		{
			BitmapDiff diff = this.sut.DiffImage(this.image1, this.image2, this.color);

			Stopwatch sw = new Stopwatch();
			sw.Start();
			Image merge = this.sut.MergeImages(this.image1, diff.Diff);
			sw.Stop();

			merge.Save("C:\\ImageProcessortestimage-mergeColorRed.png", ImageFormat.Png);

			Console.WriteLine(sw.ElapsedMilliseconds.ToString());
		}

		[TestInitialize]
		public void Setup()
		{
			this.sut = new ImageProcessor();
			this.image1 = this.sut.GetBitmapFromFile("testimage1.png");
			this.image2 = this.sut.GetBitmapFromFile("testimage2.png");
			this.color = Color.Red;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.image1.Dispose();
				this.image2.Dispose();
			}
		}

		private void CreateImages()
		{
			this.image1 = new Bitmap(400, 400);

			using (Graphics g = Graphics.FromImage(this.image1))
			{
				g.DrawRectangle(Pens.Blue, new Rectangle(0, 0, 50, 50));
				g.DrawRectangle(Pens.Red, new Rectangle(40, 40, 100, 100));
			}
			this.image1.Save("C:\\ImageProcessortest-1.png", ImageFormat.Png);

			this.image2 = (Bitmap)this.image1.Clone();

			using (Graphics g = Graphics.FromImage(this.image2))
			{
				g.DrawRectangle(Pens.Purple, new Rectangle(0, 0, 40, 40));
			}
			this.image2.Save("C:\\ImageProcessortest-2.png", ImageFormat.Png);
		}
	}
}