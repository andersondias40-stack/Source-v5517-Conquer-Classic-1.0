using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COServer
{
    public class SafeRandom
    {
        Random Rand = new Random();

        public SafeRandom(int seed = 0)
        {
            Rand = seed > 0 ? new Random(seed) : new Random();
        }
        public int Next()
        {
            return Rand.Next();
        }

        public int Next(int MaxVal)
        {
            return Rand.Next(MaxVal);
        }

        public byte Next(byte Minval, byte Maxval)
        {
            return Convert.ToByte(Rand.Next(Minval, Maxval));
        }
        public int Next(int Minval, int Maxval)
        {
            return Convert.ToInt32(Rand.Next(Minval, Maxval));
        }

        public uint Next(uint Minval, uint Maxval)
        {
            return Convert.ToUInt32(Rand.Next(Convert.ToInt32(Minval), Convert.ToInt32(Maxval)));
        }

        public sbyte Next(sbyte Minval, sbyte Maxval)
        {
            return Convert.ToSByte(Rand.Next(Minval, Maxval));
        }

        public short Next(short Minval, short Maxval)
        {
            return Convert.ToInt16(Rand.Next(Minval, Maxval));
        }

        public void NextBytes(byte[] buffer)
        {
            Rand.NextBytes(buffer);
        }

        public double NextDouble()
        {
            return Rand.NextDouble();
        }
        public int Sign()
        {
            if (this.Next(0, 2) == 0)
            {
                return -1;
            }
            return 1;
        }
    }
}
