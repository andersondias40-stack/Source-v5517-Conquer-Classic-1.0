//using System;
//using System.Collections.Generic;

//namespace COServer.Game.MsgServer.AttackHandler.Algoritms
//{
//    public class MoveCoords
//    {
//        public static bool InRange(ushort X, ushort Y, byte Range, List<InLineAlgorithm.coords> bas)
//        {
//            foreach (InLineAlgorithm.coords line in bas)
//            {
//                byte distance = (byte)InLineAlgorithm.GetDistance((ushort)X, (ushort)Y, (ushort)line.X, (ushort)line.Y);
//                if (distance <= Range)
//                    return true;
//            }
//            return false;
//        }
//    }
//}
