using System;
using System.Collections.Generic;
using System.IO;

namespace FileSpy.Classes
{
    [Serializable]
    public class DirMessage
    {   
        [Serializable]
        public class DriveI
        {
            public DriveType DriveType { get; set; }
            public string Name { get; set; }
            public long TotalSize { get; set; }
            public long AvailableFreeSpace { get; set; }

            public DriveI(DriveInfo info)
            {
                this.DriveType = info.DriveType;
                this.Name = info.Name;
                this.TotalSize = info.TotalSize;
                this.AvailableFreeSpace = info.AvailableFreeSpace;
            }
        }

        [Serializable]
        public class DirectoryI
        {
            public FileAttributes Attributes { get; set; }
            public string FullName { get; set; }
            public string Name { get; set; }

            public DirectoryI(DirectoryInfo info)
            {
                this.Attributes = info.Attributes;
                this.FullName = info.FullName;
                this.Name = info.Name;
            }
        }

        [Serializable]
        public class FileI
        {
            public FileAttributes Attributes { get; set; }
            public string FullName { get; set; }
            public string Name { get; set; }
            public DateTime CreationTime { get; set; }
            public DateTime LastWriteTime { get; set; }
            public long Length { get; set; }

            public FileI(FileInfo info)
            {
                this.Attributes = info.Attributes;
                this.FullName = info.FullName;
                this.Name = info.Name;
                this.CreationTime = info.CreationTime;
                this.LastWriteTime = info.LastWriteTime;
                this.Length = info.Length;
            }
        }

        public string Path { get; set; }
        public DriveI[] Drives { get; set; }
        public DirectoryI[] Directories { get; set; }
        public FileI[] Files { get; set; }

        public DirMessage(string path)
        {
            try
            {
                Path = path;

                if (Path == "<drives>")
                {
                    Path = "";
                    var drives = DriveInfo.GetDrives();
                    int count = 0;
                    for (int i = 0; i < drives.Length; i++)
                        if (drives[i].IsReady)
                            count++;
                    Drives = new DriveI[count];
                    for (int i = 0, id = 0; i < Drives.Length; i++)
                        if (drives[i].IsReady)
                        {
                            Drives[id] = new DriveI(drives[i]);
                            id++;
                        }
                    return;
                }

                DirectoryInfo dir = new DirectoryInfo(Path);
                var dirs = dir.GetDirectories();
                Directories = new DirectoryI[dirs.Length];
                for (int i = 0; i < Directories.Length; i++)
                    Directories[i] = new DirectoryI(dirs[i]);

                var files = dir.GetFiles();
                Files = new FileI[files.Length];
                for (int i = 0; i < Files.Length; i++)
                    Files[i] = new FileI(files[i]);
            }
            catch (Exception e)
            {
                Path = "<Error>\n" + e.Message;
            }
        }
    }
}
