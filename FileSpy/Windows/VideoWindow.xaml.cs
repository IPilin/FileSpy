using FileSpy.Classes;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace FileSpy.Windows
{
    /// <summary>
    /// Логика взаимодействия для VideoWindow.xaml
    /// </summary>
    public partial class VideoWindow : Window
    {
        public int ID { get; set; }
        public int UserID { get; set; }

        int MaxFps;
        int FPSCount;
        DateTime LastFrame;
        UInt64 FrameID;

        WaveOut MicroOut;
        public BufferedWaveProvider MicroBuffer { get; set; }
        VolumeSampleProvider MicroVolume;

        WasapiOut LoopOut = new WasapiOut();
        public BufferedWaveProvider LoopBuffer { get; set; }
        VolumeSampleProvider LoopVolume;

        public delegate void CloseHandler(VideoWindow window);
        public event CloseHandler CloseEvent;

        ConnectionClass Connection;

        DateTime LastMove = DateTime.Now;
        DoubleAnimation DownAnimation = new DoubleAnimation();
        DoubleAnimation UpAnimation = new DoubleAnimation();

        public VideoWindow(int id, int userId, string userName, ConnectionClass connection)
        {
            InitializeComponent();

            ID = id;
            UserID = userId;
            Title = userName;
            Connection = connection;

            MaxFps = 10;
            LastFrame = DateTime.Now;
            FrameID = 0;

            MicroOut = new WaveOut();
            MicroBuffer = new BufferedWaveProvider(new WaveFormat(8000, 16, 1));
            MicroVolume = new VolumeSampleProvider(MicroBuffer.ToSampleProvider());
            MicroOut.Init(MicroVolume);
            MicroOut.Play();

            Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.RVideoModule, ID));
            Task.Run(Pulsar);
            Task.Run(FpsCounter);
            Task.Run(UITimer);
        }

        public void SetVideoData(byte[] buffer)
        {
            var data = new VideoData(buffer);
            if (data.ID < FrameID)
                return;
            else
                FrameID++;
            StatusLabel.Opacity = 0;
            FPSCount++;
            LastFrame = DateTime.Now;
            ImageTable.Source = ConvertBM(data.Data);
        }

        public void Denied()
        {
            StatusLabel.Content = "Request denied :<";
        }

        public void SetupLoop(byte[] buffer)
        {
            using (var ms = new MemoryStream(buffer))
            {
                using (var br = new BinaryReader(ms))
                {
                    WaveFormat format = new WaveFormat(br);
                    LoopBuffer = new BufferedWaveProvider(format);
                    LoopVolume = new VolumeSampleProvider(LoopBuffer.ToSampleProvider());
                    LoopOut.Init(LoopVolume);
                    LoopOut.Play();
                }
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            LastMove = DateTime.Now;
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            LastMove = DateTime.MinValue;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DownAnimation.From = 1;
            DownAnimation.To = 0;
            DownAnimation.Duration = TimeSpan.FromMilliseconds(500);

            UpAnimation.From = 0;
            UpAnimation.To = 1;
            UpAnimation.Duration = TimeSpan.FromMilliseconds(500);
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            CloseEvent(this);
        }

        private void UITimer()
        {
            while (Dispatcher.Invoke(() => IsLoaded))
            {
                if ((DateTime.Now - LastMove).TotalSeconds < 3)
                {
                    if (Dispatcher.Invoke(() => UIGrid.Opacity != 1))
                        Dispatcher.Invoke(() => UIGrid.Opacity = 1);
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        System.Windows.Point mouse = Mouse.GetPosition(this);
                        if (UIGrid.Opacity != 0 && mouse.Y < ActualHeight - 150)
                            UIGrid.Opacity = 0;
                    });
                }

                Thread.Sleep(100);
            }
        }

        private void FpsCounter()
        {
            while (Dispatcher.Invoke(() => IsLoaded))
            {
                Dispatcher.Invoke(() =>
                {
                    FPSLabel.Content = FPSCount.ToString() + "FPS";
                    if (FPSCount == 0)
                    {
                        WarningCircle.Fill = System.Windows.Media.Brushes.Red;
                        WarningCircle.Opacity = 1;
                    }
                    else if (FPSCount < Convert.ToInt32(MaxFpsBox.Text) / 3 * 2)
                    {
                        WarningCircle.Fill = System.Windows.Media.Brushes.YellowGreen;
                        WarningCircle.Opacity = 1;
                    }
                    else
                    {
                        WarningCircle.Opacity = 0;
                    }
                });
                FPSCount = 0;
                Thread.Sleep(1000);
            }
        }

        private void Pulsar()
        {
            while (Dispatcher.Invoke(() => IsLoaded))
            {
                Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.VideoPulsar, ID));
                Thread.Sleep(5000);
            }
        }

        private static BitmapImage ConvertBM(byte[] imageData)
        {
            var image = new BitmapImage();
            using (var mem = new MemoryStream(UnZipper(imageData)))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        private static byte[] UnZipper(byte[] data)
        {
            byte[] source = null;
            using (var fRead = new MemoryStream(data))
            using (var fWrite = new MemoryStream())
            using (var gzip = new GZipStream(fRead, CompressionMode.Decompress))
            {
                var buffer = new byte[4096];
                int readed = 0;

                while ((readed = gzip.Read(buffer, 0, buffer.Length)) != 0)
                    fWrite.Write(buffer, 0, readed);

                source = fWrite.ToArray();
            }

            return source;
        }

        private void MicroGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender == MicroButton)
            {
                if (MicroButton.Opacity == 1)
                    MicroPopup.IsOpen = true;
            }
            else
            {
                if (AudioButton.Opacity == 1)
                    AudioPopup.IsOpen = true;
            }
        }

        private void MicroGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender as Popup == MicroPopup)
                MicroPopup.IsOpen = false;
            else
                AudioPopup.IsOpen = false;
        }

        private void PauseButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (PauseButton.Opacity == 1)
            {
                PauseButton.BeginAnimation(Grid.OpacityProperty, DownAnimation);
                PlayButton.BeginAnimation(Grid.OpacityProperty, UpAnimation);
                Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.SetVideo, ID, false.ToString()));
            }
            else if (PauseButton.Opacity == 0)
            {
                PauseButton.BeginAnimation(Grid.OpacityProperty, UpAnimation);
                PlayButton.BeginAnimation(Grid.OpacityProperty, DownAnimation);
                Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.SetVideo, ID, true.ToString()));
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                PauseButton_MouseLeftButtonUp(null, null);
                LastMove = DateTime.Now;
            }

            if (e.Key == Key.D1)
            {
                MicroButton_MouseLeftButtonUp(null, null);
            }

            if (e.Key == Key.D2)
            {
                AudioButton_MouseLeftButtonUp(null, null);
            }
        }

        private void MicroButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (MicroButton.Opacity == 0.5)
            {
                MicroButton.Opacity = 1;
                Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.SetMicro, ID, true.ToString()));
            }
            else
            {
                MicroButton.Opacity = 0.5;
                Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.SetMicro, ID, false.ToString()));
            }
        }

        private void AudioButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (AudioButton.Opacity == 0.5)
            {
                AudioButton.Opacity = 1;
                Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.SetLoop, ID, true.ToString()));
            }
            else
            {
                AudioButton.Opacity = 0.5;
                Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.SetLoop, ID, false.ToString()));
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender == MicroSlider)
            {
                if (MicroVolume != null)
                    MicroVolume.Volume = (float)(sender as Slider).Value;
            }
            else
            {
                if (LoopVolume != null)
                    LoopVolume.Volume = (float)(sender as Slider).Value;
            }
        }

        private void MicroSlider_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender == MicroSlider)
            {
                MicroSlider.Value = 1;
                if (MicroVolume != null)
                    MicroVolume.Volume = 1;
            }
            else
            {
                AudioSlider.Value = 1;
                if (LoopVolume != null)
                    LoopVolume.Volume = 1;
            }
        }

        private void SizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Connection != null)
                Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.SetSize, ID, (2 - SizeComboBox.SelectedIndex).ToString()));
        }

        private void QComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Connection != null)
                switch(QComboBox.SelectedIndex)
                {
                    case 0:
                        Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.SetQuality, ID, "100"));
                        break;
                    case 1:
                        Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.SetQuality, ID, "75"));
                        break;
                    case 2:
                        Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.SetQuality, ID, "50"));
                        break;
                    case 3:
                        Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.SetQuality, ID, "25"));
                        break;
                }
        }

        private void FpsButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (FpsButton.Opacity == 0.5)
            {
                FPSLabel.Opacity = 1;
                FpsButton.Opacity = 1;
            }
            else
            {
                FPSLabel.Opacity = 0;
                FpsButton.Opacity = 0.5;
            }
        }

        private void CursorButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (CursorButton.Opacity == 0.5)
            {
                if (Connection != null)
                    Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.SetCursor, ID, true.ToString()));
                CursorButton.Opacity = 1;
            }
            else
            {
                if (Connection != null)
                    Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.SetCursor, ID, false.ToString()));
                CursorButton.Opacity = 0.5;
            }
        }

        private void MaxFpsBox_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (Connection != null)
                        Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.SetMaxFps, ID, MaxFpsBox.Text));
                }
                int a = Convert.ToInt32(MaxFpsBox.Text);
            }
            catch
            {
                MaxFpsBox.Text = "10";
            }
        }
    }
}
