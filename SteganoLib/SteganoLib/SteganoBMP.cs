using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace SteganoLib
{
	public class SteganoBMP
	{
		//Embed a file in an image. Because Bitmap is an abstract representation of an image,
		//it can then be saved in any image format. Though the file will not be retrievable from
		//a lossy format.
		public static Bitmap Embed(Bitmap target, String inputFilePath)
		{
			BitmapData bmpData = PrepareImage(target);

			//Math.Abs because Stride can be negative if the image is saved upside down
			//i.e. The first pixel in memory is the bottom right one.
			int imageSize = Math.Abs(bmpData.Stride) * bmpData.Height;

			byte[] fileBytes = File.ReadAllBytes(inputFilePath);

			//If the file we want to embed is larger than 8 times the size of the image, we can't store it.
			//This is because We need one byte in the image to store each bit of the file
			if (fileBytes.Length * 8 > imageSize)
			{
				throw new FileTooLargeException("The file you are trying to embed needs an image of at least" + fileBytes.Length * 8 + "bytes large");
			}

			unsafe
			{
				byte* ptr = (byte*)bmpData.Scan0;

				Parallel.For(0, fileBytes.Length, (i) =>
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
						ptr[i * 8 + j] = (byte)(ptr[i * 8 + j] & ~1 | Helper.GetBitAsByte(fileBytes[i], 7 - j));
					}
				});

				//This is the last byte in the file we're embedding, so we've used 8 times as many in our image, hence times 8
				int lastByte = fileBytes.Length * 8;

				//Write an EOF character (0xff) after our data.
				for (int i = lastByte; i < lastByte + 8; i++)
				{
					ptr[i] = (byte)(ptr[i] | 1);
				}
			}

			target.UnlockBits(bmpData);

			return target;
		}

		//Extract an embedded file from an image.
		//Returns a byte[], so the consumer can do with the data whatever he likes.
		public static byte[] Extract(Bitmap source)
		{
			BitmapData bmpData = PrepareImage(source);

			int imageSize = Math.Abs(bmpData.Stride) * bmpData.Height;

			//We use a List, because we don't know in advance how big the enbedded file is.
			List<byte> fileBytes = new List<byte>();

			bool fileRead = false;

			unsafe
			{
				//Get a pointer to the first pixel;
				byte* ptr = (byte*)bmpData.Scan0;

				for (int i = 0; i < imageSize; i++)
				{
					//Add a zero byte to our List
					fileBytes.Add(0x0);

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
						fileBytes[i] <<= 1;
						fileBytes[i] = (byte)(fileBytes[i] & ~1 | Helper.GetBitAsByte(ptr[i * 8 + j], 0));

						//If we read EOF we're done.
						if (fileBytes[i] == 0xff)
						{
							fileRead = true;
							break;
						}
					}

					if (fileRead)
					{
						break;
					}
				}
			}

			source.UnlockBits(bmpData);

			//Remove our last byte, this is the EOF byte.
			fileBytes.RemoveAt(fileBytes.Count - 1);

			byte[] byteArray = fileBytes.ToArray<byte>();

			return byteArray;
		}

		//This is some boilerplate code to prepare our image for unmanaged access.
		private static BitmapData PrepareImage(Bitmap image)
		{
			//We don't require a specific area to be locked, but we still need to specify one
			//So we choose to lock the entire image.
			Rectangle lockArea = new Rectangle(0, 0, image.Width, image.Height);

			//We use LockBits instead of GetPixel, because it is much faster.
			//As a consequence we need to work with unmanaged data.
			BitmapData bmpData = image.LockBits(lockArea,
				System.Drawing.Imaging.ImageLockMode.ReadWrite,
				image.PixelFormat);

			return bmpData;
		}
	}
}
