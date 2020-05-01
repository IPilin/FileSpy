using System.IO;
using System.Text;

namespace FileSpy.Classes
{
    public class MessageClass
    {
        public int Sender { get; set; }
        public int Getter { get; set; }
        public int Command { get; set; }
        public int ElementID { get; set; }
        public byte[] Package { get; set; }

        public MessageClass() { }

        public MessageClass(int sender, int getter, int command, int id)
        {
            Sender = sender;
            Getter = getter;
            Command = command;
            ElementID = id;
        }

        public MessageClass(int sender, int getter, int command, int id, string buffer)
        {
            Sender = sender;
            Getter = getter;
            Command = command;
            ElementID = id;
            Package = Encoding.UTF8.GetBytes(buffer);
        }

        public MessageClass(int sender, int getter, int command, int id, byte[] buffer)
        {
            Sender = sender;
            Getter = getter;
            Command = command;
            ElementID = id;
            Package = buffer;
        }

        public MessageClass(byte[] buffer)
        {
            using (var ms = new MemoryStream(buffer))
            {
                using (var br = new BinaryReader(ms))
                {
                    Getter = br.ReadInt32();
                    Sender = br.ReadInt32();
                    Command = br.ReadInt32();
                    ElementID = br.ReadInt32();
                    if (buffer.Length > 16)
                        Package = br.ReadBytes(buffer.Length - 16);
                }
            }
        }

        public string GetStringPackage()
        {
            return Encoding.UTF8.GetString(Package);
        }

        public byte[] GetBytes()
        {
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    bw.Write(Sender);
                    bw.Write(Getter);
                    bw.Write(Command);
                    bw.Write(ElementID);
                    if (Package != null)
                        bw.Write(Package);
                    return ms.ToArray();
                }
            }
        }
    }
}
