using COServer.Database;
using COServer.Game.MsgFloorItem;
using COServer.Game.MsgTournaments;
using System;
using System.IO;
using System.Linq;

namespace COServer.Game.MsgServer
{
    public class MsgMessage
    {
        public enum MsgColor : uint
        {
            Blue = 0x0000FF,
            red = 0xFF0000,
            Green = 0x00FF00,
            yellow = 0xFFFF00,
            Pink = 0xFF00FF,
            black = 0x000000,
            white = 0xFFFFFF,
        }
        public enum ChatMode : uint
        {
            MsgTrade = 2201,
            MsgFriend = 2202,
            MsgTeam = 2203,
            MsgSyn = 2204,
            MsgOther = 2205,
            MsgSystem = 2206,
            Talk = 2000,
            Whisper = 2001,
            Action = 2002,
            Team = 2003,
            Guild = 2004,
            TopLeftSystem = 2005,
            Family = 2006,
            System = 2007,//2007,
            Yelp = 2008,
            Friend = 2009,
            Global = 2010,
            Center = 2011,
            TopLeft = 2012,
            Die = 2013,
            Service = 2014,
            Tip = 2015,
            CrossServerIcon = 2016,
            Ally = 2025,
            WebSite = 2105,
            World = 2021,
            CryOut = 2104,
            PetTalk = 2103,
            Shop = 2102,
            LeaveWord = 2110,
            PopUP = 2100,
            Dialog = 2101,
            MessageBox = 2112,
            FirstRightCorner = 2108,
            ContinueRightCorner = 2109,
            SystemWhisper = 2110,
            GuildAnnouncement = 2111,
            Task =2106,
            Help = 2014,
            BroadcastMessage = 2500,
            Monster = 2600,
            SlideFromRight = 100000,
            HawkMessage = 2104,
            SlideFromRightRedVib = 1000000,
            WhiteVibrate = 10000000
        }

        public string _From;
        public string _To;
        public ChatMode ChatType;
        public MsgColor Color;
        public string __Message;
        public string ServerName = string.Empty;

        public uint Mesh;
        public uint MessageUID1 = 0;
        public uint MessageUID2 = 0;


        public MsgMessage(string _Message, MsgColor _Color, ChatMode _ChatType)
        {
            this.Mesh = 0;
            this.__Message = _Message;
            this._To = "ALL";
            this._From = "SYSTEM";
            this.Color = _Color;
            this.ChatType = _ChatType;
        }
        public MsgMessage(string _Message, string __To, MsgColor _Color, ChatMode _ChatType)
        {
            this.Mesh = 0;
            this.__Message = _Message;
            this._To = __To;
            this._From = "SYSTEM";
            this.Color = _Color;
            this.ChatType = _ChatType;
        }
        public MsgMessage(string _Message, string __To, string __From, MsgColor _Color, ChatMode _ChatType)
        {
            this.Mesh = 0;
            this.__Message = _Message;
            this._To = __To;
            this._From = __From;
            this.Color = _Color;
            this.ChatType = _ChatType;
        }
        public MsgMessage()
        {
            this.Mesh = 0;
        }
        public unsafe void Deserialize(ServerSockets.Packet stream)
        {
            //stream.ReadUInt32();
            Color = (MsgColor)stream.ReadUInt32();
            ChatType = (ChatMode)stream.ReadUInt32();
            MessageUID1 = stream.ReadUInt32();
            MessageUID2 = stream.ReadUInt32();
            Mesh = stream.ReadUInt32();//24
            string[] str = stream.ReadStringList();//34

            _From = str[0];
            _To = str[1];
            __Message = str[3];
            if (str.Length > 6)
            {
                ServerName = str[6];
            }
        }
        public unsafe ServerSockets.Packet GetArray(ServerSockets.Packet stream, uint Rank = 0)
        {
            stream.InitWriter();
            stream.Write((uint)this.Color);//4
            stream.Write((uint)this.ChatType);//8
            stream.Write(MessageUID1);//12
            stream.Write(MessageUID2);//16
            stream.Write(Mesh);//20
            stream.Write(_From, _To, string.Empty, __Message/*, string.Empty, string.Empty, ServerName*/);
            //too 43
            stream.Finalize(GamePackets.Chat);
            return stream;

        }


        [PacketAttribute(GamePackets.Chat)]
        public unsafe static void MsgHandler(Client.GameClient client, ServerSockets.Packet packet)
        {

            MsgMessage msg = new MsgMessage();
            msg.Deserialize(packet);
            if (!ChatCommands(client, msg))
            {
                if (msg._From == "SYSTEM" || msg._To == "SYSTEM")
                    return;
                msg.Mesh = client.Player.Mesh;
                if (msg.ChatType == ChatMode.Die)
                    msg.ChatType = MsgMessage.ChatMode.Talk;
                switch (msg.ChatType)
                {
                    case ChatMode.GuildAnnouncement:
                        {
                            if (client.Player.MyGuild != null && client.Player.GuildRank == Role.Flags.GuildMemberRank.GuildLeader)
                            {
                                if (client.Player.Name != client.Player.MyGuild.Info.LeaderName)
                                    break;
                                if (msg.__Message.Length > 0 && msg.__Message != null)
                                {
                                    if (Program.NameStrCheck(msg.__Message, false))
                                    {
                                        client.Player.MyGuild.Bulletin = msg.__Message;
                                        client.Player.MyGuild.SendThat(client.Player);
                                        client.Send(msg.GetArray(packet));
                                    }
                                    else
                                    {
                                        client.SendSysMesage("Invalid characters in Bulletin.");
                                    }
                                }
                            }
                            break;
                        }
                    case ChatMode.Friend:
                        {
                            System.Collections.Concurrent.ConcurrentDictionary<uint, Role.Instance.Associate.Member> friends;
                            if (client.Player.Associate.Associat.TryGetValue(Role.Instance.Associate.Friends, out friends))
                            {
                                foreach (var user in Database.Server.GamePoll.Values)
                                {
                                    if (friends.ContainsKey(user.Player.UID))
                                        user.Send(msg.GetArray(packet));
                                }
                            }
                            //Program.DiscordAPI.Enqueue("{msg._From} speaks to {msg._To}: {msg.__Message}");
                            //Program.SendGlobalPackets.Enqueue(msg.GetArray());
                            break;
                        }
                    case ChatMode.HawkMessage:
                        {
                            if (client.IsVendor)
                            {
                                client.MyVendor.HalkMeesaje = msg;

                                client.Player.View.SendView(msg.GetArray(packet), true);
                            }
                            break;
                        }
                    case ChatMode.Team:
                        {
                            if (client.Team != null)
                                client.Team.SendTeam(msg.GetArray(packet), client.Player.UID);
                            break;
                        }
                    case MsgMessage.ChatMode.Talk:
                        {
                            //if (msg.__Message.StartsWith("/all"))
                            //{
                            //    msg.__Message.Replace("/all", "");
                            //    if (Time32.Now > client.Player.LastWorldMessaj.AddSeconds(15))
                            //    {
                            //        client.Player.LastWorldMessaj = Time32.Now;
                            //        foreach (var user in Database.Server.GamePoll.Values)
                            //        {
                            //            if (user.Player.UID != client.Player.UID)
                            //            {
                            //                user.Send(msg.GetArray(packet));
                            //            }
                            //        }
                            //    }
                            //}
                            //else
                                 client.Player.View.SendView(msg.GetArray(packet), false);
                            break;
                        }
                    case MsgMessage.ChatMode.Service://World://Change Service to World for the in-game messaging.
                        {
                            if (Time32.Now > client.Player.LastWorldMessaj.AddSeconds(15))
                            {
                                if (msg._To != string.Empty)
                                {
                                }
                                else
                                {
                                    msg._To = "[World]";
                                }
                                client.Player.LastWorldMessaj = Time32.Now;
                                foreach (var user in Database.Server.GamePoll.Values)
                                {
                                    if (user.Player.UID != client.Player.UID)
                                    {
                                        user.Send(msg.GetArray(packet));
                                    }
                                }
                                // Adiciona a chamada da API Discord
                                Program.DiscordAPIworld.Enqueue($"``[{client.Player.Name}] speaks to the world: {msg.__Message}``");
                            }
                            break;
                        }
                    case MsgMessage.ChatMode.World:
                        {
                            if (Time32.Now > client.Player.LastWorldMessaj.AddSeconds(15))
                            {
                                client.Player.LastWorldMessaj = Time32.Now;

                                // Envia a mensagem para todos os jogadores, exceto para o remetente
                                foreach (var user in Database.Server.GamePoll.Values)
                                {
                                    if (user.Player.UID != client.Player.UID)
                                    {
                                        user.Send(msg.GetArray(packet));
                                    }
                                }
                            }
                            break;
                        }
                    case ChatMode.Whisper:
                        {
                            bool send = false;
                            foreach (var user in Database.Server.GamePoll.Values)
                            {

                                if (user.Player.Name == msg._To)
                                {
                                    client.Player.Target = user.Player;
                                    if (msg._To == client.Player.Target.Name && msg.__Message == ".") break;
                                    msg.Mesh = client.Player.Mesh;
                                    user.Send(msg.GetArray(packet));
                                    send = true;
                                    break;
                                }
                            }
                            if (!send)
                            {
                                client.SendSysMesage("This player isn't online.", ChatMode.System, MsgColor.white);
                            }
                            break;
                        }
                    case ChatMode.Guild:
                        {
                            if (client.Player.MyGuild != null)
                            {
                                if (client.Player.MyGuild.Info.SilverFund >= 50000)
                                {
                                    //if (msg.__Message.ToLower().StartsWith("/ally")) 
                                    //{
                                    //    msg._To = "[GUILDALLIES]";
                                    //    foreach (var guild in client.Player.MyGuild.Ally.Values)
                                    //        guild.SendPacket(msg.GetArray(packet));
                                    //}
                                    //else
                                        client.Player.MyGuild.SendPacket(msg.GetArray(packet), client.Player.UID);
                                    if (MsgSchedules.GuildWar.Proces == ProcesType.Alive)
                                    {
                                        msg._To = "[GUILDALLIES]";
                                        foreach (var guild in client.Player.MyGuild.Ally.Values)
                                            guild.SendPacket(msg.GetArray(packet));
                                    }

                                }
                                else
                                {
                                    client.SendSysMesage("Your Guild Fund is too low. You need to donate some gold to use the guild chat!", ChatMode.Guild);
                                }
                            }
                           
                            break;
                        }
                    case ChatMode.MsgTrade:
                        {
                            Role.MessageBoard.MessageInfo Info =
                                Role.MessageBoard.GetMsgInfoByAuthor(client.Player.Name, (UInt16)msg.ChatType);

                            Role.MessageBoard.Delete(Info, (UInt16)msg.ChatType);
                            Role.MessageBoard.Add(client.Player.Name, msg.__Message, (UInt16)msg.ChatType);
                            break;
                        }
                    case ChatMode.MsgFriend:
                        {
                            Role.MessageBoard.MessageInfo Info =
                                Role.MessageBoard.GetMsgInfoByAuthor(client.Player.Name, (UInt16)msg.ChatType);

                            Role.MessageBoard.Delete(Info, (UInt16)msg.ChatType);
                            Role.MessageBoard.Add(client.Player.Name, msg.__Message, (UInt16)msg.ChatType);
                            break;
                        }
                    case ChatMode.MsgTeam:
                        {
                            Role.MessageBoard.MessageInfo Info =
                                Role.MessageBoard.GetMsgInfoByAuthor(client.Player.Name, (UInt16)msg.ChatType);

                            Role.MessageBoard.Delete(Info, (UInt16)msg.ChatType);
                            Role.MessageBoard.Add(client.Player.Name, msg.__Message, (UInt16)msg.ChatType);
                            break;
                        }
                    case ChatMode.MsgSyn:
                        {
                            Role.MessageBoard.MessageInfo Info =
                                Role.MessageBoard.GetMsgInfoByAuthor(client.Player.Name, (UInt16)msg.ChatType);

                            Role.MessageBoard.Delete(Info, (UInt16)msg.ChatType);
                            Role.MessageBoard.Add(client.Player.Name, msg.__Message, (UInt16)msg.ChatType);
                            break;
                        }
                    case ChatMode.MsgOther:
                        {
                            Role.MessageBoard.MessageInfo Info =
                                Role.MessageBoard.GetMsgInfoByAuthor(client.Player.Name, (UInt16)msg.ChatType);

                            Role.MessageBoard.Delete(Info, (UInt16)msg.ChatType);
                            Role.MessageBoard.Add(client.Player.Name, msg.__Message, (UInt16)msg.ChatType);
                            break;
                        }
                    case ChatMode.MsgSystem:
                        {
                            Role.MessageBoard.MessageInfo Info =
                                Role.MessageBoard.GetMsgInfoByAuthor(client.Player.Name, (UInt16)msg.ChatType);

                            Role.MessageBoard.Delete(Info, (UInt16)msg.ChatType);
                            Role.MessageBoard.Add(client.Player.Name, msg.__Message, (UInt16)msg.ChatType);
                            break;
                        }
                }

            }
        }
        public static unsafe bool ChatCommands(Client.GameClient client, MsgMessage msg)
        {

            try
            {


                string logss = "[Chat]" + msg._From + " to " + msg._To + " " + msg.__Message + "";
                Database.ServerDatabase.LoginQueue.Enqueue(logss);

                msg.__Message = msg.__Message.Replace("#60", "").Replace("#61", "").Replace("#62", "").Replace("#63", "").Replace("#64", "").Replace("#65", "").Replace("#66", "").Replace("#67", "").Replace("#68", "");

                if (msg.__Message.StartsWith("/"))
                {
                    string logs = "[GMLogs]" + client.Player.Name + " ";

                    string Message = msg.__Message.Substring(1);//.ToLower();
                    string[] data = Message.Split(' ');
                    for (int x = 0; x < data.Length; x++)
                        logs += data[x] + " ";
                    Database.ServerDatabase.LoginQueue.Enqueue(logs);
                    bool keep = false;
                    switch (data[0])
                    {
                        case "addtop":
                            {
                                client.EffectStatus.Add(data[1], DateTime.Now.AddSeconds(10));
                                break;
                            }
                        case "remtop":
                            {
                                client.EffectStatus.Remove(data[1]);
                                break;
                            }
                        case "start":
                            {
                                switch (data[1])
                                {
                                    case "fivenot": MsgSchedules._Get5Out.Open(); break;
                                    //case "ss_fb": MsgTournaments.MsgSchedules._Ss_Fb.Open(); break;
                                    //case "topconquer": MsgTournaments.MsgSchedules._ConquerPk.Open(); break;
                                    case "lastman": MsgTournaments.MsgSchedules._LastMan.Open(); break;
                                    //case "firstkiller": MsgSchedules._FirstKiller.Open(); break;
                                    //case "topblack": MsgSchedules._Top_Black.Open(); break;
                                    case "topgender": MsgSchedules._GenderWar.Open(); break;
                                        //case "dragonwar": MsgSchedules.Dragonwar = true; break;
                                        //case "luckybox": MsgSchedules.Luckybox = true; break;
                                        //case "killthecaptain": MsgSchedules.Killthecaptain = true; break;
                                        //case "capturetheBag": MsgSchedules.CapturetheBag = true; break;
                                        //case "deathmatch": MsgSchedules.Deathmatch = true; break;
                                        //case "freezewar": MsgSchedules.Freezewar = true; break;
                                        //case "guildsdeathmatch": MsgSchedules.Guildsdeathmatch = true; break;
                                        //case "teamfreezewar": MsgSchedules.Teamfreezewar = true; break;
                                        //case "war_portal": MsgSchedules.War_portal = true; break;
                                        //case "midnight_wolf": MsgSchedules.Midnight_wolf = true; break;
                                }
                                break;
                            }
                        #region PoleDomination
                        case "pole_phoenix":
                            {
                                switch (data[1])
                                {
                                    case "on": MsgSchedules.PoleDominationPC.Start(); break;
                                    case "off": MsgSchedules.PoleDominationPC.CompleteEndGuildWar(); break;
                                }
                                break;
                            }
                        case "pole_desert":
                            {
                                switch (data[1])
                                {
                                    case "on": MsgSchedules.PoleDominationDC.Start(); break;
                                    case "off": MsgSchedules.PoleDominationDC.CompleteEndGuildWar(); break;
                                }
                                break;
                            }
                        case "pole_bird":
                            {
                                switch (data[1])
                                {
                                    case "on": MsgSchedules.PoleDominationBI.Start(); break;
                                    case "off": MsgSchedules.PoleDominationBI.CompleteEndGuildWar(); break;
                                }
                                break;
                            }
                        case "pole_ape":
                            {
                                switch (data[1])
                                {
                                    case "on": MsgSchedules.PoleDomination.Start(); break;
                                    case "off": MsgSchedules.PoleDomination.CompleteEndGuildWar(); break;
                                }
                                break;
                            }
                        #endregion
                        case "extremeflag":
                            {
                                switch (data[1])
                                {
                                    case "on": MsgSchedules._ExtremeFlagWar.Start(); break;
                                    case "off": MsgSchedules._ExtremeFlagWar.CompleteEndGuildWar(); break;
                                }
                                break;
                            }
                        case "eliteguildwar":
                            {
                                switch (data[1])
                                {
                                    case "on": MsgSchedules._EliteGuildWar.Start(); break;
                                    case "off": MsgSchedules._EliteGuildWar.CompleteEndGuildWar(); break;
                                }
                                break;
                            }
                        //case "firepolewar":
                        //    {
                        //        switch (data[1])
                        //        {
                        //            case "on": MsgSchedules._FirePoleWar.Start(); break;
                        //            case "off": MsgSchedules._FirePoleWar.CompleteEndGuildWar(); break;
                        //        }
                        //        break;
                        //    }
                        case "pvp":
                            {
                                EventsLib.EventManager.JoinPVP(client);
                                keep = true;
                                break;
                            }
                    }
                    if (keep)
                        return true;
                    if (client.ProjectManager)
                    {
                        switch (data[0])
                        {
                            case "boss1":
                                {
                                    MsgMonster.BossesBase.SpawnHandler(1015, 806, 558, 20070, "Snow Banshee", "will appear at " + DateTime.Now.Hour + ":30! Get ready to fight! You only have 5 minutes left!", " has spawned in " +
                    Database.Server.MapName[1015] + "!");
                                    break;
                                }
                            case "event":
                                {
                                    switch (data[1])
                                    {
                                        case "2":
                                            {
                                                EventsLib.EventManager.freezewar.LastSpawn = DateTime.Now;
                                                EventsLib.EventManager.freezewar.senton = DateTime.Now;
                                                EventsLib.EventManager.SetEvent(EventsLib.EventManager.freezewar.name,
                                                    EventsLib.EventManager.freezewar.map);
                                                EventsLib.EventManager.freezewar.SendInvitation();
                                                break;
                                            }
                                        case "3":
                                            {
                                                EventsLib.EventManager.guildsdm.LastSpawn = DateTime.Now;
                                                EventsLib.EventManager.guildsdm.senton = DateTime.Now;
                                                EventsLib.EventManager.SetEvent(EventsLib.EventManager.guildsdm.name,
                                                    EventsLib.EventManager.guildsdm.map);
                                                EventsLib.EventManager.guildsdm.SendInvitation();

                                                break;
                                            }
                                        //case "4":
                                        //    {
                                        //        EventsLib.EventManager.kingofthehill.LastSpawn = DateTime.Now;
                                        //        EventsLib.EventManager.kingofthehill.senton = DateTime.Now;
                                        //        EventsLib.EventManager.SetEvent(EventsLib.EventManager.kingofthehill.name,
                                        //            EventsLib.EventManager.kingofthehill.map);
                                        //        EventsLib.EventManager.kingofthehill.SendInvitation();

                                        //        break;
                                        //    }
                                        //case "4":
                                        //    {
                                        //        EventsLib.EventManager.ctb.LastSpawn = DateTime.Now;
                                        //        EventsLib.EventManager.ctb.senton = DateTime.Now;
                                        //        EventsLib.EventManager.SetEvent(EventsLib.EventManager.ctb.name,
                                        //            EventsLib.EventManager.ctb.map);
                                        //        EventsLib.EventManager.ctb.SendInvitation();

                                        //        break;
                                        //    }
                                        case "4":
                                            {
                                                EventsLib.EventManager.deathmatch.LastSpawn = DateTime.Now;
                                                EventsLib.EventManager.deathmatch.senton = DateTime.Now;
                                                EventsLib.EventManager.SetEvent(EventsLib.EventManager.deathmatch.name,
                                                    EventsLib.EventManager.deathmatch.map);
                                                EventsLib.EventManager.deathmatch.SendInvitation();

                                                break;
                                            }
                                        case "5":
                                            {
                                                EventsLib.EventManager.killthecaptain.LastSpawn = DateTime.Now;
                                                EventsLib.EventManager.killthecaptain.senton = DateTime.Now;
                                                EventsLib.EventManager.SetEvent(EventsLib.EventManager.killthecaptain.name,
                                                    EventsLib.EventManager.killthecaptain.map);
                                                EventsLib.EventManager.killthecaptain.SendInvitation();

                                                break;
                                            }
                                        case "6":
                                            {
                                                EventsLib.EventManager.killhunted.LastSpawn = DateTime.Now;
                                                EventsLib.EventManager.killhunted.senton = DateTime.Now;
                                                EventsLib.EventManager.SetEvent(EventsLib.EventManager.killhunted.name,
                                                    EventsLib.EventManager.killhunted.map);
                                                EventsLib.EventManager.killhunted.SendInvitation();

                                                break;
                                            }
                                        //case "9":
                                        //    {
                                        //        EventsLib.EventManager.teamfreezewar.LastSpawn = DateTime.Now;
                                        //        EventsLib.EventManager.teamfreezewar.senton = DateTime.Now;
                                        //        EventsLib.EventManager.SetEvent(EventsLib.EventManager.teamfreezewar.name,
                                        //            EventsLib.EventManager.teamfreezewar.map);
                                        //        EventsLib.EventManager.teamfreezewar.SendInvitation();

                                        //        break;
                                        //    }
                                        //case "10":
                                        //    {
                                        //        EventsLib.EventManager.dragonwar.LastSpawn = DateTime.Now;
                                        //        EventsLib.EventManager.dragonwar.senton = DateTime.Now;
                                        //        EventsLib.EventManager.SetEvent(EventsLib.EventManager.dragonwar.name,
                                        //            EventsLib.EventManager.dragonwar.map);
                                        //        EventsLib.EventManager.dragonwar.SendInvitation();

                                        //        break;
                                        //    }
                                    }
                                    break;
                                }
                            case "test11":
                                {

                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.Player.View.SendView(new Game.MsgServer.MsgMessage("You've gained experience worth ExpBalls.", MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.Center).GetArray(stream), true);
                                    }
                                    break;
                                }
                            case "trash":
                                {
                                    if (client.Player.Map == 1572) // Checa se o jogador está no mapa 1572
                                    {
                                        client.SendSysMesage("Você não pode usar o comando /trash neste mapa!", MsgMessage.ChatMode.TopLeftSystem);
                                    }
                                    else
                                    {
                                        client.Player.TrashGold = !client.Player.TrashGold;
                                        client.Player.TrashItems = !client.Player.TrashItems;
                                        client.SendSysMesage("VIP Drop Gold: " + (client.Player.TrashGold ? "Enabled" : "Disabled"), MsgMessage.ChatMode.TopLeftSystem);
                                        client.SendSysMesage("VIP Drop Trash Items: " + (client.Player.TrashItems ? "Enabled" : "Disabled"), MsgMessage.ChatMode.TopLeftSystem);
                                    }
                                    break;
                                }
                            case "vip":
                                {
                                    foreach (var user in Database.Server.GamePoll.Values)
                                    {
                                        if (user.Player.Name.ToLower() == data[1].ToLower())
                                        {

                                            if (DateTime.Now > user.Player.ExpireVip)
                                                user.Player.ExpireVip = DateTime.Now.AddDays(30);
                                            else
                                                user.Player.ExpireVip = user.Player.ExpireVip.AddDays(30);

                                            user.Player.VipLevel = (byte)uint.Parse(data[2]);
                                            using (var rec = new ServerSockets.RecycledPacket())
                                            {
                                                var stream = rec.GetStream();
                                                user.Player.SendUpdate(stream, user.Player.VipLevel, MsgUpdate.DataType.VIPLevel);
                                            }
                                            user.CreateBoxDialog("You`ve received VIP6 (30 days). Thanks for your donation!");

                                            break;
                                        }
                                    }
                                    break;
                                }
                            case "hp":
                                {
                                    client.Player.HitPoints = int.Parse(data[1]);
                                    break;
                                }
                            case "w":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();

                                        ActionQuery action = new ActionQuery()
                                        {
                                            dwParam = uint.Parse(data[1]),
                                            Type = ActionType.OpenDialog,
                                            ObjId = client.Player.UID,
                                            wParam1 = client.Player.X,
                                            wParam2 = client.Player.Y
                                        };
                                        client.Send(stream.ActionCreate(&action));
                                    }
                                    break;
                                }
                            case "d":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();

                                        ActionQuery action = new ActionQuery()
                                        {
                                            dwParam = uint.Parse(data[1]),
                                            Type = ActionType.Dialog,
                                            ObjId = client.Player.UID,
                                            wParam1 = client.Player.X,
                                            wParam2 = client.Player.Y
                                        };
                                        client.Send(stream.ActionCreate(&action));
                                    }
                                    break;
                                }
                            case "c":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();

                                        ActionQuery action = new ActionQuery()
                                        {
                                            dwParam = uint.Parse(data[1]),
                                            Type = ActionType.OpenCustom,
                                            ObjId = client.Player.UID,
                                            wParam1 = client.Player.X,
                                            wParam2 = client.Player.Y
                                        };
                                        client.Send(stream.ActionCreate(&action));
                                    }
                                    break;
                                }
                            case "sob":
                                {
                                    var Pole = new Role.SobNpc();
                                    Pole.X = client.Player.X;
                                    Pole.Map = client.Player.Map;
                                    Pole.ObjType = Role.MapObjectType.SobNpc;
                                    Pole.Y = client.Player.Y;
                                    Pole.DynamicID = 0;
                                    Pole.UID = 890;//3333444
                                    Pole.Type = Role.Flags.NpcType.Stake;
                                    Pole.Mesh = (Role.SobNpc.StaticMesh)(uint.Parse(data[1]));
                                    Pole.Name = "Pole";
                                    Pole.HitPoints = 30000000;

                                    Pole.MaxHitPoints = 30000000;
                                    Pole.Sort = 17;
                                    client.Map.View.EnterMap<Role.IMapObj>(Pole);
                                    client.Map.SetFlagNpc(Pole.X, Pole.Y);
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        foreach (var user in client.Map.View.Roles(Role.MapObjectType.Player, Pole.X, Pole.Y))
                                        {
                                            client.Send(Pole.GetArray(stream, false));
                                        }
                                    }
                                    break;
                                }
                            case "ggb":
                                {
                                    client.Player.HitPoints += int.Parse(data[1]);
                                    client.Player.Owner.Status.MaxHitpoints = uint.Parse(data[2]);
                                    client.Player.SendUpdateHP();
                                    break;
                                }
                            case "aa":
                                {

                                    MsgMonster.BossesBase.SpawnHandler(1787, 48, 38, 20070, "Dragon", "will appear at " + DateTime.Now.Hour + ":30! Get ready to fight! You only have 5 minutes left!", " has spawned in Dragon Island!");
                                    break;
                                }
                            case "addeffect":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();

                                        client.Player.SendString(stream, MsgStringPacket.StringID.Effect, true, new string[1] { data[1].ToString() });

                                    }
                                    //using (var rec = new ServerSockets.RecycledPacket())
                                    //{
                                    //    var stream = rec.GetStream();

                                    //    ActionQuery action = new ActionQuery();
                                    //    action.ObjId = client.Player.UID;
                                    //    action.Type = ActionType.ChangeStance;
                                    //    action.dwParam = uint.Parse(data[1]);
                                    //    client.Player.Action = (Role.Flags.ConquerAction)action.dwParam;
                                    //    client.Player.View.SendView(stream.ActionCreate(&action), true);
                                    //}
                                    //client.SendSysMesage("You are lucky, 1", MsgMessage.ChatMode.FirstRightCorner, MsgMessage.MsgColor.black);
                                    //client.SendSysMesage("You are lucky, 1", MsgMessage.ChatMode.ContinueRightCorner, MsgMessage.MsgColor.Blue);
                                    //client.SendSysMesage("You are lucky, 1123", (MsgMessage.ChatMode)2112, MsgMessage.MsgColor.Blue);
                                    //client.SendSysMesage("You are lucky, 213", (MsgMessage.ChatMode)2113, MsgMessage.MsgColor.Blue);
                                    //client.SendSysMesage("You are lucky, 214", (MsgMessage.ChatMode)2114, MsgMessage.MsgColor.Blue);
                                    //client.SendSysMesage("You are lucky, 205", (MsgMessage.ChatMode)2205, MsgMessage.MsgColor.Blue);
                                    //client.SendSysMesage("You are lucky, _TXTATR_DIALOG_BEGIN", (MsgMessage.ChatMode)2500, MsgMessage.MsgColor.Blue);
                                    //client.SendSysMesage("You are lucky, _TXTATR_ 2006", (MsgMessage.ChatMode)2600, MsgMessage.MsgColor.Blue);
                                    //client.SendSysMesage("You are lucky, 1", MsgMessage.ChatMode.Action, MsgMessage.MsgColor.Green);
                                    //client.SendSysMesage("You are lucky, 1", MsgMessage.ChatMode.Action, MsgMessage.MsgColor.Pink);
                                    //client.SendSysMesage("You are lucky, 1", MsgMessage.ChatMode.Action, MsgMessage.MsgColor.red);
                                    //client.SendSysMesage("You are lucky, 1", MsgMessage.ChatMode.Action, MsgMessage.MsgColor.white);
                                    //client.SendSysMesage("You are lucky, 1", MsgMessage.ChatMode.Action, MsgMessage.MsgColor.yellow);

                                    //client.SendSysMesage("You are lucky, 0", MsgMessage.ChatMode.System);
                                    //client.SendSysMesage("You are lucky, 1", MsgMessage.ChatMode.Action, MsgMessage.MsgColor.Green);
                                    //client.SendSysMesage("You are lucky, 2", MsgMessage.ChatMode.Family, MsgMessage.MsgColor.yellow);
                                    //client.SendSysMesage("You are lucky, 3", MsgMessage.ChatMode.Yelp, MsgMessage.MsgColor.yellow);
                                    //client.SendSysMesage("You are lucky, 4", MsgMessage.ChatMode.Global, MsgMessage.MsgColor.yellow);
                                    //client.SendSysMesage("You are lucky, 5", MsgMessage.ChatMode.Tip, MsgMessage.MsgColor.yellow);
                                    //client.SendSysMesage("You are lucky, 6", MsgMessage.ChatMode.CryOut, MsgMessage.MsgColor.yellow);
                                    //client.SendSysMesage("You are lucky, 7", MsgMessage.ChatMode.PetTalk, MsgMessage.MsgColor.yellow);
                                    //client.SendSysMesage("You are lucky, 8", MsgMessage.ChatMode.Shop, MsgMessage.MsgColor.yellow);
                                    //client.SendSysMesage("You are lucky, 9", MsgMessage.ChatMode.LeaveWord, MsgMessage.MsgColor.yellow);
                                    //client.SendSysMesage("You are lucky, 10", MsgMessage.ChatMode.MessageBox, MsgMessage.MsgColor.yellow);
                                    //client.SendSysMesage("You are lucky, 11", MsgMessage.ChatMode.Task, MsgMessage.MsgColor.yellow);
                                    //client.SendSysMesage("You are lucky, 13", MsgMessage.ChatMode.Help, MsgMessage.MsgColor.yellow);
                                    //client.SendSysMesage("You are lucky, 14", MsgMessage.ChatMode.BroadcastMessage, MsgMessage.MsgColor.yellow);
                                    //client.SendSysMesage("You are lucky, 15", MsgMessage.ChatMode.Monster, MsgMessage.MsgColor.yellow);

                                    break;
                                }
                            //case "ai":
                            //    {
                            //        MsgSchedules.CityWar.Open();
                            //        break;
                            //    }
                            case "chr":
                                {
                                    client.CreateBoxDialog("You killed a Heaven Demon and found a Frost CP Pack (69000CPs)!");
                                    client.TotalMobsKilled += 100000000;
                                    break;
                                }
                            case "gb":
                                {
                                    if (client.Player.SpawnGuildBeast)
                                    {
                                        var Map = Database.Server.ServerMaps[1038];

                                        if (!Map.ContainMobID(3120))
                                        {
                                            using (var rec = new ServerSockets.RecycledPacket())
                                            {
                                                var stream = rec.GetStream();
                                                Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Warning! The GuildBeast has appeared in the GuildCastle!", "ALLUSERS", "Server", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                                                Database.Server.AddMapMonster(stream, Map, 3120, 87, 77, 1, 1, 1);
                                            }
                                        }
                                    }

                                    break;
                                }
                            case "gbr":
                                {
                                    #region GuildBeast
                                    if (client.Player.SpawnGuildBeast)
                                    {
                                        client.Player.SpawnGuildBeast = false;
                                        var Map = Database.Server.ServerMaps[1038];
                                        if (Map.GetMobLoc(3120) != null)
                                        {
                                            using (var rec = new ServerSockets.RecycledPacket())
                                            {
                                                var stream = rec.GetStream();

                                                var Array = Map.View.GetAllMapRoles(Role.MapObjectType.Monster);
                                                foreach (var map_mob in Array)
                                                {
                                                    if (map_mob.Map == 1038 && map_mob.Alive)
                                                    {
                                                        var Monster = (map_mob as Game.MsgMonster.MonsterRole);
                                                        if (Monster.Name == "GuildBeast")
                                                        {
                                                            Monster.Dead(stream, client, 3120, Map);
                                                            if (client.Player.GuildID == Game.MsgTournaments.MsgSchedules.GuildWar.Winner.GuildID
                                                           && client.Player.MyGuild != null
                                                           && client.Player.GuildRank == Role.Flags.GuildMemberRank.GuildLeader &&
                                                           Game.MsgTournaments.MsgSchedules.GuildWar.Proces == Game.MsgTournaments.ProcesType.Dead)
                                                            {
                                                                client.Player.GuildBeastClaimd = true;
                                                                client.SendSysMesage("You've received a DragonBall from GuildBest!");
                                                                client.Inventory.Add(stream, Database.ItemType.DragonBall, 1);
                                                            }
                                                        }
                                                    }

                                                }

                                            }
                                        }

                                    }

                                    #endregion
                                    break;
                                }
                            case "xxx":
                                {
                                    //var Map = Database.Server.ServerMaps[client.Player.Map];
                                    //using (var rec = new ServerSockets.RecycledPacket())
                                    //{
                                    //    var stream = rec.GetStream();

                                    //    client.Inventory.AddReturnedItem(stream, Database.ItemType.PowerExpBall, 4);
                                    //}
                                    break;
                                }
                            case "spm":
                                {
                                    var Map = Database.Server.ServerMaps[client.Player.Map];
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        Database.Server.AddMapMonster(stream, Map, uint.Parse(data[1]), client.Player.X, client.Player.Y, 1, 1, 1);
                                    }
                                    break;
                                }
                            case "hvb":
                                {
                                    client.Player.HeavenBlessing = int.Parse(data[1]);
                                    break;
                                }
                            case "npcsload":
                                {
                                    NpcServer.LoadNpcs();

                                    break;
                                }
                            case "reward":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        Role.Player.Reward(client, stream, "test");

                                    }
                                    break;
                                }
                            case "sinfo":
                                {
                                    DateTime now = DateTime.Now;
                                    TimeSpan t2 = new TimeSpan(Program.StartDate.ToBinary());
                                    TimeSpan t1 = new TimeSpan(now.ToBinary());
                                    client.SendSysMesage("The server has been online " + (int)(t1.TotalHours - t2.TotalHours) + " hours, " + (int)((t1.TotalMinutes - t2.TotalMinutes) % 60) + " minutes.");

                                    client.SendSysMesage("Online players count: " + Server.GamePoll.Count);

                                    var proc = System.Diagnostics.Process.GetCurrentProcess();
                                    client.SendSysMesage("Thread count: " + proc.Threads.Count);
                                    client.SendSysMesage("Memory set(MB): " + ((double)((double)proc.WorkingSet64 / 1024)) / 1024);
                                    proc.Close();

                                    client.SendSysMesage("QueuePackets: " + ServerSockets.PacketRecycle.Count);
                                    break;
                                }
                            case "addspawns":
                                {
                                    ushort mobid = ushort.Parse(data[1]);
                                    byte amount = byte.Parse(data[2]);
                                    byte radius = byte.Parse(data[3]);
                                    byte freq = byte.Parse(data[4]);

                                    ushort X = (ushort)(client.Player.X - radius / 2.0);
                                    ushort Y = (ushort)(client.Player.Y - radius / 2.0);

                                    if (!client.Map.ValidLocation(X, Y))
                                    {
                                        client.SendSysMesage("Invalid (X,Y)");
                                        break;
                                    }
                                    ushort BoundX = (ushort)(radius * 2), BoundY = (ushort)(radius * 2);
                                    var MapId = client.Player.Map;
                                    Game.MsgMonster.MobCollection colletion = new Game.MsgMonster.MobCollection(MapId);
                                    if (MapId == 8800)
                                    {

                                    }
                                    if (colletion.ReadMap())
                                    {

                                        colletion.LocationSpawn = "";
                                        Game.MsgMonster.MonsterFamily famil;
                                        if (!Server.MonsterFamilies.TryGetValue(mobid, out famil))
                                        {
                                            client.SendSysMesage("Invalid Monster Id");
                                            break;
                                        }
                                        if (Game.MsgMonster.MonsterRole.SpecialMonsters.Contains(famil.ID))
                                        {
                                            client.SendSysMesage("You can't add spawns for this boss.");
                                            break;
                                        }
                                        Game.MsgMonster.MonsterFamily Monster = famil.Copy();

                                        Monster.SpawnX = X;
                                        Monster.SpawnY = Y;
                                        Monster.MaxSpawnX = (ushort)(Monster.SpawnX + BoundX);
                                        Monster.MaxSpawnY = (ushort)(Monster.SpawnY + BoundY);
                                        Monster.MapID = MapId;
                                        Monster.SpawnCount = amount;//"maxnpc", 0);//max_per_gen", 0);
                                                                    //if (Monster.ID == 18)
                                                                    //    Monster.SpawnCount *= 2;
                                        Monster.rest_secs = freq;

                                        Monster.SpawnCount = amount;
                                        colletion.Add(Monster, false, 0, true);
                                        using (var stream = new StreamWriter(Program.ServerConfig.DbLocation + "\\Spawns.txt", true))
                                        {
                                            stream.WriteLine($"{mobid},{MapId},{X},{Y},{BoundX},{BoundY},{amount},{freq},{amount}");
                                            stream.Close(); ;
                                            client.SendSysMesage("Saved Spawn.");
                                        }
                                    }
                                    else
                                        client.SendSysMesage("Failed to make this spawn.");

                                    break;
                                }
                            case "steedu":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.Inventory.AddSteed(stream, 300000, 1, byte.Parse(data[4]), false, byte.Parse(data[1]), byte.Parse(data[2]), byte.Parse(data[3]));
                                    }
                                    break;
                                }
                            case "addguiitem":
                                {

                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var apacket = rec.GetStream();
                                        client.Inventory.AddReturnedItem(apacket, uint.Parse(data[1]), byte.Parse(data[2]));
                                    }
                                    //13369344

                                    //  for (int x = 0; x < 50; x++)
                                    /* {
                                         Game.MsgServer.MsgGameItem GameItem = new Game.MsgServer.MsgGameItem();
                                         GameItem.UID = 1000;// (uint)Program.GetRandom.Next();
                                         GameItem.Durability = GameItem.MaximDurability = 1000;
                                         GameItem.ITEM_ID = 721300;
                                         GameItem.Mode = (Role.Flags.ItemMode)uint.Parse(data[1]) ;//.AddItem; //8


                                         var containeritem = MsgDetainedItem.Create(GameItem);
                                         containeritem.Action = (MsgDetainedItem.ContainerType)(  13369344);// uint.Parse(data[1]);
                                         using (var rec = new ServerSockets.RecycledPacket())
                                         {
                                             var apacket = rec.GetStream();
                                             GameItem.Send(client, apacket);
                                           //  client.Send(apacket.DetainedItemCreate(containeritem));
                                         }
                                     }*/
                                    break;
                                }
                            case "addgui":
                                {
                                    ActionQuery action = new ActionQuery()
                                    {
                                        Type = ActionType.OpenDialog,
                                        ObjId = client.Player.UID,
                                        wParam1 = client.Player.X,
                                        wParam2 = client.Player.Y,
                                        dwParam = uint.Parse(data[1])//MsgServer.DialogCommands.JiangHuSetName,
                                    };
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var apacket = rec.GetStream();
                                        client.Send(apacket.ActionCreate(&action));
                                    }
                                    break;
                                }
                            case "ali":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();

                                        client.Send(stream.GuildRequestCreate((MsgGuildProces.GuildAction)ushort.Parse(data[1]), client.Player.UID, new int[3], "Basta"));
                                    }
                                    break;
                                }
                            case "bc":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        string message = string.Join(" ", data.Skip(1));
                                        Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(message, "ALLUSERS", MsgColor.red, ChatMode.Center).GetArray(stream));
                                        Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(message, "ALLUSERS", MsgColor.red, ChatMode.BroadcastMessage).GetArray(stream));
                                        Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(message, "ALLUSERS", MsgColor.red, ChatMode.System).GetArray(stream));
                                    }
                                    break;
                                }


                            case "invisible":
                                {
                                    client.Player.Invisible = true;
                                    break;
                                }
                            case "visible":
                                {
                                    client.Player.Invisible = false;
                                    break;
                                }
                            case "tour":
                                {
                                    Game.MsgTournaments.MsgSchedules.CurrentTournament = Game.MsgTournaments.MsgSchedules.Tournaments[(MsgTournaments.TournamentType)ushort.Parse(data[1])];
                                    Game.MsgTournaments.MsgSchedules.CurrentTournament.Open();
                                    break;
                                }
                            case "hit":
                                {
                                    client.Player.HitPoints = ushort.Parse(data[1]);
                                    client.Player.SendUpdateHP();
                                    break;

                                }
                            case "beginmatamata":
                                MsgSchedules.eventMataMata.StartMatch(client);
                                break;
                            case "startmatamata":
                                MsgSchedules.eventMataMata.StartEvent(client);
                                break;
                            case "moveall":
                                MsgSchedules.eventMataMata.MoveAllPlayers(client);
                                break;
                            case "dd":
                                {
                                    byte[] buf = new byte[]
                                    {
                            0x92 ,0xAE, 0x6D, 0x00,    0xCD ,0xFF,0x0A,0x00,0x01,0x00,0x01,0x00
,0x03,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x03,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
                                    };
                                    /*67 E0 A9 00 37 2D 13 00 0C 00 00 00      ;* 'gà© 7-    
        00 00 00 00 00 00 00 00 AB 00 07 00 BD 01 4D 00      ;        «  ½M 
        00 00 00 00 00 00 00 00 00 00*/
                                    ActionQuery action = new ActionQuery()
                                    {
                                        ObjId = client.Player.UID,
                                        Type = (ActionType)0xab,
                                        Fascing = 7,
                                        wParam1 = client.Player.X,
                                        wParam2 = client.Player.Y,
                                        dwParam = 0x0c,


                                    };

                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();

                                        unsafe
                                        {
                                            client.Send(stream.ActionCreate(&action));
                                        }

                                        /*stream.InitWriter();
                                        for (int x = 0; x < buf.Length; x++)
                                            stream.Write((byte)buf[x]);
                                        stream.Finalize(1008);
                                        client.Send(stream);*/
                                    }
                                    break;
                                }
                            case "jjj":
                                {
                                    /*  *((ushort*)(ptr)) = 38;
                        *((ushort*)(ptr + 2)) = 1070;
                        *((ushort*)(ptr + 4)) = 1;*/

                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        stream.InitWriter();
                                        stream.Write(uint.Parse(data[1]));
                                        stream.Write(uint.Parse(data[2]));//ushort.MaxValue);//ushort.Parse(data[2]));
                                        stream.Finalize(ushort.Parse(data[3]));
                                        Console.PrintPacketAdvanced(stream.Memory, stream.Size);
                                        client.Send(stream);
                                    }
                                    break;
                                }
                            case "data":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        ActionQuery action = new ActionQuery()
                                        {
                                            Type = ActionType.OpenDialog,
                                            ObjId = client.Player.UID,
                                            dwParam = uint.Parse(data[1]),
                                            wParam1 = client.Player.X,
                                            wParam2 = client.Player.Y,

                                        };
                                        client.Send(stream.ActionCreate(&action));
                                    }
                                    break;
                                }
                            case "invitegw":
                                {
                                    MsgSchedules.SendInvitation("GuildWar", 355, 337, 1002, 0, 60, MsgStaticMessage.Messages.GuildWar);
                                    break;
                                }
                            case "invitegw2":
                                {
                                    client.Player.MessageBox("", new Action<Client.GameClient>(
                                        p
                                        =>
                                        p.Teleport(355, 337, 1002, 0)
                                        ), null, 60, MsgServer.MsgStaticMessage.Messages.GuildWar); 
                                    break;
                                }
                            case "leave":
                                {
                                    if (Program.ArenaMaps.ContainsKey(client.Player.Map) || Program.ArenaMaps.ContainsKey(client.Player.DynamicID) || Program.ArenaMaps.ContainsKey((uint)client.Player.Betting))
                                    {
                                        foreach (var bot in Bots.BotProcessring.Bots.Values)
                                        {
                                            if (bot.Bot != null)
                                            {
                                                if (bot.Bot.Player.Map == client.Player.Map && bot.Bot.Player.DynamicID == client.Player.DynamicID)
                                                    bot.Dispose();
                                            }
                                        }
                                        if (client.Player.Betting > 0)
                                        {
                                            foreach (var player in client.Map.Values.Where(e => e.Player.DynamicID == client.Player.DynamicID))
                                            {
                                                client.SendSysMesage("You need to finish your match.", ChatMode.Talk);
                                                break;
                                            }

                                            client.Player.ConquerPoints += (uint)client.Player.Betting;
                                            client.Player.Betting = 0;
                                        }
                                        client.Teleport(428, 378, 1002);
                                    }
                                    break;
                                }
                            case "data1":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        ActionQuery action = new ActionQuery()
                                        {
                                            Type = ActionType.OpenCustom,
                                            ObjId = client.Player.UID,
                                            dwParam = uint.Parse(data[1]),
                                            wParam1 = client.Player.X,
                                            wParam2 = client.Player.Y,

                                        };
                                        client.Send(stream.ActionCreate(&action));
                                    }
                                    break;
                                }
                            case "clearspells":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        foreach (var spell in client.MySpells.ClientSpells.Values)
                                            client.MySpells.Remove(spell.ID, stream);
                                    }
                                    break;
                                }
                            case "clearprof":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        foreach (var spell in client.MyProfs.ClientProf.Values)
                                            client.MyProfs.Remove(spell.ID, stream);
                                    }
                                    break;
                                }
                            case "rr":
                                {
                                    client.Player.TCCaptainTimes = 0;
                                    break;
                                }
                            case "cards":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        stream.InitWriter();
                                        stream.Write((ushort)7);
                                        stream.Write((ushort)4);
                                        stream.ZeroFill(22);
                                        stream.Write((ushort)1);
                                        stream.Write(client.Player.UID);//starter or dealer?
                                        stream.Write(0);
                                        stream.Write(0);//??

                                        stream.Write(ushort.Parse(data[1]));
                                        stream.Write((ushort)1);
                                        stream.Write(client.Player.UID);


                                        stream.Finalize(GamePackets.PokerDrawCards);
                                        client.Send(stream);
                                    }
                                    break;
                                }

                            case "trans":
                                {

                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();

                                        //client.Player.Body = x;//ushort.Parse(data[1]);
                                        // client.Player.SendUpdate(stream, client.Player.Mesh, MsgUpdate.DataType.Mesh);
                                        client.Player.TransformInfo = new Role.ClientTransform(client.Player);
                                        client.Player.TransformInfo.CreateTransform(stream, 817, ushort.Parse(data[1]), (int)ushort.MaxValue - 1, 8213);
                                    }


                                    break;
                                }
                            case "pick":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.Player.AddPick(stream, "Pick", 5);
                                    }
                                    break;
                                }
                            case "addnpc":
                                {
                                    Game.MsgNpc.Npc np = Game.MsgNpc.Npc.Create();
                                    np.UID = (uint)Program.GetRandom.Next(10000, 100000);
                                    np.NpcType = (Role.Flags.NpcType)byte.Parse(data[1]);
                                    np.Mesh = ushort.Parse(data[2]);
                                    np.Map = client.Player.Map;//ushort.Parse(data[3]);
                                    np.X = client.Player.X;//ushort.Parse(data[4]);
                                    np.Y = client.Player.Y;//ushort.Parse(data[5]);
                                    client.Map.AddNpc(np);
                                    break;
                                }
                            case "itemstack":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.Inventory.AddItemWitchStack(uint.Parse(data[1]), byte.Parse(data[2]), ushort.Parse(data[3]), stream);
                                    }
                                    break;
                                }
                            case "itemeffect":
                                {
                                    MsgGameItem item;
                                    if (client.Equipment.TryGetEquip(Role.Flags.ConquerItem.RightWeapon, out item))
                                    {
                                        item.Effect = (Role.Flags.ItemEffect)ushort.Parse(data[1]);
                                        item.Mode = Role.Flags.ItemMode.Update;
                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            var stream = rec.GetStream();
                                            item.Send(client, stream);
                                        }
                                    }
                                    break;
                                }
                            case "dura":
                                {
                                    MsgServer.MsgGameItem GameItem;
                                    if (client.Equipment.TryGetEquip((Role.Flags.ConquerItem)byte.Parse(data[1]), out GameItem))
                                    {
                                        GameItem.Durability = GameItem.MaximDurability = ushort.Parse(data[2]);

                                        GameItem.Mode = Role.Flags.ItemMode.Update;
                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            var stream = rec.GetStream();
                                            GameItem.Send(client, stream);
                                        }
                                    }
                                    break;
                                }
                            case "realbp":
                                {
                                    client.SendSysMesage("You real BatterPower is = " + client.Player.RealBattlePower + "");
                                    break;
                                }
                            case "bp":
                                {
                                    client.SendSysMesage("You BatterPower is = " + client.Player.BattlePower + "");
                                    break;
                                }
                            case "newsuper":
                                {
                                    client.Status.MinAttack = uint.MaxValue - 10;
                                    client.Status.MaxAttack = uint.MaxValue;
                                    break;
                                }
                            case "superman":
                                {
                                    client.Player.Vitality += 500;
                                    client.Player.Strength += 500;
                                    client.Player.Spirit += 500;
                                    client.Player.Agility += 500;

                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.Player.SendUpdate(stream, client.Player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                                        client.Player.SendUpdate(stream, client.Player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                                        client.Player.SendUpdate(stream, client.Player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                                        client.Player.SendUpdate(stream, client.Player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);

                                    }
                                    break;
                                }
                            case "resetstats":
                                {
                                    client.Player.Vitality = 0;
                                    client.Player.Strength = 0;
                                    client.Player.Spirit = 0;
                                    client.Player.Agility = 0;

                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.Player.SendUpdate(stream, client.Player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                                        client.Player.SendUpdate(stream, client.Player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                                        client.Player.SendUpdate(stream, client.Player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                                        client.Player.SendUpdate(stream, client.Player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);

                                    }
                                    break;
                                }
                            case "addmem":
                                {
                                    for (int x = 0; x < ushort.Parse(data[1]); x++)
                                    {
                                        client.Player.MyGuild.Members.TryAdd((uint)(client.Player.UID + x + 1000)
                                            , new Role.Instance.Guild.Member() { Name = "test" + x.ToString() + " ", Level = (byte)x, IsOnline = true, UID = (uint)(client.Player.UID + x + 1000) });
                                    }
                                    break;
                                }
                            case "addmem2":
                                {
                                    for (int x = 0; x < ushort.Parse(data[1]); x++)
                                    {
                                        client.Player.MyGuild.Members.TryAdd((uint)(client.Player.UID + x + 1100)
                                            , new Role.Instance.Guild.Member() { Name = "test" + x.ToString() + " ", Level = (byte)x, IsOnline = false, UID = (uint)(client.Player.UID + x + 1100) });
                                    }
                                    break;
                                }
                            case "give":
                                {
                                    foreach (var user in Database.Server.GamePoll.Values)
                                    {
                                        if (user.Player.Name.ToLower() == data[1].ToLower() || data[1].ToLower() == "me")
                                        {

                                            switch (data[2])
                                            {
                                                case "spell":
                                                    {
                                                        ushort ID = 0;
                                                        if (!ushort.TryParse(data[3], out ID))
                                                        {
                                                            client.SendSysMesage("Invalid spell ID!");
                                                            break;
                                                        }
                                                        byte level = 0;
                                                        if (!byte.TryParse(data[4], out level))
                                                        {
                                                            client.SendSysMesage("Invalid spell level!");
                                                            break;
                                                        }

                                                        using (var rec = new ServerSockets.RecycledPacket())
                                                            user.MySpells.Add(rec.GetStream(), ID, level, 0, 0, 0);
                                                        break;
                                                    }
                                                case "level":
                                                    {
                                                        byte amount = 0;
                                                        if (byte.TryParse(data[3], out amount))
                                                        {
                                                            using (var rec = new ServerSockets.RecycledPacket())
                                                            {
                                                                var stream = rec.GetStream();
                                                                user.UpdateLevel(stream, amount, true);
                                                            }
                                                        }
                                                        break;
                                                    }
                                                case "money":
                                                    {
                                                        user.Player.Money += uint.Parse(data[3]);
                                                        using (var rec = new ServerSockets.RecycledPacket())
                                                        {
                                                            var stream = rec.GetStream();
                                                            user.Player.SendUpdate(stream, user.Player.Money, MsgUpdate.DataType.Money);
                                                        }
                                                        break;
                                                    }
                                                case "cps":
                                                    {
                                                        user.Player.ConquerPoints += uint.Parse(data[3]);

                                                        break;
                                                    }
                                                case "tessst":
                                                    {
                                                        using (var rec = new ServerSockets.RecycledPacket())
                                                        {
                                                            var stream = rec.GetStream();
                                                            client.Send(stream.ItemGarm(uint.Parse(data[1])));
                                                        }
                                                        break;
                                                    }
                                                case "reborns":
                                                    {
                                                        user.Player.Reborn = byte.Parse(data[3]);

                                                        break;
                                                    }

                                                case "item":
                                                    {
                                                        uint ID = 0;
                                                        if (!uint.TryParse(data[3], out ID))
                                                        {
                                                            client.SendSysMesage("Invalid item ID!");
                                                            break;
                                                        }
                                                        byte plus = 0;
                                                        if (!byte.TryParse(data[4], out plus))
                                                        {
                                                            client.SendSysMesage("Invalid item plus!");
                                                            break;
                                                        }
                                                        byte bless = 0;
                                                        if (!byte.TryParse(data[5], out bless))
                                                        {
                                                            client.SendSysMesage("Invalid item enchant!");
                                                            break;
                                                        }
                                                        byte enchant = 0;
                                                        if (!byte.TryParse(data[6], out enchant))
                                                        {
                                                            client.SendSysMesage("Invalid item enchant!");
                                                            break;
                                                        }
                                                        byte sockone = 0;
                                                        if (!byte.TryParse(data[7], out sockone))
                                                        {
                                                            client.SendSysMesage("Invalid item socket one!");
                                                            break;
                                                        }
                                                        byte socktwo = 0;
                                                        if (!byte.TryParse(data[8], out socktwo))
                                                        {
                                                            client.SendSysMesage("Invalid item socket two!");
                                                            break;
                                                        }
                                                        byte count = 1;
                                                        if (data.Length > 9)
                                                        {
                                                            if (!byte.TryParse(data[9], out count))
                                                            {
                                                                client.SendSysMesage("Invalid item count!");
                                                                break;
                                                            }
                                                        }
                                                        byte Effect = 0;
                                                        if (data.Length > 10)
                                                        {
                                                            if (!byte.TryParse(data[10], out Effect))
                                                            {
                                                                client.SendSysMesage("Invalid effect type!");
                                                                break;
                                                            }
                                                        }
                                                        using (var rec = new ServerSockets.RecycledPacket())
                                                            user.Inventory.Add(rec.GetStream(), ID, count, plus, bless, enchant, (Role.Flags.Gem)sockone, (Role.Flags.Gem)socktwo, false, (Role.Flags.ItemEffect)Effect, false, "", 3);

                                                        break;
                                                    }
                                            }
                                            break;
                                        }
                                    }
                                    break;
                                }
                            case "unbanstr"://remove banned
                                {
                                    Database.SystemBannedAccount.RemoveBan(data[1]);
                                    break;
                                }
                            case "unbanuid":
                                {
                                    Database.SystemBannedAccount.RemoveBan(uint.Parse(data[1]));
                                    break;
                                }
                            case "ban":
                                {
                                    foreach (var user in Database.Server.GamePoll.Values)
                                    {
                                        if (user.Player.Name.ToLower() == data[1].ToLower())
                                        {

                                            Database.SystemBannedAccount.AddBan(user.Player.UID, user.Player.Name, uint.Parse(data[2]));
                                            user.SendSysMesage("You Account was Banned by [PM]/[GM].", ChatMode.System, MsgColor.white);
                                            user.Socket.Disconnect();
                                            break;
                                        }
                                    }
                                    break;
                                }
                            case "banip":
                                {
                                    Console.WriteLine("banip");
                                    foreach (var user in Database.Server.GamePoll.Values)
                                    {
                                        if (user.Player.Name.ToLower() == data[1].ToLower())
                                        {
                                            Console.WriteLine($"banip - Found player ${data[1].ToLower()}");

                                            Database.SystemBanned.AddBan(user.Socket.RemoteIp, uint.Parse(data[2]), user.Player.Name);

                                            Console.WriteLine($"banip - Teste");

                                            user.SendSysMesage("You Ip Address was Banned by [PM]/[GM].", ChatMode.System, MsgColor.white);
                                            user.Socket.Disconnect();
                                            break;
                                        }
                                    }
                                    break;
                                }
                            //case "banpc":
                            //    {
                            //        foreach (var user in Database.Server.GamePoll.Values)
                            //        {
                            //            if (user.Player.Name.ToLower() == data[1].ToLower())
                            //            {
                            //                uint Days = 360;
                            //                if(data.Length > 1)
                            //                {
                            //                    Days = uint.Parse(data[2]) * 24;
                            //                }
                            //                Database.SystemBanned.PCAddBan(user.Player.Name, user.OnLogin.HWID, Days);
                            //                user.SendSysMesage("You PC was Banned by [PM]/[GM].", ChatMode.System, MsgColor.white);
                            //                user.Socket.Disconnect();
                            //                foreach (var player in Database.Server.GamePoll.Values)
                            //                {
                            //                    if (player.OnLogin.HWID == user.OnLogin.HWID)
                            //                    {
                            //                        player.Socket.Disconnect();
                            //                    }
                            //                }
                            //                string banmsg = "";
                            //                Database.SystemBanned.IsBanned(user.OnLogin.HWID, out banmsg, true);
                            //                foreach (var player in Database.Server.GamePoll.Values)
                            //                {
                            //                    player.SendSysMesage("Player " + data[1] + " " + banmsg + " by [PM]/[GM].", ChatMode.Center, MsgColor.red);
                            //                }
                            //                break;
                            //            }
                            //        }
                            //        break;
                            //    }
                            
                            case "kick":
                                {
                                    foreach (var user in Database.Server.GamePoll.Values)
                                    {
                                        if (user.Player.Name.ToLower() == data[1].ToLower())
                                        {
                                            user.Socket.Disconnect();
                                            break;
                                        }
                                    }
                                    break;
                                }
                            case "flags":
                                {
                                    client.Player.ClearFlags();
                                    break;
                                }
                            case "interact":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        InteractQuery action = new InteractQuery()
                                        {
                                            AtkType = (MsgAttackPacket.AttackID)uint.Parse(data[1]),
                                            ResponseDamage = uint.Parse(data[2]),
                                        };
                                        client.Player.View.SendView(stream.InteractionCreate(&action), true);
                                    }
                                    break;
                                }
                            case "rev":
                            case "revive":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                        client.Player.Revive(rec.GetStream());

                                    break;
                                }
                            case "estats":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();

                                        stream.InitWriter();

                                        stream.Write((uint)2);//4
                                        stream.Write((uint)1);//8

                                        stream.Write(1000004);//12
                                        stream.Write((uint)0);//16
                                        stream.Write((uint)0);//20
                                        stream.Write((uint)0);//24
                                        stream.Write((uint)0);//28
                                        stream.Write((uint)0);//32
                                        stream.Write((uint)0);//36
                                        stream.Write((uint)0);//40
                                                              //stream.Write((uint)0);//16
                                        stream.Write(370);//3
                                                          //stream.Write(0);//36

                                        //stream.Write((uint)0);
                                        stream.Finalize(GamePackets.ElitePKMatchUI);
                                        client.Send(stream);
                                    }
                                    break;
                                }

                            case "info":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();

                                        foreach (var user in Database.Server.GamePoll.Values)
                                        {
                                            if (user.Player.Name.ToLower() == data[1].ToLower())
                                            {

                                                client.Send(new MsgMessage("[Info" + user.Player.Name + "]", MsgColor.yellow, ChatMode.FirstRightCorner).GetArray(stream));
                                                client.Send(new MsgMessage("UID = " + user.Player.UID + " ", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                                client.Send(new MsgMessage("IP = " + user.Socket.RemoteIp + " ", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                                client.Send(new MsgMessage("ConquerPoints = " + user.Player.ConquerPoints + " ", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                                client.Send(new MsgMessage("Money = " + user.Player.Money + " ", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                                client.Send(new MsgMessage("Map = " + user.Player.Map + " ", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                                client.Send(new MsgMessage("X = " + user.Player.X + " ", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                                client.Send(new MsgMessage("Y = " + user.Player.Y + " ", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                                client.Send(new MsgMessage("BattlePower = " + user.Player.BattlePower + " ", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                                var list = MsgLoginClient.PlayersIP.Where(e => e.Key == user.IP).FirstOrDefault();
                                                client.Send(new MsgMessage("----- \n", MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                                foreach (var pl in list.Value)
                                                    client.Send(new MsgMessage(pl, MsgColor.yellow, ChatMode.ContinueRightCorner).GetArray(stream));
                                                break;
                                            }
                                        }
                                    }
                                    break;
                                }
                            case "scroll":
                                {
                                    switch (data[1].ToLower())
                                    {
                                        case "tc": client.Teleport(428, 378, 1002); break;
                                        case "pc": client.Teleport(195, 267, 1011); break;
                                        case "ap": client.Teleport(566, 563, 1020); break;
                                        case "dc": client.Teleport(493, 646, 1000); break;
                                        case "bi": client.Teleport(723, 573, 1015); break;
                                        case "ma": client.Teleport(211, 196, 1036); break;
                                    }
                                    break;
                                }


                            case "irate":
                                {
                                    foreach (var user in Database.Server.GamePoll.Values)
                                    {
                                        if (user.Player.Name.ToLower().Contains(data[1].ToLower()))
                                        {
                                            client.Teleport(user.Player.X, user.Player.Y, user.Player.Map, user.Player.DynamicID);
                                            break;
                                        }
                                    }

                                    break;
                                }
                            case "mover":
                                {
                                    foreach (var user in Database.Server.GamePoll.Values)
                                    {
                                        if (user.Player.Name.ToLower() == data[1].ToLower())
                                        {
                                            user.Teleport(client.Player.X, client.Player.Y, client.Player.Map);
                                            break;
                                        }
                                    }
                                    break;
                                }

                            case "life":
                                {
                                    client.Player.HitPoints = (int)client.Status.MaxHitpoints;
                                    client.Player.Mana = (ushort)client.Status.MaxMana;
                                    client.Player.SendUpdateHP();
                                    break;
                                }
                            case "onlinepoints":
                                {
                                    client.Player.OnlinePoints = uint.Parse(data[1]);

                                    break;
                                }
                            case "staticrole":
                                {
                                    var staticrole = new Role.StaticRole(client.Player.X, client.Player.Y);
                                    staticrole.Map = client.Player.Map;

                                    client.Map.AddStaticRole(staticrole);
                                    break;
                                }
                            case "facke":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();

                                        for (int i = 0; i < ushort.Parse(data[1]); i++)
                                        {
                                            Client.GameClient pclient = new Client.GameClient(null);
                                            pclient.Fake = true;

                                            pclient.Player = new Role.Player(pclient);
                                            pclient.Inventory = new Role.Instance.Inventory(pclient);
                                            pclient.Equipment = new Role.Instance.Equip(pclient);
                                            pclient.Warehouse = new Role.Instance.Warehouse(pclient);
                                            pclient.MyProfs = new Role.Instance.Proficiency(pclient);
                                            pclient.MySpells = new Role.Instance.Spell(pclient);
                                            pclient.Status = new MsgStatus();
                                            pclient.Player.Name = "[GM]Lords" + i;
                                            pclient.Player.Body = 1003;
                                            pclient.Player.UID = (uint)(1050000 + i * 10);
                                            pclient.Player.HitPoints = 1000;
                                            pclient.Status.MaxHitpoints = 1000;
                                            pclient.Player.AddFlag(MsgUpdate.Flags.TopTrojan, int.MaxValue, false);
                                            pclient.Player.X = (ushort)Program.GetRandom.Next(427, 500);
                                            pclient.Player.Y = (ushort)Program.GetRandom.Next(327, 390);
                                            pclient.Player.Map = 1002;
                                            pclient.Player.Level = 137;
                                            pclient.Player.ServerID = (ushort)Database.GroupServerList.MyServerInfo.ID;
                                            pclient.Player.Face = 64;
                                            pclient.Player.Action = Role.Flags.ConquerAction.Dance;
                                            pclient.Player.Angle = Role.Flags.ConquerAngle.SouthWest;
                                            pclient.Player.Hair = 164;
                                            pclient.Player.GarmentId = 191905;
                                            pclient.Player.LeftWeaponId = 410019;
                                            pclient.Player.RightWeaponId = 410019;
                                            client.Send(pclient.Player.GetArray(stream, false));

                                            pclient.Map = Database.Server.ServerMaps[1002];
                                            pclient.Map.Enquer(pclient);
                                            Database.Server.GamePoll.TryAdd(pclient.Player.UID, pclient);
                                        }
                                    }
                                    break;
                                }
                            case "dis":
                                {
                                    Game.MsgTournaments.MsgSchedules.DisCity.Open();
                                    break;
                                }
                            case "testmoob":
                                {
                                    if (client.Map.ContainMobID(uint.Parse(data[1])))
                                        break;
                                    using (var rec = new ServerSockets.RecycledPacket())
                                        Database.Server.AddMapMonster(rec.GetStream(), client.Map, uint.Parse(data[1]), client.Player.X, client.Player.Y, ushort.Parse(data[2]), ushort.Parse(data[3]), byte.Parse(data[4]));
                                    break;
                                }
                            case "ops":
                                {
                                    client.Player.OnlinePoints = uint.Parse(data[1]);
                                    break;
                                }
                            case "statue":
                                {
                                    //  Role.Statue.ElitePkStatue(client);
                                    Role.Statue.CreateStatue(client, client.Player.X, client.Player.Y, (int)client.Player.Action, 0, false);
                                    break;
                                }
                            case "max_attack":
                                {
                                    client.Status.Defence = uint.MaxValue;
                                    client.Status.MaxAttack = uint.Parse(data[1]);
                                    client.Status.MinAttack = uint.Parse(data[1]) - 1;
                                    break;
                                }
                            case "max_magic":
                                {
                                    client.Status.MagicAttack = uint.Parse(data[1]);
                                    client.Status.MagicPercent = uint.Parse(data[1]) - 1;
                                    break;
                                }
                            case "haire":
                                {
                                    client.Player.Hair = ushort.Parse(data[1]); using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.Player.SendUpdate(stream, client.Player.Hair, MsgUpdate.DataType.HairStyle);
                                    }
                                    break;
                                }
                            case "mapstat":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.Send(stream.MapStatusCreate(client.Map.ID, client.Map.ID, (ulong)(1U << int.Parse(data[1]))));
                                    }
                                    break;
                                }
                            case "pkp":
                                {
                                    client.Player.PKPoints = ushort.Parse(data[1]);
                                    break;
                                }

                            case "searchguard":
                                {
                                    foreach (var mob in client.Map.View.GetAllMapRoles(Role.MapObjectType.Monster))
                                    {
                                        if (mob.X == client.Player.X && mob.Y == client.Player.Y)
                                        {
                                            client.SendSysMesage("Location Spawn --> " + (mob as Game.MsgMonster.MonsterRole).LocationSpawn, ChatMode.System, MsgColor.red);
                                        }
                                    }
                                    break;
                                }

                            case "sound":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.Player.SendString(stream, MsgStringPacket.StringID.Sound, false, "sound/wind.wav", "1");

                                    }
                                    break;
                                }
                            case "sound2":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.Player.SendString(stream, MsgStringPacket.StringID.Sound, false, "sound/fc2.wav", "1");

                                    }
                                    break;
                                }
                            case "attacknormal":
                                {
                                    foreach (var monster in client.Player.View.Roles(Role.MapObjectType.Monster))
                                    {


                                        //for (int x = 50; x < 60; x++)
                                        {

                                            InteractQuery attack = new InteractQuery();
                                            attack.UID = monster.UID;
                                            attack.AtkType = (MsgAttackPacket.AttackID)ushort.Parse(data[1]);
                                            // attack.SpellID = 12070;
                                            attack.Damage = 1;
                                            attack.OpponentUID = client.Player.UID;
                                            attack.X = client.Player.X;
                                            attack.Y = client.Player.Y;
                                            // attack.ResponseDamage = 12070;
                                            using (var rec = new ServerSockets.RecycledPacket())
                                            {
                                                var stream = rec.GetStream();
                                                client.Player.View.SendView(stream.InteractionCreate(&attack), true);
                                            }
                                        }
                                        break;
                                    }
                                    break;
                                }
                            case "ef":
                                {
                                    Game.MsgServer.MsgMovement.eeffect = int.Parse(data[1]);
                                    break;
                                }
                            case "teleback":
                                {
                                    client.TeleportCallBack();
                                    break;
                                }
                            case "map":
                                {
                                    client.SendSysMesage("MapID = " + client.Player.Map, ChatMode.System);
                                    break;
                                }
                            case "expball":
                                {
                                    client.GainExpBall(double.Parse(data[1]), true, Role.Flags.ExperienceEffect.angelwing);
                                    break;
                                }
                            case "string_effect":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.Player.SendString(stream, MsgStringPacket.StringID.Effect, true, data[1]);
                                    }
                                    break;
                                }
                            case "string_effect3":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        // for (int x = 0; x < 100; x++)
                                        {
                                            Game.MsgServer.MsgStringPacket packet = new Game.MsgServer.MsgStringPacket();
                                            packet.ID = (MsgStringPacket.StringID)ushort.Parse(data[1]);
                                            packet.X = ushort.Parse(data[2]);
                                            packet.Y = ushort.Parse(data[3]);
                                            packet.Strings = new string[1] { "movego" }; ;
                                            client.Send(stream.StringPacketCreate(packet));
                                        }
                                    }
                                    break;
                                }
                            case "string_effect2":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        Game.MsgNpc.Npc npc = new MsgNpc.Npc();

                                        stream.InitWriter();

                                        stream.Write(1);
                                        stream.Write(0);
                                        stream.Write(0);
                                        stream.Write(0);
                                        stream.Write(ushort.Parse(data[1]));
                                        stream.Write(ushort.Parse(data[2]));
                                        stream.Write((ushort)385);
                                        stream.Write((ushort)26);
                                        stream.Write((uint)0);
                                        stream.Write((uint)0);
                                        stream.Write((uint)0);
                                        stream.Write(" ");
                                        stream.Finalize(Game.GamePackets.SobNpcs);

                                        client.Send(stream);
                                        client.Player.SendString(stream, MsgStringPacket.StringID.Effect, 1, true, data[3]);

                                        /*System.Threading.Thread.Sleep(3000);
                                        var action = new ActionQuery()
                                        {
                                             Type = ActionType.RemoveEntity,
                                             ObjId =1
                                        };
                                       client.Send(stream.ActionCreate(&action));*/


                                    }
                                    break;
                                }
                            case "gh":
                                {
                                    byte[] pp = new byte[]
                                    {
                                0x02,0x00,0x00,0x00
,0x19,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x20,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x14,0x00,0x00,0x00
,0x2E,0x00,0x00,0x00,0x35,0x00,0x00,0x00,0x03,0x00,0x00,0x00,0x28,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
                                    };
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        stream.InitWriter();
                                        stream.Write(Environment.TickCount);
                                        stream.Write(client.Player.UID);
                                        for (int x = 0; x < pp.Length; x++)
                                            stream.Write((byte)pp[x]);
                                        stream.Finalize(10017);
                                        client.Send(stream);
                                    }
                                    byte[] pp2 = new byte[]
                                    {
                                0x01,0x00,0x00,0x00,0x35,0x00,0x00,0x00
,0x00,0x01,0x00,0x00,0x28,0x00,0x00,0x00,0x03,0x00,0x00,0x00,0,0,0,0
                                    };
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        stream.InitWriter();

                                        stream.Write(client.Player.UID);
                                        for (int x = 0; x < pp2.Length; x++)
                                            stream.Write((byte)pp2[x]);
                                        stream.Finalize(2075);
                                        client.Send(stream);
                                    }
                                    break;
                                }
                            case "xp":
                                {
                                    client.Player.AddFlag(MsgUpdate.Flags.XPList, 20, true);
                                    break;
                                }
                            case "addflag":
                                {
                                    client.Player.AddFlag((MsgUpdate.Flags)int.Parse(data[1]), 10, true, 0, 50, 39);
                                    break;
                                }
                            case "remflag":
                                {
                                    client.Player.RemoveFlag((MsgUpdate.Flags)int.Parse(data[1]));
                                    break;
                                }

                            case "level":
                                {
                                    byte amount = 0;
                                    if (byte.TryParse(data[1], out amount))
                                    {
                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            var stream = rec.GetStream();
                                            client.UpdateLevel(stream, amount, true);
                                        }
                                    }

                                    break;
                                }
                            case "incexp":
                                {
                                    uint exp;
                                    if (uint.TryParse(data[1], out exp))
                                    {
                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            var stream = rec.GetStream();
                                            client.IncreaseExperience(stream, exp);
                                        }
                                    }
                                    break;
                                }
                            case "money":
                                {
                                    long amount = 0;
                                    if (long.TryParse(data[1], out amount))
                                    {
                                        client.Player.Money = (uint)amount;
                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            var stream = rec.GetStream();
                                            client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);
                                        }
                                    }
                                    break;
                                }
                            case "do":
                                {
                                    int n = int.Parse(data[1]);
                                    string rest = Message.Substring(3 + data[1].Length + 1);
                                    for (int i = 0; i < n; i++)
                                        ChatCommands(client, new MsgMessage(rest, MsgColor.red, msg.ChatType));
                                    break;
                                }
                            case "couplespk":
                                {
                                    MsgSchedules.CouplesPKWar.Open();
                                    break;
                                }
                            case "test123":
                                {
                                    client.Player.AddFlag((MsgUpdate.Flags)byte.Parse(data[1]), Role.StatusFlagsBigVector32.PermanentFlag, false);
                                    break;
                                }
                            case "lotteryreset":
                                {
                                    client.Player.LotteryEntries = 0;
                                    break;
                                }
                            case "cps":
                                {
                                    uint amount = 0;
                                    if (uint.TryParse(data[1], out amount))
                                    {
                                        client.Player.ConquerPoints = amount;

                                    }
                                    break;
                                }
                            case "tessst":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.Send(stream.ItemGarm(181355));
                                    }
                                    break;
                                }
                            case "flame":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        var Map = Database.Server.ServerMaps[client.Player.Map];
                                        Database.Server.AddMapMonster(stream, Map, 4145, client.Player.X, client.Player.Y, 1, 1, 1);
                                    }
                                    break;
                                }
                            case "stats":
                                {
                                    client.SendSysMesage("Cps hunted since restart : " + string.Format("{0:n0}", Program.CPsHuntedSinceRestart));
                                    client.SendSysMesage("ExpBalls hunted since restart : " + string.Format("{0:n0}", Program.ExpBallsDropped));
                                    client.SendSysMesage("+8 Elite: " + string.Format("{0:n0}", Program.Plus8));
                                    client.SendSysMesage("Super1Sock: " + string.Format("{0:n0}", Program.Super1Soc));
                                    client.SendSysMesage("Super2Sock: " + string.Format("{0:n0}", Program.Super2Soc));
                                    client.SendSysMesage("SuperNoSock: " + string.Format("{0:n0}", Program.SuperNoSoc));
                                    break;
                                }
                            case "boundcps":
                                {
                                    int amount = 0;
                                    if (int.TryParse(data[1], out amount))
                                    {
                                        client.Player.BoundConquerPoints = amount;

                                    }
                                    break;
                                }
                            case "presentflag":
                                {
                                    Console.WriteLine("");
                                    Console.WriteLine(client.Map.cells[client.Player.X, client.Player.Y].ToString());
                                    break;
                                }
                            case "remspell":
                                {
                                    ushort ID = 0;
                                    if (!ushort.TryParse(data[1], out ID))
                                    {
                                        client.SendSysMesage("Invlid spell ID !");
                                        break;
                                    }
                                    using (var rec = new ServerSockets.RecycledPacket())
                                        client.MySpells.Remove(ID, rec.GetStream());
                                    break;
                                }
                            case "spell":
                                {
                                    ushort ID = 0;
                                    if (!ushort.TryParse(data[1], out ID))
                                    {
                                        client.SendSysMesage("Invlid spell ID !");
                                        break;
                                    }
                                    byte level = 0;
                                    if (!byte.TryParse(data[2], out level))
                                    {
                                        client.SendSysMesage("Invlid spell Level ! ");
                                        break;
                                    }
                                    byte levelHu = 0;
                                    if (data.Length >= 3)
                                    {
                                        if (!byte.TryParse(data[3], out levelHu))
                                        {
                                            client.SendSysMesage("Invlid spell Level Souls ! ");
                                            break;
                                        }
                                    }
                                    int Experience = 0;
                                    if (!int.TryParse(data[4], out Experience))
                                    {
                                        client.SendSysMesage("Invlid spell Experience ! ");
                                        break;
                                    }

                                    using (var rec = new ServerSockets.RecycledPacket())
                                        client.MySpells.Add(rec.GetStream(), ID, level, levelHu, 0, Experience);
                                    break;
                                }
                            case "prof":
                                {
                                    ushort ID = 0;
                                    if (!ushort.TryParse(data[1], out ID))
                                    {
                                        client.SendSysMesage("Invlid prof ID !");
                                        break;
                                    }
                                    byte level = 0;
                                    if (!byte.TryParse(data[2], out level))
                                    {
                                        client.SendSysMesage("Invlid prof Level ! ");
                                        break;
                                    }
                                    uint Experience = 0;
                                    if (!uint.TryParse(data[3], out Experience))
                                    {
                                        client.SendSysMesage("Invlid prof Experience ! ");
                                        break;
                                    }
                                    using (var rec = new ServerSockets.RecycledPacket())
                                        client.MyProfs.Add(rec.GetStream(), ID, level, Experience);
                                    break;
                                }
                            case "clear":
                            case "clearinventory":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                        client.Inventory.Clear(rec.GetStream());
                                    break;
                                }
                            case "semper":
                                {
                                    client.SendSysMesage("Welcome to OrigensCO!", ChatMode.TopLeftSystem);
                                    break;
                                }

                                //mapa para o matamata

                            case "matamata":
                                {
                                    client.Teleport(200, 200, 1601);
                                    client.Player.MessageBox("Welcome back to Fifty Shades of Semper ;)", null, null);
                                    break;
                                }
                            //case "arenaduel":
                            //    {
                            //        MsgSchedules._ArenaDuel.Open();
                            //        break;
                            //    }
                            case "tele":
                                {
                                    client.TerainMask = 0;
                                    uint mapid = 0;
                                    if (!uint.TryParse(data[1], out mapid))
                                    {
                                        client.SendSysMesage("Invalid Map ID!");
                                        break;
                                    }
                                    ushort X = 0;
                                    if (!ushort.TryParse(data[2], out X))
                                    {
                                        client.SendSysMesage("Invalid X!");
                                        break;
                                    }
                                    ushort Y = 0;
                                    if (!ushort.TryParse(data[3], out Y))
                                    {
                                        client.SendSysMesage("Invalid Y!");
                                        break;
                                    }
                                    uint DinamicID = 0;
                                    if (!uint.TryParse(data[4], out DinamicID))
                                    {
                                        client.SendSysMesage("Invalid DinamicID!");
                                        break;
                                    }

                                    // Verifica se o jogador possui flags específicas ou está em um mapa específico
                                    if (client.Player.ContainFlag(MsgUpdate.Flags.FlashingName) || client.Player.ContainFlag(MsgUpdate.Flags.BlackName))
                                        break;
                                    if (client.Player.Map == 6000 || client.Player.DynamicID != 0 || client.Player.Alive == false)
                                        return false;

                                    client.Teleport(X, Y, mapid, DinamicID);
                                    break;

                                    // Verifica se o jogador possui flags específicas ou está em um mapa específico
                                    if (client.Player.ContainFlag(MsgUpdate.Flags.FlashingName) || client.Player.ContainFlag(MsgUpdate.Flags.BlackName))
                                        break;
                                    if (client.Player.Map == 6000 || client.Player.DynamicID != 0 || client.Player.Alive == false)
                                        return false;

                                    client.Teleport(X, Y, mapid, DinamicID);
                                    break;
                                }

                            case "effectfloor":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        //   for (uint x = 700; x < 750; x++)
                                        {
                                            MsgServer.MsgGameItem item = new MsgServer.MsgGameItem();
                                            item.Color = (Role.Flags.Color)2;
                                            item.ITEM_ID = uint.Parse(data[1]);//1182;
                                            MsgFloorItem.MsgItem DropItem = new MsgFloorItem.MsgItem(item, client.Player.X, client.Player.Y, MsgFloorItem.MsgItem.ItemType.Effect, 0, 0, client.Player.Map
                                                   , 0, false, client.Map, 4);

                                            if (client.Map.EnqueueItem(DropItem))
                                                DropItem.SendAll(stream, MsgFloorItem.MsgDropID.Effect);
                                        }
                                    }
                                    break;
                                }
                            case "tele2":
                                {
                                    uint mapid = 0;
                                    if (!uint.TryParse(data[1], out mapid))
                                    {
                                        client.SendSysMesage("Invalid Map ID!");
                                        break;
                                    }
                                    ushort X = 0;
                                    if (!ushort.TryParse(data[2], out X))
                                    {
                                        client.SendSysMesage("Invalid X!");
                                        break;
                                    }
                                    ushort Y = 0;
                                    if (!ushort.TryParse(data[3], out Y))
                                    {
                                        client.SendSysMesage("Invalid Y!");
                                        break;
                                    }
                                    foreach (var map in Database.Server.ServerMaps.Values)
                                    {
                                        mapid = map.ID;
                                        Console.WriteLine(map.ID);
                                        ActionQuery action = new ActionQuery()
                                        {
                                            ObjId = client.Player.UID,
                                            Type = ActionType.Teleport,
                                            dwParam = mapid,
                                            wParam1 = X,
                                            wParam2 = Y,
                                            dwParam3 = mapid
                                        };
                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            client.Send(rec.GetStream().ActionCreate(&action));
                                            client.Send(rec.GetStream().MapStatusCreate(mapid, mapid, 8));
                                        }
                                        System.Threading.Thread.Sleep(1000);
                                    }
                                    break;
                                }
                            case "loadpackets":
                                {
                                    Database.ServerDatabase.LoadDBPackets();

                                    break;
                                }
                            case "sendpackets":
                                {
                                    int ax = 0;
                                    foreach (var packet in Program.LoadPackets)
                                    {
                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            var stream = rec.GetStream();
                                            ushort PacketID = BitConverter.ToUInt16(packet, 2);
                                            /* if (PacketID == 10017)
                                             {
                                                 stream.InitWriter();
                                                 stream.Write(Environment.TickCount);
                                                 stream.Write(client.Player.UID);
                                                 for (int x = 12; x < packet.Length - 8; x++)
                                                 {
                                                     stream.Write((byte)packet[x]);
                                                 }
                                                 stream.Finalize(10017);
                                                 client.Send(stream);
                                                 MyConsole.PrintPacketAdvanced(stream.Memory, stream.Size);
                                             }*/
                                            /*     stream.InitWriter();
                                                 for (int x = 4; x < packet.Length - 4 - 8; x++)
                                                 {
                                                     stream.Write((byte)packet[x]);
                                                 }
                                                 stream.Finalize(PacketID);

                                                 client.Send(stream);
                                                 */
                                            if (PacketID == 1101 || PacketID == 1105)
                                            {
                                                if (PacketID == 1105)
                                                {

                                                    stream.InitWriter();
                                                    for (int x = 4; x < packet.Length - 4 - 8; x++)
                                                    {
                                                        stream.Write((byte)packet[x]);
                                                    }
                                                    stream.Finalize(PacketID);

                                                    int size = stream.Size;
                                                    stream.Seek(12);
                                                    ushort SpelliD = stream.ReadUInt16();

                                                    stream.Seek(stream.Size);
                                                    if (SpelliD == 12990)
                                                    {
                                                        if (ax == 0)
                                                        {
                                                            ax += 1;
                                                            continue;
                                                        }
                                                        ax++;
                                                        client.Send(stream);
                                                        System.Threading.Thread.Sleep(200);
                                                        Console.PrintPacketAdvanced(stream.Memory, stream.Size);
                                                        Console.WriteLine(PacketID);
                                                    }
                                                }
                                                else
                                                {
                                                    if (PacketID == 1101)
                                                    {

                                                        //1530
                                                        stream.InitWriter();
                                                        for (int x = 4; x < packet.Length - 4 - 8; x++)
                                                        {
                                                            stream.Write((byte)packet[x]);
                                                        }
                                                        stream.Finalize(PacketID);

                                                        int size = stream.Size;
                                                        stream.Seek(12);
                                                        ushort SpelliD = stream.ReadUInt16();
                                                        stream.Seek(stream.Size);
                                                        if (SpelliD == 1530)
                                                        {

                                                            ax++;
                                                            if (ax == 4)
                                                                continue;
                                                            client.Send(stream);
                                                            System.Threading.Thread.Sleep(200);
                                                            Console.PrintPacketAdvanced(stream.Memory, stream.Size);
                                                            Console.WriteLine(PacketID);
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                    }
                                    break;
                                }
                            case "itemm":
                                {
                                    uint ID = 0;
                                    if (!uint.TryParse(data[1], out ID))
                                    {
                                        client.SendSysMesage("Invlid item ID !");
                                        break;
                                    }
                                    using (var rec = new ServerSockets.RecycledPacket())
                                        client.Inventory.AddItemWitchStack(ID, 0, 10, rec.GetStream(), false);

                                    break;
                                }
                            case "item2":
                                {
                                    uint ID = 0;
                                    if (!uint.TryParse(data[1], out ID))
                                    {
                                        client.SendSysMesage("Invlid item ID !");
                                        break;
                                    }
                                    using (var rec = new ServerSockets.RecycledPacket())
                                        client.Inventory.Add(rec.GetStream(), ID, 0, 0, 0, 0, (Role.Flags.Gem)0, (Role.Flags.Gem)0, false, (Role.Flags.ItemEffect)0);

                                    break;
                                }
                            case "itemtime":
                                {
                                    uint ID = 0;
                                    if (!uint.TryParse(data[1], out ID))
                                    {
                                        client.SendSysMesage("Invlid item ID !");
                                        break;
                                    }
                                    byte plus = 0;
                                    if (!byte.TryParse(data[2], out plus))
                                    {
                                        client.SendSysMesage("Invlid item plus !");
                                        break;
                                    }
                                    byte bless = 0;
                                    if (!byte.TryParse(data[3], out bless))
                                    {
                                        client.SendSysMesage("Invlid item Enchant !");
                                        break;
                                    }
                                    byte enchant = 0;
                                    if (!byte.TryParse(data[4], out enchant))
                                    {
                                        client.SendSysMesage("Invlid item Enchant !");
                                        break;
                                    }
                                    byte sockone = 0;
                                    if (!byte.TryParse(data[5], out sockone))
                                    {
                                        client.SendSysMesage("Invlid item Socket One !");
                                        break;
                                    }
                                    byte socktwo = 0;
                                    if (!byte.TryParse(data[6], out socktwo))
                                    {
                                        client.SendSysMesage("Invlid item Socket Two !");
                                        break;
                                    }
                                    byte count = 1;
                                    if (data.Length > 7)
                                    {
                                        if (!byte.TryParse(data[7], out count))
                                        {
                                            client.SendSysMesage("Invlid item count !");
                                            break;
                                        }
                                    }
                                    byte Effect = 0;
                                    if (data.Length > 8)
                                    {
                                        if (!byte.TryParse(data[8], out Effect))
                                        {
                                            client.SendSysMesage("Invlid Effect Type !");
                                            break;
                                        }
                                    }
                                    byte days = 1;

                                    if (data.Length > 9)
                                    {
                                        if (!byte.TryParse(data[9], out days))
                                        {
                                            client.SendSysMesage("Invlid Days Type !");
                                            break;
                                        }
                                    }
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        client.Inventory.AddItemTime(rec.GetStream(), ID, count, plus, bless, enchant, (Role.Flags.Gem)sockone, (Role.Flags.Gem)socktwo, false, (Role.Flags.ItemEffect)Effect, false, "", days, 0, 0);
                                    }
                                    break;
                                }
                            case "item":
                                {
                                    uint ID = 0;
                                    if (!uint.TryParse(data[1], out ID))
                                    {
                                        client.SendSysMesage("Invlid item ID !");
                                        break;
                                    }
                                    byte plus = 0;
                                    if (!byte.TryParse(data[2], out plus))
                                    {
                                        client.SendSysMesage("Invlid item plus !");
                                        break;
                                    }
                                    byte bless = 0;
                                    if (!byte.TryParse(data[3], out bless))
                                    {
                                        client.SendSysMesage("Invlid item Enchant !");
                                        break;
                                    }
                                    byte enchant = 0;
                                    if (!byte.TryParse(data[4], out enchant))
                                    {
                                        client.SendSysMesage("Invlid item Enchant !");
                                        break;
                                    }
                                    byte sockone = 0;
                                    if (!byte.TryParse(data[5], out sockone))
                                    {
                                        client.SendSysMesage("Invlid item Socket One !");
                                        break;
                                    }
                                    byte socktwo = 0;
                                    if (!byte.TryParse(data[6], out socktwo))
                                    {
                                        client.SendSysMesage("Invlid item Socket Two !");
                                        break;
                                    }
                                    byte count = 1;
                                    if (data.Length > 7)
                                    {
                                        if (!byte.TryParse(data[7], out count))
                                        {
                                            client.SendSysMesage("Invlid item count !");
                                            break;
                                        }
                                    }
                                    byte Effect = 0;
                                    if (data.Length > 8)
                                    {
                                        if (!byte.TryParse(data[8], out Effect))
                                        {
                                            client.SendSysMesage("Invlid Effect Type !");
                                            break;
                                        }
                                    }
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        client.Inventory.Add(rec.GetStream(), ID, count, plus, bless, enchant, (Role.Flags.Gem)sockone, (Role.Flags.Gem)socktwo, false, (Role.Flags.ItemEffect)Effect);
                                    }
                                    break;
                                }
                            case "bcps":
                                {
                                    client.Player.BoundConquerPoints = int.Parse(data[1]);
                                    break;
                                }
                            case "pkwar":
                                {
                                    MsgTournaments.MsgSchedules.PkWar.Open();
                                    break;
                                }
                            case "additemstack":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.Inventory.AddItemWitchStack(uint.Parse(data[1]), 0, ushort.Parse(data[2]), stream);
                                    }
                                    break;
                                }
                            case "remitemstack":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.Inventory.RemoveStackItem(uint.Parse(data[1]), ushort.Parse(data[2]), stream);
                                    }
                                    break;
                                }
                            case "fftest":
                                {
                                    ushort x1 = ushort.Parse(data[1]);
                                    ushort y1 = ushort.Parse(data[2]);
                                    ushort x2 = ushort.Parse(data[3]);
                                    ushort y2 = ushort.Parse(data[4]);
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        byte[] pp = new byte[]
                                {
                            0xE3,0xBC,0x82,0x00,0xFC,0xBB,0x0D,0x00,0xFA,0x05,0x00,0x00
,0x3C,0x01,0x9A,0x01,0x00,0x00,0x0A,0x00,0x00,0x00,0x00,0x0E,0x89,0x45,0x0F,0x00
,0x02,0x00,0x00,0x00,0x03,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x3B,0x01,0x9B
,0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
                                };

                                        int size = stream.Size;

                                        stream.InitWriter();
                                        for (int x = 0; x < pp.Length; x++)
                                            stream.Write((byte)pp[x]);


                                        stream.Finalize(1101);

                                        stream.Seek(16);
                                        stream.Write(x1);
                                        stream.Write(y1);
                                        stream.Seek(60);
                                        stream.Write(x2);
                                        stream.Write(y2);

                                        stream.Seek(size);

                                        client.Send(stream);


                                        pp = new byte[]
                                  {
                              0xFC,0xBB,0x0D,0x00,0x3B,0x01,0x9B,0x01,0xBE,0x32,0x00,0x00
,0x00,0x00,0x00,0x00,0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00

                                  };

                                        stream.InitWriter();
                                        for (int x = 0; x < pp.Length; x++)
                                            stream.Write((byte)pp[x]);

                                        size = stream.Size;
                                        stream.Seek(8);
                                        stream.Write(x2);
                                        stream.Write(y2);

                                        stream.Seek(size);
                                        stream.Finalize(1105);
                                        client.Send(stream);

                                        pp = new byte[]
                                        {
                                    0x21,0xC2,0x82,0x00,0xFC,0xBB,0x0D,0x00,0xFA,0x05,0x00,0x00
,0x3C,0x01,0x9A,0x01,0x00,0x00,0x0C,0x00,0x00,0x00,0x00,0x0E,0x89,0x45,0x0F,0x00
,0x02,0x00,0x00,0x00,0x03,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x3B,0x01,0x9B
,0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00

                                        };
                                        stream.InitWriter();
                                        for (int x = 0; x < pp.Length; x++)
                                            stream.Write((byte)pp[x]);


                                        stream.Finalize(1101);
                                        size = stream.Size;
                                        stream.Seek(16);
                                        stream.Write(x1);
                                        stream.Write(y1);
                                        stream.Seek(60);
                                        stream.Write(x2);
                                        stream.Write(y2);

                                        stream.Seek(size);

                                        client.Send(stream);
                                    }
                                    break;
                                }
                            case "atest":
                                {

                                    break;
                                }
                            case "ftest":
                                {
                                    int size = 0;

                                    byte[] pp = new byte[]
                                    {
                         0x89,0x45,0x0F,0x00,0x3C,0x01,0x9A,0x01,0xBE,0x32,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00
                                    };

                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();


                                        stream.InitWriter();
                                        for (int x = 0; x < pp.Length; x++)
                                            stream.Write((byte)pp[x]);


                                        stream.Finalize(1105);
                                        size = stream.Size;
                                        stream.Seek(4);
                                        stream.Write(client.Player.UID);
                                        stream.Write((ushort)(client.Player.X - 1));
                                        stream.Write((ushort)(client.Player.Y - 1));
                                        stream.Seek(size);


                                        client.Send(stream);




                                        pp = new byte[]
                                        {
                                   0xE3,0xBC,0x82,0x00,0xFC,0xBB,0x0D,0x00,0xFA,0x05,0x00,0x00
,0x3C,0x01,0x9A,0x01,0x00,0x00,0x0A,0x00,0x00,0x00,0x00,0x0E,0x89,0x45,0x0F,0x00
,0x02,0x00,0x00,0x00,0x03,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x3B,0x01,0x9B
,0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
                                        };

                                        stream.InitWriter();
                                        for (int x = 0; x < pp.Length; x++)
                                            stream.Write((byte)pp[x]);
                                        stream.Finalize(1101);
                                        size = stream.Size;
                                        stream.Seek(28);
                                        stream.Write(client.Player.UID);
                                        stream.Seek(16);
                                        stream.Write((ushort)(client.Player.X - 1));
                                        stream.Write((ushort)(client.Player.Y - 1));

                                        stream.Seek(size);
                                        client.Send(stream);



                                        pp = new byte[]
                                        {
                                 0xFC,0xBB,0x0D,0x00,0x3B,0x01,0x9B,0x01,0xBE,0x32,0x00,0x00
,0x00,0x00,0x00,0x00,0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00
                                        };
                                        stream.InitWriter();
                                        for (int x = 0; x < pp.Length; x++)
                                            stream.Write((byte)pp[x]);
                                        stream.Finalize(1105);
                                        size = stream.Size;
                                        stream.Seek(4);
                                        stream.Write(client.Player.UID);
                                        stream.Write((ushort)(client.Player.X - 1));
                                        stream.Write((ushort)(client.Player.Y - 1));
                                        stream.Seek(size);


                                        client.Send(stream);
                                        pp = new byte[]
                                        {
                                  0x21,0xC2,0x82,0x00,0xFC,0xBB,0x0D,0x00,0xFA,0x05,0x00,0x00
,0x3C,0x01,0x9A,0x01,0x00,0x00,0x0C,0x00,0x00,0x00,0x00,0x0E,0x89,0x45,0x0F,0x00
,0x02,0x00,0x00,0x00,0x03,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x3B,0x01,0x9B
,0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
                                        };


                                        stream.InitWriter();
                                        for (int x = 0; x < pp.Length; x++)
                                            stream.Write((byte)pp[x]);
                                        stream.Finalize(1101);
                                        size = stream.Size;
                                        stream.Seek(28);
                                        stream.Write(client.Player.UID);
                                        stream.Seek(16);
                                        stream.Write((ushort)(client.Player.X - 1));
                                        stream.Write((ushort)(client.Player.Y - 1));
                                        stream.Seek(size);
                                        client.Send(stream);


                                    }
                                    break;
                                }
                            case "floor":
                                {
                                    Game.MsgFloorItem.MsgItemPacket FloorPacket = Game.MsgFloorItem.MsgItemPacket.Create();
                                    FloorPacket.m_UID = Game.MsgFloorItem.MsgItem.UIDS.Next;
                                    FloorPacket.m_ID = uint.Parse(data[1]);
                                    FloorPacket.m_X = client.Player.X;
                                    FloorPacket.m_Y = client.Player.Y;
                                    FloorPacket.Timer = Role.Core.TqTimer(DateTime.Now.AddSeconds(4));
                                    FloorPacket.m_Color = (byte)14;//4;
                                    FloorPacket.m_Color2 = (byte)14;//14
                                    FloorPacket.FlowerType = (byte)0;
                                    FloorPacket.DropType = MsgDropID.Effect;
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var packet = rec.GetStream();
                                        client.Player.View.SendView(packet.ItemPacketCreate(FloorPacket), true);
                                    }
                                    break;
                                }
                            case "eat":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var packet = rec.GetStream();
                                        //   for(int i =0; i < 30; i++)
                                        //for(int x = 0; x< 30; x++)
                                        {
                                            Game.MsgFloorItem.MsgItemPacket effect = Game.MsgFloorItem.MsgItemPacket.Create();
                                            effect.m_UID = (uint)uint.Parse(data[1]);// Game.MsgFloorItem.MsgItemPacket.EffectMonsters.EarthquakeUpDown;
                                            effect.DropType = (MsgDropID)13;
                                            effect.m_X = client.Player.X;
                                            effect.m_Y = client.Player.Y;
                                            effect.ItemOwnerUID = client.Player.UID;
                                            client.Send(packet.ItemPacketCreate(effect));
                                        }
                                    }
                                    break;
                                }
                            case "activetrap":
                                {
                                    // for (ushort x = ushort.Parse(data[1]); x < ushort.Parse(data[2]); x++)
                                    {
                                        Game.MsgFloorItem.MsgItemPacket FloorPacket = Game.MsgFloorItem.MsgItemPacket.Create();
                                        FloorPacket.m_UID = Game.MsgFloorItem.MsgItem.UIDS.Count;
                                        FloorPacket.m_ID = 1390;
                                        FloorPacket.m_X = client.Player.X;
                                        FloorPacket.m_Y = client.Player.Y;

                                        FloorPacket.ItemOwnerUID = client.Player.UID;


                                        FloorPacket.m_Color = (byte)0;//4;
                                        FloorPacket.m_Color2 = (byte)14;
                                        FloorPacket.FlowerType = 3;
                                        FloorPacket.DropType = Game.MsgFloorItem.MsgDropID.RemoveEffect;
                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            var packet = rec.GetStream();
                                            client.Send(packet.ItemPacketCreate(FloorPacket));
                                        }
                                    }
                                    break;
                                }
                            case "bodynpcs":
                                {
                                    Game.MsgServer.MsgMovement.Bodyyyy = uint.Parse(data[1]);
                                    break;
                                }

                            case "hps":
                                {
                                    client.Player.HitPoints = int.Parse(data[1]);
                                    client.Player.SendUpdateHP();
                                    break;
                                }
                            case "tgui":
                                {
                                    /*Data datapacket = new Data(true);
                                            datapacket.UID = client.Entity.UID;
                                            datapacket.ID = 162;
                                            datapacket.dwParam = 4020;
                                            datapacket.Facing = (Game.Enums.ConquerAngle)client.Entity.Facing;
                                            datapacket.wParam1 = 73;
                                            datapacket.wParam2 = 98;
                                            client.Send(datapacket);*/
                                    var action = new ActionQuery()
                                    {
                                        Type = ActionType.OpenDialog,
                                        ObjId = client.Player.UID,
                                        dwParam = uint.Parse(data[1]),
                                        wParam1 = client.Player.X,
                                        wParam2 = client.Player.Y
                                    };
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var packet = rec.GetStream();
                                        client.Send(packet.ActionCreate(&action));

                                    }
                                    break;
                                }
                            case "hair":
                                {
                                    client.Player.Hair = (ushort)((client.Player.Hair - (client.Player.Hair % 100)) + ushort.Parse(data[1]));
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var packet = rec.GetStream();
                                        client.Player.SendUpdate(packet, client.Player.Hair, MsgServer.MsgUpdate.DataType.HairStyle);

                                    }
                                    break;
                                }
                            case "dcinter":
                                {

                                    client.Socket.Disconnect();
                                    break;
                                }



                            case "floor2":
                                {
                                    //for (ushort x = ushort.Parse(data[1]); x < ushort.Parse(data[2]); x++)
                                    {
                                        Game.MsgFloorItem.MsgItemPacket FloorPacket = Game.MsgFloorItem.MsgItemPacket.Create();
                                        FloorPacket.m_UID = Game.MsgFloorItem.MsgItem.UIDS.Next;
                                        FloorPacket.m_ID = 930;
                                        FloorPacket.m_X = client.Player.X;
                                        FloorPacket.m_Y = client.Player.Y;
                                        FloorPacket.m_Color = 13;
                                        FloorPacket.FlowerType = 2;
                                        FloorPacket.Name = "AuroraLotus";
                                        FloorPacket.DropType = Game.MsgFloorItem.MsgDropID.Effect;
                                        FloorPacket.Timer = Role.Core.TqTimer(DateTime.Now.AddSeconds(5));
                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            var packet = rec.GetStream();
                                            client.Send(packet.ItemPacketCreate(FloorPacket));
                                        }
                                        //ItemPacketCreate
                                    }
                                    break;
                                }
                            case "exit":
                                {
                                    Program.ProcessConsoleEvent(0);
                                    break;
                                }
                            case "quiz":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();

                                        client.Send(stream.QuizShowCreate((MsgServer.MsgQuizShow.AcotionID)ushort.Parse(data[1]), (ushort)5, 0, 0, 0, (ushort)900, 600, 300,
                                         "TEst1", "TEst1", "TEst1", "TEst1", "TEst1"));


                                    }
                                    break;
                                }
                            case "learnspells":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        foreach (var spell in Database.Server.Magic.Values)
                                        {
                                            if (spell.Keys.Count > 0)
                                            {
                                                var sp = spell[(ushort)(spell.Keys.Count - 1)];
                                                client.MySpells.Add(stream, sp.ID, sp.Level);
                                                System.Threading.Thread.Sleep(2);
                                            }
                                        }

                                    }
                                    break;
                                }
                            case "testupd":
                                {


                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        for (int i = 100; i < 110; i++)
                                            for (int x = 50; x < 220; x++)
                                            {

                                                Game.MsgServer.MsgUpdate packet = new Game.MsgServer.MsgUpdate(stream, client.Player.UID, 1);
                                                stream = packet.Append(stream, (MsgUpdate.DataType)i, (uint)x, 30, 300, 300);
                                                stream = packet.GetArray(stream);
                                                client.Player.View.SendView(stream, true);
                                            }
                                    }
                                    break;
                                }
                            case "asd":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();

                                        for (int x = 50; x < 200; x++)
                                        {
                                            client.Player.AddFlag((MsgUpdate.Flags)171, 10, true, 0);
                                            client.Player.SendUpdate(stream, (Game.MsgServer.MsgUpdate.Flags)171, 60
                                , 1, 4, (MsgUpdate.DataType)x, true);
                                        }
                                    }
                                    break;
                                }
                            case "exp":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();

                                        client.Player.SendUpdate(stream, uint.Parse(data[1]), MsgUpdate.DataType.Experience, false);
                                    }
                                    break;
                                }

                            case "testsinglepacket":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        //   for (int x = 100; x < 200; x++)
                                        {
                                            stream.Seek(4);

                                            byte[] pack = new byte[]
                                            {
                                        0x0A ,0x18 ,0x08 ,0x1A ,0x10 ,0xCE ,0x87 ,0x01 ,0x18 ,0xDF ,0x1E ,0x20
,0xC1 ,0x03 ,0x28 ,0xB9 ,0x03 ,0x30 ,0x00 ,0x3A ,0x05 ,0x4B ,0x79 ,0x6C ,0x69 ,0x6E ,0x0A ,0x1A
,0x08 ,0x15 ,0x10 ,0xC9 ,0x87 ,0x01 ,0x18 ,0xDF ,0x1E ,0x20 ,0xA3 ,0x01 ,0x28 ,0x9F ,0x01 ,0x30
,0x00 ,0x3A ,0x07 ,0x50 ,0x79 ,0x72 ,0x61 ,0x6D ,0x69 ,0x64 ,0x0A ,0x18 ,0x08 ,0x16 ,0x10 ,0xCA
,0x87 ,0x01 ,0x18 ,0xDF ,0x1E ,0x20 ,0xF7 ,0x01 ,0x28 ,0x94 ,0x01 ,0x30 ,0x00 ,0x3A ,0x05 ,0x48
,0x65 ,0x62 ,0x62 ,0x79 ,0x0A ,0x1B ,0x08 ,0x17 ,0x10 ,0xCB ,0x87 ,0x01 ,0x18 ,0xDF ,0x1E ,0x20
,0xE4 ,0x02 ,0x28 ,0xCE ,0x01 ,0x30 ,0x00 ,0x3A ,0x08 ,0x42 ,0x61 ,0x73 ,0x69 ,0x6C ,0x69 ,0x73
,0x6B ,0x0A ,0x1A ,0x08 ,0x18 ,0x10 ,0xCC ,0x87 ,0x01 ,0x18 ,0xDF ,0x1E ,0x20 ,0xBC ,0x03 ,0x28
,0x87 ,0x02 ,0x30 ,0x00 ,0x3A ,0x07 ,0x46 ,0x72 ,0x65 ,0x65 ,0x64 ,0x6F ,0x6D ,0x0A ,0x18 ,0x08
,0x19 ,0x10 ,0xCD ,0x87 ,0x01 ,0x18 ,0xDF ,0x1E ,0x20 ,0xC7 ,0x03 ,0x28 ,0xE8 ,0x02 ,0x30 ,0x00
,0x3A ,0x05 ,0x48 ,0x6F ,0x6E ,0x6F ,0x72 ,0x0A ,0x17 ,0x08 ,0x1B ,0x10 ,0xCF ,0x87 ,0x01 ,0x18
,0xDF ,0x1E ,0x20 ,0xE7 ,0x02 ,0x28 ,0xCB ,0x03 ,0x30 ,0x00 ,0x3A ,0x04 ,0x4C ,0x69 ,0x6F ,0x6E
,0x0A ,0x1B ,0x08 ,0x1C ,0x10 ,0xD0 ,0x87 ,0x01 ,0x18 ,0xDF ,0x1E ,0x20 ,0x88 ,0x02 ,0x28 ,0xC0
,0x03 ,0x30 ,0x00 ,0x3A ,0x08 ,0x41 ,0x71 ,0x75 ,0x61 ,0x72 ,0x69 ,0x75 ,0x73 ,0x0A ,0x18 ,0x08
,0x1D ,0x10 ,0xD1 ,0x87 ,0x01 ,0x18 ,0xDF ,0x1E ,0x20 ,0xB4 ,0x01 ,0x28 ,0xE0 ,0x02 ,0x30 ,0x00
,0x3A ,0x05 ,0x45 ,0x61 ,0x67 ,0x6C ,0x65 ,0x0A ,0x1C ,0x08 ,0x1E ,0x10 ,0xD2 ,0x87 ,0x01 ,0x18
,0xDF ,0x1E ,0x20 ,0x9C ,0x01 ,0x28 ,0x8A ,0x02 ,0x30 ,0x00 ,0x3A ,0x09 ,0x4C ,0x69 ,0x67 ,0x68
,0x74 ,0x6E ,0x69 ,0x6E ,0x67 ,0x0A ,0x19 ,0x08 ,0x67 ,0x10 ,0x80 ,0x87 ,0x01 ,0x18 ,0xDF ,0x1E
,0x20 ,0xB0 ,0x02 ,0x28 ,0xB4 ,0x02 ,0x30 ,0x01 ,0x3A ,0x06 ,0x52 ,0x65 ,0x61 ,0x6C ,0x6D ,0x33

                                            };

                                            for (int x = 0; x < pack.Length; x++)
                                                stream.Write((byte)pack[x]);
                                            //        stream.Write(byte.Parse(data[1]));//109576
                                            //        stream.Write((byte)71);
                                            //   stream.Write((uint)uint.Parse(data[2]));
                                            //  stream.Write(0);
                                            //     stream.Write(uint.MaxValue);
                                            //     stream.Write(uint.MaxValue); stream.Write(uint.MaxValue);

                                            stream.Finalize(2501);
                                            client.Send(stream);
                                        }
                                    }
                                    break;
                                }
                            case "testpacket":
                                {
                                    using (var rec2 = new ServerSockets.RecycledPacket())
                                    {
                                        var stream2 = rec2.GetStream();
                                        stream2.InitWriter();
                                        stream2.ZeroFill(4);
                                        stream2.Write((ushort)41);
                                        stream2.Write((ushort)40);
                                        stream2.Write((ushort)10183);
                                        stream2.ZeroFill(2);
                                        stream2.Write((uint)1);
                                        stream2.Write((uint)0);
                                        stream2.ZeroFill(8);
                                        stream2.Finalize(0x451);

                                        client.Player.Send(stream2);
                                    }
                                    break;
                                }
                            case "test":
                                {

                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        for (int u = 30; u < 200; u++)
                                        {
                                            stream.Seek(4);
                                            //   stream.Write(Time32.Now.Value);
                                            stream.Write(client.Player.UID);
                                            stream.Write(1);

                                            stream.Write(u);//78
                                            stream.Write(620);//173);
                                            stream.Write(172);//0x02);//2
                                            stream.Write(4);//300
                                            for (int x = 0; x < 10; x++)
                                            {
                                                stream.Write(0);
                                            }
                                            stream.Finalize(10017);
                                            client.Send(stream);
                                        }
                                    }
                                    break;
                                }
                            case "ada":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        for (ushort x = ushort.Parse(data[1]); x < ushort.Parse(data[2]); x++)
                                        {


                                            InteractQuery inter = new InteractQuery();
                                            inter.AtkType = (MsgAttackPacket.AttackID)x;
                                            inter.OpponentUID = client.Player.UID;
                                            inter.UID = client.Player.UID;
                                            inter.Damage = 3;
                                            inter.SpellID = 12550;
                                            inter.X = client.Player.X;
                                            inter.Y = client.Player.Y;

                                            stream.InteractionCreate(&inter);
                                            client.Send(stream);
                                        }

                                    }
                                    break;
                                }
                            case "reborn":
                                {
                                    client.Player.Reborn = byte.Parse(data[1]);
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.Player.SendUpdate(stream, client.Player.Reborn, MsgUpdate.DataType.Reborn);
                                    }
                                    break;
                                }
                            case "class":
                                {
                                    client.Player.Class = byte.Parse(data[1]);
                                    break;
                                }

                            case "str":
                                {
                                    ushort atr = 0;
                                    ushort.TryParse(data[1], out atr);
                                    if (client.Player.Atributes >= atr)
                                    {
                                        client.Player.Strength += atr;
                                        client.Player.Atributes -= atr;
                                        client.Equipment.QueryEquipment(false);
                                    }
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.Player.SendUpdate(stream, client.Player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                                        client.Player.SendUpdate(stream, client.Player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                                        client.Player.SendUpdate(stream, client.Player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                                        client.Player.SendUpdate(stream, client.Player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);
                                        client.Player.SendUpdate(stream, client.Player.Atributes, Game.MsgServer.MsgUpdate.DataType.Atributes);

                                    }
                                    break;
                                }
                            case "vit":
                                {
                                    ushort atr = 0;
                                    ushort.TryParse(data[1], out atr);
                                    if (client.Player.Atributes >= atr)
                                    {
                                        client.Player.Vitality += atr;
                                        client.Player.Atributes -= atr;
                                        client.Equipment.QueryEquipment(false);
                                    }
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.Player.SendUpdate(stream, client.Player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                                        client.Player.SendUpdate(stream, client.Player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                                        client.Player.SendUpdate(stream, client.Player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                                        client.Player.SendUpdate(stream, client.Player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);
                                        client.Player.SendUpdate(stream, client.Player.Atributes, Game.MsgServer.MsgUpdate.DataType.Atributes);

                                    }
                                    break;
                                }
                            case "reallot":
                                {
                                    client.Player.Agility = 0;
                                    client.Player.Strength = 0;
                                    client.Player.Vitality = 1;
                                    client.Player.Spirit = 0;

                                    client.CreateBoxDialog("You have successfully reloaded your attribute points.");
                                    if (client.Player.Reborn == 0)
                                    {
                                        client.Player.Atributes = 0;
                                        Database.DataCore.AtributeStatus.ResetStatsNonReborn(client.Player);
                                        if (Database.AtributesStatus.IsWater(client.Player.Class))
                                        {
                                            if (client.Player.Level > 110)
                                                client.Player.Atributes = (ushort)((client.Player.Level - 110) * 3 + client.Player.ExtraAtributes);
                                        }
                                        else
                                        {
                                            if (client.Player.Level > 120)
                                                client.Player.Atributes = (ushort)((client.Player.Level - 120) * 3 + client.Player.ExtraAtributes);
                                        }
                                    }
                                    else if (client.Player.Reborn == 1)
                                    {
                                        client.Player.Atributes = (ushort)(Database.Server.RebornInfo.ExtraAtributePoints(client.Player.FirstRebornLevel, client.Player.FirstClass)
                                            + 52 + 3 * (client.Player.Level - 15) + client.Player.ExtraAtributes);
                                    }
                                    else
                                    {
                                        if (client.Player.SecoundeRebornLevel == 0)
                                            client.Player.SecoundeRebornLevel = 130;
                                        client.Player.Atributes = (ushort)(Database.Server.RebornInfo.ExtraAtributePoints(client.Player.FirstRebornLevel, client.Player.FirstClass) +
                                            Database.Server.RebornInfo.ExtraAtributePoints(client.Player.SecoundeRebornLevel, client.Player.SecondClass) + 52 + 3 * (client.Player.Level - 15) + client.Player.ExtraAtributes);
                                    }
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.Player.SendUpdate(stream, client.Player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                                        client.Player.SendUpdate(stream, client.Player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                                        client.Player.SendUpdate(stream, client.Player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                                        client.Player.SendUpdate(stream, client.Player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);
                                        client.Player.SendUpdate(stream, client.Player.Atributes, Game.MsgServer.MsgUpdate.DataType.Atributes);

                                    }
                                    break;
                                }
                            case "spi":
                                {
                                    ushort atr = 0;
                                    ushort.TryParse(data[1], out atr);
                                    if (client.Player.Atributes >= atr)
                                    {
                                        client.Player.Spirit += atr;
                                        client.Player.Atributes -= atr;
                                        client.Equipment.QueryEquipment(false);
                                    }
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.Player.SendUpdate(stream, client.Player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                                        client.Player.SendUpdate(stream, client.Player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                                        client.Player.SendUpdate(stream, client.Player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                                        client.Player.SendUpdate(stream, client.Player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);
                                        client.Player.SendUpdate(stream, client.Player.Atributes, Game.MsgServer.MsgUpdate.DataType.Atributes);

                                    }
                                    break;
                                }
                            case "stuck":
                                {
                                    if (client.Player.Map == 6000)
                                        client.Teleport(30, 74, 6000);
                                    if (!Program.BlockTeleportMap.Contains(client.Player.Map))
                                    {
                                        client.Teleport(430, 378, 1002);
                                    }
                                    break;
                                }
                            case "allieschat":
                                client.Player.SendAllies = !client.Player.SendAllies;
                                client.SendSysMesage($"Allies Chat mode: {client.Player.SendAllies}");
                                break;
                            case "resetscores":
                                client.Player.TotalHits = client.Player.Hits = client.Player.Chains = client.Player.MaxChains = 0;
                                break;
                            case "dc":
                                client.Socket.Disconnect();
                                break;
                            case "visualeffects":
                                client.Player.ShowGemEffects = !client.Player.ShowGemEffects;
                                client.SendSysMesage("Visual effects status: " + client.Player.ShowGemEffects);
                                break;
                            case "agi":
                                {
                                    ushort atr = 0;
                                    ushort.TryParse(data[1], out atr);
                                    if (client.Player.Atributes >= atr)
                                    {
                                        client.Player.Agility += atr;
                                        client.Player.Atributes -= atr;
                                        client.Equipment.QueryEquipment(false);
                                    }
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.Player.SendUpdate(stream, client.Player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                                        client.Player.SendUpdate(stream, client.Player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                                        client.Player.SendUpdate(stream, client.Player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                                        client.Player.SendUpdate(stream, client.Player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);
                                        client.Player.SendUpdate(stream, client.Player.Atributes, Game.MsgServer.MsgUpdate.DataType.Atributes);

                                    }
                                    break;
                                }
                        }
                        return true;
                    }
                    if (client.PlayerHelper)
                    {
                        string PHlogs = "[PHLogs]@" + client.Player.Name + "@";
                        string Message2 = msg.__Message.Substring(1);//.ToLower();
                        string[] data2 = Message2.Split(' ');
                        for (int x = 0; x < data2.Length; x++)
                            PHlogs += data2[x] + " ";
                        Database.ServerDatabase.LoginQueue.Enqueue(PHlogs);

                        switch (data[0])
                        {
                            case "ph":
                                {
                                    client.SendSysMesage("Welcome Player Helper.");
                                    break;
                                }
                            case "trace":
                                {
                                    foreach (var user in Database.Server.GamePoll.Values)
                                    {
                                        if (user.Player.Name.ToLower().Contains(data[1].ToLower()))
                                        {
                                            client.Teleport(user.Player.X, user.Player.Y, user.Player.Map, user.Player.DynamicID);
                                            break;
                                        }
                                    }

                                    break;
                                }
                                ///// New Commands /////
                                ///

                        }
                        return true;
                    }
                    if (client.Player.VipLevel >= 6)
                    {
                        VIPCommands(client, msg);
                        return true;
                    }
                    if (client.Player.VipLevel == 0)
                    {
                        PlayerCommands(client, msg);
                        return true;
                    }
                    return true;
                }

            }
            catch
            {
                return false;

            }
            return false;
        }
        public static unsafe bool VIPCommands(Client.GameClient client, MsgMessage msg)
        {
            try
            {


                string logss = "[Chat]" + msg._From + " to " + msg._To + " " + msg.__Message + "";
                Database.ServerDatabase.LoginQueue.Enqueue(logss);

                msg.__Message = msg.__Message.Replace("#60", "").Replace("#61", "").Replace("#62", "").Replace("#63", "").Replace("#64", "").Replace("#65", "").Replace("#66", "").Replace("#67", "").Replace("#68", "");

                if (msg.__Message.StartsWith("/"))
                {
                    string logs = "[VIPLogs]" + client.Player.Name + " ";

                    string Message = msg.__Message.Substring(1);//.ToLower();
                    string[] data = Message.Split(' ');
                    for (int x = 0; x < data.Length; x++)
                        logs += data[x] + " ";
                    Database.ServerDatabase.LoginQueue.Enqueue(logs);

                    if (client.Player.VipLevel >= 4 && !client.ProjectManager && client.Player.Alive)
                    {
                        switch (data[0])
                        {
                            #region /joinpvp
                            case "pvp":
                                {
                                    EventsLib.EventManager.JoinPVP(client);
                                    break;
                                }
                            #endregion
                            case "trash":
                                {
                                    if (client.Player.Map == 1572) // Checa se o jogador está no mapa 1572
                                    {
                                        client.SendSysMesage("Você não pode usar o comando /trash neste mapa!", MsgMessage.ChatMode.TopLeftSystem);
                                    }
                                    else
                                    {
                                        client.Player.TrashGold = !client.Player.TrashGold;
                                        client.Player.TrashItems = !client.Player.TrashItems;
                                        client.SendSysMesage("VIP Drop Gold: " + (client.Player.TrashGold ? "Enabled" : "Disabled"), MsgMessage.ChatMode.TopLeftSystem);
                                        client.SendSysMesage("VIP Drop Trash Items: " + (client.Player.TrashItems ? "Enabled" : "Disabled"), MsgMessage.ChatMode.TopLeftSystem);
                                    }
                                    break;
                                }
                            case "clear":
                            case "clearinventory":
                                {
                                    using (var rec = new ServerSockets.RecycledPacket())
                                        client.Inventory.Clear(rec.GetStream());
                                    break;
                                }
                            case "scroll":
                                {
                                    if (client.Player.ContainFlag(MsgUpdate.Flags.FlashingName) || client.Player.ContainFlag(MsgUpdate.Flags.BlackName))
                                        break;
                                    if (client.Player.Map == 6000 || client.Player.DynamicID != 0 || client.Player.Alive == false)
                                        return false;
                                    switch (data[1].ToLower())
                                    {
                                        case "tc": client.Teleport(428, 378, 1002); break;
                                        case "pc": client.Teleport(195, 260, 1011); break;
                                        case "am": client.Teleport(566, 563, 1020); break;
                                        case "dc": client.Teleport(500, 645, 1000); break;
                                        case "bi": client.Teleport(723, 573, 1015); break;
                                        case "ma": client.Teleport(211, 196, 1036); break;
                                    }
                                    break;
                                }
                            case "agi":
                                {
                                    ushort atr = 0;
                                    ushort.TryParse(data[1], out atr);
                                    if (client.Player.Atributes >= atr)
                                    {
                                        client.Player.Agility += atr;
                                        client.Player.Atributes -= atr;
                                        client.Equipment.QueryEquipment(false);
                                    }
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.Player.SendUpdate(stream, client.Player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                                        client.Player.SendUpdate(stream, client.Player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                                        client.Player.SendUpdate(stream, client.Player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                                        client.Player.SendUpdate(stream, client.Player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);
                                        client.Player.SendUpdate(stream, client.Player.Atributes, Game.MsgServer.MsgUpdate.DataType.Atributes);

                                    }
                                    break;
                                }
                            case "str":
                                {
                                    ushort atr = 0;
                                    ushort.TryParse(data[1], out atr);
                                    if (client.Player.Atributes >= atr)
                                    {
                                        client.Player.Strength += atr;
                                        client.Player.Atributes -= atr;
                                        client.Equipment.QueryEquipment(false);
                                    }
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.Player.SendUpdate(stream, client.Player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                                        client.Player.SendUpdate(stream, client.Player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                                        client.Player.SendUpdate(stream, client.Player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                                        client.Player.SendUpdate(stream, client.Player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);
                                        client.Player.SendUpdate(stream, client.Player.Atributes, Game.MsgServer.MsgUpdate.DataType.Atributes);

                                    }
                                    break;
                                }
                            case "vit":
                                {
                                    ushort atr = 0;
                                    ushort.TryParse(data[1], out atr);
                                    if (client.Player.Atributes >= atr)
                                    {
                                        client.Player.Vitality += atr;
                                        client.Player.Atributes -= atr;
                                        client.Equipment.QueryEquipment(false);
                                    }
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.Player.SendUpdate(stream, client.Player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                                        client.Player.SendUpdate(stream, client.Player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                                        client.Player.SendUpdate(stream, client.Player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                                        client.Player.SendUpdate(stream, client.Player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);
                                        client.Player.SendUpdate(stream, client.Player.Atributes, Game.MsgServer.MsgUpdate.DataType.Atributes);

                                    }
                                    break;
                                }
                            case "reallot":
                                {
                                    if (client.Inventory.Contain(Database.ItemType.DragonBall, 1))
                                    {
                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            var stream = rec.GetStream();
                                            client.Inventory.Remove(Database.ItemType.DragonBall, 1, stream);
                                        }
                                        client.Player.Agility = 0;
                                        client.Player.Strength = 0;
                                        client.Player.Vitality = 1;
                                        client.Player.Spirit = 0;

                                        client.CreateBoxDialog("You've successfully reloaded your attribute points.");
                                        if (client.Player.Reborn == 0)
                                        {
                                            client.Player.Atributes = 0;
                                            Database.DataCore.AtributeStatus.ResetStatsNonReborn(client.Player);
                                            if (Database.AtributesStatus.IsWater(client.Player.Class))
                                            {
                                                if (client.Player.Level > 110)
                                                    client.Player.Atributes = (ushort)((client.Player.Level - 110) * 3 + client.Player.ExtraAtributes);
                                            }
                                            else
                                            {
                                                if (client.Player.Level > 120)
                                                    client.Player.Atributes = (ushort)((client.Player.Level - 120) * 3 + client.Player.ExtraAtributes);
                                            }
                                        }
                                        else if (client.Player.Reborn == 1)
                                        {
                                            client.Player.Atributes = (ushort)(Database.Server.RebornInfo.ExtraAtributePoints(client.Player.FirstRebornLevel, client.Player.FirstClass)
                                                + 52 + 3 * (client.Player.Level - 15) + client.Player.ExtraAtributes);
                                        }
                                        else
                                        {
                                            if (client.Player.SecoundeRebornLevel == 0)
                                                client.Player.SecoundeRebornLevel = 130;
                                            client.Player.Atributes = (ushort)(Database.Server.RebornInfo.ExtraAtributePoints(client.Player.FirstRebornLevel, client.Player.FirstClass) +
                                                Database.Server.RebornInfo.ExtraAtributePoints(client.Player.SecoundeRebornLevel, client.Player.SecondClass) + 52 + 3 * (client.Player.Level - 15) + client.Player.ExtraAtributes);
                                        }
                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            var stream = rec.GetStream();
                                            client.Player.SendUpdate(stream, client.Player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                                            client.Player.SendUpdate(stream, client.Player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                                            client.Player.SendUpdate(stream, client.Player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                                            client.Player.SendUpdate(stream, client.Player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);
                                            client.Player.SendUpdate(stream, client.Player.Atributes, Game.MsgServer.MsgUpdate.DataType.Atributes);

                                        }
                                    }
                                    else
                                    {
                                        client.SendSysMesage("Sorry, you need 1 DragonBall to use this feature!");
                                    }
                                    break;
                                }
                            case "spi":
                                {
                                    ushort atr = 0;
                                    ushort.TryParse(data[1], out atr);
                                    if (client.Player.Atributes >= atr)
                                    {
                                        client.Player.Spirit += atr;
                                        client.Player.Atributes -= atr;
                                        client.Equipment.QueryEquipment(false);
                                    }
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.Player.SendUpdate(stream, client.Player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                                        client.Player.SendUpdate(stream, client.Player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                                        client.Player.SendUpdate(stream, client.Player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                                        client.Player.SendUpdate(stream, client.Player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);
                                        client.Player.SendUpdate(stream, client.Player.Atributes, Game.MsgServer.MsgUpdate.DataType.Atributes);

                                    }
                                    break;
                                }
                            case "vipinfo":
                                {
                                    if (client.Player.VipLevel >= 4)
                                    {
                                        TimeSpan timer1 = new TimeSpan(client.Player.ExpireVip.Ticks);
                                        TimeSpan Now2 = new TimeSpan(DateTime.Now.Ticks);
                                        int days_left = (int)(timer1.TotalDays - Now2.TotalDays);
                                        int hour_left = (int)(timer1.TotalHours - Now2.TotalHours);
                                        int left_minutes = (int)(timer1.TotalMinutes - Now2.TotalMinutes);
                                        if (days_left > 0)
                                            client.SendSysMesage("Your VIP " + client.Player.VipLevel + " will expire in : " + days_left + " days.", MsgMessage.ChatMode.System);
                                        else if (hour_left > 0)
                                            client.SendSysMesage("Your VIP " + client.Player.VipLevel + " will expire in : " + hour_left + " hours.", MsgMessage.ChatMode.System);
                                        else if (left_minutes > 0)
                                            client.SendSysMesage("Your VIP " + client.Player.VipLevel + " will expire in : " + left_minutes + " minutes.", MsgMessage.ChatMode.System);

                                    }
                                    else client.SendSysMesage("You`re not VIP6.");
                                    break;

                                }
                            case "leave":
                                {
                                    if (Program.ArenaMaps.ContainsKey(client.Player.Map) || Program.ArenaMaps.ContainsKey(client.Player.DynamicID) || Program.ArenaMaps.ContainsKey((uint)client.Player.Betting))
                                    {
                                        foreach (var bot in Bots.BotProcessring.Bots.Values)
                                        {
                                            if (bot.Bot != null)
                                            {
                                                if (bot.Bot.Player.Map == client.Player.Map && bot.Bot.Player.DynamicID == client.Player.DynamicID)
                                                    bot.Dispose();
                                            }
                                        }
                                        if (client.Player.Betting > 0)
                                        {
                                            foreach (var player in client.Map.Values.Where(e => e.Player.DynamicID == client.Player.DynamicID))
                                            {
                                                client.SendSysMesage("You can't leave yet.", ChatMode.Talk);
                                                break;
                                            }

                                            client.Player.ConquerPoints += (uint)client.Player.Betting;
                                            client.Player.Betting = 0;
                                        }
                                        client.Teleport(428, 378, 1002);
                                    }
                                    break;
                                }
                            case "resetscores":
                                client.Player.TotalHits = client.Player.Hits = client.Player.Chains = client.Player.MaxChains = 0;
                                break;
                            case "stuck":
                                {
                                    if (client.Player.ContainFlag(MsgUpdate.Flags.FlashingName) || client.Player.ContainFlag(MsgUpdate.Flags.BlackName))
                                        break;
                                    if (client.Player.Map == 6000)
                                        client.Teleport(30, 74, 6000);
                                    if (!Program.BlockTeleportMap.Contains(client.Player.Map))
                                    {
                                        client.Teleport(428, 378, 1002);
                                    }
                                    break;
                                }
                            case "dc":
                                {
                                    client.Socket.Disconnect();
                                    break;
                                }
                            case "commands":
                                {
                                    client.SendSysMesage("@skipore => SkipOres");
                                    client.SendSysMesage("@scroll tc => Twin City scroll. citys => pc ac bi");
                                    client.SendSysMesage("@agi => Adjust your Atributtes. @str @vit @spi");
                                    client.SendSysMesage("@stuck => If stuck, this will help you");
                                    client.SendSysMesage("@dc => Disconnect from the server");
                                    client.SendSysMesage("@clear => Clear your inventory");
                                    break;
                                }
                            case "queroir":
                                MsgSchedules.eventMataMata.JoinEvent(client);
                                break;

                        }
                    }
                    return true;
                }
            }
            catch
            {
                return false;

            }
            return false;
        }
        public static unsafe bool PlayerCommands(Client.GameClient client, MsgMessage msg)
        {
            try
            {
                string logss = "[Chat]" + msg._From + " to " + msg._To + " " + msg.__Message + "";
                Database.ServerDatabase.LoginQueue.Enqueue(logss);

                msg.__Message = msg.__Message.Replace("#60", "").Replace("#61", "").Replace("#62", "").Replace("#63", "").Replace("#64", "").Replace("#65", "").Replace("#66", "").Replace("#67", "").Replace("#68", "");

                if (msg.__Message.StartsWith("/"))
                {
                    string logs = "[PlayerLogs]" + client.Player.Name + " ";

                    string Message = msg.__Message.Substring(1);//.ToLower();
                    string[] data = Message.Split(' ');
                    for (int x = 0; x < data.Length; x++)
                        logs += data[x] + " ";
                    Database.ServerDatabase.LoginQueue.Enqueue(logs);

                    if (!client.ProjectManager)
                    {
                        switch (data[0])
                        {
                            case "leave":
                                {
                                    if (Program.ArenaMaps.ContainsKey(client.Player.Map) || Program.ArenaMaps.ContainsKey(client.Player.DynamicID) || Program.ArenaMaps.ContainsKey((uint)client.Player.Betting))
                                    {
                                        foreach (var bot in Bots.BotProcessring.Bots.Values)
                                        {
                                            if (bot.Bot != null)
                                            {
                                                if (bot.Bot.Player.Map == client.Player.Map && bot.Bot.Player.DynamicID == client.Player.DynamicID)
                                                    bot.Dispose();
                                            }
                                        }
                                        if (client.Player.Betting > 0)
                                        {
                                            foreach (var player in client.Map.Values.Where(e => e.Player.DynamicID == client.Player.DynamicID))
                                            {
                                                client.SendSysMesage("You can't leave yet.", ChatMode.Talk);
                                                break;
                                            }

                                            client.Player.ConquerPoints += (uint)client.Player.Betting;
                                            client.Player.Betting = 0;
                                        }
                                        client.Teleport(428, 378, 1002);
                                    }
                                    break;
                                }
                            case "resetscores":
                                client.Player.TotalHits = client.Player.Hits = client.Player.Chains = client.Player.MaxChains = 0;
                                break;
                            //case "stuck":
                            //    {
                            //        if (client.Player.Map == 6000)
                            //            client.Teleport(30, 74, 6000);
                            //        break;
                            //    }
                            case "stuck":
                                {
                                    if (client.Player.ContainFlag(MsgUpdate.Flags.FlashingName) || client.Player.ContainFlag(MsgUpdate.Flags.BlackName))
                                        break;
                                    if (client.Player.Map == 6000)
                                        client.Teleport(30, 74, 6000);
                                    if (!Program.BlockTeleportMap.Contains(client.Player.Map))
                                    {
                                        client.Teleport(428, 378, 1002);
                                    }
                                    break;
                                }
                            case "dc":
                                {
                                    client.Socket.Disconnect();
                                    break;
                                }
                            case "queroir":
                                MsgSchedules.eventMataMata.JoinEvent(client);
                                break;

                            #region /joinpvp
                            case "pvp":
                                {
                                    EventsLib.EventManager.JoinPVP(client);
                                    break;
                                }

                                #endregion
                        }
                    }
                    return true;
                }
            }
            catch
            {
                return false;

            }
            return false;
        }

    }
}
