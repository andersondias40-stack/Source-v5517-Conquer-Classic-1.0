using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COServer.Game.MsgTournaments
{
    public class GenderWar
    {
        public const int Reward_Boy = 100000, Reward_Girl = 100000, FinishMinutes = 1;
        public const uint Map = 503;
        private ProcesType Mode;
        private DateTime FinishTimer = new DateTime();
        private string Title = "Top_Gender";
        public uint Winner_Boy_UID = 0, Winner_Girl_UID = 0;
        public GenderWar()
        {
            Mode = ProcesType.Dead;
            if (!Program.FreePkMap.Contains(Map))
                Program.FreePkMap.Add(Map);
            if (!Program.OutMap.Contains(Map))
                Program.OutMap.Add(Map);
        }
        public void Open()
        {
            if (Mode == ProcesType.Dead)
            {
                FinishTimer = DateTime.Now.AddMinutes(FinishMinutes);
                Mode = ProcesType.Alive;
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
            if (DateTime.Now.Hour == 11 && DateTime.Now.Minute == 00 && DateTime.Now.Second < 2)
            {
                if (Mode == ProcesType.Dead)
                {
                    FinishTimer = DateTime.Now.AddMinutes(FinishMinutes);
                    Mode = ProcesType.Alive;
                }
            }
        }

        public bool AllowJoin(Client.GameClient user, ServerSockets.Packet stream)
        {
            if (Mode == ProcesType.Alive)
            {
                ushort x = 0;
                ushort y = 0;
                if (user.Player.IsBoy())
                {
                    Database.Server.ServerMaps[Map].GetRandCoord(ref x, ref y);
                    user.Teleport(x, y, Map);
                    return true;
                }
                if (user.Player.IsGirl())
                {
                    Database.Server.ServerMaps[Map].GetRandCoord(ref x, ref y);
                    user.Teleport(x, y, Map, 1);
                    return true;
                }
            }
            return false;
        }
        public bool IsFinished() { return Mode == ProcesType.Dead; }
        public bool TheLastBoy()
        {
            return Database.Server.GamePoll.Values.Where(p => p.Player.Map == Map && p.Player.Alive).Count() == 1;
        }
        public bool TheLastGirl()
        {
            return Database.Server.GamePoll.Values.Where(p => p.Player.Map == Map && p.Player.DynamicID == 1 && p.Player.Alive).Count() == 1;
        }
        public void GiveReward(Client.GameClient client, ServerSockets.Packet stream)
        {
            #region Boy
            if (MsgSchedules._GenderWar.TheLastBoy())
            {
                Winner_Boy_UID = client.Player.UID;
                client.SendSysMesage("You received " + Reward_Boy.ToString() + " ConquerPoints.", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);
                MsgSchedules.SendSysMesage("" + client.Player.Name + " has won " + Title + " , he received " + Reward_Boy.ToString() + " ConquerPoints!", MsgServer.MsgMessage.ChatMode.TopLeftSystem, MsgServer.MsgMessage.MsgColor.white);
                client.Player.Money += Reward_Boy;
                AddTop(client);
                client.Player.HitPoints = (int)client.Status.MaxHitpoints;
                client.Teleport(428, 378, 1002);
            }
            #endregion
            #region Girl
            else if (MsgSchedules._GenderWar.TheLastGirl())
            {
                Winner_Girl_UID = client.Player.UID;
                client.SendSysMesage("You received " + Reward_Girl.ToString() + " ConquerPoints.", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);
                MsgSchedules.SendSysMesage("" + client.Player.Name + " has won " + Title + " , he received " + Reward_Girl.ToString() + " ConquerPoints!", MsgServer.MsgMessage.ChatMode.TopLeftSystem, MsgServer.MsgMessage.MsgColor.white);
                client.Player.Money += Reward_Girl;
                AddTop(client);
                client.Player.HitPoints = (int)client.Status.MaxHitpoints;
                client.Teleport(428, 378, 1002);
            }
            #endregion
            #region Still Player Alive
            else
            {
                client.CreateDialog(stream, "Sorry, other players are still alive.", "Ah ok.");
            }
            #endregion
        }
        public void AddTop(Client.GameClient client)
        {
            if (Winner_Boy_UID == client.Player.UID || Winner_Girl_UID == client.Player.UID)
                client.EffectStatus.Add("topgender", DateTime.Now.AddHours(12));
            foreach (var user in Database.Server.GamePoll.Values)
            {
                if (user.Player.UID != Winner_Boy_UID && user.Player.UID != Winner_Girl_UID)
                    client.EffectStatus.Remove("topgender");
            }
        }
    }
}
