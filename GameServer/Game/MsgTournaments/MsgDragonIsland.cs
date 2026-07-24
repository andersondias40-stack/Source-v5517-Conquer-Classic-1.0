using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COServer.Game.MsgTournaments
{
    public class MsgDragonIsland
    {
        public string BossName;
        public Time32 LastMonsterSapwn;
        private const ushort MapID = 1787,
            X = 048, Y = 037;

        public ProcesType Process { get; set; }

        public Role.GameMap Map
        {
            get { return Database.Server.ServerMaps[MapID]; }
        }

        public MsgDragonIsland(ProcesType _type)
        {
            Process = _type;
        }

        public bool Join(Client.GameClient user, ServerSockets.Packet stream)
        {
            if (user.Player.Level < 100)
                return false;

            user.Teleport((ushort)(50 - Program.GetRandom.Next(0, 5)), (ushort)(60 - Program.GetRandom.Next(0, 5)), MapID);
            return true;
        }
        public void SendMapPacket(ServerSockets.Packet stream)
        {
            foreach (var user in MapPlayers())
                user.Send(stream);
        }
        public Client.GameClient[] MapPlayers()
        {
            return Map.Values.Where(p => InTournament(p)).ToArray();
        }
        public bool InTournament(Client.GameClient user)
        {
            if (Map == null)
                return false;
            return user.Player.Map == Map.ID;
        }
        public void CheckUp()
        {
            if (Process == ProcesType.Alive)
            {
                if (!Map.ContainMobID(20160)
                    && !Map.ContainMobID(20300)
                    && !Map.ContainMobID(20070))
                    Process = ProcesType.Idle;
            }
            if (Process == ProcesType.Idle)
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    if (LastMonsterSapwn > Game.MsgMonster.MonsterRole.LastBossesKilled)
                    {
                        if (!Map.ContainMobID(20160))//Thrilling Spook
                        {
                            LastMonsterSapwn = Time32.Now;
                            Database.Server.AddMapMonster(stream, Map, 20160, X, Y, 1, 1, 1, 0, true, Game.MsgFloorItem.MsgItemPacket.EffectMonsters.EarthquakeAndNight);
                            Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("[Thrilling Spook] has appeared in Bosses Island! Go and kill it now.", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                            Console.WriteLine("[Dragon Island] Thrilling Spook has spawned!");
                            Process = ProcesType.Alive;
                        }
                    }
                }
            }
        }

    }
}