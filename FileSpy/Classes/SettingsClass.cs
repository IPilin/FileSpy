using System;
using System.IO;
using System.Xml.Serialization;

namespace FileSpy.Classes
{
    public class SettingsClass
    {
        public string UserName { get; set; }
        public string PathToSave { get; set; }
        public bool AutoRun { get; set; }
        public bool HiddenStart { get; set; }
        public string KeyWord { get; set; }

        public SettingsClass()
        {
            UserName = Environment.UserName;
            PathToSave = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\";
            AutoRun = true;
            HiddenStart = true;
            KeyWord = "Simple";
        }

        public static SettingsClass Create()
        {
            if (File.Exists("settings.xml"))
            {
                var ser = new XmlSerializer(typeof(SettingsClass));
                using (var fs = new FileStream("settings.xml", FileMode.Open, FileAccess.Read))
                {
                    return ser.Deserialize(fs) as SettingsClass;
                }
            }
            else
            {
                return new SettingsClass();
            }
        }

        public void Save()
        {
            using (var fs = new FileStream("settings.xml", FileMode.Create, FileAccess.Write))
            {
                var ser = new XmlSerializer(typeof(SettingsClass));
                ser.Serialize(fs, this);
            }
        }

        public void SetKeyWord(string word)
        {
            KeyWord = word;
            using (var fs = new FileStream("settings.xml", FileMode.OpenOrCreate, FileAccess.Write))
            {
                var ser = new XmlSerializer(typeof(SettingsClass));
                ser.Serialize(fs, this);
            }
        }

        public void Restore()
        {
            UserName = Environment.UserName;
            PathToSave = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\";
            AutoRun = true;
            HiddenStart = true;
            KeyWord = "Simple";
        }
    }
}
