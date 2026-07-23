using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaTrixAICore
{
    public class MathHelper
    {
        public static int BitFold32(int lower16, int higher16)
        {
            return (lower16) | (higher16 << 16);
        }
        public static void BitUnfold32(int bits32, out int lower16, out int upper16)
        {
            lower16 = (int)(bits32 & UInt16.MaxValue);
            upper16 = (int)(bits32 >> 16);
        }
        public static void BitUnfold64(ulong bits64, out int lower32, out int upper32)
        {
            lower32 = (int)(bits64 & UInt32.MaxValue);
            upper32 = (int)(bits64 >> 32);
        }
        public static int RoughDistance(int x1, int y1, int x2, int y2)
        {
            return Math.Max(Math.Abs(x1 - x2), Math.Abs(y1 - y2));
        }
        public static int ConquerDirection(int x1, int y1, int x2, int y2)
        {
            double angle = Math.Atan2(y2 - y1, x2 - x1);
            angle -= Math.PI / 2;

            if (angle < 0) angle += 2 * Math.PI;

            angle *= 8 / (2 * Math.PI);
            return (int)angle;
        }
        public static int MulDiv(int number, int numerator, int denominator)
        {
            return (number * numerator + denominator / 2) / denominator;
        }
        public static bool OverflowAdd(ref int acc, int add)
        {
            if (int.MaxValue - acc < add)
                return true;
            acc = Math.Max(acc + add, 0);
            return false;
        }
        public static int AdjustDataEx(int data, int adjust, int maxData = 0)
        {
            if (adjust >= StatusConstants.AdjustPercent)
                return MulDiv(data, adjust - StatusConstants.AdjustPercent, 100);

            if (adjust <= StatusConstants.AdjustSet)
                return -1 * adjust + StatusConstants.AdjustSet;

            if (adjust == StatusConstants.AdjustFull)
                return maxData;

            return data + adjust;
        }
    }
    public class StatusConstants
    {
        public const int AdjustSet = -30000,
                         AdjustFull = -32768,
                         AdjustPercent = 30000;
    }

}
