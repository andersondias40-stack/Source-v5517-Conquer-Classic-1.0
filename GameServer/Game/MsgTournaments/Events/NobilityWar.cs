using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COServer.Game.MsgTournaments
{
    public class NobilityWar
    {
        public const int Reward_King = 5000, Reward_Prince = 3000, Reward_Duke = 2000, Reward_Earl = 1000,
                            FinishMinutes = 1;
        public const uint King_Map = 130, Prince_Map = 131, Duke_Map = 132, Earl_Map = 133;
        private ProcesType Mode;
        private DateTime FinishTimer = new DateTime();
        private string Title = "Nobility_War";
        public uint WinnerKing = 0, WinnerPrince = 0, WinnerDuke = 0, WinnerEarl = 0;
        public NobilityWar()
        {
            Mode = ProcesType.Dead;
            if (!Program.OutMap.Contains(King_Map))
                Program.OutMap.Add(King_Map);
            if (!Program.OutMap.Contains(Prince_Map))
                Program.OutMap.Add(Prince_Map);
            if (!Program.OutMap.Contains(Duke_Map))
                Program.OutMap.Add(Duke_Map);
            if (!Program.OutMap.Contains(Earl_Map))
                Program.OutMap.Add(Earl_Map);
        }
        public void Open()
        {
            if (Mode == ProcesType.Dead)
            {
                Mode = ProcesType.Alive;

                FinishTimer = DateTime.Now.AddMinutes(FinishMinutes);
                if (!Program.FreePkMap.Contains(King_Map))
                    Program.FreePkMap.Add(King_Map);
                if (!Program.FreePkMap.Contains(Prince_Map))
                    Program.FreePkMap.Add(Prince_Map);
                if (!Program.FreePkMap.Contains(Duke_Map))
                    Program.FreePkMap.Add(Duke_Map);
                if (!Program.FreePkMap.Contains(Earl_Map))
                    Program.FreePkMap.Add(Earl_Map);
            }
        }
        public bool AllowJoin(Client.GameClient user, ServerSockets.Packet stream)
        {
            if (Mode == ProcesType.Alive)
            {
                ushort x = 0;
                ushort y = 0;
                if (user.Player.NobilityRank == Role.Instance.Nobility.NobilityRank.King)
                {
                    Database.Server.ServerMaps[King_Map].GetRandCoord(ref x, ref y);
                    user.Teleport(x, y, King_Map);
                    user.Player.SetPkMode(Role.Flags.PKMode.PK);
                    return true;
                }
                else if (user.Player.NobilityRank == Role.Instance.Nobility.NobilityRank.Prince)
                {
                    Database.Server.ServerMaps[Prince_Map].GetRandCoord(ref x, ref y);
                    user.Teleport(x, y, Prince_Map);
                    user.Player.SetPkMode(Role.Flags.PKMode.PK);
                    return true;
                }
                else if (user.Player.NobilityRank == Role.Instance.Nobility.NobilityRank.Duke)
                {
                    Database.Server.ServerMaps[Duke_Map].GetRandCoord(ref x, ref y);
                    user.Teleport(x, y, Duke_Map);
                    user.Player.SetPkMode(Role.Flags.PKMode.PK);
                    return true;
                }
                else if (user.Player.NobilityRank == Role.Instance.Nobility.NobilityRank.Earl)
                {
                    Database.Server.ServerMaps[Earl_Map].GetRandCoord(ref x, ref y);
                    user.Teleport(x, y, Earl_Map);
                    user.Player.SetPkMode(Role.Flags.PKMode.PK);
                    return true;
                }
            }
            return false;
        }
        public bool NextMap(Client.GameClient user, ServerSockets.Packet stream)
        {
            if (Mode == ProcesType.Alive)
            {
                ushort x = 0;
                ushort y = 0;
                if (user.Player.Map == Prince_Map)
                {
                    if (MsgSchedules._NobilityWar.TheLastPrince())
                    {
                        Database.Server.ServerMaps[King_Map].GetRandCoord(ref x, ref y);
                        user.Teleport(x, y, King_Map);
                        user.Player.SetPkMode(Role.Flags.PKMode.PK);
                        return true;
                    }
                    else
                        user.CreateDialog(stream, "Sorry, other players are still alive.", "Ah ok.");
                }
                else if (user.Player.Map == Duke_Map)
                {
                    if (MsgSchedules._NobilityWar.TheLastPrince())
                    {
                        Database.Server.ServerMaps[Prince_Map].GetRandCoord(ref x, ref y);
                        user.Teleport(x, y, Prince_Map);
                        user.Player.SetPkMode(Role.Flags.PKMode.PK);
                        return true;
                    }
                    else
                        user.CreateDialog(stream, "Sorry, other players are still alive.", "Ah ok.");
                }
                else if (user.Player.Map == Earl_Map)
                {
                    if (MsgSchedules._NobilityWar.TheLastPrince())
                    {
                        Database.Server.ServerMaps[Duke_Map].GetRandCoord(ref x, ref y);
                        user.Teleport(x, y, Duke_Map);
                        user.Player.SetPkMode(Role.Flags.PKMode.PK);
                        return true;
                    }
                    else
                        user.CreateDialog(stream, "Sorry, other players are still alive.", "Ah ok.");
                }
            }
            return false;
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
        }
        public bool IsFinished() { return Mode == ProcesType.Dead; }
        public bool TheLastKing()
        {
            return Database.Server.GamePoll.Values.Where(p => p.Player.Map == King_Map && p.Player.Alive).Count() == 1;
        }
        public bool TheLastPrince()
        {
            return Database.Server.GamePoll.Values.Where(p => p.Player.Map == Prince_Map && p.Player.Alive).Count() == 1;
        }
        public bool TheLastDuke()
        {
            return Database.Server.GamePoll.Values.Where(p => p.Player.Map == Duke_Map && p.Player.Alive).Count() == 1;
        }
        public bool TheLastEarl()
        {
            return Database.Server.GamePoll.Values.Where(p => p.Player.Map == Earl_Map && p.Player.Alive).Count() == 1;
        }
        public void GiveReward(Client.GameClient client, ServerSockets.Packet stream)
        {
            #region King
            if (MsgSchedules._NobilityWar.TheLastKing())
            {
                WinnerKing = client.Player.UID;
                client.SendSysMesage("You received " + Reward_King.ToString() + " ConquerPoints.", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);
                MsgSchedules.SendSysMesage("" + client.Player.Name + " has won " + Title + " , he received " + Reward_King.ToString() + " ConquerPoints!", MsgServer.MsgMessage.ChatMode.TopLeftSystem, MsgServer.MsgMessage.MsgColor.white);
                client.Player.ConquerPoints += Reward_King;
                AddTop(client);
                client.Player.HitPoints = (int)client.Status.MaxHitpoints;
                client.Teleport(301, 278, 1002);
            }
            #endregion
            #region Prince
            else if (MsgSchedules._NobilityWar.TheLastPrince())
            {
                WinnerPrince = client.Player.UID;
                client.SendSysMesage("You received " + Reward_Prince.ToString() + " ConquerPoints.", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);
                MsgSchedules.SendSysMesage("" + client.Player.Name + " has won " + Title + " , he received " + Reward_Prince.ToString() + " ConquerPoints!", MsgServer.MsgMessage.ChatMode.TopLeftSystem, MsgServer.MsgMessage.MsgColor.white);
                client.Player.ConquerPoints += Reward_Prince;
                AddTop(client);
                client.Player.HitPoints = (int)client.Status.MaxHitpoints;
                client.Teleport(301, 278, 1002);
            }
            #endregion
            #region Duke
            else if(MsgSchedules._NobilityWar.TheLastDuke())
            {
                WinnerDuke = client.Player.UID;
                client.SendSysMesage("You received " + Reward_Duke.ToString() + " ConquerPoints.", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);
                MsgSchedules.SendSysMesage("" + client.Player.Name + " has won " + Title + " , he received " + Reward_Duke.ToString() + " ConquerPoints!", MsgServer.MsgMessage.ChatMode.TopLeftSystem, MsgServer.MsgMessage.MsgColor.white);
                client.Player.ConquerPoints += Reward_Duke;
                AddTop(client);
                client.Player.HitPoints = (int)client.Status.MaxHitpoints;
                client.Teleport(301, 278, 1002);
            }
            #endregion
            #region Earl
            else if (MsgSchedules._NobilityWar.TheLastEarl())
            {
                WinnerEarl = client.Player.UID;
                client.SendSysMesage("You received " + Reward_Earl.ToString() + " ConquerPoints.", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.red);
                MsgSchedules.SendSysMesage("" + client.Player.Name + " has won " + Title + " , he received " + Reward_Earl.ToString() + " ConquerPoints!", MsgServer.MsgMessage.ChatMode.TopLeftSystem, MsgServer.MsgMessage.MsgColor.white);
                client.Player.ConquerPoints += Reward_Earl;
                AddTop(client);
                client.Player.HitPoints = (int)client.Status.MaxHitpoints;
                client.Teleport(301, 278, 1002);
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
            if (WinnerKing == client.Player.UID)
                client.EffectStatus.Add("TopKing", DateTime.Now.AddHours(12));
            if (WinnerPrince == client.Player.UID)
                client.EffectStatus.Add("TopPrince", DateTime.Now.AddHours(12));
            if (WinnerDuke == client.Player.UID)
                client.EffectStatus.Add("TopDuke", DateTime.Now.AddHours(12));
            foreach (var user in Database.Server.GamePoll.Values)
            {
                if (user.Player.UID != WinnerKing)
                    client.EffectStatus.Remove("TopKing");
                if (user.Player.UID != WinnerPrince)
                    client.EffectStatus.Remove("TopPrince");
                if (user.Player.UID != WinnerDuke)
                    client.EffectStatus.Remove("TopDuke");
            }
        }
    }
}
