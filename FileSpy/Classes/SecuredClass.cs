using System;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Controls;

namespace FileSpy.Classes
{
    public class SecuredClass
    {
        public byte[] AesKey;

        RSACryptoServiceProvider RSA;

        public SecuredClass()
        {
            RSA = new RSACryptoServiceProvider();

            using (var aes = Aes.Create())
            {
                aes.GenerateKey();
                AesKey = aes.Key;
            }
        }

        public string GetPublicKey()
        {
            return RSA.ToXmlString(false);
        }

        public void SetPublicKey(string key)
        {
            RSA.FromXmlString(key);
        }

        public byte[] RsaEncrypt(byte[] data)
        {
            return RSA.Encrypt(data, true);
        }

        public byte[] RsaDecrypt(byte[] data)
        {
            return RSA.Decrypt(data, true);
        }

        public void SetAesKey(byte[] key)
        {
            AesKey = key;
        }

        public byte[] Encrypt(byte[] data, bool secured)
        {
            if (secured)
            {
                using (var aes = Aes.Create())
                {
                    aes.GenerateIV();
                    aes.Key = AesKey;

                    var crypto = aes.CreateEncryptor(aes.Key, aes.IV);
                    using (var ms = new MemoryStream())
                    {
                        ms.Write(aes.IV, 0, aes.IV.Length);
                        using (var cs = new CryptoStream(ms, crypto, CryptoStreamMode.Write))
                        {
                            cs.Write(BitConverter.GetBytes(data.Length), 0, 4);
                            cs.Write(data, 0, data.Length);
                        }

                        byte[] enc = ms.ToArray();

                        using (var ms1 = new MemoryStream())
                        {
                            ms1.Write(BitConverter.GetBytes(enc.Length), 0, 4);
                            ms1.Write(enc, 0, enc.Length);

                            return ms1.ToArray();
                        }
                    }
                }
            }
            else
            {
                using (var ms = new MemoryStream())
                {
                    ms.Write(BitConverter.GetBytes(data.Length), 0, 4);
                    ms.Write(data, 0, data.Length);
                    return ms.ToArray();
                }
            }
        }

        public byte[] Decrypt(byte[] data)
        {
            using (var aes = Aes.Create())
            {
                byte[] iv = new byte[16];
                for (int i = 0; i < 16; i++)
                {
                    iv[i] = data[i];
                }
                var crypto = aes.CreateDecryptor(AesKey, iv);
                using (var ms = new MemoryStream())
                {
                    ms.Write(data, 16, data.Length - 16);
                    ms.Seek(0, SeekOrigin.Begin);
                    using (var cs = new CryptoStream(ms, crypto, CryptoStreamMode.Read))
                    {
                        using (var br = new BinaryReader(cs))
                        {
                            int count = br.ReadInt32();
                            return br.ReadBytes(count);
                        }
                    }
                }
            }
        }
    }
}
