using FileSpy.Classes;
using System.Windows.Controls;
using System.Windows.Input;

namespace FileSpy.Elements
{
    /// <summary>
    /// Логика взаимодействия для UserControll.xaml
    /// </summary>
    public partial class UserControll : UserControl
    {
        public delegate void EventHandler(int id, string name, int command);
        public event EventHandler ActiveEvent;

        public int ID { get; set; }
        bool Admin;

        public UserControll(int id, string name, string desktop)
        {
            InitializeComponent();
            ID = id;
            NameLabel.Content = name;
            DesktopLabel.Content = desktop;
        }

        public void SetEnabled(int opacity, bool enabled)
        {
            FileButton.Opacity = opacity;
            FileButton.IsEnabled = enabled;
            VideoButton.Opacity = opacity;
            VideoButton.IsEnabled = enabled;
            KeyButton.Opacity = opacity;
            KeyButton.IsEnabled = enabled;

            Admin = enabled;
        }

        private void FileButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void VideoButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ActiveEvent(ID, NameLabel.Content as string, ElementCommands.VideoModule);
        }

        private void KeyButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void SendButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ActiveEvent(ID, NameLabel.Content as string, ElementCommands.SendModule);
        }

        private void NameLabel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Admin)
                ActiveEvent(ID, NameLabel.Content as string, ElementCommands.InfoModule);
        }
    }
}
