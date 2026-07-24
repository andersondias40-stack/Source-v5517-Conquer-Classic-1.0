using COServer.Database;
using COServer.EventsLib;
using COServer.Game.MsgNpc;
using COServer.Game.MsgServer;
using COServer.Game.MsgServer.AttackHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace COServer.Game.MsgTournaments
{
    public class MsgSchedules
    {
        public static Time32 Stamp = Time32.Now.AddMilliseconds(KernelThread.TournamentsStamp);
        public static Dictionary<TournamentType, ITournament> Tournaments = new Dictionary<TournamentType, ITournament>();
        public static ITournament CurrentTournament;
        internal static GuildSurvival GuildSurvival;
        internal static DateTime LastClassPKStart = DateTime.MinValue;
        internal static Fivenout FiveNOut;
        internal static MataMata eventMataMata;
        internal static bool DisCityInvitationSent = false; // Variável de controle 
        #region PoleDomination
        internal static MsgPoleDomination PoleDomination;
        internal static MsgPoleDominationBI PoleDominationBI;
        internal static MsgPoleDominationDC PoleDominationDC;
        internal static MsgPoleDominationPC PoleDominationPC;
        #endregion
        internal static ExtremeFlagWar _ExtremeFlagWar;
        internal static EliteGuildWar _EliteGuildWar;
        //internal static FirePoleWar _FirePoleWar;
        internal static MsgGuildWar GuildWar;
        internal static MsgClassPKWar ClassPkWar;
        internal static MsgCouples CouplesPKWar;
        internal static MsgPkWar PkWar;
        internal static MsgDisCity DisCity;
        internal static MsgMonster.BossesBase Bosses;
        internal static MsgSquama Squama;
        //internal static Ss_Fb _Ss_Fb;
        //internal static ConquerPk _ConquerPk;
        internal static LastMan _LastMan;
        internal static Get5Out _Get5Out;
        internal static LuckyBox _LuckyBox;
        internal static NobilityWar _NobilityWar;
        internal static GenderWar _GenderWar;
        //internal static Top_Black _Top_Black;
        //internal static FirstKiller _FirstKiller;
        //internal static ArenaDuel _ArenaDuel;
        //internal static MsgCityWarAnimation CityWarAnimation;
        //internal static MsgCityWar CityWar;
        internal static MsgDragonIsland DragonIsland;
        internal static ProjectControl PlayerTop;
        public static bool SpawnDevil = false;
        static bool pkDeathMatchStarted = false;
        internal static bool SuperDropStarted = false;
        internal static void Create()
        {
            #region PoleDomination
            PoleDomination = new MsgPoleDomination();
            PoleDominationBI = new MsgPoleDominationBI();
            PoleDominationDC = new MsgPoleDominationDC();
            PoleDominationPC = new MsgPoleDominationPC();
            #endregion

            GuildSurvival = new GuildSurvival();
            FiveNOut = new Fivenout();
            eventMataMata = new MataMata();
            _ExtremeFlagWar = new ExtremeFlagWar();
            _EliteGuildWar = new EliteGuildWar();
            //_FirePoleWar = new FirePoleWar();
            //_Ss_Fb = new Ss_Fb();
            //_ConquerPk = new ConquerPk();
            _LastMan = new LastMan();
            _NobilityWar = new NobilityWar();
            _GenderWar = new GenderWar();
            _LuckyBox = new LuckyBox();
            _Get5Out = new Get5Out();
            //_Top_Black = new Top_Black();
            //_FirstKiller = new FirstKiller();
            //_ArenaDuel = new ArenaDuel();
            Tournaments.Add(TournamentType.None, new MsgNone(TournamentType.None));
            Tournaments.Add(TournamentType.TreasureThief, new MsgTreasureChests(TournamentType.TreasureThief));
            Tournaments.Add(TournamentType.DBShower, new MsgDBShower(TournamentType.DBShower));
            CurrentTournament = Tournaments[TournamentType.None];
            GuildWar = new MsgGuildWar();
            ClassPkWar = new MsgClassPKWar(ProcesType.Dead);
            PkWar = new MsgPkWar();
            CouplesPKWar = new MsgCouples();
            DisCity = new MsgDisCity();
            Squama = new MsgSquama();
            //CityWarAnimation = new MsgCityWarAnimation();
            DragonIsland = new MsgDragonIsland(ProcesType.Dead);
            MsgBroadcast.Create();
            PlayerTop = new ProjectControl();
        }
        internal static void SendInvitation(string Name, ushort X, ushort Y, ushort map, ushort DinamicID, int Seconds, Game.MsgServer.MsgStaticMessage.Messages messaj = Game.MsgServer.MsgStaticMessage.Messages.None)
        {
            string Message = " " + Name + " is about to begin! Will you join it?";
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Program.SendGlobalPackets.Enqueue(new MsgMessage($"{Name} has started!", MsgMessage.MsgColor.yellow, MsgMessage.ChatMode.TopLeftSystem).GetArray(rec.GetStream()));

                var packet = new Game.MsgServer.MsgMessage(Message, MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream);
                foreach (var client in Database.Server.GamePoll.Values)
                {
                    client.Send(packet);
                    client.Player.MessageBox(Message, new Action<Client.GameClient>(user => user.Teleport(X, Y, map, DinamicID)), null, Seconds, messaj);
                }
            }

            // Envia uma mensagem para o Discord informando que o evento começou
            Program.DiscordAPIevents.Enqueue($"``{Name} has started!``");
            Program.DiscordAPI.Enqueue($"`` Server Online ``");
        }

        internal unsafe static void SendSysMesage(string Messaj, Game.MsgServer.MsgMessage.ChatMode ChatType = Game.MsgServer.MsgMessage.ChatMode.TopLeft, Game.MsgServer.MsgMessage.MsgColor color = Game.MsgServer.MsgMessage.MsgColor.white, bool SendScren = false)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                var packet = new Game.MsgServer.MsgMessage(Messaj, color, ChatType).GetArray(stream);
                foreach (var client in Database.Server.GamePoll.Values)
                    client.Send(packet);
            }
        }
        static List<string> SystemMsgs = new List<string>() {
            "Guild War will begin at 17:00 on Saturdays, and will end at 17:00 on Sundays.",
            "Join our discord server to be in touch with the community and suggest/report stuff.",
            " /rolescale /scale /trash use the commands!",
            "Administrators have [GM/PM] in their names, do not trust anyone else claiming to be a [GM].",
            "Thanks for supporting Discord! We will keep on working to provide the best for our players!",
            "Remember to vote! By voting you're helping to increase the community and you can earn some cool rewards for it!",
            "Top Weekly Voter will receive 3 SurpriseBox."
        };
        internal static void CheckUp(Time32 clock)
        {
            if (clock > Stamp)
            {

                try
                {
                    DateTime Now64 = DateTime.Now;
                    if (!Database.Server.FullLoading)
                        return;

                    // Dentro do método CheckUp, modifique o bloco do SuperDrop:
                    bool isSuperDropTime = (Now64.Hour == 0 && Now64.Minute >= 0 && Now64.Minute < 10) ||
                                           (Now64.Hour == 12 && Now64.Minute >= 30 && Now64.Minute < 40);

                    if (isSuperDropTime)
                    {
                        if (!SuperDropStarted)
                        {
                            // Mensagem de início do SuperDrop (com atraso de 10 segundos)
                            Task.Run(async () =>
                            {
                                await Task.Delay(10000); // Atraso de 10 segundos
                                SendSysMesage("SuperDrop começou vá até o NPC em TwinCity, duração do evento 10minutos!", MsgMessage.ChatMode.Center, MsgMessage.MsgColor.red);
                            });

                            Program.DiscordAPIevents.Enqueue($"``SuperDrop has started!``");
                            SuperDropStarted = true;
                        }
                    }
                    else
                    {
                        if (SuperDropStarted)
                        {
                            // Mensagem de término e teleporte
                            var superDropPlayers = Database.Server.GamePoll.Values
                                .Where(c => c.Player.Map == 1572).ToList();

                            foreach (var client in superDropPlayers)
                            {
                                client.Teleport(428, 378, 1002); // Twin City
                                client.SendSysMesage("O SuperDrop terminou!", MsgMessage.ChatMode.TopLeft);
                            }

                            SendSysMesage("O SuperDrop terminou!", MsgMessage.ChatMode.Center, MsgMessage.MsgColor.red);
                            SuperDropStarted = false; // Finaliza o evento
                        }
                    }
                    // Reseta o ClassPK se o dia mudou
                    if (LastClassPKStart.Date != Now64.Date && LastClassPKStart != DateTime.MinValue)
                    {
                        ClassPkWar.Stop(); // Reseta o torneio para o próximo dia
                    }

                    if ((Now64.Hour == 12 && Now64.Minute == 30 || Now64.Hour == 0 && Now64.Minute == 30))
                    {
                        Console.WriteLine($"Tentando abrir DisCity em {Now64}");
                        DisCity.Open();
                    }
                    CurrentTournament.CheckUp();
                    DisCity.CheckUp();
                    PkWar.CheckUp();
                    CouplesPKWar.CheckUp();
                    //_Ss_Fb.CheckUp();
                    //_ConquerPk.CheckUp();
                    _LastMan.CheckUp();
                    _NobilityWar.CheckUp();
                    //_FirePoleWar.CheckUp();
                    _GenderWar.CheckUp();
                    _ExtremeFlagWar.CheckUp();
                    _EliteGuildWar.CheckUp();
                    //_ArenaDuel.CheckUp();
                    PoleDomination.CheckUp();
                    PoleDominationPC.CheckUp();
                    PoleDominationDC.CheckUp();
                    PoleDominationBI.CheckUp();
                    if (Now64.Hour == 20 && Now64.Minute == 0 && Now64.Second == 0)
                    {
                        Squama.Open();
                    }
                    Game.MsgMonster.BossesBase.BossesTimer();
                    //if (DateTime.Now.Hour == 21 && DateTime.Now.Minute == 1 || DateTime.Now.Hour == 9 && DateTime.Now.Minute == 1)
                    //    CityWar.Open();
                    //CityWar.CheckUp(Now64);
                    if (Now64.Minute % 10 == 0 && Now64.Second > 58 && CurrentTournament.Process == ProcesType.Dead)
                    {
                        var rndMsg = SystemMsgs[Program.GetRandom.Next(0, SystemMsgs.Count)];
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            Program.SendGlobalPackets.Enqueue(new MsgServer.MsgMessage(rndMsg, "ALLUSERS", "Server", MsgServer.MsgMessage.MsgColor.white, MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                        }
                    }
                    //1800000
                    if (Now64 > EventManager.TimeEvent.AddMinutes(30))
                    {
                        EventManager.TimeEvent = Now64;
                        EventManager.CountEvent++;
                        Console.WriteLine("Iniciar evento: " + EventManager.CountEvent);
                        switch (EventManager.CountEvent)
                        {

                            case 2:
                                {
                                    EventsLib.EventManager.freezewar.LastSpawn = DateTime.Now;
                                    EventsLib.EventManager.freezewar.senton = DateTime.Now;
                                    EventsLib.EventManager.SetEvent(EventsLib.EventManager.freezewar.name,
                                        EventsLib.EventManager.freezewar.map);
                                    EventsLib.EventManager.freezewar.SendInvitation();
                                    Program.DiscordAPIevents.Enqueue($"``Freezewar has started!``");
                                    break;
                                }
                            case 3:
                                {
                                    EventsLib.EventManager.guildsdm.senton = DateTime.Now;
                                    EventsLib.EventManager.SetEvent(EventsLib.EventManager.guildsdm.name,
                                        EventsLib.EventManager.guildsdm.map);
                                    EventsLib.EventManager.guildsdm.SendInvitation();
                                    Program.DiscordAPIevents.Enqueue($"``GuildDeathmatch has started!``");
                                    break;
                                }
                            //case 4:
                            //    {
                            //        EventsLib.EventManager.kingofthehill.LastSpawn = DateTime.Now;
                            //        EventsLib.EventManager.kingofthehill.senton = DateTime.Now;
                            //        EventsLib.EventManager.SetEvent(EventsLib.EventManager.kingofthehill.name,
                            //            EventsLib.EventManager.kingofthehill.map);
                            //        EventsLib.EventManager.kingofthehill.SendInvitation();
                            //        break;
                            //    }
                            //case 4:
                            //    {
                            //        EventsLib.EventManager.ctb.LastSpawn = DateTime.Now;
                            //        EventsLib.EventManager.ctb.senton = DateTime.Now;
                            //        EventsLib.EventManager.SetEvent(EventsLib.EventManager.ctb.name,
                            //            EventsLib.EventManager.ctb.map);
                            //        EventsLib.EventManager.ctb.SendInvitation();
                            //        Program.DiscordAPIevents.Enqueue($"``CaptureTheBag has started!``");
                            //        break;
                            //    }
                            case 4:
                                {
                                    EventsLib.EventManager.deathmatch.LastSpawn = DateTime.Now;
                                    EventsLib.EventManager.deathmatch.senton = DateTime.Now;
                                    EventsLib.EventManager.SetEvent(EventsLib.EventManager.deathmatch.name,
                                        EventsLib.EventManager.deathmatch.map);
                                    EventsLib.EventManager.deathmatch.SendInvitation();
                                    Program.DiscordAPIevents.Enqueue($"``Deathmatch has started!``");

                                    break;
                                }
                            case 5:
                                {
                                    EventsLib.EventManager.killthecaptain.LastSpawn = DateTime.Now;
                                    EventsLib.EventManager.killthecaptain.senton = DateTime.Now;
                                    EventsLib.EventManager.SetEvent(EventsLib.EventManager.killthecaptain.name,
                                        EventsLib.EventManager.killthecaptain.map);
                                    EventsLib.EventManager.killthecaptain.SendInvitation();
                                    Program.DiscordAPIevents.Enqueue($"``Kill the captain has started!``");
                                    break;
                                }
                            case 6:
                                {
                                    EventsLib.EventManager.killhunted.LastSpawn = DateTime.Now;
                                    EventsLib.EventManager.killhunted.senton = DateTime.Now;
                                    EventsLib.EventManager.SetEvent(EventsLib.EventManager.killhunted.name,
                                        EventsLib.EventManager.killhunted.map);
                                    EventsLib.EventManager.killhunted.SendInvitation();
                                    Program.DiscordAPIevents.Enqueue($"``Kill hunted has started!``");
                                    break;
                                }

                            //case 9:
                            //    {
                            //        EventsLib.EventManager.teamfreezewar.LastSpawn = DateTime.Now;
                            //        EventsLib.EventManager.teamfreezewar.senton = DateTime.Now;
                            //        EventsLib.EventManager.SetEvent(EventsLib.EventManager.teamfreezewar.name,
                            //            EventsLib.EventManager.teamfreezewar.map);
                            //        EventsLib.EventManager.teamfreezewar.SendInvitation();
                            //        break;
                            //    }
                            //case 10:
                            //    {
                            //        EventsLib.EventManager.dragonwar.LastSpawn = DateTime.Now;
                            //        EventsLib.EventManager.dragonwar.senton = DateTime.Now;
                            //        EventsLib.EventManager.SetEvent(EventsLib.EventManager.dragonwar.name,
                            //            EventsLib.EventManager.dragonwar.map);
                            //        EventsLib.EventManager.dragonwar.SendInvitation();
                            //        break;
                            //    }
                            default:
                                EventManager.CountEvent = 1;
                                break;
                        }
                    }
                    #region Days
                    #region GuildSurvival 21:00
                    if (Now64.Hour == 21 && Now64.Minute == 00 && Now64.Second == 0)
                    {
                        if (GuildSurvival.Process == ProcesType.Dead)
                        {
                            SendInvitation("GuildSurvival", 458, 352, 1002, 0, 60, MsgServer.MsgStaticMessage.Messages.GuildSurvival);
                            GuildSurvival.Open();
                        }
                    }
                    GuildSurvival.CheckUp();
                    #endregion
                    #region FiveAndOut 19:00
                    if (Now64.Hour == 19 && Now64.Minute == 0 && Now64.Second == 0)
                    {
                        if (FiveNOut.Process == ProcesType.Dead)
                        {
                            SendInvitation("FiveNOut", 450, 353, 1002, 0, 60, MsgServer.MsgStaticMessage.Messages.Fiveout);
                            FiveNOut.Open();
                        }
                    }
                    eventMataMata.CheckUp();
                    FiveNOut.CheckUp();
                    #endregion
                    #region PKDeathMatch 20:00
                    if (Now64.Hour == 20 && (Now64.Minute >= 0 && Now64.Minute <= 15))
                    {
                        if (!PkWar.AllowJoin() && !pkDeathMatchStarted)
                        {
                            pkDeathMatchStarted = true; // Marca que já foi iniciado
                            PkWar.Open();
                            SendInvitation("PKDeathMatch", 439, 362, 1002, 0, 60, MsgServer.MsgStaticMessage.Messages.PKDeathMatch);
                        }
                    }
                    else
                    {
                        pkDeathMatchStarted = false; // Reseta flag quando o horário do evento termina
                    }
                    #endregion
                    #region GuildWar
                    if (Now64.DayOfWeek == DayOfWeek.Sunday)
                    {
                        
                        if (Now64.Hour >= 14 && Now64.Hour < 17)
                        {
                            if (GuildWar.Proces == ProcesType.Dead)
                            {

                                GuildWar.Start();
                            }
                            if (GuildWar.Proces == ProcesType.Idle)
                            {
                                if (Now64 > GuildWar.StampRound)
                                    GuildWar.Began();
                            }
                            if (GuildWar.Proces != ProcesType.Dead)
                            {
                                if (DateTime.Now > GuildWar.StampShuffleScore)
                                {
                                    GuildWar.ShuffleGuildScores();
                                }
                            }
                            if (Now64.Hour == MsgGuildWar.FlameActive)
                            {
                                if (GuildWar.FlamesQuest.ActiveFlame10 == false)
                                {
                                    SendSysMesage("The Flame Stone 9 is Active now. Light up the Flame Stone (62,59) near the Stone Pole in the Guild City.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.white);
                                    GuildWar.FlamesQuest.ActiveFlame10 = true;
                                }
                            }
                            else if (GuildWar.SendInvitation == false && Now64.Hour == 9)
                            {
                                SendInvitation("GuildWar", 200, 254, 1038, 0, 60, MsgServer.MsgStaticMessage.Messages.GuildWar);
                                GuildWar.SendInvitation = true;
                            }

                        }
                        else if (Now64.Hour >= 17 && Now64.Minute == 0 && Now64.Second < 5)
                        {
                            if (GuildWar.Proces == ProcesType.Alive || GuildWar.Proces == ProcesType.Idle)
                                GuildWar.CompleteEndGuildWar();
                        }
                        
                    }
                    #endregion
                    #region TournamentType
                    if (Now64.Minute == 20)
                    {
                        if (CurrentTournament.Process == ProcesType.Dead)
                        {
                            CurrentTournament = Tournaments[TournamentType.TreasureThief];
                            CurrentTournament.Open();
                            Console.WriteLine("Started Tournament " + CurrentTournament.Type.ToString(), ConsoleColor.Yellow);
                        }
                    }

                    #endregion 
                    #region ClassPK 22:00
                    if (Now64.Hour == 22 && Now64.Minute == 0)
                    {
                        // Verifica se o evento ainda não foi disparado hoje
                        if (LastClassPKStart.Date != Now64.Date)
                        {
                            SendInvitation("ClassPk", 429, 242, 1002, 0, 60, MsgServer.MsgStaticMessage.Messages.ClassPk);
                            ClassPkWar.Stop(); // Para o torneio atual, se estiver rodando
                            ClassPkWar.Start(); // Inicia o torneio com base no dia atual
                            LastClassPKStart = Now64; // Atualiza a última execução
                        }
                    }
                    if (Now64.Hour == 22 && Now64.Minute >= 10)
                    {
                        foreach (var war in ClassPkWar.PkWars)
                            foreach (var map in war)
                            {
                                var players_in_map = Database.Server.GamePoll.Values.Where(e => e.Player.DynamicID == map.DinamicID && e.Player.Alive);
                                if (players_in_map.Count() == 1)
                                {
                                    var winner = players_in_map.SingleOrDefault();
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        map.GetMyReward(winner, stream);
                                    }
                                }
                            }
                    }
                    #endregion 
                }
                #endregion
                catch (Exception e)
                {
                    Console.SaveException(e);
                }
                Stamp.Value = clock.Value + KernelThread.TournamentsStamp;
            }
        }
    }
}
