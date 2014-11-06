using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SteganoLib
{
	public class SteganoBMP
	{
		//32 bits in an int
		public const int FILE_LENGTH_LENGTH = 32;
		public const int FILE_LENGTH_OFFSET = 0;

		//24 bits in three bytes. Our entension is always three characters
		public const int FILE_EXT_LENGTH = 24;
		public const int FILE_EXT_OFFSET = FILE_LENGTH_LENGTH;

		public const int FILE_OFFSET = FILE_EXT_OFFSET +  FILE_EXT_LENGTH;

		public static Bitmap Embed(String targetFilePath, String inputFilePath)
		{
			Bitmap target = (Bitmap)Bitmap.FromFile(targetFilePath);
			target = Embed(target, inputFilePath);

			return target;
		}

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

			//Length of file in bits plus the space needed by our file header.
			//We need the length in bits because each bit in our file needs a byte of space in the image.
			int bytesNeeded = (fileBytes.Length + FILE_OFFSET) * 8;

			//If the file we want to embed is too large, throw an exception.
			if (bytesNeeded > imageSize)
			{
				throw new FileTooLargeException("The file you are trying to embed needs an image of at least" + bytesNeeded + "bytes large");
			}

			//Split our filename on '.', and select the extension
			string fileExtension = inputFilePath.Split('.').Last();

			WriteFileHeader(bmpData, fileBytes.Length, fileExtension);
			WriteFile(bmpData, fileBytes);

			target.UnlockBits(bmpData);

			return target;
		}

		//Write the file header to the image, consisting of the filelength and the extension
		private static void WriteFileHeader(BitmapData bmpData, int fileLength, string extension)
		{
			WriteFileLength(bmpData, fileLength);
			WriteFileExtension(bmpData, extension);
		}

		//Write the length of the file to the image
		unsafe private static void WriteFileLength(BitmapData bmpData, int fileLength)
		{
			byte* ptr = (byte*)bmpData.Scan0;

			//Start writing the file length at FILE_LENGTH_OFFSET. This is the first byte.
			for (int i = FILE_LENGTH_OFFSET; i < FILE_LENGTH_OFFSET + FILE_LENGTH_LENGTH; i++)
			{
				//See WriteFile() for an explanation of this line.
				ptr[i] = (byte)(ptr[i] & ~1 | Helper.GetBitAsInt(fileLength, 31 - i));
			}
		}

		//Write the extension of the file to the image
		unsafe private static void WriteFileExtension(BitmapData bmpData, string extension)
		{
			//Convert extension to its UTF-8 representation.
			byte[] extensionBytes = System.Text.Encoding.UTF8.GetBytes(extension);

			byte* ptr = (byte*)bmpData.Scan0;

			//Start writing the file extension at FILE_EXT_OFFSET. This is the first byte after our file length.
			//Division by 8 because we've stored the LENGTH as bits, but need to iterate through bytes.
			for (int i = 0; i < FILE_EXT_LENGTH / 8; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					//Calculate the index of the image, since it's not equal to the index of the file.
					int index = i * 8 + FILE_EXT_OFFSET + j;
					//See WriteFile() for an explanation of this line.
					ptr[index] = (byte)(ptr[index] & ~1 | Helper.GetBitAsByte(extensionBytes[i], 7 - j));
				}
			}
		}

		//Write the file to the image
		unsafe private static void WriteFile(BitmapData bmpData, byte[] fileBytes)
		{
			byte* ptr = (byte*)bmpData.Scan0;

			Parallel.For(0, fileBytes.Length, (i) =>
			{
				//We need to do this step 8 times per iteration because we need to access 8 bytes
				//in the image for each byte in the file.
				for (int j = 0; j < 8; j++)
				{
					//Calculate the index of the image, since it's not equal to the index of the file.
					int index = i * 8 + FILE_OFFSET + j;

					//AND the current value with ~1 (inverse of 1: 11111110).
					//This wil set the last bit to 0.
					//Then OR with the new bit
					//Helper.GetBitAsByte extracts a single bit from a byte.
					//And converts it to a byte, so we can do boolean arithmetic with it.
					ptr[index] = (byte)(ptr[index] & ~1 | Helper.GetBitAsByte(fileBytes[i], 7 - j));
				}
			});
	}

		public static byte[] Extract(String source, out string extension)
		{
			Bitmap carrier = (Bitmap)Bitmap.FromFile(source);

			byte[] file = Extract(carrier, out extension);

			carrier.Dispose();

			return file;
		}

		//Extract an embedded file from an image.
		//Returns a byte[], so the consumer can do with the data whatever he likes.
		public static byte[] Extract(Bitmap source, out string extension)
		{
			BitmapData bmpData = PrepareImage(source);

			int length = ExtractFileLength(bmpData);
			extension = ExtractFileExtension(bmpData);

			byte[] file = ExtractFile(bmpData, length);

			source.UnlockBits(bmpData);
			source.Dispose();

			return file;
		}

		//Extract the file length from the image
		unsafe private static int ExtractFileLength(BitmapData bmpData)
		{
			byte* ptr = (byte*)bmpData.Scan0;

			int length = 0;

			//Start reading at FILE_LENGTH_OFFSET, this is the first byte.
			for (int i = FILE_LENGTH_OFFSET; i < FILE_LENGTH_OFFSET + FILE_LENGTH_LENGTH; i++)
			{
				//Set the last bit of the current file byte to the last bit of the image byte.
				//Then shift it one spot to the left.
				//This allows us to do this:
				//0000 0001
				//0000 0011
				//0000 0110
				//0000 1101
				//etc.
				length <<= 1;
				length = length & ~1 | Helper.GetBitAsInt(ptr[i], 0);
			}

			return length;
		}

		//Extract the extension of the file from the image
		unsafe private static string ExtractFileExtension(BitmapData bmpData)
		{
			byte* ptr = (byte*)bmpData.Scan0;

			//byte[3] because we know the extension will always be three characters long.
			byte[] extension = new byte[3];

			for (int i = 0; i < FILE_EXT_LENGTH / 8; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					//Calculate the index of the image, since it's not equal to the index of the file.
					int index = i * 8 + FILE_EXT_OFFSET + j;

					//See ExtractFileLength for an explanation of this technique
					extension[i] <<= 1;
					extension[i] = (byte)(extension[i] & ~1 | Helper.GetBitAsByte(ptr[index], 0));
				}
			}

			return System.Text.Encoding.UTF8.GetString(extension);
		}

		//Extract the file from the image
		unsafe private static byte[] ExtractFile(BitmapData bmpData, int length)
		{
			byte* ptr = (byte*)bmpData.Scan0;

			byte[] file = new byte[length];

			for (int i = 0; i < length; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					//Calculate the index of the image, since it's not equal to the index of the file.
					int index = i * 8 + FILE_OFFSET + j;

					//See ExtractFileLength for an explanation of this technique
					file[i] <<= 1;
					file[i] = (byte)(file[i] & ~1 | Helper.GetBitAsByte(ptr[index], 0));
				}
			}

			return file;
		}

		//This is some boilerplate code to prepare our image for unmanaged access.
		public static BitmapData PrepareImage(Bitmap image)
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
