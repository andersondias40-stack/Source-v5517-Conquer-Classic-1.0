using System;
using System.Runtime.InteropServices;

namespace COServer
{
    public static class BaseFunc
    {
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int64 srand(UInt64 seed);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int64 rand();
        public static double PointDirecton(double x1, double y1, double x2, double y2)
        {
            double direction = 0;

            double AddX = x2 - x1;
            double AddY = y2 - y1;
            double r = (double)Math.Atan2(AddY, AddX);

            if (r < 0) r += (double)Math.PI * 2;

            direction = 360 - (r * 180 / (double)Math.PI);
            return direction;
        }
        public static int RandGet(int nMax, bool bRealRand = false)
        {
            if (nMax <= 0)
                nMax = 1;

            if (bRealRand)
                srand(Time32.Now.Value);
            long val = rand();
            return (int)(val % nMax);
        }

        //////////////////////////////////////////////////////////////////////
        public static double RandomRateGet(double dRange)
        {
            double pi = 3.1415926;

            int nRandom = RandGet(999, true) + 1;
            double a = Math.Sin(nRandom * pi / 1000);
            double b;
            if (nRandom >= 90)
                b = (1.0 + dRange) - Math.Sqrt(Math.Sqrt(a)) * dRange;
            else
                b = (1.0 - dRange) + Math.Sqrt(Math.Sqrt(a)) * dRange;

            return b;
        }

    }
}
