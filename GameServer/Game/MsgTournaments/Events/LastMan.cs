using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COServer.Game.MsgTournaments
{
    public class LastMan
    {
        public const uint Map = 1844;
        private ProcesType Mode;
        private DateTime FinishTimer = new DateTime();
        private string Title = "[ LastMan ]";
        public uint WinnerUID = 0;

        public LastMan()
        {
            Mode = ProcesType.Dead;
            if (!Program.OutMap.Contains(Map))
                Program.OutMap.Add(Map);
            if (!Program.FreePkMap.Contains(Map))
                Program.FreePkMap.Add(Map);
        }

        public void Open()
        {
            if (Mode == ProcesType.Dead)
            {
                FinishTimer = DateTime.Now.AddMinutes(1);
                Mode = ProcesType.Alive;
                MsgSchedules.SendSysMesage("" + Title + " has started in TwinCity", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.yellow);
            }
        }

        public void CheckUp()
        {
            if (Mode == ProcesType.Alive)
            {
                if (DateTime.Now > FinishTimer)
                {
                    Mode = ProcesType.Dead;
                }
            }
            if (DateTime.Now.Minute == 50 && DateTime.Now.Second < 2)
            {
                if (Mode == ProcesType.Dead)
                {
                    FinishTimer = DateTime.Now.AddMinutes(1);
                    Mode = ProcesType.Alive;
                    MsgSchedules.SendSysMesage("" + Title + " has started in TwinCity", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.red);
                }
            }
        }

        public bool AllowJoin(Client.GameClient user, ServerSockets.Packet stream)
        {
            if (Mode == ProcesType.Alive)
            {
                ushort x = 0;
                ushort y = 0;
                Database.Server.ServerMaps[Map].GetRandCoord(ref x, ref y);
                user.Teleport(x, y, Map);
                return true;
            }
            return false;
        }

        public bool IsFinished() { return Mode == ProcesType.Dead; }

        public bool TheLastPlayer()
        {
            return Database.Server.GamePoll.Values.Where(p => p.Player.Map == Map && p.Player.Alive).Count() == 1;
        }

        public void GiveReward(Client.GameClient client, ServerSockets.Packet stream)
        {
            WinnerUID = client.Player.UID;

            // Quantidade de silver que o jogador vai receber
            uint silverAmount = 50000; //  50 mil silver

            
            client.SendSysMesage($"You received {silverAmount} silver.", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);
            MsgSchedules.SendSysMesage($"{client.Player.Name} has won {Title} and received {silverAmount} silver!", MsgServer.MsgMessage.ChatMode.TopLeftSystem, MsgServer.MsgMessage.MsgColor.white);

           
            Program.DiscordAPIwinners.Enqueue($"``[{client.Player.Name}] Won {Title} and received {silverAmount} silver.``");

            
            client.Player.Money += silverAmount;

            
            client.Player.HitPoints = (int)client.Status.MaxHitpoints;
            client.Teleport(438, 387, 1002);
        }

        public void AddTop(Client.GameClient client)
        {
            if (WinnerUID == client.Player.UID)
                client.EffectStatus.Add("toplast", DateTime.Now.AddHours(12));

            foreach (var user in Database.Server.GamePoll.Values)
            {
                if (user.Player.UID != WinnerUID)
                    user.EffectStatus.Remove("toplast");
            }
        }
    }
}

