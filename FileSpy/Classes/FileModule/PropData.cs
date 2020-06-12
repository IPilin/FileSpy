using System;
using System.IO;

namespace FileSpy.Classes.FileModule
{
    [Serializable]
    public class PropData
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public bool IsFolder { get; set; }
        public long FilesCount { get; set; }
        public long Size { get; set; }
        public string Attributes { get; set; }

        public DateTime Creation { get; set; }
        public DateTime Changing { get; set; }
        public DateTime Opened { get; set; }

        public PropData(DirectoryInfo info)
        {
            Name = info.Name;
            FullName = info.FullName;
            IsFolder = true;
            long filesCount = 0;
            Size = GetFolderSize(info.FullName, ref filesCount);
            FilesCount = filesCount;
            Attributes = info.Attributes.ToString();
            Creation = info.CreationTime;
            Changing = info.LastWriteTime;
            Opened = info.LastAccessTime;
        }

        public PropData(FileInfo info)
        {
            Name = info.Name;
            FullName = info.FullName;
            IsFolder = false;
            Size = info.Length;
            FilesCount = 1;
            Attributes = info.Attributes.ToString();
            Creation = info.CreationTime;
            Changing = info.LastWriteTime;
            Opened = info.LastAccessTime;
        }

        private long GetFolderSize(string path, ref long filesCount)
        {
            try
            {
                long result = 0;

                DirectoryInfo dir = new DirectoryInfo(path);

                FileInfo[] files = dir.GetFiles();

                for (int i = 0; i < files.Length; i++)
                {
                    result += files[i].Length;
                    filesCount++;
                }

                DirectoryInfo[] dirs = dir.GetDirectories();

                for (int i = 0; i < dirs.Length; i++)
                    result += GetFolderSize(dirs[i].FullName, ref filesCount);

                return result;
            }
            catch
            {
                return 0;
            }
        }
    }
}
