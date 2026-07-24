using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public class BitVector32
    {
        public uint[] bits;

        public int Size => 32 * bits.Length;
        public BitVector32(int BitCount) {

            int sections = BitCount / 32;
            if (BitCount % 32 != 0)
                sections += 1;
            bits = new uint[sections];
        }
        public void Add(int index)
        {
            if (index < Size)
            {
                int id = index / 32;
                uint bites = (uint)(1 << (index % 32));
                bits[id] |= bites;
            }
        }
        public void Remove(int index)
        {
            if (index < Size)
            {
                int id = index / 32;
                uint bites = (uint)(1 << (index % 32));
                bits[id] &= ~bites;
            }
        }
        public bool Contain(int index)
        {
            if (index > Size)
                return false;
            int id = index / 32;
            uint bites = (uint)(1 << (index % 32));
            return (bits[id] & bites) == bites;
        }
        public int Count()
        {
            int num = 0;
            for (int i = 0; i < Size / 32; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    if ((bits[i] & (1 << j)) == 1 << j)
                        num++;
                }
            }
            return num;
        }

        public void Clear()
        {
            ushort num = (byte)(Size / 32);
            for (int i = 0; i < num; i++)
                bits[i] = 0;
        }
    }
}
