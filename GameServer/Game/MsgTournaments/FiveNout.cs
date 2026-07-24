using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using COServer.Game.MsgServer;
using static COServer.Game.MsgServer.MsgStringPacket;

namespace COServer.Game.MsgTournaments
{
    public class Fivenout
    {
        public ProcesType Process { get; set; }
        public DateTime StartTimer = new DateTime();
        public DateTime InfoTimer = new DateTime();
        public Role.GameMap Map;
        public uint DinamicMap = 0;
        public KillerSystem KillSystem;

        public Fivenout()
        {
            Process = ProcesType.Dead;
            InfoTimer = DateTime.Now; // Inicializa InfoTimer
        }

        public void Open()
        {
            if (Process == ProcesType.Dead)
            {
                KillSystem = new KillerSystem();
                StartTimer = DateTime.Now;



                if (Map == null)
                {
                    Map = Database.Server.ServerMaps[700];
                    DinamicMap = Map.GenerateDynamicID();

                }
                InfoTimer = DateTime.Now;
                Process = ProcesType.Idle;

            }
        }

        public bool Join(Client.GameClient user, ServerSockets.Packet stream)
        {
            if (Process == ProcesType.Idle)
            {
                ushort x = 0;
                ushort y = 0;
                Map.GetRandCoord(ref x, ref y);
                user.Teleport(x, y, Map.ID, DinamicMap);
                user.Player.FiveNOut = 10; // 10 vidas conforme NPC

                return true;
            }

            return false;
        }

        public void CheckUp()
        {


            // Só exibe vidas e contador se o mapa estiver inicializado (Idle ou Alive)
            if (Process != ProcesType.Dead && Map != null)
            {
                #region Lives and Timer Display
                if (DateTime.Now > InfoTimer.AddSeconds(1)) // Atualiza a cada segundo
                {

                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();

                        string headerText = Process == ProcesType.Idle ? "  [TRAINING]  " : "  [LIVE]  ";
                        var headerMsg = new MsgServer.MsgMessage(headerText, MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.FirstRightCorner);
                        SendMapPacket(headerMsg.GetArray(stream));

                        var separatorMsg = new MsgServer.MsgMessage("--------------------------------", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                        SendMapPacket(separatorMsg.GetArray(stream));

                        var playersTitleMsg = new MsgServer.MsgMessage("[PLAYERS]              [LIFE]", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                        SendMapPacket(playersTitleMsg.GetArray(stream));

                        var players = MapPlayers();

                        foreach (var user in players)
                        {
                            var lifeMsg = new MsgServer.MsgMessage($"{user.Player.Name}:              {user.Player.FiveNOut} Lifes", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                            SendMapPacket(lifeMsg.GetArray(stream));
                        }

                        var separatorMsg1 = new MsgServer.MsgMessage("--------------------------------", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
                        SendMapPacket(separatorMsg.GetArray(stream));

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
                    MsgSchedules.SendSysMesage("[ Five and Out ] has started TwinCity - Signup.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.white);
                    Process = ProcesType.Alive;
                    StartTimer = DateTime.Now;
 
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
                    foreach (var user in MapPlayers())
                    {
                        user.Teleport(428, 378, 1002);
                        MsgSchedules.SendSysMesage($"{user.Player.Name} He was teleported back to Twin City with {user.Player.FiveNOut} remaining lives.", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.white);
                    }
                    MsgSchedules.SendSysMesage("[ Five and Out ] has ended. Players have been teleported back to Twin City.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.white);
                    Process = ProcesType.Dead;

                }

                if (Process == ProcesType.Alive)
                {
                    if (DateTime.Now > StartTimer.AddMinutes(15))
                    {
                        foreach (var user in MapPlayers())
                        {
                            user.Teleport(428, 378, 1002);
                            MsgSchedules.SendSysMesage($"{user.Player.Name} He was teleported back to Twin City with {user.Player.FiveNOut} remaining lives.",
                                MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.white);
                        }
                        MsgSchedules.SendSysMesage("[ Five and Out ] has ended. Players have been teleported back to Twin City.",
                            MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.white);
                        Process = ProcesType.Dead;
                    }

                    if (MapPlayers().Length == 1)
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();

                            var winner = MapPlayers().First();

                            // Quantidade de silver como recompensa
                            uint silverAmount = 150000; //  silver
                            winner.Player.Money += silverAmount;

                            MsgSchedules.SendSysMesage($"{winner.Player.Name} received a prize for winning FiveNOut, {silverAmount} silver!",
                                MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.yellow);

                            Program.DiscordAPIwinners.Enqueue($"``[{winner.Player.Name}] received a prize for winning FiveNOut, {silverAmount} silver!``");

                            var mymsg = $"[EVENT] {winner.Player.Name} received {silverAmount} silver from the Five and Out Tournament!";
                            MsgSchedules.SendSysMesage(mymsg, MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.white);

                            winner.Teleport(428, 378, 1002, 0);
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
                                    if (user.Player.FiveNOut > 1)
                                    {
                                        user.Player.FiveNOut--;
                                        ushort x = 0;
                                        ushort y = 0;
                                        Map.GetRandCoord(ref x, ref y);
                                        user.Teleport(x, y, Map.ID, DinamicMap);
                                        user.Player.Revive(stream);
                                        MsgSchedules.SendSysMesage($"{user.Player.Name} One life was lost! Remaining lives: {user.Player.FiveNOut}",
                                            MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.yellow);
                                    }
                                    else
                                    {
                                        user.Player.FiveNOut = 0;
                                        user.Teleport(428, 378, 1002);
                                        var mymsg = "[EVENT]" + user.Player.Name + " has lost all lives in the Five and Out!";
                                        MsgSchedules.SendSysMesage(mymsg, MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.white);
                                    }
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
                }
            }
        }

        public Client.GameClient[] MapPlayers()
        {
            if (Map == null)
            {
                return new Client.GameClient[0]; // Retorna array vazio se Map não estiver inicializado
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