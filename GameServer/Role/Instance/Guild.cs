using COServer.Game.MsgServer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace COServer.Role.Instance
{
    public class Guild
    {
        public DateTime SummonGuild;
        public class UpdateDB
        {

            public uint UID;
            public uint GuildID;
            public Flags.GuildMemberRank Rank;
        }
        public class Member
        {
            public uint UID;
            public string Name;
            public Flags.GuildMemberRank Rank;
            public uint Level;
            public long MoneyDonate;//save
            public bool IsOnline = false;
            public bool Accepter
            {
                get { return (ushort)Rank > 89; }
            }

            public uint Mesh { get; internal set; }
        }
        public void CreateMembersRank()
        {
            lock (this)
            {
                //remove all ranks
                foreach (Member memb in Members.Values)
                {
                    if ((ushort)memb.Rank < 89)
                    {
                        if (RanksCounts[(ushort)memb.Rank] > 0)
                            RanksCounts[(ushort)memb.Rank]--;
                        memb.Rank = Role.Flags.GuildMemberRank.Member;
                        RanksCounts[(ushort)memb.Rank]++;
                    }
                    else // create old rank
                    {
                        //for deputi leaders and others.
                        RanksCounts[(ushort)memb.Rank]++;
                    }
                }
            }
        }
        public Member GetGuildLeader
        {

            get
            {
                return Members.Values.Where(p => p.Rank == Flags.GuildMemberRank.GuildLeader).FirstOrDefault();
            }
        }
        public uint AllFlowers { get { return (uint)(Lilies + Orchids + Tulips + Rouses); } }
        public static Counter Counter = new Counter(1000);
        public static ConcurrentDictionary<uint, Guild> GuildPoll = new ConcurrentDictionary<uint, Guild>();

        public Game.MsgServer.MsgGuildInformation Info;
        public Member[] GetOnlineMembers
        {
            get { return Members.Values.OrderByDescending(p => p.IsOnline).ToArray(); }
        }

        public int Lilies { get; private set; }
        public int Orchids { get; private set; }
        public int Tulips { get; private set; }
        public int Rouses { get; private set; }

        public ConcurrentDictionary<uint, Member> Members;
        public ConcurrentDictionary<uint, Guild> Ally;
        public ConcurrentDictionary<uint, Guild> Enemy;
        public string Bulletin = "None";

        public string GuildName = "";
        public byte[] RanksCounts = new byte[(byte)Flags.GuildMemberRank.GuildLeader + 1];//+1

        public bool CanSave = true;

        public Guild(Client.GameClient client, string Name, ServerSockets.Packet stream)
        {
            Info = Game.MsgServer.MsgGuildInformation.Create();
            Members = new ConcurrentDictionary<uint, Member>();
            Ally = new ConcurrentDictionary<uint, Guild>();
            Enemy = new ConcurrentDictionary<uint, Guild>();
            GuildName = Name;
            if (client != null)
            {
                DateTime Now = DateTime.Now;
                Info.LeaderName = client.Player.Name;
                Info.GuildID = Counter.Next;
                Info.SilverFund = 1000000;
                //Info.CreateTime = (uint)GetTime(Now.Year, Now.Month, Now.Day);
                // Info.Level = MyArsenal.GetGuildLevel;
                AddPlayer(client.Player, stream);
                GuildPoll.TryAdd(Info.GuildID, this);
            }
        }
        public int AppendMember(MsgStringPacket msg, int idx, int idxFirst, int nCount)
        {
            int nAmount = Members.Count;
            string Txt = "";
            for (int i = idxFirst; i < nAmount; i++)
            {
                foreach (Member memb in Members.Values)
                {
                    Txt = $"{memb.Name} {memb.Level} {memb.IsOnline.ToString()}";
                }
                msg.Strings[i + 1] = Txt;
                nCount++;
            }
            return nCount;
        }
        public void SendMemberList(Role.Player user, int page, ServerSockets.Packet stream)
        {
            if (page < 0) return;
            Game.MsgServer.MsgStringPacket packet = new Game.MsgServer.MsgStringPacket();
            packet.ID = MsgStringPacket.StringID.MemberList;
            int nCount = 0;
            AppendMember(packet, 0, page, nCount);
            var Guild = GuildPoll.Values.ToArray();
            if (nCount < 18)
            {
                int idx = Members.Count;
                page -= Members.Count;
                for (int i = 0; i < Members.Count; i++)
                {
                    var pSyn = Guild[i];
                    if (pSyn != null )
                    {
                        pSyn.AppendMember(packet, idx, page, nCount);
                        if (nCount < 18)
                            return;
                        idx += pSyn.Members.Count;
                        page -= pSyn.Members.Count;

                        for (int x = 0; x < Members.Count; x++)
                        {
                            var pSynTeam = Guild[x];
                            pSynTeam.AppendMember(packet, idx, page, nCount);
                            if (nCount < 18)
                                return;
                            idx += pSynTeam.Members.Count;
                            page -= pSynTeam.Members.Count;
                        }
                        if (nCount >= 18)
                            return;
                    }
                }
            }
            user.Send(stream.StringPacketCreate(packet));
            //packet.UID = UID;

            //packet.Strings = args;

            //if (SendScreen)
            //    View.SendView(stream.StringPacketCreate(packet), true);
            //else
            //    Owner.Send(stream.StringPacketCreate(packet));
        }
        public unsafe void AddPlayer(Role.Player player, ServerSockets.Packet stream)
        {
            if (player.MyGuild == null)
            {
                Member memb = new Member();

                memb.IsOnline = true;
                memb.Name = player.Name;
                if (Members.Count == 0)
                    memb.Rank = Flags.GuildMemberRank.GuildLeader;
                else
                    memb.Rank = Flags.GuildMemberRank.Member;

                memb.UID = player.UID;
                memb.Level = player.Level;
                if (Members.Count == 0)
                    Info.SilverFund = memb.MoneyDonate = 1000000;

                Members.TryAdd(memb.UID, memb);
                Info.MembersCount = (ushort)Members.Count;


                player.GuildID = Info.GuildID;
                player.GuildRank = memb.Rank;
                player.MyGuild = this;
                player.MyGuildMember = memb;

                Info.MyRank = (ushort)memb.Rank;
                SendThat(player);
                player.View.SendView(player.GetArray(stream, false), false);

                player.SendString(stream, Game.MsgServer.MsgStringPacket.StringID.GuildName, Info.GuildID, true, GuildName);// new string[1] { GuildName /*+ " " + Info.LeaderName + " " + Info.Level + " " + Info.MembersCount */});
                RanksCounts[(ushort)memb.Rank]++;

            }
        }
        public unsafe bool Promote(uint rank, Role.Player owner, string Name, ServerSockets.Packet stream)
        {
            Member membru = GetMember(Name);

            if (membru != null)
            {
                RanksCounts[(ushort)membru.Rank]--;
                switch (rank)
                {
                    case (ushort)Role.Flags.GuildMemberRank.GuildLeader:
                        {
                            membru.Rank = Role.Flags.GuildMemberRank.GuildLeader;
                            Info.LeaderName = membru.Name;
                            bool Online = false;
                            Client.GameClient client;
                            if (Database.Server.GamePoll.TryGetValue(membru.UID, out client))
                            {
                                Online = true;
                                client.Player.GuildRank = Role.Flags.GuildMemberRank.GuildLeader;
                                client.Player.View.SendView(client.Player.GetArray(stream, false), false);
                                Info.MyRank = (ushort)membru.Rank;
                                SendThat(client.Player);
                            }

                            owner.MyGuildMember.Rank = Role.Flags.GuildMemberRank.Member;
                            owner.GuildRank = Role.Flags.GuildMemberRank.Member;
                            owner.View.SendView(owner.GetArray(stream, false), false);
                            Info.MyRank = (ushort)membru.Rank;
                            SendThat(owner);
                            //   SendMessajGuild("" + membru.Name + " is now " + ((Role.Flags.GuildMemberRank)rank).ToString() + ".");
                            if (!Online)
                            {
                                Database.ServerDatabase.LoginQueue.TryEnqueue(membru);
                            }
                            break;
                        }
                    case (ushort)Role.Flags.GuildMemberRank.DeputyLeader:
                        {
                            bool online = false;
                            if (RanksCounts[(ushort)Role.Flags.GuildMemberRank.DeputyLeader] == 8) return false;
                            RanksCounts[rank]++;
                            membru.Rank = Role.Flags.GuildMemberRank.DeputyLeader;
                            Client.GameClient client;
                            if (Database.Server.GamePoll.TryGetValue(membru.UID, out client))
                            {
                                online = true;
                                client.Player.GuildRank = Role.Flags.GuildMemberRank.DeputyLeader;
                                client.Player.View.SendView(client.Player.GetArray(stream, false), false);
                                Info.MyRank = (ushort)membru.Rank;
                                SendThat(client.Player);
                            }
                            //    SendMessajGuild("" + membru.Name + " is now " + ((Role.Flags.GuildMemberRank)rank).ToString() + ".");
                            if (!online)
                            {
                                Database.ServerDatabase.LoginQueue.TryEnqueue(membru);
                            }
                            break;
                        }
                    default:
                        {
                            bool online = false;
                            RanksCounts[rank]++;
                            membru.Rank = (Role.Flags.GuildMemberRank)rank;
                            Client.GameClient client;
                            if (Database.Server.GamePoll.TryGetValue(membru.UID, out client))
                            {
                                online = true;
                                client.Player.GuildRank = membru.Rank;
                                client.Player.View.SendView(client.Player.GetArray(stream, false), false);
                                Info.MyRank = (ushort)membru.Rank;
                                SendThat(client.Player);
                            }
                            // SendMessajGuild("" + membru.Name + " is now Rank : [" + ((Role.Flags.GuildMemberRank)rank).ToString() + "].");
                            if (!online)
                            {
                                Database.ServerDatabase.LoginQueue.TryEnqueue(membru);
                            }
                            break;
                        }

                }
                SendMessajGuild("" + owner.Name + " has appointed " + Name + " as " + ((Role.Flags.GuildMemberRank)rank).ToString() + ".");
                return true;

            }
            return false;
        }

        public unsafe bool NewPromote(uint rank, Role.Player owner, string Name, ServerSockets.Packet stream)
        {
            SendMessajGuild("" + owner.Name + " has appointed " + Name + " as " + ((Role.Flags.GuildMemberRank)rank).ToString() + ".");
            Member membru = GetMember(Name);

            if (membru != null)
            {
                RanksCounts[(ushort)membru.Rank]--;
                switch (rank)
                {
                    case (ushort)Role.Flags.GuildMemberRank.GuildLeader:
                        {
                            membru.Rank = Role.Flags.GuildMemberRank.GuildLeader;
                            Info.LeaderName = membru.Name;
                            bool Online = false;
                            Client.GameClient client;
                            if (Database.Server.GamePoll.TryGetValue(membru.UID, out client))
                            {
                                Online = true;
                                client.Player.GuildRank = Role.Flags.GuildMemberRank.GuildLeader;
                                client.Player.View.SendView(client.Player.GetArray(stream, false), false);
                                Info.MyRank = (ushort)membru.Rank;
                                SendThat(client.Player);
                            }

                            owner.MyGuildMember.Rank = Role.Flags.GuildMemberRank.Member;
                            owner.GuildRank = Role.Flags.GuildMemberRank.Member;
                            owner.View.SendView(owner.GetArray(stream, false), false);
                            Info.MyRank = (ushort)membru.Rank;
                            SendThat(owner);
                            //   SendMessajGuild("" + membru.Name + " is now " + ((Role.Flags.GuildMemberRank)rank).ToString() + ".");
                            if (!Online)
                            {
                                Database.ServerDatabase.LoginQueue.TryEnqueue(membru);
                            }
                            break;
                        }
                    case (ushort)Role.Flags.GuildMemberRank.DeputyLeader:
                        {
                            bool online = false;
                            if (RanksCounts[(ushort)Role.Flags.GuildMemberRank.DeputyLeader] == 8) return false;
                            RanksCounts[rank]++;
                            membru.Rank = Role.Flags.GuildMemberRank.DeputyLeader;
                            Client.GameClient client;
                            if (Database.Server.GamePoll.TryGetValue(membru.UID, out client))
                            {
                                online = true;
                                client.Player.GuildRank = Role.Flags.GuildMemberRank.DeputyLeader;
                                client.Player.View.SendView(client.Player.GetArray(stream, false), false);
                                Info.MyRank = (ushort)membru.Rank;
                                SendThat(client.Player);
                            }
                            //    SendMessajGuild("" + membru.Name + " is now " + ((Role.Flags.GuildMemberRank)rank).ToString() + ".");
                            if (!online)
                            {
                                Database.ServerDatabase.LoginQueue.TryEnqueue(membru);
                            }
                            break;
                        }
                    default:
                        {
                            bool online = false;
                            RanksCounts[rank]++;
                            membru.Rank = (Role.Flags.GuildMemberRank)rank;
                            Client.GameClient client;
                            if (Database.Server.GamePoll.TryGetValue(membru.UID, out client))
                            {
                                online = true;
                                client.Player.GuildRank = membru.Rank;
                                client.Player.View.SendView(client.Player.GetArray(stream, false), false);
                                Info.MyRank = (ushort)membru.Rank;
                                SendThat(client.Player);
                            }
                            // SendMessajGuild("" + membru.Name + " is now Rank : [" + ((Role.Flags.GuildMemberRank)rank).ToString() + "].");
                            if (!online)
                            {
                                Database.ServerDatabase.LoginQueue.TryEnqueue(membru);
                            }
                            break;
                        }

                }
                return true;
            }
            return false;
        }

        public bool AllowAddAlly(string Name)
        {
            foreach (Guild all in Ally.Values)
                if (all.GuildName == Name) return false;
            return true;
        }
        public bool AllowAddEnemy(string name)
        {
            foreach (Guild all in Enemy.Values)
                if (all.GuildName == name) return false;
            return true;
        }
        public bool IsEnemy(string Name)
        {
            foreach (Guild gui in Enemy.Values)
                if (gui.GuildName == Name)
                    return true;
            return false;
        }
        public static Member GetLeaderGuild(string guildname)
        {
            foreach (var obj in GuildPoll.Values)
            {
                if (obj.GuildName == guildname)
                {
                    return obj.GetGuildLeader;
                }
            }
            return null;
        }
        public bool AddAlly(ServerSockets.Packet stream, string Name)
        {
            if (Ally.Count >= 15)
                return false;
            if (!AllowAddAlly(Name))
                return false;
            Guild GuildAlly = null;
            foreach (Guild gui in GuildPoll.Values)
            {
                if (gui.GuildName == Name)
                {
                    GuildAlly = gui;
                    break;
                }
            }
            if (GuildAlly != null)
            {
                //if (GuildAlly.Info.GuildID == Info.GuildID)
                {
                    GuildAlly.Ally.TryAdd(Info.GuildID, this);
                    Ally.TryAdd(GuildAlly.Info.GuildID, GuildAlly);
                    GuildAlly.SendGuildAlly(stream, false, null);
                    SendGuildAlly(stream, false, null);
                    return true;
                }
            }
            return false;
        }
        public void AddEnemy(ServerSockets.Packet stream, string Name)
        {
            if (Enemy.Count >= 15) return;
            if (AllowAddEnemy(Name))
            {
                Guild GuildEnnemy = null;
                foreach (Guild gui in GuildPoll.Values)
                    if (gui.GuildName == Name)
                    {
                        GuildEnnemy = gui;
                        break;
                    }
                if (GuildEnnemy != null)
                {
                    Enemy.TryAdd(GuildEnnemy.Info.GuildID, GuildEnnemy);
                    SendGuilEnnemy(stream, false, null);
                }
            }
        }
        public void UpdateGuildInfo()
        {
            Info.MembersCount = (uint)Members.Count;
        }
        public unsafe void RemoveAlly(string Name, ServerSockets.Packet stream)
        {
            if (AllowAddAlly(Name))
                return;
            Guild GuildAlly = null;
            foreach (Guild gui in Ally.Values)
                if (gui.GuildName == Name)
                {
                    GuildAlly = gui;
                    break;
                }
            if (GuildAlly != null)
            {
                foreach (Client.GameClient aclient in Database.Server.GamePoll.Values)
                {
                    if (aclient.Player.GuildID == Info.GuildID)
                        aclient.Send(stream.GuildRequestCreate(MsgGuildProces.GuildAction.RemoveAlly, GuildAlly.Info.GuildID, new int[3], ""));
                    if (aclient.Player.GuildID == GuildAlly.Info.GuildID)
                        aclient.Send(stream.GuildRequestCreate(MsgGuildProces.GuildAction.RemoveAlly, Info.GuildID, new int[3], ""));
                }
                Guild rem;
                GuildAlly.Ally.TryRemove(Info.GuildID, out rem);
                Ally.TryRemove(GuildAlly.Info.GuildID, out rem);
            }
        }
        public void SendGuilEnnemy(ServerSockets.Packet stream, bool JustMe, Client.GameClient client)
        {
            if (JustMe)
            {
                foreach (Guild GuildEnemie in Enemy.Values)
                {
                    client.Player.SendString(stream, Game.MsgServer.MsgStringPacket.StringID.GuildEnemies, GuildEnemie.Info.GuildID, false
                      , GuildEnemie.GuildName);//, new string[1] { GuildEnemie.GuildName });// + " " + GuildEnemie.Info.LeaderName + " " + GuildEnemie.Info.Level + " " + GuildEnemie.Members.Count + "" });
                }
            }
            else
            {
                foreach (Client.GameClient GuildMember in Database.Server.GamePoll.Values)
                {
                    if (GuildMember.Player.GuildID == Info.GuildID)
                    {
                        foreach (Guild GuildEnemie in Enemy.Values)
                        {
                            GuildMember.Player.SendString(stream, Game.MsgServer.MsgStringPacket.StringID.GuildEnemies, GuildEnemie.Info.GuildID, false
                             , GuildEnemie.GuildName);//  , new string[1] { GuildEnemie.GuildName });// + " " + GuildEnemie.Info.LeaderName + " " + GuildEnemie.Info.Level + " " + GuildEnemie.Members.Count });

                        }
                    }
                }
            }
        }
        public void SendGuildAlly(ServerSockets.Packet stream, bool JustMe, Client.GameClient client)
        {
            if (JustMe)
            {
                foreach (Guild AllyGuild in Ally.Values)
                {
                    client.Player.SendString(stream, Game.MsgServer.MsgStringPacket.StringID.GuildAllies, AllyGuild.Info.GuildID, false
                        , new string[1] { AllyGuild.GuildName });// " " + AllyGuild.Info.LeaderName + " " + AllyGuild.Info.Level + " " + AllyGuild.Members.Count });
                }
            }
            else
            {
                foreach (Client.GameClient GuildMember in Database.Server.GamePoll.Values)
                {
                    if (GuildMember.Player.GuildID == Info.GuildID)
                    {
                        foreach (Guild AllyGuild in Ally.Values)
                        {
                            GuildMember.Player.SendString(stream, Game.MsgServer.MsgStringPacket.StringID.GuildAllies, AllyGuild.Info.GuildID, false
                                , new string[1] { AllyGuild.GuildName });//+ " " + AllyGuild.Info.LeaderName + " " + AllyGuild.Info.Level + " " + AllyGuild.Members.Count });
                        }
                    }
                }
            }
        }
        public unsafe void RemoveEnemy(string Name, ServerSockets.Packet stream)
        {
            if (AllowAddEnemy(Name))
                return;
            Guild GuildEnemy = null;
            foreach (Guild gui in Enemy.Values)
                if (gui.GuildName == Name)
                {
                    GuildEnemy = gui;
                    break;
                }
            if (GuildEnemy != null)
            {
                foreach (Client.GameClient aclient in Database.Server.GamePoll.Values)
                {
                    if (aclient.Player.GuildID == Info.GuildID)
                    {

                        aclient.Send(stream.GuildRequestCreate(MsgGuildProces.GuildAction.RemoveEnemy, GuildEnemy.Info.GuildID, new int[3], ""));//3
                    }
                }

                Guild rem;
                Enemy.TryRemove(GuildEnemy.Info.GuildID, out rem);
            }
        }
        public unsafe void Dismis(Client.GameClient client, ServerSockets.Packet stream)
        {
            try
            {
                if (Members.Count == 1)
                {
                    Guild dismising;
                    if (GuildPoll.TryRemove(Info.GuildID, out dismising))
                    {
                        if (Ally.Count > 0)
                        {
                            foreach (var GuildAlly in Ally.Values)
                                GuildAlly.RemoveAlly(GuildName, stream);
                        }
                        //if (Enemy.Count > 0)
                        //{
                        //    foreach (var GuildEnemy in Enemy.Values)
                        //        GuildEnemy.RemoveEnemy(GuildName, stream);
                        //}
                        foreach (var Guilds in GuildPoll.Values)
                        {
                            if (Guilds.Info.GuildID != Info.GuildID)
                            {
                                //if (Guilds.Enemy.ContainsKey(Info.GuildID))
                                //    Guilds.RemoveEnemy(GuildName, stream);

                                if (Enemy.ContainsKey(Guilds.Info.GuildID))
                                    this.RemoveEnemy(Guilds.GuildName, stream);
                            }
                        }
                        client.Player.GuildID = 0;
                        client.Player.GuildRank = 0;
                        client.Player.MyGuild = null;
                        client.Player.MyGuildMember = null;


                        client.Player.View.SendView(stream.GuildRequestCreate(MsgGuildProces.GuildAction.Disband, Info.GuildID, new int[3], ""), true);

                        client.Player.View.SendView(client.Player.GetArray(stream, false), false);
                        Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(string.Format("Guild {0} has been disbanded by {1}.", GuildName, client.Player.Name), Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));

                    }
                }
                else
                {
                    SendMessajGuild("Please kick all members before deleting.");
                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }

        public unsafe void SendMessajGuild(string Messaj, Game.MsgServer.MsgMessage.ChatMode ChatType = Game.MsgServer.MsgMessage.ChatMode.Guild
            , Game.MsgServer.MsgMessage.MsgColor color = Game.MsgServer.MsgMessage.MsgColor.yellow)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                foreach (var user in Database.Server.GamePoll.Values)
                {
                    if (user.Player.GuildID == Info.GuildID)
                        user.Send(new Game.MsgServer.MsgMessage(Messaj, color, ChatType).GetArray(stream));
                }
            }
        }

        public unsafe void SendPacket(ServerSockets.Packet packet, uint sender = 0)
        {
            foreach (var user in Database.Server.GamePoll.Values.Where(e => e.Player.UID != sender))
            {
                if (user.Player.GuildID == Info.GuildID)
                    user.Send(packet);
            }
        }
        public unsafe void SendThat(Role.Player player)
        {
            if (player.MyGuildMember == null)
                return;
            if (Bulletin != null && Bulletin != "" && Bulletin != "None")
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    //  player.Owner.Send(stream.GuildRequestCreate(MsgGuildProces.GuildAction.Bulletin, (uint)0, new int[3], Bulletin));
                    foreach (Member memb in player.MyGuild.Members.Values)
                        player.Owner.Send(new MsgMessage(Bulletin, memb.Name, MsgMessage.MsgColor.white, MsgMessage.ChatMode.GuildAnnouncement).GetArray(stream));
                }
            }
            Info.MembersCount = (uint)Members.Count;
            Info.MyRank = (ushort)player.MyGuildMember.Rank;

            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();

                player.Owner.Send(stream.GuildInformationCreate(Info));

            }
        }
        public Member GetMember(string name)
        {
            foreach (Member memb in Members.Values)
                if (memb.Name == name)
                    return memb;
            return null;
        }
        public void Quit(string name, bool ReceiveKick, ServerSockets.Packet stream)
        {
            var getleader = Members.Values.Where(p => p.Rank == Flags.GuildMemberRank.GuildLeader).FirstOrDefault();
            if (ReceiveKick && name == getleader.Name)
                return;
            Member member = GetMember(name);
            if (member == null) return;
            if (ReceiveKick)
            {
                SendMessajGuild(member.Name + " has been kicked from our Guild.");
            }
            else
            {
                SendMessajGuild(member.Name + " has quit our Guild.");
            }
            if (member.Rank == Flags.GuildMemberRank.DeputyLeader)
                RanksCounts[(ushort)Flags.GuildMemberRank.DeputyLeader]--;

            Client.GameClient client;
            if (Database.Server.GamePoll.TryGetValue(member.UID, out client))
            {
                client.Player.GuildID = 0;
                client.Player.GuildRank = 0;
                client.Player.MyGuild = null;
                client.Player.MyGuildMember = null;

                client.Send(stream.GuildRequestCreate(MsgGuildProces.GuildAction.Disband, Info.GuildID, new int[3], ""));


                client.Player.View.Clear(stream);
                client.Player.View.Role();

            }
            else
            {
                Database.ServerDatabase.LoginQueue.TryEnqueue(new UpdateDB()
                {
                    GuildID = 0,
                    Rank = Flags.GuildMemberRank.None,
                    UID = member.UID
                });



            }
            Members.TryRemove(member.UID, out member);
            Info.MembersCount = (uint)Members.Count;




        }
        public static bool AllowToCreate(string Name)
        {
            if (!Program.NameStrCheck(Name))
                return false;
            foreach (Guild guil in GuildPoll.Values)
                if (guil.GuildName == Name)
                    return false;

            return true;
        }
        public override string ToString()
        {
            Database.DBActions.WriteLine writer = new Database.DBActions.WriteLine('/');
            return writer.Add(Info.GuildID).Add(GuildName).Add(Info.LeaderName).Add(Info.SilverFund).Add(Info.Donation)
                  .Add(Info.CreateTime).Add(Bulletin).Close();
        }
    }
}
