using SteganoLib;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace SteganoTest
{
	class Program
	{
		static void Main(string[] args)
		{
			String originalFilePath = @"..\..\testFiles\hide.txt";
			String extractedFilePath = @"..\..\testFiles\extract";
			String savePath = @"..\..\testFiles\save.png";
			Bitmap originalImage = (Bitmap)Bitmap.FromFile(@"..\..\testFiles\test.png");
			Bitmap steganoImage;

			steganoImage = SteganoBMP.Embed(originalImage, originalFilePath);

			EncoderParameters encoderParams = new EncoderParameters(1);
			EncoderParameter encoderP = new EncoderParameter(Encoder.Quality, 100L);
			encoderParams.Param[0] = encoderP;

			steganoImage.Save(savePath, GetEncoder(ImageFormat.Png), encoderParams);

			string extension;
			byte[] file = SteganoBMP.Extract(steganoImage, out extension);

			File.WriteAllBytes(extractedFilePath + "." + extension, file);

			Console.Write("Done. Press any key.");
			Console.ReadLine();

			String orFilePath = @"..\..\testFiles\blub.txt";
			String sPath = @"..\..\testFiles\resultJungle.wav";
			String waveFile = @"..\..\testFiles\WelcometotheJungle.wav";
			byte[] resultBytes = SteganoWav.Embed(waveFile, orFilePath);
			File.WriteAllBytes(sPath, resultBytes);



			byte[] originalBytes = SteganoWav.Extract(sPath);
			File.WriteAllBytes(@"..\..\testFiles\resulttext."+ SteganoWav.Extention, originalBytes);
			Console.ReadLine();

		}

		private static ImageCodecInfo GetEncoder(ImageFormat format)
		{

			ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

			foreach (ImageCodecInfo codec in codecs)
			{
				if (codec.FormatID == format.Guid)
				{
					return codec;
				}
			}
			return null;
		}
	}
}
