using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using COServer.Database;
using COServer.Role.Instance;

namespace COServer.Game.MsgTournaments
{
    public class MsgGuildWar
    {

        public bool SendInvitation = false;
        internal ushort[][] StatueCoords =
        {
          new ushort[] {140 ,134 }
         ,new ushort[] {144 ,124 }
         ,new ushort[] {130 ,138 }
         ,new ushort[] {153 ,124 }
         ,new ushort[] {161 ,124 }
         ,new ushort[] {130 ,147 }
         ,new ushort[] {130 ,155 }
        };

        public class GuildConductor
        {
            public static List<uint> BlockMaps = new List<uint>()
            {
                1038
            };
            public Game.MsgNpc.Npc Npc;
            public uint ToMap;
            public ushort ToX, ToY;

            public override string ToString()
            {
                Database.DBActions.WriteLine Line = new Database.DBActions.WriteLine(',');
                Line.Add(Npc.UID).Add(Npc.X).Add(Npc.Y).Add(Npc.Map).Add(Npc.Mesh).Add((ushort)Npc.NpcType)
                    .Add(ToX).Add(ToY).Add(ToMap);
                return Line.Close();
            }
            internal void Load(string Line, MsgNpc.NpcID UID)
            {
                Npc = Game.MsgNpc.Npc.Create();
                if (Line == "")
                {
                    Npc.UID = (uint)UID;
                    return;
                }
                Database.DBActions.ReadLine Reader = new Database.DBActions.ReadLine(Line, ',');
                Npc.UID = Reader.Read((uint)0);
                Npc.X = Reader.Read((ushort)0);
                Npc.Y = Reader.Read((ushort)0);
                Npc.Map = Reader.Read((ushort)0);
                Npc.Mesh = Reader.Read((ushort)0);
                Npc.NpcType = (Role.Flags.NpcType)Reader.Read((ushort)0);

                ToX = Reader.Read((ushort)0);
                ToY = Reader.Read((ushort)0);
                ToMap = Reader.Read((ushort)0);
            }
            internal static bool ChangeNpcLocation(Role.GameMap map, ref ushort X, ref ushort Y, ref uint Map)
            {


                return false;
            }

            internal void GetCoords(out ushort x, out ushort y, out uint map)
            {
                if (ToMap != 0 && ToX != 0 && ToY != 0)
                {
                    x = ToX;
                    y = ToY;
                    map = ToMap;
                    return;
                }
                x = 429;
                y = 378;
                map = 1002;
            }
        }

        public class DataFlameQuest
        {
            public List<uint> Registred;
            public bool ActiveFlame10;

            public DataFlameQuest()
            {
                Registred = new List<uint>();
                ActiveFlame10 = false;
            }
        }
        public class GuildWarScrore
        {
            public const int ConquerPointsReward = 10000;

            public uint GuildID;
            public string Name;
            public uint Score;

            public uint GuildKillCount = 0;

            //for reward
            public int LeaderReward = 1;
            public int DeputiLeaderReward = 4;
        }

        public List<uint> RewardLeader = new List<uint>();
        public List<uint> RewardDeputiLeader = new List<uint>();

        public DateTime StampRound = new DateTime();
        public DateTime StampShuffleScore = new DateTime();
        private Role.GameMap GuildWarMap;

        public ProcesType Proces { get; set; }
        public DataFlameQuest FlamesQuest;
        public Dictionary<Role.SobNpc.StaticMesh, Role.SobNpc> Furnitures { get; set; }
        public ConcurrentDictionary<uint, GuildWarScrore> ScoreList;
        public GuildWarScrore Winner;
        public Dictionary<MsgNpc.NpcID, GuildConductor> GuildConductors;

        public bool LeftGateOpen { get { return Furnitures[Role.SobNpc.StaticMesh.LeftGate].Mesh == Role.SobNpc.StaticMesh.OpenLeftGate; } }
        public bool RightGateOpen { get { return Furnitures[Role.SobNpc.StaticMesh.RightGate].Mesh == Role.SobNpc.StaticMesh.OpenRightGate; } }
        public MsgGuildWar()
        {
            FlamesQuest = new DataFlameQuest();
            Proces = ProcesType.Dead;
            Furnitures = new Dictionary<Role.SobNpc.StaticMesh, Role.SobNpc>();
            GuildConductors = new Dictionary<MsgNpc.NpcID, GuildConductor>();
            ScoreList = new ConcurrentDictionary<uint, GuildWarScrore>();
            Winner = new GuildWarScrore() { Name = "None", Score = 100, GuildID = 0 };
        }

        public unsafe void CreateFurnitures()
        {
            Furnitures.Add(Role.SobNpc.StaticMesh.LeftGate, Database.Server.ServerMaps[1038].View.GetMapObject<Role.SobNpc>(Role.MapObjectType.SobNpc, 516074));
            Furnitures.Add(Role.SobNpc.StaticMesh.RightGate, Database.Server.ServerMaps[1038].View.GetMapObject<Role.SobNpc>(Role.MapObjectType.SobNpc, 516075));
            Furnitures.Add(Role.SobNpc.StaticMesh.Pole, Database.Server.ServerMaps[1038].View.GetMapObject<Role.SobNpc>(Role.MapObjectType.SobNpc, 810));
        }
        public unsafe bool Bomb(ServerSockets.Packet stream, Client.GameClient client, Role.SobNpc.StaticMesh gate)
        {
            if (Furnitures[gate].HitPoints > 3000000)
            {
                Furnitures[gate].HitPoints -= 2000000;

                MsgServer.MsgUpdate upd = new MsgServer.MsgUpdate(stream, Furnitures[gate].UID, 1);
                stream = upd.Append(stream, MsgServer.MsgUpdate.DataType.Hitpoints, Furnitures[gate].HitPoints);
                Furnitures[gate].SendScrennPacket(upd.GetArray(stream));

                client.Player.Dead(null, client.Player.X, client.Player.Y, client.Player.UID);
                Furnitures[gate].SendString(stream, MsgServer.MsgStringPacket.StringID.Effect, "firemagic");
                Furnitures[gate].SendString(stream, MsgServer.MsgStringPacket.StringID.Effect, "bombarrow");

                Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage("" + client.Player.Name + " from " + (client.Player.MyGuild != null ? client.Player.MyGuild.GuildName.ToString() : "None".ToString()).ToString() + " detonated the Bomb and killed herself/himself. But the " + gate.ToString() + " was blown up!"
                    , MsgServer.MsgMessage.MsgColor.white, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));

                return true;
            }
            return false;
        }
        internal unsafe void ResetFurnitures(ServerSockets.Packet stream)
        {
            Furnitures[Role.SobNpc.StaticMesh.LeftGate].Mesh = Role.SobNpc.StaticMesh.LeftGate;
            Furnitures[Role.SobNpc.StaticMesh.RightGate].Mesh = Role.SobNpc.StaticMesh.RightGate;

            foreach (var npc in Furnitures.Values)
                npc.HitPoints = npc.MaxHitPoints;

            foreach (var client in Database.Server.GamePoll.Values)
            {
                if (client.Player.Map == 1038)
                {
                    foreach (var npc in Furnitures.Values)
                    {
                        if (Role.Core.GetDistance(client.Player.X, client.Player.Y, npc.X, npc.Y) <= Role.SobNpc.SeedDistrance)
                        {
                            MsgServer.MsgUpdate upd = new MsgServer.MsgUpdate(stream, npc.UID, 2);
                            stream = upd.Append(stream, MsgServer.MsgUpdate.DataType.Mesh, (long)npc.Mesh);
                            stream = upd.Append(stream, MsgServer.MsgUpdate.DataType.Hitpoints, npc.HitPoints);
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
            foreach (var client in Database.Server.GamePoll.Values)
            {
                if (client.Player.Map == 1038 || client.Player.Map == 6001)
                {
                    client.Send(packet);
                }
            }
        }
        internal unsafe void CompleteEndGuildWar()
        {
            SendInvitation = false;
            ShuffleGuildScores();
            Proces = ProcesType.Dead;
            ScoreList.Clear();
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage("Congratulations to " + Winner.Name + ", they've won the Guild War with a score of " + Winner.Score.ToString() + ""
                  , MsgServer.MsgMessage.MsgColor.white, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage("Congratulations to " + Winner.Name + ", they've won the Guild War with a score of " + Winner.Score.ToString() + ""
                   , MsgServer.MsgMessage.MsgColor.white, MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
            }

            RewardDeputiLeader.Clear();
            RewardLeader.Clear();
            Winner.DeputiLeaderReward = 4;
            Winner.LeaderReward = 1;

        }

        internal unsafe void Start()
        {
            FlamesQuest = new DataFlameQuest();
            Proces = ProcesType.Alive;
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();

                ResetFurnitures(stream);
                ScoreList.Clear();
                Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage("Guild War has started!", MsgServer.MsgMessage.MsgColor.white, MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
            }

            foreach (var player in Database.Server.GamePoll.Values
                     .Where(e => e.Player.Map == 1038)
                     .Where(e => e.Player.MyGuild != null))
            {
                if (!ScoreList.ContainsKey(player.Player.GuildID))
                {
                    Console.WriteLine($"Start - Guild {player.Player.MyGuild.GuildName} not found");

                    ScoreList.TryAdd(player.Player.GuildID, new GuildWarScrore()
                    {
                        GuildID = player.Player.MyGuild.Info.GuildID,
                        Name = player.Player.MyGuild.GuildName,
                        Score = 0,
                        GuildKillCount = 0,
                    });
                }
            }
        }

        internal unsafe void FinishRound()
        {
            ShuffleGuildScores(true);
            Furnitures[Role.SobNpc.StaticMesh.Pole].Name = Winner.Name;
            Proces = ProcesType.Idle;
            ScoreList.Clear();
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage("Congratulations to " + Winner.Name + ", they've won the Guild War with a score of " + Winner.Score.ToString() + ""
                   , MsgServer.MsgMessage.MsgColor.white, MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
                Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage("Congratulations to " + Winner.Name + ", they've won the Guild War with a score of " + Winner.Score.ToString() + ""
                    , MsgServer.MsgMessage.MsgColor.white, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));

                ResetFurnitures(stream);
            }
            StampRound = DateTime.Now.AddSeconds(3);
        }
        internal unsafe void Began()
        {
            if (Proces == ProcesType.Idle)
            {
                Proces = ProcesType.Alive;
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage("Guild War has began!", MsgServer.MsgMessage.MsgColor.white, MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
                }
            }
        }
        internal void PoleFundHolder(Role.Player user, uint damage, uint hitpoint, bool check)
        {
            if (Game.MsgTournaments.MsgSchedules.GuildWar.Proces == MsgTournaments.ProcesType.Alive)
            {
                if (user.MyGuild != null)
                {
                    if (Game.MsgTournaments.MsgSchedules.GuildWar.Winner != null)
                    {
                        Role.Instance.Guild guild;
                        if (Role.Instance.Guild.GuildPoll.TryGetValue(Game.MsgTournaments.MsgSchedules.GuildWar.Winner.GuildID, out guild))
                        {
                            if (check)
                            {
                                if (guild.Info.SilverFund > damage / 40)
                                {
                                    guild.Info.SilverFund -= damage / 40;
                                    if (damage >= 500)
                                        user.Money += damage / 50;

                                    user.MyGuild.Info.SilverFund += damage / 50;
                                    user.MyGuild.Info.Donation += damage / 50;
                                }
                                else guild.Info.SilverFund = 0;
                            }
                            else
                            {
                                if (guild.Info.SilverFund > hitpoint / 45)
                                {
                                    guild.Info.SilverFund -= hitpoint / 45;
                                    if (damage >= 500)
                                        user.Money += 10;
                                }
                                else guild.Info.SilverFund = 0;

                                user.MyGuild.Info.SilverFund += hitpoint / 50;
                                user.MyGuild.Info.Donation += hitpoint / 50;
                            }
                        }
                    }
                }
            }
        }
        internal void UpdateScore(Role.Player client, uint Damage)
        {
            if (client.MyGuild == null)
                return;
            if (Proces == ProcesType.Alive)
            {
                if (!ScoreList.ContainsKey(client.GuildID))
                {
                    Console.WriteLine($"UpdateScore - Guild {client.MyGuild.GuildName} not found");

                    ScoreList.TryAdd(client.GuildID, new GuildWarScrore() { 
                        GuildID = client.MyGuild.Info.GuildID, 
                        Name = client.MyGuild.GuildName,
                        Score = Damage });
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
            if (Proces != ProcesType.Dead)
            {
                StampShuffleScore = DateTime.Now.AddSeconds(1);

                // Cálculo do tempo restante até as 13:00 de domingo
                DateTime now = DateTime.Now;

                DateTime eventEndTime = new DateTime(now.Year, now.Month, now.Day, 17, 0, 0);
                if (now.DayOfWeek != DayOfWeek.Sunday)
                {
                    eventEndTime = eventEndTime.AddDays(((int)DayOfWeek.Sunday - (int)now.DayOfWeek + 7) % 7);
                }
                TimeSpan timeRemaining = eventEndTime - now;

                // Formatação da mensagem de tempo restante
                string timeRemainingMessage = $"GuildWar End In: [{timeRemaining.Hours}h {timeRemaining.Minutes}m {timeRemaining.Seconds}s]";

                // Enviar a mensagem de tempo restante para acabar gw
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    Game.MsgServer.MsgMessage timeMsg = new MsgServer.MsgMessage(
                        timeRemainingMessage,
                        MsgServer.MsgMessage.MsgColor.yellow,
                        MsgServer.MsgMessage.ChatMode.FirstRightCorner
                    );

                    SendMapPacket(timeMsg.GetArray(stream));
                }

                // Enviar a mensagem de separação
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    Game.MsgServer.MsgMessage timeMsg = new MsgServer.MsgMessage(
                        "----------------------------------------", //separador 
                        MsgServer.MsgMessage.MsgColor.yellow,
                        MsgServer.MsgMessage.ChatMode.ContinueRightCorner
                    );

                    SendMapPacket(timeMsg.GetArray(stream));
                }

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
                        Game.MsgServer.MsgMessage msg = new MsgServer.MsgMessage(
                            $"No {x + 1}. {element.Name} [{element.Score}]",
                            MsgServer.MsgMessage.MsgColor.yellow,
                            x == 0 ? MsgServer.MsgMessage.ChatMode.ContinueRightCorner : MsgServer.MsgMessage.ChatMode.ContinueRightCorner
                        );

                        // Enviar a mensagem ao mapa
                        SendMapPacket(msg.GetArray(stream));
                    }
                    if (x == 6)
                        break;
                }

                // Enviar a mensagem de separação
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    Game.MsgServer.MsgMessage timeMsg = new MsgServer.MsgMessage(
                        "----------------------------------------", //separador 
                        MsgServer.MsgMessage.MsgColor.yellow,
                        MsgServer.MsgMessage.ChatMode.ContinueRightCorner
                    );

                    SendMapPacket(timeMsg.GetArray(stream));
                }

                foreach (var guildScore in ScoreList)
                    guildScore.Value.GuildKillCount = 0;

                // Dicionário para armazenar jogadores e suas kills
                var playerKills = new Dictionary<string, uint>();

                foreach (var player in Database.Server.GamePoll.Values
                     .Where(e => e.Player.Map == 1038)
                     .Where(e => e.Player.MyGuild != null))
                {
                    if (!ScoreList.ContainsKey(player.Player.GuildID))
                    {
                        ScoreList.TryAdd(player.Player.GuildID, new GuildWarScrore()
                        {
                            GuildID = player.Player.MyGuild.Info.GuildID,
                            Name = player.Player.MyGuild.GuildName,
                            Score = 0,
                            GuildKillCount = 0,
                        });
                    }
                    else
                    {
                        ScoreList[player.Player.GuildID].GuildKillCount += (uint)player.TotalKillsGW;
                    }

                    // Adicionando jogadores ao dicionário de kills
                    if (!playerKills.ContainsKey(player.Player.Name))
                    {
                        playerKills[player.Player.Name] = (uint)player.TotalKillsGW;
                    }
                    else
                    {
                        playerKills[player.Player.Name] += (uint)player.TotalKillsGW;
                    }
                }

                var guilds = ScoreList.Values.ToArray();
                foreach (var guild in guilds)
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        Game.MsgServer.MsgMessage msg = new MsgServer.MsgMessage(
                        $"Guild - [{guild.Name}] Total Kills: [{guild.GuildKillCount}]",
                        MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);

                        // Enviar a mensagem ao mapa
                        SendMapPacket(msg.GetArray(stream));
                    }
                }

                // Enviar a mensagem de separação
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    Game.MsgServer.MsgMessage timeMsg = new MsgServer.MsgMessage(
                        "----------------------------------------", //separador 
                        MsgServer.MsgMessage.MsgColor.yellow,
                        MsgServer.MsgMessage.ChatMode.ContinueRightCorner
                    );

                    SendMapPacket(timeMsg.GetArray(stream));
                }
                // Ordenar os jogadores por kills e exibir os 5 principais
                var topPlayers = playerKills.OrderByDescending(k => k.Value).Take(5).ToList();

                // Enviar a mensagem dos top 5 jogadores
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();

                    // Remover a linha de título e usar a numeração
                    for (int x = 0; x < topPlayers.Count; x++)
                    {
                        var topPlayer = topPlayers[x];

                        // Criar a mensagem formatada para cada jogador
                        string playerMessage = $"Player{x + 1}. {topPlayer.Key} Total Kills: [{topPlayer.Value}]";

                        Game.MsgServer.MsgMessage msg = new MsgServer.MsgMessage(
                            playerMessage,
                            MsgServer.MsgMessage.MsgColor.yellow,
                            MsgServer.MsgMessage.ChatMode.ContinueRightCorner
                        );

                        // Enviar a mensagem ao mapa
                        SendMapPacket(msg.GetArray(stream));
                    }
                }
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    Game.MsgServer.MsgMessage timeMsg = new MsgServer.MsgMessage(
                        "----------------------------------------", //separador 
                        MsgServer.MsgMessage.MsgColor.yellow,
                        MsgServer.MsgMessage.ChatMode.ContinueRightCorner
                    );

                    SendMapPacket(timeMsg.GetArray(stream));
                }
            }
        }




        internal bool ValidJump(int Current, out int New, ushort X, ushort Y)
        {
            if (Role.Core.GetDistance(217, 177, X, Y) <= 3)
            {
                New = 0;
                return true;
            }
            New = Current;
            int new_FloorType = GuildWarMap.FloorType[X, Y];
            if (Current == 3)
            {
                if (new_FloorType == 0 || new_FloorType == 9 || new_FloorType == 13)
                {
                    if (Role.Core.GetDistance(X, Y, 164, 209) <= 20)
                    {
                        if (LeftGateOpen)
                        {
                            New = new_FloorType;
                            return true;
                        }
                    }
                    if (Role.Core.GetDistance(X, Y, 220, 178) <= 15)
                    {
                        if (RightGateOpen)
                        {
                            New = new_FloorType;
                            return true;
                        }
                    }
                    return false;
                }
            }
            New = new_FloorType;
            return true;
        }
        internal bool ValidWalk(int Current, out int New, ushort X, ushort Y)
        {
            if (Role.Core.GetDistance(217, 177, X, Y) <= 3)
            {
                New = 0;
                return true;
            }
            New = Current;
            int new_mask = GuildWarMap.FloorType[X, Y];
            if (Current == 3)
            {
                if (new_mask == 0 || new_mask == 9 || new_mask == 13)
                {
                    if (Y == 209 || Y == 208)
                    {
                        if (Role.Core.GetDistance(X, Y, 164, 209) <= 3)
                        {
                            if (LeftGateOpen)
                            {
                                New = new_mask;
                                return true;
                            }
                        }
                    }
                    else if (X == 216)
                    {
                        if (Role.Core.GetDistance(X, Y, 216, 177) <= 4)
                        {
                            if (RightGateOpen)
                            {
                                New = new_mask;
                                return true;
                            }
                        }
                    }

                    return false;
                }
            }
            New = new_mask;
            return true;
        }
        internal void Save()
        {
            WindowsAPI.IniFile write = new WindowsAPI.IniFile("\\GuildWarInfo.ini");
            if (Proces == ProcesType.Dead)
            {
                write.Write<uint>("Info", "ID", Winner.GuildID);
                write.WriteString("Info", "Name", Winner.Name);
                write.Write<int>("Info", "LeaderReward", Winner.LeaderReward);
                write.Write<int>("Info", "DeputiLeaderReward", Winner.DeputiLeaderReward);

                for (int x = 0; x < RewardLeader.Count; x++)
                    write.Write<uint>("Info", "LeaderTop" + x.ToString() + "", RewardLeader[x]);
                for (int x = 0; x < 8; x++)
                {
                    if (x >= RewardDeputiLeader.Count)
                        break;
                    write.Write<uint>("Info", "DeputiTop" + x.ToString() + "", RewardDeputiLeader[x]);
                }
                write.WriteString("Pole", "Name", Winner.Name);
                write.Write<int>("Pole", "HitPoints", Furnitures[Role.SobNpc.StaticMesh.Pole].HitPoints);
            }

            write.WriteString("Condutors", "condutora01", GuildConductors[MsgNpc.NpcID.TeleGuild1].ToString());
            write.WriteString("Condutors", "condutora02", GuildConductors[MsgNpc.NpcID.TeleGuild2].ToString());
            write.WriteString("Condutors", "condutora03", GuildConductors[MsgNpc.NpcID.TeleGuild3].ToString());
            write.WriteString("Condutors", "condutora04", GuildConductors[MsgNpc.NpcID.TeleGuild4].ToString());
        }
        public static byte HourOff = 0;
        public static byte FlameActive = 0;
        internal void Load()
        {
            MySqlCommand cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("GuildWar").Where("Owner", "AbdallahKhalel");
            MySqlReader r = new MySqlReader(cmd);
            if (r.Read())
            {
                //Time On & Off
                HourOff = r.ReadByte("HourOff");
                FlameActive = r.ReadByte("FlameActive");
            }
            Console.WriteLine("GuildWar-Time Loaded");
            WindowsAPI.IniFile reader = new WindowsAPI.IniFile("\\GuildWarInfo.ini");
            Winner.GuildID = reader.ReadUInt32("Info", "ID", 0);
            Winner.Name = reader.ReadString("Info", "Name", "None");
            Winner.LeaderReward = reader.ReadInt32("Info", "LeaderReward", 0);
            Winner.DeputiLeaderReward = reader.ReadInt32("Info", "DeputiLeaderReward", 0);

            RewardLeader.Add(reader.ReadUInt32("Info", "LeaderTop0", 0));
            for (int x = 0; x < 8; x++)
            {
                RewardDeputiLeader.Add(reader.ReadUInt32("Info", "DeputiTop" + x.ToString() + "", 0));
            }

            Furnitures[Role.SobNpc.StaticMesh.Pole].Name = reader.ReadString("Pole", "Name", "None");
            Furnitures[Role.SobNpc.StaticMesh.Pole].HitPoints = reader.ReadInt32("Pole", "HitPoints", 0);

            for (int x = 0; x < 4; x++)
            {
                GuildConductor conductor = new GuildConductor();
                conductor.Load(reader.ReadString("Condutors", "GuildConductor" + (x + 1).ToString() + "", ""), (MsgNpc.NpcID)(101614 + x * 2));
                GuildConductors.Add((MsgNpc.NpcID)(101614 + x * 2), conductor);
                if (conductor.Npc.Map != 0)
                {
                    if (Database.Server.ServerMaps.ContainsKey(conductor.Npc.Map))
                        Database.Server.ServerMaps[conductor.Npc.Map].AddNpc(conductor.Npc);
                }
            }
            GuildWarMap = Database.Server.ServerMaps[1038];

        }

    }
}
