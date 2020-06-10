using FileSpy.Classes;
using FileSpy.Elements;
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
    /// Логика взаимодействия для FileWindow.xaml
    /// </summary>
    public partial class FileWindow : Window
    {
        public FileWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DirectoryControll dir = new DirectoryControll();
            Table.Items.Add(dir);
        }
    }
}
