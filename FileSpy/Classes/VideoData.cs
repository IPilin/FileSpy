using System;
using System.IO;

namespace FileSpy.Classes
{
    class VideoData
    {
        public byte[] Data { get; set; }
        public UInt64 ID { get; set; }

        public VideoData(byte[] data, UInt64 id)
        {
            Data = data;
            ID = id;
        }

        public VideoData(byte[] buffer)
        {
            using (var ms = new MemoryStream(buffer))
            {
                using (var br = new BinaryReader(ms))
                {
                    ID = br.ReadUInt64();
                    Data = br.ReadBytes(buffer.Length - 8);
                }
            }
        }

        public byte[] ToArray()
        {
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    bw.Write(ID);
                    bw.Write(Data);
                }

                return ms.ToArray();
            }
        }
    }
}
