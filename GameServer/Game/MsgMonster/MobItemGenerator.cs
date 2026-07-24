using System;
using System.Collections.Generic;
using System.Linq;

namespace COServer.Game.MsgMonster
{
    public class MobRateWatcher
    {
        private int tick;
        private int count;
        public static implicit operator bool(MobRateWatcher q)
        {
            bool result = false;
            q.count++;
            if (q.count == q.tick)
            {
                q.count = 0;
                result = true;
            }
            return result;
        }
        public MobRateWatcher(int Tick)
        {
            tick = Tick;
            count = 0;
        }
    }

    public struct SpecialItemWatcher
    {
        public uint ID;
        public MobRateWatcher Rate;
        public SpecialItemWatcher(uint ID, int Tick)
        {
            this.ID = ID;
            Rate = new MobRateWatcher(Tick);
        }
    }

    public class MobItemGenerator
    {
        private static ushort[] NecklaceType = new ushort[] { 120, 121 };
        private static ushort[] RingType = new ushort[] { 150, 151, 152 };
        private static ushort[] ArmetType = new ushort[] { 111, 112, 113, 114, 117, 118 };
        private static ushort[] ArmorType = new ushort[] { 130, 131, 132, 133, 134 };
        private static ushort[] OneHanderType = new ushort[] { 410, 420, 421, 430, 440, 450, 460, 480, 481, 490, 500, 601 };
        private static ushort[] TwoHanderType = new ushort[] { 510, 530, 560, 561, 580, 900, };
        private MonsterFamily Family;

        private MobRateWatcher Refined;
        private MobRateWatcher Unique;
        private MobRateWatcher Elite;
        private MobRateWatcher Super;
        private MobRateWatcher PlusOne;
        private MobRateWatcher OneBless;
        private MobRateWatcher ThereBless;
        private MobRateWatcher FiveBless;


        private MobRateWatcher OneSocketItem;
        private MobRateWatcher TwoSocketItem;

        private MobRateWatcher DropHp;
        private MobRateWatcher DropMp;

        private MobRateWatcher Meteor;

        public MobItemGenerator(MonsterFamily family)
        {

            // Chances Drop items
            Family = family;
            Refined = new MobRateWatcher(450);
            Unique = new MobRateWatcher(900);
            Elite = new MobRateWatcher(4000);
            Super = new MobRateWatcher(15000);
            PlusOne = new MobRateWatcher(2000);
            OneSocketItem = new MobRateWatcher(1000000);
            TwoSocketItem = new MobRateWatcher(2000000);

            OneBless = new MobRateWatcher(50000);
            ThereBless = new MobRateWatcher(100000);
            FiveBless = new MobRateWatcher(250000);

            DropHp = new MobRateWatcher(99999999);
            DropMp = new MobRateWatcher(99999999);
            Meteor = new MobRateWatcher((int)ProjectControl.Vip_Drop_Meteors);
        }
        public List<uint> GenerateBossFamily()
        {
            List<uint> Items = new List<uint>();
            byte rand = (byte)Program.GetRandom.Next(1, 7);
            for (int x = 0; x < 4; x++)
            {
                byte dwItemQuality = GenerateQuality();
                uint dwItemSort = 0;
                uint dwItemLev = 0;
                switch (rand)
                {
                    case 1:
                        {
                            dwItemSort = NecklaceType[Program.GetRandom.Next(0, NecklaceType.Length)];
                            dwItemLev = Family.DropNecklace;
                            break;
                        }
                    case 2:
                        {
                            dwItemSort = RingType[Program.GetRandom.Next(0, RingType.Length)];
                            dwItemLev = Family.DropRing;
                            break;
                        }
                    case 3:
                        {
                            dwItemSort = ArmorType[Program.GetRandom.Next(0, ArmorType.Length)];
                            dwItemLev = Family.DropArmor;
                            break;
                        }
                    case 4:
                        {
                            dwItemSort = TwoHanderType[Program.GetRandom.Next(0, TwoHanderType.Length)];
                            dwItemLev = ((dwItemSort == 900) ? Family.DropShield : Family.DropWeapon);
                            break;
                        }
                    default:
                        {
                            dwItemSort = OneHanderType[Program.GetRandom.Next(0, OneHanderType.Length)];
                            dwItemLev = Family.DropWeapon;
                            break;
                        }
                }
                dwItemLev = AlterItemLevel(dwItemLev, dwItemSort);
                uint idItemType = (dwItemSort * 1000) + (dwItemLev * 10) + dwItemQuality;
                if (Database.Server.ItemsBase.ContainsKey(idItemType))
                    Items.Add(idItemType);
            }
            return Items;
        }
        public uint GenerateItemId(uint map, out byte dwItemQuality, out bool Special, out Database.ItemType.DBItem DbItem)
        {
            Special = false;
            foreach (SpecialItemWatcher sp in Family.DropSpecials)
            {
                if (sp.Rate)
                {
                    Special = true;
                    dwItemQuality = (byte)(sp.ID % 10);
                    if (Database.Server.ItemsBase.TryGetValue(sp.ID, out DbItem))
                        return sp.ID;
                }
            }

            if (DropHp)
            {
                dwItemQuality = 0;
                Special = true;
                if (Database.Server.ItemsBase.TryGetValue(Family.DropHPItem, out DbItem))
                    return Family.DropHPItem;
            }
            if (DropMp)
            {
                dwItemQuality = 0;
                Special = true;
                if (Database.Server.ItemsBase.TryGetValue(Family.DropMPItem, out DbItem))
                    return Family.DropMPItem;
            }

            if (Meteor)
            {
                dwItemQuality = 0;
                Special = true;
                if (Database.Server.ItemsBase.TryGetValue(Database.ItemType.Meteor, out DbItem))
                    return Database.ItemType.Meteor;
            }
            dwItemQuality = GenerateQuality();
            uint dwItemSort = 0;
            uint dwItemLev = 0;

            int nRand = Program.GetRandom.Next(0, 1200);

            if (nRand < 200) // 16.67% - Botas
            {
                dwItemSort = 160;
                dwItemLev = Family.DropBoots;
            }
            else if (nRand < 400) // 16.67% - Colares
            {
                dwItemSort = NecklaceType[Program.GetRandom.Next(0, NecklaceType.Length)];
                dwItemLev = Family.DropNecklace;
            }
            else if (nRand < 600) // 16.67% - Anéis
            {
                dwItemSort = RingType[Program.GetRandom.Next(0, RingType.Length)];
                dwItemLev = Family.DropRing;
            }
            else if (nRand < 800) // 16.67% - Capacetes
            {
                dwItemSort = ArmetType[Program.GetRandom.Next(0, ArmetType.Length)];
                dwItemLev = Family.DropArmet;
            }
            else if (nRand < 1000) // 16.67% - Armaduras
            {
                dwItemSort = ArmorType[Program.GetRandom.Next(0, ArmorType.Length)];
                dwItemLev = Family.DropArmor;
            }
            else // 16.67% - Armas (divididas em subtipos)
            {
                int nRate = Program.GetRandom.Next(0, 100);
                if (nRate < 33) // 5.56% - Backswords
                {
                    dwItemSort = 421;
                }
                else if (nRate < 66) // 5.56% - One handers
                {
                    dwItemSort = OneHanderType[Program.GetRandom.Next(0, OneHanderType.Length)];
                    dwItemLev = Family.DropWeapon;
                }
                else // 5.56% - Two handers ou Shield
                {
                    dwItemSort = TwoHanderType[Program.GetRandom.Next(0, TwoHanderType.Length)];
                    dwItemLev = (dwItemSort == 900) ? Family.DropShield : Family.DropWeapon;
                }
            }
            if (dwItemLev != 99)
            {
                dwItemLev = AlterItemLevel(dwItemLev, dwItemSort);

                uint idItemType = (dwItemSort * 1000) + (dwItemLev * 10) + dwItemQuality;
                if (Database.Server.ItemsBase.TryGetValue(idItemType, out DbItem))
                {
                    ushort position = Database.ItemType.ItemPosition(idItemType);
                    byte level = Database.ItemType.ItemMaxLevel((Role.Flags.ConquerItem)position);
                    if (DbItem.Level > level)
                        return 0;
                    return idItemType;
                }
            }
            DbItem = null;
            return 0;
        }
        public byte GeneratePurity()
        {
            if (PlusOne)
                return 1;
            return 0;
        }
        public byte GenerateBless()
        {
            if (ThereBless)
            {
                return 3;
            }
            else if (OneBless)
            {
                return 1;
            }
            else if (FiveBless)
            {
                return 5;
            }
            return 0;
        }
        public byte GenerateSocketCount(uint ItemID)
        {
            if (ItemID >= 410000 && ItemID <= 580339)
            {
                if (TwoSocketItem)
                    return 2;
                else if (OneSocketItem) 
                    return 1;
            }
            return 0;
        }
        private byte GenerateQuality()
        {
            if (Refined)
                return 6;
            else if (Unique)
                return 7;
            else if (Elite)
                return 8;
            else if (Super)
                return 9;
            return 3;
        }
        public uint GenerateGold(out uint ItemID, bool normal = false, bool twin = false)
        {
            uint amount = (uint)Program.GetRandom.Next(100, 1000);

            ItemID = Database.ItemType.MoneyItemID(amount);
            return amount;
        }
        private uint AlterItemLevel(uint dwItemLev, uint dwItemSort)
        {
            int nRand = Program.GetRandom.Next(0, 1000) % 100;

            if (nRand < 50) // 50% down one level
            {
                uint dwLev = dwItemLev;
                dwItemLev = (uint)(Program.GetRandom.Next(0, (int)(dwLev / 2)) + dwLev / 3);

                if (dwItemLev > 1)
                    dwItemLev--;
            }
            else if (nRand > 80) // 20% up one level
            {
                if ((dwItemSort >= 110 && dwItemSort <= 114) ||
                    (dwItemSort >= 130 && dwItemSort <= 134) ||
                    (dwItemSort >= 900 && dwItemSort <= 999))
                {
                    dwItemLev = Math.Min(dwItemLev + 1, 9);
                }
                else
                {
                    dwItemLev = Math.Min(dwItemLev + 1, 23);
                }
            }

            return dwItemLev;
        }
    }
}
