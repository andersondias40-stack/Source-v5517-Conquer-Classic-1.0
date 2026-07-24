using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using COServer.Game.MsgServer;
using static COServer.Game.MsgServer.MsgStringPacket;

namespace COServer.Game.MsgTournaments
{
    public class GuildSurvival
    {
        public ProcesType Process { get; set; }
        public DateTime StartTimer = new DateTime();
        public DateTime InfoTimer = new DateTime();
        public Role.GameMap Map;
        public uint DinamicMap = 0;
        public Dictionary<uint, int> GuildLives; // ID da guilda -> Vidas restantes

        public GuildSurvival()
        {
            Process = ProcesType.Dead;
            InfoTimer = DateTime.Now;
            GuildLives = new Dictionary<uint, int>();
        }

        public void Open()
        {
            if (Process == ProcesType.Dead)
            {
                StartTimer = DateTime.Now;

                if (Map == null)
                {
                    Map = Database.Server.ServerMaps[700];
                    DinamicMap = Map.GenerateDynamicID();
                }
                GuildLives.Clear();
                Process = ProcesType.Idle;
                InfoTimer = DateTime.Now; // Resetar InfoTimer aqui para começar imediatamente
            }
        }

        public bool Join(Client.GameClient user, ServerSockets.Packet stream)
        {
            if (Process == ProcesType.Idle && user.Player.MyGuild != null)
            {
                ushort x = 0;
                ushort y = 0;
                Map.GetRandCoord(ref x, ref y);
                user.Teleport(x, y, Map.ID, DinamicMap);
                if (!GuildLives.ContainsKey(user.Player.GuildID))
                {
                    GuildLives[user.Player.GuildID] = 100;
                }   
                return true;
            }
            return false;
        }

        public void CheckUp()
        {
            if (Process != ProcesType.Dead && Map != null)
            {
                #region Lives and Timer Display
                if (DateTime.Now > InfoTimer.AddSeconds(1))
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        string headerText = Process == ProcesType.Idle ? "[ TIME TRAINING ]" : "   [WAR!]   ";
                        var headerMsg = new MsgServer.MsgMessage(headerText, MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.FirstRightCorner);
                        SendMapPacket(headerMsg.GetArray(stream));

                        var separatorMsg = new MsgServer.MsgMessage("--------------------------------", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                        SendMapPacket(separatorMsg.GetArray(stream));

                        var guildsTitleMsg = new MsgServer.MsgMessage("[Guilds]            [Life]", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                        SendMapPacket(guildsTitleMsg.GetArray(stream));

                        var separatorMsg2 = new MsgServer.MsgMessage("--------------------------------", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                        SendMapPacket(separatorMsg2.GetArray(stream));

                        var players = MapPlayers(); // Chamar MapPlayers() uma vez para otimizar
                        foreach (var guild in GuildLives)
                        {
                            var guildName = "Unknown";
                            var player = players.FirstOrDefault(p => p.Player.GuildID == guild.Key);
                            if (player != null && player.Player.MyGuild != null)
                                guildName = player.Player.MyGuild.GuildName;
                            var lifeMsg = new MsgServer.MsgMessage($"{guildName}:             {guild.Value} Lifes", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                            SendMapPacket(lifeMsg.GetArray(stream));
                        }
                        var separatorMsg3 = new MsgServer.MsgMessage("--------------------------------", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                        SendMapPacket(separatorMsg3.GetArray(stream));

                        string timerText = Process == ProcesType.Idle
                            ? $"[Fight starts in]: {(StartTimer.AddMinutes(3) - DateTime.Now).ToString(@"mm\:ss")}"
                            : $"[Time left]: {(StartTimer.AddMinutes(15) - DateTime.Now).ToString(@"mm\:ss")}";
                        var timerMsg = new MsgServer.MsgMessage(timerText, MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                        SendMapPacket(timerMsg.GetArray(stream));
                    }
                    InfoTimer = DateTime.Now;
                }
                #endregion
            }

            if (Process == ProcesType.Idle)
            {
                if (DateTime.Now > StartTimer.AddMinutes(3))
                {
                    MsgSchedules.SendSysMesage("[ Guild Survival ] event has started TwinCity - Signup", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.white);
                    Process = ProcesType.Alive;
                    StartTimer = DateTime.Now;
                    InfoTimer = DateTime.Now; // Resetar InfoTimer ao mudar para Alive
                }
                Time32 Timer = Time32.Now;
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    foreach (var user in MapPlayers())
                    {
                        if (user.Player.Alive == false)
                        {
                            if (user.Player.DeadStamp.AddSeconds(4) < Timer)
                            {
                                ushort x = 0;
                                ushort y = 0;
                                Map.GetRandCoord(ref x, ref y);
                                user.Teleport(x, y, Map.ID, DinamicMap);
                                user.Player.Revive(stream);
                            }
                        }
                    }
                }
            }

            if (Process == ProcesType.Alive)
            {
                if (DateTime.Now > StartTimer.AddMinutes(15))
                {
                    var winnerGuild = GuildLives.OrderByDescending(g => g.Value).FirstOrDefault();
                    if (winnerGuild.Value > 0)
                    {
                        var guildName = "Unknown";
                        var players = MapPlayers();
                        var player = players.FirstOrDefault(p => p.Player.GuildID == winnerGuild.Key);
                        if (player != null && player.Player.MyGuild != null)
                            guildName = player.Player.MyGuild.GuildName;

                        MsgSchedules.SendSysMesage($"[EVENT] Guild {guildName} win [ Guild Survival ] with {winnerGuild.Value} lives remaining!", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.white);

                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            foreach (var user in players.Where(p => p.Player.GuildID == winnerGuild.Key))
                            {
                                user.Player.Money += 200000; // exemplo: 200k silver
                                user.Player.SendUpdate(stream, user.Player.Money, MsgServer.MsgUpdate.DataType.Money);

                                MsgSchedules.SendSysMesage($"{user.Player.Name} received 200,000 silver for winning Guild Survival!", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.yellow);
                                Program.DiscordAPIwinners.Enqueue("``[" + user.Player.Name + "] received 200,000 silver for winning Guild Survival!``");
                            }
                        }
                    }

                    // Teleportar todos de volta
                    foreach (var user in MapPlayers())
                    {
                        user.Teleport(428, 378, 1002);
                        MsgSchedules.SendSysMesage($"{user.Player.Name} He was teleported back to Twin City.", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.white);
                    }
                    MsgSchedules.SendSysMesage("[ Guild Survival ] has ended. Players have been teleported back to Twin City.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.white);
                    Process = ProcesType.Dead;
                }

                var aliveGuilds = GuildLives.Where(g => g.Value > 0).ToList();
                if (aliveGuilds.Count == 1)
                {
                    var winnerGuild = aliveGuilds.First();
                    var guildName = "Unknown";
                    var players = MapPlayers();
                    var player = players.FirstOrDefault(p => p.Player.GuildID == winnerGuild.Key);
                    if (player != null && player.Player.MyGuild != null)
                        guildName = player.Player.MyGuild.GuildName;

                    MsgSchedules.SendSysMesage($"[EVENT] Guild {guildName} win Guild Survival with {winnerGuild.Value} lives remaining!", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.white);

                    // Distribuir prêmios para os jogadores da guilda vencedora
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        foreach (var user in players.Where(p => p.Player.GuildID == winnerGuild.Key))
                        {
                            uint silverReward = 200000; // recompensa
                            user.Player.Money += silverReward;
                            user.Player.SendUpdate(stream, user.Player.Money, MsgServer.MsgUpdate.DataType.Money);

                            MsgSchedules.SendSysMesage($"{user.Player.Name} received {silverReward} silver for winning Guild Survival!", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.yellow);
                            Program.DiscordAPIwinners.Enqueue("``[" + user.Player.Name + "] received " + silverReward + " silver for winning Guild Survival!``");
                        }
                    }

                    // Teleportar todos de volta
                    foreach (var user in MapPlayers())
                    {
                        user.Teleport(428, 378, 1002);
                    }
                    Process = ProcesType.Dead;
                }

                Time32 Timer = Time32.Now;
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    foreach (var user in MapPlayers())
                    {
                        if (user.Player.Alive == false)
                        {
                            if (user.Player.DeadStamp.AddSeconds(4) < Timer)
                            {
                                if (GuildLives.ContainsKey(user.Player.GuildID) && GuildLives[user.Player.GuildID] > 1)
                                {
                                    GuildLives[user.Player.GuildID]--;
                                    ushort x = 0;
                                    ushort y = 0;
                                    Map.GetRandCoord(ref x, ref y);
                                    user.Teleport(x, y, Map.ID, DinamicMap);
                                    user.Player.Revive(stream);
                                    MsgSchedules.SendSysMesage($"{user.Player.Name}'s guild lost a life! Remaining: {GuildLives[user.Player.GuildID]}", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.yellow);
                                }
                                else if (GuildLives.ContainsKey(user.Player.GuildID))
                                {
                                    GuildLives[user.Player.GuildID] = 0;
                                    user.Teleport(428, 378, 1002);
                                    MsgSchedules.SendSysMesage($"{user.Player.Name}'s guild has no lives left and is out!", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.white);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SendMapPacket(ServerSockets.Packet packet)
        {
            var players = MapPlayers();
            if (players.Length == 0)
            {
                return;
            }
            foreach (var user in players)
            {
                try
                {
                    user.Send(packet);
                }
                catch (Exception e)
                {
                    // Exceção silenciosa
                }
            }
        }

        public Client.GameClient[] MapPlayers()
        {
            if (Map == null)
            {
                return new Client.GameClient[0];
            }
            return Map.Values.Where(p => p.Player.DynamicID == DinamicMap && p.Player.Map == Map.ID).ToArray();
        }

        public bool InTournament(Client.GameClient user)
        {
            if (Map == null) return false;
            return user.Player.Map == Map.ID && user.Player.DynamicID == DinamicMap;
        }
    }
}