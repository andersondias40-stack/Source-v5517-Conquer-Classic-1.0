using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace COServer.Game.MsgServer.AttackHandler.Algoritms
{
    public sealed class InLineAlgorithm
    {
        /// <summary>
        /// The algorithm type.
        /// </summary>
        public enum Algorithm
        {
            /// <summary>
            /// DDA (Digital differential analyzer.)
            /// </summary>
            DDA,
            /// <summary>
            /// Some math.
            /// </summary>
            SomeMath
        }

        /// <summary>
        /// A coordinate associated with the ILA algorithm.
        /// </summary>
        public struct ILACoordinate
        {
            /// <summary>
            /// The x coordinate.
            /// </summary>
            public int X;

            /// <summary>
            /// The y coordinate.
            /// </summary>
            public int Y;

            /// <summary>
            /// Creates a new ILA coordinate.
            /// </summary>
            /// <param name="x">The x coordinate.</param>
            /// <param name="y">The y coordinate.</param>
            public ILACoordinate(double x, double y)
            {
                X = (int)x;
                Y = (int)y;
            }
        }

        /// <summary>
        /// A list of all line coordinates.
        /// </summary>
        public List<coords> _lineCoordinates;
        /// 
        /// <summary>
        /// The algorithm type.
        /// </summary>
        private Algorithm _algorithm;

        /// <summary>
        /// Gets the max distance.
        /// </summary>
        public byte MaxDistance { get; private set; }

        /// <summary>
        /// Gets the x1 coordinate.
        /// </summary>
        public ushort X1 { get; private set; }

        /// <summary>
        /// Gets the y1 coordinate.
        /// </summary>
        public ushort Y1 { get; private set; }
        public List<coords> lcoords;

        /// <summary>
        /// Gets the x2 coordinate.
        /// </summary>
        public ushort X2 { get; private set; }

        /// <summary>
        /// Gets the y2 coordinate.
        /// </summary>
        public ushort Y2 { get; private set; }

        /// <summary>
        /// Gets the direction.
        /// </summary>
        public byte Direction { get; private set; }

        /// <summary>
        /// Checks whether a result of coordinates contains a coordinate.
        /// </summary>
        /// <param name="coords">The coordinates.</param>
        /// <param name="checkCoordinate">The coordinate to check.</param>
        /// <returns>True if the coordinates contains the coordinate.</returns>
        private bool Contains(List<coords> coords, coords checkCoordinate)
        {
            foreach (var coord in coords)
            {
                if (coord.X == checkCoordinate.X && checkCoordinate.Y == coord.Y)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Lines all coordinates.
        /// </summary>
        /// <param name="userx">The user x coordinate.</param>
        /// <param name="usery">The user y coordinate.</param>
        /// <param name="shotx">The shot x coordinate.</param>
        /// <param name="shoty">The shot y coordinate.</param>
        /// <returns>A list of all coordinates in line.</returns>
        List<coords> LineCoords(ushort userx, ushort usery, ushort shotx, ushort shoty)
        {
            return linedda(userx, usery, shotx, shoty);
        }

        /// <summary>
        /// Gets a list of all lines.
        /// </summary>
        /// <param name="xa">The xa.</param>
        /// <param name="ya">The ya.</param>
        /// <param name="xb">The xb.</param>
        /// <param name="yb">The yb.</param>
        /// <returns>A list of all coordinates.</returns>
        List<coords> linedda(int xa, int ya, int xb, int yb)
        /// 
        {
            int dx = xb - xa, dy = yb - ya, steps, k;
            float xincrement, yincrement, x = xa, y = ya;

            if (Math.Abs(dx) > Math.Abs(dy))
            {
                steps = Math.Abs(dx);
            }
            else
            {
                steps = Math.Abs(dy);
            }

            xincrement = dx / (float)steps;
            yincrement = dy / (float)steps;
            List<coords> thisLine = new List<coords>();
            thisLine.Add(new coords((int)Math.Round(x), (int)Math.Round(y)));

            for (k = 0; k < MaxDistance; k++)
            {
                x += xincrement;
                y += yincrement;

                thisLine.Add(new coords((int)Math.Round(x), (int)Math.Round(y)));

            }

            return thisLine;
        }
        private Role.GameMap map;

        private ushort SpellID = 0;
        /// <summary>
        /// Creates a new ILA algorithm.
        /// </summary>
        /// <param name="x1">The x1 coordinate.</param>
        /// <param name="x2">The x2 coordinate.</param>
        /// <param name="y1">The y1 coordinate.</param>
        /// <param name="y2">The y2 coordinate.</param>
        /// <param name="maxDistance">The max distance.</param>
        /// <param name="algorithm">The algorithm type.</param>
        public InLineAlgorithm(ushort x1, ushort x2, ushort y1, ushort y2, byte maxDistance = 10, Algorithm algorithm = Algorithm.DDA)
        {
            _algorithm = algorithm;
            this.X1 = x1;
            this.Y1 = y1;
            this.X2 = x2;
            this.Y2 = y2;
            this.MaxDistance = maxDistance;

            if (_algorithm == Algorithm.DDA)
            {
                _lineCoordinates = LineCoords(x1, y1, x2, y2);
            }

            Direction = (byte)Role.Core.GetAngle(x1, y1, x2, y2); ;
        }

        public InLineAlgorithm(ushort X1, ushort X2, ushort Y1, ushort Y2, Role.GameMap _map, byte MaxDistance, byte MaxRange, ushort spelldid = 0)
        {
            map = _map;

            this.X1 = X1;
            this.Y1 = Y1;
            this.X2 = X2;
            this.Y2 = Y2;

            this.MaxDistance = MaxDistance;
            SpellID = spelldid;
            lcoords = LineCoords(X1, Y1, X2, Y2);

        }
        public bool GetNewCoords(ref ushort X, ref ushort Y)
        {
            if (_lineCoordinates.Count > 0)
            {
                var coord = _lineCoordinates.Last();

                X = (ushort)coord.X;
                Y = (ushort)coord.Y;

                return true;
            }

            return false;
        }
        /// <summary>
        /// Checks whether a coordinate is in line.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <returns>True if the coordinate is in line.</returns>
        public bool InLine(ushort x, ushort y)
        {
            int mydst = (int)Role.Core.GetDistance((ushort)X1, (ushort)Y1, x, y);
            byte dir = (byte)Role.Core.GetAngle(X1, Y1, x, y);

            if (mydst <= MaxDistance)
            {
                if (_algorithm == Algorithm.SomeMath)
                {
                    if (dir != Direction)
                    {
                        return false;
                    }

                    //calculate line eq
                    if (X2 - X1 == 0)
                    {
                        //=> X - X1 = 0
                        //=> X = X1
                        return x == X1;
                    }
                    else if (Y2 - Y1 == 0)
                    {
                        //=> Y - Y1 = 0
                        //=> Y = Y1
                        return y == Y1;
                    }
                    else
                    {
                        double val1 = ((double)(x - X1)) / ((double)(X2 - X1));
                        double val2 = ((double)(y + Y1)) / ((double)(Y2 + Y1));
                        bool works = Math.Floor(val1) == Math.Floor(val2);

                        return works;
                    }
                }
                else if (_algorithm == Algorithm.DDA)
                {
                    return Contains(_lineCoordinates, new coords(x, y));

                }
            }

            return false;
        }
        public struct Point
        {
            public int X;
            public int Y;
            public Point(int x, int y) { X = x; Y = y; }
        }
        private static void DDALineEx(int x0, int y0, int x1, int y1, List<Point> vctPoint)
        {
            //vctPoint.Clear();
            if ((x0 != x1) || (y0 != y1))
            {
                int dx = x1 - x0;
                int dy = y1 - y0;
                int abs_dx = Math.Abs(dx);
                int abs_dy = Math.Abs(dy);
                if (abs_dx > abs_dy)
                {
                    int _0_5 = abs_dx * ((dy > 0) ? 1 : -1);
                    int numerator = dy * 2;
                    int denominator = abs_dx * 2;
                    if (dx > 0)
                    {
                        for (int i = 1; i <= abs_dx; i++)
                        {
                            Point point;
                            point.X = x0 + i;
                            point.Y = y0 + (((numerator * i) + _0_5) / denominator);
                            vctPoint.Add(point);
                        }
                    }
                    else if (dx < 0)
                    {
                        for (int i = 1; i <= abs_dx; i++)
                        {
                            Point point;
                            point.X = x0 - i;
                            point.Y = y0 + (((numerator * i) + _0_5) / denominator);
                            vctPoint.Add(point);
                        }
                    }
                }
                else
                {
                    int _0_5 = abs_dy * ((dx > 0) ? 1 : -1);
                    int numerator = dx * 2;
                    int denominator = abs_dy * 2;
                    if (dy > 0)
                    {
                        for (int i = 1; i <= abs_dy; i++)
                        {
                            Point point;
                            point.Y = y0 + i;
                            point.X = x0 + (((numerator * i) + _0_5) / denominator);
                            vctPoint.Add(point);
                        }
                    }
                    else if (dy < 0)
                    {
                        for (int i = 1; i <= abs_dy; i++)
                        {
                            Point point;
                            point.Y = y0 - i;
                            point.X = x0 + (((numerator * i) + _0_5) / denominator);
                            vctPoint.Add(point);
                        }
                    }
                }
            }
        }
        public static Point[] DDALine(ushort x0, ushort y0, ushort x1, ushort y1, byte nRange)
        {
            double dist = Role.Core.GetE2DDistance(x0, y0, x1, y1);
            List<Point> vctPoint = new List<Point>();
            if (dist <= nRange)
                vctPoint.Add(new Point(x1, y1));
            if (x0 != x1 && y0 != y1)
            {

                float scale = (float)(1.0f * nRange / Math.Sqrt(((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0))));
                x1 = (ushort)(0.5f + scale * (x1 - x0) + x0);
                y1 = (ushort)(0.5f + scale * (y1 - y0) + y0);
                DDALineEx(x0, y0, x1, y1, vctPoint);
            }
            return vctPoint.ToArray();

        }
    }
}
