using System;
using System.Text;

namespace AccServer.Network.AuthPackets
{
    public unsafe class Forward : Interfaces.IPacket
    {
        public enum ForwardType : byte
        {
            Ready = 2,
            InvalidInfo = 1,
            WrongAccount = 57,
            ServersNotConfigured = 59,
            InvalidAuthenticationProtocol = 73,
            Banned = 25
        }
        private byte[] Buffer;
        public Forward()
        {
            Buffer = new byte[32];
            Network.Writer.WriteUInt16(32, 0, Buffer);
            Network.Writer.WriteUInt16(1055, 2, Buffer);
        }
        public uint Identifier
        {
            get { return BitConverter.ToUInt32(Buffer, 4); }
            set { Network.Writer.WriteUInt32(value, 4, Buffer); }
        }
        public uint State
        {
            get { return BitConverter.ToUInt32(Buffer, 8); }
            set { Network.Writer.WriteUInt32(value, 8, Buffer); }
        }
        public ForwardType Type
        {
            get { return (ForwardType) (byte) BitConverter.ToUInt32(Buffer, 8); }
            set { Network.Writer.WriteUInt32((byte) value, 8, Buffer); }
        }
        public string IP
        {
            get { return Encoding.Default.GetString(Buffer, 12, 16).Replace("\0", ""); }
            set { Network.Writer.WriteString(value, 12, Buffer); }
        }
        public ushort Port
        {
            get { return BitConverter.ToUInt16(Buffer, 28); }
            set { Network.Writer.WriteUInt16(value, 28, Buffer); }
        }
        public void Deserialize(byte[] buffer)
        {
        }
        public byte[] ToArray()
        {
            return Buffer;
        }
    }
}