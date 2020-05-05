using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Логика взаимодействия для RequestWindow.xaml
    /// </summary>
    public partial class RequestWindow : Window
    {
        public bool Result { get; private set; }
        public RequestWindow(string sender, string file)
        {
            InitializeComponent();
            SenderLabel.Content = sender + " send you a file:";
            FileLabel.Content = "\"" + file + "\"";
        }

        private void CancelButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void AcceptButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Result = true;
            Close();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            Result = true;
            Close();
        }
    }
}
