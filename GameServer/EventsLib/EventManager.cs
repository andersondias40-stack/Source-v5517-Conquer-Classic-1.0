using COServer.Client;
using COServer.Game.MsgServer.AttackHandler.ReceiveAttack;
using COServer.Role;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COServer.EventsLib
{
    class EventManager
    {
        public static Random rnd = new Random();
        public static DateTime EventsCheck;
        public static string NextEvent = "";
        static uint NextMap = 0;
        static DateTime signUp;
        public static DateTime TimeEvent;
        static int KillTheCapTeams = 0;
        static bool EventStarted = true;
        public static int ExpHour = 0, DBHour = 0;
        public static int CountDown = 0;
        public static bool
            EventDoubleExp = false,
            EventDoubleDB = false;

        //public static PassTheBomb passthebombEv = new PassTheBomb();
        public static KingOfTheHill kingofthehill = new KingOfTheHill();
        public static FreezeWar freezewar = new FreezeWar();
        public static CaptureTheBag ctb = new CaptureTheBag();
        public static DeathMatch deathmatch = new DeathMatch();
        public static KillTheCaptain killthecaptain = new KillTheCaptain();
        public static KillTheHunted killhunted = new KillTheHunted();
        public static GuildsDeathMatch guildsdm = new GuildsDeathMatch();
        public static TeamFreezeWar teamfreezewar = new TeamFreezeWar();
        public static DragonWar dragonwar = new DragonWar();
        public static uint CountEvent = 1;
        /// <summary>
        /// RndCoordinates
        /// </summary>
        /// return player alive
        /// with an X & Y have benn edit
        /// random revive
        /// <param name="Map"></param>
        /// <param name="chaa"></param>
        public static void RndCoordinates(uint Map, Client.GameClient chaa)
        {
            ushort X = 0, Y = 0;
            if (Map == killthecaptain.map)
                chaa.Teleport((ushort)rnd.Next(80, 110), (ushort)rnd.Next(80, 110), killthecaptain.map);
            else if (Map == guildsdm.map)
            {
                if (MyMath.ChanceSuccess(50))
                    chaa.Teleport((ushort)rnd.Next(200, 300), (ushort)rnd.Next(200, 300), guildsdm.map);
                else
                    chaa.Teleport((ushort)rnd.Next(200, 300), (ushort)rnd.Next(200, 300), guildsdm.map);
            }
            else if (Map == killhunted.map)
            {
                ushort x = 0, y = 0;
                switch (rnd.Next(0, 5))
                {
                    case 0:
                        x = 102;
                        y = 168;

                        break;
                    case 1:
                        x = 155;
                        y = 200;
                        break;
                    case 2:
                        x = 175;
                        y = 221;
                        break;
                    case 3:
                        x = 217;
                        y = 190;
                        break;
                    case 4:
                        x = 216;
                        y = 163;
                        break;
                    case 5:
                        x = 178;
                        y = 130;
                        break;


                }
                chaa.Teleport(x, y, Map);
            }
            else
            {
                switch (rnd.Next(0, 12))
                {
                    default:
                    case 0:
                        X = 35;
                        Y = 65;
                        break;
                    case 1:
                        X = 36;
                        Y = 45;
                        break;
                    case 2:
                        X = 34;
                        Y = 32;
                        break;
                    case 3:
                        X = 36;
                        Y = 45;
                        break;
                    case 4:
                        X = 50;
                        Y = 49;
                        break;
                    case 5:
                        X = 45;
                        Y = 56;
                        break;
                    case 6:
                        X = 58;
                        Y = 59;
                        break;
                    case 7:
                        X = 60;
                        Y = 42;
                        break;
                    case 8:
                        X = 67;
                        Y = 33;
                        break;
                    case 9:
                        X = 64;
                        Y = 47;
                        break;
                    case 10:
                        X = 65;
                        Y = 68;
                        break;
                    case 12:
                    case 11:
                        X = 52;
                        Y = 65;
                        break;


                }
                chaa.Teleport(X, Y, Map);
            }
        }

        // ID DO MAPA ORIGINAL DE EVENTO: 1767
        public static void SetEvent(string Name, uint Map)
        {
            //if (Name == passthebombEv.name)
            //    passthebombEv.isChosen = false;
            signUp = DateTime.Now;
            NextEvent = Name;
            NextMap = Map;
            CountDown = 60;
            EventStarted = false;
            if (!Program.FreePkMap.Contains(Map))
                Program.FreePkMap.Add(Map);
            if (!Program.FreePkMap.Contains(1767))
                Program.FreePkMap.Add(1767);
            if (!Program.BlockAttackMap.Contains(1767))
                Program.BlockAttackMap.Add(1767);
            if (!Program.BlockAttackMap.Contains(1601))
                Program.BlockAttackMap.Add(1601);
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();

                Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("The signups for " + NextEvent + " has started. Type /pvp to signup.", "ALLUSERS", "PVPEvents", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.TopLeft).GetArray(stream));
            }
            if (NextEvent == dragonwar.name)
                dragonwar.ChooseKing = true;

            Console.WriteLine("Start event " + NextEvent + "!", ConsoleColor.Yellow);
        }
        public static void MapRules(GameClient player)
        {
            if (NextMap == deathmatch.map)//1
            {
                player.DMTeam = deathmatch.NextTeam();
                player.DMScore = 0;
            }
            if (NextMap == kingofthehill.map)//2
                player.KingOfTheHill = 0;

            player.Player.HitPoints = (int)player.Status.MaxHitpoints;


            //else if (NextMap == passthebombEv.map)//4
            //{
            //    if (!passthebombEv.isChosen)
            //    {
            //        passthebombEv.isChosen = true;
            //        player.HasTheBomb = true;
                //}
            //}
            if (NextMap == freezewar.map)//5
                player.FreezewarPoints = 0;
            else if (NextMap == ctb.map)//no
                player.CTBScore = 0;
            else if (NextMap == 9020)//no
                player.ElitePKPoints = 0;
            else if (NextMap == dragonwar.map)//6
            {
                player.DragonwarPts = 0;
                player.isDragonKing = false;
            }
            else if (NextMap == killthecaptain.map)//7
            {
                if (KillTheCapTeams % 2 == 0)
                    player.TeamKillTheCaptain = KillTheCaptainTeams.Blue;
                else
                    player.TeamKillTheCaptain = KillTheCaptainTeams.Red;
                KillTheCapTeams++;
            }
            else if (NextMap == teamfreezewar.map)//8
            {
                player.TeamFreeze = (FreezeWarTeams)teamfreezewar.NextTeam();

                player.SendSysMesage("You're in the " + player.TeamFreeze.ToString() + " team.");
            }
        }
        public static void JoinPVP(GameClient player)
        {
            // TODO : Checks for teleport
            if (player.Player.Map == 6003 || player.Player.Map == 6001 || player.Player.Map == 6000) return;// Botjail
            if (NextEvent == "DeathMatch" && !player.Inventory.HaveSpace(1))
            {
                player.SendSysMesage("You need 1 free spot in your inventory for your garment", (Game.MsgServer.MsgMessage.ChatMode)2005);
                return;
            }
            
            if (!EventStarted)
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    player.MySpells.Add(stream, 1045, 4);
                    player.MySpells.Add(stream, 1046, 4);
                }

                player.Teleport(50, 50, 1767);
                player.SendSysMesage("You've signed up! Please wait for 1 minute, and don't teleport or you'll be disqualified.", (Game.MsgServer.MsgMessage.ChatMode)2005);
            }
            else
                player.SendSysMesage("There are no live events!", (Game.MsgServer.MsgMessage.ChatMode)2005);
        }
        public static void Worker()
        {
            try
            {

                if (DateTime.Now > signUp.AddMinutes(1) && !EventStarted && NextMap != 0 && CountDown <= 0)
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        foreach (var C in Database.Server.GamePoll.Values.Where(e => e.Player.Map == 1767))
                        {
                            C.Send(new Game.MsgServer.MsgMessage("", "ALLUSERS", "SYSTEM", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.FirstRightCorner).GetArray(stream));
                            C.Send(new Game.MsgServer.MsgMessage("", "ALLUSERS", "SYSTEM", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.ContinueRightCorner).GetArray(stream));
                        }
                    }
                    EventStarted = true;
                    if (!Program.FreePkMap.Contains(NextMap))
                        Program.FreePkMap.Add(NextMap);
                    int c = Database.Server.GamePoll.Values.Where(e => e.Player.Map == 1767).Count();
                    if (NextEvent == "Death Match" && c < 4)
                    {
                        foreach (var pl in Database.Server.GamePoll.Values.Where(e => e.Player.Map == 1767))
                            pl.Teleport(431, 346, 1002);
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Death Match has been cancelled, because it needs 4 players to start.", "ALLUSERS", "PVPEvents", Game.MsgServer.MsgMessage.MsgColor.white, (Game.MsgServer.MsgMessage.ChatMode)2011).GetArray(stream));
                        }
                    }
                    else if (c < 2)
                    {

                        foreach (var pl in Database.Server.GamePoll.Values.Where(e => e.Player.Map == 1767))
                            pl.Teleport(431, 346, 1002);
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Event has been cancelled, because it needs 2 players to start.", "ALLUSERS", "PVPEvents", Game.MsgServer.MsgMessage.MsgColor.white, (Game.MsgServer.MsgMessage.ChatMode)2011).GetArray(stream));
                        }
                    }
                    else
                    {
                        if (NextEvent == ctb.name)
                            ctb.TeleportPlayersToMap();
                        else
                        {
                            foreach (var pl in Database.Server.GamePoll.Values.Where(e => e.Player.Map == 1767))
                            {
                                pl.Player.RemoveFlag(Game.MsgServer.MsgUpdate.Flags.Fly);
                                if (pl.Player.OnTransform)
                                {
                                    pl.Player.TransformInfo.FinishTransform();
                                }
                                MapRules(pl);
                                RndCoordinates(NextMap, pl);
                            }
                        }
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("The signups for " + NextEvent + " has been disabled.", "ALLUSERS", "PVPEvents", Game.MsgServer.MsgMessage.MsgColor.white, (Game.MsgServer.MsgMessage.ChatMode)2011).GetArray(stream));
                        }
                    }

                    if (NextEvent == guildsdm.name)
                    {
                        foreach (var pl in Database.Server.GamePoll.Values.Where(e => e.Player.Map == 1767))
                        {
                            #region rndcoor
                            ushort X = 0, Y = 0;
                            switch (rnd.Next(0, 6))
                            {
                                default:
                                case 0:
                                    X = 229;
                                    Y = 183;
                                    break;
                                case 1:
                                    X = 255;
                                    Y = 168;
                                    break;
                                case 2:
                                    X = 285;
                                    Y = 211;
                                    break;
                                case 3:
                                    X = 320;
                                    Y = 280;
                                    break;
                                case 4:
                                    X = 293;
                                    Y = 373;
                                    break;
                            }
                            #endregion

                            pl.Teleport(X, Y, NextMap);
                        }

                    }
                }
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    TimeSpan T = TimeSpan.FromSeconds(CountDown);
                    var stream = rec.GetStream();
                    foreach (var C in Database.Server.GamePoll.Values.Where(e => e.Player.Map == 1767))
                    {
                        C.Send(new Game.MsgServer.MsgMessage($"----------{NextEvent}----------", "ALLUSERS", "SYSTEM", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.FirstRightCorner).GetArray(stream));
                        C.Send(new Game.MsgServer.MsgMessage($"Start in : {T.ToString(@"mm\ss")}", "ALLUSERS", "SYSTEM", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.ContinueRightCorner).GetArray(stream));
                    }
                }
                if (CountDown > 0)
                    --CountDown;

                //passthebombEv.worker();
                kingofthehill.worker();
                deathmatch.worker();
                ctb.worker();
                killhunted.worker();
                guildsdm.worker();
                killthecaptain.worker();
                freezewar.worker();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public static void Init()
        {
            GameMap.CreateDynamicMap(1004, 6010, false);
            Program.BlockAttackMap.Add(6010);
            GameMap.CreateDynamicMap(700, dragonwar.map, true);
            GameMap.CreateDynamicMap(1216, 8757, true);
            //GameMap.CreateDynamicMap(700, passthebombEv.map, true);
            GameMap.CreateDynamicMap(700, EventManager.teamfreezewar.map, true);
            GameMap.CreateDynamicMap(700, kingofthehill.map, true);
            GameMap.CreateDynamicMap(700, freezewar.map, true);
            GameMap.CreateDynamicMap(1080, ctb.map, true);
            GameMap.CreateDynamicMap(1844, EventManager.killhunted.map, true);
            GameMap.CreateDynamicMap(700, deathmatch.map, true);
            GameMap.CreateDynamicMap(1767, 9010, true);
            GameMap.CreateDynamicMap(1002, 10200, true);
            GameMap.CreateDynamicMap(1037, EventManager.guildsdm.map, true);
            GameMap.CreateDynamicMap(1002, 10201, true);
            GameMap.CreateDynamicMap(1070, 10202, true);
            GameMap.CreateDynamicMap(1507, EventManager.killthecaptain.map, true);
            GameMap.CreateDynamicMap(1505, 9020, true);
        }
        public static bool ExecuteSkill(uint attktype, ushort skillId, GameClient GC)
        {
            if (GC.Player.Map == dragonwar.map ||
                GC.Player.Map == teamfreezewar.map ||
                GC.Player.Map == kingofthehill.map)
            {
                if (attktype != 24)
                {
                    GC.SendSysMesage("You can use only FB/SS.", (Game.MsgServer.MsgMessage.ChatMode)2000);
                    return false;
                }
                if (attktype == 24 && skillId == 0) return true;
                if (skillId != 1046 && skillId != 1045)
                {
                    GC.SendSysMesage("You can use only FB/SS.", (Game.MsgServer.MsgMessage.ChatMode)2000);
                    return false;
                }
                else
                {
                    //if (GC.Player.Map == passthebombEv.map && !GC.HasTheBomb)
                    {
                        GC.SendSysMesage("You cant attack unless you have the bomb.", (Game.MsgServer.MsgMessage.ChatMode)2001);
                        return false;
                    }

                }
            }
            return true;
        }
        public static void ExecuteAttack(GameClient attacked, GameClient attacker, ref uint Damage)
        {
            if (attacker != null)
                if (attacker.Player.Map == kingofthehill.map)
                    attacker.KingOfTheHill += 5;
            if (attacked.Player.Map == killhunted.map)
            {
                if (!attacker.Hunted && !attacked.Hunted)
                {
                    Damage = 1;
                    attacker.SendSysMesage("You're attacking a non-fugitive player.", (Game.MsgServer.MsgMessage.ChatMode)2005);
                }
            }
            //else if (attacked.Player.Map == passthebombEv.map)
                //passthebombEv.Pass(attacked);
            else if (attacked.Player.Map == teamfreezewar.map)
            {
                if (attacked.TeamFreeze == attacker.TeamFreeze && attacked.Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Freeze))
                {
                    attacked.Player.RemoveFlag(Game.MsgServer.MsgUpdate.Flags.Freeze);
                    attacked.SendSysMesage("Your team removed your transform.", (Game.MsgServer.MsgMessage.ChatMode)2000);

                }
                else if (attacked.TeamFreeze != attacker.TeamFreeze && !attacked.Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Freeze))
                {
                    attacked.Player.AddFlag(Game.MsgServer.MsgUpdate.Flags.Freeze, 60 * 60 * 24 * 30, false);
                    attacked.SendSysMesage("You got transformed by the enemy.", (Game.MsgServer.MsgMessage.ChatMode)2000);

                }
            }
            if (attacked.Player.Map == dragonwar.map)
            {
                if (dragonwar.ChooseKing)
                {
                    dragonwar.ChooseKing = false;
                    attacker.isDragonKing = true;
                    attacker.Player.AddFlag(Game.MsgServer.MsgUpdate.Flags.DragonWar, 60 * 60 * 24 * 30, false);
                }
                else
                {
                    if (attacker.isDragonKing)
                        Damage *= 3;
                    else if (!attacked.isDragonKing)
                        Damage = 1;
                    else
                    {
                        attacker.DragonwarPts += 5;
                        attacked.SendSysMesage("You've got 5 points for attacking the Spell Eater!", (Game.MsgServer.MsgMessage.ChatMode)2000);
                    }
                }
            }
        }
        public static void Die(GameClient killed, GameClient killer)
        {
            if (killer != null)
            {
                Console.WriteLine($"1 - Player {killer.Player.Name} killed {killer.Player.Name} on map {killed.Player.Map}");

                if (killed.Player.Map == dragonwar.map)
                {
                    if (killed.isDragonKing)
                    {
                        killed.Player.RemoveFlag(Game.MsgServer.MsgUpdate.Flags.DragonWar);
                        killed.isDragonKing = false;

                        killer.Player.AddFlag(Game.MsgServer.MsgUpdate.Flags.DragonWar, 60 * 60 * 24 * 30, false);
                        killer.isDragonKing = true;
                        foreach (var player in Database.Server.GamePoll.Values.Where(e => e.Player.Map == dragonwar.map).ToArray())
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                player.Send(new Game.MsgServer.MsgMessage("The Dragon King is " + killer.Player.Name, "ALLUSERS", "Dragon war", Game.MsgServer.MsgMessage.MsgColor.white, (Game.MsgServer.MsgMessage.ChatMode)2000).GetArray(stream));
                            }
                        }
                    }
                }
                if (killed.Player.Map == killthecaptain.map)
                {
                    if (killed.TeamKillTheCaptain != killer.TeamKillTheCaptain)
                    {
                        if (killed.Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Flashy))
                        {
                            killed.Player.RemoveFlag(Game.MsgServer.MsgUpdate.Flags.Flashy);
                            killer.KillTheCaptainPoints += 10;
                            foreach (var player in Database.Server.GamePoll.Values.Where(e => e.Player.Map == dragonwar.map).ToArray())
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    player.Send(new Game.MsgServer.MsgMessage(killer.Player.Name + " has killed the leader of the other team and has gained 10 points.", "ALLUSERS", "Vendetta", Game.MsgServer.MsgMessage.MsgColor.white, (Game.MsgServer.MsgMessage.ChatMode)2000).GetArray(stream));
                                }
                            }
                        }
                        else
                        {
                            killer.KillTheCaptainPoints++;
                            killer.SendSysMesage("You've gained a point for killing an enemy.", (Game.MsgServer.MsgMessage.ChatMode)2000);
                        }
                    }
                }
                if (killed.Player.Map == killhunted.map)
                {
                    killed.Teleport(38, 35, 1006);
                    if (killed.Hunted)
                    {
                        killed.Hunted = false;
                        var list = Database.Server.GamePoll.Values.Where(e => e.Player.Map == killhunted.map && e.Player.Alive);
                        if (list.Count() > 1)
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(killer.Player.Name + " has killed the target and won 100K.", "ALLUSERS", "PrisonOfficer", Game.MsgServer.MsgMessage.MsgColor.white, (Game.MsgServer.MsgMessage.ChatMode)2005).GetArray(stream));
                            }
                            killer.Player.Money += 100000;
                        }
                        killhunted.Pass();
                    }
                }
                if (killer.Player.Map == 9020)
                {
                    killer.ElitePKPoints++;
                }
                if (killed.Player.Map == freezewar.map)
                {
                    if (killed.Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Freeze))
                    {
                        killer.SendSysMesage("You won't get points for attacking a frozen player.", (Game.MsgServer.MsgMessage.ChatMode)2005);
                        return;
                    }
                    killed.FrozenStamp = DateTime.Now;
                    killed.Player.AddFlag(Game.MsgServer.MsgUpdate.Flags.Freeze, 60 * 60 * 24 * 30, false);
                    killed.Player.AddFlag(Game.MsgServer.MsgUpdate.Flags.Fly, 60 * 60 * 24 * 30, false);
                    killer.FreezewarPoints++;
                }
                if (killed.Player.Map == ctb.map)
                {
                    bool _hadbag = false;
                    byte _score = 2;
                    _hadbag = false;
                    if (killed.Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Flashy))
                    {
                        _score = 6;
                        _hadbag = true;
                        killed.Player.RemoveFlag(Game.MsgServer.MsgUpdate.Flags.Flashy);
                        killed.HasBag = false;
                    }
                    killer.CTBScore += _score;

                    if (killed.TeamColor == CTBTeam.Blue)
                    {
                        if (_hadbag)
                        {
                            CaptureTheBag.Red = false;
                            ctb.Broadcast(killed.Player.Name + " from the BlueTeam was killed while holding the RedBag!", BroadCastLoc.Map);
                            ctb.DropRed();
                        }
                        ctb.ScoreRed += _score;
                    }
                    else
                    {
                        if (_hadbag)
                        {
                            CaptureTheBag.Blue = false;
                            ctb.Broadcast(killed.Player.Name + " from the RedTeam was killed while holding the BlueBag!", BroadCastLoc.Map);
                            ctb.DropBlue();
                        }
                        ctb.ScoreBlue += _score;

                    }
                }
                if (killer.Player.Map == deathmatch.map)
                {
                    if (killer.DMTeam != killed.DMTeam)
                    {
                        killer.DMScore++;
                        switch (killer.DMTeam)
                        {
                            case 1:
                                deathmatch.WhiteTeam++;
                                break;
                            case 2:
                                deathmatch.RedTeam++;
                                break;
                            case 3:
                                deathmatch.BlueTeam++;
                                break;
                            case 4:
                                deathmatch.BlackTeam++;
                                break;
                        }
                        killer.SendSysMesage("You've gained 1 point for yourself and your team!", (Game.MsgServer.MsgMessage.ChatMode)2000);
                    }
                    else
                    {
                        if (killer.DMScore > 0)
                        {
                            killer.DMScore--;
                            killer.SendSysMesage("You lost 1 point for killing your teammate!", (Game.MsgServer.MsgMessage.ChatMode)2000);
                        }
                        else killer.SendSysMesage("If you kill your teammate you'll lose 1 point.", (Game.MsgServer.MsgMessage.ChatMode)2000);
                    }
                }
                if (killed.Player.Map == 1038)
                {  
                    if (killer.Player.GuildID != killed.Player.GuildID)
                    {
                        killer.TotalKillsGW++;
                    }
                }
                if (killed.Player.Map == guildsdm.map)
                {
                    killed.Teleport(430, 329, 1002);

                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        killed.Player.Revive(stream);
                    }
                }
            }
        }
    }
}
