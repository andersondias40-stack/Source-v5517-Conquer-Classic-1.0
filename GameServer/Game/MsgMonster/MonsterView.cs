using System;
using System.Collections.Generic;
using System.Linq;
namespace COServer.Game.MsgMonster
{
    public class MonsterView
    {
        public const int ViewThreshold = 18;



        private MonsterRole role;
        public MonsterView(MonsterRole _role)
        {
            role = _role;

        }
        /// <summary>
        /// make sure the collection
        /// if all its in screen ( -> ViewThreshold )
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Role.IMapObj> Roles(Role.GameMap map, Role.MapObjectType typ)
        {
            return map.View.Roles(typ, role.X, role.Y, p => CanSee(p));
        }
        public Role.IMapObj GetTarget(Role.GameMap map, Role.MapObjectType typ)
        {
            var array = map.View.Roles(typ, role.X, role.Y, p => CanSee(p) && p.ObjType == Role.MapObjectType.Player);
            if (array.Count() > 0)
                return array.OrderByDescending(p => p.IndexInScreen).FirstOrDefault();
            else
                return null;
            //return map.View.Roles(typ, role.X, role.Y, p => CanSee(p) && p.ObjType == Role.MapObjectType.Player).OrderByDescending(p => p.IndexInScreen).First();
        }
        public unsafe void SendScreen(ServerSockets.Packet msg, Role.GameMap map)
        {
            foreach (var obj in Roles(map, Role.MapObjectType.Player))
            {
                obj.Send(msg);
            }
        }


        public bool CanSee(Role.IMapObj obj)
        {
            if (obj.Map != role.Map)
                return false;
            if (obj.DynamicID != role.DynamicID)
                return false;
            if (obj.UID == role.UID)
                return false;
            return GetDistance(obj.X, obj.Y, role.X, role.Y) <= ViewThreshold;
        }

        public static short GetDistance(ushort X, ushort Y, ushort X2, ushort Y2)
        {
            return (short)Math.Sqrt((X - X2) * (X - X2) + (Y - Y2) * (Y - Y2));

            //short x = 0;
            //short y = 0;
            //if (X >= X2) x = (short)(X - X2);
            //else if (X2 >= X) x = (short)(X2 - X);
            //if (Y >= Y2) y = (short)(Y - Y2);
            //else if (Y2 >= Y) y = (short)(Y2 - Y);
            //if (x > y) return x;
            //else return y;
        }



        internal void CheckScren()
        {

        }
    }
}
