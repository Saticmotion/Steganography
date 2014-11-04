using System.IO;
using System.Media;
using System.Security.Principal;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Timers;
using SteganoLib;

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
        

        public MainWindow()
        {
            InitializeComponent();
			MakeAudioHidden();
        }
        private void BtnOpenFile_Click(object sender, RoutedEventArgs e)
        {

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            Nullable<bool> result = dialog.ShowDialog();
            if (result == true)
            {
                _messageString = dialog.FileName;
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
                _carrierString = dialog.FileName;
                TxtFileCarrier.Text = dialog.FileName;

	            LblOriginalAudio.Content = dialog.SafeFileName;
                AudioPlayer.InitializeMedia(_carrierString);

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
		    AudioPlayer.Visibility = Visibility.Visible;
		    AudioPlayer2.Visibility = Visibility.Visible;
		    AudioRectangle.Visibility = Visibility.Visible;

	    }

	    private void MakeAudioHidden()
	    {
		    LblEncryptedAudio.Visibility = Visibility.Hidden;
		    LblOriginalAudio.Visibility = Visibility.Hidden;
		    AudioPlayer.Visibility = Visibility.Hidden;
			AudioPlayer2.Visibility = Visibility.Hidden;
			AudioRectangle.Visibility = Visibility.Hidden;
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
				MessageBox.Show(_outputMessageFilepath);
			}
		}

		private void BtnOpenMessageFile_Click(object sender, RoutedEventArgs e)
		{
			System.Diagnostics.Process.Start(_outputMessageFilepath);
		}

		
    }
}
