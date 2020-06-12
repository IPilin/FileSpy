using FileSpy.Classes;
using FileSpy.Classes.FileModule;
using FileSpy.Elements;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FileSpy.Windows
{
    /// <summary>
    /// Логика взаимодействия для FileWindow.xaml
    /// </summary>
    public partial class FileWindow : Window
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        ConnectionClass Connection;
        bool Connected;

        public delegate void CloseHandler(FileWindow window);
        public event CloseHandler CloseEvent;

        LoadingWindow Loading;

        public FileWindow(int id, int userId, string name, ConnectionClass connection)
        {
            InitializeComponent();

            ID = id;
            UserID = userId;
            Title = name;
            Connection = connection;
            Connected = true;

            Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.RFileModule, ID));
        }

        public void Accept()
        {
            Task.Run(Pulsar);
        }

        public void SetDirs(byte[] data)
        {
            DirMessage msg;
            using (var ms = new MemoryStream(data))
            {
                msg = new BinaryFormatter().Deserialize(ms) as DirMessage;
            }

            if (msg.Path.IndexOf("<Error>") > -1)
            {
                MessageBox.Show(msg.Path);
                return;
            }

            Dispatcher.Invoke(Table.Items.Clear);
            PathBox.Text = msg.Path;

            if (msg.Path == "")
            {
                if (msg.Drives != null)
                    for (int i = 0; i < msg.Drives.Length; i++)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            DirectoryControll controll = new DirectoryControll(msg.Drives[i]);
                            Table.Items.Add(controll);

                        });
                    }

                return;
            }

            Dispatcher.Invoke(() =>
            {
                DirectoryControll controll = new DirectoryControll(System.IO.Path.GetDirectoryName(msg.Path));
                Table.Items.Add(controll);
            });

            if (msg.Directories != null)
                for (int i = 0; i < msg.Directories.Length; i++)
                {
                    Dispatcher.Invoke(() =>
                    {
                        DirectoryControll controll = new DirectoryControll(msg.Directories[i]);
                        Table.Items.Add(controll);
                    });
                }

            if (msg.Files != null)
                for (int i = 0; i < msg.Files.Length; i++)
                {
                    Dispatcher.Invoke(() =>
                    {
                        DirectoryControll controll = new DirectoryControll(msg.Files[i]);
                        Table.Items.Add(controll);
                    });
                }
        }

        public void SetData(byte[] buffer)
        {
            try
            {
                var data = FromBytes(buffer);
                if (data.Done)
                    Dispatcher.Invoke(Loading.Done);
                else if (data.Error)
                    Dispatcher.Invoke(Loading.Error);
                else
                    Loading.SetData(data);
            }
            catch
            {
                Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.StopDownload, ID));
            }
        }

        public void SetProp(byte[] data)
        {
            Dispatcher.Invoke(() =>
            {
                PropWindow window = new PropWindow();
                using (var ms = new MemoryStream(data))
                    window.SetProps(new BinaryFormatter().Deserialize(ms) as PropData);
                window.Owner = this;
                window.Show();
            });
        }

        private FileData FromBytes(byte[] buffer)
        {
            using (var ms = new MemoryStream(buffer))
                return new BinaryFormatter().Deserialize(ms) as FileData;
        }

        private void Pulsar()
        {
            while (Dispatcher.Invoke(() => IsLoaded))
            {
                Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.FilePulsar, ID));
                Thread.Sleep(5000);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Table_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DirectoryControll controll = Table.SelectedItem as DirectoryControll;
                Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.CD, ID, controll.FullName));
            }
            catch { }
        }

        private void PathBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.CD, ID, PathBox.Text));
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DirectoryControll controll = Table.SelectedItem as DirectoryControll;
                Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.Run, ID, controll.FullName));
            }
            catch { }
        }

        private void RunWithButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DirectoryControll controll = Table.SelectedItem as DirectoryControll;

                TextWindow textWindow = new TextWindow(false);
                textWindow.Owner = this;
                textWindow.ShowDialog();

                Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.RunWith, ID, controll.FullName + ";" + textWindow.TextBox.Text));
            }
            catch { }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            CloseEvent(this);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DirectoryControll controll = Table.SelectedItem as DirectoryControll;
                Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.Delete, ID, controll.FullName));
            }
            catch { }
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Loading = new LoadingWindow();
                Loading.Show();
                DirectoryControll controll = Table.SelectedItem as DirectoryControll;
                Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.StartDownload, ID, controll.FullName));
            }
            catch { }
        }

        private void PropButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DirectoryControll controll = Table.SelectedItem as DirectoryControll;
                Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.GetDirInfo, ID, controll.FullName));
            }
            catch { }
        }
    }
}
