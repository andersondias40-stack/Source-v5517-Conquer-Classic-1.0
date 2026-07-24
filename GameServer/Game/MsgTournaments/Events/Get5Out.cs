using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using COServer.Client;

namespace COServer.Game.MsgTournaments
{
    public class Get5Out
    {
        public const int RewardConquerPoints = 6000, FinishMinutes = 1;
        public const uint Map = 130;
        private ProcesType Mode;
        private DateTime FinishTimer = new DateTime();
        private string Title = "Five(n)out";
        public uint WinnerUID = 0;
        public DateTime StartTime;
        DateTime lastSent = DateTime.Now;
        List<string> score = new List<string>();

        public Get5Out()
        {
            Mode = ProcesType.Dead;
            if (!Program.OutMap.Contains(Map))
                Program.OutMap.Add(Map);
            if (!Program.FreePkMap.Contains(Map))
                Program.FreePkMap.Add(Map);
            if (!Program.NoDrugMap.Contains(Map))
                Program.NoDrugMap.Add(Map);
            if (!Program.SsFbMap.Contains(Map))
                Program.SsFbMap.Add(Map);
        }

        public void Open()
        {
            if (Mode == ProcesType.Dead)
            {
                FinishTimer = DateTime.Now.AddMinutes(FinishMinutes);
                Mode = ProcesType.Alive;

            }
        }

        public bool AllowJoin(Client.GameClient user, ServerSockets.Packet stream)
        {
            if (Mode == ProcesType.Alive)
            {
                if (user.Get5Out_points == 0 && user.Player.GetPoint == false)
                {
                    ushort x = 0;
                    ushort y = 0;
                    Database.Server.ServerMaps[Map].GetRandCoord(ref x, ref y);
                    user.Teleport(x, y, Map);
                    user.Player.HitPoints = 1;
                    user.Player.Get5OutPoint = 5;
                    user.Player.GetPoint = true;
                    return true;
                }
                if (user.Player.Get5OutPoint > 0)
                {
                    ushort x = 0;
                    ushort y = 0;
                    Database.Server.ServerMaps[Map].GetRandCoord(ref x, ref y);
                    user.Teleport(x, y, Map);
                    user.Player.HitPoints = 1;
                    return true;
                }
                else
                {
                    user.CreateDialog(stream, "Sorry, You have lost your point.", "I see.");
                }
            }
            return false;
        }

        public void CheckUp()
        {
            // Evento configurado para acontecer todos os dias às 20:00 no horário local do servidor
            if (DateTime.Now.Hour == 20 && DateTime.Now.Minute == 00 && DateTime.Now.Second < 2)
            {
                if (Mode == ProcesType.Dead)
                {
                    FinishTimer = DateTime.Now.AddMinutes(FinishMinutes);
                    Mode = ProcesType.Alive;
                }
            }

            if (Mode == ProcesType.Alive)
            {
                score.Clear();
                int rank = 0;
                foreach (var player in Database.Server.GamePoll.Values
                    .Where(e => e.Player.Map == Map)
                    .OrderByDescending(e => e.Player.Get5OutPoint).Take(5))
                {
                    rank++;
                    score.Add(player.Player.Name + " : " + player.Player.Get5OutPoint + " Lives.");
                    if (DateTime.Now > FinishTimer)
                    {
                        if (rank == 1)
                            WinnerUID = player.Player.UID;
                    }
                }
                if (DateTime.Now > lastSent.AddSeconds(2))
                {
                    SendScore(score);
                    lastSent = DateTime.Now;
                }
                if (DateTime.Now > FinishTimer)
                {
                    Mode = ProcesType.Dead;
                }
            }
        }

        public void SendScore(List<string> text)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                foreach (var C in Database.Server.GamePoll.Values.Where(e => e.Player.Map == Map))
                {
                    C.Send(new Game.MsgServer.MsgMessage("Five(n)out - Scores", "ALLUSERS", "SYSTEM", Game.MsgServer.MsgMessage.MsgColor.red, Game.MsgServer.MsgMessage.ChatMode.FirstRightCorner).GetArray(stream));
                    C.Send(new Game.MsgServer.MsgMessage("Lives Remaining : " + C.Player.Get5OutPoint, "ALLUSERS", "SYSTEM", Game.MsgServer.MsgMessage.MsgColor.red, Game.MsgServer.MsgMessage.ChatMode.ContinueRightCorner).GetArray(stream));
                    foreach (string t in text)
                        C.Send(new Game.MsgServer.MsgMessage(t, "ALLUSERS", "SYSTEM", Game.MsgServer.MsgMessage.MsgColor.red, Game.MsgServer.MsgMessage.ChatMode.ContinueRightCorner).GetArray(stream));
                }
            }
        }

        public bool IsFinished() { return Mode == ProcesType.Dead; }
        public bool IsStarted() { return Mode == ProcesType.Alive; }

        public bool TheLastPlayer()
        {
            return Database.Server.GamePoll.Values.Where(p => p.Player.Map == Map && p.Player.Alive).Count() == 1;
        }

        public void GiveReward(Client.GameClient client, ServerSockets.Packet stream)
        {
            if (client.Player.UID == WinnerUID)
            {
                client.SendSysMesage("You received 5k Cps.", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);
                MsgSchedules.SendSysMesage("" + client.Player.Name + " has won " + Title + " , he received 5k Cps!", MsgServer.MsgMessage.ChatMode.TopLeftSystem, MsgServer.MsgMessage.MsgColor.white);
                client.Player.ConquerPoints += 5000;
                AddTop(client);
                client.Player.HitPoints = (int)client.Status.MaxHitpoints;
                client.Teleport(428, 378, 1002);
            }
        }


        public void AddTop(Client.GameClient client)
        {
            if (WinnerUID == client.Player.UID)
                client.EffectStatus.Add("fivenout", DateTime.Now.AddHours(12));
            foreach (var user in Database.Server.GamePoll.Values)
            {
                if (user.Player.UID != WinnerUID)
                    client.EffectStatus.Remove("fivenout");
            }
        }

        public static void ExecuteAttack(GameClient attacked, GameClient attacker, ref uint Damage)
        {
            if (attacked.Player.Map == Map)
            {
                if (attacked.Get5Out_points > 0)
                {
                    attacked.Get5Out_points--;
                    if (attacked.Get5Out_points == 0)
                        attacked.SendSysMesage("You`ve just lost your final point, next hit you`re out.");
                    else
                        attacked.SendSysMesage("You`ve just lost 1 point. Current points left " + attacked.Get5Out_points + "");
                }
                else
                {
                    attacked.SendSysMesage("You lost all your points and got disqualifed", (Game.MsgServer.MsgMessage.ChatMode)2000);
                    attacked.Teleport(428, 378, 1002);
                }
            }
        }
    }
}
