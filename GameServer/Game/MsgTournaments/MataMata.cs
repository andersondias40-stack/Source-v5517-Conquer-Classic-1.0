using System;
using System.Collections.Generic;
using System.Linq;
using COServer.Game.MsgServer;
using static COServer.Game.MsgServer.MsgMessage;
using static COServer.Game.MsgServer.MsgStringPacket;

namespace COServer.Game.MsgTournaments
{
    public class MataMata
    {
        private List<Client.GameClient> QueroIrPlayers = new List<Client.GameClient>();
        private List<Client.GameClient> CurrentRoundPlayers = new List<Client.GameClient>();
        private Dictionary<Client.GameClient, int> FightLives = new Dictionary<Client.GameClient, int>();
        private bool IsEventActive = false;
        private bool IsMatchStarted = false;
        private Client.GameClient[] CurrentFighters = new Client.GameClient[2];
        private int CurrentRound = 1;
        private DateTime InfoTimer = DateTime.Now;
        private DateTime StartTimer = DateTime.MinValue;
        private DateTime NextFightTimer = DateTime.MinValue;
        private int Fighter1Score = 0;
        private int Fighter2Score = 0;
        private int InitialLives = 0;

        private const ushort WaitingMapId = 1601;
        private const ushort FightMapId = 1571;
        private const ushort WaitingX = 200;
        private const ushort WaitingY = 200;
        private const ushort FightX = 50;
        private const ushort FightY = 50;

        public MataMata()
        {
            Console.WriteLine("MataMata configurado: Espera=1601 (200,200), Luta=1571 (50,50)");
        }

        public void StartEvent(Client.GameClient gm)
        {
            if (IsEventActive)
            {
                gm.SendSysMesage("O evento já está ativo!");
                return;
            }
            IsEventActive = true;
            CurrentRound = 1;
            MsgSchedules.SendSysMesage("O evento Mata-Mata OrigensCO começou! Digite /queroir para participar.", ChatMode.Center, MsgColor.red);
            Program.DiscordAPIMataMata.Enqueue($"```diff\n+ 🎉 O evento Mata-Mata OrigensCO vai começar! Digite /queroir para participar.```");
        }

        public void JoinEvent(Client.GameClient player)
        {
            if (!IsEventActive || IsMatchStarted)
            {
                player.SendSysMesage("Evento não está aceitando inscrições.");
                return;
            }
            if (QueroIrPlayers.Contains(player))
            {
                player.SendSysMesage("Você já está inscrito no Mata-Mata OrigensCO!");
                return;
            }
            QueroIrPlayers.Add(player);
            player.SendSysMesage("Você está inscrito! Aguarde o GM para ser levado ao mapa do Mata-Mata OrigensCO.");
        }

        public void MoveAllPlayers(Client.GameClient gm)
        {
            if (!IsEventActive)
            {
                gm.SendSysMesage("O evento Mata-Mata OrigensCO não foi iniciado.");
                return;
            }
            if (QueroIrPlayers.Count == 0)
            {
                gm.SendSysMesage("Nenhum jogador inscrito para mover!");
                return;
            }

            int movedCount = 0;
            foreach (var player in QueroIrPlayers.ToList())
            {
                if (player != null && player.Player != null)
                {
                    player.Teleport(WaitingX, WaitingY, WaitingMapId, 0);
                    movedCount++;
                }
            }

            MsgSchedules.SendSysMesage($"Jogadores movidos para a área do Mata Mata! ({movedCount}Jogadores)", ChatMode.Center, MsgColor.red);

            StartTimer = DateTime.Now;
            MsgSchedules.SendSysMesage("As lutas começarão em 1 minuto!", ChatMode.Center, MsgColor.red);
            Program.DiscordAPIMataMata.Enqueue("```As lutas começarão em 1 minuto!```");
        }

        public void StartMatch(Client.GameClient gm)
        {
            if (!IsEventActive)
            {
                gm.SendSysMesage("O evento Mata-Mata não foi iniciado.");
                return;
            }
            if (QueroIrPlayers.Count + CurrentRoundPlayers.Count < 2)
            {
                gm.SendSysMesage("É necessário pelo menos 2 jogadores inscritos!");
                return;
            }
            IsMatchStarted = true;
            CurrentRoundPlayers.AddRange(QueroIrPlayers);
            QueroIrPlayers.Clear();
            MsgSchedules.SendSysMesage($"Round [0{CurrentRound}] Começou!", ChatMode.Center, MsgColor.red);
            Program.DiscordAPIMataMata.Enqueue($"```Round [0{CurrentRound}] Começou```");
            StartNextRound();
        }

        private void StartNextRound()
        {
            if (NextFightTimer != DateTime.MinValue && DateTime.Now < NextFightTimer.AddSeconds(30))
            {
                return;
            }

            if (CurrentRoundPlayers.Count == 0 && QueroIrPlayers.Count >= 2)
            {
                CurrentRound++;
                CurrentRoundPlayers.AddRange(QueroIrPlayers);
                QueroIrPlayers.Clear();
                MsgSchedules.SendSysMesage($"Round [0{CurrentRound}] Começou!", ChatMode.Center, MsgColor.red);
                Program.DiscordAPIMataMata.Enqueue($"```Round [0{CurrentRound}] Começou```");
            }

            if (CurrentRoundPlayers.Count >= 2)
            {
                CurrentFighters[0] = CurrentRoundPlayers[0];
                CurrentFighters[1] = CurrentRoundPlayers[1];
                CurrentRoundPlayers.RemoveRange(0, 2);

                InitialLives = (CurrentRoundPlayers.Count == 0 && QueroIrPlayers.Count <= 1) ? 5 : 3;
                FightLives[CurrentFighters[0]] = InitialLives;
                FightLives[CurrentFighters[1]] = InitialLives;
                Fighter1Score = 0;
                Fighter2Score = 0;

                CurrentFighters[0].Teleport(FightX, FightY, FightMapId, 0);
                CurrentFighters[1].Teleport(FightX, FightY, FightMapId, 0);

                MsgSchedules.SendSysMesage($"[Round {CurrentRound}] - {CurrentFighters[0].Player.Name} vs {CurrentFighters[1].Player.Name}!", ChatMode.Center, MsgColor.red);

                Program.DiscordAPIMataMata.Enqueue($"```diff\n+ [Round {CurrentRound}] - {CurrentFighters[0].Player.Name} vs {CurrentFighters[1].Player.Name}```");
            }
            else if (CurrentRoundPlayers.Count == 1 && QueroIrPlayers.Count == 0)
            {
                EndEventWithSinglePlayer();
            }
            else if (CurrentRoundPlayers.Count == 0 && QueroIrPlayers.Count == 1)
            {
                EndEvent();
            }
            else if (CurrentRoundPlayers.Count == 0 && QueroIrPlayers.Count == 0)
            {
                MsgSchedules.SendSysMesage("Nenhum jogador restante no Mata-Mata. Evento encerrado.", ChatMode.Center, MsgColor.red);
                ResetEvent();
            }
        }

        private void CheckFightResult(Client.GameClient loser, string reason = "perdeu todas as vidas")
        {
            Client.GameClient winner = (loser == CurrentFighters[0]) ? CurrentFighters[1] : CurrentFighters[0];
            if (winner == null) return;

            // Corrigindo a lógica para mostrar as vidas restantes
            int winnerLives = FightLives[winner];
            int loserLives = FightLives[loser];

            MsgSchedules.SendSysMesage($"[Round {CurrentRound}] - Resultado Final: {winner.Player.Name} {winnerLives} x {loser.Player.Name} {loserLives}. {winner.Player.Name} venceu!", ChatMode.Center, MsgColor.red);
            Program.DiscordAPIMataMata.Enqueue($"```diff\n+ [Round {CurrentRound}] - Resultado Final:\n\n" +
                                                $"{winner.Player.Name} {winnerLives} x {loserLives} {loser.Player.Name}\n\n" +
                                                $"{winner.Player.Name} venceu!\n\n" +
                                                $"⏰ Hora: {DateTime.Now:yyyy-MM-dd HH:mm}```");


            winner.Teleport(WaitingX, WaitingY, WaitingMapId, 0);
            QueroIrPlayers.Add(winner);

            FightLives.Remove(loser);
            CurrentFighters[0] = null;
            CurrentFighters[1] = null;
            NextFightTimer = DateTime.Now;

            if (CurrentRoundPlayers.Count == 0 && QueroIrPlayers.Count == 1)
            {
                EndEvent();
            }
            else
            {
                MsgSchedules.SendSysMesage("Próxima luta em 30 segundos!", ChatMode.Center, MsgColor.red);
            }
        }

        private void EndEventWithSinglePlayer()
        {
            if (CurrentRoundPlayers.Count == 1)
            {
                var winner = CurrentRoundPlayers[0];
                winner.Teleport(428, 378, 1002, 0);
                MsgSchedules.SendSysMesage($"{winner.Player.Name} venceu o Mata-Mata por ser o único participante!", ChatMode.Center, MsgColor.red);
                Program.DiscordAPIMataMata.Enqueue($"```diff\n+ {winner.Player.Name} venceu o Mata-Mata OrigensCO ! 🎉```");
            }
            ResetEvent();
        }

        private void EndEvent()
        {
            if (QueroIrPlayers.Count == 1)
            {
                var winner = QueroIrPlayers[0];
                winner.Teleport(200, 200, 1601, 0);
                MsgSchedules.SendSysMesage($"[{winner.Player.Name}] venceu o Mata-Mata OrigensCO.!", ChatMode.Center, MsgColor.red);
            }
            ResetEvent();
        }

        public void CheckUp()
        {
            if (!IsEventActive) return;

            if (StartTimer != DateTime.MinValue && !IsMatchStarted && DateTime.Now > StartTimer.AddMinutes(1))
            {
                StartMatch(null);
            }

            if (DateTime.Now > InfoTimer.AddSeconds(1))
            {
                DisplayWaitingInfo();
                DisplayFightInfo();
                InfoTimer = DateTime.Now;
            }

            if (IsMatchStarted && (CurrentFighters[0] != null || CurrentFighters[1] != null))
            {
                Time32 timer = Time32.Now;
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();

                    if (CurrentFighters[0] == null || CurrentFighters[0].Player.Map != FightMapId)
                    {
                        if (CurrentFighters[1] != null && CurrentFighters[1].Player.Map == FightMapId)
                        {
                            Fighter2Score = InitialLives;
                            CheckFightResult(CurrentFighters[0], "desconectou ou saiu do mapa");
                        }
                        return;
                    }
                    if (CurrentFighters[1] == null || CurrentFighters[1].Player.Map != FightMapId)
                    {
                        if (CurrentFighters[0] != null && CurrentFighters[0].Player.Map == FightMapId)
                        {
                            Fighter1Score = InitialLives;
                            CheckFightResult(CurrentFighters[1], "desconectou ou saiu do mapa");
                        }
                        return;
                    }

                    if (!CurrentFighters[0].Player.Alive && CurrentFighters[0].Player.DeadStamp.AddSeconds(4) < timer)
                    {
                        if (FightLives[CurrentFighters[0]] > 1)
                        {
                            FightLives[CurrentFighters[0]]--;
                            Fighter2Score++;
                            MsgSchedules.SendSysMesage($"[Round {CurrentRound}] {CurrentFighters[1].Player.Name} {Fighter2Score} x {Fighter1Score} {CurrentFighters[0].Player.Name}", ChatMode.Center, MsgColor.red);
                            CurrentFighters[0].Teleport(FightX, FightY, FightMapId, 0);
                            CurrentFighters[0].Player.Revive(stream);
                        }
                        else
                        {
                            FightLives[CurrentFighters[0]] = 0;
                            Fighter2Score = InitialLives;
                            CurrentFighters[0].Teleport(428, 378, 1002, 0); // Teleporta para Twin City
                            CurrentFighters[0].Player.Revive(stream); // Revive antes de finalizar
                            CheckFightResult(CurrentFighters[0]);
                        }
                    }
                    else if (!CurrentFighters[1].Player.Alive && CurrentFighters[1].Player.DeadStamp.AddSeconds(4) < timer)
                    {
                        if (FightLives[CurrentFighters[1]] > 1)
                        {
                            FightLives[CurrentFighters[1]]--;
                            Fighter1Score++;
                            MsgSchedules.SendSysMesage($"Round {CurrentRound}: {CurrentFighters[0].Player.Name} {Fighter1Score} x {Fighter2Score} {CurrentFighters[1].Player.Name}", ChatMode.Center, MsgColor.red);
                            CurrentFighters[1].Teleport(FightX, FightY, FightMapId, 0);
                            CurrentFighters[1].Player.Revive(stream);
                        }
                        else
                        {
                            FightLives[CurrentFighters[1]] = 0;
                            Fighter1Score = InitialLives;
                            CurrentFighters[1].Teleport(428, 378, 1002, 0); // Teleporta para Twin City
                            CurrentFighters[1].Player.Revive(stream); // Revive antes de finalizar
                            CheckFightResult(CurrentFighters[1]);
                        }
                    }
                }
            }

            if (NextFightTimer != DateTime.MinValue && DateTime.Now > NextFightTimer.AddSeconds(30))
            {
                NextFightTimer = DateTime.MinValue;
                StartNextRound();
            }
        }

        private void DisplayWaitingInfo()
        {
            var waitingPlayers = QueroIrPlayers.Concat(CurrentRoundPlayers).ToList();
            if (waitingPlayers.Count == 0) return;

            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();

                SendToMap(WaitingMapId, new MsgMessage("  [MATA-MATA ESPERA]  ", MsgColor.yellow, ChatMode.FirstRightCorner).GetArray(stream));
                SendToMap(WaitingMapId, new MsgMessage("--------------------------------", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                SendToMap(WaitingMapId, new MsgMessage("[PLAYERS]       [ROUND]", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));

                foreach (var player in waitingPlayers)
                {
                    SendToMap(WaitingMapId, new MsgMessage($"{player.Player.Name}       Round {CurrentRound}", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                }

                if (StartTimer != DateTime.MinValue && !IsMatchStarted)
                {
                    var timeLeft = (StartTimer.AddMinutes(1) - DateTime.Now).ToString(@"mm\:ss");
                    SendToMap(WaitingMapId, new MsgMessage($"[Lutas em]: {timeLeft}", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                }
                else if (NextFightTimer != DateTime.MinValue)
                {
                    var timeLeft = (NextFightTimer.AddSeconds(30) - DateTime.Now).ToString(@"ss");
                    SendToMap(WaitingMapId, new MsgMessage($"[Próxima luta em]: {timeLeft}s", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                }

                SendToMap(WaitingMapId, new MsgMessage("--------------------------------", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
            }
        }

        private void DisplayFightInfo()
        {
            if (CurrentFighters[0] == null || CurrentFighters[1] == null) return;

            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();

                SendToMap(FightMapId, new MsgMessage("  [MATA-MATA LUTA]  ", MsgColor.yellow, ChatMode.FirstRightCorner).GetArray(stream));
                SendToMap(FightMapId, new MsgMessage("--------------------------------", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                SendToMap(FightMapId, new MsgMessage("[FIGHTERS]       [LIVES]", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                SendToMap(FightMapId, new MsgMessage($"{CurrentFighters[0].Player.Name}       {FightLives[CurrentFighters[0]]} Lives", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                SendToMap(FightMapId, new MsgMessage($"{CurrentFighters[1].Player.Name}       {FightLives[CurrentFighters[1]]} Lives", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                SendToMap(FightMapId, new MsgMessage("--------------------------------", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
            }
        }

        private void SendToMap(ushort mapId, ServerSockets.Packet packet)
        {
            foreach (var player in Database.Server.GamePoll.Values.Where(p => p.Player != null && p.Player.Map == mapId))
            {
                try
                {
                    player.Send(packet);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Erro ao enviar pacote para {player.Player.Name}: {e.Message}");
                }
            }
        }

        private void ResetEvent()
        {
            IsEventActive = false;
            IsMatchStarted = false;
            QueroIrPlayers.Clear();
            CurrentRoundPlayers.Clear();
            FightLives.Clear();
            CurrentRound = 1;
            StartTimer = DateTime.MinValue;
            NextFightTimer = DateTime.MinValue;
            Fighter1Score = 0;
            Fighter2Score = 0;
            InitialLives = 0;
            CurrentFighters[0] = null;
            CurrentFighters[1] = null;
        }
    }
}