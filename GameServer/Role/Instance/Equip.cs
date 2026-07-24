using COServer.Game.MsgServer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace COServer.Role.Instance
{
    public class Equip
    {
        public ConcurrentDictionary<uint, Game.MsgServer.MsgGameItem> ClientItems = new ConcurrentDictionary<uint, Game.MsgServer.MsgGameItem>();
        public uint ArmorID;
        public bool CreateSpawn = true;
        public bool SuperArmor = false;
        public bool FullSuper
        {
            get
            {
                if (!SuperArmor)
                    return false;
                foreach (var item in CurentEquip)
                {
                    if (item.Position != (ushort)Role.Flags.ConquerItem.Garment
                        && item.Position != (ushort)Role.Flags.ConquerItem.Bottle)
                    {
                        if (item.ITEM_ID % 10 != 9)
                            return false;
                    }
                }
                return true;
            }
        }
        public Role.Flags.ItemEffect RightWeaponEffect = Flags.ItemEffect.None;
        public Role.Flags.ItemEffect LeftWeaponEffect = Flags.ItemEffect.None;
        public Role.Flags.ItemEffect RingEffect = Flags.ItemEffect.None;
        public Role.Flags.ItemEffect NecklaceEffect = Flags.ItemEffect.None;
        public Role.Flags.ItemEffect BootEffect = Flags.ItemEffect.None;
        public Role.Flags.ItemEffect HeadEffect = Flags.ItemEffect.None;

        public uint ShieldID = 0;
        public uint HeadID;
        public uint RightWeapon = 0;
        public uint LeftWeapon = 0;
        public uint Boots = 0;

        public int rangeR = 0;
        public int rangeL = 0;
        public int SizeAdd = 0;

        public int SpeedR = 0;
        public int SpeedL = 0;
        public int SpeedRing = 0;

        public bool SuperDragonGem = false;
        public bool SuperPheonixGem = false;
        public bool SuperVioletGem = false;
        public bool SuperRaibowGem = false;
        public bool SuperMoonGem = false;
        public bool SuprtTortoiseGem = false;
        public bool SuperKylinGem = false;
        public bool HaveBless = false;

        public int AttackSpeed(int MS_Delay)
        {
            MS_Delay = Math.Max(300, MS_Delay - 100);

            MS_Delay = Math.Max(300, MS_Delay - SpeedR);
            MS_Delay = Math.Max(300, MS_Delay - SpeedL);
            MS_Delay = Math.Max(300, MS_Delay - SpeedRing);
            MS_Delay = Math.Max(300, MS_Delay - Owner.Player.Agility);

            if (Owner.Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Cyclone))
                MS_Delay = Math.Max(300, MS_Delay - 150);

            return MS_Delay;
        }
        public int AttackSpeed(bool physical)
        {
            int speed = 1100;//800
            speed = Math.Max(300, speed - SpeedR);
            speed = Math.Max(300, speed - SpeedL);
            speed = Math.Max(300, speed - SpeedRing);
            speed = Math.Max(300, speed - Owner.Player.Agility);

            if (Owner.Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Cyclone))
                speed = Math.Max(50, speed - 600);//600

            if (Owner.Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Superman))
                speed = Math.Max(100, speed - 500);//600

            return speed;
        }
        public int GetAttackRange(int targetSizeAdd)
        {
            var range = 1;

            if (rangeR != 0 && rangeL != 0)
                range = (rangeR + rangeL) / 2;
            else if (rangeR != 0)
                range = rangeR;
            else if (rangeL != 0)
                range = rangeL;

            range += (SizeAdd + targetSizeAdd + 1) / 2;

            return range;
        }
        
        private Client.GameClient Owner;
        public Equip(Client.GameClient client)
        {
            Owner = client;
        }
        public Game.MsgServer.MsgGameItem[] CurentEquip = new Game.MsgServer.MsgGameItem[0];

        public unsafe bool Add(ServerSockets.Packet stream, uint ID, Role.Flags.ConquerItem position, byte plus = 0, byte bless = 0, byte Enchant = 0
           , Role.Flags.Gem sockone = Flags.Gem.NoSocket
            , Role.Flags.Gem socktwo = Flags.Gem.NoSocket, bool bound = false, Role.Flags.ItemEffect Effect = Flags.ItemEffect.None)
        {
            if (FreeEquip(position))
            {
                Database.ItemType.DBItem DbItem;
                if (Database.Server.ItemsBase.TryGetValue(ID, out DbItem))
                {
                    Game.MsgServer.MsgGameItem ItemDat = new Game.MsgServer.MsgGameItem();
                    ItemDat.UID = Database.Server.ITEM_Counter.Next;
                    ItemDat.ITEM_ID = ID;
                    ItemDat.Effect = Effect;
                    ItemDat.Durability = ItemDat.MaximDurability = DbItem.Durability;
                    ItemDat.Plus = plus;
                    ItemDat.Bless = bless;
                    ItemDat.Enchant = Enchant;
                    ItemDat.SocketOne = sockone;
                    ItemDat.SocketTwo = socktwo;
                    ItemDat.Color = (Role.Flags.Color)Program.GetRandom.Next(3, 9);
                    ItemDat.Bound = (byte)(bound ? 1 : 0);
                    CheakUp(ItemDat);
                    ItemDat.Position = (ushort)position;
                    ItemDat.Mode = Flags.ItemMode.AddItem;

                    ItemDat.Send(Owner, stream);


                    Owner.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsuageID.Equip, ItemDat.UID, ItemDat.Position, 0, 0, 0, 0));

                    return true;

                }
            }
            return false;
        }
        private void CheakUp(Game.MsgServer.MsgGameItem ItemDat)
        {
            if (ItemDat.UID == 0)
                ItemDat.UID = Database.Server.ITEM_Counter.Next;

            if (!ClientItems.TryAdd(ItemDat.UID, ItemDat))
            {
                do
                {
                  //  Console.WriteLine("Modifica uidul  " + ItemDat.UID);
                    ItemDat.UID = Database.Server.ITEM_Counter.Next;
                }
                while
                  (ClientItems.TryAdd(ItemDat.UID, ItemDat) == false);
            }
        }

        public bool Exist(Func<Game.MsgServer.MsgGameItem, bool> predicate)
        {
            bool Exist = false;
            foreach (var item in CurentEquip)
                if (predicate(item))
                {
                    Exist = true;
                    break;
                }
            return Exist;
        }
        public void Have(Func<Game.MsgServer.MsgGameItem, bool> predicate, out int count)
        {
            count = 0;
            foreach (var item in CurentEquip)
                if (predicate(item))
                {
                    count++;
                }

        }
        public bool Exist(Func<Game.MsgServer.MsgGameItem, bool> predicate, int count)
        {
            int counter = 0;
            foreach (var item in CurentEquip)
                if (predicate(item))
                {
                    counter++;
                }
            return counter >= count;
        }
        public ICollection<Game.MsgServer.MsgGameItem> AllItems
        {
            get { return ClientItems.Values; }
        }
        public bool TryGetValue(uint UID, out Game.MsgServer.MsgGameItem itemdata)
        {
            return ClientItems.TryGetValue(UID, out itemdata);
        }
        public bool FreeEquip(Role.Flags.ConquerItem position)
        {
            var item = ClientItems.Values.Where(p => p.Position == (ushort)position)
                .FirstOrDefault();
            return item == null;
        }
        public bool TryGetEquip(Role.Flags.ConquerItem position, out Game.MsgServer.MsgGameItem itemdata)
        {

            itemdata = ClientItems.Values.Where(p => p.Position == (ushort)position).FirstOrDefault();
            return itemdata != null;
        }
        public Game.MsgServer.MsgGameItem TryGetEquip(Role.Flags.ConquerItem position)
        {
            return ClientItems.Values.Where(p => p.Position == (ushort)position).FirstOrDefault();
        }
        public bool Remove(Role.Flags.ConquerItem position, ServerSockets.Packet stream)
        {
            if (Owner.Player.ContainFlag(MsgUpdate.Flags.Fly))
                Owner.Player.RemoveFlag(MsgUpdate.Flags.Fly);
            if (!FreeEquip(position))
            {
                bool Accept = Owner.Inventory.HaveSpace(1);
                if (Accept)
                {
                    Game.MsgServer.MsgGameItem itemdata;
                    if (TryGetEquip(position, out itemdata))
                    {
                        if (ClientItems.TryRemove(itemdata.UID, out itemdata))
                        {
                            Owner.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsuageID.Unequip, itemdata.UID, itemdata.Position, 0, 0, 0, 0));

                            itemdata.Position = 0;
                            itemdata.Mode = Flags.ItemMode.AddItem;
                            Owner.Inventory.Update(itemdata, AddMode.MOVE, stream);

                        }
                    }
                }
                else
                {
                    Owner.SendSysMesage("Your inventory is full.");
                }
                return Accept;
            }
            else
                return false;
        }
        public void Add(Game.MsgServer.MsgGameItem item, ServerSockets.Packet stream)
        {
            if (item.Position != (ushort)Role.Flags.ConquerItem.Tower && item.Position != (ushort)Role.Flags.ConquerItem.Fan)
            {
                CheakUp(item);
                ClientItems.TryAdd(item.UID, item);
                item.Mode = Flags.ItemMode.AddItem;
                item.Send(Owner, stream);
            }
        }
        public void Show(ServerSockets.Packet stream, bool eqtemp = true)
        {
            foreach (var item in ClientItems.Values)
            {
                
                if (item.Position == (ushort)Role.Flags.ConquerItem.Garment)
                {
                    uint id = 0;
                    if (Owner.Player.Map == EventsLib.EventManager.deathmatch.map && Owner.DMTeam != 0)
                        id = EventsLib.EventManager.deathmatch.garments[Owner.DMTeam - 1];
                    else if (Owner.Player.Map == EventsLib.EventManager.ctb.map)
                    {
                        if (Owner.TeamColor == EventsLib.CTBTeam.Blue)
                            id = 181805;
                        else id = 181625;
                    }
                    else if (Owner.Player.Map == EventsLib.EventManager.killthecaptain.map)
                    {
                        if (Owner.TeamKillTheCaptain == EventsLib.KillTheCaptainTeams.Blue)
                            id = 181825;
                        else id = 181625;
                    }
                    else if (Owner.Player.Map == EventsLib.EventManager.teamfreezewar.map)
                    {
                        if (Owner.TeamFreeze == EventsLib.FreezeWarTeams.Blue)
                            id = 181825;
                        else id = 181625;
                    }
                    if (id != 0)
                    {
                        Owner.TempGarmentID = item.ITEM_ID;
                        item.ITEM_ID = id;
                    }
                    if (eqtemp)
                    {
                        if (Owner.TempGarmentID != 0)
                        {
                            item.ITEM_ID = Owner.TempGarmentID;
                            Owner.TempGarmentID = 0;
                        }
                    }
                    item.Mode = Flags.ItemMode.AddItem;
                    item.Send(Owner, stream);
                }
                else
                {

                    item.Mode = Flags.ItemMode.AddItem;
                    item.Send(Owner, stream);
                }
            }
            QueryEquipment();
        }
        public unsafe void ClearItemSpawn()
        {
            Owner.Player.ClearItemsSpawn();
        }
        public unsafe void AddSpawn(Game.MsgServer.MsgGameItem DataItem)
        {

            switch ((Role.Flags.ConquerItem)DataItem.Position)
            {
                case Role.Flags.ConquerItem.Armor:
                    {
                        Owner.Player.ArmorId = DataItem.ITEM_ID;
                        Owner.Player.ColorArmor = (ushort)DataItem.Color;

                        break;
                    }
                case Role.Flags.ConquerItem.Head:
                    {

                        Owner.Player.HeadId = DataItem.ITEM_ID;
                        Owner.Player.ColorHelment = (ushort)DataItem.Color;
                        break;
                    }
                case Role.Flags.ConquerItem.LeftWeapon:
                    {
                        Owner.Player.LeftWeaponId = DataItem.ITEM_ID;
                        break;
                    }
                case Role.Flags.ConquerItem.RightWeapon:
                    {
                        Owner.Player.RightWeaponId = DataItem.ITEM_ID;
                        Owner.Player.ColorShield = (ushort)DataItem.Color;
                        break;
                    }
                case Role.Flags.ConquerItem.Garment:
                    {

                        Owner.Player.GarmentId = DataItem.ITEM_ID;
                        break;
                    }
            }
        }
        public unsafe void UpdateStats(Game.MsgServer.MsgGameItem[] MyGear, ServerSockets.Packet stream)
        {

            try
            {

                rangeR = rangeL = SizeAdd = 0;
                SpeedR = SpeedL = SpeedRing = 0;
                RightWeapon = 0;
                LeftWeapon = 0;
                SuperArmor = false;
                HeadID = 0;
                HaveBless = false;
                RingEffect = Flags.ItemEffect.None;
                RightWeaponEffect = Flags.ItemEffect.None;
                LeftWeaponEffect = Flags.ItemEffect.None;
                if (CreateSpawn)
                {
                    lock (CurentEquip)
                        CurentEquip = MyGear;
                    ClearItemSpawn();
                }
                Owner.Status = new MsgStatus();
                Owner.Status.UID = Owner.Player.UID;

                Owner.Status.MaxAttack = (ushort)(Owner.Player.Strength + 1);
                Owner.Status.MinAttack = (ushort)(Owner.Player.Strength);
                Owner.Status.MagicAttack = Owner.Player.Spirit;// (ushort)(Owner.Player.Spirit * 10);

                Owner.Gems = new ushort[13];

                foreach (var item in MyGear)
                {

                    if (Database.ItemType.ItemPosition(item.ITEM_ID) == (ushort)Role.Flags.ConquerItem.Head)
                        HeadID = item.ITEM_ID;
                    //aici sa fac schimbare de position !!!!!!!!!!!
                    try
                    {

                        if (CreateSpawn)
                            AddSpawn(item);

                        if (item.Durability == 0)
                            continue;

                        ushort ItemPostion = (ushort)(item.Position % 20);


                        if (item.Bless >= 1)
                            HaveBless = true;
                        if (ItemPostion == (ushort)Role.Flags.ConquerItem.Armor)
                        {
                            SuperArmor = (item.ITEM_ID % 10) == 9;

                            ArmorID = item.ITEM_ID;
                        }
                        if (item.SocketOne != Role.Flags.Gem.NoSocket && item.SocketOne != Role.Flags.Gem.EmptySocket)
                        {
                            if (item.SocketOne == Role.Flags.Gem.SuperDragonGem)
                                SuperDragonGem = true;
                            if (item.SocketOne == Role.Flags.Gem.SuperPhoenixGem)
                                SuperPheonixGem = true;
                            if (item.SocketOne == Role.Flags.Gem.SuperVioletGem)
                                SuperVioletGem = true;
                            if (item.SocketOne == Role.Flags.Gem.SuperRainbowGem)
                                SuperRaibowGem = true;
                            if (item.SocketOne == Role.Flags.Gem.SuperMoonGem)
                                SuperMoonGem = true;
                            if (item.SocketOne == Role.Flags.Gem.SuperKylinGem)
                                SuperKylinGem = true;
                        }


                        if (ItemPostion == (ushort)Role.Flags.ConquerItem.LeftWeapon)
                        {
                            LeftWeapon = item.ITEM_ID;
                            LeftWeaponEffect = item.Effect;
                            if (Database.ItemType.IsShield(item.ITEM_ID))
                            {
                                ShieldID = item.ITEM_ID;
                            }
                        }
                        if (ItemPostion == (ushort)Role.Flags.ConquerItem.RightWeapon)
                        {
                            RightWeaponEffect = item.Effect;
                            RightWeapon = item.ITEM_ID;
                        }
                        if (ItemPostion == (ushort)Role.Flags.ConquerItem.Boots)
                        {
                            Boots = item.ITEM_ID;
                        }

                        if (ItemPostion == (ushort)Role.Flags.ConquerItem.Ring)
                            RingEffect = item.Effect;

                        if (ItemPostion == (ushort)Role.Flags.ConquerItem.Necklace)
                            NecklaceEffect = item.Effect;

                        AddGem(item.SocketOne);
                        AddGem(item.SocketTwo);
                        if (!Database.Server.ItemsBase.ContainsKey(item.ITEM_ID))
                            continue;
                        var DBItem = Database.Server.ItemsBase[item.ITEM_ID];

                        // Console.WriteLine(item.Position + " " + item.ItemPoints);
                        if (ShieldID != 0)
                        {
                            if (Database.ItemType.IsShield(item.ITEM_ID))
                            {
                                Owner.Status.ShieldDefenece += DBItem.PhysicalDefence;
                            }
                        }
                        if (ItemPostion == (byte)Role.Flags.ConquerItem.Fan)
                        {
                            Owner.Status.PhysicalDamageIncrease += DBItem.MaxAttack;
                            Owner.Status.MagicDamageIncrease += DBItem.MagicAttack;
                        }
                        else
                        {
                            if (ItemPostion == (ushort)Role.Flags.ConquerItem.Ring)
                            {
                                SpeedRing = DBItem.Frequency;
                            }
                            if (ItemPostion == (ushort)Role.Flags.ConquerItem.LeftWeapon)
                            {
                                rangeL = DBItem.AttackRange;
                                SpeedL = DBItem.Frequency;

                                Owner.Status.MaxAttack += (uint)(DBItem.MaxAttack * 0.5F);
                                Owner.Status.MinAttack += (uint)(DBItem.MinAttack * 0.5F);
                                Owner.Status.MagicAttack += (uint)DBItem.MagicAttack;
                            }
                            else
                            {
                                if (ItemPostion == (ushort)Role.Flags.ConquerItem.RightWeapon)
                                {
                                    rangeR = DBItem.AttackRange;
                                    SpeedR = DBItem.Frequency;
                                }

                                Owner.Status.MaxAttack += DBItem.MaxAttack;
                                Owner.Status.MinAttack += DBItem.MinAttack;
                                Owner.Status.MagicAttack += DBItem.MagicAttack;
                            }
                        }

                        if (ItemPostion == (byte)Role.Flags.ConquerItem.Tower)
                        {
                            Owner.Status.MagicDamageDecrease += DBItem.MagicDefence;
                            Owner.Status.PhysicalDamageDecrease += DBItem.PhysicalDefence;
                        }
                        else
                        {

                            Owner.Status.MDefence += DBItem.MagicDefence;
                            Owner.Status.Defence += DBItem.PhysicalDefence;
                        }
                        Owner.Status.Dodge += DBItem.Dodge;
                        Owner.Status.AgilityAtack += DBItem.Frequency;//.Agility;
                        Owner.Status.ItemBless += item.Bless;
                        Owner.Status.MaxHitpoints += item.Enchant;

                        Owner.Status.MaxHitpoints += DBItem.ItemHP;
                        Owner.Status.MaxMana += DBItem.ItemMP;


                        if (item.Plus > 0)
                        {
                            try
                            {
                                var extraitematributes = DBItem.Plus[item.Plus];
                                if (extraitematributes != null)
                                {
                                    if (ItemPostion == (ushort)Role.Flags.ConquerItem.LeftWeapon && ShieldID != 0)
                                    {
                                        if (Database.ItemType.IsShield(item.ITEM_ID))
                                        {
                                            Owner.Status.ShieldDefenece += extraitematributes.PhysicalDefence;
                                        }
                                    }
                                    if (ItemPostion == (ushort)Role.Flags.ConquerItem.LeftWeapon || ItemPostion == (ushort)Role.Flags.ConquerItem.RightWeapon)
                                    {
                                        Owner.Status.Accuracy += extraitematributes.Agility;
                                    }

                                    if (ItemPostion == (byte)Role.Flags.ConquerItem.Fan)
                                    {
                                        Owner.Status.PhysicalDamageIncrease += extraitematributes.MaxAttack;
                                        Owner.Status.MagicDamageIncrease += extraitematributes.MagicAttack;
                                    }
                                    else
                                    {
                                        Owner.Status.MaxAttack += extraitematributes.MaxAttack;
                                        Owner.Status.MinAttack += extraitematributes.MinAttack;
                                        Owner.Status.MagicAttack += extraitematributes.MagicAttack;
                                    }
                                    if (ItemPostion == (byte)Role.Flags.ConquerItem.Tower)
                                    {
                                        Owner.Status.MagicDamageDecrease += extraitematributes.MagicDefence;
                                        Owner.Status.PhysicalDamageDecrease += extraitematributes.PhysicalDefence;
                                    }
                                    else
                                    {
                                        Owner.Status.MagicDefence += extraitematributes.MagicDefence;
                                        Owner.Status.Defence += extraitematributes.PhysicalDefence;
                                    }


                                    Owner.Status.Dodge += extraitematributes.Dodge;
                                    //   Owner.Status.AgilityAtack += extraitematributes.Agility;
                                    Owner.Status.MaxHitpoints += extraitematributes.ItemHP;
                                }
                                else
                                    Console.WriteLine("Invalid plus -> item " + item.ITEM_ID.ToString() + " ->  plus " + item.Plus.ToString() + "");
                            }
                            catch
                            {
                                Console.WriteLine("Invalid plus -> item " + item.ITEM_ID.ToString() + " ->  plus " + item.Plus.ToString() + "");
                            }
                        }
                    }
                    catch (Exception e) { Console.WriteLine(e.ToString()); }
                }

                //add gem stats
                // Owner.Status.MinAttack = (uint)(Owner.Status.MinAttack * Owner.GemValues(Role.Flags.Gem.NormalDragonGem)) / 100;
                //  Owner.Status.MaxAttack = (uint)(Owner.Status.MaxAttack * Owner.GemValues(Role.Flags.Gem.NormalDragonGem)) / 100;

                // Console.WriteLine(Owner.Status.MaxAttack);
                Owner.Status.MaxAttack = (uint)(Owner.Status.MaxAttack * (1 + Owner.Status.PhysicalPercent / 100f));
                Owner.Status.MinAttack = (uint)(Owner.Status.MinAttack * (1 + Owner.Status.PhysicalPercent / 100f));
                Owner.Status.MagicAttack = (uint)(Owner.Status.MagicAttack * (1 + Owner.Status.MagicPercent / 100f));


                Owner.Status.MaxHitpoints += Owner.CalculateHitPoint();
                Owner.Status.MaxMana += Owner.CalculateMana();

                CalculateBattlePower();

                if (CreateSpawn)
                    Owner.Player.View.SendView(Owner.Player.GetArray(stream, false), false);

                SendMentorShare(stream);
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }

        public unsafe void SendMentorShare(ServerSockets.Packet stream)
        {
            Game.MsgServer.MsgApprenticeInformation Information = Game.MsgServer.MsgApprenticeInformation.Create();
            Information.Mode = Game.MsgServer.MsgApprenticeInformation.Action.Mentor;
            if (Owner.Player.MyMentor != null)
            {
                if (Owner.Player.Associate.Associat.ContainsKey(Role.Instance.Associate.Mentor))
                {
                    if (Owner.Player.MyMentor.MyClient != null)
                    {
                        if (Owner.Player.Associate.Associat[Role.Instance.Associate.Mentor].ContainsKey(Owner.Player.MyMentor.MyUID))
                        {
                            Role.Player mentor = Owner.Player.MyMentor.MyClient.Player;
                            Owner.Player.SetMentorBattlePowers(mentor.GetShareBattlePowers((uint)Owner.Player.RealBattlePower), (uint)mentor.RealBattlePower);

                            Information.Mentor_ID = mentor.UID;
                            Information.Apprentice_ID = Owner.Player.UID;
                            Information.Enrole_date = (uint)Owner.Player.Associate.Associat[Role.Instance.Associate.Mentor][mentor.UID].Timer;
                            Information.Fill(mentor.Owner);
                            Information.Shared_Battle_Power = mentor.GetShareBattlePowers((uint)Owner.Player.RealBattlePower);
                            Information.WriteString(mentor.Name, Owner.Player.Spouse, Owner.Player.Name);
                            Owner.Send(Information.GetArray(stream));
                        }
                    }

                }
            }
            if (Owner.Player.Associate != null)
            {
                if (!Owner.Player.Associate.Associat.ContainsKey(Role.Instance.Associate.Apprentice))
                    return;
            }
       

            foreach (var Apprentice in Owner.Player.Associate.OnlineApprentice.Values)
            {
                if (!Owner.Player.Associate.Associat[Role.Instance.Associate.Apprentice].ContainsKey(Apprentice.Player.UID))
                    continue;
                Role.Player target = Apprentice.Player;

                target.SetMentorBattlePowers(Owner.Player.GetShareBattlePowers((uint)target.RealBattlePower), (uint)Owner.Player.RealBattlePower);

                Information.Apprentice_ID = target.UID;
                Information.Enrole_date = (uint)Owner.Player.Associate.Associat[Role.Instance.Associate.Apprentice][target.UID].Timer;
                Information.Level = (byte)Owner.Player.Level;
                Information.Class = Owner.Player.Class;
                Information.PkPoints = Owner.Player.PKPoints;
                Information.Mesh = Owner.Player.Mesh;
                Information.Online = 1;
                Information.Shared_Battle_Power = Owner.Player.GetShareBattlePowers((uint)target.RealBattlePower);
                Information.WriteString(Owner.Player.Name, Owner.Player.Spouse, target.Name);
                target.Owner.Send(Information.GetArray(stream));
            }

        }
        public void AddGem(Role.Flags.Gem gem)
        {
            switch (gem)
            {
                case Role.Flags.Gem.NormalPhoenixGem:
                case Role.Flags.Gem.NormalDragonGem: Owner.AddGem(gem, 5); break;
                case Role.Flags.Gem.RefinedPhoenixGem:
                case Role.Flags.Gem.RefinedDragonGem: Owner.AddGem(gem, 10); break;
                case Role.Flags.Gem.SuperPhoenixGem:
                case Role.Flags.Gem.SuperDragonGem: Owner.AddGem(gem, 15); break;
                case Role.Flags.Gem.SuperRainbowGem: Owner.AddGem(gem, 25); break;
                case Flags.Gem.RefinedRainbowGem: Owner.AddGem(gem, 15); break;
                case Flags.Gem.NormalRainbowGem: Owner.AddGem(gem, 10); break;

            }
        }
        public int BattlePower = 0;
        public void CalculateBattlePower()
        {
            BattlePower = 0;
            int val = 0;
            int val_item = 0;

            foreach (var item in CurentEquip)
            {
                if (Database.ItemType.ItemPosition(item.ITEM_ID) == (ushort)Role.Flags.ConquerItem.Bottle || Database.ItemType.ItemPosition(item.ITEM_ID) == (ushort)Role.Flags.ConquerItem.Garment)
                    continue;
                val_item = 0;
                byte Quality = (byte)(item.ITEM_ID % 10);
                switch (Quality)
                {
                    case 9: val_item += 4; break;
                    case 8: val_item += 3; break;
                    case 7: val_item += 2; break;
                    case 6: val_item += 1; break;
                }
                val_item += item.Plus;

                if (item.SocketOne != Role.Flags.Gem.NoSocket)
                    val_item += 1;
                if ((byte)(((byte)item.SocketOne % 10) - 3) == 0)
                    val_item += 1;
                if (item.SocketTwo != Role.Flags.Gem.NoSocket)
                    val_item += 1;
                if ((byte)(((byte)item.SocketTwo % 10) - 3) == 0)
                    val_item += 1;

                if (Database.ItemType.IsBacksword(item.ITEM_ID))
                {
                    val_item *= 2;
                }
                if (Database.ItemType.IsBow(item.ITEM_ID))
                {
                    val_item *= 2;
                }
                else if (Database.ItemType.IsTwoHand(item.ITEM_ID) && FreeEquip(Flags.ConquerItem.LeftWeapon))
                {
                    val_item += val_item;
                }

                val += val_item;
            }
            BattlePower = val;
        }
        public void OnDequeue()
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                try
                {
                    Dictionary<uint, Game.MsgServer.MsgGameItem> statusitens = new Dictionary<uint, Game.MsgServer.MsgGameItem>();

                    foreach (var it in AllItems)
                    {
                        if (it.Position < 20)
                            if (!statusitens.ContainsKey(it.Position))
                                statusitens.Add(it.Position, it);
                    }
                   //AppendItems(CreateSpawn, statusitens.Values.ToArray(), stream);
                    UpdateStats(statusitens.Values.ToArray(), stream);

                    Owner.Player.HitPoints = Math.Min((int)Owner.Player.HitPoints, (int)Owner.Status.MaxHitpoints);
                    if (Owner.Player.OnTransform && Owner.Player.TransformInfo != null)
                        Owner.Player.TransformInfo.UpdateStatus();
                    else
                        Owner.Player.SendUpdateHP();
                }
                catch (Exception e)
                {
                    Console.SaveException(e);
                }


            }
        }
        public void AppendItems(bool CreateSpawn, Game.MsgServer.MsgGameItem[] Items, ServerSockets.Packet stream)
        {
            Game.MsgServer.MsgShowEquipment ShowEquip = new MsgShowEquipment();
            ShowEquip.wParam = Game.MsgServer.MsgShowEquipment.Show;

            if (CreateSpawn)
            {
                foreach (var item in Items)
                {
                    if (item != null)
                    {
                        switch ((Role.Flags.ConquerItem)item.Position)
                        {
                            case Flags.ConquerItem.Ring:
                                ShowEquip.Ring = item.UID; break;
                            case Flags.ConquerItem.Head: ShowEquip.Head = item.UID; break;
                            case Flags.ConquerItem.Necklace: ShowEquip.Necklace = item.UID; break;
                            case Flags.ConquerItem.RightWeapon: ShowEquip.RightWeapon = item.UID; break;
                            case Flags.ConquerItem.LeftWeapon: ShowEquip.LeftWeapon = item.UID; break;
                            case Flags.ConquerItem.Armor:
                                {
                                    ShowEquip.Armor = item.UID;
                                    break;
                                }
                            case Flags.ConquerItem.Boots: ShowEquip.Boots = item.UID; break;
                            case Flags.ConquerItem.Bottle: ShowEquip.Bottle = item.UID; break;
                            case Flags.ConquerItem.Garment:
                                {
                                    if (Owner.Player.Map == EventsLib.EventManager.deathmatch.map && Owner.DMTeam != 0)
                                        ShowEquip.Garment = EventsLib.EventManager.deathmatch.garments[Owner.DMTeam - 1];
                                    else if (Owner.Player.Map == EventsLib.EventManager.ctb.map)
                                    {
                                        if (Owner.TeamColor == EventsLib.CTBTeam.Blue)
                                            ShowEquip.Garment = 181805;
                                        else ShowEquip.Garment = 181625;
                                    }
                                    else if (Owner.Player.Map == EventsLib.EventManager.killthecaptain.map)
                                    {
                                        if (Owner.TeamKillTheCaptain == EventsLib.KillTheCaptainTeams.Blue)
                                            ShowEquip.Garment = 181825;
                                        else ShowEquip.Garment = 181625;
                                    }
                                    else if (Owner.Player.Map == EventsLib.EventManager.teamfreezewar.map)
                                    {
                                        if (Owner.TeamFreeze == EventsLib.FreezeWarTeams.Blue)
                                            ShowEquip.Garment = 181825;
                                        else ShowEquip.Garment = 181625;
                                    }
                                    ShowEquip.Garment = item.UID;
                                    break;
                                }
                        }
                    }
                }
                //if (Owner.Player.SpecialGarment != 0)
                //    ShowEquip.Garment = uint.MaxValue - 1;
                //Owner.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsuageID.SetEquipPosition, ShowEquip.UID, ShowEquip.Durability, 0, 0, 0, 0));

                //Owner.Send(stream.ShowEquipmentCreate(ShowEquip));


            }
        }
        public unsafe void QueryEquipment(bool CallItems = true)
        {
            CreateSpawn = CallItems;
            OnDequeue();
        }

        public bool DestoyArrow(Role.Flags.ConquerItem position, ServerSockets.Packet stream)
        {
            if (!FreeEquip(position))
            {
                Game.MsgServer.MsgGameItem itemdata;
                if (TryGetEquip(position, out itemdata))
                {
                    if (!(itemdata.ITEM_ID >= 1050000 && itemdata.ITEM_ID <= 1051000))
                        return false;
                    if (ClientItems.TryRemove(itemdata.UID, out itemdata))
                    {
                        Owner.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsuageID.Unequip, itemdata.UID, itemdata.Position, 0, 0, 0, 0));
                        itemdata.Position = 0;
                        itemdata.Mode = Flags.ItemMode.AddItem;
                        Owner.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsuageID.RemoveInventory, itemdata.UID, 0, 0, 0, 0, 0));
                    }
                }
            }
            return false;
        }

        public bool Contains(uint UID)
        {
            return ClientItems.ContainsKey(UID);
        }
        public bool Delete(Role.Flags.ConquerItem position, ServerSockets.Packet stream)
        {
            if (!FreeEquip(position))
            {
                bool Accept = Owner.Inventory.HaveSpace(1);
                if (Accept)
                {
                    Game.MsgServer.MsgGameItem itemdata;
                    if (TryGetEquip(position, out itemdata))
                    {
                        if (ClientItems.TryRemove(itemdata.UID, out itemdata))
                        {
                            if (position == Flags.ConquerItem.Garment)
                            {
                                itemdata.Position = 0;
                            }
                            else
                            {
                                Owner.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsuageID.Unequip, itemdata.UID, itemdata.Position, 0, 0, 0, 0));
                                itemdata.Position = 0;
                                itemdata.Mode = Flags.ItemMode.AddItem;
                                Owner.Inventory.Update(itemdata, AddMode.REMOVE, stream);
                            }
                        }
                    }
                }
                else
                {
                    Owner.SendSysMesage("Your inventory is full.");
                }
                return Accept;
            }
            else
                return false;
        }

    }
}
