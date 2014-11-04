using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace gui
{
    /// <summary>
    /// Interaction logic for UCaudioPlayer.xaml
    /// </summary>
    public partial class UCaudioPlayer : UserControl
    {
        private bool _isplaying;
        private MediaPlayer mediaPlayer;
        private DispatcherTimer timer;
        private MediaTimeline _mTimeline;
        private bool _isdragging;
	    private int _audioTotalMinutes;
	    private int _audioTotalSeconds;
	    private int _audioCurrentMinutes;
	    private int _audioCurrentSeconds;
        public UCaudioPlayer()
        {
            InitializeComponent();
            _isplaying = false;
            _isdragging = false;
            PlayImageBtn.Source = (ImageSource)FindResource("PlayImage");
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(200);
            timer.Tick += timer_Tick;

        }

        public void InitializeMedia(string path)
        {
            mediaPlayer = new MediaPlayer();
            _mTimeline = new MediaTimeline(new Uri(path));
            _mTimeline.CurrentTimeInvalidated += MTimeline_CurrentTimeInvalidated;
            mediaPlayer.Clock = _mTimeline.CreateClock(true) as MediaClock;
            mediaPlayer.Clock.Controller.Stop();


            mediaPlayer.MediaOpened += media_MediaOpened;
        }

        private void MTimeline_CurrentTimeInvalidated(object sender, EventArgs e)
        {
            //
        }

        private void media_MediaOpened(object sender, EventArgs e)
        {
	        AudioSlider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
	        _audioTotalMinutes= (int) AudioSlider.Maximum/1000/60;
	        _audioTotalSeconds = (int)AudioSlider.Maximum/1000%60;
			CurrentAudioTime((int)AudioSlider.Value);
		}

	    private void CurrentAudioTime(int value)
	    {
			_audioCurrentMinutes = value / 1000 / 60;
			_audioCurrentSeconds = value / 1000 % 60;
			TimeLabel.Content = _audioCurrentMinutes + ":" + _audioCurrentSeconds + "/" + _audioTotalMinutes + ":" + _audioTotalSeconds;
	    }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (!_isdragging)
            {
	            AudioSlider.Value = mediaPlayer.Clock.CurrentTime.Value.TotalMilliseconds;
				CurrentAudioTime((int)AudioSlider.Value);
			}
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isplaying)
            {
                if (mediaPlayer.Clock.CurrentGlobalSpeed != 0.0)
                {
                    mediaPlayer.Clock.Controller.Pause();
                    timer.Stop();
                    PlayImageBtn.Source = (ImageSource)FindResource("PlayImage");

                }
                else
                {
                    mediaPlayer.Clock.Controller.Resume();
                    PlayImageBtn.Source = (ImageSource)FindResource("PauseImage");
                    timer.Start();
                }    
            }
            else
            {
				mediaPlayer.Clock.Controller.Begin();
				mediaPlayer.Clock.Controller.Seek(new TimeSpan(0, 0, 0, 0, (int)AudioSlider.Value), 0);
				mediaPlayer.Clock.Controller.Resume();
                
                timer.Start();
                _isplaying = true;
                PlayImageBtn.Source = (ImageSource)FindResource("PauseImage");
            }
            
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
			mediaPlayer.Clock.Controller.Pause();
            mediaPlayer.Clock.Controller.Stop();
			
            timer.Stop();
            AudioSlider.Value = 0;
			CurrentAudioTime((int)AudioSlider.Value);
			mediaPlayer.Clock.Controller.Seek(new TimeSpan(0, 0, 0, 0, (int)AudioSlider.Value), 0);
            _isplaying = false;
            PlayImageBtn.Source = (ImageSource)FindResource("PlayImage");
        }
        private void AudioSlider_OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            _isdragging = false;
            
            mediaPlayer.Clock.Controller.Seek(new TimeSpan(0, 0, 0, 0, (int)AudioSlider.Value), 0);
        }
        private void AudioSlider_DragStarted(object sender, DragStartedEventArgs e)
        {
            _isdragging = true;
        }
    }
}
