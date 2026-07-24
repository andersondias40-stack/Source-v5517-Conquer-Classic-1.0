using COServer.Game.MsgServer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace COServer.Role.Instance
{
    public class Associate
    {

        public const byte Friends = 1, Enemy = 2, Partener = 3, Mentor = 4, Apprentice = 5, PKExplorer = 6;

        public class Member
        {
            public uint UID = 0;
            public ulong Timer = 0;
            public uint ExpBalls = 0;
            public uint Stone = 0;
            public uint Blessing = 0;
            public string Map = "";
            public string Name = "";
            public ushort KillsCount = 0;
            public ushort BattlePower = 0;
            public bool IsOnline { get { return Database.Server.GamePoll.ContainsKey(UID); } }
            public int GetTimerLeft()
            {
                if (Timer == 0)
                    return 0;
                int timer = (int)(new TimeSpan((long)Timer).TotalMinutes - new TimeSpan(DateTime.Now.Ticks).TotalMinutes);
                if (timer <= 0)
                {
                    Timer = 0;
                    return 0;
                }

                return timer;
            }
            public override string ToString()
            {
                Database.DBActions.WriteLine writer = new Database.DBActions.WriteLine('/');
                writer.Add(UID).Add(Timer).Add(ExpBalls).Add(Stone).Add(Blessing).Add(Name).Add(KillsCount).Add(BattlePower).Add(Map);
                return writer.Close();
            }

        }
        public class MyAsociats
        {
            public ConcurrentDictionary<byte, ConcurrentDictionary<uint, Member>> Associat = new ConcurrentDictionary<byte, ConcurrentDictionary<uint, Member>>();

            public ConcurrentDictionary<uint, Client.GameClient> OnlineApprentice = new ConcurrentDictionary<uint, Client.GameClient>();


            public bool HaveAsociats()
            {
                foreach (var items in Associat)
                {
                    if (items.Key != PKExplorer && items.Key != Enemy)
                        if (items.Value.Count > 0)
                            return true;
                }
                return false;
            }

            public bool Online = false;
            public Client.GameClient MyClient;
            public uint MyUID = 0;
            public uint Mentor_Stones = 0;
            public uint Mentor_ExpBalls = 0;
            public uint Mentor_Blessing = 0;

            public MyAsociats(uint uid)
            {
                MyUID = uid;
            }
            public bool OnUse()
            {
                if (MyUID != 0)
                {
                    if (!Associates.ContainsKey(MyUID))
                        Associates.TryAdd(MyUID, this);
                }
                else
                    return false;

                return true;
            }

            public bool Contain(byte Mode, uint UID)
            {
                if (Associat.ContainsKey(Mode))
                {
                    if (Associat[Mode].ContainsKey(UID))
                        return true;
                }
                return false;
            }
            public bool Remove(byte Mode, uint UID)
            {
                if (!Associat.ContainsKey(Mode))
                    return false;
                if (!Associat[Mode].ContainsKey(UID))
                    return false;

                Member mem;
                return Associat[Mode].TryRemove(UID, out mem);
            }
            public bool AllowAdd(byte Mode, uint UID, byte amout)
            {
                if (!Associat.ContainsKey(Mode))
                    return true;
                if (Associat[Mode].ContainsKey(UID))
                    return false;
                if (Associat[Mode].Count < amout)
                    return true;

                return false;
            }
            public void Add(byte mode, Member member)
            {

                if (!OnUse())
                    return;

                if (Associat.ContainsKey(mode))
                    Associat[mode].TryAdd(member.UID, member);
                else
                {
                    Associat.TryAdd(mode, new ConcurrentDictionary<uint, Member>());
                    Associat[mode].TryAdd(member.UID, member);
                }
            }
            public void AddPartener(Client.GameClient Owner, Role.Player client)
            {
                if (AllowAdd(Partener, client.UID, 10))
                {
                    Member member = new Member()
                    {
                        UID = client.UID,
                        Name = client.Name,
                        Timer = (ulong)DateTime.Now.AddDays(3).Ticks
                    };
                    Add(Partener, member);
                }
                else
                {
                    Owner.SendSysMesage("Sorry, your partner's list is full.");

                }
            }
            public void AddAprrentice(Client.GameClient Owner, Role.Player client)
            {
                if (AllowAdd(Apprentice, client.UID, (byte)Database.TutorInfo.AddAppCount(Owner)))
                {
                    if (OnlineApprentice.TryAdd(client.UID, client.Owner))
                    {
                        Member member = new Member()
                        {
                            UID = client.UID,
                            Name = client.Name
                        };
                        Add(Apprentice, member);
                    }
                }
                else
                {
                    Owner.SendSysMesage("Sorry, your apprentice`s list is full.");
                }
            }
            public void AddMentor(Client.GameClient Owner, Role.Player client)
            {
                if (AllowAdd(Mentor, client.UID, 1))
                {
                    Member member = new Member()
                    {
                        UID = client.UID,
                        Name = client.Name
                    };
                    Add(Mentor, member);
                }
            }
            public void AddFriends(Client.GameClient Owner, Role.Player client)
            {
                if (AllowAdd(Friends, client.UID, 50))
                {
                    Member member = new Member()
                    {
                        UID = client.UID,
                        Name = client.Name
                    };
                    Add(Friends, member);
                }
                else
                {
                    Owner.SendSysMesage("Sorry, your friends list is full.");
                }
            }
            public uint GetTimePkExplorer()
            {
                uint valu = 0;
                DateTime timer = DateTime.Now;
                valu = (uint)((timer.Month * 1000000) + (timer.Day * 10000) + (timer.Hour * 100) + timer.Minute);
                return valu;
            }
            public void AddPKExplorer(Client.GameClient Owner, Role.Player client)
            {
                if (!OnUse())
                    return;

                Member member = new Member()
                {
                    UID = client.UID,
                    Timer = GetTimePkExplorer(),
                    BattlePower = (ushort)client.BattlePower,
                    Name = client.Name,
                    Map = GetMapName(client.Map)
                };
                member.Timer = (uint)GetTimePkExplorer();
                member.KillsCount++;
                if (Associat.ContainsKey(PKExplorer))
                {
                    Member Gmem;
                    if (Associat[PKExplorer].TryGetValue(member.UID, out Gmem))
                    {
                        Gmem.Timer = GetTimePkExplorer();
                        Gmem.KillsCount++;
                        Gmem.BattlePower = (ushort)client.BattlePower;
                    }
                    else
                    {
                        if (AllowAdd(PKExplorer, member.UID, 50))
                        {
                            Associat[PKExplorer].TryAdd(member.UID, member);
                        }
                        else
                        {
                            var remover = Associat[PKExplorer].Values.ToArray()[0];
                            Remove(PKExplorer, remover.UID);
                            if (AllowAdd(PKExplorer, member.UID, 50))
                                Add(PKExplorer, member);
                        }
                    }
                }
                else
                {
                    Associat.TryAdd(PKExplorer, new ConcurrentDictionary<uint, Member>());
                    Associat[PKExplorer].TryAdd(member.UID, member);
                }
            }

            public static string GetMapName(uint MapID)
            {
                string Name = "Unknown";
                switch (MapID)
                {
                    case 601:
                        Name = "OfflineTG";
                        break;
                    case 1767:
                        Name = "Event";
                        break;
                    case 700:
                        Name = "LotteryMap";
                        break;
                    case 1000:
                        Name = "Desert";
                        break;
                    case 1001:
                        Name = "MysticCastle";
                        break;
                    case 1002:
                        Name = "CentralPlain";
                        break;
                    case 1003:
                        Name = "MineCave";
                        break;
                    case 1004:
                        Name = "JobCenter";
                        break;
                    case 1005:
                        Name = "Arena";
                        break;
                    case 1006:
                        Name = "Stable";
                        break;
                    case 1007:
                        Name = "Blachsmith";
                        break;
                    case 1008:
                        Name = "Grocery";
                        break;
                    case 1009:
                        Name = "ArmorStore";
                        break;
                    case 1010:
                        Name = "BirthVillage";
                        break;
                    case 1011:
                        Name = "Forest";
                        break;
                    case 1012:
                        Name = "Dreamland";
                        break;
                    case 1013:
                        Name = "TigerCave";
                        break;
                    case 1014:
                        Name = "DragonPool";
                        break;
                    case 1015:
                        Name = "Island";
                        break;
                    case 1016:
                        Name = "KylinCave";
                        break;
                    case 1018:
                        Name = "Arena";
                        break;
                    case 1020:
                        Name = "Canyon";
                        break;
                    case 1021:
                        Name = "CopperMine";
                        break;
                    case 1025:
                        Name = "IronMine";
                        break;
                    case 1026:
                        Name = "CopperMine";
                        break;
                    case 1027:
                        Name = "SilverMine";
                        break;
                    case 1028:
                        Name = "GoldMine";
                        break;
                    case 1036:
                        Name = "Market";
                        break;
                    case 1038:
                        Name = "GuildArea";
                        break;
                    case 1039:
                        Name = "TrainingGround";
                        break;
                    case 1040:
                        Name = "SkyCityPass";
                        break;
                    case 1041:
                        Name = "PrizeClaimingMa";
                        break;
                    case 1042:
                        Name = "PassPortal";
                        break;
                    case 1043:
                        Name = "Peace";
                        break;
                    case 1044:
                        Name = "Chaos";
                        break;
                    case 1045:
                        Name = "Deserted";
                        break;
                    case 1046:
                        Name = "Prosperous";
                        break;
                    case 1047:
                        Name = "Disturbed";
                        break;
                    case 1048:
                        Name = "Calmed";
                        break;
                    case 1049:
                        Name = "Death";
                        break;
                    case 1050:
                        Name = "Life";
                        break;
                    case 1051:
                        Name = "MysticIsland";
                        break;
                    case 1052:
                        Name = "TestIsland";
                        break;
                    case 1060:
                        Name = "Maze1";
                        break;
                    case 1061:
                        Name = "Maze2";
                        break;
                    case 1062:
                        Name = "Maze3";
                        break;
                    case 1063:
                        Name = "AdventureIsland";
                        break;
                    case 1070:
                        Name = "SnakeDen";
                        break;
                    case 1072:
                        Name = "CityArena4";
                        break;
                    case 1073:
                        Name = "Arena1";
                        break;
                    case 1074:
                        Name = "Arena2";
                        break;
                    case 1075:
                        Name = "NewCanyon";
                        break;
                    case 1076:
                        Name = "NewForest";
                        break;
                    case 1077:
                        Name = "NewDesert";
                        break;
                    case 1078:
                        Name = "NewIsland";
                        break;
                    case 1080:
                        Name = "Arena2";
                        break;
                    case 1081:
                        Name = "Arena3";
                        break;
                    case 1090:
                        Name = "Arena1";
                        break;
                    case 1091:
                        Name = "Arena2";
                        break;
                    case 1092:
                        Name = "Arena1";
                        break;
                    case 1093:
                        Name = "Arena2";
                        break;
                    case 1094:
                        Name = "Arena1";
                        break;
                    case 1095:
                        Name = "Arena2";
                        break;
                    case 1100:
                        Name = "MoonPlatform";
                        break;
                    case 1101:
                        Name = "MoonPlatform";
                        break;
                    case 1102:
                        Name = "MoonPlatform";
                        break;
                    case 1103:
                        Name = "MoonPlatform";
                        break;
                    case 1104:
                        Name = "MoonPlatform";
                        break;
                    case 1105:
                        Name = "MoonPlatform";
                        break;
                    case 1106:
                        Name = "MoonPlatform";
                        break;
                    case 1107:
                        Name = "MoonPlatform";
                        break;
                    case 1108:
                        Name = "MoonPlatform";
                        break;
                    case 1109:
                        Name = "MoonPlatform";
                        break;
                    case 1201:
                        Name = "GlobeQuest1";
                        break;
                    case 1202:
                        Name = "GlobeQuest2";
                        break;
                    case 1204:
                        Name = "GlobeQuest4";
                        break;
                    case 1205:
                        Name = "GlobeQuest5";
                        break;
                    case 1207:
                        Name = "GlobeQuest7";
                        break;
                    case 1208:
                        Name = "GlobeQuest8";
                        break;
                    case 1210:
                        Name = "GlobeQuest10";
                        break;
                    case 1211:
                        Name = "GlobeQuest11";
                        break;
                    case 1212:
                        Name = "GlobeIsland";
                        break;
                    case 1213:
                        Name = "GlobeDesert";
                        break;
                    case 1214:
                        Name = "GlobeCanyon";
                        break;
                    case 1215:
                        Name = "GlobeForest";
                        break;
                    case 1216:
                        Name = "GlobePlain";
                        break;
                    case 1217:
                        Name = "JointCanyon";
                        break;
                    case 1218:
                        Name = "IronMine1";
                        break;
                    case 1219:
                        Name = "GlobeExit";
                        break;
                    case 1300:
                        Name = "MysticCave";
                        break;
                    case 1351:
                        Name = "Labyrinth";
                        break;
                    case 1352:
                        Name = "Labyrinth";
                        break;
                    case 1353:
                        Name = "Labyrinth";
                        break;
                    case 1354:
                        Name = "Labyrinth";
                        break;
                    case 1451:
                        Name = "MeteorArena";
                        break;
                    case 1500:
                        Name = "ClassPKArena1";
                        break;
                    case 1501:
                        Name = "ClassPKArena2";
                        break;
                    case 1502:
                        Name = "ClassPKArena3";
                        break;
                    case 1505:
                        Name = "CityArena1";
                        break;
                    case 1506:
                        Name = "CityArena2";
                        break;
                    case 1508:
                        Name = "CityArena4";
                        break;
                    case 1511:
                        Name = "FurnitureStore";
                        break;
                    case 1515:
                        Name = "CityArena1";
                        break;
                    case 1516:
                        Name = "CityArena2";
                        break;
                    case 1518:
                        Name = "CityArena4";
                        break;
                    case 1525:
                        Name = "CityArena1";
                        break;
                    case 1526:
                        Name = "CityArena2";
                        break;
                    case 1528:
                        Name = "CityArena4";
                        break;
                    case 1550:
                        Name = "HalloweenCity1";
                        break;
                    case 1551:
                        Name = "HalloweenCity1";
                        break;
                    case 1700:
                        Name = "EvilAbyss";
                        break;
                    case 1763:
                        Name = "Dreamland";
                        break;
                    case 1764:
                        Name = "Dreamland";
                        break;
                    case 1765:
                        Name = "Hall";
                        break;
                    case 1768:
                        Name = "KunLun";
                        break;
                    case 1769:
                        Name = "Garden";
                        break;
                    case 1770:
                        Name = "ArenaStage1";
                        break;
                    case 1771:
                        Name = "ArenaStage2";
                        break;
                    case 1772:
                        Name = "ArenaStage3";
                        break;
                    case 1773:
                        Name = "ArenaStage4";
                        break;
                    case 1774:
                        Name = "ArenaStage5";
                        break;
                    case 1775:
                        Name = "ArenaStage6";
                        break;
                    case 1777:
                        Name = "ArenaStage7";
                        break;
                    case 1778:
                        Name = "DangerCave";
                        break;
                    case 1779:
                        Name = "GhostCity";
                        break;
                    case 1780:
                        Name = "DarkCity";
                        break;
                    case 1782:
                        Name = "TreasureHouse";
                        break;
                    case 1783:
                        Name = "TreasureHouse1";
                        break;
                    case 1784:
                        Name = "Hut";
                        break;
                    case 1785:
                        Name = "Dungeon1F";
                        break;
                    case 1786:
                        Name = "Dungeon2F";
                        break;
                    case 1787:
                        Name = "Dungeon3F";
                        break;
                    case 1571:
                        Name = "MataMataFight";
                        break;
                    case 1572:
                        Name = "SuperDrop";
                        break;
                    case 1791:
                        Name = "RoseGarden";
                        break;
                    case 1792:
                        Name = "SwanLake";
                        break;
                    case 1794:
                        Name = "ViperCave";
                        break;
                    case 1801:
                        Name = "Crypt";
                        break;
                    case 1806:
                        Name = "OrchidGarden";
                        break;
                    case 1807:
                        Name = "LockerRoomA";
                        break;
                    case 1808:
                        Name = "MalePKArena";
                        break;
                    case 1809:
                        Name = "LockerRoomB";
                        break;
                    case 1810:
                        Name = "FemalePKArena";
                        break;
                    case 1812:
                        Name = "ExtremePKArena";
                        break;
                    case 1818:
                        Name = "BanditChamer";
                        break;
                    case 1837:
                        Name = "ClassPKArena4";
                        break;
                    case 1838:
                        Name = "ClassPKArena5";
                        break;
                    case 1839:
                        Name = "ClassPKArena6";
                        break;
                    case 1858:
                        Name = "PokerRoom";
                        break;
                    case 1860:
                        Name = "VIPPokerRoom";
                        break;
                    case 1863:
                        Name = "TwinCityArena";
                        break;
                    case 1864:
                        Name = "WindPlainArena";
                        break;
                    case 1868:
                        Name = "PhoenixCastleArena";
                        break;
                    case 1869:
                        Name = "MapleForestArena";
                        break;
                    case 1873:
                        Name = "ApeCityArena";
                        break;
                    case 1874:
                        Name = "LoveCanyonArena";
                        break;
                    case 1878:
                        Name = "BirdIslandArena";
                        break;
                    case 1879:
                        Name = "BirdIslandArena";
                        break;
                    case 1883:
                        Name = "DesertCityArena";
                        break;
                    case 1884:
                        Name = "DesertArena";
                        break;
                    case 1888:
                        Name = "LotteryHouse";
                        break;
                    case 1889:
                        Name = "CouplesPKGround";
                        break;
                    case 1926:
                        Name = "FrozenGrotto1";
                        break;
                    case 1927:
                        Name = "FrozenGrotto2";
                        break;
                    case 1928:
                        Name = "ClassPKArena10";
                        break;
                    case 1946:
                        Name = "ClassPKArena7";
                        break;
                    case 1947:
                        Name = "ClassPKArena8";
                        break;
                    case 1948:
                        Name = "ClassPKArena9";
                        break;
                    case 1950:
                        Name = "HorseRacing";
                        break;
                    case 1951:
                        Name = "RockMonsterDen";
                        break;
                    case 1961:
                        Name = "Mausoleum";
                        break;
                    case 1962:
                        Name = "Mausoleum";
                        break;
                    case 1963:
                        Name = "Mausoleum";
                        break;
                    case 1964:
                        Name = "Mausoleum";
                        break;
                    case 1965:
                        Name = "Mausoleum";
                        break;
                    case 1966:
                        Name = "Mausoleum";
                        break;
                    case 1967:
                        Name = "Mausoleum";
                        break;
                    case 1968:
                        Name = "Mausoleum";
                        break;
                    case 1969:
                        Name = "Mausoleum";
                        break;
                    case 1971:
                        Name = "Mausoleum";
                        break;
                    case 1972:
                        Name = "Mausoleum";
                        break;
                    case 1973:
                        Name = "Mausoleum";
                        break;
                    case 1974:
                        Name = "Mausoleum";
                        break;
                    case 1975:
                        Name = "Mausoleum";
                        break;
                    case 1976:
                        Name = "Mausoleum";
                        break;
                    case 1977:
                        Name = "Mausoleum";
                        break;
                    case 1978:
                        Name = "Mausoleum";
                        break;
                    case 1979:
                        Name = "Mausoleum";
                        break;
                    case 1981:
                        Name = "Mausoleum";
                        break;
                    case 1982:
                        Name = "Mausoleum";
                        break;
                    case 1983:
                        Name = "Mausoleum";
                        break;
                    case 1984:
                        Name = "Mausoleum";
                        break;
                    case 1985:
                        Name = "Mausoleum";
                        break;
                    case 1986:
                        Name = "Mausoleum";
                        break;
                    case 1987:
                        Name = "Mausoleum";
                        break;
                    case 1988:
                        Name = "Mausoleum";
                        break;
                    case 1989:
                        Name = "Mausoleum";
                        break;
                    case 1999:
                        Name = "FrozenGrotto3";
                        break;
                    case 2000:
                        Name = "IronMine2";
                        break;
                    case 2001:
                        Name = "IronMine2F";
                        break;
                    case 2002:
                        Name = "IronMine3F";
                        break;
                    case 2003:
                        Name = "IronMine3F";
                        break;
                    case 2004:
                        Name = "IronMine3F";
                        break;
                    case 2005:
                        Name = "IronMine3F";
                        break;
                    case 2006:
                        Name = "IronMine4F";
                        break;
                    case 2007:
                        Name = "IronMine4F";
                        break;
                    case 2008:
                        Name = "IronMine4F";
                        break;
                    case 2009:
                        Name = "IronMine4F";
                        break;
                    case 2010:
                        Name = "IronMine4F";
                        break;
                    case 2011:
                        Name = "IronMine4F";
                        break;
                    case 2012:
                        Name = "IronMine4F";
                        break;
                    case 2013:
                        Name = "IronMine4F";
                        break;
                    case 2020:
                        Name = "CopperMine2F";
                        break;
                    case 2021:
                        Name = "CopperMine2F";
                        break;
                    case 2022:
                        Name = "CopperMine3F";
                        break;
                    case 2023:
                        Name = "CopperMine3F";
                        break;
                    case 2024:
                        Name = "CopperMine3F";
                        break;
                    case 2025:
                        Name = "CopperMine3F";
                        break;
                    case 2026:
                        Name = "CopperMine4F";
                        break;
                    case 2027:
                        Name = "CopperMine4F";
                        break;
                    case 2028:
                        Name = "CopperMine4F";
                        break;
                    case 2029:
                        Name = "CopperMine4F";
                        break;
                    case 2030:
                        Name = "CopperMine4F";
                        break;
                    case 2031:
                        Name = "CopperMine4F";
                        break;
                    case 2032:
                        Name = "CopperMine4F";
                        break;
                    case 2033:
                        Name = "CopperMine4F";
                        break;
                    case 2040:
                        Name = "SilverMine2F";
                        break;
                    case 2041:
                        Name = "SilverMine2F";
                        break;
                    case 2042:
                        Name = "SilverMine3F";
                        break;
                    case 2043:
                        Name = "SilverMine3F";
                        break;
                    case 2044:
                        Name = "SilverMine3F";
                        break;
                    case 2045:
                        Name = "SilverMine3F";
                        break;
                    case 2046:
                        Name = "SilverMine4F";
                        break;
                    case 2047:
                        Name = "SilverMine4F";
                        break;
                    case 2048:
                        Name = "SilverMine4F";
                        break;
                    case 2049:
                        Name = "SilverMine4F";
                        break;
                    case 2050:
                        Name = "SilverMine4F";
                        break;
                    case 2051:
                        Name = "SilverMine4F";
                        break;
                    case 2052:
                        Name = "SilverMine4F";
                        break;
                    case 2053:
                        Name = "SilverMine4F";
                        break;
                    case 2054:
                        Name = "FrozenGrotto4";
                        break;
                    case 2055:
                        Name = "FrozenGrotto5";
                        break;
                    case 2056:
                        Name = "FrozenGrotto6";
                        break;
                    case 2060:
                        Name = "GuildContest";
                        break;
                    case 2071:
                        Name = "GuildPKTournamentMap";
                        break;
                    case 2072:
                        Name = "GuildPKTournamentMap";
                        break;
                    case 2073:
                        Name = "GuildPKTournamentMap";
                        break;
                    case 2074:
                        Name = "GuildPKTournamentMap";
                        break;
                    case 2075:
                        Name = "ElitePKWaitingArea1";
                        break;
                    case 2076:
                        Name = "ElitePKWaitingArea2";
                        break;
                    case 2077:
                        Name = "ElitePKWaitingArea3";
                        break;
                    case 2078:
                        Name = "ElitePKWaitingArea4";
                        break;
                    case 4021:
                        Name = "HellGate";
                        break;
                    case 4022:
                        Name = "HellHall";
                        break;
                    case 4023:
                        Name = "LeftCloister";
                        break;
                    case 4024:
                        Name = "RightCloister";
                        break;
                    case 4025:
                        Name = "BattleFormation";
                        break;
                    case 5000:
                        Name = "NPCJail";
                        break;
                    case 6000:
                        Name = "PKerJail";
                        break;
                    case 6001:
                        Name = "Jail";
                        break;
                    case 6002:
                        Name = "MacroJail";
                        break;
                    case 6003:
                        Name = "BotJail";
                        break;
                    case 6010:
                        Name = "AFKerJail";
                        break;
                    case 8822:
                        Name = "FiendLairFloor1";
                        break;
                    case 8823:
                        Name = "FiendLairFloor2";
                        break;
                    case 8824:
                        Name = "FiendLairFloor3";
                        break;
                    case 8825:
                        Name = "FiendLairFloor4";
                        break;
                    case 8826:
                        Name = "FiendLairFloor5";
                        break;
                    case 8827:
                        Name = "FiendLairFloor6";
                        break;
                    case 8828:
                        Name = "FiendLairFloor7";
                        break;
                    case 8829:
                        Name = "FiendLairFloor8";
                        break;
                    case 8830:
                        Name = "FiendLairFloor9";
                        break;
                    case 8831:
                        Name = "FiendLairFloor10";
                        break;
                    case 8832:
                        Name = "FiendLairFloor11";
                        break;
                    case 8833:
                        Name = "FiendLairFloor12";
                        break;
                    case 8834:
                        Name = "FiendLairFloor13";
                        break;
                    case 8835:
                        Name = "FiendLairFloor14";
                        break;
                    case 8836:
                        Name = "FiendLairFloor15";
                        break;
                    case 8837:
                        Name = "FiendLairFloor16";
                        break;
                    case 8838:
                        Name = "FiendLairFloor17";
                        break;
                    case 8839:
                        Name = "FiendLairFloor18";
                        break;
                    case 900000:
                        Name = "PlayersArena";
                        break;
                    case 910000:
                        Name = "ElitePKTournament";
                        break;
                    case 1000000:
                        Name = "ClanQualifier";
                        break;
                    case 3055:
                        Name = "GaleShallow";
                        break;
                    case 3056:
                        Name = "SeaOfDeath";
                        break;

                    default:
                        Name = "OtherMap";
                        break;
                }
                return Name;
            }
            public Member[] GetPkExplorerRank()
            {
                if (Associat.ContainsKey(PKExplorer))
                {
                    var rnk = Associat[PKExplorer].Values.OrderByDescending(kill => kill.KillsCount).ToArray();
                    return rnk;
                }
                return new Member[0];
            }

            public void AddEnemy(Client.GameClient Owner, Role.Player Killer)
            {
                Member member = new Member()
                {
                    UID = Killer.UID,
                    Name = Killer.Name
                };

                if (AllowAdd(Enemy, Killer.UID, 20))
                {
                    Add(Enemy, member);
                }
                else
                {
                    var remover = Associat[Enemy].Values.ToArray()[0];
                    Remove(Enemy, remover.UID);
                    if (AllowAdd(Enemy, Killer.UID, 20))
                        Add(Enemy, member);
                }
                unsafe
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();

                        Owner.Send(stream.KnowPersonsCreate(MsgKnowPersons.Action.AddEnemy, Killer.UID, true, Killer.Name, (uint)Killer.NobilityRank, Killer.Body));

                    }
                }
            }
            public unsafe void OnLoading(Client.GameClient client)
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();

                    Game.MsgServer.MsgApprenticeInformation mentorandapprentice = Game.MsgServer.MsgApprenticeInformation.Create();

                    foreach (var typ in Associat)
                    {
                        foreach (Member mem in typ.Value.Values)
                        {
                            if (typ.Key == Apprentice)
                            {
                                Client.GameClient clients;
                                if (Database.Server.GamePoll.TryGetValue(mem.UID, out clients))
                                {
                                    if (client.Player.Associate.OnlineApprentice.TryAdd(clients.Player.UID, clients))
                                    {
                                        var my_apprentice = Associat[Apprentice][clients.Player.UID];
                                        mentorandapprentice.Mode = Game.MsgServer.MsgApprenticeInformation.Action.Apprentice;
                                        mentorandapprentice.Mentor_ID = client.Player.UID;
                                        mentorandapprentice.Apprentice_ID = clients.Player.UID;
                                        mentorandapprentice.Apprentice_Blessing = (ushort)my_apprentice.Blessing;
                                        mentorandapprentice.Apprentice_Composing = (ushort)my_apprentice.Stone;
                                        mentorandapprentice.Apprentice_Experience = (ushort)my_apprentice.ExpBalls;
                                        mentorandapprentice.Class = clients.Player.Class;
                                        mentorandapprentice.Enrole_date = (uint)my_apprentice.Timer;
                                        mentorandapprentice.Mesh = clients.Player.Mesh;
                                        mentorandapprentice.Level = (byte)clients.Player.Level;
                                        mentorandapprentice.Online = 1;
                                        mentorandapprentice.PkPoints = clients.Player.PKPoints;
                                        mentorandapprentice.WriteString(client.Player.Name, clients.Player.Spouse, clients.Player.Name);
                                        client.Send(mentorandapprentice.GetArray(stream));



                                        clients.Player.SetMentorBattlePowers(client.Player.GetShareBattlePowers((uint)clients.Player.RealBattlePower), (uint)client.Player.RealBattlePower);
                                        mentorandapprentice.Mode = Game.MsgServer.MsgApprenticeInformation.Action.Mentor;
                                        mentorandapprentice.Class = client.Player.Class;
                                        mentorandapprentice.Enrole_date = (uint)my_apprentice.Timer;
                                        mentorandapprentice.Mesh = client.Player.Mesh;
                                        mentorandapprentice.Level = (byte)client.Player.Level;
                                        mentorandapprentice.Online = 1;
                                        mentorandapprentice.PkPoints = client.Player.PKPoints;
                                        mentorandapprentice.Shared_Battle_Power = client.Player.GetShareBattlePowers((uint)clients.Player.RealBattlePower);
                                        mentorandapprentice.WriteString(client.Player.Name, clients.Player.Spouse, clients.Player.Name);
                                        clients.Send(mentorandapprentice.GetArray(stream));
                                    }
                                }
                                else
                                {

                                    mentorandapprentice.Class = 0;
                                    mentorandapprentice.Mesh = 0;
                                    mentorandapprentice.Level = 0;
                                    mentorandapprentice.Online = 0;
                                    mentorandapprentice.PkPoints = 0;
                                    mentorandapprentice.Shared_Battle_Power = 0;

                                    mentorandapprentice.Mode = Game.MsgServer.MsgApprenticeInformation.Action.Apprentice;
                                    mentorandapprentice.Mentor_ID = MyUID;
                                    mentorandapprentice.Apprentice_ID = mem.UID;
                                    mentorandapprentice.Enrole_date = (uint)mem.Timer;
                                    mentorandapprentice.WriteString("NULL", "NULL", mem.Name);
                                    client.Send(mentorandapprentice.GetArray(stream));

                                }
                            }
                            if (typ.Key == Mentor)
                            {

                                Client.GameClient clients;
                                if (Database.Server.GamePoll.TryGetValue(mem.UID, out clients))
                                {
                                    Member apprentice;
                                    if (clients.Player.Associate.Associat[Apprentice].TryGetValue(client.Player.UID, out apprentice))
                                    {
                                        if (clients.Player.Associate.OnlineApprentice.TryAdd(client.Player.UID, client))
                                        {
                                            mentorandapprentice.Mode = Game.MsgServer.MsgApprenticeInformation.Action.Apprentice;
                                            mentorandapprentice.Mentor_ID = clients.Player.UID;
                                            mentorandapprentice.Apprentice_ID = client.Player.UID;
                                            mentorandapprentice.Apprentice_Blessing = (ushort)apprentice.Blessing;
                                            mentorandapprentice.Apprentice_Composing = (ushort)apprentice.Stone;
                                            mentorandapprentice.Apprentice_Experience = (ushort)apprentice.ExpBalls;
                                            mentorandapprentice.Class = client.Player.Class;
                                            mentorandapprentice.Enrole_date = (uint)apprentice.Timer;
                                            mentorandapprentice.Mesh = client.Player.Mesh;
                                            mentorandapprentice.Level = (byte)client.Player.Level;
                                            mentorandapprentice.Online = 1;
                                            mentorandapprentice.PkPoints = client.Player.PKPoints;
                                            mentorandapprentice.WriteString(clients.Player.Name, client.Player.Spouse, client.Player.Name);
                                            clients.Send(mentorandapprentice.GetArray(stream));



                                            client.Player.SetMentorBattlePowers(clients.Player.GetShareBattlePowers((uint)client.Player.RealBattlePower), (uint)clients.Player.RealBattlePower);
                                            mentorandapprentice.Mode = Game.MsgServer.MsgApprenticeInformation.Action.Mentor;
                                            mentorandapprentice.Class = clients.Player.Class;
                                            mentorandapprentice.Enrole_date = (uint)apprentice.Timer;
                                            mentorandapprentice.Mesh = clients.Player.Mesh;
                                            mentorandapprentice.Level = (byte)clients.Player.Level;
                                            mentorandapprentice.Online = 1;
                                            mentorandapprentice.PkPoints = clients.Player.PKPoints;
                                            mentorandapprentice.Shared_Battle_Power = clients.Player.GetShareBattlePowers((uint)client.Player.RealBattlePower);
                                            mentorandapprentice.WriteString(clients.Player.Name, client.Player.Spouse, client.Player.Name);
                                            client.Send(mentorandapprentice.GetArray(stream));
                                        }
                                    }
                                }
                                else
                                {
                                    mentorandapprentice.Class = 0;
                                    mentorandapprentice.Mesh = 0;
                                    mentorandapprentice.Level = 0;
                                    mentorandapprentice.Online = 0;
                                    mentorandapprentice.PkPoints = 0;
                                    mentorandapprentice.Shared_Battle_Power = 0;

                                    mentorandapprentice.Mode = Game.MsgServer.MsgApprenticeInformation.Action.Mentor;
                                    mentorandapprentice.Mentor_ID = mem.UID;
                                    mentorandapprentice.Apprentice_ID = MyUID;
                                    mentorandapprentice.Enrole_date = (uint)mem.Timer;
                                    mentorandapprentice.WriteString(mem.Name, "", "");
                                    client.Send(mentorandapprentice.GetArray(stream));
                                }
                            }
                            if (typ.Key == Friends)
                            {
                                Client.GameClient Targer;
                                if (Database.Server.GamePoll.TryGetValue(mem.UID, out Targer))
                                {
                                    client.Send(stream.KnowPersonsCreate(MsgKnowPersons.Action.AddFriend, mem.UID, true, mem.Name, (uint)Targer.Player.NobilityRank, Targer.Player.Body));
                                    Targer.Send(stream.KnowPersonsCreate(MsgKnowPersons.Action.AddOnline, client.Player.UID, true, client.Player.Name, (uint)client.Player.NobilityRank, client.Player.Body));
                                }
                                else
                                    client.Send(stream.KnowPersonsCreate(MsgKnowPersons.Action.AddFriend, mem.UID, false, mem.Name, 0, 0));

                            }
                            if (typ.Key == Enemy)
                            {
                                Client.GameClient Targer;
                                if (Database.Server.GamePoll.TryGetValue(mem.UID, out Targer))
                                {
                                    client.Send(stream.KnowPersonsCreate(MsgKnowPersons.Action.AddEnemy, mem.UID, true, mem.Name, (uint)Targer.Player.NobilityRank, Targer.Player.Body));
                                }
                                else
                                    client.Send(stream.KnowPersonsCreate(MsgKnowPersons.Action.AddEnemy, mem.UID, false, mem.Name, 0, 0));

                            }
                            if (typ.Key == Partener)
                            {
                                Client.GameClient Targer;
                                if (Database.Server.GamePoll.TryGetValue(mem.UID, out Targer))
                                {
                                    client.Send(stream.TradePartnerCreate(mem.UID, MsgTradePartner.Action.AddPartner, true, mem.GetTimerLeft(), mem.Name));

                                    Targer.Send(stream.TradePartnerCreate(client.Player.UID, MsgTradePartner.Action.AddOnline, true, mem.GetTimerLeft(), client.Player.Name));
                                }
                                else client.Send(stream.TradePartnerCreate(mem.UID, MsgTradePartner.Action.AddPartner, false, mem.GetTimerLeft(), mem.Name));

                            }
                        }
                    }
                }

            }
            public unsafe void OnDisconnect(ServerSockets.Packet stream, Client.GameClient client)
            {
                Game.MsgServer.MsgApprenticeInformation mentorandapprentice = Game.MsgServer.MsgApprenticeInformation.Create();

                foreach (Client.GameClient clients in Database.Server.GamePoll.Values)
                {
                    foreach (var typ in Associat)
                    {
                        if (!typ.Value.ContainsKey(clients.Player.UID))
                            continue;
                        if (typ.Key == Apprentice)
                        {
                            mentorandapprentice.Mentor_ID = client.Player.UID;
                            mentorandapprentice.Apprentice_ID = clients.Player.UID;
                            clients.Player.SetMentorBattlePowers(0, 0);
                            mentorandapprentice.Mode = Game.MsgServer.MsgApprenticeInformation.Action.Mentor;
                            mentorandapprentice.Online = 0;
                            mentorandapprentice.WriteString(client.Player.Name, clients.Player.Spouse, clients.Player.Name);
                            clients.Send(mentorandapprentice.GetArray(stream));

                        }
                        if (typ.Key == Mentor)
                        {
                            mentorandapprentice.Mode = Game.MsgServer.MsgApprenticeInformation.Action.Apprentice;
                            mentorandapprentice.Mentor_ID = clients.Player.UID;
                            mentorandapprentice.Apprentice_ID = client.Player.UID;
                            mentorandapprentice.Online = 0;
                            mentorandapprentice.WriteString(clients.Player.Name, client.Player.Spouse, client.Player.Name);
                            clients.Send(mentorandapprentice.GetArray(stream));

                        }
                        if (typ.Key == Friends)
                        {

                            clients.Send(stream.KnowPersonsCreate(MsgKnowPersons.Action.AddOffline, client.Player.UID, false, client.Player.Name, 0, 0));
                        }
                        if (typ.Key == Partener)
                        {

                            clients.Send(stream.TradePartnerCreate(client.Player.UID, MsgTradePartner.Action.AddOffline, false, 0, client.Player.Name));
                        }
                    }
                }
            }
            public IEnumerable<string> ToStringMember()
            {
                foreach (var typ in Associat)
                    foreach (Member member in typ.Value.Values)
                        yield return MyUID + "/" + typ.Key + "/" + Mentor_ExpBalls + "/" + Mentor_Blessing + "/" + Mentor_Stones +
                            "/" + 0 + "/" + 0 + "/" + 0 + "/" + member.ToString();
            }
        }
        public static ConcurrentDictionary<uint, MyAsociats> Associates = new ConcurrentDictionary<uint, MyAsociats>();
        public static void RemoveOffline(byte Mode, uint UID, uint OnRemove)
        {
            MyAsociats associate;
            if (Associates.TryGetValue(UID, out associate))
            {
                associate.Remove(Mode, OnRemove);
            }
        }

        public static void Save()
        {
            try
            {
                using (Database.DBActions.Write _wr = new Database.DBActions.Write("Associate.txt"))
                {
                    foreach (var x in Associates)
                    {
                        foreach (var member in x.Value.ToStringMember())
                        {
                            _wr.Add(member);
                        }
                    }
                    _wr.Execute(Database.DBActions.Mode.Open);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public static void Load()
        {
            try
            {
                using (Database.DBActions.Read r = new Database.DBActions.Read("Associate.txt"))
                {
                    if (r.Reader())
                    {
                        int count = r.Count;
                        for (uint x = 0; x < count; x++)
                        {
                            string[] data = r.ReadString("").Split('/');
                            uint UID = uint.Parse(data[0]);
                            byte Mod = byte.Parse(data[1]);
                            uint MentorExpBalls = uint.Parse(data[2]);
                            uint MentorBless = uint.Parse(data[3]);
                            uint MentorStone = uint.Parse(data[4]);
                            Member membru = new Member();
                            membru.UID = uint.Parse(data[8]);
                            membru.Timer = ulong.Parse(data[9]);
                            membru.ExpBalls = uint.Parse(data[10]);
                            membru.Stone = uint.Parse(data[11]);
                            membru.Blessing = uint.Parse(data[12]);
                            membru.Name = data[13];
                            membru.KillsCount = ushort.Parse(data[14]);
                            membru.BattlePower = ushort.Parse(data[15]);
                            membru.Map = data[16];
                            if (Associates.ContainsKey(UID))
                            {
                                if (Associates[UID].Associat.ContainsKey(Mod))
                                    Associates[UID].Associat[Mod].TryAdd(membru.UID, membru);
                                else
                                {
                                    Associates[UID].Associat.TryAdd(Mod, new ConcurrentDictionary<uint, Member>());
                                    Associates[UID].Associat[Mod].TryAdd(membru.UID, membru);
                                }
                            }
                            else
                            {
                                MyAsociats assoc = new MyAsociats(UID);
                                assoc.MyUID = UID;
                                assoc.Mentor_ExpBalls = MentorExpBalls;
                                assoc.Mentor_Blessing = MentorBless;
                                assoc.Mentor_Stones = MentorStone;
                                assoc.Associat.TryAdd(Mod, new ConcurrentDictionary<uint, Member>());
                                assoc.Associat[Mod].TryAdd(membru.UID, membru);
                                Associates.TryAdd(UID, assoc);

                            }
                        }
                    }
                }
                GC.Collect();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
