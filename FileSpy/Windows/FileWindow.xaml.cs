using FileSpy.Classes;
using FileSpy.Elements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

        public FileWindow(int id, int userId, string name, ConnectionClass connection)
        {
            InitializeComponent();

            ID = id;
            UserID = userId;
            Title = name;
            Connection = connection;
            Connected = true;

            Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.RFileModule, ID));
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
                            if (msg.Drives[i].IsReady)
                            {
                                DirectoryControll controll = new DirectoryControll(msg.Drives[i]);
                                Table.Items.Add(controll);
                            }
    
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
    }
}
