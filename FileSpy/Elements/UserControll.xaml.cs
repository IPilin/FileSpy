using FileSpy.Classes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;
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

        public bool Checked { get; set; }
        public int ID { get; set; }
        bool Admin;

        public UserControll(int id, string name, string desktop)
        {
            InitializeComponent();

            ID = id;
            NameLabel.Content = name;
            DesktopLabel.Content = desktop;
            Checked = true;
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
            ActiveEvent(ID, NameLabel.Content as string, ElementCommands.FileModule);
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

        public void LoadedAnimation()
        {
            int delay = 15;
            for (int i = 0; i < 10; i++)
            {
                Dispatcher.Invoke(() => MainGrid.Margin = new Thickness(0, MainGrid.Margin.Top + 5, 0, 0));
                Thread.Sleep(delay);
                delay--;
            }
        }

        public void UnloadedAnimation()
        {
            int delay = 15;
            for (int i = 0; i < 10; i++)
            {
                //Dispatcher.Invoke(() => MainGrid.Margin = new Thickness(0, MainGrid.Margin.Top - 5, 0, 0));
                Dispatcher.Invoke(() => RealGrid.Height -= 5);
                Thread.Sleep(delay);
                delay--;
            }
        }
    }
}
