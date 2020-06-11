using System;
using System.IO;
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
    }
}
