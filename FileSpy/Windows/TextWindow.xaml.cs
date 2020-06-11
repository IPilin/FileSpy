using System.Windows;
using System.Windows.Input;

namespace FileSpy.Windows
{
    /// <summary>
    /// Логика взаимодействия для TextWindow.xaml
    /// </summary>
    public partial class TextWindow : Window
    {
        bool Password;

        public TextWindow(bool password)
        {
            InitializeComponent();

            Password = password;

            if (password)
                PassBox.Opacity = 1;
            else
                TextBox.Opacity = 1;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Password)
                PassBox.Focus();
            else
                TextBox.Focus();
        }

        private void PassBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Close();
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Close();
        }
    }
}
