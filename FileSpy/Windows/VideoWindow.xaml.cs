using FileSpy.Classes;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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

        bool Working;

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
            Working = true;

            Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.RVideoModule, ID));
            Task.Run(Pulsar);
        }

        public void SetVideoData(byte[] data)
        {
            ImageTable.Source = ConvertBM(data);
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

        private void Window_Closed(object sender, System.EventArgs e)
        {
            Working = false;
            CloseEvent(this);
        }

        private void Pulsar()
        {
            while (Working)
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
    }
}
