
using COServer.Cryptography;
using COServer.EventsLib;
using COServer.Game.MsgServer;
using COServer.Role;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;


namespace COServer
{
    //
    using PacketInvoker = CachedAttributeInvocation<Action<Client.GameClient, ServerSockets.Packet>, PacketAttribute, ushort>;

    class Program
    {
        public static Dictionary<uint, uint> ArenaMaps = new Dictionary<uint, uint>();

        public static VoteRank VoteRank;
        public static List<EventsLib.BaseEvent> Events = new List<EventsLib.BaseEvent>();
        public static Discord DiscordAPI = new Discord("https://discord.com/api/webhooks/1350119432281653260/_9LEa3oSPi0NVSw0RmSPAHgVyS6cDgk6GqN4DWCLLs3xh1NFFc2cOeex22nJRxnZ-RNR");
        public static Discord DiscordAPIsocket = new Discord("https://discord.com/api/webhooks/1344900319850659890/xWx54eysEH7nrth7hI8Z_5eeAvCNuPr1I4C5dMy9aANACwriRDR7nDGZZknMYBWULvKR");
        public static Discord DiscordAPIevents = new Discord("https://discord.com/api/webhooks/1344900809166422086/NgpmlUFSejpvg6hfGiPvLvroWs1i8qykVB5NA00p9FRgxQypyVf3Q9_ouPTDxU1vyWOO");
        public static Discord DiscordAPIworld = new Discord("https://discord.com/api/webhooks/1344902820616339486/ltKy_GfRDxjpnfFn8w2A5J7-ZYHQU-235E9EPKWxcgDibyBrGcX4mJJsCCoBR3lDEexw");
        public static Discord DiscordAPIfoundslog = new Discord("https://discord.com/api/webhooks/1294239789532315648/5ThwjJ1MCdEjzqIaN3-bSg-KJ6y425-wuP1juVW9cGxcx0AMJwVx3bZxvnRQRhFWoWYC");
        public static Discord DiscordAPIQuest = new Discord("https://discord.com/api/webhooks/1344900130062602310/7TulMmij4Ze7x86umCH0Qq6QnLVK5L-YTWSzVO_FcD3rl53j4fnNO94DyMbEv6MtrnD8");
        public static Discord DiscordAPILotery = new Discord("https://discord.com/api/webhooks/1294239212224385064/Ov086-5kATJBFwBnRMNB6RreLdCEr4gjzvXjCkhwGskfShbQhJywUsNPvh4HhiFMsrZT");
        public static Discord DiscordAPIRedDrop = new Discord("https://discord.com/api/webhooks/1344901857507545160/jo29SM0a5-G0jQ-SDLNMxsWgYPqvYe-InBgkk8ltP-Q11Clh5NPFg-VpyhP5RcCZtYnf");
        public static Discord DiscordAPIwinners = new Discord("https://discord.com/api/webhooks/1346216518521852057/m3yJrq-OXLUlGXI-zcjGru8h65UbUriLdCUiynjeea0vtkbmpjOwxBtK_p-cGdQg9pIB");
        public static Discord DiscordAPIplus = new Discord("https://discord.com/api/webhooks/1349117915449917481/XH3leGRYIC8R50ZhHXOK3pv0NpBWDgvy5YVuj7dCWWtSTshcK1O6NbxPKclLVt14IWMg");
        public static Discord DiscordAPITapete = new Discord("https://discord.com/api/webhooks/1344902388175208479/I0AcxtEOiQUzDguf4pND41LEv-9vcCAlwnZrQhkzl_NCqPNCgl-IiTCRUu5Jf4dLG36_");
        public static Discord DiscordAPIVote = new Discord("https://discord.com/api/webhooks/1344903113181757500/iU9bGOTvHy8Op6hH88isdRx6ENvpasdTD9a6NDmtRnU-oAKYShc6IMWoFE8rF0lPqpSD");
        public static Discord DiscordAPISurpriseBox = new Discord("https://discord.com/api/webhooks/1344905879459332137/TZJwOtJmaP9SJ2T5yVwynRguHIArkqikrdjkPx87IRC0woTF3Cfsoz_sFPFll0VSxHbL");
        public static Discord DiscordAPIClainFreeVip = new Discord("https://discord.com/api/webhooks/1344909426737414195/GWySjy2nAZnZ1PCQGcYfmOK-jBlJcmmQrrB2oNLjDD7JwKw8gDUjtYPkN-lWPUW_uYFi");
        public static Discord DiscordAPIBlessDrop = new Discord("https://discord.com/api/webhooks/1347945521729507328/f1QW8xIVB6M0uRoOS7qX_YJ6p6mhECn9yEPUs2UwYdcFZMqm8VCRrFEzPNdQiOi1dlLa");
        public static Discord DiscordAPI2rbnQuest = new Discord("https://discord.com/api/webhooks/1349052577982971944/WwgeAgJzVvb2Qmh3q9wVXRDV0p2CoUSMwWSDnoa4Zvq0KB6o8x_oopqKDazy1Jlf4NpE");
        public static Discord DiscordAPIMataMata = new Discord("https://discord.com/api/webhooks/1349414685094445127/RI3lbtEOpfJJ2eIwNHDV36kiTK58De310SnQFa6XmcRyYeYyPUWP8n5-9c1QkTUe3tHv");
        public static ulong CPsHuntedSinceRestart = 0;
        public static List<byte[]> LoadPackets = new List<byte[]>();
        public static List<uint> ProtectMapSpells = new List<uint>() { 1038 };
        public static List<uint> MapCounterHits = new List<uint>() { 1005, 6000 };
        public static List<uint> OutMap = new List<uint>(){ 50, 51, 52, 53 };
        public static List<uint> NoDrugMap = new List<uint>() { 50, 51, 52, 53 };
        public static List<uint> SsFbMap = new List<uint>();
        public static bool OnMainternance = false;
        public static Cryptography.TransferCipher transferCipher;
        public static List<uint> MapDC = new List<uint>() { 1572, 1036, 1017, 1081, 2060, 9972, 1080, 3820, 3954, 1806, 1508, 1768, 1505, 1506, 1509, 1508, 1801, 1780, 1779, 3071, 1068, 3830, 3831, 3832, 3834, 3826, 3827, 3828, 3829, 3833, 3825, 1601, 1201, 4000, 4003, 4006, 4008, 4009 };
        public static List<uint> RevivePoint = new List<uint>() { 1201, 1202 };
        public static List<uint> NoDropItems = new List<uint>() { 1764, 700, 1780, 3820, 1005 };
        public static List<uint> FreePkMap = new List<uint>() { 50, 51, 52, 53, 2071, 3998, 3071, 6000, 6001, 1505, 1005, 1038, 700, 1508, 1201, 1767 };
        public static List<uint> BlockAttackMap = new List<uint>() { 9999, 1700, 1202, 3825, 3830, 3831, 3832, 3834, 3826, 3827, 3828, 3829, 3833, 9995, 1068, 4020, 4000, 4003, 4006, 4008, 4009, 1860, 1858, 1801, 1780, 1779/*Ghost Map*/, 9972, 1806, 3954, 3081, 1036, 1601, 1004, 1008, 601, 1006, 1511, 1039, 700 };
        public static List<uint> BlockTeleportMap = new List<uint>() { 601, 1043, 1044, 1045, 1046, 1047, 1048, 1049, 1780, 1202, 6000, 6001, 1005, 700, 1860, 3852, 1768, 1038 };
        public static Role.Instance.Nobility.NobilityRanking NobilityRanking = new Role.Instance.Nobility.NobilityRanking();
        public static Role.Instance.Flowers.FlowersRankingToday FlowersRankToday = new Role.Instance.Flowers.FlowersRankingToday();
        public static Role.Instance.Flowers.FlowerRanking GirlsFlowersRanking = new Role.Instance.Flowers.FlowerRanking();
        public static Role.Instance.Flowers.FlowerRanking BoysFlowersRanking = new Role.Instance.Flowers.FlowerRanking(false);
        public static DateTime UPTimeServer = DateTime.Now;

        public static SendGlobalPacket SendGlobalPackets;
        public static PacketInvoker MsgInvoker;
        public static ServerSockets.ServerSocket GameServer;
        public static DateTime StartDate;

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleCtrlHandler(ConsoleHandlerDelegate handler, bool add);
        private delegate bool ConsoleHandlerDelegate(int type);
        private static ConsoleHandlerDelegate handlerKeepAlive;

        public static bool ProcessConsoleEvent(int type)
        {
            try
            {
                Console.WriteLine("Saving Database...");
                if (GameServer != null)
                    GameServer.Close();
                foreach (var client in Database.Server.GamePoll.Values)
                {
                    try
                    {
                        if (client.Socket != null)//for my fake accounts !
                            client.Socket.Disconnect();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }

                Database.Server.SaveDatabase();
                if (Database.ServerDatabase.LoginQueue.Finish())
                {
                    System.Threading.Thread.Sleep(1000);
                    Console.WriteLine("Database saved successfully.");
                }
            }
            catch (Exception e)
            {
                Console.SaveException(e);
            }
            return true;
        }

        public static Time32 ResetRandom = new Time32();

        public static FastRandom GetRandom = new FastRandom();

        public static class ServerConfig
        {
            public static string CO2Folder = "";
            public static string XtremeTopLink = "https://www.xtremetop100.com/in.php?site=1132376247";
            public static string IPAddres = "144.217.173.221";
            public static ushort GamePort = 5816;
            public static string ServerName = "Emulator";
            public static string OfficialWebSite = "google.com";
            public static ushort Port_BackLog;
            public static ushort Port_ReceiveSize = 4096;
            public static ushort Port_SendSize = 4096;//8191
            //Database
            public static string DbLocation = "Database5103";

            public static uint ExpRateSpell = 5;
            public static uint ExpRateProf = 5;
            public static bool IsInterServer = false;
            public static uint UserExpRate = 5;
            public static int PhysicalDamage = 100;// + 150%
        }

        public static void Main(string[] args)
        {

            try
            {
                StartDate = DateTime.Now;
                Console.DissableButton();
                ServerSockets.Packet.SealString = "TQServer";
                System.Console.ForegroundColor = ConsoleColor.Yellow;
                MsgInvoker = new PacketInvoker(PacketAttribute.Translator);
                //MsgProtect.HelperFunctions.Init();
                //GuardShield.MsgGuardShield.Load(true);
                Game.MsgTournaments.MsgSchedules.Create();
                Database.Server.Initialize();
                VoteRank = new VoteRank();
                SendGlobalPackets = new SendGlobalPacket();
                Cryptography.AuthCryptography.PrepareAuthCryptography();
                Database.Server.LoadDatabase();
                handlerKeepAlive = ProcessConsoleEvent;
                SetConsoleCtrlHandler(handlerKeepAlive, true);
                TransferCipher.Key = Encoding.ASCII.GetBytes("456KhLvYJ3zdLCTyz9Ak8RAgM78tY5F32b7CUXDuLDJDFBH8H67BWy9QThmaN5VS");
                TransferCipher.Salt = Encoding.ASCII.GetBytes("456VgBf3ytALHWLXbJxSUX4uFEu3Xmz2UAY9sTTm8AScB7Kk2uwqDSnuNJske4BJ");
                transferCipher = new TransferCipher("127.0.0.1");
                EventManager.Init();
                GameServer = new ServerSockets.ServerSocket(
                    new Action<ServerSockets.SecuritySocket>(p => new Client.GameClient(p))
                    , Game_Receive, Game_Disconnect);
                GameServer.Initilize(ServerConfig.Port_SendSize, ServerConfig.Port_ReceiveSize, 1, 3);
                GameServer.Open(ServerConfig.IPAddres, ServerConfig.GamePort, ServerConfig.Port_BackLog);
                TQHandle.Network.MsgServer.Run((ushort)(ServerConfig.GamePort + 1000));
                new ServerSockets.SocketThread(GameServer);
                //Game.MsgTournaments.MsgSchedules.CityWar = new Game.MsgTournaments.MsgCityWar();
                EventManager.TimeEvent = DateTime.Now;
                new KernelThread(1000);
                new MapGroupThread(100);
              //  new Bots.BotProcessring();
                System.Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine("  ");
                System.Console.WriteLine("  ");
                System.Console.WriteLine("  (  SOURCE CLASSIC 1.0 - v5517   )");
                System.Console.WriteLine("  ");
                System.Console.WriteLine("  ");
                Console.WriteLine("[Online]\n", ConsoleColor.Green);
                using (Database.MySqlCommand cmd = new Database.MySqlCommand(Database.MySqlCommandType.SELECT).Select("configuration"))
                {
                    using (Database.MySqlReader r = new Database.MySqlReader(cmd))
                    {
                        if (r.Read())
                        {
                            new Database.MySqlCommand(Database.MySqlCommandType.UPDATE).Update("configuration").Set("serveronline", 1).Execute();
                        }
                    }
                }
            }
            catch (Exception e) { Console.WriteException(e); }

            for (; ; )
                ConsoleCMD(Console.ReadLine());
        }
        public static string UpTimeMsg
        {
            get
            {
                var _uptime = (DateTime.Now - UPTimeServer);
                string msg = $"Uptime: {(int)_uptime.Days} Days {(int)_uptime.Hours} Hours {(int)_uptime.Minutes} Minutes {(int)_uptime.Seconds} Seconds.";
                return msg;
            }
        }
        public static string OnlineMsg
        {
            get
            {
                string msg = $"Online: {KernelThread.GetOnline() * 2} ({KernelThread.GetMaxOnline() * 2} Max).";
                return msg;
            }
        }
        public static Time32 SaveDBStamp = Time32.Now.AddMilliseconds(KernelThread.SaveDatabaseStamp);
        public static Time32 ResetStamp = Time32.Now.AddMilliseconds(KernelThread.ResetDayStamp);

        public static void SaveDBPayers(Time32 clock, bool forceSave = false)
        {
            // Verifica se o tempo atual excedeu o timestamp de salvamento ou se o salvamento foi forçado
            if (clock > SaveDBStamp || forceSave)
            {
                // Verifica se o servidor está totalmente carregado antes de prosseguir
                if (Database.Server.FullLoading)
                {
                    try
                    {
                        // Itera sobre todos os usuários conectados
                        foreach (var user in Database.Server.GamePoll.Values)
                        {
                            // Verifica se o usuário está logado completamente
                            if ((user.ClientFlag & Client.ServerFlag.LoginFull) == Client.ServerFlag.LoginFull)
                            {
                                // Marca o usuário para salvamento e adiciona na fila de login
                                user.ClientFlag |= Client.ServerFlag.QueuesSave;
                                Database.ServerDatabase.LoginQueue.TryEnqueue(user);
                            }
                        }

                        // Chama o método de salvamento do banco de dados
                        Database.Server.SaveDatabase();
                        Console.WriteLine("Database saved update !", ConsoleColor.Magenta);
                    }
                    catch (Exception e)
                    {
                        // Loga qualquer exceção ocorrida durante o processo de salvamento
                        Console.WriteLine($"SaveDBPayers - Exception: {e.Message}");
                        Console.SaveException(e);
                    }
                }

                // Atualiza o timestamp para o próximo salvamento, exceto se o salvamento foi forçado
                if (!forceSave)
                {
                    SaveDBStamp.Value = clock.Value + KernelThread.SaveDatabaseStamp;
                }
            }
        }

        public unsafe static void ConsoleCMD(string cmd)
        {
            try
            {
                string[] line = cmd.Split(' ');

                switch (line[0])
                {
                    //case "cp2":
                    //    {
                    //        MsgProtect.ProGuardControl cp = new MsgProtect.ProGuardControl();
                    //        cp.ShowDialog();
                    //        break;
                    //    }
                    case "clear":
                        {
                            System.Console.Clear();
                            break;
                        }
                    case "testwar":
                        {
                            COServer.Game.MsgTournaments.MsgSchedules.PkWar.Open();
                            break;
                        }
                    case "testwar2":
                        {
                            COServer.Game.MsgTournaments.MsgSchedules.CurrentTournament = COServer.Game.MsgTournaments.MsgSchedules.Tournaments[COServer.Game.MsgTournaments.TournamentType.FiveNOut];
                            COServer.Game.MsgTournaments.MsgSchedules.CurrentTournament.Open();
                            break;
                        }
                    case "dc":
                        {
                            foreach (var item in COServer.Database.Server.GamePoll.Values)
                            {
                                item.Socket.Disconnect();
                            }
                            break;
                        }
                    case "save":
                        {
                            Database.Server.SaveDatabase();
                            if (Database.Server.FullLoading)
                            {
                                foreach (var user in Database.Server.GamePoll.Values)
                                {
                                    if ((user.ClientFlag & Client.ServerFlag.LoginFull) == Client.ServerFlag.LoginFull)
                                    {
                                        user.ClientFlag |= Client.ServerFlag.QueuesSave;
                                        Database.ServerDatabase.LoginQueue.TryEnqueue(user);
                                    }
                                }
                                // Console.WriteLine("Database got saved ! ");
                            }
                            if (Database.ServerDatabase.LoginQueue.Finish())
                            {
                                System.Threading.Thread.Sleep(1000);
                                Console.WriteLine("Database saved update successfully.");
                            }
                            break;
                        }
                    case "loadcontrol":
                        {
                            ProjectControl.ServerControl();
                            Console.WriteLine("Done .");
                            break;
                        }
                    case "cp":
                        {
                            Controlpanel cp = new Controlpanel();
                            cp.ShowDialog();
                            break;
                        }
                    case "resetnobility":
                        {
                            WindowsAPI.IniFile ini = new WindowsAPI.IniFile("");
                            foreach (string fname in System.IO.Directory.GetFiles(Program.ServerConfig.DbLocation + "\\Users\\"))
                            {
                                ini.FileName = fname;

                                ulong nobility = ini.ReadUInt64("Character", "DonationNobility", 0);
                                ini.Write<ulong>("Character", "DonationNobility", 0);
                            }

                            break;
                        }
                    case "check":
                        {
                            WindowsAPI.IniFile ini = new WindowsAPI.IniFile("");
                            foreach (string fname in System.IO.Directory.GetFiles(Program.ServerConfig.DbLocation + "\\Users\\"))
                            {
                                ini.FileName = fname;

                                long nobility = ini.ReadInt64("Character", "Money", 0);
                                if (nobility < 0)
                                {
                                    Console.WriteLine("");
                                }

                            }
                            break;
                        }
                    case "fixedgamemap":
                        {
                            Dictionary<int, string> maps = new Dictionary<int, string>();
                            using (var gamemap = new BinaryReader(new FileStream(Path.Combine(Program.ServerConfig.CO2Folder, "ini/gamemap.dat"), FileMode.Open)))
                            {

                                var amount = gamemap.ReadInt32();
                                for (var i = 0; i < amount; i++)
                                {

                                    var id = gamemap.ReadInt32();
                                    var fileName = Encoding.ASCII.GetString(gamemap.ReadBytes(gamemap.ReadInt32()));
                                    var puzzleSize = gamemap.ReadInt32();
                                    if (id == 1017)
                                    {
                                        Console.WriteLine(puzzleSize);
                                    }
                                    if (!maps.ContainsKey(id))
                                        maps.Add(id, fileName);
                                    else
                                        maps[id] = fileName;
                                }
                            }
                            break;
                        }


                    case "startgw":
                        {
                            Game.MsgTournaments.MsgSchedules.GuildWar.Proces = Game.MsgTournaments.ProcesType.Alive;
                            Game.MsgTournaments.MsgSchedules.GuildWar.Start();
                            break;
                        }
                    case "finishgw":
                        {
                            Game.MsgTournaments.MsgSchedules.GuildWar.Proces = Game.MsgTournaments.ProcesType.Dead;
                            Game.MsgTournaments.MsgSchedules.GuildWar.CompleteEndGuildWar();
                            break;
                        }

                    case "exit":
                        {
                            new Thread(new ThreadStart(Maintenance)).Start();
                            using (Database.MySqlCommand Close = new Database.MySqlCommand(Database.MySqlCommandType.SELECT).Select("configuration"))
                            {
                                using (Database.MySqlReader r = new Database.MySqlReader(Close))
                                {
                                    if (r.Read())
                                    {
                                        new Database.MySqlCommand(Database.MySqlCommandType.UPDATE).Update("configuration").Set("serveronline", 0).Execute();
                                    }
                                }
                            }
                            break;
                        }
                    //case "cp2":
                    //    {
                    //        new Panel().ShowDialog();
                    //        break;
                    //    }
                    case "forceexit":
                        {
                            ProcessConsoleEvent(0);

                            Environment.Exit(0);
                            break;
                        }
                    case "restart":
                        {
                            ProcessConsoleEvent(0);

                            System.Diagnostics.Process hproces = new System.Diagnostics.Process();
                            hproces.StartInfo.FileName = "COServer.exe";
                            hproces.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
                            hproces.Start();

                            Environment.Exit(0);

                            break;
                        }

                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        public static void Maintenance()
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                OnMainternance = true;
            }

            int totalSeconds = 5 * 60; // 5 minutos
            int interval = 30; // Intervalo de 30 segundos

            for (int timeLeft = totalSeconds; timeLeft > 0; timeLeft -= interval)
            {
                string message;

                if (timeLeft > 60)
                    message = $"The server will be brought down for maintenance in {timeLeft / 60} minutes.";
                else if (timeLeft == 60)
                    message = "The server will be brought down for maintenance in 1 minute.";
                else
                    message = $"The server will be brought down for maintenance in {timeLeft} seconds.";

                message += " Please log off immediately to avoid data loss.";

                Console.WriteLine(message);

                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    MsgMessage msg = new MsgMessage(message, "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
                    SendGlobalPackets.Enqueue(msg.GetArray(stream));
                }

                Thread.Sleep(1000 * interval);
            }

            // Mensagem final antes de desligar o servidor
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                MsgMessage msg = new MsgMessage("Server maintenance. Please log off immediately to avoid data loss.", "ALLUSERS", "GM", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Center);
                SendGlobalPackets.Enqueue(msg.GetArray(stream));
            }

            Thread.Sleep(10000); // Espera 10 segundos antes de desligar
            ProcessConsoleEvent(0);
            Environment.Exit(0);
        }


        public unsafe static void Game_Receive(ServerSockets.SecuritySocket obj, ServerSockets.Packet stream)
        {
            if (!obj.SetDHKey)
                CreateDHKey(obj, stream);
            else
            {
                try
                {
                    if (obj.Game == null)
                        return;
                    ushort PacketID = stream.ReadUInt16();
                    //if (PacketID == 1052)
                    //{
                    //    obj.Disconnect();
                    //    return;
                    //}
                    Action<Client.GameClient, ServerSockets.Packet> hinvoker;
                    if (MsgInvoker.TryGetInvoker(PacketID, out hinvoker))
                    {
                        // Console.WriteLine("the packet ----> " + PacketID);
                        hinvoker(obj.Game, stream);
                    }
                    else
                    {
                        Console.WriteLine("Not found the packet ----> " + PacketID);
                    }
                }
                catch (Exception e) { Console.WriteException(e); }
                finally
                {
                    ServerSockets.PacketRecycle.Reuse(stream);
                }
            }

        }
        public unsafe static void CreateDHKey(ServerSockets.SecuritySocket obj, ServerSockets.Packet Stream)
        {
            try
            {
                byte[] buffer = new byte[36];
                bool extra = false;
                string text = System.Text.ASCIIEncoding.ASCII.GetString(obj.DHKeyBuffer.buffer, 0, obj.DHKeyBuffer.Length());

                

                if (!text.EndsWith("TQClient"))
                {
                    
                    System.Buffer.BlockCopy(obj.EncryptedDHKeyBuffer.buffer, obj.EncryptedDHKeyBuffer.Length() - 36, buffer, 0, 36);
                    extra = true;
                }

                string key;
                if (Stream.GetHandshakeReplyKey(out key))
                {
                    
                    obj.SetDHKey = true;
                    obj.Game.Cryptography = obj.Game.DHKeyExchance.HandleClientKeyPacket(key, obj.Game.Cryptography);
                }
                else
                {
                    
                    obj.Disconnect();
                    return;
                }

                if (extra)
                {
                    
                    Stream.Seek(0);
                    obj.Game.Cryptography.Decrypt(buffer);
                    fixed (byte* ptr = buffer)
                        Stream.memcpy(Stream.Memory, ptr, 36);
                    Stream.Size = buffer.Length;
                    Stream.Seek(2);
                    ushort PacketID = Stream.ReadUInt16();

                    

                    Action<Client.GameClient, ServerSockets.Packet> hinvoker;
                    if (MsgInvoker.TryGetInvoker(PacketID, out hinvoker))
                    {
                        
                        hinvoker(obj.Game, Stream);
                    }
                    else
                    {
                        
                        obj.Disconnect();
                    }
                }
            }
            catch (Exception e)
            {
                // Log the exception with more details
                
                obj.Disconnect();
            }
        }
        public unsafe static void Game_Disconnect(ServerSockets.SecuritySocket obj)
        {
            if (obj.Game != null && obj.Game.Player != null)
            {
                try
                {
                    Client.GameClient client;

                    if (Database.Server.GamePoll.TryGetValue(obj.Game.Player.UID, out client))
                    {
                        if ((client.ClientFlag & Client.ServerFlag.LoginFull) == Client.ServerFlag.LoginFull)
                        {
                            Console.WriteLine($"[{DateTime.Now}] {client.Player.Name} has logged out.", ConsoleColor.Red);

                            // Verifica mineração offline (VIP 4+)
                            if (client.Player.Mining && client.Player.VipLevel >= 4)
                            {
                                Console.WriteLine($"[{DateTime.Now}] {client.Player.Name} está minerando offline (VIP {client.Player.VipLevel}). Mantendo no mapa...");
                                client.Player.OfflineMiner = true;
                                Role.OfflineMiningManager.StartOfflineMining(client);
                                client.ClientFlag &= ~Client.ServerFlag.LoginFull;
                                client.ClientFlag |= Client.ServerFlag.Disconnect;

                                // Adiciona o jogador offline à lista do GameMap
                                client.Map.AddOfflinePlayer(client.Player);

                                // Forçar atualização da visão para outros jogadores no mapa
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    Console.WriteLine($"[{DateTime.Now}] Atualizando visão para {client.Player.Name} no mapa {client.Player.Map}");
                                    client.Player.View.SendView(client.Player.GetArray(stream, false), true);
                                }
                                return;
                            }
                            else if (client.Player.Mining && client.Player.VipLevel < 4)
                            {
                                Console.WriteLine($"[{DateTime.Now}] {client.Player.Name} está minerando, mas não é VIP 4 ou superior. Desconectando normalmente...");
                            }

                            // Verifica se o jogador está vendendo
                            if (client.MyVendor != null && client.MyVendor.InVending)
                            {
                                Console.WriteLine($"[{DateTime.Now}] {client.Player.Name} está vendendo offline. Mantendo no mapa...");
                                Role.Instance.OfflineVendorManager.StartOfflineVending(client);
                                client.ClientFlag &= ~Client.ServerFlag.LoginFull;
                                client.ClientFlag |= Client.ServerFlag.Disconnect;

                                // Forçar atualização da visão para outros jogadores no mapa
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    Console.WriteLine($"[{DateTime.Now}] Atualizando visão para {client.Player.Name} no mapa {client.Player.Map}");
                                    client.Player.View.SendView(client.Player.GetArray(stream, false), true);
                                }
                                return;
                            }

                            // Desconexão normal (se não estiver minerando offline nem vendendo)
                            if (client.Player.Associate.Associat.ContainsKey(Role.Instance.Associate.Friends))
                            {
                                foreach (var fr in client.Player.Associate.Associat[Role.Instance.Associate.Friends].Values)
                                {
                                    Client.GameClient gameClient;
                                    if (Database.Server.GamePoll.TryGetValue(fr.UID, out gameClient))
                                    {
                                        gameClient.SendSysMesage("Your friend " + client.Player.Name + " has logged off.", (Game.MsgServer.MsgMessage.ChatMode)2005);
                                    }
                                    else
                                    {
                                        Console.WriteLine($"[{DateTime.Now}] Friend with UID {fr.UID} not found in GamePoll.");
                                    }
                                }
                            }

                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();

                                try
                                {
                                    if (client.Player.ContainFlag(MsgUpdate.Flags.XPList))
                                    {
                                        client.Player.RemoveFlag(MsgUpdate.Flags.XPList);
                                    }

                                    client.Player.ClearFlags();

                                    foreach (var bot in Bots.BotProcessring.Bots.Values.Where(x => x.Bot.Player.Map == client.Player.Map || x.Bot.Player.DynamicID == client.Player.DynamicID))
                                    {
                                        if (bot != null)
                                        {
                                            bot.Dispose();
                                            Console.WriteLine($"[{DateTime.Now}] Bot disposed in map {client.Player.Map} or dynamic ID {client.Player.DynamicID}.");
                                        }
                                    }

                                    if (client.Team != null)
                                    {
                                        client.Team.Remove(client, true);
                                        Console.WriteLine($"[{DateTime.Now}] Client removed from team.");
                                    }

                                    if (client.IsVendor)
                                    {
                                        client.MyVendor.StopVending(stream);
                                        Console.WriteLine($"[{DateTime.Now}] Vendor stopped.");
                                    }

                                    if (client.InTrade)
                                    {
                                        client.MyTrade.CloseTrade();
                                        Console.WriteLine($"[{DateTime.Now}] Trade closed.");
                                    }

                                    if (client.Player.MyGuildMember != null)
                                    {
                                        client.Player.MyGuildMember.IsOnline = false;
                                        Console.WriteLine($"[{DateTime.Now}] Guild member set offline.");
                                    }

                                    if (client.Pet != null)
                                    {
                                        client.Pet.DeAtach(stream);
                                        Console.WriteLine($"[{DateTime.Now}] Pet detached.");
                                    }

                                    if (client.Player.ObjInteraction != null)
                                    {
                                        client.Player.InteractionEffect.AtkType = Game.MsgServer.MsgAttackPacket.AttackID.InteractionStopEffect;
                                        InteractQuery action = InteractQuery.ShallowCopy(client.Player.InteractionEffect);
                                        client.Send(stream.InteractionCreate(&action));

                                        client.Player.ObjInteraction.Player.OnInteractionEffect = false;
                                        client.Player.ObjInteraction.Player.ObjInteraction = null;
                                        Console.WriteLine($"[{DateTime.Now}] Interaction stopped.");
                                    }

                                    client.Player.View.Clear(stream);
                                    Console.WriteLine($"[{DateTime.Now}] Player view cleared.");
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine($"[{DateTime.Now}] Exception during cleanup: {e}");
                                    client.Player.View.Clear(stream);
                                }
                                finally
                                {
                                    client.ClientFlag &= ~Client.ServerFlag.LoginFull;
                                    client.ClientFlag |= Client.ServerFlag.Disconnect;
                                    client.ClientFlag |= Client.ServerFlag.QueuesSave;
                                    Database.ServerDatabase.LoginQueue.TryEnqueue(client);
                                    Console.WriteLine($"[{DateTime.Now}] Client flags updated and enqueued for saving.");
                                }

                                try
                                {
                                    client.Player.Associate.OnDisconnect(stream, client);

                                    if (client.Player.MyMentor != null)
                                    {
                                        Client.GameClient me;
                                        client.Player.MyMentor.OnlineApprentice.TryRemove(client.Player.UID, out me);
                                        client.Player.MyMentor = null;
                                        Console.WriteLine($"[{DateTime.Now}] Mentor association removed.");
                                    }
                                    client.Player.Associate.Online = false;
                                    lock (client.Player.Associate.MyClient)
                                        client.Player.Associate.MyClient = null;
                                    foreach (var clien in client.Player.Associate.OnlineApprentice.Values)
                                    {
                                        clien.Player.SetMentorBattlePowers(0, 0);
                                        Console.WriteLine($"[{DateTime.Now}] Mentor battle powers reset for apprentice.");
                                    }
                                    client.Player.Associate.OnlineApprentice.Clear();
                                    client.Map?.Denquer(client);
                                    Console.WriteLine($"[{DateTime.Now}] Client denqueued from map.");
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine($"[{DateTime.Now}] Exception during additional disconnect actions: {e}");
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[{DateTime.Now}] Client with UID {obj.Game.Player.UID} not found in GamePoll.");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[{DateTime.Now}] General exception: {e}");
                }
            }
            else if (obj.Game != null)
            {
                if (obj.Game.ConnectionUID != 0)
                {
                    Client.GameClient client;
                    if (Database.Server.GamePoll.TryRemove(obj.Game.ConnectionUID, out client))
                    {
                        Console.WriteLine($"[{DateTime.Now}] Client with UID {obj.Game.ConnectionUID} removed from GamePoll.");
                    }
                    else
                    {
                        Console.WriteLine($"[{DateTime.Now}] Failed to remove client with UID {obj.Game.ConnectionUID} from GamePoll.");
                    }
                }
            }
        }

        public static bool NameStrCheck(string name, bool ExceptedSize = true)
        {
            if (name == null)
                return false;
            if (name == "")
                return false;
            string ValidChars = "[^A-Za-z0-9ء-ي*~.&.$]$";
            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(ValidChars);
            if (r.IsMatch(name))
                return false;
            if (name.ToLower().Contains("none"))
                return false;
            if (name.ToLower().Contains("kosomk"))
                return false;
            if (name.ToLower().Contains("fuck"))
                return false;
            if (name.ToLower().Contains("semp"))
                return false;
            if (name.ToLower().Contains("semper"))
                return false;
            if (name.ToLower().Contains("semperfoo"))
                return false;
            if (name.ToLower().Contains("OrigensCO"))
                return false;
            if (name.ToLower().Contains("fuckers"))
                return false;
            if (name.ToLower().Contains("bitchass"))
                return false;
            if (name.ToLower().Contains("Vs"))
                return false;
            if (name.ToLower().Contains("gm"))
                return false;
            if (name.ToLower().Contains("pm"))
                return false;
            if (name.ToLower().Contains("p~m"))
                return false;
            if (name.ToLower().Contains("p!m"))
                return false;
            if (name.ToLower().Contains("g~m"))
                return false;
            if (name.ToLower().Contains("g!m"))
                return false;
            if (name.ToLower().Contains("help"))
                return false;
            if (name.ToLower().Contains("desk"))
                return false;
            if (name.ToLower().Contains("test"))
                return false;
            if (name.ToLower().Contains("SuckMyDuck"))
                return false;
            if (name.ToLower().Contains("suckit"))
                return false;
            if (name.ToLower().Contains("bitch"))
                return false;
            if (name.ToLower().Contains("Ass"))
                return false;
            if (name.Contains("/"))
                return false;
            if (name.Contains(@"\"))
                return false;
            if (name.Contains(@"'"))
                return false;
            //    if (name.Contains('#'))
            //      return false;
            if (name.Contains("GM") ||
                name.Contains("PM") ||
                name.Contains("SYSTEM") ||
                name.Contains("{") || name.Contains("}") || name.Contains("[") || name.Contains("]"))
                return false;
            if (name.Length > 16 && ExceptedSize)
                return false;
            for (int x = 0; x < name.Length; x++)
                if (name[x] == 25)
                    return false;
            return true;
        }
        public static bool StringCheck(string pszString)
        {
            for (int x = 0; x < pszString.Length; x++)
            {
                if (pszString[x] > ' ' && pszString[x] <= '~')
                    return false;
            }
            return true;
        }

        public static string LogginKey = "DR654dt34trg4UI6";

        public static int ExpBallsDropped;
        public static int Plus8, Super2Soc, Super1Soc, SuperNoSoc;

    }
}
