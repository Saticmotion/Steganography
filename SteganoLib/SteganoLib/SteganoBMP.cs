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
		//Embed a file in an image. Because Bitmap is an abstract representation of an image,
		//it can then be saved in any image format. Though the file will not be retrievable from
		//a lossy format.
		public static Bitmap Embed(Bitmap target, String inputFilePath)
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

			//Get all the bytes from the file we want to embed, and save it in a byte array
			byte[] fileBytes = File.ReadAllBytes(inputFilePath);

			//If the file we want to embed is larger than 8 times the size of the image, we can't store it.
			//This is because We need one byte in the image to store each bit of the file
			if (fileBytes.Length * 8 > imageSize)
				throw new FileTooLargeException("The file you are trying to embed needs an image of at least" + fileBytes.Length * 8 +  "bytes large");

			unsafe
			{
				byte* ptr = (byte*)bmpData.Scan0;

				for (int i = 0; i < imageSize; i += 8)
				{
					
					//We need to do this step 8 times per iteration because we need to access 8 bytes
					//in the image for each byte in the file.
					for (int j = 0; j < 8; j++)
					{
						//AND the current value with ~1 (inverse of 1: 11111110). 
						//This wil set the last bit to 0.
						//Then OR with the new bit
						//Helper.GetBitAsByte extracts a single bit from a byte.
						//And converts it to a byte, so we can do boolean arithmetic with it.
						ptr[i + j] = (byte)(ptr[i + j] & ~1 | Helper.GetBitAsByte(fileBytes[i], j));
					}
				}
			}

			target.UnlockBits(bmpData);

			return target;
		}

		public static byte[] Extract(Bitmap source)
		{
			//For explanatiosn of this first bit of boilerplate code, see Embed()
			Rectangle lockArea = new Rectangle(0, 0, source.Width, source.Height);

			BitmapData bmpData = source.LockBits(lockArea,
				ImageLockMode.ReadOnly,
				source.PixelFormat);

			int imageSize = Math.Abs(bmpData.Stride) * bmpData.Height;

			//We need one byte for every 8 bytes in the image
			byte[] fileBytes = new byte[imageSize / 8];

			unsafe
			{
				//Get a pointer to the first pixel;
				byte* ptr = (byte*)bmpData.Scan0;

				for (int i = 0; i < imageSize; i++)
				{
					for (int j = 0; j < 8; j++)
					{
						//Set the last bit of the current file byte to the last bit of the image byte.
						//Then shift it one spot to the left.
						//This allows us to do this:
						//0000 0001
						//0000 0011
						//0000 0110
						//0000 1101
						//etc.
						fileBytes[i] = (byte)(fileBytes[i] & ~1 | Helper.GetBitAsByte(ptr[i + j], 7));
						fileBytes[i] <<= 1;
					}
				}
			}

			return fileBytes;

			//TODO: Make an actual file out of the byte array
			//Possibly also try to detect MIME types to correctly save the file.
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
