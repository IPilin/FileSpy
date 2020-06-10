using FileSpy.Classes;
using Microsoft.Win32;
using System;
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
            AutorunCheck.IsChecked = settings.AutoRun;
            HiddenCheck.IsChecked = settings.HiddenStart;
        }

        private void SaveButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Settings.UserName = UserBox.Text;
            Settings.PathToSave = PathLabel.Content as string;
            Settings.AutoRun = (bool)AutorunCheck.IsChecked;
            if ((bool)AutorunCheck.IsChecked)
            {
                if (!IsStartupItem())
                    SetAutorunValue(true, Environment.CurrentDirectory + "\\Updater.exe");
            }
            else
            {
                if (IsStartupItem())
                    SetAutorunValue(false, Environment.CurrentDirectory + "\\Updater.exe");
            }
            Settings.HiddenStart = (bool)HiddenCheck.IsChecked;

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

        public bool SetAutorunValue(bool autorun, string path)
        {
            RegistryKey reg;
            reg = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");
            try
            {
                if (autorun)
                    reg.SetValue("FileSpy", path);
                else
                    reg.DeleteValue("FileSpy");

                reg.Close();
            }
            catch
            {
                return false;
            }
            return true;
        }

        private bool IsStartupItem()
        {
            // The path to the key where Windows looks for startup applications
            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (rkApp.GetValue("FileSpy") == null)
                // The value doesn't exist, the application is not set to run at startup
                return false;
            else
                // The value exists, the application is set to run at startup
                return true;
        }
    }
}
