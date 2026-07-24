using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace COServer.Database
{
    public class ItemType : Dictionary<uint, ItemType.DBItem>
    {
        public uint Time = 7; //7 days default

        public static Dictionary<uint, ItemType.DBItem> Garments = new Dictionary<uint, ItemType.DBItem>();
        public static Dictionary<uint, ItemType.DBItem> Garments2 = new Dictionary<uint, ItemType.DBItem>();
        public static List<uint> TopGarments = new List<uint>()
        {
            183845 ,184160 ,184170 ,184180 ,191710,191750 ,191930 ,181495 ,194165 ,194400 ,194410
            ,194480 ,194500 ,183925 ,183935 ,183945 ,183955 ,183985 ,184025 ,184035 ,184045 ,184055
            ,184065 ,184075 ,184085 ,184095 ,193825 ,193835 ,194755 ,194915 ,194925 ,194945 ,195725
            ,197775 ,197785 ,197815 ,197845 ,197865 ,197885 ,197895 ,197905 ,197935 ,197005 ,197045
            ,197135 ,197185 ,197195 ,197215 ,197275 ,197285 ,197295 ,197315 ,184805 ,184835 ,184785
            ,194825 ,194855 ,197925 ,197945 ,197955 ,197015 ,197025

        };
        public static List<uint> unabletradeitem = new List<uint>()
        {
            750000,
            723751,
            729549,
            729960,
            729961,
            729962,
            729963,729964,
            729965,729966,729967,
            729968,729969,729970,
            7112141,
            7112151,
            7112161,
            7112171,
            7112181,
            7112191,
            7112201,
            729935,
            729936,
            729937,

        };
        public const uint

            MaxPlus = 9,
            MaxEnchant = 255,
            MaxDurability = 7099,
            MemoryAgate = 720828,
            ExpBall = 723700,
            PrayingStoneS = 1200000,
            MeteorTearPacket = 723711,
            MeteorTear = 1088002,
            OneStonePacket = 723712,
            ArenaExperience = 723912,
            DragonBallScroll = 720028,
            DragonBall = 1088000,
            CelestialStone = 721259,
            CleanWater = 721258,
            MeteorScroll = 720027,
            Meteor = 1088001,
            MoonBox = 721080,
            ExperiencePotion = 723017,
            NinjaAmulet = 723583,
            PowerExpBall = 722057,
            DoubleExp = 723017,
            SkillTeamPKPack = 720981,
            ToughDrill = 1200005,
            StarDrill = 1200006,
            SuperToroiseGem = 700073,
            ClassMoney5 = 723717,
            ClassMoney4 = 723716,
            Stone_1 = 730001,
            Stone_2 = 730002,
            Stone_3 = 730003,
            Stone_4 = 730004,
            Stone_5 = 730005,
            Stone_6 = 730006,
            Stone_7 = 730006,
            Stone_8 = 730006,
            RDG = 700012,
            RFG = 700022,
            RBG = 700032,
            KYG = 700042,
            VIG = 700052,
            MOG = 700062,
            TOG = 700072,
            LotteryTick = 710212,
            Moss = 722723,
            DreamGrass = 722724,
            SoulAroma = 722725,
            EvilTooth = 722721,
            ImmortalStone = 722728,
            ImpureVigor = 722730,
            HiddenAttributeToken = 700072,
            BronzePrize = 2100055,
            SilverPrize = 2100065,
            GoldPrize = 2100075,
            GoldTrophy = 2100085,
            GoldCup = 2100095,
            UpgradeCert_2 = 721174,
            UpgradeCert_3 = 711198,
            UpgradeCert_4 = 711200,
            UpgradeCert_5 = 711553,
            YellowEgg = 720004,
            GreenEgg = 720005,
            WhiteEgg = 720006,
            ExpBall2 = 723834,
            Letra1 = 7112141,
            Letra2 = 7112151,
            Letra3 = 7112161,
            Letra4 = 7112171,
            Letra5 = 7112181,
            Letra6 = 7112191,
            Letra7 = 7112201,
            ProfToken = 159753,
            MetScrollPack = 723792,
            DbScrollPack = 723793,
            SoulAromaBag = 720372,
            DreamGrassBag = 720373,
            MossBag = 720375,
            NormalPhoenixGem = 700001,
            NormalDragonGem = 700011,
            NormalFuryGem = 700021,
            NormalRainbowGem = 700031,
            NormalKylinGem = 700041,
            NormalVioletGem = 700051,
            NormalMoonGem = 700061,
            EuxeniteOre = 107203;

        public static bool IsMoneyBag(uint ID)
        {
            return ID >= 723713 && ID <= 723723 || ID == 3005945 || ID == 3008452;
        }

        public static bool CheckAddGemFan(Role.Flags.Gem gem)
        {
            // ThunderGem totalmente removida (Normal, Refined e Super)
            return false;
        }

        public static bool CheckAddGemTower(Role.Flags.Gem gem)
        {
            // GloryGem totalmente removida (Normal, Refined e Super)
            return false;
        }

        public static bool CheckAddGemWing(Role.Flags.Gem gem, byte slot)
        {
            if (slot == 1)
                return CheckAddGemFan(gem);
            else if (slot == 2)
                return CheckAddGemTower(gem);
            return false;
        }

        public static uint GetGemID(Role.Flags.Gem Gem)
        {
            return (uint)(700000 + (byte)Gem);
        }

        public static ushort PurifyStabilizationPoints(byte plevel)
        {
            return purifyStabilizationPoints[Math.Min(plevel - 1, (byte)5)];
        }

        static ushort[] purifyStabilizationPoints = new ushort[6] { 10, 30, 60, 100, 150, 200 };

        public static ushort RefineryStabilizationPoints(byte elevel)
        {
            return refineryStabilizationPoints[Math.Min(elevel - 1, (byte)4)];
        }
        static ushort[] refineryStabilizationPoints = new ushort[5] { 10, 30, 70, 150, 270 };

        public string GetItemName(uint ID)
        {
            DBItem item;
            if (Server.ItemsBase.TryGetValue(ID, out item))
            {
                return item.Name;
            }
            return "";
        }

        public static string GetItemIdByName(string name)
        {
            var item = Server.ItemsBase.Values.Where(e => e.Name == name);
            int itemC = item.Count();
            if (item.Count() == 0)
                return "0";
            if (item.Count() == 1)
                return item.SingleOrDefault().ID.ToString();
            else
            {
                string ret = "";
                foreach (var s in item)
                    ret += s.ID + "-";
                return ret;
            }
        }

        public static bool IsPickAxe(uint ID)
        {
            return ID == 562000;
        }

        public static bool IsHoe(uint ID)
        {
            return ID == 562001;
        }

        public static uint ComposePlusPoints(byte plus)
        {
            // 🔒 Se já está em +9 ou acima, não precisa de pontos
            if (plus >= 9)
                return 0;

            return ComposePoints[plus];
        }

        public static uint StonePlusPoints(byte plus)
        {
            // 🔒 Se o item principal já está em +9, a pedra não deve gerar pontos
            if (plus >= 9)
                return 0;

            // Garante que nunca ultrapasse índice 8
            return StonePoints[Math.Min((int)plus, 8)];
        }

        static ushort[] StonePoints = new ushort[9]
        {
    1, 10, 40, 120, 360, 1080, 3240, 9720, 29160
        };

        static ushort[] ComposePoints = new ushort[10]
        {
    20, 20, 80, 240, 720, 2160, 6480, 19440, 58320, 0
        };

        public static Role.Flags.SoulTyp GetSoulPosition(uint ID)
        {
            if (ID >= 820001 && ID <= 820076)
                return Role.Flags.SoulTyp.Headgear;
            if (ID >= 821002 && ID <= 821034)
                return Role.Flags.SoulTyp.Necklace;
            if (ID >= 824002 && ID <= 824020)
                return Role.Flags.SoulTyp.Boots;
            if (ID >= 823000 && ID <= 823062)
                return Role.Flags.SoulTyp.Ring;
            if (ID >= 800000 && ID <= 800142 || ID >= 800701 && ID <= 800917 || ID >= 801000 && ID <= 801104 || ID >= 801200 && ID <= 801308) //800916
                return Role.Flags.SoulTyp.OneHandWeapon;
            if (ID >= 800200 && ID <= 800618 || ID == 801103)
                return Role.Flags.SoulTyp.TwoHandWeapon;
            if (ID >= 822001 && ID <= 822072)
                return Role.Flags.SoulTyp.Armor;

            return Role.Flags.SoulTyp.None;
        }
        public static bool CompareSoul(uint ITEMID, uint SoulID)
        {
            Role.Flags.SoulTyp soul = GetSoulPosition(SoulID);
            var positionit = GetItemSoulTYPE(ITEMID);
            if (positionit == soul)
                return true;
            return false;
        }
        public static Role.Flags.SoulTyp GetItemSoulTYPE(UInt32 itemid)
        {
            UInt32 iType = itemid / 1000;

            if (iType >= 111 && iType <= 118 || iType == 123 || iType >= 141 && iType <= 148 || itemid >= 170000 && itemid <= 170309)
                return Role.Flags.SoulTyp.Headgear;

            else if (iType >= 120 && iType <= 121)
                return Role.Flags.SoulTyp.Necklace;

            else if (iType >= 130 && iType <= 139 || itemid >= 101000 && itemid <= 101309)
                return Role.Flags.SoulTyp.Armor;

            else if (iType >= 150 && iType <= 152)
                return Role.Flags.SoulTyp.Ring;

            else if (iType == 160)
                return Role.Flags.SoulTyp.Boots;

            else if (IsTwoHand(itemid) || itemid >= 421003 && itemid <= 421439)
                return Role.Flags.SoulTyp.TwoHandWeapon;

            else if ((iType >= 410 && iType <= 490) || (iType >= 500 && iType <= 580) || (iType >= 601 && iType <= 613) || iType == 616 || iType == 614 || iType == 617 || iType == 622
                || iType == 624 || iType == 619)
                return Role.Flags.SoulTyp.OneHandWeapon;

            else
                return Role.Flags.SoulTyp.TwoHandWeapon;
        }

        public static uint MoneyItemID(uint value)
        {
            if (value < 100)
                return 1090000;
            else if (value < 399)
                return 1090010;
            else if (value < 5099)
                return 1090020;
            else if (value < 8099)
                return 1091000;
            else if (value < 12099)
                return 1091010;
            else
                return 1091020;
        }

        public static ulong CalculateExpBall(byte Level)
        {
            if (Level < 130)
                return (ulong)(680462.7536 + (3479.5308 * (Level / 2)) * Level);
            else if (Level < 135)
                return (ulong)((680462.7536 + (3479.5308 * (Level / 2)) * Level) * ((Level % 10) + 6));
            else
                return (ulong)((680462.7536 + (3479.5308 * (Level / 2)) * Level) * ((Level % 10) + 8));
        }
        public void Loading()
        {
            Dictionary<uint, ITPlus[]> itemsplus = new Dictionary<uint, ITPlus[]>();

            string[] baseplusText = File.ReadAllLines(Program.ServerConfig.DbLocation + "ItemAdd.ini");
            foreach (string line in baseplusText)
            {
                string _item_ = line.Trim();
                ITPlus pls = new ITPlus();
                pls.Parse(_item_);
                if (itemsplus.ContainsKey(pls.ID))
                {
                    itemsplus[pls.ID][pls.Plus] = pls;
                }
                else
                {
                    ITPlus[] a_pls = new ITPlus[13];
                    a_pls[pls.Plus] = pls;
                    itemsplus.Add(pls.ID, a_pls);
                }
            }
            string[] baseText = File.ReadAllLines(Program.ServerConfig.DbLocation + "itemtype.txt");
            foreach (string line in baseText)
            {
                string _item_ = line.Trim();
                if (_item_.Length > 11)
                {
                    if (_item_.IndexOf("//", 0, 2) != 0)
                    {
                        DBItem item = new DBItem();
                        item.Parse(line);
                        // if (item.ID == PowerExpBall)
                        //     continue;
                        if (itemsplus.ContainsKey(GetBaseID(item.ID)) || itemsplus.ContainsKey(GetBaseID(item.ID) + 10) || itemsplus.ContainsKey(GetBaseID(item.ID) + 20))
                        {
                            item.AllowUpgradePlus = true;
                            if (!itemsplus.TryGetValue(GetBaseID(item.ID), out item.Plus))
                            {
                                if (!itemsplus.TryGetValue(GetBaseID(item.ID) + 10, out item.Plus))
                                {
                                    if (!itemsplus.TryGetValue(GetBaseID(item.ID) + 20, out item.Plus))
                                    {
                                        int pos = ItemPosition(item.ID);
                                        if (pos < 6)
                                            Console.WriteLine("eroror item " + item.ID + " " + item.Name);
                                    }
                                }
                            }
                        }
                        if (!ContainsKey(item.ID))
                            Add(item.ID, item);
                    }
                }
            }
            Console.WriteLine("Loading [" + Count + "] items");
            itemsplus = null;
            GC.Collect();
        }
        public uint DowngradeItem(uint ID)
        {
            try
            {
                ushort Tryng = 0;
                ushort firstposition = ItemPosition(ID);
                uint rebornid = ID;
                while (true)
                {
                    if (Tryng > 1000)
                        break;
                    Tryng++;
                    //shield !
                    if (ID >= 900000 && ID <= 900309)
                    {
                        if (this[rebornid].Level <= 40)
                            break;
                    }
                  
                    if (this[rebornid].Level <= ItemMinLevel((Role.Flags.ConquerItem)ItemPosition(ID)))
                    {
                        break;
                    }
                    if (this.ContainsKey(rebornid - 10))
                    {
                        rebornid -= 10;
                    }
                    else if (this.ContainsKey(rebornid - 20))
                    {
                        rebornid -= 20;
                    }
                    else if (this.ContainsKey(rebornid - 30))
                    {
                        rebornid -= 30;
                    }
                    else if (this.ContainsKey(rebornid - 40))
                    {
                        rebornid -= 40;
                    }
                }
                if (firstposition == ItemPosition(rebornid) && this.ContainsKey(rebornid))
                    return rebornid;
                else
                    return ID;

            }
            catch (Exception e) { Console.WriteLine(e.ToString()); return ID; }
        }
        public uint UpdateItem(uint id, out bool succesed)
        {
            ushort firstposition = ItemPosition(id);
            uint nextId = 0;
            if (this[id].Level < ItemMaxLevel((Role.Flags.ConquerItem)ItemPosition(id)))
            {
                if (this.ContainsKey(id + 10))
                {
                    nextId = id + 10;
                }
                else if (this.ContainsKey(id + 20))
                {
                    nextId = id + 20;
                }
                else if (this.ContainsKey(id + 30))
                {
                    nextId = id + 30;
                }
                else if (this.ContainsKey(id + 40))
                {
                    nextId = id + 40;
                }
            }
            if (firstposition == ItemPosition(nextId) && this.ContainsKey(nextId))
            {
                succesed = true;
                return nextId;
            }
            else
            {
                succesed = false;
                return id;
            }
        }
        public uint UpdateItem(uint id, Client.GameClient client)
        {
            uint nextId = id;
            if ((this[id].Level < ItemMaxLevel((Role.Flags.ConquerItem)ItemPosition(id))) && this[id].Level < client.Player.Level)
            {
                if (this.ContainsKey(id + 10))
                {
                    nextId = id + 10;
                }
            }
            return nextId;
        }
        public static byte ItemMinLevel(Role.Flags.ConquerItem postion)
        {
            switch (postion)
            {
                case 0: return 0;
                case Role.Flags.ConquerItem.Head: return 15;
                case Role.Flags.ConquerItem.Necklace: return 7;
                case Role.Flags.ConquerItem.Armor: return 15;
                case Role.Flags.ConquerItem.LeftWeapon: return 15;
                case Role.Flags.ConquerItem.RightWeapon: return 15;
                case Role.Flags.ConquerItem.Boots: return 10;
                case Role.Flags.ConquerItem.Ring: return 10;
                case Role.Flags.ConquerItem.Tower: return 0;
                case Role.Flags.ConquerItem.Fan: return 0;
                case Role.Flags.ConquerItem.Garment: return 0;
            }
            return 0;
        }
        public static byte GetSashCounts(uint ID)
        {
            if (ID == 1100009) return 12;
            if (ID == 1100006) return 6;
            if (ID == 1100003) return 3;
            return 0;
        }
        public static bool IsSash(uint ID)
        {
            if (ID == 1100009 || ID == 1100006 || ID == 1100003) return true;
            return false;
        }
        public static bool IsBoot(uint ID)
        {
            return ID >= 160013 && ID <= 160249;
        }
        public static bool IsRapier(uint ID)
        {
            return ID >= 611000 && ID <= 611439;
        }
        public static bool IsKnife(uint ID)
        {
            return ID >= 613000 && ID <= 613429;
        }
        public static bool IsShield(uint ID)
        {
            return ID >= 900000 && ID <= 900309;
        }
        public static bool IsArmor(uint ID)
        {
            return (ID >= 130003 && ID <= 139309 || ID >= 101000 && ID <= 101309);
        }
        public static bool IsHelmet(uint ID)
        {
            return ((ID >= 111003 && ID <= 118309) || (ID >= 123000 && ID <= 123309) || (ID >= 141003 && ID <= 145309) || (ID >= 148000 && ID <= 148309)
              || ID >= 170000 && ID <= 170309);
        }
        public static bool IsPrayedBead(uint ID)
        {
            return ID >= 610000 && ID <= 610439;
        }
        public static bool IsTalisman(uint ID)
        {
            return ItemPosition(ID) == (ushort)Role.Flags.ConquerItem.Tower || ItemPosition(ID) == (ushort)Role.Flags.ConquerItem.Fan;
        }

        public static bool AllowToUpdate(Role.Flags.ConquerItem Position)
        {
            if (Position == Role.Flags.ConquerItem.Fan
                  || Position == Role.Flags.ConquerItem.Tower
                || Position == Role.Flags.ConquerItem.Garment
                || Position == Role.Flags.ConquerItem.Bottle)
                return false;
            return true;
        }
        public static ushort ItemPosition(uint ID)
        {

            UInt32 iType = ID / 1000;
            if (iType == 622 || iType == 624 || iType == 626)
                return (ushort)Role.Flags.ConquerItem.RightWeapon;
            if (iType == 620)
                return (ushort)Role.Flags.ConquerItem.RightWeapon;
            if (iType == 619)
                return (ushort)Role.Flags.ConquerItem.LeftWeapon;
            if (iType == 617)
                return (ushort)Role.Flags.ConquerItem.RightWeapon;
            if (iType == 148)
                return (ushort)Role.Flags.ConquerItem.Head;
            if (iType == 614)
                return (ushort)Role.Flags.ConquerItem.RightWeapon;
            if (iType == 615 || iType == 616)
                return (ushort)Role.Flags.ConquerItem.RightWeapon;
            if (iType >= 111 && iType <= 118 || iType == 123 || iType >= 141 && iType <= 144 || iType == 145 || iType == 170)
                return (ushort)Role.Flags.ConquerItem.Head;
            else if (iType >= 120 && iType <= 121)
                return (ushort)Role.Flags.ConquerItem.Necklace;
            else if ((iType >= 130 && iType <= 139) || (iType == 101))
                return (ushort)Role.Flags.ConquerItem.Armor;
            else if (iType >= 150 && iType <= 152)
                return (ushort)Role.Flags.ConquerItem.Ring;
            else if (iType == 160)
                return (ushort)Role.Flags.ConquerItem.Boots;
            else if (iType >= 181 && iType <= 200)//194
                return (ushort)Role.Flags.ConquerItem.Garment;
            else if (iType == 201)
                return (ushort)Role.Flags.ConquerItem.Fan;
            else if (iType == 202)
                return (ushort)Role.Flags.ConquerItem.Tower;
            else if (iType == 2100)
                return (ushort)Role.Flags.ConquerItem.Bottle;
            else if (iType == 1050 || iType == 900)
                return (ushort)Role.Flags.ConquerItem.LeftWeapon;
            else if ((iType >= 410 && iType <= 490) || (iType >= 500 && iType <= 580) || (iType >= 601 && iType <= 613))
                return (ushort)Role.Flags.ConquerItem.RightWeapon;
            else return 0;
        }
        public static bool IsArrow(uint ID)
        {
            if (ID >= 1050000 && ID <= 1051000)
                return true;
            return false;
        }
        public static bool IsTwoHand(uint ID)
        {
            return ((ID.ToString()[0] == '5' || ID >= 421003 && ID <= 421439) ? true : false);
        }
        public static bool IsBow(uint ID)
        {
            return ID >= 500003 && ID <= 500429;
        }
        public static bool IsBraclet(uint ID)
        {
            return ID >= 152013 && ID <= 152279;
        }
        public static bool IsRing(uint ID)
        {
            return ID >= 150013 && ID <= 150269;
        }
        public static bool IsHeavyRing(uint ID)
        {
            return ID >= 151013 && ID <= 151269;
        }
        public static byte ItemMaxLevel(Role.Flags.ConquerItem Position)
        {
            switch (Position)
            {
                case 0: return 0;
                case Role.Flags.ConquerItem.Head: return 120;
                case Role.Flags.ConquerItem.Necklace: return 130;
                case Role.Flags.ConquerItem.Armor: return 120;
                case Role.Flags.ConquerItem.LeftWeapon: return 130;
                case Role.Flags.ConquerItem.RightWeapon: return 130;
                case Role.Flags.ConquerItem.Boots: return 129;
                case Role.Flags.ConquerItem.Ring: return 126;
               case Role.Flags.ConquerItem.Tower: return 100;
                case Role.Flags.ConquerItem.Fan: return 100;
            }
            return 0;
        }
        public static bool IsBacksword(uint ID)
        {
            return ID >= 421003 && ID <= 421439;
        }
        public static bool IsKatana(uint ID)
        {
            return ID >= 601000 && ID <= 601339;
        }
        public static bool EquipPassStatsReq(DBItem baseInformation, Client.GameClient client)
        {
            if (client.Player.Strength >= baseInformation.Strength && client.Player.Agility >= baseInformation.Agility)
                return true;
            else
                return false;
        }
        public static bool EquipPassJobReq(DBItem baseInformation, Client.GameClient client)
        {
            switch (baseInformation.Class)
            {
                #region Trojan
                case 10: if (client.Player.Class <= 15 && client.Player.Class >= 10) return true; break;
                case 11: if (client.Player.Class <= 15 && client.Player.Class >= 11) return true; break;
                case 12: if (client.Player.Class <= 15 && client.Player.Class >= 12) return true; break;
                case 13: if (client.Player.Class <= 15 && client.Player.Class >= 13) return true; break;
                case 14: if (client.Player.Class <= 15 && client.Player.Class >= 14) return true; break;
                case 15: if (client.Player.Class == 15) return true; break;
                #endregion
                #region Warrior
                case 20: if (client.Player.Class <= 25 && client.Player.Class >= 20) return true; break;
                case 21: if (client.Player.Class <= 25 && client.Player.Class >= 21) return true; break;
                case 22: if (client.Player.Class <= 25 && client.Player.Class >= 22) return true; break;
                case 23: if (client.Player.Class <= 25 && client.Player.Class >= 23) return true; break;
                case 24: if (client.Player.Class <= 25 && client.Player.Class >= 24) return true; break;
                case 25: if (client.Player.Class == 25) return true; break;
                #endregion
                #region Archer
                case 40: if (client.Player.Class <= 45 && client.Player.Class >= 40) return true; break;
                case 41: if (client.Player.Class <= 45 && client.Player.Class >= 41) return true; break;
                case 42: if (client.Player.Class <= 45 && client.Player.Class >= 42) return true; break;
                case 43: if (client.Player.Class <= 45 && client.Player.Class >= 43) return true; break;
                case 44: if (client.Player.Class <= 45 && client.Player.Class >= 44) return true; break;
                case 45: if (client.Player.Class == 45) return true; break;
                #endregion
                #region Ninja
                case 50: if (client.Player.Class <= 55 && client.Player.Class >= 50) return true; break;
                case 51: if (client.Player.Class <= 55 && client.Player.Class >= 51) return true; break;
                case 52: if (client.Player.Class <= 55 && client.Player.Class >= 52) return true; break;
                case 53: if (client.Player.Class <= 55 && client.Player.Class >= 53) return true; break;
                case 54: if (client.Player.Class <= 55 && client.Player.Class >= 54) return true; break;
                case 55: if (client.Player.Class == 55) return true; break;
                #endregion
                #region Taoist
                case 190: if (client.Player.Class >= 100) return true; break;
                #endregion
                case 0: return true;
                default: return false;
            }
            return false;
        }
        public static byte GetNextRefineryItem()
        {
            if (BaseFunc.RandGet(100, false) < 30)
                return 2;
            if (BaseFunc.RandGet(100, false) < 30)
                return 1;
            return 0;
        }

        public static byte GetLevel(uint ID)
        {
            if (ItemPosition(ID) == (ushort)Role.Flags.ConquerItem.Armor || ItemPosition(ID) == (ushort)Role.Flags.ConquerItem.Head || IsShield(ID))
                return (byte)((ID % 100) / 10);
            else
                return (byte)((ID % 1000) / 10);
        }

        public uint GetNextItemQuality()
        {
            DBItem item = new DBItem();
            if (item.ID % 10 < 3 || item.ID % 10 == 9)
                return item.ID;
            var tempID = item.ID;
            if (item.ID < 5)
                tempID = (item.ID / 10) * 10 + 5;
            var newItem = Server.ItemsBase.ContainsKey(tempID + 1);
            return newItem ? tempID + 1 : item.ID;
        }
        internal static int ChanceToUpgradeQuality(uint ID)
        {
            var chance = 0;
            if (ID % 10 == 9 || ID % 10 < 3)
                return 0;
            var quality = Math.Max((byte)5, ID % 10);
            switch (quality)
            {
                case 6: chance = 50; break;
                case 7: chance = 33; break;
                case 8: chance = 20; break;
                default: chance = 100; break;
            }
            var lvl = Database.Server.ItemsBase[ID].Level;
            if (lvl > 70)
                chance = chance * (100 - (lvl - 70)) / 100;

            return Math.Max(1, chance);
        }
        public static int GetUpEpLevelInfo(uint ID)
        {
            int cost = 0;
            int nLev = GetLevel(ID);

            if (ItemPosition(ID) == (ushort)Role.Flags.ConquerItem.Armor || ItemPosition(ID) == (ushort)Role.Flags.ConquerItem.Head || IsShield(ID))
            {
                switch (nLev)
                {
                    case 5: cost = 50; break;
                    case 6: cost = 40; break;
                    case 7: cost = 30; break;
                    case 8:
                    case 9: cost = 20; break;
                    default: cost = 500; break;
                }

                int nQuality = (int)(ID % 10);
                switch (nQuality)
                {
                    case 6: cost = cost * 90 / 100; break;
                    case 7: cost = cost * 70 / 100; break;
                    case 8: cost = cost * 30 / 100; break;
                    case 9: cost = cost * 10 / 100; break;
                    default:
                        break;
                }
            }
            else
            {
                switch (nLev)
                {
                    case 11: cost = 95; break;
                    case 12: cost = 90; break;
                    case 13: cost = 85; break;
                    case 14: cost = 80; break;
                    case 15: cost = 75; break;
                    case 16: cost = 70; break;
                    case 17: cost = 65; break;
                    case 18: cost = 60; break;
                    case 19: cost = 55; break;
                    case 20: cost = 50; break;
                    case 21: cost = 45; break;
                    case 22: cost = 40; break;
                    default:
                        cost = 500;
                        break;
                }

                int nQuality = (int)(ID % 10);
                switch (nQuality)
                {
                    case 6: cost = cost * 90 / 100; break;
                    case 7: cost = cost * 70 / 100; break;
                    case 8: cost = cost * 30 / 100; break;
                    case 9: cost = cost * 10 / 100; break;
                    default:
                        break;
                }
            }
            return (100 / cost + 1) * 12 / 10;
        }
        public static int ChanceToUpgradeLevel(uint ID)
        {
            int chance = 0;
            int nLev = GetLevel(ID);
            if (ItemPosition(ID) == (ushort)Role.Flags.ConquerItem.Armor || ItemPosition(ID) == (ushort)Role.Flags.ConquerItem.Head || IsShield(ID))
            {
                switch (nLev)
                {
                    case 5: chance = 50; break;
                    case 6: chance = 40; break;
                    case 7: chance = 30; break;
                    case 8:
                    case 9: chance = 20; break;
                    default: chance = 500; break;
                }

                int nQuality = (int)(ID % 10);
                switch (nQuality)
                {
                    case 6: chance = chance * 90 / 100; break;
                    case 7: chance = chance * 70 / 100; break;
                    case 8: chance = chance * 30 / 100; break;
                    case 9: chance = chance * 10 / 100; break;
                    default:
                        break;
                }
            }
            else
            {
                switch (nLev)
                {
                    case 11: chance = 95; break;
                    case 12: chance = 90; break;
                    case 13: chance = 85; break;
                    case 14: chance = 80; break;
                    case 15: chance = 75; break;
                    case 16: chance = 70; break;
                    case 17: chance = 65; break;
                    case 18: chance = 60; break;
                    case 19: chance = 55; break;
                    case 20: chance = 50; break;
                    case 21: chance = 45; break;
                    case 22: chance = 40; break;
                    default: chance = 100; break;
                }

                int nQuality = (int)(ID % 10);
                switch (nQuality)
                {
                    case 6: chance = chance * 90 / 100; break;
                    case 7: chance = chance * 70 / 100; break;
                    case 8: chance = chance * 30 / 100; break;
                    case 9: chance = chance * 10 / 100; break;
                    default:
                        break;
                }
            }
            chance = Math.Min(100, Math.Max(2, chance));
            return chance;
        }
        public static int GetUpEpQualityInfo(uint ID)
        {
            var item = Server.ItemsBase[ID];
            int change = 100;
            switch (ID % 10)
            {
                case 6: change = 50; break;
                case 7: change = 33; break;
                case 8: change = 20; break;
                default: change = 100; break;
            }
            int level = item.Level;
            if (level > 70)
                change = (int)(change * (100 - (level - 70) * 1.0) / 100);

            return Math.Max(1, 100 / change);
        }
        public static bool UpQualityDB(uint ID, uint DBs)
        {
            int cost = GetUpEpQualityInfo(ID);
            if (DBs >= cost)
                return true;
            else
            {
                double percent = 100 / cost;
                double MyCost = DBs * percent;
                return BaseFunc.RandGet(100, true) < MyCost;
            }
        }
        public static bool UpItemMeteors(uint ID, uint Meteors)
        {
            int CompleteCost = ChanceToUpgradeLevel(ID);
            if (Meteors >= CompleteCost)
                return true;
            else
            {
                double percent = 100 / CompleteCost;
                double MyCost = Meteors * percent;
                return BaseFunc.RandGet(100, true) < MyCost;
            }
        }
        public static bool EquipPassSexReq(DBItem baseInformation, Client.GameClient client)
        {
            int ClientGender = client.Player.Body % 10000 < 1005 ? 1 : 2;
            if (baseInformation.Gender == 2 && ClientGender == 2)
                return true;
            if (baseInformation.Gender == 1 && ClientGender == 1)
                return true;
            if (baseInformation.Gender == 0)
                return true;
            return false;
        }
        public static bool EquipPassRbReq(DBItem baseInformation, Client.GameClient client)
        {
            if (baseInformation.Level < 71 && client.Player.Reborn > 0 && client.Player.Level >= 70)
                return true;
            else
                return false;
        }
        public static bool EquipPassLvlReq(DBItem baseInformation, Client.GameClient client)
        {
            if (client.Player.Level < baseInformation.Level)
                return false;
            else
                return true;
        }
        public static bool Equipable(Game.MsgServer.MsgGameItem item, Client.GameClient client)
        {
            DBItem BaseInformation = null;
            if (Server.ItemsBase.TryGetValue((uint)item.ITEM_ID, out BaseInformation))
            {
                bool pass = false;
                if (!EquipPassSexReq(BaseInformation, client))
                    return false;
                if (EquipPassRbReq(BaseInformation, client))
                    pass = true;
                else
                    if (EquipPassJobReq(BaseInformation, client) && EquipPassStatsReq(BaseInformation, client) && EquipPassLvlReq(BaseInformation, client))
                    pass = true;
                if (!pass)
                    return false;

                if (client.Player.Reborn > 0)
                {
                    if (client.Player.Level >= 70 && BaseInformation.Level <= 70)
                        return pass;
                }
                return pass;
            }
            return false;
        }
        public static bool Equipable(uint ItemID, Client.GameClient client, bool UpgradeByPass = false)
        {
            DBItem BaseInformation = null;
            if (Server.ItemsBase.TryGetValue(ItemID, out BaseInformation))
            {
                bool pass = false;

                if (EquipPassRbReq(BaseInformation, client))
                    pass = true;
                else if (UpgradeByPass)
                    if ((EquipPassJobReq(BaseInformation, client) || UpgradeByPass) && EquipPassLvlReq(BaseInformation, client))
                        pass = true;
                if (!pass)
                    return false;

                if (client.Player.Reborn > 0)
                {
                    if (client.Player.Level >= 70 && BaseInformation.Level <= 70)
                        return pass;
                }
                return pass;
            }
            return false;
        }
        public static uint GetBaseID(uint ID)
        {
            int itemtype = (int)(ID / 1000);
            if (ID == 300000)//steed
                return ID;
            switch ((byte)(ID / 10000))
            {
                case 20://tower/ Fan / RidingCrop
                case 14://hood`s. cap`s . band`s
                case 11://helment / hat / cap...
                case 90://shields
                case 13://armors
                case 12://necklace / bag / hood
                case 15://ring
                case 16://boots
                case 42:/*BackswordID*/
                    if (itemtype == 420)//Normal Sword
                        goto default;
                    ID = (uint)(ID - (ID % 10));
                    break;
                case 60:/*NinjaSwordID*/
                case 61:/*AssasinKnifeID*/
                case 62://epic tao backsword
                    ID = (uint)(ID - (ID % 10));
                    break;
                default://someting weapon`s / someting coronet`s earing`s / bow`s
                    {
                        byte def_val = (byte)(ID / 100000);
                        ID = (uint)(((def_val * 100000) + (def_val * 10000) + (def_val * 1000)) + ((ID % 1000) - (ID % 10)));
                        break;
                    }
            }
            return ID;
        }
        public class ITPlus
        {
            public uint ID;
            public byte Plus;
            public ushort ItemHP;
            public uint MinAttack;
            public uint MaxAttack;
            public ushort PhysicalDefence;
            public ushort MagicAttack;
            public ushort MagicDefence;
            public ushort Agility;
            public ushort Vigor { get { return Agility; } }
            public byte Dodge;
            public ushort SpeedPlus { get { return Dodge; } }
            public void Parse(string Line)
            {
                string[] Info = Line.Split(' ');
                ID = uint.Parse(Info[0]);
                Plus = byte.Parse(Info[1]);
                ItemHP = ushort.Parse(Info[2]);
                MinAttack = uint.Parse(Info[3]);
                MaxAttack = uint.Parse(Info[4]);
                PhysicalDefence = ushort.Parse(Info[5]);
                MagicAttack = ushort.Parse(Info[6]);
                MagicDefence = ushort.Parse(Info[7]);
                Agility = ushort.Parse(Info[8]);
                Dodge = byte.Parse(Info[9]);

            }
        }
        public class DBItem
        {
            public uint ID;
            public ITPlus[] Plus;
            public bool AllowUpgradePlus = false;
            public string Name;
            public byte Class;
            public byte Proficiency;
            public byte Level;
            public byte Gender;
            public ushort Strength;
            public ushort Agility;
            public uint GoldWorth;
            public ushort MinAttack;
            public ushort MaxAttack;
            public ushort PhysicalDefence;
            public ushort MagicDefence;
            public ushort MagicAttack;
            public byte Dodge;
            public ushort Frequency;
            public uint ConquerPointsWorth;
            public ushort Durability;
            public ushort MaxDurability;
            public ushort StackSize;
            public ushort ItemHP;
            public ushort ItemMP;
            public ushort AttackRange;
            public ItemType Type;
            public string Description;
            public int GradeKey;

            public uint Weight;
            
            public void Parse(string Line)
            {
                Plus = new ITPlus[10];
                string[] data = Line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (data[2] == "")
                {
                    for (int x = 2; x < data.Length - 1; x++)
                        data[x] = data[x + 1];
                }
                try
                {
                    if (data.Length > 39 && data[0] != "\0")
                    {
                        ID = Convert.ToUInt32(data[0]);
                        Name = data[1].Trim();
                        Class = Convert.ToByte(data[2]);
                        Proficiency = Convert.ToByte(data[3]);
                        Level = Convert.ToByte(data[4]);
                        Gender = Convert.ToByte(data[5]);
                        Strength = Convert.ToUInt16(data[6]);
                        Agility = Convert.ToUInt16(data[7]);
                        Type = Convert.ToUInt32(data[10]) == 0 ? ItemType.Dropable : ItemType.Others;
                        Weight = Convert.ToUInt32(data[11]);
                        GoldWorth = Convert.ToUInt32(data[12]);
                        MaxAttack = Convert.ToUInt16(data[14]);
                        MinAttack = Convert.ToUInt16(data[15]);
                        PhysicalDefence = Convert.ToUInt16(data[16]);
                        Frequency = Convert.ToUInt16(data[17]);
                        Dodge = Convert.ToByte(data[18]);
                        ItemHP = Convert.ToUInt16(data[19]);
                        ItemMP = Convert.ToUInt16(data[20]);
                        if (ID == 1050002)
                        {
                            Durability = 10000;
                            MaxDurability = 10000;
                        }
                        else
                        {
                            Durability = Convert.ToUInt16(data[21]);
                            MaxDurability = Convert.ToUInt16(data[22]);
                        }
                        MagicAttack = Convert.ToUInt16(data[29]);
                        MagicDefence = Convert.ToUInt16(data[30]);
                        AttackRange = Convert.ToUInt16(data[31]);
                        //   if(ID == )
                        ConquerPointsWorth = Convert.ToUInt32(data[36]);
                        // if (ID == 117309)
                        //     return;

                        //Crytical = Convert.ToUInt32(data[40]);
                        //SCrytical = Convert.ToUInt32(data[41]);
                        //Imunity = Convert.ToUInt32(data[42]);
                        //Penetration = Convert.ToUInt32(data[43]);
                        //Block = Convert.ToUInt32(data[44]);
                        //BreackTrough = Convert.ToUInt32(data[45]);
                        //ConterAction = Convert.ToUInt32(data[46]);
                        //Detoxication = Convert.ToUInt32(data[47]);

                        //MetalResistance = Convert.ToByte(data[48]);
                        //WoodResistance = Convert.ToByte(data[49]);
                        //WaterResistance = Convert.ToByte(data[50]);
                        //FireResistance = Convert.ToByte(data[51]);

                        //EarthResistance = Convert.ToByte(data[52]);
                        //StackSize = Convert.ToUInt16(data[47]);
                        //ushort.TryParse(data[47].ToString(), out StackSize);


                        Description = data[38].Replace("`s", "");
                        if (Description == "Earrings")
                            Description = "Earring";
                        if (Description == "Bow")
                            Description = "ArcherBow";
                        if (Description == "Backsword")
                            Description = "TaoistBackSword";
                        Description = Description.ToLower();

                        if (ID >= 730001 && ID <= 730009)
                        {
                            Name = "(+" + (ID % 10).ToString() + ")Stone";
                        }
                        if (ItemPosition(ID) == (ushort)Role.Flags.ConquerItem.Garment)
                            if (!Garments.ContainsKey(ID))
                                Garments.Add(ID, this);

                        if (ID == 754099)
                            ConquerPointsWorth = 299;

                        if (ID == 754999 || ID == 753999 || ID == 751999)
                            ConquerPointsWorth = 1699;

                        if (ID == 619028)
                            ConquerPointsWorth = 489;

                        if (ID == 723723)
                            ConquerPointsWorth = 7100;
                        if (ID == 3005945)
                            ConquerPointsWorth = 3100;

                    }
                    else
                        Console.WriteLine(data[0]);
                }
                catch (Exception e) {
                    Console.WriteLine(e.ToString());
                    Console.WriteLine(Line); }
            }
            public enum ItemType : byte
            {
                Dropable = 0,
                Others
            }
        }
    }
}
