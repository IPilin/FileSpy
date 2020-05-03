using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace FileSpy.Windows
{
    /// <summary>
    /// Логика взаимодействия для StatusWindow.xaml
    /// </summary>
    public partial class StatusWindow : Window
    {
        public StatusWindow(string ver, string status)
        {
            WindowBlur.SetIsEnabled(this, true);
            InitializeComponent();

            VersionLabel.Content = ver;

            if (status == "Simple")
                StatusLabel.Foreground = Brushes.Green;
            else if (status == "Admin")
                StatusLabel.Foreground = Brushes.Blue;
            else
                StatusLabel.Foreground = Brushes.Black;

            StatusLabel.Content = status;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                for (int i = 0; i < 20; i++)
                {
                    Dispatcher.Invoke(() => Opacity += 0.05);
                    Thread.Sleep(36);
                }
                Thread.Sleep(2000);
                for (int i = 0; i < 20; i++)
                {
                    Dispatcher.Invoke(() => Opacity -= 0.05);
                    Thread.Sleep(36);
                }

                Dispatcher.Invoke(Close);
            });
        }
    }
}
