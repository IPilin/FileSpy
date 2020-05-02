using FileSpy.Classes;
using FileSpy.Elements;
using FileSpy.Windows;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
        List<VideoWindow> Videos;

        ConnectionClass Connection;

        SettingsClass Settings;

        string Version = "[0.1.0.0]";
        string Status = "Simple";

        public MainWindow()
        {
            //WindowBlur.SetIsEnabled(this, true);
            InitializeComponent();
            Settings = SettingsClass.Create();
            Users = new List<UserControll>();
            Setupers = new List<SetupWindow>();
            Getters = new List<GettingWindow>();
            Videos = new List<VideoWindow>();

            Connection = new ConnectionClass(Settings);
            Connection.AcceptMessage += Connection_AcceptMessage;
        }

        private void Topper_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void CloseButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Task.Run(() => OpenAnim(false));
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

        private void Window_KeyUp(object sender, KeyEventArgs e)
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

        private void LostConnection()
        {
            for (int i = 0; i < 7; i++)
            {
                Dispatcher.Invoke(() => GreenColor.Offset += 0.1);
                Thread.Sleep(50);
            }

            for (int i = 0; i < 7; i++)
            {
                Dispatcher.Invoke(() => RedColor.Offset -= 0.1);
                Thread.Sleep(50);
            }
        }

        private void FindConnection()
        {
            for (int i = 0; i < 7; i++)
            {
                Dispatcher.Invoke(() => RedColor.Offset += 0.1);
                Thread.Sleep(50);
            }

            for (int i = 0; i < 7; i++)
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

                Dispatcher.Invoke(Close);
            }
        }

        private void Connection_AcceptMessage(MessageClass message)
        {
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

            if (message.Command == Commands.ChangeStatus)
            {
                Status = message.GetStringPackage();
                if (Status != "Simple")
                {
                    Dispatcher.Invoke(() =>
                    {
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

            if (message.Command == Commands.RFileSend)
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
        }

        private void User_ActiveEvent(int id, string name, int command)
        {
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
                    for (int i = 0; i < Videos.Count; i++)
                    {
                        if (gid == Videos[i].ID)
                            ok = true;
                    }
                }

                var video = new VideoWindow(gid, id, name, Connection);
                Videos.Add(video);
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
            Videos.Remove(window);
            GC.Collect();
        }
    }
}