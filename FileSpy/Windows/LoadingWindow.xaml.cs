using FileSpy.Classes.FileModule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FileSpy.Windows
{
    /// <summary>
    /// Логика взаимодействия для LoadingWindow.xaml
    /// </summary>
    public partial class LoadingWindow : Window
    {
        long Downloaded = 0;
        long LastSize = 0;
        DateTime LastTime = DateTime.Now;
        string dir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\";

        public LoadingWindow()
        {
            InitializeComponent();
        }

        public void SetData(FileData data)
        {
            Downloaded += data.Data.Length;
            Dispatcher.Invoke(() => FileNameLabel.Content = data.File.Name);
            Dispatcher.Invoke(() => CountLabel.Content = ToNormal(Downloaded) + "/" + ToNormal(data.Size));
            Dispatcher.Invoke(() => ProgressBar.Maximum = data.Size);
            Dispatcher.Invoke(() => ProgressBar.Value = Downloaded);
            if ((DateTime.Now - LastTime).TotalSeconds >= 1)
            {
                Dispatcher.Invoke(() => StatusLabel.Content = ((ProgressBar.Value - LastSize) / (DateTime.Now - LastTime).TotalSeconds / 1024 / 1024).ToString("0.00") + "MB/s");
                LastTime = DateTime.Now;
                Dispatcher.Invoke(() => LastSize = (long)ProgressBar.Value);
            }

            using (var fs = new FileStream(dir + data.File.Name, FileMode.OpenOrCreate, FileAccess.Write))
            {
                fs.Seek(0, SeekOrigin.End);
                fs.Write(data.Data, 0, data.Data.Length);
            }
        }

        public void GetData(FileData data)
        {
            Downloaded += data.Data.Length;
            Dispatcher.Invoke(() => FileNameLabel.Content = data.File.Name);
            Dispatcher.Invoke(() => CountLabel.Content = ToNormal(Downloaded) + "/" + ToNormal(data.Size));
            Dispatcher.Invoke(() => ProgressBar.Maximum = data.Size);
            Dispatcher.Invoke(() => ProgressBar.Value = Downloaded);
            if ((DateTime.Now - LastTime).TotalSeconds >= 1)
            {
                Dispatcher.Invoke(() => StatusLabel.Content = ((ProgressBar.Value - LastSize) / (DateTime.Now - LastTime).TotalSeconds / 1024 / 1024).ToString("0.00") + "MB/s");
                LastTime = DateTime.Now;
                Dispatcher.Invoke(() => LastSize = (long)ProgressBar.Value);
            }
        }

        public void Error()
        {
            StatusLabel.Content = "Error!";
        }

        public void Done()
        {
            StatusLabel.Content = "Done!";
        }

        private string ToNormal(long size)
        {
            int k = 0;
            float fsize;
            for (fsize = size; fsize > 1024; fsize /= 1024)
                k++;
            string result = fsize.ToString("0.0");
            switch (k)
            {
                case 0:
                    result += "B";
                    break;
                case 1:
                    result += "KB";
                    break;
                case 2:
                    result += "MB";
                    break;
                case 3:
                    result += "GB";
                    break;
            }

            return result;
        }
    }
}
