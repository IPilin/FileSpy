using FileSpy.Classes;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace FileSpy.Windows
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public bool Save { get; private set; }
        SettingsClass Settings;
        public SettingsWindow(SettingsClass settings)
        {
            InitializeComponent();
            Settings = settings;
            UserBox.Text = settings.UserName;
            PathLabel.Content = settings.PathToSave;
        }

        private void SaveButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Settings.UserName = UserBox.Text;
            Settings.PathToSave = PathLabel.Content as string;

            Settings.Save();
            Save = true;
            Close();
        }

        private void CancelButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void RestoreButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Settings.Restore();
            if (File.Exists("settings.xml"))
                File.Delete("settings.xml");
            Save = true;
            Close();
        }

        private void PathLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var window = new FolderBrowserDialog();
            if (window.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                PathLabel.Content = window.SelectedPath;
        }
    }
}
