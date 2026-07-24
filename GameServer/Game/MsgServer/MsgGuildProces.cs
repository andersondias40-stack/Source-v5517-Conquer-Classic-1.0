using System.Collections.Generic;
using System.Text;

namespace COServer.Game.MsgServer
{
    public unsafe static class MsgGuildProces
    {
        public enum GuildAction : uint
        {
            JoinRequest = 1,
            AcceptRequest = 2,
            Quit = 3,
            InfoName = 6,
            Allied = 7,
            RemoveAlly = 8,
            Enemy = 9,
            RemoveEnemy = 10,
            SilverDonate = 11,
            Show = 12,
            Disband = 19,
            CpDonate = 20,
            RequestAllied = 16,
            Requirements = 24,
            Bulletin = 27,
            Promote = 28,
            ConfirmPromote = 29,
            Discharge = 30,
            Resign = 32,
            RequestPromote = 37,
            UpdatePromote = 38
        }

        public static void GuildRequest(this ServerSockets.Packet stream, out GuildAction requesttype, out uint UID, out int[] args, out string[] strlist)
        {
            requesttype = (GuildAction)stream.ReadInt32();
            UID = stream.ReadUInt32();
            args = new int[3];
            for (int i = 0; i < 3; i++)
            {
                args[i] = stream.ReadInt32();
            }
            strlist = stream.ReadStringList();
            var test = new int[2];
            stream.ReadBytes(3); //unknown
        }

        public static unsafe ServerSockets.Packet GuildRequestCreate(this ServerSockets.Packet stream, GuildAction requesttype, uint UID, int[] args, params string[] strlist)
        {
            stream.InitWriter();

            stream.Write((uint)requesttype);
            stream.Write(UID);
            stream.Write(args[0]);
            stream.Write(args[1]);
            stream.Write(args[2]);
            stream.Write(strlist);
            //stream.ZeroFill(3);

            stream.Finalize(GamePackets.ProcesGuild);
            return stream;
        }

        [PacketAttribute(GamePackets.ProcesGuild)]
        private static void Process(Client.GameClient user, ServerSockets.Packet stream)
        {
            if (!user.Player.OnMyOwnServer)
                return;
            GuildAction Action;
            uint UID;
            int[] args;
            string[] strlist;
            stream.GuildRequest(out Action, out UID, out args, out strlist);

            switch (Action)
            {
                case GuildAction.Resign:
                    {
                        if (user.Player.MyGuild == null) break;
                        if (user.Player.MyGuildMember == null) break;

                        user.Player.MyGuild.Promote((uint)Role.Flags.GuildMemberRank.Member, user.Player, user.Player.Name, stream);
                        break;
                    }
                case GuildAction.InfoName:
                    {
                        Role.Instance.Guild Guild;
                        if (Role.Instance.Guild.GuildPoll.TryGetValue(UID, out Guild))
                        {

                            user.Player.SendString(stream, Game.MsgServer.MsgStringPacket.StringID.GuildName, Guild.Info.GuildID, true
                            , Guild.GuildName);// new string[1] { Guild.GuildName });
                        }
                        break;
                    }
                case GuildAction.AcceptRequest:
                    {
                        if (user.Player.MyGuild == null) break;
                        if (user.Player.MyGuildMember == null) break;
                        Role.IMapObj obj;
                        if (user.Player.View.TryGetValue(UID, out obj, Role.MapObjectType.Player))
                        {
                            Client.GameClient Target = null;
                            Target = (obj as Role.Player).Owner;
                            if (Target == null) break;
                            if (Target.Player.MyGuild != null || Target.Player.MyGuildMember != null)
                                break;

                            if (Target.Player.TargetGuild == user.Player.UID)
                            {
                                Target.Player.TargetGuild = 0;
                                user.Player.MyGuild.SendMessajGuild(" " + user.Player.MyGuildMember.Rank.ToString() + " " + user.Player.Name + " of " + user.Player.MyGuild.GuildName + " has recruited " + Target.Player.Name + " to the Guild.");
                                user.Player.MyGuild.AddPlayer(Target.Player, stream);
                            }
                            else
                            {
                                Target.AcceptedGuildID = user.Player.GuildID;
                                Target.Send(stream.GuildRequestCreate(GuildAction.AcceptRequest, user.Player.UID, args, strlist));


                            }
                        }
                        break;
                    }
                case GuildAction.JoinRequest:
                    {
                        if (user.Player.MyGuild != null) break;
                        Role.IMapObj obj;
                        if (user.Player.View.TryGetValue(UID, out obj, Role.MapObjectType.Player))
                        {
                            Client.GameClient Target = null;
                            Target = (obj as Role.Player).Owner;
                            if (Target == null) break;
                            if (Target.Player.MyGuild == null || Target.Player.MyGuildMember == null)
                                break;
                            if (Target.Player.MyGuildMember.Accepter)
                            {
                                if (user.AcceptedGuildID == Target.Player.GuildID)
                                {
                                    user.AcceptedGuildID = 0;
                                    Target.Player.MyGuild.SendMessajGuild(" " + Target.Player.MyGuildMember.Rank.ToString() + " " + Target.Player.Name + " of " + Target.Player.MyGuild.GuildName + " has recruited " + user.Player.Name + " to the Guild.");
                                    Target.Player.MyGuild.AddPlayer(user.Player, stream);
                                }
                                else
                                {
                                    user.Player.TargetGuild = Target.Player.UID;
                                    Target.Send(stream.GuildRequestCreate(GuildAction.JoinRequest, user.Player.UID, args, strlist));
                                }
                            }
                        }
                        break;
                    }
                case GuildAction.SilverDonate:
                    {
                        if (UID >= 10000)
                        {
                            if (user.InTrade)
                                return;
                            if (user.Player.Money < UID)
                                break;
                            if (user.Player.MyGuild != null && user.Player.MyGuildMember != null)
                            {
                                user.Player.Money -= (uint)UID;
                                user.Player.SendUpdate(stream, user.Player.Money, MsgUpdate.DataType.Money);
                                user.Player.MyGuildMember.MoneyDonate += UID;
                                user.Player.MyGuild.Info.Donation += UID / 2;
                                user.Player.MyGuild.Info.SilverFund += UID;
                                user.Player.MyGuild.SendThat(user.Player);
                                user.Player.MyGuild.SendMessajGuild($"{user.Player.Name} has donated {UID} to {user.Player.MyGuild.GuildName}.", MsgMessage.ChatMode.Guild);
                            }
                        }
                        break;
                    }
                case GuildAction.Show:
                    {
                        if (user.Player.MyGuild != null)
                        {
                            user.Player.MyGuild.SendThat(user.Player);
                        }
                        break;
                    }
                case GuildAction.Bulletin:
                    {
                        if (user.Player.MyGuild != null)
                        {
                            if (user.Player.Name != user.Player.MyGuild.Info.LeaderName)
                                break;
                            if (strlist.Length > 0 && strlist[0] != null)
                            {
                                if (Program.NameStrCheck(strlist[0], false))
                                {
                                    user.Player.MyGuild.Bulletin = strlist[0];
                                    user.Player.MyGuild.SendThat(user.Player);

                                }
                                else
                                {
                                    user.SendSysMesage("Invalid characters in Bulletin.");
                                }
                            }
                        }
                        break;
                    }
                case GuildAction.Quit:
                    {
                        if (user.Player.MyGuild == null) break;
                        if (user.Player.MyGuildMember == null) break;
                        if (user.Player.MyGuildMember.Rank != Role.Flags.GuildMemberRank.GuildLeader)
                        {
                            if (user.Player.MyGuild.Info.Donation >= 20000)
                            {
                                user.Player.MyGuild.Quit(user.Player.Name, false, stream);
                            }
                            else
                            {
                                user.SendSysMesage("You need to donate 20,000 gold to leave your Guild.", MsgMessage.ChatMode.System);
                            }
                        }
                        break;
                    }
                case GuildAction.RemoveAlly:
                    {
                        if (user.Player.MyGuild == null) break;
                        if (user.Player.MyGuildMember == null) break;
                        if (user.Player.MyGuildMember.Rank != Role.Flags.GuildMemberRank.GuildLeader)
                            break;
                        if (strlist.Length > 0 && strlist[0] != null)
                        {
                            user.Player.MyGuild.RemoveAlly(strlist[0], stream);
                        }
                        break;
                    }
                case GuildAction.RequestAllied:
                    {
                        if (user.Player.MyGuild == null) break;
                        if (user.Player.MyGuildMember == null) break;
                        if (user.Player.MyGuildMember.Rank != Role.Flags.GuildMemberRank.GuildLeader)
                            break;
                        if (strlist.Length > 0 && strlist[0] != null)
                        {
                            string name = strlist[0];
                            if (name == user.Player.MyGuild.GuildName)
                                break;
                            if (!user.Player.MyGuild.IsEnemy(name))
                                user.Player.MyGuild.AddAlly(stream, name);
                            else
                            {
                                user.SendSysMesage("Sorry, this Guild is in your Enemies list.");
                            }
                        }

                        break;
                    }
                case GuildAction.Allied:
                    {
                        if (user.Player.MyGuild == null) break;
                        if (user.Player.MyGuildMember == null) break;
                        if (user.Player.MyGuildMember.Rank != Role.Flags.GuildMemberRank.GuildLeader)
                            break;
                       
                        if (strlist.Length > 0 && strlist[0] != null)
                        {
                            string name = strlist[0];
                            if (name == user.Player.MyGuild.GuildName)
                                break;
                            if (!user.Player.MyGuild.IsEnemy(name))
                            {
                                var leader = Role.Instance.Guild.GetLeaderGuild(name);
                                if (leader != null && leader.IsOnline)
                                {
                                    Client.GameClient LeaderClient;
                                    if (Database.Server.GamePoll.TryGetValue(leader.UID, out LeaderClient))
                                    {
                                        if (user.Team != null)
                                        {
                                            if (!user.Team.IsTeamMember(leader.UID))
                                            {
                                                user.SendSysMesage($"Sorry, you must be on same team with {LeaderClient.Player.Name}.");
                                                return;
                                            }
                                        }
                                        LeaderClient.Send(stream.GuildRequestCreate(GuildAction.RequestAllied, 0, new int[1], user.Player.MyGuild.GuildName));//3
                                        //user.Player.MyGuild.AddAlly(stream, name);
                                    }
                                }
                            }
                            else
                            {
                                user.SendSysMesage("Sorry, this Guild is in your Enemies list.");

                            }
                        }
                        break;
                    }
                case GuildAction.Enemy:
                    {
                        if (user.Player.MyGuild == null) break;
                        if (user.Player.MyGuildMember == null) break;
                        if (user.Player.MyGuildMember.Rank != Role.Flags.GuildMemberRank.GuildLeader)
                            break;
                        if (strlist.Length > 0 && strlist[0] != null)
                        {
                            string name = strlist[0];
                            if (name == user.Player.MyGuild.GuildName)
                                break;
                            if (user.Player.MyGuild.AllowAddAlly(name))
                            {
                                user.Player.MyGuild.AddEnemy(stream, name);
                                user.Player.MyGuild.SendMessajGuild("Guild Leader " + user.Player.Name + " has added Guild " + name + " to the Enemies list!");
                            }
                            else
                            {
                                user.SendSysMesage("Sorry, this Guild is in the Allies list.");
                            }
                        }
                        break;
                    }
                case GuildAction.RemoveEnemy:
                    {
                        if (user.Player.MyGuild == null) break;
                        if (user.Player.MyGuildMember == null) break;
                        if (user.Player.MyGuildMember.Rank != Role.Flags.GuildMemberRank.GuildLeader)
                            break;
                        if (strlist.Length > 0 && strlist[0] != null)
                        {
                            user.Player.MyGuild.RemoveEnemy(strlist[0], stream);
                        }
                        break;
                    }
                case (GuildAction)33:
                case GuildAction.Discharge:
                    {
                        if (user.Player.MyGuild == null) break;
                        if (user.Player.MyGuildMember == null) break;
                        if (user.Player.MyGuildMember.Rank != Role.Flags.GuildMemberRank.GuildLeader)
                            break;
                        if (strlist.Length == 0)
                            break;
                        var player = user.Player.MyGuild.GetMember(strlist[0]);
                        if (player != null && player.Rank != Role.Flags.GuildMemberRank.Member)
                        {
                            user.Player.MyGuild.RanksCounts[(ushort)player.Rank]--;
                            user.Player.MyGuild.RanksCounts[(ushort)Role.Flags.GuildMemberRank.Member]++;
                            user.Player.MyGuild.Members[player.UID].Rank = Role.Flags.GuildMemberRank.Member;

                            foreach (var userc in Database.Server.GamePoll.Values)
                            {
                                if (userc.Player.Name.ToLower() == player.Name.ToLower())
                                {
                                    userc.Player.MyGuild.Info.MyRank = (uint)Role.Flags.GuildMemberRank.Member;
                                    userc.Player.GuildRank = Role.Flags.GuildMemberRank.Member;
                                    userc.Player.View.SendView(userc.Player.GetArray(stream, false), false);
                                    userc.Player.MyGuild.SendThat(userc.Player);
                                    break;
                                }
                            }

                        }
                        user.Player.MyGuild.SendMessajGuild("" + user.Player.Name + " has discharged " + strlist[0] + " to a Member.");

                        //user.Player.MyGuild.Dismis(user, stream);
                        break;
                    }

            }
        }
        private static string CreatePromotionString(StringBuilder builder, Role.Flags.GuildMemberRank rank, int occupants,
           int maxOccupants, int extraBattlePower, int conquerPoints)
        {
            builder.Remove(0, builder.Length);
            builder.Append((int)rank);
            builder.Append(" ");
            builder.Append(occupants);
            builder.Append(" ");
            builder.Append(maxOccupants);
            builder.Append(" ");
            builder.Append(extraBattlePower);
            builder.Append(" ");
            builder.Append(conquerPoints);
            builder.Append(" ");
            return builder.ToString();
        }

        public static uint RedRouses;
        public static uint Tulips;
        public static uint Lilies;
        public static uint Orchids;
        public static uint AllFlowersDonation;
    }
}
