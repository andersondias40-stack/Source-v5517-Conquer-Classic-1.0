using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using COServer.Game.MsgServer;
using COServer.Game.MsgTournaments;

namespace COServer.Game.MsgMonster
{
    public class BossesBase
    {
        internal static void SendInvitation(string Name, ushort X, ushort Y, ushort map, ushort DinamicID, int Seconds, Game.MsgServer.MsgStaticMessage.Messages messaj = Game.MsgServer.MsgStaticMessage.Messages.None)
        {
            string Message = " " + Name + " is about to begin! Will you join it?";
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Program.SendGlobalPackets.Enqueue(new MsgMessage($"{Name} has started!", MsgMessage.MsgColor.yellow, MsgMessage.ChatMode.TopLeftSystem).GetArray(rec.GetStream()));

                var packet = new Game.MsgServer.MsgMessage(Message, MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream);
                foreach (var client in Database.Server.GamePoll.Values)
                {
                    client.Send(packet);
                    client.Player.MessageBox(Message, new Action<Client.GameClient>(user => user.Teleport(X, Y, map, DinamicID)), null, Seconds, messaj);
                }
            }

            // Envia uma mensagem para o Discord informando que o evento começou
            Program.DiscordAPIevents.Enqueue($"``{Name} has started!``");
            Program.DiscordAPI.Enqueue($"`` Server Online ``");
        }



        public static DateTime lastCleanwaterSpawnTime = DateTime.Now; // Armazena o último spawn do Cleanwater      
        public static DateTime lastSpawnTime = DateTime.Now; // Armazena o último spawn dos outros bosses
        public static DateTime lastSnakeKingSpawnTime = DateTime.Now; // Armazena o último spawn do Sna
        public static DateTime lastGanodermaSpawnTime = DateTime.Now;
        public static DateTime lastTitanSpawnTime = DateTime.Now;
        public static DateTime lastExpMobSpawnTime = DateTime.Now;


        public static void BossesTimer()
        {
            DateTime now = DateTime.Now;

            // ExpMob: respawna a cada 2 hora
            if ((now - lastExpMobSpawnTime).TotalHours >= 2)
            {
                var map = Database.Server.ServerMaps[1004];
                if (!map.ContainMobID(41299))
                {
                    SpawnHandler(1004, 51, 49, 41299, "", "",
                        "[ ExpMob ] has started in Promoted Center !");

                    lastExpMobSpawnTime = now;
                }
            }

            // Titan: Spawna às XX:37 de toda hora
            if (now.Minute % 37 == 0 && now.Second == 0)
            {
                var map = Database.Server.ServerMaps[1020];
                if (!map.ContainMobID(3134))
                {
                    SpawnHandler(1020, 594, 656, 3134, "",
    "",
    "TitanL72 has spawned in Ape City at ( 593,653 )");

                    lastTitanSpawnTime = now;
                }
            }
            // Cleanwater: Spawna a cada 30 minutos (xx:00 e xx:30)
            if (now.Minute % 30 == 0 && now.Second == 0)
            {
                var map = Database.Server.ServerMaps[1212];
                if (!map.ContainMobID(8500))
                {
                    SpawnHandler(1212, 797, 818, 8500, "",
    "",
    "WaterLord has spawned in Adventure Island at ( 797,818)");

                    lastCleanwaterSpawnTime = now;
                }
            }
        }

        public static void SpawnHandler(uint MapID, ushort X, ushort Y, uint MobID, string MonsterName, string PrepareMsg, string Msg, MsgServer.MsgStaticMessage.Messages idmsg = MsgServer.MsgStaticMessage.Messages.None)
        {
            var Map = Database.Server.ServerMaps[MapID];
            if (!Map.ContainMobID(MobID))
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(MonsterName + Msg, "ALLUSERS", "Server", MsgServer.MsgMessage.MsgColor.white, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                    Database.Server.AddMapMonster(stream, Map, MobID, X, Y, 1, 1, 1);
                }
            }
        }
    }
}
