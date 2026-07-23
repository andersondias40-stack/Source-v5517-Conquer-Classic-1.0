namespace System
{
    public class FastRandom
    {
        private readonly object _object;
        private uint _uint0;
        private uint _uint1;
        private uint _uint2;
        private uint _uint3;

        public FastRandom() : this(Time32.Now.TotalMilliseconds)
        {
        }

        public FastRandom(int seed)
        {
            _object = new object();
            Reinitialise(seed);
        }

        public int Next()
        {
            lock (_object)
            {
                uint num = _uint0 ^ (_uint0 << 11);
                _uint0 = _uint1;
                _uint1 = _uint2;
                _uint2 = _uint3;
                _uint3 = (_uint3 ^ (_uint3 >> 0x13)) ^ (num ^ (num >> 8));
                uint num2 = _uint3 & 0x7fffffff;
                if (num2 == 0x7fffffff)
                {
                    return Next();
                }
                return (int)num2;
            }
        }

        public int Next(int upperBound)
        {
            lock (_object)
            {
                if (upperBound < 0)
                {
                    upperBound = 0;
                }
                uint num = _uint0 ^ (_uint0 << 11);
                _uint0 = _uint1;
                _uint1 = _uint2;
                _uint2 = _uint3;
                return (int)((4.6566128730773926E-10 * (0x7fffffff & (_uint3 = (_uint3 ^ (_uint3 >> 0x13)) ^ (num ^ (num >> 8))))) * upperBound);
            }
        }

        public int Next(int lowerBound, int upperBound)
        {
            lock (_object)
            {
                if (lowerBound > upperBound)
                {
                    int num = lowerBound;
                    lowerBound = upperBound;
                    upperBound = num;
                }
                uint num2 = _uint0 ^ (_uint0 << 11);
                _uint0 = _uint1;
                _uint1 = _uint2;
                _uint2 = _uint3;
                int num3 = upperBound - lowerBound;
                if (num3 < 0)
                {
                    return (lowerBound + ((int)((2.3283064365386963E-10 * (_uint3 = (_uint3 ^ (_uint3 >> 0x13)) ^ (num2 ^ (num2 >> 8)))) * (upperBound - lowerBound))));
                }
                return (lowerBound + ((int)((4.6566128730773926E-10 * (0x7fffffff & (_uint3 = (_uint3 ^ (_uint3 >> 0x13)) ^ (num2 ^ (num2 >> 8))))) * num3)));
            }
        }

        public unsafe void NextBytes(byte[] buffer)
        {
            lock (_object)
            {
                if ((buffer.Length % 8) != 0)
                {
                    new Random().NextBytes(buffer);
                }
                else
                {
                    uint num = _uint0;
                    uint num2 = _uint1;
                    uint num3 = _uint2;
                    uint num4 = _uint3;
                    fixed (byte* numRef = buffer)
                    {
                        uint* numPtr = (uint*)numRef;
                        int index = 0;
                        int num6 = buffer.Length >> 2;
                        while (index < num6)
                        {
                            uint num7 = num ^ (num << 11);
                            num = num2;
                            num2 = num3;
                            num3 = num4;
                            numPtr[index] = num4 = (num4 ^ (num4 >> 0x13)) ^ (num7 ^ (num7 >> 8));
                            num7 = num ^ (num << 11);
                            num = num2;
                            num2 = num3;
                            num3 = num4;
                            numPtr[index + 1] = num4 = (num4 ^ (num4 >> 0x13)) ^ (num7 ^ (num7 >> 8));
                            index += 2;
                        }
                    }
                }
            }
        }

        public double NextDouble()
        {
            lock (_object)
            {
                uint num = _uint0 ^ (_uint0 << 11);
                _uint0 = _uint1;
                _uint1 = _uint2;
                _uint2 = _uint3;
                return (4.6566128730773926E-10 * (0x7fffffff & (_uint3 = (_uint3 ^ (_uint3 >> 0x13)) ^ (num ^ (num >> 8)))));
            }
        }

        public int NextInt()
        {
            lock (_object)
            {
                uint num = _uint0 ^ (_uint0 << 11);
                _uint0 = _uint1;
                _uint1 = _uint2;
                _uint2 = _uint3;
                return (0x7fffffff & ((int)(_uint3 = (_uint3 ^ (_uint3 >> 0x13)) ^ (num ^ (num >> 8)))));
            }
        }

        public uint NextUInt()
        {
            lock (_object)
            {
                uint num = _uint0 ^ (_uint0 << 11);
                _uint0 = _uint1;
                _uint1 = _uint2;
                _uint2 = _uint3;
                return (_uint3 = (_uint3 ^ (_uint3 >> 0x13)) ^ (num ^ (num >> 8)));
            }
        }

        public void Reinitialise(int seed)
        {
            lock (_object)
            {
                _uint0 = (uint)seed;
                _uint1 = 0x32378fc7;
                _uint2 = 0xd55f8767;
                _uint3 = 0x104aa1ad;
            }
        }

        public int Sign()
        {
            if (Next(0, 2) == 0)
            {
                return -1;
            }
            return 1;
        }
    }
}

