namespace System
{// CPTSKY ALL RIGHT RECEIVED
    public class MSRandom
    {
        private const int Rand_Max = 0x7FFF;
        private uint Seed = 1;
        public MSRandom()
        {
            Seed = 1;
        }
        public MSRandom(uint seed)
        {
            Seed = seed;
        }
        public int Next()
        {
            return ((int)((Seed = Seed * 0x343FD + 0x269EC3) >> 16) & Rand_Max);
        }
    }
}
