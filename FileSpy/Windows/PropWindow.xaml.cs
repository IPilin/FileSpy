using FileSpy.Classes.FileModule;
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
    /// Логика взаимодействия для PropWindow.xaml
    /// </summary>
    public partial class PropWindow : Window
    {
        public PropWindow()
        {
            InitializeComponent();
        }

        public void SetProps(PropData data)
        {
            if (data.IsFolder)
                FolderIcon.Opacity = 1;
            else
                FileIcon.Opacity = 1;

            NameLabel.Content = data.Name;
            FullNameLabel.Content = data.FullName;
            SizeLabel.Content = ToNormal(data.Size) + " (" + ToLong(data.Size) + " bytes)";
            FileCountLabel.Content = data.FilesCount + " files";
            CreatedLabel.Content = data.Creation.ToString();
            ChangingLabel.Content = data.Changing.ToString();
            OpenedLabel.Content = data.Opened.ToString();
            AttributesLabel.Content = data.Attributes;
        }

        private string ToLong(long size)
        {
            string result = "";
            string r_result = "";
            int counter = 0;
            string buffer = size.ToString();
            for (int i = buffer.Length - 1; i > -1; i--)
            {
                result += buffer[i];
                counter++;
                if (counter == 3)
                {
                    result += " ";
                    counter = 0;
                }
            }

            for (int i = result.Length - 1; i > -1; i--)
                r_result += result[i];

            return r_result;
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
