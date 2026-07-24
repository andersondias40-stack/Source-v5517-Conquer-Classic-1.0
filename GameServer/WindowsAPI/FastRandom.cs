
namespace System
{
    public class FastRandom
    {
        const double REAL_UNIT_INT = 1.0 / ((double)int.MaxValue + 1.0);
        const double REAL_UNIT_UINT = 1.0 / ((double)uint.MaxValue + 1.0);
        const uint Y = 842502087, Z = 3579807591, W = 273326509;

        private uint x, y, z, w;
        private object SyncRoot;

        public FastRandom()
            : this(Time32.Now.GetHashCode())
        {

        }
        public FastRandom(int seed)
        {
            SyncRoot = new object();
            Reinitialise(seed);
        }

        public void Reinitialise(int seed)
        {
            lock (SyncRoot)
            {
                x = (uint)seed;
                y = Y;
                z = Z;
                w = W;
            }
        }
        public int Next()
        {
            lock (SyncRoot)
            {
                uint t = (x ^ (x << 11));
                x = y; y = z; z = w;
                w = (w ^ (w >> 19)) ^ (t ^ (t >> 8));

                uint rtn = w & 0x7FFFFFFF;
                if (rtn == 0x7FFFFFFF) return Next();
                return (int)rtn;
            }
        }
        public int Next(int upperBound)
        {
            lock (SyncRoot)
            {
                if (upperBound < 0) upperBound = 0;

                uint t = (x ^ (x << 11));
                x = y; y = z; z = w;
                return (int)((REAL_UNIT_INT * (int)(0x7FFFFFFF & (w = (w ^ (w >> 19)) ^ (t ^ (t >> 8))))) * upperBound);
            }
        }
        public int Sign()
        {
            int next = Next(0, 2);
            if (next == 0) return -1;
            return 1;
        }
        public int Next(int lowerBound, int upperBound)
        {
            lock (SyncRoot)
            {
                if (lowerBound > upperBound)
                {
                    int aux = lowerBound;
                    lowerBound = upperBound;
                    upperBound = aux;
                }
                uint t = (x ^ (x << 11));
                x = y; y = z; z = w;
                int range = upperBound - lowerBound;
                if (range < 0)
                {
                    return lowerBound + (int)((REAL_UNIT_UINT * (double)(w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)))) * (double)((long)upperBound - (long)lowerBound));
                }
                return lowerBound + (int)((REAL_UNIT_INT * (double)(int)(0x7FFFFFFF & (w = (w ^ (w >> 19)) ^ (t ^ (t >> 8))))) * (double)range);
            }
        }
        public double NextDouble()
        {
            lock (SyncRoot)
            {
                uint t = (x ^ (x << 11));
                x = y; y = z; z = w;
                return (REAL_UNIT_INT * (int)(0x7FFFFFFF & (w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)))));
            }
        }
        public unsafe void NextBytes(byte[] buffer)
        {
            lock (SyncRoot)
            {
                if (buffer.Length % 8 != 0)
                {
                    new Random().NextBytes(buffer);
                    return;
                }

                uint x = this.x, y = this.y, z = this.z, w = this.w;

                fixed (byte* pByte0 = buffer)
                {
                    uint* pDWord = (uint*)pByte0;
                    for (int i = 0, len = buffer.Length >> 2; i < len; i += 2)
                    {
                        uint t = (x ^ (x << 11));
                        x = y; y = z; z = w;
                        pDWord[i] = w = (w ^ (w >> 19)) ^ (t ^ (t >> 8));

                        t = (x ^ (x << 11));
                        x = y; y = z; z = w;
                        pDWord[i + 1] = w = (w ^ (w >> 19)) ^ (t ^ (t >> 8));
                    }
                }
            }
        }
        public uint NextUInt()
        {
            lock (SyncRoot)
            {
                uint t = (x ^ (x << 11));
                x = y; y = z; z = w;
                return (w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)));
            }
        }
        public int NextInt()
        {
            lock (SyncRoot)
            {
                uint t = (x ^ (x << 11));
                x = y; y = z; z = w;
                return (int)(0x7FFFFFFF & (w = (w ^ (w >> 19)) ^ (t ^ (t >> 8))));
            }
        }
    }
}
