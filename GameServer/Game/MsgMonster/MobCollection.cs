using System;
using System.Threading.Tasks;

namespace COServer.Game.MsgMonster
{
    public class MobCollection
    {
        public const byte Multiple = 3;

        public string LocationSpawn = "";

        public object SyncRoot = new object();
        public static Counter GenerateUid = new Counter(400000);
        public Role.GameMap DMap = null;

        private uint DmapID = 0;

        public MobCollection(uint Map)
        {
            DmapID = Map;
            if (Database.Server.ServerMaps != null)
            {
                if (Database.Server.ServerMaps.TryGetValue(Map, out DMap))
                    DMap.MonstersColletion = this;
            }
        }

        public bool ReadMap()
        {
            if (Database.Server.ServerMaps != null)
            {
                var mapId = DmapID;
                if (Database.Server.ServerMaps.TryGetValue(mapId, out DMap))
                    DMap.MonstersColletion = this;
            }

            return DMap != null;
        }

        public MonsterRole Add(MonsterFamily Famili, bool RemoveOnDead = false, uint dinamicid = 0, bool justone = false)
        {
            if (DMap == null)
                ReadMap();
            return SpawnNormalMonsters(Famili, RemoveOnDead, dinamicid, justone);
        }

        private MonsterRole SpawnNormalMonsters(MonsterFamily Famili, bool RemoveOnDead, uint dinamicid, bool justone)
        {
            int count = (int)((Famili.Boss > 0) ? 1 : (int)(Math.Max(1, (int)Famili.SpawnCount * 2)));
            if (justone)
                count = Math.Max(1, (int)Famili.SpawnCount);

            MonsterRole monsterr = null;

            for (int x = 0; x < count; x++)
            {
                ushort _x = 0, _y = 0;
                TryObtainSpawnXY(Famili, out _x, out _y);
                if (!DMap.ValidLocation(_x, _y) || (DMap.MonsterOnTile(_x, _y) && Famili.Boss == 0))
                    continue;

                monsterr = SpawnMonster(Famili, RemoveOnDead, dinamicid, _x, _y, DMap.ID);
            }
            return monsterr;
        }

        private MonsterRole SpawnFixedMonster(MonsterFamily Famili, uint dinamicid, ushort x, ushort y, uint mapId)
        {
            MonsterRole Mob = new MonsterRole(Famili.Copy(), GenerateUid.Next, LocationSpawn, DMap);
            Mob.RemoveOnDead = true; // Permitir que o monstro seja removido ao morrer

            Mob.X = x; // Coordenada fixa
            Mob.Y = y; // Coordenada fixa
            Mob.RespawnX = x;
            Mob.RespawnY = y;
            Mob.Map = mapId;
            Mob.DynamicID = dinamicid;

            if (DMap != null)
            {
                DMap.View.EnterMap<MonsterRole>(Mob);
            }
            return Mob;
        }

        private MonsterRole SpawnMonster(MonsterFamily Famili, bool RemoveOnDead, uint dinamicid, ushort x, ushort y, uint mapId)
        {
            MonsterRole Mob = new MonsterRole(Famili.Copy(), GenerateUid.Next, LocationSpawn, DMap);
            Mob.RemoveOnDead = RemoveOnDead; // Permitir que o monstro seja removido ao morrer

            Mob.X = x;
            Mob.Y = y;
            Mob.RespawnX = x;
            Mob.RespawnY = y;
            Mob.Map = mapId;
            Mob.DynamicID = dinamicid;

            if (DMap != null)
            {
                DMap.View.EnterMap<MonsterRole>(Mob);
            }

            return Mob;
        }


        public void TryObtainSpawnXY(MonsterFamily Monster, out ushort X, out ushort Y)
        {
            X = (ushort)Program.GetRandom.Next(Monster.SpawnX, Monster.MaxSpawnX);
            Y = (ushort)Program.GetRandom.Next(Monster.SpawnY, Monster.MaxSpawnY);

            for (byte i = 0; i < 10; i++)
            {
                if (DMap == null)
                    break;
                if (DMap.ValidLocation(X, Y) && !DMap.MonsterOnTile(X, Y))
                    break;

                X = (ushort)Program.GetRandom.Next(Monster.SpawnX, Monster.MaxSpawnX);
                Y = (ushort)Program.GetRandom.Next(Monster.SpawnY, Monster.MaxSpawnY);
            }
        }
    }
}
