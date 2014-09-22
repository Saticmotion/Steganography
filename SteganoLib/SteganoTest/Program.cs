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
			String originalFilePath = @"..\..\testFiles\hide.txt";
			Bitmap originalImage = (Bitmap)Bitmap.FromFile(@"..\..\testFiles\test.jpg");
			Bitmap steganoImage;
			byte[] originalFile = File.ReadAllBytes(originalFilePath);
			byte[] embeddedFile;

			steganoImage = SteganoBMP.Embed(originalImage, originalFilePath);
			Console.WriteLine(String.Join(" ", originalFile));
			embeddedFile = SteganoBMP.Extract(steganoImage);
			Console.WriteLine(String.Join(" ", embeddedFile));

			if (Enumerable.SequenceEqual(originalFile, embeddedFile))
			{
				Console.WriteLine("Succes!");
			}
			Console.ReadLine();

		}
	}
}
