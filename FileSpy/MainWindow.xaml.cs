using FileSpy.Classes;
using FileSpy.Elements;
using FileSpy.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace FileSpy
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<UserControll> Users;
        List<SetupWindow> Setupers;
        List<GettingWindow> Getters;
        List<VideoWindow> VideoWindows;
        List<VideoClass> VideoClasses;

        ConnectionClass Connection;

        SettingsClass Settings;

        string Version = "[0.1.1.1]";
        string Status = "Simple";

        #region Imports
        [DllImport("Kernel32.dll")]
        static extern long GetTickCount64();
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hwnd, WinStyle style);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(string lpClassName, string lpWindowName);

        enum WinStyle
        {
            Hide = 0,
            ShowNormal = 1,
            ShowMinimized = 2,
            ShowMaximized = 3,
            Show = 5
        }
        #endregion

        public MainWindow()
        {
            //WindowBlur.SetIsEnabled(this, true);
            InitializeComponent();
            Settings = SettingsClass.Create();
            Users = new List<UserControll>();
            Setupers = new List<SetupWindow>();
            Getters = new List<GettingWindow>();
            VideoWindows = new List<VideoWindow>();
            VideoClasses = new List<VideoClass>();

            if (Environment.GetCommandLineArgs().Length == 1)
                Close();

            Connection = new ConnectionClass(Settings);
            Connection.AcceptMessage += Connection_AcceptMessage;
        }

        private void Topper_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void CloseButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Task.Run(() => OpenAnim(false));
            this.Hide();
            NotifyIcon icon = new NotifyIcon();
            using (var iconStream = System.Windows.Application.GetResourceStream(new Uri("Resources/magnet.ico", UriKind.Relative)).Stream)
            {
                icon.Icon = new System.Drawing.Icon(iconStream);
                icon.Visible = true;
                icon.DoubleClick +=
                    delegate (object sender1, EventArgs args)
                    {
                        this.Show();
                        icon.Visible = false;
                    };
            }
        }

        private void MinimazeButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void SettingsButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SettingsWindow window = new SettingsWindow(Settings);
            window.Owner = this;
            window.ShowDialog();
            if (window.Save)
                Connection.SendMessage(new MessageClass(Connection.ID, -1, Commands.ChangeName, 0, Settings.UserName));
        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (e.Key == Key.K)
                {
                    var window = new TextWindow();
                    window.Owner = this;
                    window.ShowDialog();
                    Settings.SetKeyWord(window.PassBox.Password);
                    Connection.SendMessage(new MessageClass(Connection.ID, -1, Commands.KeyPass, 0, window.PassBox.Password));
                }

                if (e.Key == Key.T)
                {
                    var window = new StatusWindow(Version, Status);
                    window.Owner = this;
                    window.Show();
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() => OpenAnim(true));
            Connection.Start();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
        }

        private void LostConnection()
        {
            while (Dispatcher.Invoke(() => GreenColor.Offset < 1.4) && !Connection.Connected)
            {
                Dispatcher.Invoke(() => GreenColor.Offset += 0.1);
                Thread.Sleep(50);
            }

            while (Dispatcher.Invoke(() => RedColor.Offset > 0.7) && !Connection.Connected)
            {
                Dispatcher.Invoke(() => RedColor.Offset -= 0.1);
                Thread.Sleep(50);
            }
        }

        private void FindConnection()
        {
            while (Dispatcher.Invoke(() => RedColor.Offset < 1.4) && Connection.Connected)
            {
                Dispatcher.Invoke(() => RedColor.Offset += 0.1);
                Thread.Sleep(50);
            }

            while (Dispatcher.Invoke(() => GreenColor.Offset > 0.7) && Connection.Connected)
            {
                Dispatcher.Invoke(() => GreenColor.Offset -= 0.1);
                Thread.Sleep(50);
            }
        }

        private void OpenAnim(bool loaded)
        {
            float delay;
            if (loaded)
            {
                delay = 5;
                for (int i = 0; i < 20; i++)
                {
                    Dispatcher.Invoke(() => Grid.Opacity += 0.05);
                    Dispatcher.Invoke(() => Grid.Margin = new Thickness(Grid.Margin.Left - 12.25, Grid.Margin.Top, Grid.Margin.Right - 12.25, Grid.Margin.Bottom));
                    delay += 0.5f;
                    Thread.Sleep(Convert.ToInt32(delay));
                }
            }
            else
            {
                delay = 15;
                for (int i = 0; i < 20; i++)
                {
                    Dispatcher.Invoke(() => Grid.Opacity -= 0.05);
                    Dispatcher.Invoke(() => Grid.Margin = new Thickness(Grid.Margin.Left + 12.25, Grid.Margin.Top, Grid.Margin.Right + 12.25, Grid.Margin.Bottom));
                    delay -= 0.5f;
                    Thread.Sleep(Convert.ToInt32(delay));
                }
            }
        }

        private void Connection_AcceptMessage(MessageClass message)
        {
            #region StandartCommands
            if (message.Command == Commands.Disconnect)
            {
                Task.Run(LostConnection);
                Dispatcher.Invoke(() =>
                {
                    int count = Users.Count;
                    for (int i = 0; i < count; i++)
                    {
                        FullTable.Items.Remove(Users[0]);
                        Users.Remove(Users[0]);
                    }

                    DisconnectLabel.Opacity = 1;
                });
            }

            if (message.Command == Commands.AcceptLogin)
            {
                Task.Run(FindConnection);
                Dispatcher.Invoke(() => DisconnectLabel.Opacity = 0);
                Connection.ID = message.Getter;
                Connection.SendMessage(new MessageClass(Connection.ID, -1, Commands.RsaKey, 0, Connection.Secure.GetPublicKey()));
            }

            if (message.Command == Commands.AesKey)
            {
                Connection.Secure.SetAesKey(Connection.Secure.RsaDecrypt(message.Package));
                Connection.Secured = true;
                Connection.SendMessage(new MessageClass(Connection.ID, -1, Commands.GetList, 0));
                Connection.SendMessage(new MessageClass(Connection.ID, -1, Commands.KeyPass, 0, Settings.KeyWord));
            }

            if (message.Command == Commands.List)
            {
                try
                {
                    string[] com = message.GetStringPackage().Split(';');

                    Dispatcher.Invoke(() =>
                    {
                        int count = Users.Count;
                        for (int i = 0; i < count; i++)
                        {
                            FullTable.Items.Remove(Users[0]);
                            Users.Remove(Users[0]);
                        }

                        for (int i = 0; i < com.Length; i += 3)
                        {
                            var user = new UserControll(Convert.ToInt32(com[i]), com[i + 1], com[i + 2]);
                            user.ActiveEvent += User_ActiveEvent;
                            Users.Add(user);
                            FullTable.Items.Add(user);
                            if (Status != "Simple")
                                user.SetEnabled(1, true);
                        }
                    });
                }
                catch { }
            }

            if (message.Command == Commands.ChangeStatus)
            {
                Status = message.GetStringPackage();
                if (Status != "Simple")
                {
                    Dispatcher.Invoke(() =>
                    {
                        for (int i = 0; i < VideoClasses.Count; i++)
                            VideoClasses[i].Close();
                        var window = new StatusWindow(Version, Status);
                        window.Owner = this;
                        window.Show();
                    });
                    for (int i = 0; i < Users.Count; i++)
                        Dispatcher.Invoke(() => Users[i].SetEnabled(1, true));
                }
                else
                {
                    for (int i = 0; i < Users.Count; i++)
                        Dispatcher.Invoke(() => Users[i].SetEnabled(0, false));
                }
            }

            if (message.Command == Commands.ChangeName)
            {
                Dispatcher.Invoke(() =>
                {
                    for (int i = 0; i < Users.Count; i++)
                    {
                        if (message.Getter == Users[i].ID)
                            Users[i].NameLabel.Content = message.GetStringPackage();
                    }
                });
            }

            if (message.Command == Commands.GetInfo)
            {
                string result = Version + "\n";
                if (Status != "Simple")
                    result += "[Secret]\n";
                else
                    result += "Simple\n";
                result += TimeSpan.FromMilliseconds(GetTickCount64()).ToString();

                Connection.SendMessage(new MessageClass(Connection.ID, message.Sender, Commands.Info, 0, result));
            }

            if (message.Command == Commands.Info)
            {
                Dispatcher.Invoke(() => System.Windows.MessageBox.Show(message.GetStringPackage()));
            }
            #endregion

            #region FileCommands
            if (message.Command == Commands.RFileSend)
            {
                Task.Run(() =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        string name = "[Secret]";
                        for (int i = 0; i < Users.Count; i++)
                        {
                            if (message.Sender == Users[i].ID)
                                name = Users[i].NameLabel.Content as string;
                        }
                        var window = new RequestWindow(name, message.GetStringPackage());
                        window.ShowDialog();
                        if (window.Result)
                            Connection.SendMessage(new MessageClass(Connection.ID, message.Sender, Commands.AcceptFile, message.ElementID));
                        else
                            Connection.SendMessage(new MessageClass(Connection.ID, message.Sender, Commands.CancelFile, message.ElementID));
                    });
                });
            }

            if (message.Command == Commands.AcceptFile)
            {
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        var setuper = FindSetuper(message.ElementID);
                        setuper.Accept();
                    }
                    catch { }
                });
            }

            if (message.Command == Commands.CancelFile)
            {
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        var setuper = FindSetuper(message.ElementID);
                        setuper.Cancel();
                    }
                    catch { }
                });
            }

            if (message.Command == Commands.FileOK)
            {
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        var setuper = FindSetuper(message.ElementID);
                        setuper.OK();
                    }
                    catch { }
                });
            }

            if (message.Command == Commands.FileInfo)
            {
                Dispatcher.Invoke(() =>
                {
                    var getter = new GettingWindow(Connection, Settings);
                    getter.ID = message.ElementID;
                    getter.UserID = message.Sender;
                    string[] com = message.GetStringPackage().Split(';');
                    getter.FileName = com[0];
                    getter.FileSize = Convert.ToInt64(com[1]);
                    getter.CloseEvent += Getter_CloseEvent;
                    getter.Owner = this;
                    getter.Show();
                    Getters.Add(getter);
                });
            }

            if (message.Command == Commands.FileData)
            {
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        var getter = FindGetter(message.ElementID, message.Sender);
                        getter.SetData(message.Package);
                    }
                    catch { }
                });
            }

            #endregion

            #region VideoCommands
            if (message.Command == Commands.RVideoModule)
            {
                Task.Run(() =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        string name = "[Secret]";
                        for (int i = 0; i < Users.Count; i++)
                        {
                            if (message.Sender == Users[i].ID)
                                name = Users[i].NameLabel.Content as string;
                        }
                        var window = new RequestWindow(name, "<Video.mp4>");
                        window.ShowDialog();
                        if (window.Result)
                        {
                            var video = new VideoClass(message.ElementID, message.Sender, Connection);
                            video.CloseEvent += VideoSender_CloseEvent;
                            video.Start();
                            VideoClasses.Add(video);
                        }
                        else
                            Connection.SendMessage(new MessageClass(Connection.ID, message.Sender, Commands.VideoDenied, message.ElementID));
                    });
                });
            }

            if (message.Command == Commands.HVideoModule)
            {
                var video = new VideoClass(message.ElementID, message.Sender, Connection);
                video.CloseEvent += VideoSender_CloseEvent;
                video.Start();
                VideoClasses.Add(video);
            }

            if (message.Command == Commands.VideoDenied)
            {
                try
                {
                    Dispatcher.Invoke(FindVideoWindow(message.ElementID).Denied);
                }
                catch { }
            }

            if (message.Command == Commands.VideoPulsar)
            {
                try
                {
                    Dispatcher.Invoke(FindVideoClasses(message.ElementID, message.Sender).Pulsar);
                }
                catch { }
            }

            if (message.Command == Commands.VideoClose)
            {
                try
                {
                    Dispatcher.Invoke(FindVideoClasses(message.ElementID, message.Sender).Close);
                }
                catch { }
            }

            if (message.Command == Commands.VideoData)
            {
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        FindVideoWindow(message.ElementID).SetVideoData(message.Package);
                    }
                    catch
                    {
                        Connection.SendMessage(new MessageClass(Connection.ID, message.Sender, Commands.VideoClose, message.ElementID));
                    }
                });
            }

            if (message.Command == Commands.SetVideo)
            {
                try
                {
                    if (message.GetStringPackage() == "True")
                        Dispatcher.Invoke(() => FindVideoClasses(message.ElementID, message.Sender).VideoStream = true);
                    else
                        Dispatcher.Invoke(() => FindVideoClasses(message.ElementID, message.Sender).VideoStream = false);
                }
                catch { }
            }

            if (message.Command == Commands.SetMaxFps)
            {
                try
                {
                    Dispatcher.Invoke(() => FindVideoClasses(message.ElementID, message.Sender).MaxFps = Convert.ToInt32(message.GetStringPackage()));
                }
                catch { }
            }

            if (message.Command == Commands.SetSize)
            {
                try
                {
                    Dispatcher.Invoke(() => FindVideoClasses(message.ElementID, message.Sender).Size = Convert.ToInt32(message.GetStringPackage()));
                }
                catch { }
            }

            if (message.Command == Commands.SetQuality)
            {
                try
                {
                    Dispatcher.Invoke(() => FindVideoClasses(message.ElementID, message.Sender).Quality = Convert.ToInt32(message.GetStringPackage()));
                }
                catch { }
            }

            if (message.Command == Commands.SetMicro)
            {
                try
                {
                    if (message.GetStringPackage() == "True")
                        Dispatcher.Invoke(FindVideoClasses(message.ElementID, message.Sender).MicroInput.StartRecording);
                    else
                        Dispatcher.Invoke(FindVideoClasses(message.ElementID, message.Sender).MicroInput.StopRecording);
                }
                catch { }
            }

            if (message.Command == Commands.MicroData)
            {
                try
                {
                    if (message.Package != null)
                        Dispatcher.Invoke(() => FindVideoWindow(message.ElementID).MicroBuffer.AddSamples(message.Package, 0, message.Package.Length));
                }
                catch { }
            }

            if (message.Command == Commands.SetCursor)
            {
                try
                {
                    if (message.GetStringPackage() == "True")
                        Dispatcher.Invoke(() => FindVideoClasses(message.ElementID, message.Sender).Cursor = true);
                    else
                        Dispatcher.Invoke(() => FindVideoClasses(message.ElementID, message.Sender).Cursor = false);
                }
                catch { }
            }
            #endregion
        }

        private void User_ActiveEvent(int id, string name, int command)
        {
            if (command == ElementCommands.InfoModule)
            {
                Connection.SendMessage(new MessageClass(Connection.ID, id, Commands.GetInfo, 0));
            }

            if (command == ElementCommands.SendModule)
            {
                Random r = new Random();
                int gid = -1;
                bool ok = true;
                while (ok)
                {
                    ok = false;
                    gid = r.Next(1, Int32.MaxValue / 2);
                    for (int i = 0; i < Setupers.Count; i++)
                    {
                        if (gid == Setupers[i].ID)
                            ok = true;
                    }
                }

                var setup = new SetupWindow(gid, id, name, Connection);
                Setupers.Add(setup);
                setup.Owner = this;
                setup.CloseEvent += Setup_CloseEvent;
                setup.Show();
            }

            if (command == ElementCommands.VideoModule)
            {
                Random r = new Random();
                int gid = -1;
                bool ok = true;
                while (ok)
                {
                    ok = false;
                    gid = r.Next(1, Int32.MaxValue / 2);
                    for (int i = 0; i < VideoWindows.Count; i++)
                    {
                        if (gid == VideoWindows[i].ID)
                            ok = true;
                    }
                }

                var video = new VideoWindow(gid, id, name, Connection);
                VideoWindows.Add(video);
                video.CloseEvent += Video_CloseEvent;
                video.Owner = this;
                video.Show();
            }
        }

        private SetupWindow FindSetuper(int id)
        {
            for (int i = 0; i < Setupers.Count; i++)
            {
                if (Setupers[i].ID == id)
                    return Setupers[i];
            }

            return null;
        }

        private GettingWindow FindGetter(int id, int userID)
        {
            for (int i = 0; i < Getters.Count; i++)
            {
                if (Getters[i].ID == id && Getters[i].UserID == userID)
                    return Getters[i];
            }

            return null;
        }

        private VideoWindow FindVideoWindow(int id)
        {
            for (int i = 0; i < VideoWindows.Count; i++)
            {
                if (VideoWindows[i].ID == id)
                    return VideoWindows[i];
            }

            return null;
        }

        private VideoClass FindVideoClasses(int id, int userID)
        {
            for (int i = 0; i < VideoClasses.Count; i++)
            {
                if (VideoClasses[i].ID == id && VideoClasses[i].UserID == userID)
                    return VideoClasses[i];
            }

            return null;
        }

        private void Setup_CloseEvent(SetupWindow window)
        {
            Setupers.Remove(window);
            GC.Collect();
        }

        private void Getter_CloseEvent(GettingWindow window)
        {
            Getters.Remove(window);
            GC.Collect();
        }

        private void Video_CloseEvent(VideoWindow window)
        {
            VideoWindows.Remove(window);
            GC.Collect();
        }

        private void VideoSender_CloseEvent(VideoClass videoClass)
        {
            VideoClasses.Remove(videoClass);
            GC.Collect();
        }
    }
}