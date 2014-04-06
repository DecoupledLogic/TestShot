namespace TestShot
{
	using System;
	using System.Drawing;
	using System.Drawing.Imaging;
	using System.IO;

	//http://social.msdn.microsoft.com/Forums/vstudio/en-US/fc41fc9c-a296-4af6-9705-fcb80cd86512/find-differences-between-images-c?forum=csharpgeneral
	//MergeImages - Added method to Overlay diff image over the original.
	//DiffImages - Added ability to change the color of the pixels output on the diff to allow a color overlay.
	public class ImageProcessor : TestShot.IImageProcessor
	{
		public ImageProcessor()
		{
		}

		public BitmapDiff CompareAndSaveDiffOnMismatch(FileCompare fileCompare)
		{
			BitmapDiff diff = this.DiffImageFromPaths(fileCompare.OriginalFilePath, fileCompare.CompareFilePath, Constants.DefaultDiffColor);

			if (!diff.AreSame)
			{
				diff.Diff.Save(fileCompare.DiffFilePath);
			}

			return diff;
		}

		public BitmapDiff DiffImage(Bitmap image1, Bitmap image2, Color? diffColor = null)
		{
			return GetDiffImage(image1, image2, diffColor);
		}

		public BitmapDiff DiffImageFromPaths(string image1, string image2, Color? diffColor = null)
		{
			Bitmap bitmap1 = this.GetBitmapFromFile(image1);
			Bitmap bitmap2 = this.GetBitmapFromFile(image2);
			BitmapDiff diff = DiffImage(bitmap1, bitmap2, diffColor);
			return diff;
		}

		public Bitmap GetBitmapFromFile(string filePath)
		{
			Byte[] buffer = File.ReadAllBytes(filePath);
			MemoryStream stream = new MemoryStream(buffer);
			Bitmap bitmap = (Bitmap)Bitmap.FromStream(stream);
			stream.Dispose();
			return bitmap;
		}

		public Image MergeImages(Image background, Image foreground)
		{
			Image result = background;

			if (foreground == null)
			{
				return background;
			}

			Image overlay = foreground;

			if (PixelFormat.Format32bppArgb != foreground.PixelFormat)
			{
				overlay = ConvertToTransparentImage(foreground);
			}

			using (Graphics graphics = Graphics.FromImage(result))
			{
				graphics.DrawImage(overlay,
														new Rectangle(0, 0, result.Width, result.Height),
														new Rectangle(0, 0, overlay.Width, overlay.Height),
														GraphicsUnit.Pixel);
			}

			return result;
		}

		//Send the index of the last screenshot for the current recording
		public void RemoveDanglingImages(string taskPath, int index)
		{
			// a new recording might take less screenshots than the previous
			var imagePath = Path.Combine(taskPath, index + ".png");

			if (File.Exists(imagePath))
			{
				return;
			}

			File.Delete(imagePath);
			this.RemoveDanglingImages(taskPath, index + 1);
		}

		private static Image ConvertToTransparentImage(Image image)
		{
			Image theOverlay = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);

			using (Graphics graphics = Graphics.FromImage(theOverlay))
			{
				graphics.DrawImage(image, new Rectangle(0, 0, theOverlay.Width, theOverlay.Height), new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
			}

			((Bitmap)theOverlay).MakeTransparent();

			return theOverlay;
		}

		private unsafe BitmapDiff GetDiffImage(Bitmap bmp, Bitmap bmp2, Color? diffColor = null)
		{
			if (bmp.Width != bmp2.Width || bmp.Height != bmp2.Height)
				throw new Exception("Sizes must be equal.");

			Bitmap bmpRes = null;

			System.Drawing.Imaging.BitmapData bmData = null;
			System.Drawing.Imaging.BitmapData bmData2 = null;
			System.Drawing.Imaging.BitmapData bmDataRes = null;

			bool areSame = true;

			try
			{
				bmpRes = new Bitmap(bmp.Width, bmp.Height);

				bmData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				bmData2 = bmp2.LockBits(new Rectangle(0, 0, bmp2.Width, bmp2.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				bmDataRes = bmpRes.LockBits(new Rectangle(0, 0, bmpRes.Width, bmpRes.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

				IntPtr scan0 = bmData.Scan0;
				IntPtr scan02 = bmData2.Scan0;
				IntPtr scan0Res = bmDataRes.Scan0;

				int stride = bmData.Stride;
				int stride2 = bmData2.Stride;
				int strideRes = bmDataRes.Stride;

				int nWidth = bmp.Width;
				int nHeight = bmp.Height;

				//for(int y = 0; y < nHeight; y++)
				System.Threading.Tasks.Parallel.For(0, nHeight, y =>
				{
					//define the pointers inside the first loop for parallelizing
					byte* p = (byte*)scan0.ToPointer();
					p += y * stride;
					byte* p2 = (byte*)scan02.ToPointer();
					p2 += y * stride2;
					byte* pRes = (byte*)scan0Res.ToPointer();
					pRes += y * strideRes;

					for (int x = 0; x < nWidth; x++)
					{
						//always get the complete pixel when differences are found
						if (p[0] != p2[0] || p[1] != p2[1] || p[2] != p2[2])
						{
							areSame = false;

							if (diffColor.HasValue)
							{
								Color color = diffColor.Value;

								pRes[0] = color.B;//b
								pRes[1] = color.G;//g
								pRes[2] = color.R;//r
								//alpha (opacity)
								pRes[3] = color.A;//a
							}
							else
							{
								pRes[0] = p2[0];//b
								pRes[1] = p2[1];//g
								pRes[2] = p2[2];//r
								//alpha (opacity)
								pRes[3] = p2[3];//a
							}
						}

						p += 4;
						p2 += 4;
						pRes += 4;
					}
				});

				bmp.UnlockBits(bmData);
				bmp2.UnlockBits(bmData2);
				bmpRes.UnlockBits(bmDataRes);
			}
			catch
			{
				if (bmData != null)
				{
					try
					{
						bmp.UnlockBits(bmData);
					}
					catch
					{
					}
				}

				if (bmData2 != null)
				{
					try
					{
						bmp2.UnlockBits(bmData2);
					}
					catch
					{
					}
				}

				if (bmDataRes != null)
				{
					try
					{
						bmpRes.UnlockBits(bmDataRes);
					}
					catch
					{
					}
				}

				if (bmpRes != null)
				{
					bmpRes.Dispose();
					bmpRes = null;
				}

				areSame = false;
			}

			BitmapDiff diff = new BitmapDiff();
			diff.Diff = bmpRes;
			diff.AreSame = areSame;
			return diff;
		}
	}
}