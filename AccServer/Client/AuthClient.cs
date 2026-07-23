using System;
using System.Net.Sockets;
using AccServer.Network.Sockets;
using AccServer.Network;

namespace AccServer.Client
{
    public unsafe class AuthClient
    {
        private ClientWrapper _socket;
        public Network.AuthPackets.Authentication Info;
        public Database.AccountTable Account;
        public Network.Cryptography.AuthCryptography Cryptographer;
        public int PasswordSeed;
        public ConcurrentPacketQueue Queue;
        public AuthClient(ClientWrapper socket)
        {
            Queue = new ConcurrentPacketQueue(0);
            _socket = socket;
        }
        public void Send(byte[] buffer)
        {
            byte[] _buffer = new byte[buffer.Length];
            Buffer.BlockCopy(buffer, 0, _buffer, 0, buffer.Length);
            Cryptographer.Encrypt(_buffer);
            _socket.Send(_buffer);
        }
        public string IP
        {
            get { return _socket.IP; }

        }
        public void Disconnect()
        {
            _socket.Disconnect();
        }
        public void Send(Interfaces.IPacket buffer)
        {
            Send(buffer.ToArray());
        }
    }
}