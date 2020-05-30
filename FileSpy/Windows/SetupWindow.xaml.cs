using FileSpy.Classes;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace FileSpy.Windows
{
    /// <summary>
    /// Логика взаимодействия для SetupWindow.xaml
    /// </summary>
    public partial class SetupWindow : Window
    {
        public int ID { get; set; }
        public int UserID { get; set; }

        public delegate void CloseHandler(SetupWindow window);
        public event CloseHandler CloseEvent;

        string FilePath;
        long FileSize;
        double SmallSize;
        string PostSize = "B";
        long LastNow;
        DateTime LastTime;

        ConnectionClass Connection;
        bool Sending;
        long Sended;

        public SetupWindow(int id, int userID, string userName, ConnectionClass connection)
        {
            InitializeComponent();
            ID = id;
            UserID = userID;
            Connection = connection;
            NameLabel.Content = userName;
        }

        public void Accept()
        {
            Dispatcher.Invoke(() =>
            {
                Circle1.Fill = Brushes.Blue;
                Circle2.Fill = Brushes.Blue;
                Circle3.Fill = Brushes.Blue;
                Circle4.Fill = Brushes.Blue;
                BytesLabel.Opacity = 1;
                LoadingBar.Opacity = 1;
                SpeedLabel.Opacity = 1;
                StatusLabel.Opacity = 0;
            });

            Sending = true;
            Sended = 0;

            FileInfo file = new FileInfo(FilePath);
            Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.FileInfo, ID, file.Name + ";" + file.Length.ToString()));
            FileSize = file.Length;
            SmallSize = SetSmallSize(file.Length);

            LoadingBar.Maximum = file.Length;

            BytesLabel.Content = "0 / " + SmallSize.ToString("0.00") + PostSize;
            LastTime = DateTime.Now;
        }

        public void Cancel()
        {
            Sending = false;
            Dispatcher.Invoke(() =>
            {
                BytesLabel.Opacity = 0;
                LoadingBar.Opacity = 0;
                SpeedLabel.Opacity = 0;
                StatusLabel.Content = "Sending canceled :<";
                StatusLabel.Opacity = 1;
                Circle1.Fill = Brushes.OrangeRed;
                Circle2.Fill = Brushes.OrangeRed;
                Circle3.Fill = Brushes.OrangeRed;
                Circle4.Fill = Brushes.OrangeRed;
            });
        }

        public void OK()
        {
            if (Sending)
            {
                Task.Run(() =>
                {
                    if (FileSize - Sended >= 50000)
                    {
                        byte[] data = new byte[50000];
                        try
                        {
                            using (var fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
                            {
                                fs.Seek(Sended, SeekOrigin.Begin);
                                fs.Read(data, 0, data.Length);
                            }
                            Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.FileData, ID, data));
                            Sended += 50000;
                            LastNow += 50000;
                            Dispatcher.Invoke(() => LoadingBar.Value += 50000);
                            Dispatcher.Invoke(() => BytesLabel.Content = GetSmallSize(Sended).ToString("0.00") + " / " + SmallSize.ToString("0.00") + PostSize);
                            if ((DateTime.Now - LastTime).TotalSeconds >= 1)
                            {
                                Dispatcher.Invoke(() => SpeedLabel.Content = (LastNow / (DateTime.Now - LastTime).TotalSeconds / 1024 / 1024).ToString("0.00") + "MB/s");
                                LastTime = DateTime.Now;
                                LastNow = 0;
                            }
                        }
                        catch { }
                    }
                    else
                    {
                        byte[] data = new byte[FileSize - Sended];
                        try
                        {
                            using (var fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
                            {
                                fs.Seek(Sended, SeekOrigin.Begin);
                                fs.Read(data, 0, data.Length);
                            }
                            Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.FileData, ID, data));
                            Sending = false;
                            LastNow += FileSize - Sended;
                            Dispatcher.Invoke(() => LoadingBar.Value += LastNow);
                            Dispatcher.Invoke(() => BytesLabel.Content = GetSmallSize(FileSize).ToString("0.00") + " / " + SmallSize.ToString("0.00") + PostSize);
                            Dispatcher.Invoke(() => SpeedLabel.Content = (LastNow / (DateTime.Now - LastTime).TotalSeconds / 1024 / 1024).ToString("0.00") + "MB/s");
                        }
                        catch { }
                    }
                });
            }
            else
            {
                GC.Collect();
            }
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

        public double SetSmallSize(long size)
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

        private void Topper_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            string[] data = e.Data.GetData(DataFormats.FileDrop) as string[];
            FilePath = data[0];
            Task.Run(() => SmallString(FilePath));
        }

        private void SmallString(string source)
        {
            string[] parts = source.Split('\\');

            while (MeasureString(parts) > 260)
            {
                int dir = 0;
                int id = parts.Length / 2;
                while (true)
                {
                    if (parts[id] == ";")
                    {
                        if (dir == 0)
                        {
                            if (id + 1 != parts.Length - 1)
                            {
                                id++;
                                if (parts[id] != ";")
                                {
                                    parts[id] = ";";
                                    break;
                                }
                            }
                            else
                                dir++;
                        }

                        if (dir == 1)
                        {
                            if (id - 1 != 0)
                            {
                                id--;
                                if (parts[id] != ";")
                                {
                                    parts[id] = ";";
                                    break;
                                }
                            }
                            else
                                dir++;
                        }

                        if (dir == 2)
                            break;
                    }
                    else
                    {
                        parts[id] = ";";
                        break;
                    }
                }

                if (dir == 2)
                    break;
            }

            string res = "";
            bool are = false;
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == ";")
                {
                    if (!are)
                    {
                        are = true;
                        res += "...\\";
                    }
                }
                else
                {
                    res += parts[i];
                    if (i != parts.Length - 1)
                        res += "\\";
                }
            }

            Dispatcher.Invoke(() => PathLabel.Content = res);
        }

        private double MeasureString(string[] candidate)
        {
            bool are = false;
            string res = "";
            for (int i = 0; i < candidate.Length; i++)
            {
                if (candidate[i] == ";")
                {
                    if (!are)
                    {
                        are = true;
                        res += "...\\";
                    }
                }
                else
                {
                    res += candidate[i];
                    if (i != candidate.Length - 1)
                        res += "\\";
                }
            }

            FormattedText formattedText = null;

            Dispatcher.Invoke(() => formattedText = new FormattedText(
                res,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(this.PathLabel.FontFamily, this.PathLabel.FontStyle, this.PathLabel.FontWeight, this.PathLabel.FontStretch),
                this.PathLabel.FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                1));

            return formattedText.Width;
        }

        private void CancelButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CloseEvent(this);
            Close();
        }

        private void SendButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            FileInfo file = null;
            if (File.Exists(FilePath))
                file = new FileInfo(FilePath);

            if (file != null && file.Length > 0)
            {
                Task.Run(() =>
                {
                    Dispatcher.Invoke(() => SetupPage.IsEnabled = false);
                    Dispatcher.Invoke(() => SenderPage.IsEnabled = true);
                    Dispatcher.Invoke(() => CloseButton.IsEnabled = true);
                    for (int i = 0; i < 10; i++)
                    {
                        Dispatcher.Invoke(() => SetupPage.Opacity -= 0.1);
                        Dispatcher.Invoke(() => CloseButton.Opacity += 0.1);
                        Dispatcher.Invoke(() => MainBorder.Margin = new Thickness(MainBorder.Margin.Left, MainBorder.Margin.Top, MainBorder.Margin.Right, MainBorder.Margin.Bottom + 12.5));
                        Dispatcher.Invoke(() => MainGrid.Margin = new Thickness(MainGrid.Margin.Left, MainGrid.Margin.Top + 12.5, MainGrid.Margin.Right, MainGrid.Margin.Bottom));
                        Thread.Sleep(17);
                    }

                    for (int i = 0; i < 10; i++)
                    {
                        Dispatcher.Invoke(() => SenderPage.Opacity += 0.1);
                        Thread.Sleep(17);
                    }

                    Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.RFileSend, ID, Path.GetFileName(FilePath)));

                    while (true)
                    {
                        try
                        {
                            Dispatcher.Invoke(() => Circle1.Width += 1);
                            Dispatcher.Invoke(() => Circle1.Height += 1);
                            Dispatcher.Invoke(() => Circle1.Opacity -= 0.017);
                            if (Dispatcher.Invoke(() => Circle1.Opacity <= 0))
                            {
                                Dispatcher.Invoke(() => Circle1.Width = 20);
                                Dispatcher.Invoke(() => Circle1.Height = 20);
                                Dispatcher.Invoke(() => Circle1.Opacity = 1);
                            }
                            Dispatcher.Invoke(() => Circle2.Width += 1);
                            Dispatcher.Invoke(() => Circle2.Height += 1);
                            Dispatcher.Invoke(() => Circle2.Opacity -= 0.017);
                            if (Dispatcher.Invoke(() => Circle2.Opacity <= 0))
                            {
                                Dispatcher.Invoke(() => Circle2.Width = 20);
                                Dispatcher.Invoke(() => Circle2.Height = 20);
                                Dispatcher.Invoke(() => Circle2.Opacity = 1);
                            }
                            Dispatcher.Invoke(() => Circle3.Width += 1);
                            Dispatcher.Invoke(() => Circle3.Height += 1);
                            Dispatcher.Invoke(() => Circle3.Opacity -= 0.017);
                            if (Dispatcher.Invoke(() => Circle3.Opacity <= 0))
                            {
                                Dispatcher.Invoke(() => Circle3.Width = 20);
                                Dispatcher.Invoke(() => Circle3.Height = 20);
                                Dispatcher.Invoke(() => Circle3.Opacity = 1);
                            }

                            Thread.Sleep(36);
                        }
                        catch
                        {
                            break;
                        }
                    }
                }
                );
            }
            else
            {
                Task.Run(() =>
                {
                    for (int i = 0; i < 10; i++)
                    {
                        Dispatcher.Invoke(() => PathLabel.Opacity -= 0.1);
                        Thread.Sleep(17);
                    }
                    for (int i = 0; i < 10; i++)
                    {
                        Dispatcher.Invoke(() => ErrorLabel.Opacity += 0.1);
                        Thread.Sleep(17);
                    }
                    Thread.Sleep(500);
                    for (int i = 0; i < 10; i++)
                    {
                        Dispatcher.Invoke(() => ErrorLabel.Opacity -= 0.1);
                        Thread.Sleep(17);
                    }
                    for (int i = 0; i < 10; i++)
                    {
                        Dispatcher.Invoke(() => PathLabel.Opacity += 0.1);
                        Thread.Sleep(17);
                    }
                }
                );
            }
        }

        private void CloseButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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
