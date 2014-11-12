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
		//32 bits in een int
		public const int FILE_LENGTH_LENGTH = 32;
		public const int FILE_LENGTH_OFFSET = 0;

		//40 bits in vijf bytes. Onze extensie is altijd 5 karakters of korter.
		public const int FILE_EXT_LENGTH = 40;
		public const int FILE_EXT_OFFSET = FILE_LENGTH_LENGTH;

		public const int FILE_OFFSET = FILE_EXT_OFFSET +  FILE_EXT_LENGTH;

		//Een overloaded method om bestandspaden in te voeren.
		public static Bitmap Embed(String targetFilePath, String inputFilePath)
		{
			Bitmap target = (Bitmap)Bitmap.FromFile(targetFilePath);
			target = Embed(target, inputFilePath);

			return target;
		}

		//Verstop een bestand in een afbeelding. Omdat Bitmap een absctracte representatie is
		//van een afbeelding kunnen we de Bitmap opslaan in eender welk formaat
		//Maar het bestand valt natuurlijk niet terug te halen uit een lossy formaat.
		public static Bitmap Embed(Bitmap target, String inputFilePath)
		{
			BitmapData bmpData = PrepareImage(target);

			//Math.Abs omdat Stride negatief is wanneer de afbeelding ondersteboven is opgeslagen.
			//D.w.z. dat de eerste pixel in het geheugen de pixel rechts onderaan is.
			int imageSize = Math.Abs(bmpData.Stride) * bmpData.Height;

			byte[] fileBytes = File.ReadAllBytes(inputFilePath);

			//Bestandslengte in bits en de ruimte nodig om de header op te slaan.
			//We hebben de lengte nodig in bits (*8) omdat elke bit in het bestand 
			//een byte nodig heeft in de afbeelding
			int bytesNeeded = (fileBytes.Length + FILE_OFFSET) * 8;

			//Als het bestand dat we willen opslaan te groot is, throw exception
			if (bytesNeeded > imageSize)
			{
				throw new FileTooLargeException("The file you are trying to embed needs an image of at least" + bytesNeeded + "bytes large");
			}

			//Splits de bestandsnaam op '.' en selecteer de extensie.
			string fileExtension = inputFilePath.Split('.').Last();

			WriteFileHeader(bmpData, fileBytes.Length, fileExtension);
			WriteFile(bmpData, fileBytes);

			target.UnlockBits(bmpData);

			return target;
		}

		//Schrijf de fileheader weg naar de afbeelding. 
		//De header bestaat uit de bestandslengte en de extensie.
		private static void WriteFileHeader(BitmapData bmpData, int fileLength, string extension)
		{
			WriteFileLength(bmpData, fileLength);
			WriteFileExtension(bmpData, extension);
		}

		//Schrijf de betandslengte naar de afbeelding
		unsafe private static void WriteFileLength(BitmapData bmpData, int fileLength)
		{
			byte* ptr = (byte*)bmpData.Scan0;

			//Begin de bestandslengte te schrijven op FILE_LENGTH_OFFSET. Dat is in dit geval de allereerste byte.
			for (int i = FILE_LENGTH_OFFSET; i < FILE_LENGTH_OFFSET + FILE_LENGTH_LENGTH; i++)
			{
				//Kijk by WriteFile voor de uitleg van deze lijn.
				ptr[i] = (byte)(ptr[i] & ~1 | Helper.GetBitAsInt(fileLength, 31 - i));
			}
		}

		//Schrijf de extensie van het bestand naar de afbeelding.
		unsafe private static void WriteFileExtension(BitmapData bmpData, string extension)
		{
			extension = extension.PadRight(5, ' ');
			//Extensie omzetten naar UTF-8. Dit voorkomt fouten door endianness en (bijv.) diakritische tekens, 
			//omdat deze in ASCII meerdere bytes gebruiken
			byte[] extensionBytes = System.Text.Encoding.UTF8.GetBytes(extension);

			byte* ptr = (byte*)bmpData.Scan0;

			//Begin de extensie te schrijven op FILE_EXT_OFFSET. Dit is de eerste byte na de bestandslengte.
			//Delen door 8 omdat we LENGTH het aantal bits is, maar we moeten itereren door de bytes.
			for (int i = 0; i < FILE_EXT_LENGTH / 8; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					//Bereken de index in de afbeelding, aangezien die niet gelijk is aan de index in ons bestand.
					int index = i * 8 + FILE_EXT_OFFSET + j;
					//Zie WriteFile() voor de uitleg van deze lijn.
					ptr[index] = (byte)(ptr[index] & ~1 | Helper.GetBitAsByte(extensionBytes[i], 7 - j));
				}
			}
		}

		//Schrijf het bestand naar de afbeelding
		unsafe private static void WriteFile(BitmapData bmpData, byte[] fileBytes)
		{
			byte* ptr = (byte*)bmpData.Scan0;

			Parallel.For(0, fileBytes.Length, (i) =>
			{
				//We moeten deze stap 8 keer per iteratie doen, omdat we 8 bytes 
				//in de afbeelding nodig hebben voor elke byte in het bestand
				for (int j = 0; j < 8; j++)
				{
					//Bereken de index in de afbeelding, aangezien die niet gelijk is aan de index in het bestand.
					int index = i * 8 + FILE_OFFSET + j;

					//AND de huidige waarde met ~1 (inverse van 1: 11111110).
					//Hierna is de laatste bit 0.
					//Dan OR met de nieuwe bit.
					//Helper.GetBitAsByte haalt een enkele bit op van een byte
					//En zet deze om naar een byte zodat we er booleanse wiskunde op kunnen toepassen.
					ptr[index] = (byte)(ptr[index] & ~1 | Helper.GetBitAsByte(fileBytes[i], 7 - j));
				}
			});
		}

		//Een overloaded method om bestandspaden in te geven
		public static byte[] Extract(String source, out string extension)
		{
			Bitmap carrier = (Bitmap)Bitmap.FromFile(source);

			byte[] file = Extract(carrier, out extension);

			carrier.Dispose();

			return file;
		}

		//Haal een verborgen bestand uit een afbeelding.
		//Geeft een byte[] terug zodat de consument met de data kan doen wat hij wilt.
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

		//Haal de bestandslengte op uit de afbeelding.
		unsafe private static int ExtractFileLength(BitmapData bmpData)
		{
			byte* ptr = (byte*)bmpData.Scan0;

			int length = 0;

			//Begin te lezen op FILE_LENGTH_OFFSET, in dit geval de allereerste byte			//Start reading at FILE_LENGTH_OFFSET, this is the first byte.
			for (int i = FILE_LENGTH_OFFSET; i < FILE_LENGTH_OFFSET + FILE_LENGTH_LENGTH; i++)
			{
				//Zet de laatste bit van de huidige file byte naar de laatste bit van de image byte.
				//Doe dan een shift naar links.
				//Dan kunnen we het volgende doen:
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

		//Haal de extensie op uit de afbeelding.
		unsafe private static string ExtractFileExtension(BitmapData bmpData)
		{
			byte* ptr = (byte*)bmpData.Scan0;

			//byte[5] omdat we weten dat de extensie maximum 5 karakters lang is.
			byte[] extension = new byte[5];

			for (int i = 0; i < FILE_EXT_LENGTH / 8; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					//Bereken de index in de afbeelding, omdat die niet gelijk is aan de index in het bestand.
					int index = i * 8 + FILE_EXT_OFFSET + j;

					//Zie ExtractFileLength voor meer uitleg.
					extension[i] <<= 1;
					extension[i] = (byte)(extension[i] & ~1 | Helper.GetBitAsByte(ptr[index], 0));
				}
			}

			string ext = System.Text.Encoding.UTF8.GetString(extension).Trim();;

			return ext;
		}

		//Haal het bestand uit de afbeelding.
		unsafe private static byte[] ExtractFile(BitmapData bmpData, int length)
		{
			byte* ptr = (byte*)bmpData.Scan0;

			byte[] file = new byte[length];

			for (int i = 0; i < length; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					//Bereken de index in de afbeelding, omdat die niet gelijk is aan de index in het bestand.
					int index = i * 8 + FILE_OFFSET + j;

					//Zie ExtractFileLength voor meer uitleg.
					file[i] <<= 1;
					file[i] = (byte)(file[i] & ~1 | Helper.GetBitAsByte(ptr[index], 0));
				}
			}

			return file;
		}

		//Dit is wat biolerplate om de image voor te bereiden op unmanaged toegang.
		public static BitmapData PrepareImage(Bitmap image)
		{
			//We hoeven geen specifiek gebied te locken, maar we moeten er toch een specifieren,
			//Dus nemen we de gehele afbeelding.
			Rectangle lockArea = new Rectangle(0, 0, image.Width, image.Height);

			//We gebruiken LockBits in plaats van GetPixel/SetPixel, omdat het zoveel sneller is.
			//Nadeel hiervan is dat we met unmanaged data en pointers moeten werken.
			BitmapData bmpData = image.LockBits(lockArea,
				System.Drawing.Imaging.ImageLockMode.ReadWrite,
				image.PixelFormat);

			return bmpData;
		}
	}
}
