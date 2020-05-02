using System;
using System.Threading;
using System.Threading.Tasks;

namespace FileSpy.Classes
{
    class VideoClass
    {
        public int ID { get; set; }
        public int UserID { get; set; }

        bool Connected;
        DateTime LastTime;

        public delegate void CloseHandler(VideoClass videoClass);
        public event CloseHandler CloseEvent;

        public VideoClass()
        {
            Connected = true;
        }

        public void Start()
        {
            Task.Run(Brander);
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
            if (Connected)
            {
                Connected = false;
                CloseEvent(this);
            }
        }
    }
}
