using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using SteganoLib;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using Forms = System.Windows.Forms;
using System.Diagnostics;

namespace gui
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private Stream _messageStreamWav;
		private Stream _carrierStreamWav;
		private String _path;
		private String inputPathBMP;
		private String targetPathBMP;
		private String ExtractPathBMP;
		private String ExtractTargetPathBMP;
		

		public MainWindow()
		{
			InitializeComponent();
		}

		private void BtnOpenFile_Click(object sender, RoutedEventArgs e)
		{

			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
			Nullable<bool> result = dialog.ShowDialog();
			if (result == true)
			{
				_messageStreamWav = dialog.OpenFile();
				TxtFileToHide.Text = dialog.SafeFileName;
			}
		}
		
		private void BtnOpenFileMessageCarrier_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "Audio files (*.wav)|*.wav|Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF";
			Nullable<bool> result = dialog.ShowDialog();
			if (result == true)
			{
				_carrierStreamWav = dialog.OpenFile();
				TxtFileCarrier.Text = dialog.SafeFileName;
				_path = dialog.FileName;

				AudioPlayer.InitializeMedia(_path);

			}
		}

		private void BtnOpenInputBMP_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
			dialog.Multiselect = false;
			bool? result = dialog.ShowDialog();
			
			if (result == true)
			{
				inputPathBMP = dialog.FileName;
				TxtFileToHideBMP.Text = inputPathBMP;
			}
		}

		private void BtnOpenTargetBMP_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "Bitmap Image (.bmp)|*.bmp|Png Image (.png)|*.png";
			dialog.Multiselect = false;
			bool? result = dialog.ShowDialog();

			if (result == true)
			{
				targetPathBMP = dialog.FileName;
				TxtFileTargetBMP.Text = targetPathBMP;
			}
		}

		private void BtnHideBMP_Click(object sender, RoutedEventArgs e)
		{
			if (targetPathBMP != "" && inputPathBMP != "")
			{
				Bitmap embedded = SteganoBMP.Embed(targetPathBMP, inputPathBMP);

				SaveFileDialog save = new SaveFileDialog();
				save.Filter = "Bitmap Image (.bmp)|*.bmp|Png Image (.png)|*.png";

				bool? result = save.ShowDialog();

				if (result == true)
				{
					string savePath = save.FileName;

					EncoderParameters encoderParams = new EncoderParameters(1);
					EncoderParameter encoderP = new EncoderParameter(Encoder.Quality, 100L);
					encoderParams.Param[0] = encoderP;

					embedded.Save(savePath, GetEncoder(savePath.Split('.').Last()), encoderParams);

					embedded.Dispose();
					showImages(targetPathBMP, savePath);
				}
			}
		}

		private void showImages(String originalPath, String embeddedPath)
		{
			showOriginal(originalPath);
			showDiff(embeddedPath, originalPath);
		}

		private void showOriginal(String originalPath)
		{
			BitmapImage image = new BitmapImage(new Uri(originalPath));
			OrigImg.Source = image;
		}

		private void showDiff(String embeddedPath, String originalPath)
		{
			BitmapImage diff = CalculateDiff(embeddedPath, originalPath);
			DiffImg.Source = diff;
		}

		unsafe private BitmapImage CalculateDiff(String embeddedPath, String originalPath)
		{
			using(Bitmap original = new Bitmap(originalPath))
			using(Bitmap embedded = new Bitmap(embeddedPath))
			using (Bitmap diff = new Bitmap(original.Width, original.Height))
			{
				BitmapData dataOrig = SteganoBMP.PrepareImage(original);
				BitmapData dataEmbed = SteganoBMP.PrepareImage(embedded);
				BitmapData dataDiff = SteganoBMP.PrepareImage(diff);

				byte* ptrOrig = (byte*)dataOrig.Scan0;
				byte* ptrEmbed = (byte*)dataEmbed.Scan0;
				byte* ptrDiff = (byte*)dataDiff.Scan0;

				int bytes = dataOrig.Stride * dataOrig.Height;

				byte diffValue = 1;

				if (chkAmplify.IsChecked == true)
				{
					diffValue = 255;
				}

				for (int i = 0; i < bytes; i++)
				{
					if (ptrOrig[i] != ptrEmbed[i])
					{
						ptrDiff[i] = diffValue;
					}
				}

				original.UnlockBits(dataOrig);
				embedded.UnlockBits(dataEmbed);
				diff.UnlockBits(dataDiff);

				return Bitmap2BitmapImage(diff);
			}
		}

		private static ImageCodecInfo GetEncoder(string extension)
		{
			if (extension == "png")
			{
				return GetEncoder(ImageFormat.Png);
			}
			else if (extension == "bmp")
			{
				return GetEncoder(ImageFormat.Bmp);
			}
			else
			{
				throw new FormatException("Invalid image format, only PNG and BMP are allowed");
			}
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

		//http://stackoverflow.com/questions/6484357/converting-bitmapimage-to-bitmap-and-vice-versa
		[System.Runtime.InteropServices.DllImport("gdi32.dll")]
		public static extern bool DeleteObject(IntPtr hObject);

		private BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
		{
			using (MemoryStream memory = new MemoryStream())
			{
				bitmap.Save(memory, ImageFormat.Png);
				memory.Position = 0;
				BitmapImage bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				bitmapImage.StreamSource = memory;
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.EndInit();

				return bitmapImage;
			}
		}
		private void BtnSelectCarrier_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "Bitmap Image (.bmp)|*.bmp|Png Image (.png)|*.png";
			dialog.Multiselect = false;
			bool? result = dialog.ShowDialog();

			if (result == true)
			{
				ExtractPathBMP = dialog.FileName;
				TxtFileToExtract.Text = ExtractPathBMP;
			}
		}

		private void BtnSelectCarrierTarget_Click(object sender, RoutedEventArgs e)
		{
			using (Forms.FolderBrowserDialog dialog = new Forms.FolderBrowserDialog())
			{
				Forms.DialogResult result = dialog.ShowDialog();

				if (result == Forms.DialogResult.OK)
				{
					ExtractTargetPathBMP = dialog.SelectedPath;
					TxtExtractTarget.Text = ExtractPathBMP;
				}
			}
		}

		private void BtnExtractFromCarrier_Click(object sender, RoutedEventArgs e)
		{
			if (ExtractPathBMP != "" && ExtractTargetPathBMP != "")
			{
				string extension;

				byte[] extracted = SteganoBMP.Extract(ExtractPathBMP, out extension);
				
				string savePath = ExtractTargetPathBMP + "Extracted." + extension;

				File.WriteAllBytes(savePath, extracted);

				Process.Start(savePath);
			}
		}
	}
}
