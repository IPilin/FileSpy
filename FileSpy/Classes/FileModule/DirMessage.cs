using System;
using System.Collections.Generic;
using System.IO;

namespace FileSpy.Classes
{
    [Serializable]
    class DirMessage
    {
        public string Path { get; set; }
        public DriveInfo[] Drives { get; set; }
        public DirectoryInfo[] Directories { get; set; }
        public FileInfo[] Files { get; set; }

        public DirMessage(string path)
        {
            try
            {
                Path = path;

                if (Path == "<drives>")
                {
                    Path = "";
                    Drives = DriveInfo.GetDrives();
                    return;
                }

                DirectoryInfo dir = new DirectoryInfo(Path);
                Directories = dir.GetDirectories();
                Files = dir.GetFiles();
            }
            catch (Exception e)
            {
                Path = "<Error>\n" + e.Message;
            }
        }
    }
}
