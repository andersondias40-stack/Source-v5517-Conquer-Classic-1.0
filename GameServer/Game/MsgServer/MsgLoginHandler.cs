using System;
using System.IO;
using System.Xml.Linq;

namespace COServer.Game.MsgServer
{
    public static unsafe partial class MsgBuilder
    {
        public static unsafe ServerSockets.Packet LoginHandlerCreate(this ServerSockets.Packet stream, uint Type, uint Map)
        {
            stream.InitWriter();

            stream.Write(0);
            stream.Write(Type);
            stream.Write(Map);
            stream.Finalize(GamePackets.MapLoading);
            return stream;
        }
    }

    public unsafe struct MsgLoginHandler
    {
        
        [PacketAttribute(GamePackets.MapLoading)]
        public unsafe static void LoadMap(Client.GameClient client, ServerSockets.Packet packet)
        {
            if ((client.ClientFlag & Client.ServerFlag.AcceptLogin) == Client.ServerFlag.AcceptLogin)
            {
                try
                {
                    client.Player.ServerID = (ushort)Database.GroupServerList.MyServerInfo.ID;
                    client.Send(packet.HeroInfo(client.Player));

                    if (Role.Core.IsGirl(client.Player.Body))
                        client.Send(packet.FlowerIconCreate(MsgFlower.FlowerAction.FlowerIcon, client.Player.Flowers));
                    else if (client.Player.Flowers.FreeFlowers > 0)
                        client.Send(packet.FlowerIconCreate(MsgFlower.FlowerAction.FlowerIcon, client.Player.Flowers));
                    client.Send(packet.NobilityIconCreate(client.Player.Nobility));

                    if (client.Player.BlessTime > 0)
                        client.Player.SendUpdate(packet, client.Player.BlessTime, MsgUpdate.DataType.LuckyTimeTimer);

                    client.Player.ProtectAttack(1000 * 10);//10 Seconds
                    client.Player.CreateHeavenBlessPacket(packet, true);

                    client.Player.Stamina = 100;
                    client.Player.SendUpdate(packet, client.Player.Stamina, Game.MsgServer.MsgUpdate.DataType.Stamina);


                    //if (MsgTournaments.MsgSchedules.CurrentTournament.Type == MsgTournaments.TournamentType.QuizShow
                    //    && MsgTournaments.MsgSchedules.CurrentTournament.Process == MsgTournaments.ProcesType.Alive)
                    //    MsgTournaments.MsgSchedules.CurrentTournament.Join(client, packet);


                    if (client.Player.DExpTime > 0)
                        client.Player.CreateExtraExpPacket(packet);

                    client.Equipment.Show(packet);
                    client.Inventory.ShowALL(packet);
                    //send confiscator items
                    foreach (var item in client.Confiscator.RedeemContainer.Values)
                    {
                        var Dataitem = item;
                        Dataitem.DaysLeft = (uint)(TimeSpan.FromTicks(DateTime.Now.Ticks).Days - TimeSpan.FromTicks(Role.Instance.Confiscator.GetTimer(item.Date).Ticks).Days);
                        if (Dataitem.DaysLeft > 7)
                        {
                            Dataitem.Action = MsgDetainedItem.ContainerType.RewardCps;
                        }
                        if (Dataitem.Action != MsgDetainedItem.ContainerType.RewardCps)
                        {
                            Dataitem.Action = MsgDetainedItem.ContainerType.DetainPage;
                            Dataitem.Send(client, packet);
                        }
                        if (Dataitem.Action == MsgDetainedItem.ContainerType.RewardCps)
                            client.Confiscator.RedeemContainer.TryRemove(item.UID, out Dataitem);
                    }
                    foreach (var item in client.Confiscator.ClaimContainer.Values)
                    {
                        var Dataitem = item;
                        Dataitem.DaysLeft = (uint)(TimeSpan.FromTicks(DateTime.Now.Ticks).Days - TimeSpan.FromTicks(Role.Instance.Confiscator.GetTimer(item.Date).Ticks).Days);
                        if (Dataitem.RewardConquerPoints != 0)
                        {
                            Dataitem.Action = MsgDetainedItem.ContainerType.RewardCps;
                        }
                        Dataitem.Send(client, packet);
                        client.Confiscator.ClaimContainer[item.UID] = Dataitem;
                    }
                    //-------------

                    if (MsgTournaments.MsgSchedules.GuildWar.RewardDeputiLeader.Contains(client.Player.UID))
                        client.Player.AddFlag(MsgUpdate.Flags.TopDeputyLeader, Role.StatusFlagsBigVector32.PermanentFlag, false);
                    if (MsgTournaments.MsgSchedules.GuildWar.RewardLeader.Contains(client.Player.UID))
                        client.Player.AddFlag(MsgUpdate.Flags.TopGuildLeader, Role.StatusFlagsBigVector32.PermanentFlag, false);
                    client.Player.PKPoints = client.Player.PKPoints;
                    if (client.Player.CursedTimer > 0)
                    {
                        client.Player.AddCursed(client.Player.CursedTimer);
                    }

                    client.Send(packet.ServerTimerCreate());


                    MsgTournaments.MsgSchedules.ClassPkWar.LoginClient(client);

                    if (MsgTournaments.MsgSchedules.CouplesPKWar.Winner1 == client.Player.Name ||
                        MsgTournaments.MsgSchedules.CouplesPKWar.Winner2 == client.Player.Name)
                        client.Player.AddFlag(MsgUpdate.Flags.TopSpouse, Role.StatusFlagsBigVector32.PermanentFlag, false);

                    if (MsgTournaments.MsgBroadcast.CurrentBroadcast.EntityID != 1)
                    {
                        client.Send(new MsgServer.MsgMessage(MsgTournaments.MsgBroadcast.CurrentBroadcast.Message
                            , "ALLUSERS"
                            , MsgTournaments.MsgBroadcast.CurrentBroadcast.EntityName
                            , MsgServer.MsgMessage.MsgColor.white
                            , MsgServer.MsgMessage.ChatMode.BroadcastMessage
                            ).GetArray(packet));
                    }
                    if (client.Player.VipLevel >= 5)
                    {
                        client.Player.UpdateVip(packet);
                        client.Player.SendUpdate(packet, client.Player.VipLevel, MsgUpdate.DataType.VIPLevel);
                    }
                    //update merchant
                    client.Player.SendUpdate(packet, 255, MsgUpdate.DataType.Merchant);

                    ActionQuery action = new ActionQuery()
                    {
                        ObjId = client.Player.UID,
                        Type = (ActionType)157,
                        dwParam = 2
                    };

                    client.Send(packet.ActionCreate(&action));


                    MsgTournaments.MsgSchedules.PkWar.AddTop(client);
                 //Welcome Messages.
                    client.SendSysMesage("Welcome to Conquer Classic 1.0", MsgMessage.ChatMode.Talk);

                    if (client.Player.VipLevel >= 6)
                    {
                        TimeSpan timer1 = new TimeSpan(client.Player.ExpireVip.Ticks);
                        TimeSpan Now2 = new TimeSpan(DateTime.Now.Ticks);
                        int days_left = (int)(timer1.TotalDays - Now2.TotalDays);
                        int hour_left = (int)(timer1.TotalHours - Now2.TotalHours);
                        int left_minutes = (int)(timer1.TotalMinutes - Now2.TotalMinutes);
                        if (days_left > 0)
                        {
                            client.SendSysMesage("Your VIP " + client.Player.VipLevel + " will expire in: " + days_left + " days.", MsgMessage.ChatMode.System);
                        }
                        else if (hour_left > 0)
                        {
                            client.SendSysMesage("Your VIP " + client.Player.VipLevel + " will expire in: " + hour_left + " hours.", MsgMessage.ChatMode.System);
                        }
                        else if (left_minutes > 0)
                        {
                            client.SendSysMesage("Your VIP " + client.Player.VipLevel + " will expire in: " + left_minutes + " minutes.", MsgMessage.ChatMode.System);
                        }

                        // Mensagem em amarelo com a quantidade de dias VIP restantes
                        Game.MsgServer.MsgMessage msg = new MsgServer.MsgMessage(
                            "VIP Expire: " + days_left + " Days.",
                            MsgServer.MsgMessage.MsgColor.white,
                            MsgServer.MsgMessage.ChatMode.ContinueRightCorner
                        );


                        client.Send(msg.GetArray(packet));
                    }

                        if (Database.AtributesStatus.IsTrojan(client.Player.Class)
                        || Database.AtributesStatus.IsTrojan(client.Player.FirstClass)
                        || Database.AtributesStatus.IsTrojan(client.Player.SecondClass))
                    {
                        if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Cyclone))
                            client.MySpells.Add(packet, (ushort)Role.Flags.SpellID.Cyclone);
                    }


                    if (client.Inventory.HaveSpace(1))
                    {
                        foreach (var item in client.Equipment.ClientItems.Values)
                        {
                            if (item.Position >= (uint)Role.Flags.ConquerItem.Head && item.Position <= (uint)Role.Flags.ConquerItem.Tower)
                            {
                                if (client.Inventory.HaveSpace(1) && item.Position == (uint)Role.Flags.ConquerItem.RightWeapon
                                    && item.Position == (uint)Role.Flags.ConquerItem.LeftWeapon)
                                {
                                    if (!Database.ItemType.IsShield(item.ITEM_ID))
                                    {
                                        if (!Database.ItemType.Equipable(item.ITEM_ID, client))
                                        {
                                            client.Equipment.Remove((Role.Flags.ConquerItem)item.Position, packet);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (!client.ProjectManager)
                    {
                        Program.SendGlobalPackets.Enqueue(
                     new Game.MsgServer.MsgMessage(client.Player.Name + " just loggin to server", "ALLUSERS", "SYSTEM", Game.MsgServer.MsgMessage.MsgColor.white, (Game.MsgServer.MsgMessage.ChatMode)2005).GetArray(packet));
                    }
                    if (client.Player.Associate.Associat.ContainsKey(Role.Instance.Associate.Friends))
                    {
                        foreach (var fr in client.Player.Associate.Associat[Role.Instance.Associate.Friends].Values)
                        {
                            Client.GameClient gameClient;
                            if (Database.Server.GamePoll.TryGetValue(fr.UID, out gameClient))
                            {
                                gameClient.SendSysMesage("Your friend " + client.Player.Name + " has loggin to server", (Game.MsgServer.MsgMessage.ChatMode)2005);
                            }
                        }
                    }
                    //client.Warehouse.SendReturnedItems(packet);

                    client.ClientFlag &= ~Client.ServerFlag.AcceptLogin;
                    client.ClientFlag |= Client.ServerFlag.LoginFull;
                }  


                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

            }
        }

    }
}
