using System;
using System.IO;
using System.Text;


namespace AccServer.Network.AuthPackets
{
   
    public unsafe class Authentication : Interfaces.IPacket
    {
        public string Username;
        public string Password;
        public string Server;
        public string Hwid;
        public string Mac;
        public string conquer_hash, c3wdbHash, magic_hash, dll_hash;

        public Authentication()
        {
        }
        private byte[] _array;
        internal int Seed;

        public string ReadString(int length, int offset)
        {
            // Read the value:
            fixed (byte* ptr = _array)
                return new string((sbyte*)ptr, offset, length, Encoding.GetEncoding(1252)).TrimEnd('\0');
        }
        public void Deserialize(byte[] buffer)
        {
            MemoryStream MS = new MemoryStream(buffer);
            BinaryReader BR = new BinaryReader(MS);
            ushort length = BR.ReadUInt16();//276//0
            if (length == 276)
            {
                ushort type = BR.ReadUInt16();//2
                if (type == 1086)
                {
                    Username = Encoding.ASCII.GetString(BR.ReadBytes(16));//4
                    Username = Username.Replace("\0", "");
                    BR.ReadBytes(112);
                    Password = Encoding.ASCII.GetString(BR.ReadBytes(16));
                    BR.ReadBytes(112);
                    Server = Encoding.ASCII.GetString(BR.ReadBytes(16));
                    Server = Server.Replace("\0", "");
                    //Server = "FireConquer";
                    var data = new byte[16];
                    Buffer.BlockCopy(buffer, 132, data, 0, 16);

                    const uint corc5PwKey = 0xB7E15163;
                    const uint corc5QwKey = 0x61C88647;
                    var corc5BufKey = new byte[]
                    {
                            0x3C, 0xDC, 0xFE, 0xE8, 0xC4, 0x54, 0xD6, 0x7E, 0x16, 0xA6, 0xF8, 0x1A, 0xE8, 0xD0,
                            0x38, 0xBE
                    };

                    var rc5 =
                        new CO2_CORE_DLL.Security.Cryptography.CORC5(corc5PwKey, corc5QwKey);
                    rc5.GenerateKey(corc5BufKey);
                    rc5.Decrypt(ref data);
                    Server = "Emulator";
                    Password = Encoding.ASCII.GetString(data);
                    Password = Password.Replace("\0", "");
                }
            }
            BR.Close();
            MS.Close();
        }
        public byte[] ToArray()
        {
            throw new NotImplementedException();
        }


    }
}