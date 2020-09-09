using FileSpy.Classes;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace FileSpy.Windows
{
    /// <summary>
    /// Логика взаимодействия для GettingWindow.xaml
    /// </summary>
    public partial class GettingWindow : Window
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string FileName { get; set; }

        public long FileSize { get; set; }
        public double SmallSize { get; set; }
        public string PostSize { get; set; } = "B";
        public long NowSize { get; set; }
        DateTime LastTime;
        long LastNow;

        public delegate void CloseHandler(GettingWindow window);
        public event CloseHandler CloseEvent;

        SettingsClass Settings;
        ConnectionClass Connection;

        public GettingWindow(ConnectionClass connection, SettingsClass settings)
        {
            InitializeComponent();
            Connection = connection;
            Settings = settings;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NameLabel.Content = FileName;
            LoadingBar.Maximum = FileSize;

            SmallSize = SetSmallSize(FileSize);
            BytesLabel.Content = "0 / " + SmallSize.ToString("0.00") + PostSize;

            Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.FileOK, ID));
            LastTime = DateTime.Now;
            LastNow = 0;

            Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    var opacityAnim = new DoubleAnimation();
                    opacityAnim.From = 0;
                    opacityAnim.To = 1;
                    opacityAnim.Duration = TimeSpan.FromSeconds(2);
                    opacityAnim.RepeatBehavior = RepeatBehavior.Forever;
                    Circle1.BeginAnimation(System.Windows.Shapes.Ellipse.OpacityProperty, opacityAnim);

                    var circleAnim = new DoubleAnimation();
                    circleAnim.From = 80;
                    circleAnim.To = 20;
                    circleAnim.Duration = TimeSpan.FromSeconds(2);
                    circleAnim.RepeatBehavior = RepeatBehavior.Forever;
                    Circle1.BeginAnimation(System.Windows.Shapes.Ellipse.WidthProperty, circleAnim);
                    Circle1.BeginAnimation(System.Windows.Shapes.Ellipse.HeightProperty, circleAnim);
                });

                Thread.Sleep(670);

                Dispatcher.Invoke(() =>
                {
                    var opacityAnim = new DoubleAnimation();
                    opacityAnim.From = 0;
                    opacityAnim.To = 1;
                    opacityAnim.Duration = TimeSpan.FromSeconds(2);
                    opacityAnim.RepeatBehavior = RepeatBehavior.Forever;
                    Circle2.BeginAnimation(System.Windows.Shapes.Ellipse.OpacityProperty, opacityAnim);

                    var circleAnim = new DoubleAnimation();
                    circleAnim.From = 80;
                    circleAnim.To = 20;
                    circleAnim.Duration = TimeSpan.FromSeconds(2);
                    circleAnim.RepeatBehavior = RepeatBehavior.Forever;
                    Circle2.BeginAnimation(System.Windows.Shapes.Ellipse.WidthProperty, circleAnim);
                    Circle2.BeginAnimation(System.Windows.Shapes.Ellipse.HeightProperty, circleAnim);
                });

                Thread.Sleep(670);

                Dispatcher.Invoke(() =>
                {
                    var opacityAnim = new DoubleAnimation();
                    opacityAnim.From = 0;
                    opacityAnim.To = 1;
                    opacityAnim.Duration = TimeSpan.FromSeconds(2);
                    opacityAnim.RepeatBehavior = RepeatBehavior.Forever;
                    Circle3.BeginAnimation(System.Windows.Shapes.Ellipse.OpacityProperty, opacityAnim);

                    var circleAnim = new DoubleAnimation();
                    circleAnim.From = 80;
                    circleAnim.To = 20;
                    circleAnim.Duration = TimeSpan.FromSeconds(2);
                    circleAnim.RepeatBehavior = RepeatBehavior.Forever;
                    Circle3.BeginAnimation(System.Windows.Shapes.Ellipse.WidthProperty, circleAnim);
                    Circle3.BeginAnimation(System.Windows.Shapes.Ellipse.HeightProperty, circleAnim);
                });

                /*
                while (true)
                {
                    try
                    {
                        Dispatcher.Invoke(() => Circle1.Width -= 1);
                        Dispatcher.Invoke(() => Circle1.Height -= 1);
                        Dispatcher.Invoke(() => Circle1.Opacity += 0.017);
                        if (Dispatcher.Invoke(() => Circle1.Opacity >= 1))
                        {
                            Dispatcher.Invoke(() => Circle1.Width = 79);
                            Dispatcher.Invoke(() => Circle1.Height = 79);
                            Dispatcher.Invoke(() => Circle1.Opacity = 0);
                        }
                        Dispatcher.Invoke(() => Circle2.Width -= 1);
                        Dispatcher.Invoke(() => Circle2.Height -= 1);
                        Dispatcher.Invoke(() => Circle2.Opacity += 0.017);
                        if (Dispatcher.Invoke(() => Circle2.Opacity >= 1))
                        {
                            Dispatcher.Invoke(() => Circle2.Width = 79);
                            Dispatcher.Invoke(() => Circle2.Height = 79);
                            Dispatcher.Invoke(() => Circle2.Opacity = 0);
                        }
                        Dispatcher.Invoke(() => Circle3.Width -= 1);
                        Dispatcher.Invoke(() => Circle3.Height -= 1);
                        Dispatcher.Invoke(() => Circle3.Opacity += 0.017);
                        if (Dispatcher.Invoke(() => Circle3.Opacity >= 1))
                        {
                            Dispatcher.Invoke(() => Circle3.Width = 79);
                            Dispatcher.Invoke(() => Circle3.Height = 79);
                            Dispatcher.Invoke(() => Circle3.Opacity = 0);
                        }

                        Thread.Sleep(36);
                    }
                    catch
                    {
                        break;
                    }
                }*/
            });
        }

        public void SetData(byte[] data)
        {
            while (true)
            {
                try
                {
                    using (var fs = new FileStream(Settings.PathToSave + FileName, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        fs.Seek(0, SeekOrigin.End);
                        fs.Write(data, 0, data.Length);
                    }

                    Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.FileOK, ID));

                    if ((DateTime.Now - LastTime).TotalSeconds >= 1)
                    {
                        SpeedLabel.Content = (LastNow / (DateTime.Now - LastTime).TotalSeconds / 1024 / 1024).ToString("0.00") + "MB/s";
                        LastTime = DateTime.Now;
                        LastNow = 0;
                    }
                    LoadingBar.Value += data.Length;
                    NowSize += data.Length;
                    LastNow += data.Length;
                    BytesLabel.Content = GetSmallSize(NowSize).ToString("0.00") + " / " + SmallSize.ToString("0.00") + PostSize;
                    break;
                }
                catch { }
            }
        }

        public void Done()
        {
            Dispatcher.Invoke(() => SpeedLabel.Content = "Done!");
        }

        private double GetSmallSize(long size)
        {
            float sdata = size;

            switch (PostSize)
            {
                case "KB":
                    sdata /= 1024;
                    break;
                case "MB":
                    sdata /= 1024;
                    sdata /= 1024;
                    break;
                case "GB":
                    sdata /= 1024;
                    sdata /= 1024;
                    sdata /= 1024;
                    break;
            };

            return sdata;
        }

        private double SetSmallSize(long size)
        {
            int Type = 0;
            double sSize = size;
            while (sSize > 1024)
            {
                Type++;
                sSize /= 1024;
            }

            switch (Type)
            {
                case 1:
                    PostSize = "KB";
                    break;
                case 2:
                    PostSize = "MB";
                    break;
                case 3:
                    PostSize = "GB";
                    break;
            };

            return sSize;
        }

        private void Topper_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseButton_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Close();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            GC.Collect();
            CloseEvent(this);
        }
    }
}
