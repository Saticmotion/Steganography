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

namespace gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Stream _messageStream;
        private Stream _carrierStream;
        private String _path;
        

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
                _messageStream = dialog.OpenFile();
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
                _carrierStream = dialog.OpenFile();
                TxtFileCarrier.Text = dialog.SafeFileName;
                _path = dialog.FileName;

                AudioPlayer.InitializeMedia(_path);

            }
        }
    }
}
