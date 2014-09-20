using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace SteganoLib
{
	public class SteganoBMP
	{
		public static void Embed(Bitmap target, String filename)
		{
			//We don't require a specific area to be locked, but we still need to specify one
			//So we choose to lock the entire image.
			Rectangle lockArea = new Rectangle(0, 0, target.Width, target.Height);
			
			//We use LockBits instead of GetPixel, because it is much faster.
			//As a consequence we need to work with unmanaged data.
			BitmapData bmpData = target.LockBits(lockArea, 
				System.Drawing.Imaging.ImageLockMode.ReadWrite, 
				target.PixelFormat);

			//Math.Abs because Stride can be negative if the image is saved upside down
			//i.e. The first pixel in memory is the bottom right one.
			int imageSize = Math.Abs(bmpData.Stride) * bmpData.Height;

			byte[] fileBytes = File.ReadAllBytes(filename);

			unsafe
			{
				byte* ptr = (byte*)bmpData.Scan0;
				for (int i = 0; i < imageSize; i += 8)
				{
					//AND the current value with ~1 (inverse of 1: 11111110). 
					//This wil set the last bit to 0.
					//Then OR with the new bit
					//Helper.GetBitAsByte extracts a single bit from a byte.
					//And converts it to a byte, so we can do boolean arithmetic with it.
					//We also need to do this 8 times in one iteration, so that the indexers line up nicely.
					ptr[i] = (byte)(ptr[i] & ~1 | Helper.GetBitAsByte(fileBytes[i], 1));
					ptr[i + 1] = (byte)(ptr[i + 1] & ~1 | Helper.GetBitAsByte(fileBytes[i], 2));
					ptr[i + 2] = (byte)(ptr[i + 2] & ~1 | Helper.GetBitAsByte(fileBytes[i], 3));
					ptr[i + 3] = (byte)(ptr[i + 3] & ~1 | Helper.GetBitAsByte(fileBytes[i], 4));
					ptr[i + 4] = (byte)(ptr[i + 4] & ~1 | Helper.GetBitAsByte(fileBytes[i], 5));
					ptr[i + 5] = (byte)(ptr[i + 5] & ~1 | Helper.GetBitAsByte(fileBytes[i], 6));
					ptr[i + 6] = (byte)(ptr[i + 6] & ~1 | Helper.GetBitAsByte(fileBytes[i], 7));
					ptr[i + 7] = (byte)(ptr[i + 7] & ~1 | Helper.GetBitAsByte(fileBytes[i], 8));
				}
			}
		}

		public static void shiftTest()
		{
			byte a = 1;

			for (int i = 0; i < 8; i++)
			{
				a = Helper.RotateRight(a, 1);
				Console.WriteLine(Convert.ToString(a, 2));
			}

			byte b = 0xAA;

			for (int i = 0; i < 16; i++)
			{
				Console.WriteLine(Helper.GetBit(b, i));
			}

			Console.ReadLine();
		}
	}
}
