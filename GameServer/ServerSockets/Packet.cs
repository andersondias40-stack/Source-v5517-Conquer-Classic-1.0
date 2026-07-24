using COServer.Role;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace COServer.ServerSockets
{
    public unsafe class Packet : IDisposable
    {

        public const int MAX_SIZE = 1024;
        private const int TQ_SEALSIZE = 8;

        private static string seal;

        /*[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void* memcpy(void* dst, void* src, int num);*/
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void* memset(void* dst, int val, int count);


        public static string SealString
        {
            get { return seal; }
            set
            {

                if (value.Length != TQ_SEALSIZE)
                    throw new ArgumentOutOfRangeException("value", "Packet seal must be " + TQ_SEALSIZE + " chars long.");

                seal = value;
            }
        }

        public static int SealSize
        {
            get { return seal != null ? TQ_SEALSIZE : 0; }
        }

        public int Size { get; set; }
        public byte* Memory { get; private set; }
        private bool IsDisposed = false;
        public byte* stream;

        public int Position { get { return (int)(stream - Memory); } }
        //  public byte* Pointer { get { return stream; } }
        public Packet(byte[] buffer)
        {
            Memory = (byte*)Marshal.AllocHGlobal(1024);
            stream = Memory;
            Marshal.Copy(buffer, 0, (IntPtr)this.stream, buffer.Length);
            this.Size = buffer.Length;
        }
        public Packet(int size)
        {
            Memory = (byte*)Marshal.AllocHGlobal(size);
            stream = Memory;
        }

        ~Packet()
        {
            this.Dispose();
        }
        public void Dispose()
        {
            // lock (this)
            {
                if (this.IsDisposed) return;
                IsDisposed = true;
                if ((IntPtr)this.Memory == IntPtr.Zero)
                    return;
                Marshal.FreeHGlobal((IntPtr)this.Memory);
                this.Memory = null;
                GC.SuppressFinalize(this);
            }
        }

        public void InitWriter()
        {
            Seek(4);//4
        }
        public void Seek(int offset)
        {
            stream = &Memory[offset];
        }
        public void SeekForward(int amount)
        {
            Seek(Position + amount);
        }
        public void SeekBackwards(int amount)
        {
            Seek(Position - amount);
        }
        public void Write(byte value)
        {
            if (Position + 1 >= MAX_SIZE)
                return;
            *stream = value;
            stream++;
        }
        public void Write(ushort value)
        {
            if (Position + 2 >= MAX_SIZE)
                return;
            *((ushort*)stream) = value;
            stream += sizeof(ushort);
        }
        public void Write(uint value)
        {
            if (Position + 4 >= MAX_SIZE)
                return;
            *((uint*)stream) = value;
            stream += sizeof(uint);
        }
        public void Write(ulong value)
        {
            if (Position + 8 >= MAX_SIZE)
                return;
            *((ulong*)stream) = value;
            stream += sizeof(ulong);
        }
        public void Write(sbyte value) { Write((byte)value); }
        public void Write(short value) { Write((ushort)value); }
        public void Write(int value) { Write((uint)value); }
        public void Write(long value) { Write((ulong)value); }
        public void Write(string value, int length)
        {
            int min = Math.Min(value.Length, length);
            var buf = Encoding.Default.GetBytes(value);
            for (int i = 0; i < min; i++)
                Write(buf[i]);

            ZeroFill(length - min);
        }
        public void Write(params string[] value)
        {
            Write((byte)value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                var str = value[i];
                if (string.IsNullOrEmpty(str))
                {
                    Write((byte)0);
                    continue;
                }

                Write((byte)str.Length);
                Write(str, str.Length);
            }
        }
        public void WriteUnsafe(void* buf, int length)
        {
            if (Position + length >= MAX_SIZE)
                return;
            memcpy(stream, buf, length);
            stream += length;
        }
        public void ZeroFill(int amount)
        {
            if (Position + amount >= MAX_SIZE)
                return;
            memset(stream, 0, amount);
            stream += amount;
        }
        public string[] ReadStringList()
        {
            var result = new string[ReadUInt8()];
            for (int i = 0; i < result.Length; i++)
                result[i] = ReadCString(ReadUInt8());
            return result;
        }
        public void ReadUnsafe(void* buf, int length)
        {
            if (Position + length >= MAX_SIZE)
                return;
            memcpy(buf, stream, length);
            stream += length;
        }
        public byte[] ReadBytes(int size)
        {
            byte[] res = new byte[size];
            for (int i = 0; i < res.Length; i++)
                res[i] = ReadUInt8();
            return res;
        }
        public string ReadCString(int size)
        {
            if (Position + size >= MAX_SIZE)
                return "";
            string result = new string((sbyte*)this.stream, 0, size);
            stream += size;
            int idx = result.IndexOf('\0');
            return (idx > -1) ? result.Substring(0, idx) : result;
        }
        public byte ReadUInt8()
        {
            if (Position + 1 >= MAX_SIZE)
                return 0;
            var result = *stream;
            stream++;
            return result;
        }
        public ushort ReadUInt16()
        {
            if (Position + 2 >= MAX_SIZE)
                return 0;
            var result = *((ushort*)stream);
            stream += sizeof(ushort);
            return result;
        }
        public uint ReadUInt32()
        {
            if (Position + 4 >= MAX_SIZE)
                return 0;
            var result = *((uint*)stream);
            stream += sizeof(uint);
            return result;
        }
        public ulong ReadUInt64()
        {
            if (Position + 8 >= MAX_SIZE)
                return 0;
            var result = *((ulong*)stream);
            stream += sizeof(ulong);
            return result;
        }
        public sbyte ReadInt8() { return (sbyte)ReadUInt8(); }
        public short ReadInt16() { return (short)ReadUInt16(); }
        public int ReadInt32() { return (int)ReadUInt32(); }
        public long ReadInt64() { return (long)ReadUInt64(); }

        public void Finalize(ushort type)
        {
            if (SealSize > 0)
            {
                WriteSeal();
            }

            this.Size = this.Position;
            Seek(0);
            Write((ushort)(this.Size - SealSize));
            Write((ushort)type);
        }


        public void WriteSeal()
        {
            Write(SealString, TQ_SEALSIZE); // NOTE: must be 8 chars
        }
        private static string CreatePacketStringWithNumbers(byte* b, int len)
        {
            //
            // -- Taken from ConquerAI
            // Special Thanks: John
            //

            int msgSize = (len) * 4 //4 chars, 2 for number, 1 for white space, 1 for letter
                + ((len / 16) + 1) * 9; //1 for /t and 2 for new line/ret, 3 for number, 2 for [ ] and 1 for space
            StringBuilder hex = new StringBuilder(msgSize);
            for (int i = 0; i < len; i += 16)
            {
                hex.AppendFormat("[{0:000}] ", i);
                for (int z = i; z < i + 16; z++)
                {
                    if (z >= len) hex.Append("   ");
                    else
                        hex.AppendFormat("{0:x2} ", b[z]);
                }
                hex.Append('\t');
                for (int z = i; z < i + 16; z++)
                {
                    if (z >= len) hex.Append(" ");
                    else
                    {
                        if (b[z] > 32 && b[z] < 127)
                        {
                            hex.AppendFormat("{0}", (char)b[z]);
                        }
                        else
                        {
                            hex.Append('.');
                        }
                    }
                }
                hex.Append("\r\n");
            }
            return hex.ToString();
        }
        public static string Dump(byte[] b)
        {
            fixed (byte* ptr = b)
                return CreatePacketStringWithNumbers(ptr, b.Length);
        }
        public string Dump(string header)
        {
            int offset = Position; Seek(2);
            int type = ReadUInt16(); Seek(offset);

            string str = "Packet (" + header + ") - Type: " + type + " Size - " + Size + "\r\n";
            str += CreatePacketStringWithNumbers(this.Memory, this.Size);
            return str;
        }

        public unsafe void memcpy(void* dest, void* src, Int32 size)
        {
            Int32 count = size / sizeof(long);
            for (Int32 i = 0; i < count; i++)
                *(((long*)dest) + i) = *(((long*)src) + i);

            Int32 pos = size - (size % sizeof(long));
            for (Int32 i = 0; i < size % sizeof(long); i++)
                *(((Byte*)dest) + pos + i) = *(((Byte*)src) + pos + i);
        }

        public void WriteStringWithLength(string value)
        {
            Write((byte)value.Length);
            Write(value, value.Length);
        }
        public void WriteStringList(List<string> value)
        {
            Write((byte)value.Count);
            for (int i = 0; i < value.Count; i++)
                Write(value[i]);
        }
    }
}
