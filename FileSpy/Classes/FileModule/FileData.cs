using System;
using System.IO;

namespace FileSpy.Classes.FileModule
{
    [Serializable]
    public class FileData
    {
        public FileInfo File { get; set; }
        public long Size { get; set; }
        public byte[] Data { get; set; }
        public bool Done { get; set; }
        public bool Error { get; set; }

        public FileData(string path, ref long id)
        {
            try
            {
                Error = false;
                Done = false;

                File = new FileInfo(path);
                Size = File.Length;
                using (var fs = new FileStream(File.FullName, FileMode.Open, FileAccess.Read))
                {
                    if (fs.Length - id == 0)
                    {
                        Done = true;
                        return;
                    }

                    fs.Seek(id, SeekOrigin.Begin);

                    if (fs.Length - id > 50000)
                    {
                        Data = new byte[50000];
                        fs.Read(Data, 0, Data.Length);
                        id += 50000;
                    }
                    else
                    {
                        Data = new byte[fs.Length - id];
                        fs.Read(Data, 0, Data.Length);
                        id += fs.Length - id;
                    }
                }
            }
            catch 
            {
                Error = true;
            }
        }
    }
}
