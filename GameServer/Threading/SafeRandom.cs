using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public class SafeRandom
    {
        private Random Rand;
        public object SyncRoot;

        public SafeRandom(int seed = 0)
        {
            SyncRoot = new object();
            if (seed != 0)
                Rand = new Random(seed);
            else
                Rand = new Random();
        }
        public int Next(int minval, int maxval)
        {
            lock (SyncRoot)
            {
                return Rand.Next(minval, maxval);
            }
        }
        public int Next(int maxval)
        {
            lock (SyncRoot)
            {
                return Rand.Next(maxval);
            }
        }
        public int Next()
        {
            lock (SyncRoot)
            {
                return Rand.Next();
            }
        }
        public void SetSeed(int seed)
        {
            lock (SyncRoot)
            {
                Rand = new Random();
            }
        }
    }
}
