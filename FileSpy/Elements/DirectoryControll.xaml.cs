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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FileSpy.Elements
{
    /// <summary>
    /// Логика взаимодействия для DirectoryControll.xaml
    /// </summary>
    public partial class DirectoryControll : UserControl
    {
        public string FullName;

        public DirectoryControll(DriveInfo info)
        {
            InitializeComponent();

            if (info.DriveType == DriveType.Fixed)
                FixedDriveIcon.Opacity = 1;
            else if (info.DriveType == DriveType.CDRom)
                CDRoomIcon.Opacity = 1;
            else if (info.DriveType == DriveType.Removable)
                USBDriveIcon.Opacity = 1;
            else
                UnknownDriveIcon.Opacity = 1;

            FullName = info.Name;
            NameLabel.Content = info.Name;
            SizeLabel.Content = ToNormal(info.TotalSize) + " (" + ToNormal(info.AvailableFreeSpace) + " free)";
        }

        public DirectoryControll(DirectoryInfo info)
        {
            InitializeComponent();

            if (info.Attributes.HasFlag(FileAttributes.Hidden))
                FolderIcon.Opacity = 0.5;
            else
                FolderIcon.Opacity = 1;

            FullName = info.FullName;
            NameLabel.Content = info.Name;
        }

        public DirectoryControll(FileInfo info)
        {
            InitializeComponent();

            if (info.Attributes.HasFlag(FileAttributes.Hidden))
                FileIcon.Opacity = 0.5;
            else
                FileIcon.Opacity = 1;

            FullName = info.FullName;
            NameLabel.Content = info.Name;
            CreationLabel.Content = info.CreationTime.ToString();
            ChangingLabel.Content = info.LastWriteTime.ToString();
            SizeLabel.Content = ToNormal(info.Length);
        }

        public DirectoryControll(string fullName)
        {
            InitializeComponent();

            if (String.IsNullOrEmpty(fullName))
                FullName = "<drives>";
            else
                FullName = fullName;
            NameLabel.Content = ".";
            PrevIcon.Opacity = 1;
        }

        private string ToNormal(long size)
        {
            int k = 0;
            for (; size > 1024; size /= 1024)
                k++;
            string result = size.ToString();
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
