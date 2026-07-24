using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public class Counter
    {
        public uint val;
        public uint Next => ++val;

        public uint Count => val;

        public Counter(uint start)
        {
            val = start;
        }
        public void Set(uint start)
        {
            val = start;
        }
    }
}
