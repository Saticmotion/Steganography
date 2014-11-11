using Microsoft.Win32;
using SteganoLib;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Forms = System.Windows.Forms;
using System.Diagnostics;
using SteganoLib;
using desEncryption;

namespace gui
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private string _messageString;
		private string _carrierString;
		private string _extractString;
		private string _outputMessageFilepath;
		private String inputPathBMP;
		private String targetPathBMP;
		private String ExtractPathBMP;
		private String ExtractTargetPathBMP;
		private String encryptionFile;
		private String keyFile;
		private String desTargetPathBMP;
		private String desTargetPathBMPExtract;
		private String desTargetPathWAV;
		private String desTargetPathWAVExtract;

		public MainWindow()
		{
			InitializeComponent();
			MakeAudioHidden();
			BtnEncrypt.IsEnabled = false;
			BtnExtract.IsEnabled = false;
			BtnOpenMessageFile.IsEnabled = false;
		}
		private void BtnOpenFile_Click(object sender, RoutedEventArgs e)
		{

			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
			Nullable<bool> result = dialog.ShowDialog();
			if (result == true)
			{
				_messageString = dialog.FileName;
				TxtFileToHide.Text = dialog.FileName;
				EnableEncryptButton();
			}
		}
		private void BtnOpenFileMessageCarrier_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "Audio files (*.wav)|*.wav";
			Nullable<bool> result = dialog.ShowDialog();
			if (result == true)
			{
				_carrierString = dialog.FileName;
				TxtFileCarrier.Text = dialog.FileName;
				EnableEncryptButton();
				LblOriginalAudio.Content = dialog.SafeFileName;
			}
		}

		private void EnableEncryptButton()
		{
			if (_carrierString != null && _messageString != null)
			{
				BtnEncrypt.IsEnabled = true;
			}
			else
			{
				BtnEncrypt.IsEnabled = false;
			}
		}

		private void BtnEncrypt_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog sdialog = new SaveFileDialog();
			sdialog.Filter = "Audio files (*.wav)|*.wav";
			Nullable<bool> result = sdialog.ShowDialog();
			if (result == true)
			{
				try
				{
					byte[] encryptedWavFile = SteganoWav.Embed(_carrierString, _messageString);
					File.WriteAllBytes(sdialog.FileName, encryptedWavFile);
					AudioPlayer2.InitializeMedia(sdialog.FileName);
					AudioPlayer.InitializeMedia(_carrierString);
					LblEncryptedAudio.Content = sdialog.SafeFileName;
					MakeAudioVisible();
				}

				catch (FileTooLargeException fileTooLargeException)
				{
					MessageBox.Show(fileTooLargeException.Message);
				}
				

			}
		}

		private void MakeAudioVisible()
		{
			LblEncryptedAudio.Visibility = Visibility.Visible;
			LblOriginalAudio.Visibility = Visibility.Visible;
			AudioPlayerLabel.Visibility = Visibility.Visible;
			AudioPlayer.Visibility = Visibility.Visible;
			AudioPlayer2.Visibility = Visibility.Visible;
			AudioRectangle.Visibility = Visibility.Visible;
			CloseMediaButton.Visibility = Visibility.Visible;
			BtnEncrypt.IsEnabled = false;
			BtnEncrypt.ToolTip = "Cannot encrypt while AudioPlayer is open";
		}

		private void MakeAudioHidden()
		{
			LblEncryptedAudio.Visibility = Visibility.Hidden;
			LblOriginalAudio.Visibility = Visibility.Hidden;
			AudioPlayerLabel.Visibility = Visibility.Hidden;
			AudioPlayer.Visibility = Visibility.Hidden;
			AudioPlayer2.Visibility = Visibility.Hidden;
			AudioRectangle.Visibility = Visibility.Hidden;
			CloseMediaButton.Visibility = Visibility.Hidden;
			BtnEncrypt.IsEnabled = true;
			BtnEncrypt.ToolTip = "Click to encrypt";
		}

		private void BtnOpenFileExtractCarrier_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "Audio files (*.wav)|*.wav";
			Nullable<bool> result = dialog.ShowDialog();
			if (result == true)
			{
				_extractString = dialog.FileName;
				TxtFileCarrierExtract.Text = _extractString;
				BtnExtract.IsEnabled = true;
			}

		}

		private void BtnExtract_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog sDialog = new SaveFileDialog();

			byte[] resultMessage = SteganoWav.Extract(_extractString);
			string extention = SteganoWav.Extention;
			sDialog.Filter = "(*." + extention + ")|*" + extention;

			Nullable<bool> result = sDialog.ShowDialog();
			if (result == true)
			{
				_outputMessageFilepath = sDialog.FileName + "." + extention;
				File.WriteAllBytes(_outputMessageFilepath,resultMessage);
				BtnOpenMessageFile.IsEnabled = true;
			}
		}

		private void BtnOpenMessageFile_Click(object sender, RoutedEventArgs e)
		{
			System.Diagnostics.Process.Start(_outputMessageFilepath);
		}

		private void CloseMediaButton_Click(object sender, RoutedEventArgs e)
		{
			AudioPlayer.CloseMedia();
			AudioPlayer2.CloseMedia();
			MakeAudioHidden();
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
				try
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
				catch (FileTooLargeException fileTooLargeException)
				{
					MessageBox.Show(fileTooLargeException.Message);
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
					TxtExtractTarget.Text = ExtractTargetPathBMP;
				}
			}
		}

		private void BtnExtractFromCarrier_Click(object sender, RoutedEventArgs e)
		{
			if (ExtractPathBMP != "" && ExtractTargetPathBMP != "")
			{
				string extension;

				byte[] extracted = SteganoBMP.Extract(ExtractPathBMP, out extension);
				
				string savePath = ExtractTargetPathBMP + "\\Extracted." + extension;

				File.WriteAllBytes(savePath, extracted);

				Process.Start(savePath);
			}
		}

		private void btnEncryptFile_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "Text files (*.txt)|*.txt|Des files (*.des)|*.des|All files (*.*)|*.*";
			dialog.Multiselect = false;
			bool? result = dialog.ShowDialog();

			if (result == true)
			{
				encryptionFile = dialog.FileName;
				txtEncryptFile.Text = encryptionFile;
			}
		}

		private void btnKeyFile_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "All files (*.*)|*.*";
			dialog.Multiselect = false;
			bool? result = dialog.ShowDialog();

			if (result == true)
			{
				keyFile = dialog.FileName;
				txtKeyFile.Text = keyFile;
			}
		}

		private void btnOutputFile_Click(object sender, RoutedEventArgs e)
		{
			encryptionFile = txtEncryptFile.Text;
			keyFile = txtKeyFile.Text;
			SaveFileDialog dialog = new SaveFileDialog();
			dialog.OverwritePrompt = true;
			
			dialog.Filter = "All files (*.*)|*.*";
			bool? result = dialog.ShowDialog();

			if (result == true)
			{
				java.lang.Class clazz = typeof(Encryption);
				java.lang.Thread.currentThread().setContextClassLoader(clazz.getClassLoader());
				EncryptionMode mode;
				String modeString;

				if (rdbEncrypt.IsChecked == true){
					mode = EncryptionMode.ENCRYPT;
					modeString = "Encryption";
					MessageBox.Show("checked");
				}
				else{
					mode = EncryptionMode.DECRYPT;
					modeString = "Decryption";
				}

				String outputFile = dialog.FileName;
				try
				{
					if (Encryption.encrypt(encryptionFile, outputFile, keyFile, mode) == java.lang.Boolean.TRUE)
						MessageBox.Show(modeString + " complete !");
				}
				catch (java.io.IOException ex)
				{
					String error = ex.getMessage();
					MessageBox.Show(error);
				}
			}
		}

		private void btnDesBitmap_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "Bitmap Image (.bmp)|*.bmp|Png Image (.png)|*.png";
			dialog.Multiselect = false;
			bool? result = dialog.ShowDialog();

			if (result == true)
			{
				desTargetPathBMP = dialog.FileName;
				txtDesBitmap.Text = desTargetPathBMP;
			}
		}

		private void btnDesOutputBitmap_Click(object sender, RoutedEventArgs e)
		{
			desTargetPathBMP = txtDesBitmap.Text;
			if (!desTargetPathBMP.Equals(""))
			{
				SaveFileDialog save = new SaveFileDialog();
				save.Filter = "Bitmap Image (.bmp)|*.bmp|Png Image (.png)|*.png";

				bool? result = save.ShowDialog();

				if (result == true)
				{

					java.lang.Class clazz = typeof(Encryption);
					java.lang.Thread.currentThread().setContextClassLoader(clazz.getClassLoader());

					String outputFile = save.FileName;
					try
					{
						if (Encryption.encrypt(encryptionFile, "encryptForBMP", keyFile, EncryptionMode.ENCRYPT) == java.lang.Boolean.TRUE)
						{
							Bitmap embedded = SteganoBMP.Embed(desTargetPathBMP, "encryptForBMP.des");
							String savePath = save.FileName;

							EncoderParameters encoderParams = new EncoderParameters(1);
							EncoderParameter encoderP = new EncoderParameter(Encoder.Quality, 100L);
							encoderParams.Param[0] = encoderP;

							embedded.Save(savePath, GetEncoder(savePath.Split('.').Last()), encoderParams);

							embedded.Dispose();
							File.Delete("encryptForBMP.des");
							MessageBox.Show("File successfully encrypted to image file");
						}
							
					}
					catch (java.io.IOException ex)
					{
						String error = ex.getMessage();
						MessageBox.Show(error);
					}
					catch (FileTooLargeException fileTooLargeException)
					{
						MessageBox.Show(fileTooLargeException.Message);
					}
				}
			}
			else
			{
				MessageBox.Show("Please select a valid image file");
			}
		}

		private void btnDesBitmapExtract_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "Bitmap Image (.bmp)|*.bmp|Png Image (.png)|*.png";
			dialog.Multiselect = false;
			bool? result = dialog.ShowDialog();

			if (result == true)
			{
				desTargetPathBMPExtract = dialog.FileName;
				txtDesBitmapExtract.Text = desTargetPathBMPExtract;
			}
		}

		private void btnDesOutputBitmapExtract_Click(object sender, RoutedEventArgs e)
		{
			desTargetPathBMPExtract = txtDesBitmapExtract.Text;
			if (!desTargetPathBMPExtract.Equals(""))
			{   
				SaveFileDialog save = new SaveFileDialog();
				save.Filter = "All files (*.*)|*.*";

				bool? result = save.ShowDialog();

				if (result == true)
				{

					String extension;
					byte[] extracted = SteganoBMP.Extract(desTargetPathBMPExtract, out extension);
					string savePath = "extractedMessage." + extension;
					File.WriteAllBytes(savePath, extracted);

					java.lang.Class clazz = typeof(Encryption);
					java.lang.Thread.currentThread().setContextClassLoader(clazz.getClassLoader());

					String outputFile = save.FileName;
					try
					{
						if (Encryption.encrypt("extractedMessage.des", outputFile, keyFile, EncryptionMode.DECRYPT) == java.lang.Boolean.TRUE)
						{
							File.Delete("extractedMessage.des");
							MessageBox.Show("File successfully decrypted from image file");
						}

					}
					catch (java.io.IOException ex)
					{
						String error = ex.getMessage();
						MessageBox.Show(error);
					}
				}
			}
			else
			{
				MessageBox.Show("Please select a valid image file");
			}
		}

		private void btnDesWAV_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "Audio files (*.wav)|*.wav";
			dialog.Multiselect = false;
			Nullable<bool> result = dialog.ShowDialog();
			if (result == true)
			{
				desTargetPathWAV = dialog.FileName;
				txtDesWAV.Text = desTargetPathWAV;
			}
		}

		private void btnDesWAVExtract_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "Audio files (*.wav)|*.wav";
			dialog.Multiselect = false;
			Nullable<bool> result = dialog.ShowDialog();
			if (result == true)
			{
				desTargetPathWAVExtract = dialog.FileName;
				txtDesWAVExtract.Text = desTargetPathWAVExtract;
			}
		}

		private void btnDesOutputWAV_Click(object sender, RoutedEventArgs e)
		{
			desTargetPathWAV = txtDesWAV.Text;
			if (!desTargetPathWAV.Equals(""))
			{
				SaveFileDialog save = new SaveFileDialog();
				save.Filter = "Audio files (*.wav)|*.wav";

				bool? result = save.ShowDialog();

				if (result == true)
				{

					java.lang.Class clazz = typeof(Encryption);
					java.lang.Thread.currentThread().setContextClassLoader(clazz.getClassLoader());

					String outputFile = save.FileName;
					try
					{
						if (Encryption.encrypt(encryptionFile, "encryptForWAV", keyFile, EncryptionMode.ENCRYPT) == java.lang.Boolean.TRUE)
						{
							String savePath = save.FileName;

							byte[] encryptedWavFile = SteganoWav.Embed(desTargetPathWAV, "encryptForWAV.des");
							File.WriteAllBytes(savePath, encryptedWavFile);

							File.Delete("encryptForWAV.des");
							MessageBox.Show("File successfully encrypted to audio file");
						}

					}
					catch (java.io.IOException ex)
					{
						String error = ex.getMessage();
						MessageBox.Show(error);
					}
					catch (FileTooLargeException fileTooLargeException)
					{
						MessageBox.Show(fileTooLargeException.Message);
					}
				}
			}
			else
			{
				MessageBox.Show("Please select a valid audio file");
			}
		}

		private void btnDesOutputWAVExtract_Click(object sender, RoutedEventArgs e)
		{
			desTargetPathWAVExtract = txtDesWAVExtract.Text;
			if (!desTargetPathWAVExtract.Equals(""))
			{
				SaveFileDialog save = new SaveFileDialog();
				save.Filter = "All files (*.*)|*.*";

				bool? result = save.ShowDialog();

				if (result == true)
				{

					byte[] extracted = SteganoWav.Extract(desTargetPathWAVExtract);
					String extension= SteganoWav.Extention;

					string savePath = "extractedMessage." + extension;
					File.WriteAllBytes(savePath, extracted);

					java.lang.Class clazz = typeof(Encryption);
					java.lang.Thread.currentThread().setContextClassLoader(clazz.getClassLoader());

					String outputFile = save.FileName;
					try
					{
						if (Encryption.encrypt("extractedMessage.des", outputFile, keyFile, EncryptionMode.DECRYPT) == java.lang.Boolean.TRUE)
						{
							File.Delete("extractedMessage.des");
							MessageBox.Show("File successfully decrypted from audio file");
						}

					}
					catch (java.io.IOException ex)
					{
						String error = ex.getMessage();
						MessageBox.Show(error);
					}
				}
			}
			else
			{
				MessageBox.Show("Please select a valid audio file");
			}
		}
	}
}

