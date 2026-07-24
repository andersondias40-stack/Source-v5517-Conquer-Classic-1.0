using System;
using System.Linq;

namespace COServer.Game.MsgTournaments
{
    public class MsgPkWar
    {
        public const int RewardConquerPoints = 860;
        public const uint MapID = 1508;
        private ProcesType Mode;
        private DateTime StartTimer = new DateTime();
        public DateTime ScoreStamp = new DateTime();
        public Role.GameMap Map;
        public uint WinnerUID = 0;
        public int Duration = 0;

        public MsgPkWar()
        {
            Mode = ProcesType.Dead;
        }

        public void Open()
        {
            if (Mode == ProcesType.Dead)
            {
                Mode = ProcesType.Idle;
                Map = Database.Server.ServerMaps[MapID];
                MsgSchedules.SendSysMesage("DeathMatch Pk War started! Join now!", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.white);
                StartTimer = DateTime.Now;
                Duration = 15 * 60; // 15 minutos para Idle
                ScoreStamp = DateTime.MinValue; // Forçar atualização imediata
            }
        }

        public bool AllowJoin()
        {
            return Mode == ProcesType.Idle; // Permite entrada durante os 15 minutos de Idle
        }

        public void Join(Client.GameClient user, ServerSockets.Packet stream)
        {
            if (AllowJoin() && !InTournament(user))
            {
                ushort x = 0, y = 0;
                Map.GetRandCoord(ref x, ref y);
                user.Teleport(x, y, Map.ID);

                ShowScoreAndStatus(user, stream); // Mostrar score imediatamente ao entrar
            }
        }

        public void CheckUp()
        {
            if (Mode != ProcesType.Dead && Map != null)
            {
                #region Score and Timer Display
                if (DateTime.Now > ScoreStamp.AddSeconds(1)) // Atualiza a cada segundo
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();

                        string headerText = Mode == ProcesType.Idle ? "  [LIVE]  " : "  [LASTMIN]  ";
                        var headerMsg = new MsgServer.MsgMessage(headerText, MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.FirstRightCorner);
                        SendMapPacket(headerMsg.GetArray(stream));

                        var separatorMsg = new MsgServer.MsgMessage("--------------------------------", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                        SendMapPacket(separatorMsg.GetArray(stream));

                        var playersTitleMsg = new MsgServer.MsgMessage("[PLAYERS]            [SCORE]", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                        SendMapPacket(playersTitleMsg.GetArray(stream));

                        var players = MapPlayers();
                        foreach (var user in players.OrderByDescending(p => p.Player.PkWarScore))
                        {
                            var scoreMsg = new MsgServer.MsgMessage($"{user.Player.Name}:            {user.Player.PkWarScore}", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                            SendMapPacket(scoreMsg.GetArray(stream));
                        }

                        var separatorMsg1 = new MsgServer.MsgMessage("--------------------------------", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                        SendMapPacket(separatorMsg1.GetArray(stream));

                        string timerText = Mode == ProcesType.Idle
                            ? $"[Fight time]: {(StartTimer.AddMinutes(15) - DateTime.Now).ToString(@"mm\:ss")}"
                            : $"[Left min]: {(StartTimer.AddMinutes(1) - DateTime.Now).ToString(@"mm\:ss")}";
                        var timerMsg = new MsgServer.MsgMessage(timerText, MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                        SendMapPacket(timerMsg.GetArray(stream));
                    }
                    ScoreStamp = DateTime.Now;
                }
                #endregion
            }

            if (Mode == ProcesType.Idle)
            {
                if (DateTime.Now > StartTimer.AddMinutes(15)) // 15 minutos para Idle
                {
                    Mode = ProcesType.Alive;
                    StartTimer = DateTime.Now; // Reinicia o timer para Alive
                    MsgSchedules.SendSysMesage("PK DeathMatch final minute! Fight to the top!", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.white);
                }

                Time32 Timer = Time32.Now;
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    foreach (var user in MapPlayers())
                    {
                        if (!user.Player.Alive)
                        {
                            if (user.Player.DeadStamp.AddSeconds(4) < Timer)
                            {
                                ushort x = 0, y = 0;
                                Map.GetRandCoord(ref x, ref y);
                                user.Teleport(x, y, Map.ID);
                                user.Player.Revive(stream);
                            }
                        }
                    }
                }
            }
            else if (Mode == ProcesType.Alive)
            {
                if (DateTime.Now > StartTimer.AddMinutes(1) || TheLastPlayer()) // 1 minuto de Alive
                {
                    EndTournament();
                }

                Time32 Timer = Time32.Now;
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    foreach (var user in MapPlayers())
                    {
                        if (!user.Player.Alive)
                        {
                            if (user.Player.DeadStamp.AddSeconds(4) < Timer)
                            {
                                ushort x = 0, y = 0;
                                Map.GetRandCoord(ref x, ref y);
                                user.Teleport(x, y, Map.ID);
                                user.Player.Revive(stream);
                            }
                        }
                    }
                }
            }
        }

        private void ShowScoreAndStatus(Client.GameClient user, ServerSockets.Packet stream)
        {
            string headerText = Mode == ProcesType.Idle ? "  [LIVE]  " : "  [LASTMIN]  ";
            var headerMsg = new MsgServer.MsgMessage(headerText, MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.FirstRightCorner);
            user.Send(headerMsg.GetArray(stream));

            var separatorMsg = new MsgServer.MsgMessage("--------------------------------", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
            user.Send(separatorMsg.GetArray(stream));

            var playersTitleMsg = new MsgServer.MsgMessage("[PLAYERS]            [SCORE]", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
            user.Send(playersTitleMsg.GetArray(stream));

            var players = MapPlayers();
            foreach (var p in players.OrderByDescending(p => p.Player.PkWarScore))
            {
                var scoreMsg = new MsgServer.MsgMessage($"{p.Player.Name}:            {p.Player.PkWarScore}", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                user.Send(scoreMsg.GetArray(stream));
            }

            var separatorMsg1 = new MsgServer.MsgMessage("--------------------------------", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
            user.Send(separatorMsg1.GetArray(stream));

            string timerText = Mode == ProcesType.Idle
                ? $"[Fight time]: {(StartTimer.AddMinutes(15) - DateTime.Now).ToString(@"mm\:ss")}"
                : $"[Left min]: {(StartTimer.AddMinutes(1) - DateTime.Now).ToString(@"mm\:ss")}";
            var timerMsg = new MsgServer.MsgMessage(timerText, MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
            user.Send(timerMsg.GetArray(stream));
        }

        private void EndTournament()
        {
            Mode = ProcesType.Dead;
            var players = MapPlayers();
            if (players.Length > 0)
            {
                var winner = players.OrderByDescending(p => p.Player.PkWarScore).First();
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    GiveReward(winner, stream);
                }
            }

            foreach (var user in players)
            {
                user.Teleport(428, 378, 1002);
                MsgSchedules.SendSysMesage($"{user.Player.Name} was teleported back to Twin City.", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.white);
            }
            MsgSchedules.SendSysMesage("PK DeathMatch has ended!", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.white);
        }

        public bool InTournament(Client.GameClient user)
        {
            return Map != null && user.Player.Map == Map.ID;
        }

        public void Revive(Time32 Timer, Client.GameClient user)
        {
            if (!user.Player.Alive && Mode != ProcesType.Dead && InTournament(user))
            {
                if (user.Player.DeadStamp.AddSeconds(4) < Timer)
                {
                    ushort x = 0, y = 0;
                    Map.GetRandCoord(ref x, ref y);
                    user.Teleport(x, y, Map.ID);
                }
            }
        }

        public Client.GameClient[] MapPlayers()
        {
            return Map?.Values.Where(p => InTournament(p)).ToArray() ?? new Client.GameClient[0];
        }

        public void SendMapPacket(ServerSockets.Packet stream)
        {
            foreach (var user in MapPlayers())
                user.Send(stream);
        }

        public bool IsFinished()
        {
            return Mode == ProcesType.Dead;
        }

        public bool TheLastPlayer()
        {
            return Database.Server.GamePoll.Values.Count(p => p.Player.Map == 1508 && p.Player.Alive) == 1;
        }

        public void GiveReward(Client.GameClient client, ServerSockets.Packet stream)
        {
            WinnerUID = client.Player.UID;

            // Quantidade de silver que o jogador vai receber
            uint silverAmount = 200000; // silver

            // Mensagem para todos os jogadores
            MsgSchedules.SendSysMesage($"{client.Player.Name} Win PK DeathMatch, he/she received Top and {silverAmount} silver.",
                MsgServer.MsgMessage.ChatMode.TopLeftSystem, MsgServer.MsgMessage.MsgColor.white);

            Program.DiscordAPIwinners.Enqueue($"``[{client.Player.Name}] Win PK DeathMatch, he/she received Top and {silverAmount} silver.``");

            // Adicionar silver diretamente ao jogador
            client.Player.Money += silverAmount;

            // flag de campeão
            AddTop(client);

            client.Teleport(430, 269, 1002, 0);
        }

        public void AddTop(Client.GameClient client)
        {
            if (WinnerUID == client.Player.UID)
                client.Player.AddFlag(MsgServer.MsgUpdate.Flags.WeeklyPKChampion, Role.StatusFlagsBigVector32.PermanentFlag, false);
        }

        public void RegisterKill(Client.GameClient killer, Client.GameClient victim)
        {
            if ((Mode == ProcesType.Idle || Mode == ProcesType.Alive) && InTournament(killer) && InTournament(victim))
            {
                killer.Player.PkWarScore += 1;
                MsgSchedules.SendSysMesage($"{killer.Player.Name} killed {victim.Player.Name} and earned 1 point!",
                    MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.yellow);
            }
        }
    }
}
