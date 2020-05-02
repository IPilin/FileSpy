using FileSpy.Classes;
using System.Windows;
using System.Windows.Input;

namespace FileSpy.Windows
{
    /// <summary>
    /// Логика взаимодействия для VideoWindow.xaml
    /// </summary>
    public partial class VideoWindow : Window
    {
        public int ID { get; set; }
        public int UserID { get; set; }

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

            Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.RVideoModule, ID));
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
            CloseEvent(this);
        }
    }
}
