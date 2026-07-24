using System;
using System.Collections.Generic;
using System.Text;

namespace COServer.Game.MsgServer.AttackHandler.Algoritms
{
    public class Misc
    {
        public static bool ChanceSuccess(double percent)
        {
            return ((percent * 10000.0) >= Program.GetRandom.Next(0xf4240));
        }
        private static sbyte[] _walkXCoords = new sbyte[8]
    {
      (sbyte) 0,
      (sbyte) -1,
      (sbyte) -1,
      (sbyte) -1,
      (sbyte) 0,
      (sbyte) 1,
      (sbyte) 1,
      (sbyte) 1
    };
        private static sbyte[] _walkYCoords = new sbyte[8]
    {
      (sbyte) 1,
      (sbyte) 1,
      (sbyte) 0,
      (sbyte) -1,
      (sbyte) -1,
      (sbyte) -1,
      (sbyte) 0,
      (sbyte) 1
    };
        private static sbyte[] _rideXCoords = new sbyte[24]
    {
      (sbyte) 0,
      (sbyte) -2,
      (sbyte) -2,
      (sbyte) -2,
      (sbyte) 0,
      (sbyte) 2,
      (sbyte) 2,
      (sbyte) 2,
      (sbyte) -1,
      (sbyte) -2,
      (sbyte) -2,
      (sbyte) -1,
      (sbyte) 1,
      (sbyte) 2,
      (sbyte) 2,
      (sbyte) 1,
      (sbyte) -1,
      (sbyte) -2,
      (sbyte) -2,
      (sbyte) -1,
      (sbyte) 1,
      (sbyte) 2,
      (sbyte) 2,
      (sbyte) 1
    };
        private static sbyte[] _rideYCoords = new sbyte[24]
    {
      (sbyte) 2,
      (sbyte) 2,
      (sbyte) 0,
      (sbyte) -2,
      (sbyte) -2,
      (sbyte) -2,
      (sbyte) 0,
      (sbyte) 2,
      (sbyte) 2,
      (sbyte) 1,
      (sbyte) -1,
      (sbyte) -2,
      (sbyte) -2,
      (sbyte) -1,
      (sbyte) 1,
      (sbyte) 2,
      (sbyte) 2,
      (sbyte) 1,
      (sbyte) -1,
      (sbyte) -2,
      (sbyte) -2,
      (sbyte) -1,
      (sbyte) 1,
      (sbyte) 2
    };

        static Misc()
        {
        }

        public static int JumpSpeed(int distance, ushort movespeed = (ushort)400)
        {
            return (int)Math.Floor((double)movespeed * Math.Floor((double)distance / 10.0));
        }

        public static int Distance(ushort x, ushort y, ushort x2, ushort y2)
        {
            return Math.Max(Math.Abs((int)x - (int)x2), Math.Abs((int)y - (int)y2));
        }

        public static byte Angle(ushort x, ushort y, ushort x2, ushort y2)
        {
            return (byte)((uint)(byte)(7.0 - Math.Floor(Misc.PointDirecton((double)x, (double)y, (double)x2, (double)y2) / 45.0 % 8.0) - 1.0) % 8U);
        }

        public static double PointDirecton(double x1, double y1, double x2, double y2)
        {
            double x = x2 - x1;
            double num = Math.Atan2(y2 - y1, x);
            if (num < 0.0)
                num += 2.0 * Math.PI;
            return Math.Ceiling(360.0 - num * 180.0 / Math.PI);
        }


        public static void IncXY(byte Facing, ref ushort x, ref ushort y, bool Riding = false)
        {
            sbyte num1;
            sbyte num2;
            if (!Riding)
            {
                num1 = Misc._walkXCoords[(int)Facing % 8];
                num2 = Misc._walkYCoords[(int)Facing % 8];
            }
            else
            {
                num1 = Misc._rideXCoords[(int)Facing % 24];
                num2 = Misc._rideYCoords[(int)Facing % 24];
            }
            x = (ushort)((uint)x + (uint)num1);
            y = (ushort)((uint)y + (uint)num2);
        }


    }

}
