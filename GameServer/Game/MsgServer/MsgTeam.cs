namespace COServer.Game.MsgServer
{
    public unsafe static class MsgTeam
    {
        public enum TeamTypes : uint
        {
            Create = 0,
            JoinRequest = 1,
            ExitTeam = 2,
            AcceptInvitation = 3,
            InviteRequest = 4,
            AcceptJoinRequest = 5,
            Dismiss = 6,
            Kick = 7,
            ForbidJoining = 8,
            UnforbidJoining = 9,
            LootMoneyOff = 10,
            LootMoneyOn = 11,
            LootItemsOff = 12,
            LootItemsOn = 13,
            RejectInvitation = 14,
            Send = 15
        }

        public static unsafe void GetTeamPacket(this ServerSockets.Packet stream, out TeamTypes Typ, out uint UID)
        {
            Typ = (TeamTypes)stream.ReadUInt32();
            UID = stream.ReadUInt32();
        }
        public static unsafe ServerSockets.Packet TeamCreate(this ServerSockets.Packet stream, TeamTypes Typ, uint UID)
        {
            stream.InitWriter();
            stream.Write((uint)Typ);
            stream.Write(UID);

            stream.Finalize(GamePackets.Team);
            return stream;
        }

        [PacketAttribute(GamePackets.Team)]
        private static void Process(Client.GameClient user, ServerSockets.Packet stream)
        {
            if (!user.Player.OnMyOwnServer)
                return;
            TeamTypes Typ;
            uint UID;
            stream.GetTeamPacket(out Typ, out UID);


            if (user.Player.Map == 700)
            {

                return;
            }

            switch (Typ)
            {
                case TeamTypes.Create:
                    {
                        user.Team = new Role.Instance.Team(user);
                        Typ = TeamTypes.Send;
                        user.Send(stream.TeamCreate(Typ, UID));
                        Typ = TeamTypes.Create;
                        user.Send(stream.TeamCreate(Typ, UID));
                        break;
                    }
                case TeamTypes.Dismiss:
                    {
                        if (user.Team != null)
                        {
                            user.Team.Remove(user, false);
                        }
                        else
                        {
                            user.SendSysMesage("Sorry, you're not in team!");
                        }
                        break;
                    }
                case TeamTypes.Kick:
                    {
                        if (user.Team == null)
                        {
                            user.SendSysMesage("Sorry, you're not in team!");
                            break;
                        }
                        Client.GameClient Kicker;
                        if (user.Team.TryGetMember(UID, out Kicker))
                            user.Team.Remove(Kicker, false);
                        break;
                    }
                case TeamTypes.ExitTeam:
                    {
                        if (user.Team != null)
                        {
                            user.Team.Remove(user, true);
                        }
                        break;
                    }
                case TeamTypes.InviteRequest:
                    {
                        if (user.Team != null)
                        {
                            if (user.Team.CkeckToAdd() && user.Team.TeamLider(user))
                            {
                                Role.IMapObj Invitee;
                                if (user.Player.View.TryGetValue(UID, out Invitee, Role.MapObjectType.Player))
                                {
                                    Role.Player Inv = Invitee as Role.Player;
                                    if (Inv.Owner.Team == null)
                                    {
                                        UID = user.Player.UID;
                                        Inv.Send(stream.TeamCreate(Typ, UID));
                                    }
                                    else
                                    {
                                        user.SendSysMesage(Inv.Name + " is already in a team.");
                                    }
                                }
                            }
                        }
                        break;
                    }
                case TeamTypes.AcceptJoinRequest:
                    {
                        if (user.Team != null && user.Player.Alive)
                        {
                            if (user.Team.CkeckToAdd() && user.Team.TeamLider(user) && !user.Team.ForbidJoin)
                            {
                                Role.IMapObj obj;
                                if (user.Player.View.TryGetValue(UID, out obj, Role.MapObjectType.Player))
                                {
                                    Client.GameClient NewTeammate = (obj as Role.Player).Owner;
                                    if (NewTeammate.Team != null)
                                    {
                                        NewTeammate.SendSysMesage("You're already in a team.");
                                        return;
                                    }
                                    NewTeammate.Team = user.Team;
                                    user.Team.Add(stream, NewTeammate);

                                }
                            }
                        }

                        break;
                    }
                case TeamTypes.AcceptInvitation:
                    {
                        if (user.Team == null && user.Player.Alive)
                        {
                            Role.IMapObj obj;
                            if (user.Player.View.TryGetValue(UID, out obj, Role.MapObjectType.Player))
                            {
                                Client.GameClient Leader = (obj as Role.Player).Owner;
                                if (Leader.Team != null)
                                {
                                    if (!Leader.Team.CkeckToAdd() || Leader.Team.ForbidJoin)
                                        return;

                                    user.Team = Leader.Team;
                                    Leader.Team.Add(stream, user);
                                }
                                else
                                {
                                    user.SendSysMesage(Leader.Player.Name + "'s doesn't have a team.");
                                }
                            }
                        }
                        break;
                    }
                case TeamTypes.JoinRequest:
                    {
                        if (user.Team == null && user.Player.Alive)
                        {
                            Role.IMapObj obj;
                            if (user.Player.View.TryGetValue(UID, out obj, Role.MapObjectType.Player))
                            {
                                Client.GameClient Leader = (obj as Role.Player).Owner;
                                if (Leader.Team != null)
                                {
                                    if (Leader.Team.TeamLider(Leader) && Leader.Team.CkeckToAdd())
                                    {
                                        UID = user.Player.UID;
                                        Leader.Send(stream.TeamCreate(Typ, UID));
                                    }
                                    else
                                    {
                                        user.SendSysMesage(Leader.Player.Name + "'s team is already full.");
                                    }
                                }
                                else
                                {
                                    user.SendSysMesage(Leader.Player.Name + "'s doesn't have a team.");
                                }
                            }
                        }
                        break;
                    }
                case TeamTypes.ForbidJoining:
                    {
                        if (user.Team != null)
                        {
                            user.Team.SendTeam(stream.TeamCreate(Typ, UID), 0);
                            user.Team.ForbidJoin = true;
                        }
                        break;
                    }
                case TeamTypes.UnforbidJoining:
                    {
                        if (user.Team != null)
                        {
                            user.Team.SendTeam(stream.TeamCreate(Typ, UID), 0);
                            user.Team.ForbidJoin = false;
                        }
                        break;
                    }
                case TeamTypes.LootMoneyOn:
                    {
                        if (user.Team != null)
                        {
                            user.Team.SendTeam(stream.TeamCreate(Typ, UID), 0);
                            user.Team.PickupMoney = true;
                        }
                        break;
                    }
                case TeamTypes.LootMoneyOff:
                    {
                        if (user.Team != null)
                        {
                            user.Team.SendTeam(stream.TeamCreate(Typ, UID), 0);
                            user.Team.PickupMoney = false;
                        }
                        break;
                    }
                case TeamTypes.LootItemsOn:
                    {
                        if (user.Team != null)
                        {
                            user.Team.SendTeam(stream.TeamCreate(Typ, UID), 0);
                            user.Team.PickupItems = true;
                        }
                        break;
                    }
                case TeamTypes.LootItemsOff:
                    {
                        if (user.Team != null)
                        {
                            user.Team.SendTeam(stream.TeamCreate(Typ, UID), 0);
                            user.Team.PickupItems = false;
                        }
                        break;
                    }
            }
        }
    }

}
