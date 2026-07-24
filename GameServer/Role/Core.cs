using COServer.Game.MsgServer;

using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace COServer.Role
{
    public static class Core
    {
        public static Boolean CanSee(Double x1, Double y1, Double x2, Double y2, Int32 Range)
        {
            if (Math.Abs(x2 - x1) > Range || Math.Abs(y2 - y1) > Range)
                return false;
            return true;
        }
        public static void SendGlobalMessage(ServerSockets.Packet stream, string Message, MsgMessage.ChatMode type = MsgMessage.ChatMode.System, MsgMessage.MsgColor color = MsgMessage.MsgColor.red)
        {
            Program.SendGlobalPackets.Enqueue(new MsgMessage(Message, color, MsgMessage.ChatMode.System).GetArray(stream));
        }
        public static bool IsBoy(uint mesh) { return (mesh == 1003 || mesh == 1004); }
        public static bool IsGirl(uint mesh) { return (mesh == 2001 || mesh == 2002); }

        internal static int CreateTimer(int year, int month, int day)
        {
            int Timer = year * 10000 + month * 100 + day;
            return Timer;
        }
        internal static int CreateTimer(DateTime timer)
        {
            int Timer = timer.Year * 10000 + timer.Month * 100 + timer.Day;
            return Timer;
        }
        internal static DateTime GetTimer(int Timer)
        {
            int Year = Timer / 10000;
            int Month = (Timer / 100) - Year * 100;
            int Day = Timer - (Year * 10000) - (Month * 100);
            return new DateTime(Year, Month, Day);
        }

        internal static ulong TqTimer(DateTime timer)
        {
            var year = (ulong)(10000000000000 * (ulong)(timer.Year - 1900));
            var month = (ulong)(100000000000 * (ulong)(timer.Month - 1));
            var dayofyear = (ulong)(100000000 * (ulong)(timer.DayOfYear - 1));
            var day = (ulong)(timer.Day * 1000000);
            var Hour = (ulong)(timer.Hour * 10000);
            var Minute = (ulong)(timer.Minute * 100);
            var Second = (ulong)(timer.Second);

            return (ulong)(year + month + dayofyear + day + Hour + Minute + Second);
        }
        public static bool Rate(double percent)
        {

            ushort testone = 0;
            if (percent == 0) return false;
            while ((int)percent > 0)
            {
                testone++;
                percent /= 10f;
                if (testone > 300)
                {
                    Console.WriteLine("Problem While in Kernel");
                    return true;
                }
            }
            int discriminant = 1;
            percent = Math.Round(percent, 4);
            testone = 0;
            while (percent != Math.Ceiling(percent))
            {
                percent *= 10;
                discriminant *= 10;
                percent = Math.Round(percent, 4);
                testone++;
                if (testone > 300)
                {
                    Console.WriteLine("Problem While in Kernel 2");
                    return true;
                }
            }
            return Rate((int)percent, discriminant);
        }

        public static bool Rate(int value, int discriminant)
        {
            int rate = Program.GetRandom.Next() % discriminant;

            return value > rate;
        }
        public static bool PercentSuccess(double _chance)
        {
            return Program.GetRandom.NextDouble() * 100 < _chance;
        }
        public static uint MulDiv(uint number, uint numerator, uint denominator)
        {
            return (number * numerator + denominator / 2) / denominator;
        }
        public static int GetJumpMiliSeconds(short Distance)
        {
            return Distance * 25;
        }
        public static void IncXY(Flags.ConquerAngle Facing, ref ushort x, ref ushort y)
        {
            sbyte xi, yi;
            xi = yi = 0;
            switch (Facing)
            {
                case Flags.ConquerAngle.North: xi = -1; yi = -1; break;
                case Flags.ConquerAngle.South: xi = 1; yi = 1; break;
                case Flags.ConquerAngle.East: xi = 1; yi = -1; break;
                case Flags.ConquerAngle.West: xi = -1; yi = 1; break;
                case Flags.ConquerAngle.NorthWest: xi = -1; break;
                case Flags.ConquerAngle.SouthWest: yi = 1; break;
                case Flags.ConquerAngle.NorthEast: yi = -1; break;
                case Flags.ConquerAngle.SouthEast: xi = 1; break;
            }
            x = (ushort)(x + xi);
            y = (ushort)(y + yi);
        }
        public static FastRandom Random = new FastRandom();
        public static short GetDistance(ushort x1, ushort y1, ushort x2, ushort y2)// forscreen
        {
           return (short)Math.Max(Math.Abs(x1 - x2), Math.Abs(y1 - y2));

          //  return (short)Math.Round(Math.Sqrt(((x1 - x2) * (x1 - x2)) + ((y1 - y2) * (y1 - y2)) * 1.0));
        }
        public static double GetRadian(float posSourX, float posSourY, float posTargetX, float posTargetY)
        {
            float PI = 3.1415926535f;
            float fDeltaX = posTargetX - posSourX;
            float fDeltaY = posTargetY - posSourY;
            float fDistance = SquareRootFloat(fDeltaX * fDeltaX + fDeltaY * fDeltaY);

            double fRadian = Math.Asin(fDeltaX / fDistance);

            return fDeltaY > 0 ? (PI / 2 - fRadian) : (PI + fRadian + PI / 2);
        }
        unsafe static float SquareRootFloat(float number)
        {
            long i;
            float x, y;
            const float f = 1.5F;

            x = number * 0.5F;
            y = number;
            i = *(long*)&y;
            i = 0x5f3759df - (i >> 1);
            y = *(float*)&i;
            y = y * (f - (x * y * y));
            y = y * (f - (x * y * y));
            return number * y;
        }
        public static Flags.ConquerAngle GetAngle(ushort X, ushort Y, ushort X2, ushort Y2)
        {
            double direction = 0;

            double AddX = X2 - X;
            double AddY = Y2 - Y;
            double r = (double)Math.Atan2(AddY, AddX);

            if (r < 0) r += (double)Math.PI * 2;

            direction = 360 - (r * 180 / (double)Math.PI);

            byte Dir = (byte)((7 - (Math.Floor(direction) / 45 % 8)) - 1 % 8);
            return (Flags.ConquerAngle)(byte)((int)Dir % 8);
        }
        public static bool Rate(int value)
        {
            return value > Program.GetRandom.Next() % 100;
        }
        public static bool RateDouble(double Chance)
        {
            return Program.GetRandom.NextDouble() < (Chance / 100.0);
        }
        public static bool RateDouble2(double Chance)
        {
            return new Random().NextDouble() < (Chance / 100.0);
        }
        public static bool ChanceCalc(double chance)
        {
            const int DIVISOR_I = 10000;
            const int MAX_VALUE = 100 * DIVISOR_I;
            try
            {
                return Program.GetRandom.Next(0, MAX_VALUE) <= chance * DIVISOR_I;
            }
            catch (Exception ex)
            {
                Console.WriteLine("chanceCalc " + ex);
                return false;
            }
        }

        public static unsafe void memcpy(void* dest, void* src, Int32 size)
        {
            Int32 count = size / sizeof(long);
            for (Int32 i = 0; i < count; i++)
                *(((long*)dest) + i) = *(((long*)src) + i);

            Int32 pos = size - (size % sizeof(long));
            for (Int32 i = 0; i < size % sizeof(long); i++)
                *(((Byte*)dest) + pos + i) = *(((Byte*)src) + pos + i);
        }
        public static int i32Direction(int x1, int y1, int x2, int y2)
        {
            int nx = x1 - x2;
            int ny = y1 - y2;
            if (ny == 0)
            {
                if (nx <= 0)
                    return 6;
                else if (ny >= 0)
                    return 2;
            }
            else if (nx == 0)
            {
                if (ny <= 0)
                    return 0;
                else if (ny >= 0)
                    return 4;
            }
            else
            {
                if (nx < 0)
                {
                    if (ny < 0)
                        return 7;
                    else //if (ny > 0)
                        return 5;
                }
                else if (nx > 0)
                {
                    if (ny > 0)
                        return 3;
                    else //if (ny < 0)
                        return 1;
                }
            }
            return 0;
        }

        public static BigInteger GeneratorLucky(BigInteger min, BigInteger max)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                return RandomInRange(rng, min, max);
            }
        }
        public static BigInteger RandomInRange(RandomNumberGenerator rng, BigInteger min, BigInteger max)
        {
            if (min > max)
            {
                var buff = min;
                min = max;
                max = buff;
            }

            // offset to set min = 0
            BigInteger offset = -min;
            min = 0;
            max += offset;

            var value = randomInRangeFromZeroToPositive(rng, max) - offset;
            return value;
        }

        private static BigInteger randomInRangeFromZeroToPositive(RandomNumberGenerator rng, BigInteger max)
        {
            BigInteger value;
            var bytes = max.ToByteArray();

            // count how many bits of the most significant byte are 0
            // NOTE: sign bit is always 0 because `max` must always be positive
            byte zeroBitsMask = 0b00000000;

            var mostSignificantByte = bytes[bytes.Length - 1];

            // we try to set to 0 as many bits as there are in the most significant byte, starting from the left (most significant bits first)
            // NOTE: `i` starts from 7 because the sign bit is always 0
            for (var i = 7; i >= 0; i--)
            {
                // we keep iterating until we find the most significant non-0 bit
                if ((mostSignificantByte & (0b1 << i)) != 0)
                {
                    var zeroBits = 7 - i;
                    zeroBitsMask = (byte)(0b11111111 >> zeroBits);
                    break;
                }
            }

            do
            {
                rng.GetBytes(bytes);

                // set most significant bits to 0 (because `value > max` if any of these bits is 1)
                bytes[bytes.Length - 1] &= zeroBitsMask;

                value = new BigInteger(bytes);

                // `value > max` 50% of the times, in which case the fastest way to keep the distribution uniform is to try again
            } while (value > max);

            return value;
        }

        public static double GetE2DDistance(int X, int Y, int X2, int Y2)
        {
            int x = Math.Abs(X - X2);
            int y = Math.Abs(Y - Y2);
            return Math.Sqrt((x * x) + (y * y));
        }
        public static double GetJDistance(int px, int py, int X, int Y)
        {
            //return __max(abs(GetPosX() - x), abs(GetPosY() - y)); int x = Math.Abs(X - X2);
            return Math.Abs((px * X) + (py * Y));
        }

        /// <summary> This function returns true if an object is within the bounds of another object's screen. </summary>
        /// <param name="x1">The first object's x coordinate.</param>
        /// <param name="y1">The first object's y coordinate.</param>
        /// <param name="x2">The second object's x coordinate.</param>
        /// <param name="y2">The second object's y coordinate.</param>
        public static bool InScreen(ushort x1, ushort y1, ushort x2, ushort y2)
        {
            return Math.Abs(x1 - x2) <= 18 && Math.Abs(y1 - y2) <= 18;
            //GetDistance(x1, y1, x2, y2) <= SCREEN_DISTANCE;
            //Math.Abs(x1 - x2) <= SCREEN_DISTANCE && Math.Abs(y1 - y2) <= SCREEN_DISTANCE;
        }
    }
}
