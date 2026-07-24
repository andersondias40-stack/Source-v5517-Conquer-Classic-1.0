using System;

namespace COServer.ServerSockets
{
    public class ReceiveBuffer
    {
        public const int RECV_BUFFER_SIZE = 2048;//16384;//8192;//2048;
        public const int HeadSize = 1024;

        public byte[] buffer;
        private int nLen;

        public ReceiveBuffer(int ReceiveBufferSize, bool changesize = false)
        {
            if (changesize)
            {
                buffer = new byte[ReceiveBufferSize];
            }
            else
            {
                //    if (ReceiveBufferSize == 0)
                buffer = new byte[RECV_BUFFER_SIZE];//RECV_BUFFER_SIZE];
                //else
                //  buffer = new byte[ReceiveBufferSize];

            }
            Reset();
        }


        public void AddLength(int length)
        {
            nLen += length;
        }

        public ushort ReadHead()
        {
            return BitConverter.ToUInt16(buffer, 0);
        }
        public void DelLength(int length)
        {
            nLen -= length;
        }


        public int Length() { return nLen; }
        public int MaxLength() { return buffer.Length; }
        public void Reset()
        {
            nLen = 0;
        }

    }
}
