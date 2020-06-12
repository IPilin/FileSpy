using FileSpy.Classes.FileModule;
using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace FileSpy.Classes
{
    class FileClass
    {
        public int ID { get; set; }
        public int UserID { get; set; }

        ConnectionClass Connection;
        DateTime LastTime;
        bool Connected;
        bool Sending;

        public delegate void CloseHandler(FileClass fileClass);
        public event CloseHandler CloseEvent;

        public FileClass(int id, int userId, ConnectionClass connection)
        {
            ID = id;
            UserID = userId;
            Connection = connection;
        }

        public void Start()
        {
            LastTime = DateTime.Now;
            Connected = true;
            Task.Run(Brander);
            Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.FileMAccepted, ID));
            Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.Dirs, ID, CD("<drives>")));
        }

        public void Pulsar()
        {
            LastTime = DateTime.Now;
        }

        public byte[] CD(string path)
        {
            using (var ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, new DirMessage(path));
                return ms.ToArray();
            }
        }

        private void Brander()
        {
            while (Connected)
            {
                try
                {
                    if ((DateTime.Now - LastTime).TotalSeconds > 10)
                        Close();
                    else
                        Thread.Sleep(1);
                }
                catch { }
            }
        }

        private void Close()
        {
            Connected = false;
            CloseEvent(this);
        }

        public void StartDownload(string path)
        {
            Sending = true;
            long id = 0;

            if (Directory.Exists(path))
            {
                if (File.Exists("temp.zip")) File.Delete("temp.zip");
                ZipFile.CreateFromDirectory(path, "temp.zip");
                path = "temp.zip";
            }

            while (Connected && Sending)
            {
                FileData data = new FileData(path, ref id);
                Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.FileDData, ID, ToBytes(data)));
                if (data.Done) Sending = false;
            }

            if (File.Exists("temp.zip"))
                File.Delete("temp.zip");
        }

        public void StopDownload()
        {
            Sending = false;
        }

        public void GetDirInfo(string path)
        {
            PropData data;
            if (Directory.Exists(path))
                data = new PropData(new DirectoryInfo(path));
            else
                data = new PropData(new FileInfo(path));
            using (var ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, data);
                Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.DirInfo, ID, ms.ToArray()));
            }
        }

        public void SetData(byte[] buffer)
        {
            try
            {
                var data = FromBytes(buffer);
                if (data.Done)
                {
                    Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.Dirs, ID, CD(data.Path)));
                    return;
                }
                using (var fs = new FileStream(data.Path + data.File.Name, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    fs.Seek(0, SeekOrigin.End);
                    fs.Write(data.Data, 0, data.Data.Length);
                }
            }
            catch (Exception e)
            {
                Connection.SendMessage(new MessageClass(Connection.ID, UserID, Commands.FileUError, ID, e.Message));
            }
        }

        private byte[] ToBytes(FileData data)
        {
            using (var ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, data);
                return ms.ToArray();
            }
        }

        private FileData FromBytes(byte[] buffer)
        {
            using (var ms = new MemoryStream(buffer))
                return new BinaryFormatter().Deserialize(ms) as FileData;
        }
    }
}
