using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using SteganoLib;

namespace SteganoTest
{
	class Program
	{
		static void Main(string[] args)
		{

			//TODO: test this test.
			String originalFilePath = args[1];
			Bitmap originalImage = (Bitmap)Bitmap.FromFile(args[0]);
			Bitmap steganoImage;
			byte[] originalFile = File.ReadAllBytes(originalFilePath);
			byte[] embeddedFile;

			steganoImage = SteganoBMP.Embed(originalImage, originalFilePath);
			embeddedFile = SteganoBMP.Extract(steganoImage);

			if (Enumerable.SequenceEqual(originalFile, embeddedFile))
			{
				Console.WriteLine("Succes!");
			}
		}
	}
}
