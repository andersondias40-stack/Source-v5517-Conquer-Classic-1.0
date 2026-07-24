using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using COServer.Database;

namespace COServer.Game.MsgTournaments
{
    public class CoordinatePC
    {
        public ushort X;
        public ushort Y;
        public uint Map;
        public CoordinatePC(uint Map, ushort X, ushort Y)
        {
            this.Map = Map;
            this.X = X;
            this.Y = Y;
        }
    }
    /*
     * في الاوقات التالية 
"00:15" "02:15" "04:15" "06:15" "08:15" "10:15" "12:15" "14:15" "16:15" "18:15" "20:15" "22:15"

     */
    public class MsgPoleDominationPC
    {
        public class GuildWarScrore
        {
            public const int ConquerPointsReward = 50000, FinishMinutes = 10;

            public uint GuildID;
            public string Name;
            public uint Score;

            //for reward
            public int LeaderReward = 1;
            public int DeputiLeaderReward = 7;
        }

        public List<uint> RewardLeader = new List<uint>();
        public List<uint> RewardDeputiLeader = new List<uint>();
        private DateTime FinishTimer = new DateTime();
        public DateTime StampShuffleScore = new DateTime();

        private ProcesType Mode { get; set; }

        public Dictionary<Role.SobNpc.StaticMesh, Role.SobNpc> Furnitures { get; set; }
        public ConcurrentDictionary<uint, GuildWarScrore> ScoreList;
        public GuildWarScrore Winner;
        public MsgPoleDominationPC()
        {
            Mode = ProcesType.Dead;
            Furnitures = new Dictionary<Role.SobNpc.StaticMesh, Role.SobNpc>();
            ScoreList = new ConcurrentDictionary<uint, GuildWarScrore>();
            Winner = new GuildWarScrore() { Name = "None", Score = 100, GuildID = 0 };
        }

        public unsafe void CreateFurnitures()
        {
            Furnitures.Add(Role.SobNpc.StaticMesh.Pole, Server.ServerMaps[1011].View.GetMapObject<Role.SobNpc>(Role.MapObjectType.SobNpc, 833));
        }
        internal unsafe void ResetFurnitures(ServerSockets.Packet stream)
        {

            foreach (var npc in Furnitures.Values)
                npc.HitPoints = npc.MaxHitPoints;

            foreach (var client in Server.GamePoll.Values)
            {
                if (client.Player.Map == 1011)
                {
                    foreach (var npc in Furnitures.Values)
                    {
                        if (Role.Core.GetDistance(client.Player.X, client.Player.Y, npc.X, npc.Y) <= Role.SobNpc.SeedDistrance)
                        {
                            MsgServer.MsgUpdate upd = new MsgServer.MsgUpdate(stream, npc.UID, 1);
                            //upd.Append(stream, MsgServer.MsgUpdate.DataType.Mesh, (uint)npc.Mesh);
                            upd.Append(stream, MsgServer.MsgUpdate.DataType.Hitpoints, npc.HitPoints);
                            stream = upd.GetArray(stream);
                            client.Send(stream);
                            if ((Role.SobNpc.StaticMesh)npc.Mesh == Role.SobNpc.StaticMesh.Pole)
                                client.Send(npc.GetArray(stream, false));
                        }
                    }
                }
            }
        }
        internal unsafe void SendMapPacket(ServerSockets.Packet packet)
        {
            foreach (var client in Server.GamePoll.Values)
            {
                if (client.Player.Map == 1011 || client.Player.Map == 6001)
                {
                    client.Send(packet);
                }
            }
        }
        internal unsafe void CompleteEndGuildWar()
        {
            ShuffleGuildScores();
            Mode = ProcesType.Dead;
            ScoreList.Clear();
            RewardLeader.Clear();
            Winner.LeaderReward = 1;
        }

        internal unsafe void Start()
        {
            if (Mode == ProcesType.Dead)
            {
                FinishTimer = DateTime.Now.AddMinutes(GuildWarScrore.FinishMinutes);
                Mode = ProcesType.Alive;
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();

                    ResetFurnitures(stream);
                    ScoreList.Clear();
                }
            }
        }
        public void CheckUp()
        {
            if (DateTime.Now.Hour == 06 && (DateTime.Now.Minute == 15 && DateTime.Now.Second < 2))
            {
                if (Mode == ProcesType.Dead)
                {
                    FinishTimer = DateTime.Now.AddMinutes(GuildWarScrore.FinishMinutes);
                    Mode = ProcesType.Alive;
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();

                        ResetFurnitures(stream);
                        ScoreList.Clear();                           
                    }
                }
            }
            if (Mode == ProcesType.Alive)
            {
                if (DateTime.Now > MsgSchedules.PoleDominationPC.StampShuffleScore)
                {
                    MsgSchedules.PoleDominationPC.ShuffleGuildScores();
                }
                if (DateTime.Now > FinishTimer)
                {
                    CompleteEndGuildWar();
                }
            }
        }
        public bool IsFinished() { return Mode == ProcesType.Dead; }
        internal unsafe void FinishRound()
        {
            ShuffleGuildScores(true);
            Furnitures[Role.SobNpc.StaticMesh.Pole].Name = Winner.Name;
            ScoreList.Clear();
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage("Congratulations to " + Winner.Name + ", they've won the PoleDomination of PC round with a score of " + Winner.Score.ToString() + ""
                   , MsgServer.MsgMessage.MsgColor.white, MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
                Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage("Congratulations to " + Winner.Name + ", they've won the PoleDomination of PC round with a score of " + Winner.Score.ToString() + ""
                    , MsgServer.MsgMessage.MsgColor.red, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));

                ResetFurnitures(stream);
            }
        }
        internal void UpdateScore(Role.Player client, uint Damage)
        {
            if (client.MyGuild == null)
                return;
            if (Mode == ProcesType.Alive)
            {
                if (!ScoreList.ContainsKey(client.GuildID))
                {
                    ScoreList.TryAdd(client.GuildID, new GuildWarScrore() { GuildID = client.MyGuild.Info.GuildID, Name = client.MyGuild.GuildName, Score = Damage });
                }
                else
                {
                    ScoreList[client.MyGuild.Info.GuildID].Score += Damage;
                }

                if (Furnitures[Role.SobNpc.StaticMesh.Pole].HitPoints == 0)
                    FinishRound();
            }
        }

        internal unsafe void ShuffleGuildScores(bool createWinned = false)
        {
            if (Mode != ProcesType.Dead)
            {
                StampShuffleScore = DateTime.Now.AddSeconds(8);
                var Array = ScoreList.Values.ToArray();
                var DescendingList = Array.OrderByDescending(p => p.Score).ToArray();
                for (int x = 0; x < DescendingList.Length; x++)
                {
                    var element = DescendingList[x];
                    if (x == 0 && createWinned)
                        Winner = element;
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        Game.MsgServer.MsgMessage msg = new MsgServer.MsgMessage("No " + (x + 1).ToString() + ". " + element.Name + " (" + element.Score.ToString() + ")"
                           , MsgServer.MsgMessage.MsgColor.yellow, x == 0 ? MsgServer.MsgMessage.ChatMode.FirstRightCorner : MsgServer.MsgMessage.ChatMode.ContinueRightCorner);

                        SendMapPacket(msg.GetArray(stream));

                    }
                    if (x == 4)
                        break;
                }
            }
        }
    }
}
