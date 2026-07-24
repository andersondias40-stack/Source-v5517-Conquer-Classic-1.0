//using COServer.Database;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace COServer.Game.MsgTournaments
//{
//    public class ArenaDuel
//    {
//        public const uint Map = 1005;
//        private ProcesType Mode;
//        private DateTime FinishTimer = new DateTime();
//        private string Title = "ArenaDuel";
//        public uint WinnerUID = 0;
//        DateTime lastSent = DateTime.Now;
//        List<string> score = new List<string>();

//        // Lista para armazenar jogadores que já participaram
//        private HashSet<uint> participants = new HashSet<uint>();

//        public ArenaDuel()
//        {
//            Mode = ProcesType.Dead;
//            if (!Program.OutMap.Contains(Map))
//                Program.OutMap.Add(Map);
//            if (!Program.FreePkMap.Contains(Map))
//                Program.FreePkMap.Add(Map);
//            if (!Program.NoDrugMap.Contains(Map))
//                Program.NoDrugMap.Add(Map);
//            if (!Program.SsFbMap.Contains(Map))
//                Program.SsFbMap.Add(Map);
//        }

//        public void Open()
//        {
//            if (Mode == ProcesType.Dead)
//            {
//                Mode = ProcesType.Alive;
//                MsgSchedules.SendInvitation("ArenaDuel", 437, 354, 1002, 0, 60);
//                MsgSchedules.SendSysMesage("" + Title + " has started!", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);
//                FinishTimer = DateTime.Now.AddMinutes(30); // Mudado para 30 minutos diretamente
//                participants.Clear(); // Limpa a lista de participantes quando o evento começa
//            }
//        }

//        public bool AllowJoin(Client.GameClient user, ServerSockets.Packet stream)
//        {
//            // Verifica se o jogador já participou
//            if (Mode == ProcesType.Alive && !participants.Contains(user.Player.UID))
//            {
//                ushort x = 0;
//                ushort y = 0;
//                Server.ServerMaps[Map].GetRandCoord(ref x, ref y);
//                user.Teleport(x, y, Map, 999);
//                user.Player.HitPoints = 1;

//                // Adiciona o jogador à lista de participantes
//                participants.Add(user.Player.UID);
//                return true;
//            }
//            return false; // Bloqueia a entrada se o jogador já tiver participado
//        }

//        public void CheckUp()
//        {
//            if (Mode == ProcesType.Alive)
//            {
//                score.Clear();
//                int rank = 0;
//                foreach (var player in Server.GamePoll.Values
//                    .Where(e => e.Player.Map == Map)
//                    .OrderByDescending(e => e.Player.ArenaDuel_Hits).Take(5))
//                {
//                    if (!player.Player.Alive && DateTime.Now > player.DeathHit.AddSeconds(5))
//                    {
//                        using (var rec = new ServerSockets.RecycledPacket())
//                        {
//                            var stream = rec.GetStream();
//                            if (!MsgSchedules._ArenaDuel.AllowJoin(player, stream))
//                            {
//                                player.Player.Revive(stream);
//                                player.Teleport(428, 378, 1002);
//                            }
//                        }
//                    }
//                    rank++;
//                    score.Add(player.Player.Name + " : " + player.Player.ArenaDuel_Hits + " hits.");
//                    if (DateTime.Now > FinishTimer || player.Player.ArenaDuel_Hits >= 100)
//                    {
//                        if (rank == 1)
//                        {
//                            using (var rec = new ServerSockets.RecycledPacket())
//                            {
//                                var stream = rec.GetStream();
//                                GiveReward(player, stream);
//                            }
//                        }
//                    }
//                }
//                if (DateTime.Now > lastSent.AddSeconds(2))
//                {
//                    SendScore(score);
//                    lastSent = DateTime.Now;
//                }
//                if (DateTime.Now > FinishTimer)
//                {
//                    Mode = ProcesType.Dead;
//                }
//            }
//            if (DateTime.Now.Minute == 15 && DateTime.Now.Second < 2)
//            {
//                Open();
//            }
//        }

//        public bool IsFinished() { return Mode == ProcesType.Dead; }

//        public bool TheLastPlayer()
//        {
//            return Server.GamePoll.Values.Where(p => p.Player.Map == Map && p.Player.DynamicID == 999 && p.Player.Alive).Count() == 1;
//        }

//        public void GiveReward(Client.GameClient client, ServerSockets.Packet stream)
//        {
//            client.SendSysMesage("You received a DragonBallScroll.", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);
//            MsgSchedules.SendSysMesage("" + client.Player.Name + " has won " + Title + " and received a DragonBallScroll!", MsgServer.MsgMessage.ChatMode.TopLeftSystem, MsgServer.MsgMessage.MsgColor.white);
//            client.Player.HitPoints = (int)client.Status.MaxHitpoints;
//            client.Teleport(428, 378, 1002);
//        }

//        public void SendScore(List<string> text)
//        {
//            using (var rec = new ServerSockets.RecycledPacket())
//            {
//                var stream = rec.GetStream();
//                foreach (var C in Server.GamePoll.Values.Where(e => e.Player.Map == Map && e.Player.DynamicID == 999))
//                {
//                    C.Send(new Game.MsgServer.MsgMessage("ArenaDuel - Hits", "ALLUSERS", "SYSTEM", Game.MsgServer.MsgMessage.MsgColor.red, Game.MsgServer.MsgMessage.ChatMode.FirstRightCorner).GetArray(stream));
//                    C.Send(new Game.MsgServer.MsgMessage("My Hits : " + C.Player.ArenaDuel_Hits, "ALLUSERS", "SYSTEM", Game.MsgServer.MsgMessage.MsgColor.red, Game.MsgServer.MsgMessage.ChatMode.ContinueRightCorner).GetArray(stream));
//                    foreach (string t in text)
//                        C.Send(new Game.MsgServer.MsgMessage(t, "ALLUSERS", "SYSTEM", Game.MsgServer.MsgMessage.MsgColor.red, Game.MsgServer.MsgMessage.ChatMode.ContinueRightCorner).GetArray(stream));
//                }
//            }
//        }
//    }
//}
