using FileSpy.Classes;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
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
        bool Framed;

        WaveOut MicroOut;
        public BufferedWaveProvider MicroBuffer { get; set; }
        VolumeSampleProvider MicroVolume;

        public delegate void CloseHandler(VideoWindow window);
        public event CloseHandler CloseEvent; 

        ConnectionClass Connection;

        public VideoWindow(int id, int userId, string userName, ConnectionClass connection)
        {
            InitializeComponent();

            ID = id;
            UserID = userId;
            Title = userName;
            Connection = connection;

            MaxFps = 10;
            Framed = true;
            LastFrame = DateTime.Now;

            MicroOut = new WaveOut();
            MicroBuffer = new BufferedWaveProvider(new WaveFormat(8000, 16, 1));
            MicroVolume = new VolumeSampleProvider(MicroBuffer.ToSampleProvider());
            MicroOut.Init(MicroVolume);
            MicroOut.Play();

            Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.RVideoModule, ID));
            Task.Run(Pulsar);
            Task.Run(FpsCounter);
        }

        public void SetVideoData(byte[] data)
        {
            Task.Run(() =>
            {
                while ((DateTime.Now - LastFrame).TotalMilliseconds < 1000 / MaxFps)
                    Thread.Sleep(1);
                Dispatcher.Invoke(() =>
                {
                    StatusLabel.Opacity = 0;
                    FPSCount++;
                    LastFrame = DateTime.Now;
                    ImageTable.Source = ConvertBM(data);
                });
            });
        }

        public void Denied()
        {
            StatusLabel.Content = "Request denied :<";
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.GetPosition(this).Y >= ActualHeight - 150)
            {
                V.Opacity = 1;
            }
            else
            {
                V.Opacity = 0;
            }
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            V.Opacity = 0;
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            CloseEvent(this);
        }

        private void FpsCounter()
        {
            while (Dispatcher.Invoke(() => IsLoaded))
            {
                Dispatcher.Invoke(() => FPSLabel.Content = FPSCount.ToString() + "FPS");
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

        #region UIEvents
        private void SizeSlider_MouseEnter(object sender, MouseEventArgs e)
        {
            InfoPopup.PlacementTarget = sender as UIElement;
            InfoPopup.HorizontalOffset = 55;
            InfoPopup.IsOpen = true;
        }

        private void SizeSlider_MouseLeave(object sender, MouseEventArgs e)
        {
            InfoPopup.IsOpen = false;
        }

        private void SizeSlider_MouseMove(object sender, MouseEventArgs e)
        {
            switch (SizeSlider.Value)
            {
                case 0:
                    InfoLabel.Content = "360p";
                    break;
                case 1:
                    InfoLabel.Content = "480p";
                    break;
                case 2:
                    InfoLabel.Content = "720p";
                    break;
            }
            InfoPopup.IsOpen = false;
            InfoPopup.IsOpen = true;
        }

        private void QualitySlider_MouseMove(object sender, MouseEventArgs e)
        {
            InfoLabel.Content = QualitySlider.Value.ToString("0.0");
            InfoPopup.IsOpen = false;
            InfoPopup.IsOpen = true;
        }

        private void MicroSlider_MouseMove(object sender, MouseEventArgs e)
        {
            InfoLabel.Content = MicroSlider.Value.ToString("0.0");
            InfoPopup.IsOpen = false;
            InfoPopup.IsOpen = true;
        }

        private void AudioSlider_MouseMove(object sender, MouseEventArgs e)
        {
            InfoLabel.Content = AudioSlider.Value.ToString("0.0");
            InfoPopup.IsOpen = false;
            InfoPopup.IsOpen = true;
        }

        private void VideoCheck_Checked(object sender, RoutedEventArgs e)
        {
            if (Connection != null)
                Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.SetVideo, ID, VideoCheck.IsChecked.ToString()));
        }

        private void MaxFpsText_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    MaxFps = Convert.ToInt32(MaxFpsText.Text);
                    Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.SetMaxFps, ID, MaxFpsText.Text));
                }
                catch
                {
                    MaxFpsText.Text = "";
                }
            }
        }

        private void SizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Connection != null)
                Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.SetSize, ID, SizeSlider.Value.ToString()));
        }

        private void QualitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Connection != null)
                Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.SetQuality, ID, Convert.ToInt32(QualitySlider.Value).ToString()));
        }

        private void MicroCheck_Checked(object sender, RoutedEventArgs e)
        {
            if (Connection != null)
                Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.SetMicro, ID, MicroCheck.IsChecked.ToString()));
        }

        private void MicroSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MicroOut != null)
                MicroVolume.Volume = (float)MicroSlider.Value;
        }

        private void AudioCheck_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void AudioSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void FpsVisibleCheck_Checked(object sender, RoutedEventArgs e)
        {
            if (FPSLabel.Opacity == 0)
                FPSLabel.Opacity = 1;
            else
                FPSLabel.Opacity = 0;
        }

        private void SecurityCheck_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CursorCheck_Checked(object sender, RoutedEventArgs e)
        {
            if (Connection != null)
                Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.SetCursor, ID, CursorCheck.IsChecked.ToString()));
        }

        private void CursorCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            if (Connection != null)
                Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.SetCursor, ID, CursorCheck.IsChecked.ToString()));
        }
        #endregion
    }
}
