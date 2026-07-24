using COServer.Game.MsgNpc;
using System;
using System.Collections.Generic;

namespace COServer.Game.MsgServer
{
    public unsafe static class MsgItemUsuagePacket
    {
        public enum ItemUsuageID : uint
        {
            CreateSocketItem = 43,
            BuyItemFromForging = 55,
            GemCompose = 39,
            ToristSuper = 51,
            AddBless = 40,
            GarmentShop = 53,
            BuyItem = 0x01,
            SellItem = 0x02,
            RemoveInventory = 0x03,
            Equip = 0x04,
            SetEquipPosition = 0x05,
            Unequip = 0x06,
            UpgradeEnchant = 0x07,
            ArrowReload = 8,
            ShowWarehouseMoney = 0x09,
            DepositWarehouse = 0x0A,
            WarehouseWithdraw = 11,
            RepairVIP = 0x0F,
            RepairItem = 0x0E,
            UpdateDurability = 0x11,
            RemoveEquipment = 0x12,
            UpgradeDragonball = 0x13,
            UpgradeMeteor = 0x14,
            ShowVendingList = 0x15,
            AddVendingItemGold = 0x16,
            RemoveVendingItem = 0x17,
            BuyVendingItem = 0x18,
            UpdateArrowCount = 0x19,
            ParticleEffect = 0x1A,
            Ping = 0x1B,
            UpdateEnchant = 0x1C,
            AddVendingItemConquerPts = 0x1D,
            UpdatePurity = 0x23,
            DropItem = 0x25,
            DropGold = 0x26,
            RedeemGear = 32,
            ClaimGear = 33,
            UnAlternante = 44,
            ActiveItems = 41,
            Alternante = 45,
            SocketTalismanWithItem = 35,
            SocketTalismanWithCPs = 36,
            MergeStackableItems = 48,
            ReturnedItems = 50,
            ShowItem = 52,
            DegradeEquipment = 54,
            OpenInventorySash = 56,
            SplitStack = 49
        }

        public static void GetUsageItem(this ServerSockets.Packet msg, out ItemUsuageID action, out uint id, out ulong dwParam, out uint timestamp, out uint dwParam2, out uint dwParam3, out uint dwparam4, out List<uint> args)
        {
            // la 16 s`a adaugat un uint


            //MyConsole.PrintPacketAdvanced(msg.Memory);
            //uint timer = msg.ReadUInt32();//4

            id = msg.ReadUInt32();//8
            dwParam = msg.ReadUInt32();//12
            action = (ItemUsuageID)msg.ReadInt32();//16

            timestamp = msg.ReadUInt32();//20
            //amount = msg.ReadInt32();
            dwParam2 = msg.ReadUInt32();//24

            dwParam3 = msg.ReadUInt32();//26

            dwparam4 = msg.ReadUInt32();

            // client never sends the equipment list
            // msg.SeekForward(3 * sizeof(int));.

            //msg.SeekForward(sizeof(int)); // more space/padding
            //msg.SeekForward(sizeof(byte));

            args = new List<uint>();
            //
            // if (dwParam2 > 0 && dwParam2 < 50)
            // {
            //     //          Console.WriteLine(msg.Position);
            //     msg.SeekForward(4);
            //     for (int i = 0; i < dwParam2; i++)
            //     {
            //         args.Add(msg.ReadUInt32());
            //     }
            // }
        }
        public unsafe static ServerSockets.Packet ItemUsageCreate(this ServerSockets.Packet msg, ItemUsuageID action, uint id, ulong dwParam1, uint timestamp, uint dwParam2, uint dwParam3, uint dwparam4, List<uint> args = null)
        {
            msg.InitWriter();
            msg.Write(id);// 4
            msg.Write((uint)dwParam1);// 8
            msg.Write((uint)action);// 12
            msg.Write((timestamp != 0) ? timestamp : Time32.Now.Value);// 16
            msg.Write(dwParam2); //20

            //msg.Write(dwParam3);//24
            //msg.Write(dwparam4);// 28

            //msg.SeekForward(3 * sizeof(int));//12
            //// msg.SeekForward(sizeof(int));
            ////Console.WriteLine("SeekForward + " + msg.Position);
            //
            //if (args != null)
            //{
            //    foreach (int arg in args)
            //        msg.Write(arg);
            //}
            // Console.WriteLine("ItemUsageCreate + " + msg.Position);

            msg.Finalize(GamePackets.Usage);


            return msg;
        }
        public static unsafe ServerSockets.Packet GarUsge(this ServerSockets.Packet stream)
        {
            stream.InitWriter();
            stream.Write(0);// 4
            stream.Write((uint)9);// 8
            stream.Write((uint)6);// 12
            stream.Finalize(GamePackets.Usage);
            return stream;
        }
        [PacketAttribute(GamePackets.Usage)]
        public unsafe static void ItemUsuage(Client.GameClient client, ServerSockets.Packet stream)
        {


            ItemUsuageID action;
            uint id;

            ulong dwParam;
            uint timestamp;
            uint dwParam2;
            uint dwParam3;
            uint dwparam4;//unknow
            List<uint> args;


            stream.GetUsageItem(out action, out id, out dwParam, out timestamp, out dwParam2, out dwParam3, out dwparam4, out args);

            switch (action)
            {
                case ItemUsuageID.UpgradeMeteor:
                case ItemUsuageID.UpgradeDragonball:
                    {
                        uint ItemUID = (uint)dwParam;

                        MsgGameItem DataItem;
                        MsgGameItem itemuse;

                        if (client.TryGetItem(ItemUID, out itemuse) && client.TryGetItem(id, out DataItem))
                        {
                            ushort Position = Database.ItemType.ItemPosition(DataItem.ITEM_ID);
                            //anti proxy --------------------
                            if (!Database.ItemType.AllowToUpdate((Role.Flags.ConquerItem)Position))
                            {
                                client.SendSysMesage("This item's level can't be upgraded anymore.");
                                return;
                            }
                            if (Database.ItemType.IsArrow(DataItem.ITEM_ID))
                                return;
                            if (DataItem.Durability < DataItem.MaximDurability)
                            {
                                client.SendSysMesage("Please repair this item first.");
                                return;
                            }
                            if (DataItem.Bound >= 1)
                            {
                                client.SendSysMesage("You can't upgrade free items.");
                                return;
                            }
                            //bool worked = true;
                            //------------------------
                            if (itemuse.ITEM_ID == Database.ItemType.DragonBall)
                            {
                                Database.ItemType.DBItem DBItem;
                                if (Database.Server.ItemsBase.TryGetValue(DataItem.ITEM_ID, out DBItem))
                                {
                                    if (DataItem.ITEM_ID % 10 == 9)
                                    {
                                        client.SendSysMesage("This item's can't be upgraded anymore.");
                                        return;
                                    }
                                    byte Chance = (byte)(70 - ((DBItem.Level - (DBItem.Level > 100 ? 30 : 0)) / (10 - DataItem.ITEM_ID % 10)));

                                    //var Chance = Database.ItemType.ChanceToUpgradeQuality(DataItem.ITEM_ID);
                                    byte Quality = (byte)(DBItem.ID % 10);

                                    if (Chance == 0 || !client.Inventory.Contain(DataItem.ITEM_ID, 1) || DataItem == null)
                                    {
                                        client.SendSysMesage("You can't upgrade your " + DBItem.Name + "", MsgMessage.ChatMode.System);
                                        return;
                                    }

                                    if (client.Inventory.Contain(Database.ItemType.DragonBall, 1, 0))
                                    {
                                        if (Role.Core.PercentSuccess(Chance))
                                        {
                                            dwParam = 1;
                                            uint oldid = DataItem.ITEM_ID;
                                            if (DataItem.ITEM_ID % 10 < 5)
                                                DataItem.ITEM_ID += 5 - DataItem.ITEM_ID % 10;
                                            DataItem.ITEM_ID = (uint)(DataItem.ITEM_ID + 1);
                                            if (DataItem.SocketOne == Role.Flags.Gem.NoSocket)
                                            {
                                                if (Role.Core.PercentSuccess((0.00 + (client.Player.BlessTime > 0 ? Global.LUCKY_TIME_BONUS_SOCKET_RATE : 1))/* * 1*/))
                                                {
                                                    DataItem.SocketOne = Role.Flags.Gem.EmptySocket;
                                                    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Player, " + client.Player.Name + " has the first socket to his/her " + DBItem.Name + "", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                                                }
                                            }

                                            if (DataItem.SocketOne != Role.Flags.Gem.NoSocket && DataItem.SocketTwo != Role.Flags.Gem.NoSocket)
                                            {
                                                if (Role.Core.PercentSuccess((0.00 + (client.Player.BlessTime > 0 ? Global.LUCKY_TIME_BONUS_SOCKET_RATE : 1))/* * 1*/))
                                                {
                                                    DataItem.SocketTwo = Role.Flags.Gem.EmptySocket;
                                                    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Player, " + client.Player.Name + " has the second socket to his/her " + DBItem.Name + "", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                                                }
                                            }
                                            DataItem.Durability = DataItem.MaximDurability;
                                            DataItem.Mode = Role.Flags.ItemMode.Update;
                                            if (oldid != DataItem.ITEM_ID)
                                            {
                                                DataItem.Send(client, stream);//.Update(itemuse, Role.Instance.AddMode.REMOVE,stream);
                                                client.SendSysMesage("Successfully upgraded the quality" + DBItem.Name + "", MsgMessage.ChatMode.TopLeftSystem);

                                            }
                                            else
                                            {
                                                client.SendSysMesage("This item's level can't be upgraded anymore.", MsgMessage.ChatMode.TopLeftSystem);
                                            }
                                        }
                                        else
                                        {
                                            int RandomDura = Role.Core.Random.Next(1, DataItem.Durability);
                                            DataItem.Durability = (ushort)RandomDura;
                                            DataItem.Mode = Role.Flags.ItemMode.Update;
                                            DataItem.Send(client, stream);
                                            client.SendSysMesage("Failed to upgrade the quality" + DBItem.Name + ".", MsgMessage.ChatMode.TopLeftSystem);
                                        }
                                        client.Inventory.Remove(Database.ItemType.DragonBall, 1, stream);

                                    }
                                }
                            }
                            else if (itemuse.ITEM_ID == Database.ItemType.Meteor)
                            {
                                Database.ItemType.DBItem DBItem;
                                if (Database.Server.ItemsBase.TryGetValue(DataItem.ITEM_ID, out DBItem))
                                {
                                    //if (DataItem.ITEM_ID % 10 == 9 || DataItem.ITEM_ID % 10 < 3)
                                    //{
                                    //    client.SendSysMesage("This item's cant be upgraded anymore.");
                                    //    return;
                                    //}
                                    //  var Chance = Database.ItemType.ChanceToUpgradeLevel(DataItem.ITEM_ID);
                                    byte Chance = 70;
                                    Chance -= (byte)(DBItem.Level / 10 * 3);
                                    Chance -= (byte)(((DataItem.ITEM_ID % 10) + 1) * 3);
                                    if (Chance == 0 || !client.Inventory.Contain(DataItem.ITEM_ID, 1) || DataItem == null)
                                    {
                                        client.SendSysMesage("The " + DBItem.Name + " can't improve anymore try to visit UpgradeMaster in Market (178,199).", MsgMessage.ChatMode.System);
                                        return;
                                    }

                                    bool succesed = false;

                                    uint nextItemId = Database.Server.ItemsBase.UpdateItem(DataItem.ITEM_ID, out succesed);

                                    if ((DBItem.Level >= 70 && Database.ItemType.Equipable(nextItemId, client) == false)
                                     && (Database.ItemType.ItemPosition(DataItem.ITEM_ID) == (ushort)Role.Flags.ConquerItem.RightWeapon
                                     || Database.ItemType.ItemPosition(DataItem.ITEM_ID) == (ushort)Role.Flags.ConquerItem.LeftWeapon))
                                    {
                                        client.SendSysMesage("You can`t upgrade this item.");
                                    }
                                    else if (DBItem.Level >= 115)
                                    {
                                        client.SendSysMesage("The " + DBItem.Name + " can't improve anymore try to visit UpgradeMaster in Market (178,199).", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        if (client.Inventory.Contain(Database.ItemType.Meteor, 1, 0))
                                        {
                                            if (Role.Core.PercentSuccess(Chance))
                                            {
                                                //dwParam = 1;
                                                DataItem.ITEM_ID = Database.Server.ItemsBase.UpdateItem(DataItem.ITEM_ID, out succesed);
                                                if (DataItem.SocketOne == Role.Flags.Gem.NoSocket)
                                                {
                                                    if (Role.Core.PercentSuccess(0.10 + (client.Player.BlessTime > 0 ? Global.LUCKY_TIME_BONUS_SOCKET_RATE : 1)))
                                                    {
                                                        DataItem.SocketOne = Role.Flags.Gem.EmptySocket;
                                                        Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Player, " + client.Player.Name + " has the first socket to his/her " + DBItem.Name + "", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                                                    }
                                                }
                                                if (DataItem.SocketOne != Role.Flags.Gem.NoSocket && DataItem.SocketTwo != Role.Flags.Gem.NoSocket)
                                                {
                                                    if (Role.Core.PercentSuccess(0.0035 + (client.Player.BlessTime > 0 ? Global.LUCKY_TIME_BONUS_SOCKET_RATE : 1)))
                                                    {
                                                        DataItem.SocketTwo = Role.Flags.Gem.EmptySocket;
                                                        Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Player, " + client.Player.Name + " has the second socket to his/her " + DBItem.Name + "", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                                                    }
                                                }
                                                DataItem.Durability = DataItem.MaximDurability;
                                                DataItem.Mode = Role.Flags.ItemMode.Update;
                                                DataItem.Send(client, stream);
                                                client.SendSysMesage("Successfully upgraded the level" + DBItem.Name + "", MsgMessage.ChatMode.TopLeftSystem);
                                            }
                                            else
                                            {
                                                //worked = false;
                                                int RandomDura = Role.Core.Random.Next(0, DataItem.Durability);
                                                DataItem.Durability = (ushort)RandomDura;
                                                DataItem.Mode = Role.Flags.ItemMode.Update;
                                                DataItem.Send(client, stream);
                                                client.SendSysMesage("Failed to upgrade the level" + DBItem.Name + "", MsgMessage.ChatMode.TopLeftSystem);

                                            }
                                        }
                                        client.Inventory.Remove(Database.ItemType.Meteor, 1, stream);
                                    }
                                }
                                if (DataItem.Position != 0)
                                    client.Equipment.QueryEquipment();
                                //if (worked)
                                //    client.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsuageID.UpgradeMeteor, ItemUID, dwParam, 0, 0, 0, 0));
                            }
                        }
                        break;
                    }
                case ItemUsuageID.ReturnedItems:
                    {
                        if (!client.Inventory.HaveSpace(1))
                        {
                            client.CreateBoxDialog("Please make 1 more space in your inventory.");
                            break;
                        }
                        if (client.Warehouse.RemoveItem(id, ushort.MaxValue, stream))
                        {

                            client.Send(stream.ItemUsageCreate(ItemUsuageID.ReturnedItems, id, 0, timestamp, 1, 1, 1));
                        }

                        break;
                    }
                case ItemUsuageID.DegradeEquipment:
                    {
                        MsgGameItem GameItem;
                        if (client.Inventory.TryGetItem((uint)id, out GameItem))
                        {
                            if (GameItem.IsEquip)
                            {
                                if (client.Player.ConquerPoints >= 54)
                                {
                                    client.Player.ConquerPoints -= 54;
                                    GameItem.ITEM_ID = Database.Server.ItemsBase.DowngradeItem(GameItem.ITEM_ID);
                                    GameItem.Mode = Role.Flags.ItemMode.Update;
                                    GameItem.Send(client, stream);
                                }
                                else
                                {
                                    client.SendSysMesage("You don't have 54 CPs.");
                                }
                            }
                        }
                        break;
                    }

                case ItemUsuageID.ActiveItems:
                    {
                        MsgGameItem GameItem;
                        if (client.Inventory.TryGetItem((uint)id, out GameItem))
                        {/*
                            switch (GameItem.ITEM_ID)
                            {
                                case 3005061:// BrightActivePack
                                    {
                                        if (client.Inventory.HaveSpace(4))
                                        {
                                            client.Inventory.Update(GameItem, Role.Instance.AddMode.REMOVE, stream);

                                            client.Inventory.Add(stream, 729022, 1);
                                            client.Inventory.Add(stream, Database.ItemType.DragonBallScroll, 1);
                                            client.Inventory.AddItemWitchStack(Database.ItemType.ExpBall, 0, 5, stream);
                                            client.Inventory.AddItemWitchStack(Database.ItemType.DoubleExp, 0, 5, stream);
                                            client.Player.ConquerPoints += 7000;

                                        }
                                        else
                                        {
                                            client.CreateBoxDialog("Please make 4 more spaces in your inventory.");
                                        }
                                        break;
                                    }
                                case 3005062:// FortuneActivePack 
                                    {
                                        if (client.Inventory.HaveSpace(6))
                                        {
                                            client.Inventory.Update(GameItem, Role.Instance.AddMode.REMOVE, stream);

                                            client.Inventory.Add(stream, 729022, 2);
                                            client.Inventory.Add(stream, Database.ItemType.DragonBallScroll, 2);
                                            client.Inventory.AddItemWitchStack(Database.ItemType.ExpBall, 0, 5, stream);
                                            client.Inventory.AddItemWitchStack(Database.ItemType.DoubleExp, 0, 5, stream);
                                            client.Player.ConquerPoints += 10000;

                                        }
                                        else
                                        {
                                            client.CreateBoxDialog("Please make 6 more spaces in your inventory.");
                                        }
                                        break;
                                    }
                                case 3005064:// SolarActivePack
                                    {
                                        if (client.Inventory.HaveSpace(11))
                                        {
                                            client.Inventory.Update(GameItem, Role.Instance.AddMode.REMOVE, stream);

                                            client.Inventory.Add(stream, 729023, 4);
                                            client.Inventory.Add(stream, Database.ItemType.DragonBallScroll, 2);
                                            client.Inventory.AddItemWitchStack(Database.ItemType.ExpBall, 0, 5, stream);
                                            client.Inventory.AddItemWitchStack(Database.ItemType.DoubleExp, 0, 5, stream);
                                            //client.Inventory.Add(stream, Database.ItemType.PowerExpBall, 2);
                                            client.Player.ConquerPoints += 20000;

                                        }
                                        else
                                        {
#if Arabic
                                                client.CreateBoxDialog("Please make 11 more spaces in your inventory.");
                                        
#else
                                            client.CreateBoxDialog("Please make 11 more spaces in your inventory.");

#endif
                                        }
                                        break;
                                    }
                                case 3005065:// DivineActivePack
                                    {
                                        if (client.Inventory.HaveSpace(14))
                                        {
                                            client.Inventory.Update(GameItem, Role.Instance.AddMode.REMOVE, stream);

                                            client.Inventory.Add(stream, 729023, 4);
                                            client.Inventory.Add(stream, Database.ItemType.DragonBallScroll, 2);
                                            client.Inventory.AddItemWitchStack(Database.ItemType.ExpBall, 0, 5, stream);
                                            client.Inventory.AddItemWitchStack(Database.ItemType.DoubleExp, 0, 5, stream);
                                            //  client.Inventory.Add(stream,Database.ItemType.PowerExpBall, 4);
                                            client.Player.ConquerPoints += 30000;

                                        }
                                        else
                                        {
#if Arabic
                                                 client.CreateBoxDialog("Please make 14 more spaces in your inventory.");
#else
                                            client.CreateBoxDialog("Please make 14 more spaces in your inventory.");
#endif

                                        }
                                        break;
                                    }
                            }
                            */
                        }
                        break;
                    }
                case ItemUsuageID.SplitStack:
                    {
                        MsgGameItem MainItem = null;
                        MsgGameItem MinorItem = null;
                        if (client.Inventory.TryGetItem((uint)id, out MainItem))
                        {
                            Database.ItemType.DBItem DBItem = null;
                            if (Database.Server.ItemsBase.TryGetValue(MainItem.ITEM_ID, out DBItem))
                            {
                                if (MainItem.StackSize > 1 && MainItem.StackSize <= DBItem.StackSize)
                                {
                                    if (!client.Inventory.HaveSpace(1))
                                    {
#if Arabic
                                          client.SendSysMesage("Please make 1 more space in your inventory.");
#else
                                        client.SendSysMesage("Please make 1 more space in your inventory.");
#endif

                                        break;
                                    }

                                    ushort Amount = (ushort)dwParam;
                                    if (Amount <= DBItem.StackSize && Amount > 0 && MainItem.StackSize > Amount)
                                    {
                                        MainItem.StackSize -= Amount;
                                        MainItem.Mode = Role.Flags.ItemMode.Update;
                                        MainItem.Send(client, stream);

                                        MinorItem = new MsgGameItem();

                                        MinorItem.ITEM_ID = MainItem.ITEM_ID;
                                        MinorItem.StackSize += Amount;
                                        MinorItem.Durability = MainItem.Durability;
                                        MinorItem.MaximDurability = MainItem.MaximDurability;
                                        MinorItem.Plus = MainItem.Plus;

                                        client.Inventory.Update(MinorItem, Role.Instance.AddMode.ADD, stream);
                                    }
                                    else
                                    {
                                        Console.WriteLine("Cheater -> " + client.Player.Name);
                                        client.Socket.Disconnect();
                                    }
                                }
                            }
                        }
                        break;
                    }
                case ItemUsuageID.MergeStackableItems:
                    {
                        if (client.Player.OnMyOwnServer == false)
                            break;
                        MsgGameItem MainItem = null;
                        MsgGameItem MinorItem = null;
                        if (client.Inventory.TryGetItem((uint)id, out MainItem) && client.Inventory.TryGetItem((uint)dwParam, out MinorItem))
                        {
                            if (MainItem.UID != MinorItem.UID)
                            {
                                if (MainItem.ITEM_ID == MinorItem.ITEM_ID)
                                {
                                    Database.ItemType.DBItem DBItem = null;
                                    if (Database.Server.ItemsBase.TryGetValue(MainItem.ITEM_ID, out DBItem))
                                    {
                                        if (MainItem.StackSize < 1)
                                            MainItem.StackSize = 1;
                                        if (MinorItem.StackSize < 1)
                                            MinorItem.StackSize = 1;

                                        if ((MainItem.StackSize + MinorItem.StackSize) <= DBItem.StackSize)
                                        {
                                            MainItem.StackSize += MinorItem.StackSize;
                                            MainItem.Mode = Role.Flags.ItemMode.Update;
                                            MainItem.Send(client, stream).Update(MinorItem, Role.Instance.AddMode.REMOVE, stream, true);
                                        }
                                        /*  else
                                           {
                                               ushort removestak = (ushort)(DBItem.StackSize - MainItem.StackSize);
                                               if (MinorItem.StackSize > removestak)
                                               {
                                                   MainItem.StackSize += removestak;
                                                   MainItem.Mode = Role.Flags.ItemMode.Update;
                                                   MainItem.Send(client, stream);

                                                   MinorItem.StackSize -= removestak;
                                                   MainItem.Mode = Role.Flags.ItemMode.Update;
                                                   MainItem.Send(client, stream);
                                                   if (MinorItem.StackSize == 0)
                                                       client.Inventory.Update(MainItem, Role.Instance.AddMode.REMOVE, stream);
                                               }
                                           }*/
                                    }
                                }
                            }
                        }
                        break;
                    }
                case ItemUsuageID.RedeemGear:
                    {
                        Game.MsgServer.MsgDetainedItem DetainedItem;
                        if (client.Confiscator.RedeemContainer.TryGetValue((uint)id, out DetainedItem))
                        {
                            DetainedItem.DaysLeft = (uint)(TimeSpan.FromTicks(DateTime.Now.Ticks).Days - TimeSpan.FromTicks(Role.Instance.Confiscator.GetTimer(DetainedItem.Date).Ticks).Days);
                            if (DetainedItem.DaysLeft > 7)
                            {
#if Arabic
                                  client.SendSysMesage("This item is expired!");
#else
                                client.SendSysMesage("This item is expired!");
#endif

                                break;
                            }
                            if (!client.Inventory.HaveSpace(1))
                            {
#if Arabic
                                                                client.SendSysMesage("Please make 1 space in your container!");
#else
                                client.SendSysMesage("Please make 1 more space in your container!");
#endif

                                break;
                            }
                            if (client.Player.ConquerPoints >= Role.Instance.Confiscator.CalculateCpsCost(DetainedItem))
                            {
                                client.Player.ConquerPoints -= (uint)Role.Instance.Confiscator.CalculateCpsCost(DetainedItem);


                                dwParam = client.Player.UID;
                                dwparam4 = (uint)Role.Instance.Confiscator.CalculateCpsCost(DetainedItem);

                                client.Send(stream.ItemUsageCreate(ItemUsuageID.RedeemGear, id, dwParam, timestamp, dwParam2, dwParam3, dwparam4));


                                client.Inventory.Update(MsgDetainedItem.CopyTo(DetainedItem), Role.Instance.AddMode.ADD, stream);

#if Arabic
  string Messajj = "" + client.Player.Name + " redeemed his equipment (" + Database.Server.ItemsBase.GetItemName(DetainedItem.ItemID) + "), he paying " + Role.Instance.Confiscator.CalculateCpsCost(DetainedItem) + " conquer points for " + DetainedItem.GainerName + ".";

#else
                                string Messajj = "" + client.Player.Name + " redeemed his equipment (" + Database.Server.ItemsBase.GetItemName(DetainedItem.ItemID) + "), he/she is paying " + Role.Instance.Confiscator.CalculateCpsCost(DetainedItem) + " CPs for " + DetainedItem.GainerName + ".";

#endif

                                //string Messaj = "" + client.Player.Name + " redeemed his equipment, hereby obtained a ransom of " + DetainedItem.ConquerPointsCost + " points, all the days Stone Award to seize its equipment to help players " + DetainedItem.GainerName + "";
                                Program.SendGlobalPackets.Enqueue(new MsgMessage(Messajj, MsgMessage.MsgColor.white, MsgMessage.ChatMode.System).GetArray(stream));

                                if (client.Confiscator.RedeemContainer.TryRemove(DetainedItem.UID, out DetainedItem))
                                {
                                    Role.Instance.Confiscator GainerCointainer;
                                    if (Database.Server.QueueContainer.PollContainers.TryGetValue(DetainedItem.GainerUID, out GainerCointainer))
                                    {
                                        if (GainerCointainer.ClaimContainer.ContainsKey(DetainedItem.UID))
                                        {
                                            DetainedItem.Action = MsgDetainedItem.ContainerType.RewardCps;
                                            DetainedItem.RewardConquerPoints = Role.Instance.Confiscator.CalculateCpsCost(DetainedItem);
                                            GainerCointainer.ClaimContainer[DetainedItem.UID] = DetainedItem;

                                            Client.GameClient Gainer;
                                            if (Database.Server.GamePoll.TryGetValue(DetainedItem.GainerUID, out Gainer))
                                            {
                                                dwParam = Gainer.Player.UID;
                                                action = ItemUsuageID.ClaimGear;

                                                Gainer.Send(stream.ItemUsageCreate(action, id, dwParam, timestamp, dwParam2, dwParam3, dwparam4));



                                                GainerCointainer.ClaimContainer[DetainedItem.UID].Send(Gainer, stream);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    }
                case ItemUsuageID.ClaimGear:
                    {
                        if (!client.Inventory.HaveSpace(1))
                        {
#if Arabic
                              client.SendSysMesage("Please make 1 space in your container!");
#else
                            client.SendSysMesage("Please make 1 more space in your container!");
#endif

                            break;
                        }

                        Game.MsgServer.MsgDetainedItem ClaimItem;
                        if (client.Confiscator.ClaimContainer.TryGetValue(id, out ClaimItem))
                        {
                            if (ClaimItem.Bound && ClaimItem.DaysLeft > 7)
                            {

                                dwParam = client.Player.UID;

                                dwparam4 = (uint)ClaimItem.RewardConquerPoints;


                                client.Send(stream.ItemUsageCreate(action, id, dwParam, timestamp, dwParam2, dwParam3, dwparam4));

                                client.Confiscator.ClaimContainer.TryRemove(ClaimItem.UID, out ClaimItem);
#if Arabic
                                 client.SendSysMesage("Unnclaimable Bound item!");
#else
                                client.SendSysMesage("Unclaimable Bound item!");
#endif

                                break;
                            }
                            ClaimItem.DaysLeft = (uint)(TimeSpan.FromTicks(DateTime.Now.Ticks).Days - TimeSpan.FromTicks(Role.Instance.Confiscator.GetTimer(ClaimItem.Date).Ticks).Days);
                            if (ClaimItem.DaysLeft < 7 && ClaimItem.Action != MsgDetainedItem.ContainerType.RewardCps)
                            {
#if Arabic
                                 client.SendSysMesage("This item is not expired. You cannot claim it yet!");
#else
                                client.SendSysMesage("This item isn't expired. You can't claim it yet!");
#endif

                                break;
                            }
                            if (ClaimItem.RewardConquerPoints != 0)
                            {
                                client.Player.ConquerPoints += (uint)ClaimItem.RewardConquerPoints;

                                client.Confiscator.ClaimContainer.TryRemove(ClaimItem.UID, out ClaimItem);
                            }
                            else if (ClaimItem.DaysLeft > 7)
                            {
                                client.Inventory.Update(MsgDetainedItem.CopyTo(ClaimItem), Role.Instance.AddMode.ADD, stream);
                                client.Confiscator.ClaimContainer.TryRemove(ClaimItem.UID, out ClaimItem);
                            }
#if Arabic
                              string Messaj = "Congratulation! " + client.Player.Name + " has received " + ClaimItem.RewardConquerPoints + " conquer points for capture the red/black player called " + ClaimItem.OwnerName + "";//"Thank you for arresting red/black name players " + client.Entity.Name + " has recived " + item.ConquerPointsCost + " CPS . Congratulations!";
                           
#else
                            string Messaj = "Congratulations! " + client.Player.Name + " has received " + ClaimItem.RewardConquerPoints + " CPs for capturing the red/black player called " + ClaimItem.OwnerName + "";//"Thank you for arresting red/black name players " + client.Entity.Name + " has recived " + item.ConquerPointsCost + " CPS . Congratulations!";

#endif
                            Program.SendGlobalPackets.Enqueue(new MsgMessage(Messaj, MsgMessage.MsgColor.white, MsgMessage.ChatMode.System).GetArray(stream));

                            dwParam = client.Player.UID;
                            dwparam4 = (uint)ClaimItem.RewardConquerPoints;
                            client.Send(stream.ItemUsageCreate(action, id, dwParam, timestamp, dwParam2, dwParam3, dwparam4));

                        }
                        break;
                    }
                case ItemUsuageID.GarmentShop:
                    {
                        uint GarmentID = (uint)dwParam;
                        bool RightInfo = true;
                        if (Database.ItemType.ItemPosition(GarmentID) == (ushort)Role.Flags.ConquerItem.Garment)
                        {
                            Database.ItemType.DBItem DBItem = Database.Server.ItemsBase[GarmentID];
                            ushort Points = 0;

                            Queue<Game.MsgServer.MsgGameItem> RemoveItem = new Queue<MsgGameItem>((int)dwParam2);
                            foreach (var itemUID in args)
                            {
                                Game.MsgServer.MsgGameItem item;
                                if (client.Inventory.TryGetItem(itemUID, out item))
                                {
                                    switch (item.ITEM_ID)
                                    {
                                        case 188435://188435
                                        case 181355:
                                        case 182435: Points += 150; break;

                                        case 191305:
                                        case 191405:
                                        case 183305:
                                        case 183375:
                                        case 183315:
                                        case 183325: Points += 300; break;
                                        default:
                                            Points += 50;
                                            break;
                                    }
                                    RemoveItem.Enqueue(item);
                                }
                                else
                                {
                                    RightInfo = false;
                                    break;
                                }
                            }
                            if (RightInfo)
                            {
                                uint price = DBItem.ConquerPointsWorth;
                                if (Points >= price)
                                    client.Inventory.Add(stream, GarmentID, 1);
                                else
                                {
                                    if (client.Player.ConquerPoints >= price - Points)
                                    {
                                        while (RemoveItem.Count > 0)
                                            client.Inventory.Update(RemoveItem.Dequeue(), Role.Instance.AddMode.REMOVE, stream);

                                        client.Player.ConquerPoints -= (uint)(price - Points);

                                        client.Inventory.Add(stream, GarmentID, 1);

                                    }
                                }
                            }
                        }
                        break;
                    }
                /*   case ItemUsuageID.AddBless:
                       {
                           MsgGameItem DataItem;
                           if (client.TryGetItem(id, out DataItem))
                           {
                               ushort Position = Database.ItemType.ItemPosition(DataItem.ITEM_ID);
                               if (Database.ItemType.AllowToUpdate((Role.Flags.ConquerItem)Position))
                               {
                                   if (Position == (ushort)Role.Flags.ConquerItem.RidingCrop && DataItem.Bless >= 1)
                                   {
                                       if (Position == (ushort)Role.Flags.ConquerItem.RidingCrop && DataItem.Bless > 1)
                                       {
                                           DataItem.Bless = 1;
                                           DataItem.Mode = Role.Flags.ItemMode.Update;
                                           DataItem.Send(client, stream);
                                       }
                                       return;
                                   }

                                   Queue<MsgGameItem> LoseItems;
                                   if (client.Inventory.VerifiedUpdateItem(args, Database.ItemType.GetGemID(Role.Flags.Gem.SuperTortoiseGem), (byte)dwParam2, out LoseItems))
                                   {
                                       byte OldBless = DataItem.Bless;
                                       if (DataItem.Bless == 0 && LoseItems.Count == 5)
                                           DataItem.Bless = 1;
                                       else if (DataItem.Bless == 1 && LoseItems.Count == 1)
                                           DataItem.Bless = 3;
                                       else if (DataItem.Bless == 3 && LoseItems.Count == 3)
                                           DataItem.Bless = 5;
                                       else if (DataItem.Bless == 5 && LoseItems.Count == 5)
                                           DataItem.Bless = 7;
                                       if (OldBless < DataItem.Bless)
                                       {
                                           DataItem.Mode = Role.Flags.ItemMode.Update;
                                           DataItem.Send(client, stream);
                                           if (DataItem.Position != 0)
                                               client.Equipment.QueryEquipment(client.Equipment.Alternante);
                                           while (LoseItems.Count > 0)
                                               client.Inventory.Update(LoseItems.Dequeue(), Role.Instance.AddMode.REMOVE, stream);


                                           client.Send(stream.ItemUsageCreate(ItemUsuageID.AddBless, 0, 1, 0, 0, 0, 0));
                                       }
                                   }
                               }
                           }
                           break;
                       }*/
                case ItemUsuageID.ToristSuper:
                    {
                        if (client.Player.Money >= 1000000)
                        {
                            client.Player.Money -= 1000000;
                            client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);

                            bool HaveAllGems = false;
                            for (uint x = 0; x < 7; x++)
                            {
                                uint ItemID = 700002 + x * 10;
                                if (client.Inventory.Contain(ItemID, 1))
                                    HaveAllGems = true;
                                else
                                {
                                    HaveAllGems = false;
                                    break;
                                }
                            }
                            if (HaveAllGems)
                            {
                                for (uint x = 0; x < 7; x++)
                                {
                                    uint ItemID = 700002 + x * 10;
                                    client.Inventory.Remove(ItemID, 1, stream);
                                }
                                client.Inventory.Add(stream, 700072, 1);

                                client.Send(stream.ItemUsageCreate(ItemUsuageID.ToristSuper, 0, 1, 0, 0, 0, 0));
                            }
                        }
                        else
                        {
#if Arabic
                             client.SendSysMesage("Sorry you don`t have 100,000 silver!.");
#else
                            client.SendSysMesage("Sorry, you don`t have 100,000 gold!");
#endif

                        }
                        break;
                    }
                case ItemUsuageID.GemCompose:
                    {
                        if (id / 10000 == 70 && Enum.IsDefined(typeof(Role.Flags.Gem), (byte)(id % 1000)))
                        {
                            uint price = 0;
                            if (id % 10 == 1)
                                price = 100000;
                            else if (id % 10 == 2)
                            {
                                price = 800000;
                                if (id % 100 == 72)
                                    price = 10000000;
                            }
                            if (client.Player.Money >= price)
                            {
                                if (client.Inventory.Contain(id, 15))
                                {
                                    if (client.Inventory.Remove(id, 15, stream))
                                    {
                                        client.Inventory.Add(stream, (uint)(id + 1), 1);

                                        dwParam = 1;
                                        client.Send(stream.ItemUsageCreate(action, id, dwParam, timestamp, dwParam2, dwParam3, dwparam4, args));

                                        client.Player.Money -= price;
                                        client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);
                                    }
                                }
                            }
                            else
                                client.SendSysMesage("Sorry you don`t have 1,000,000 silver!.");
                        }
                        else if (dwParam == 7 && dwParam2 == 500000)
                            client.Inventory.Add(stream, id, 1, (byte)dwParam3, 0, 0);
                        break;
                    }
                case ItemUsuageID.UpdateEnchant:
                    {
                        MsgGameItem DataItem;
                        if (client.TryGetItem(id, out DataItem))
                        {
                            ushort Position = Database.ItemType.ItemPosition(DataItem.ITEM_ID);
                            if (Database.ItemType.AllowToUpdate((Role.Flags.ConquerItem)Position))
                            {
                                MsgGameItem Gem;
                                if (client.Inventory.ClientItems.TryGetValue((uint)dwParam, out Gem))
                                {
                                    if (Enum.IsDefined(typeof(Role.Flags.Gem), (byte)(Gem.ITEM_ID % 1000)))
                                    {
                                        byte Enchant = 0;
                                        switch (Gem.ITEM_ID % 10)
                                        {
                                            case 1:
                                                {
                                                    Enchant = (byte)Program.GetRandom.Next(1, 59);
                                                    break;
                                                }
                                            case 2:
                                                {
                                                    if (Gem.ITEM_ID == 700012)
                                                        Enchant = (byte)Program.GetRandom.Next(100, 159);
                                                    else if (Gem.ITEM_ID == 700002 || Gem.ITEM_ID == 700052 || Gem.ITEM_ID == 700062)
                                                        Enchant = (byte)Program.GetRandom.Next(60, 109);
                                                    else if (Gem.ITEM_ID == 700032)
                                                        Enchant = (byte)Program.GetRandom.Next(80, 129);
                                                    else
                                                        Enchant = (byte)Program.GetRandom.Next(40, 89);
                                                    break;
                                                }
                                            default:
                                                {
                                                    if (Gem.ITEM_ID == 700013)
                                                        Enchant = (byte)Program.GetRandom.Next(200, 256);
                                                    else if (Gem.ITEM_ID == 700003 || Gem.ITEM_ID == 700073 || Gem.ITEM_ID == 700033)
                                                        Enchant = (byte)Program.GetRandom.Next(170, 229);
                                                    else if (Gem.ITEM_ID == 700063 || Gem.ITEM_ID == 700053)
                                                        Enchant = (byte)Program.GetRandom.Next(140, 199);
                                                    else if (Gem.ITEM_ID == 700023)
                                                        Enchant = (byte)Program.GetRandom.Next(90, 149);
                                                    else
                                                        Enchant = (byte)Program.GetRandom.Next(70, 119);
                                                    break;
                                                }
                                        }
                                        if (Enchant > DataItem.Enchant)
                                        {
                                            DataItem.Enchant = Enchant;
                                            DataItem.Mode = Role.Flags.ItemMode.Update;
                                            DataItem.Send(client, stream).Update(Gem, Role.Instance.AddMode.REMOVE, stream);
                                            if (DataItem.Position != 0)
                                                client.Equipment.QueryEquipment();
                                        }
                                        else
                                        {
                                            client.Inventory.Update(Gem, Role.Instance.AddMode.REMOVE, stream);
                                        }

                                        dwParam = Enchant;
                                        client.Send(stream.ItemUsageCreate(action, id, dwParam, timestamp, dwParam2, dwParam3, dwparam4));
                                    }
                                }
                            }
                        }

                        break;
                    }
                case ItemUsuageID.BuyItemFromForging:
                    {
                        uint ItemID = (uint)dwParam;
                        uint ItemsCount = dwParam2;
                        if (ItemID == 1088001 || ItemID == 1088000 || ItemID == 730001 || ItemID == 730003 || ItemID == 730006
                                            || ItemID == 723694 || ItemID == 723695 || ItemID == 700073 || ItemID == 1200005)
                        {
                            if (client.Inventory.HaveSpace((byte)ItemsCount))
                            {
                                Database.ItemType.DBItem DBItem = null;
                                if (Database.Server.ItemsBase.TryGetValue(ItemID, out DBItem))
                                {
                                    for (int x = 0; x < ItemsCount; x++)
                                    {
                                        if (client.Player.ConquerPoints >= DBItem.ConquerPointsWorth)
                                        {
                                            client.Player.ConquerPoints -= DBItem.ConquerPointsWorth;

                                            if ((ItemID % 730000) <= 9)
                                                client.Inventory.Add(DBItem.ID, (byte)(ItemID % 730000), DBItem, stream);
                                            else
                                                client.Inventory.Add(DBItem.ID, 0, DBItem, stream);
                                        }
                                        else break;
                                    }
                                }

                            }
                        }
                        break;
                    }
                case ItemUsuageID.CreateSocketItem:
                    {
                        uint effectuid = 0;
                        uint effectdwparam1 = 0;

                        switch (dwParam2)
                        {

                            case 1:
                                {
                                    MsgGameItem DataItem;
                                    if (client.TryGetItem(id, out DataItem))
                                    {
                                        effectuid = DataItem.UID;

                                        ushort Position = Database.ItemType.ItemPosition(DataItem.ITEM_ID);
                                        if (Database.ItemType.AllowToUpdate((Role.Flags.ConquerItem)Position))
                                        {
                                            if (DataItem.SocketOne == Role.Flags.Gem.NoSocket)
                                            {
                                                MsgGameItem LoseItem;
                                                if (client.Inventory.ClientItems.TryGetValue(args[0], out LoseItem))
                                                {
                                                    if (LoseItem.ITEM_ID == Database.ItemType.DragonBall)
                                                    {
                                                        DataItem.SocketOne = Role.Flags.Gem.EmptySocket;
                                                        DataItem.Mode = Role.Flags.ItemMode.Update;

                                                        if (DataItem.Position != 0)
                                                            client.Equipment.QueryEquipment();

                                                        effectdwparam1 = 1;



                                                        DataItem.Send(client, stream).Update(LoseItem, Role.Instance.AddMode.REMOVE, stream);
                                                    }
                                                }
                                            }
                                            else if (DataItem.SocketTwo == Role.Flags.Gem.NoSocket)
                                            {
                                                MsgGameItem LoseItem;
                                                if (client.Inventory.ClientItems.TryGetValue(args[0], out LoseItem))
                                                {
                                                    if (LoseItem.ITEM_ID == Database.ItemType.ToughDrill)
                                                    {
                                                        if (Role.Core.Rate(20))
                                                        {
                                                            effectdwparam1 = 1;

                                                            DataItem.SocketTwo = Role.Flags.Gem.EmptySocket;
                                                            DataItem.Mode = Role.Flags.ItemMode.Update;
                                                            DataItem.Send(client, stream).Update(LoseItem, Role.Instance.AddMode.REMOVE, stream);
                                                            if (DataItem.Position != 0)
                                                                client.Equipment.QueryEquipment();
                                                        }
                                                        else
                                                        {
                                                            client.Inventory.Update(LoseItem, Role.Instance.AddMode.REMOVE, stream);
                                                            client.Inventory.Add(stream, Database.ItemType.StarDrill, 1);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
#if Arabic
                                              client.SendSysMesage("Sorry can't make socket in this item !");
#else
                                            client.SendSysMesage("Sorry, you can't make a socket in this item!");
#endif

                                        }
                                    }
                                    break;
                                }
                            case 5:
                                {
                                    MsgGameItem DataItem;
                                    if (client.TryGetItem(id, out DataItem))
                                    {
                                        effectuid = DataItem.UID;

                                        ushort Position = Database.ItemType.ItemPosition(DataItem.ITEM_ID);
                                        if (Database.ItemType.AllowToUpdate((Role.Flags.ConquerItem)Position))
                                        {
                                            if (DataItem.SocketTwo == Role.Flags.Gem.NoSocket)
                                            {
                                                Queue<MsgGameItem> LoseItems;
                                                if (client.Inventory.VerifiedUpdateItem(args, Database.ItemType.DragonBall, (byte)dwParam2, out LoseItems))
                                                {
                                                    effectdwparam1 = 1;

                                                    DataItem.SocketTwo = Role.Flags.Gem.EmptySocket;
                                                    DataItem.Mode = Role.Flags.ItemMode.Update;
                                                    DataItem.Send(client, stream);
                                                    if (DataItem.Position != 0)
                                                        client.Equipment.QueryEquipment();

                                                    while (LoseItems.Count > 0)
                                                        client.Inventory.Update(LoseItems.Dequeue(), Role.Instance.AddMode.REMOVE, stream);
                                                }
                                            }
                                        }
                                        else
                                        {
#if Arabic
                                             client.SendSysMesage("Sorry can't make socket in this item !");
#else
                                            client.SendSysMesage("Sorry, you can't make a socket in this item!");
#endif

                                        }
                                    }
                                    break;
                                }
                            case 7:
                                {
                                    MsgGameItem DataItem;
                                    if (client.TryGetItem(id, out DataItem))
                                    {
                                        effectuid = DataItem.UID;

                                        ushort Position = Database.ItemType.ItemPosition(DataItem.ITEM_ID);
                                        if (Database.ItemType.AllowToUpdate((Role.Flags.ConquerItem)Position))
                                        {
                                            if (DataItem.SocketTwo == Role.Flags.Gem.NoSocket)
                                            {
                                                Queue<MsgGameItem> LoseItems;
                                                if (client.Inventory.VerifiedUpdateItem(args, Database.ItemType.StarDrill, (byte)dwParam2, out LoseItems))
                                                {
                                                    effectdwparam1 = 1;

                                                    DataItem.SocketTwo = Role.Flags.Gem.EmptySocket;
                                                    DataItem.Mode = Role.Flags.ItemMode.Update;
                                                    DataItem.Send(client, stream);
                                                    if (DataItem.Position != 0)
                                                        client.Equipment.QueryEquipment();

                                                    while (LoseItems.Count > 0)
                                                        client.Inventory.Update(LoseItems.Dequeue(), Role.Instance.AddMode.REMOVE, stream);
                                                }
                                            }
                                        }
                                        else
                                        {
#if Arabic
                                             client.SendSysMesage("Sorry can't make socket in this item !");
#else
                                            client.SendSysMesage("Sorry can't make socket in this item !");
#endif

                                        }
                                    }
                                    break;
                                }
                            case 12:
                                {
                                    MsgGameItem DataItem;
                                    if (client.TryGetItem(id, out DataItem))
                                    {
                                        effectuid = DataItem.UID;

                                        ushort Position = Database.ItemType.ItemPosition(DataItem.ITEM_ID);
                                        if (Database.ItemType.AllowToUpdate((Role.Flags.ConquerItem)Position))
                                        {
                                            if (DataItem.SocketOne == Role.Flags.Gem.NoSocket)
                                            {
                                                Queue<MsgGameItem> LoseItems;
                                                if (client.Inventory.VerifiedUpdateItem(args, Database.ItemType.DragonBall, (byte)dwParam2, out LoseItems))
                                                {
                                                    effectdwparam1 = 1;

                                                    DataItem.SocketOne = Role.Flags.Gem.EmptySocket;
                                                    DataItem.Mode = Role.Flags.ItemMode.Update;
                                                    DataItem.Send(client, stream);
                                                    if (DataItem.Position != 0)
                                                        client.Equipment.QueryEquipment();

                                                    while (LoseItems.Count > 0)
                                                        client.Inventory.Update(LoseItems.Dequeue(), Role.Instance.AddMode.REMOVE, stream);
                                                }
                                            }
                                        }
                                        else
                                        {
#if Arabic
                                             client.SendSysMesage("Sorry can't make socket in this item !");
#else
                                            client.SendSysMesage("Sorry, you can't make a socket in this item!");
#endif

                                        }
                                    }
                                    break;
                                }
                        }
                        client.Send(stream.ItemUsageCreate(ItemUsuageID.CreateSocketItem, effectuid, effectdwparam1, 0, 0, 0, 0));

                        break;
                    }
                case ItemUsuageID.RemoveVendingItem:
                    {
                        if (client.IsVendor)
                        {
                            Role.Instance.Vendor.VendorItem VItem = null;
                            if (client.MyVendor.Items.TryRemove(id, out VItem))
                            {
                                client.Send(stream.ItemUsageCreate(action, id, dwParam, timestamp, dwParam2, dwParam3, dwparam4));
                            }
                        }
                        break;
                    }
                case ItemUsuageID.BuyVendingItem:
                    {
                        if (client.Inventory.HaveSpace(1))
                        {
                            Role.IMapObj Obj;
                            if (client.Player.View.TryGetValue((uint)dwParam, out Obj, Role.MapObjectType.SobNpc))
                            {
                                Role.SobNpc npc = Obj as Role.SobNpc;
                                if (npc.OwnerVendor.IsVendor)
                                {
                                    if (npc.OwnerVendor.Inventory.ClientItems.ContainsKey(id))
                                    {
                                        Role.Instance.Vendor.VendorItem VItem = null;
                                        if (npc.OwnerVendor.MyVendor.Items.TryGetValue(id, out VItem))
                                        {
                                            bool RightBuy = false;
                                            if (VItem.CostType == MsgItemView.ActionMode.CPs)
                                            {
                                                if (RightBuy = (client.Player.ConquerPoints >= VItem.AmountCost))
                                                {
                                                    client.Player.ConquerPoints -= (uint)VItem.AmountCost;
                                                    npc.OwnerVendor.Player.ConquerPoints += (uint)VItem.AmountCost;
                                                }
                                            }
                                            else if (VItem.CostType == MsgItemView.ActionMode.Gold)
                                            {
                                                if (RightBuy = (client.Player.Money >= VItem.AmountCost))
                                                {
                                                    client.Player.Money -= VItem.AmountCost;
                                                    npc.OwnerVendor.Player.Money += VItem.AmountCost;

                                                    client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);
                                                    npc.OwnerVendor.Player.SendUpdate(stream, npc.OwnerVendor.Player.Money, MsgUpdate.DataType.Money);
                                                }
                                            }
                                            if (RightBuy)
                                            {
                                                if (npc.OwnerVendor.MyVendor.Items.TryRemove(id, out VItem))
                                                {
                                                    client.Inventory.Update(VItem.DataItem, Role.Instance.AddMode.MOVE, stream);



                                                    client.Send(stream.ItemUsageCreate(action, id, dwParam, timestamp, dwParam2, dwParam3, dwparam4));

                                                    action = ItemUsuageID.RemoveVendingItem;
                                                    npc.OwnerVendor.Send(stream.ItemUsageCreate(action, id, dwParam, timestamp, dwParam2, dwParam3, dwparam4));


                                                    npc.OwnerVendor.Inventory.Update(VItem.DataItem, Role.Instance.AddMode.REMOVE, stream, true);


                                                    var sellit = Database.Server.ItemsBase[VItem.DataItem.ITEM_ID];
#if Arabic
                                                           string Messaj = "" + npc.OwnerVendor.Player.Name + " just sold " + sellit.Name + " to " + client.Player.Name + " for " + VItem.AmountCost + (VItem.CostType == MsgItemView.ActionMode.CPs ? " ConquerPoints." : " Gold.");
                                              
#else
                                                    string Messaj = "" + npc.OwnerVendor.Player.Name + " just sold " + sellit.Name + " to " + client.Player.Name + " for " + VItem.AmountCost + (VItem.CostType == MsgItemView.ActionMode.CPs ? " ConquerPoints." : " Gold.");

#endif
                                                    client.SendSysMesage(Messaj, MsgMessage.ChatMode.TopLeft, MsgMessage.MsgColor.red, true);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    }
                case ItemUsuageID.ShowVendingList:
                    {
                        Role.IMapObj Obj;
                        if (client.Player.View.TryGetValue(id, out Obj, Role.MapObjectType.SobNpc))
                        {
                            Role.SobNpc npc = Obj as Role.SobNpc;
                            if (npc.OwnerVendor.IsVendor)
                            {
                                foreach (var item in npc.OwnerVendor.MyVendor.Items.Values)
                                {
                                    client.Send(stream.ItemViewCreate(npc.UID, item.AmountCost, item.DataItem, item.CostType));
                                }
                            }
                        }
                        break;
                    }
                case ItemUsuageID.AddVendingItemConquerPts:
                    {
                        if (client.IsVendor)
                        {
                            MsgGameItem item;
                            if (client.Inventory.TryGetItem(id, out item))
                            {
                                if (Database.ItemType.unabletradeitem.Contains(item.ITEM_ID))
                                {
                                    break;
                                }
                                if (client.MyVendor.AddItem(item, MsgItemView.ActionMode.CPs, (uint)dwParam))
                                {
                                    client.Send(stream.ItemUsageCreate(action, id, dwParam, timestamp, dwParam2, dwParam3, dwparam4, args));
                                }
                            }
                        }
                        break;
                    }
                case ItemUsuageID.AddVendingItemGold:
                    {
                        if (client.IsVendor)
                        {
                            MsgGameItem item;
                            if (client.Inventory.TryGetItem(id, out item))
                            {
                                if (Database.ItemType.unabletradeitem.Contains(item.ITEM_ID))
                                    break;
                                if (client.MyVendor.AddItem(item, MsgItemView.ActionMode.Gold, (uint)dwParam))
                                {
                                    client.Send(stream.ItemUsageCreate(action, id, dwParam, timestamp, dwParam2, dwParam3, dwparam4, args));
                                }
                            }
                        }
                        break;
                    }
                case ItemUsuageID.DropItem:
                    {
                        if (client.InTrade)
                            return;
                        if (Program.ArenaMaps.ContainsValue(client.Player.DynamicID))
                            return;
                        if (client.Player.Map == 1005)//You can still drop any items on ground in PK Arena. *Not fixed, it only is fixed for UnlimitedArena, not PK Arena.
                            return;
                        if (client.Player.DynamicID != 0 && client.Player.Map == 700)//You can still drop any items on ground in PK Arena. *Not fixed, it only is fixed for UnlimitedArena, not PK Arena.
                            return;
                        MsgGameItem item = null;
                        if (client.Inventory.TryGetItem(id, out item))
                        {
                            if (item.Locked > 0)
                                break;
                            if (Database.ItemType.unabletradeitem.Contains(item.ITEM_ID) || item.Bound >= 1)
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream, true);
                                break;
                            }
                            ushort x = client.Player.X;
                            ushort y = client.Player.Y;
                            if (client.Map.AddGroundItem(ref x, ref y))
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream, true);
                                MsgFloorItem.MsgItem DropItem = new MsgFloorItem.MsgItem(item, x, y, MsgFloorItem.MsgItem.ItemType.Item, 0, client.Player.DynamicID, client.Player.Map
                                    , client.Player.UID, false, client.Map);
                                if (client.Map.EnqueueItem(DropItem))
                                {
                                    DropItem.SendAll(stream, MsgFloorItem.MsgDropID.Visible);
                                }
                            }
                        }
                        break;
                    }
                case ItemUsuageID.DropGold:
                    {
                        if (client.InTrade || client.IsVendor)
                            return;
                        uint Money = (uint)id;
                        if (Money > 100)
                        {
                            if (Money <= client.Player.Money)
                            {
                                ushort x = client.Player.X;
                                ushort y = client.Player.Y;
                                if (client.Map.AddGroundItem(ref x, ref y))
                                {
                                    Game.MsgServer.MsgGameItem DataItem = new Game.MsgServer.MsgGameItem();
                                    DataItem.ITEM_ID = Database.ItemType.MoneyItemID((uint)Money);
                                    DataItem.Durability = (ushort)Program.GetRandom.Next(1000, 3000);
                                    DataItem.MaximDurability = (ushort)Program.GetRandom.Next(DataItem.Durability, 6000);
                                    DataItem.Color = Role.Flags.Color.Red;

                                    Game.MsgFloorItem.MsgItem DropItem = new Game.MsgFloorItem.MsgItem(DataItem, x, y, Game.MsgFloorItem.MsgItem.ItemType.Money, Money, client.Player.DynamicID, client.Player.Map, client.Player.UID, false, client.Map);
                                    if (client.Map.EnqueueItem(DropItem))
                                    {
                                        DropItem.SendAll(stream, Game.MsgFloorItem.MsgDropID.Visible);

                                        client.Player.Money -= Money;
                                        client.Player.SendUpdate(stream, client.Player.Money, Game.MsgServer.MsgUpdate.DataType.Money);
                                    }
                                }

                            }
                        }
                        break;
                    }
                case ItemUsuageID.RepairVIP:
                    {
                        //MsgGameItem item = null;
                        //if (client.TryGetItem(id, out item))
                        foreach (var item in client.Equipment.CurentEquip)
                        {
                            if (!Database.ItemType.Equipable(item, client))
                                continue;
                            if (item.Suspicious > 0)
                                return;
                            if (Database.ItemType.IsArrow(item.ITEM_ID))
                                return;
                            if (item.Durability > 0 && item.Durability < item.MaximDurability)
                            {
                                uint Price = Database.Server.ItemsBase[item.ITEM_ID].GoldWorth;
                                byte Quality = (byte)(item.ITEM_ID % 10);
                                double QualityMultipier = 0;

                                switch (Quality)
                                {
                                    case 9: QualityMultipier = 1.100; break;
                                    case 8: QualityMultipier = 0.900; break;
                                    case 7: QualityMultipier = 0.5; break;
                                    case 6: QualityMultipier = 0.500; break;
                                    default: QualityMultipier = 0.25; break;
                                }

                                int nRepairCost = 0;
                                if (Price > 0)
                                    nRepairCost = (int)Math.Ceiling((Price * (item.MaximDurability - item.Durability) / item.MaximDurability) * QualityMultipier);

                                nRepairCost = Math.Max(1, nRepairCost);
                                if (client.Player.Money >= nRepairCost)
                                {
                                    client.Player.Money -= (uint)nRepairCost;
                                    client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);

                                    //  item.Durability = item.MaximDurability;
                                    item.Durability = item.MaximDurability;

                                    item.Mode = Role.Flags.ItemMode.Update;
                                    item.Send(client, stream);
                                }
                            }
                            else if (item.Durability == 0)
                            {
                                if (client.Inventory.Remove(1088001, 5, stream))
                                {
                                    item.Durability = item.MaximDurability;
                                    item.Mode = Role.Flags.ItemMode.Update;
                                    item.Send(client, stream);
                                }
                            }
                        }
                        break;
                    }
                case ItemUsuageID.RepairItem:
                    {
                        MsgGameItem item = null;
                        if (client.TryGetItem(id, out item))
                        {
                            if (item.Durability > 0 && item.Durability < item.MaximDurability)
                            {

                                uint Price = Database.Server.ItemsBase[item.ITEM_ID].GoldWorth;
                                byte Quality = (byte)(item.ITEM_ID % 10);
                                double QualityMultipier = 0;

                                switch (Quality)
                                {
                                    case 9: QualityMultipier = 1.100; break;
                                    case 8: QualityMultipier = 0.900; break;
                                    case 7: QualityMultipier = 0.5; break;
                                    case 6: QualityMultipier = 0.500; break;
                                    default: QualityMultipier = 0.25; break;
                                }

                                int nRepairCost = 0;
                                if (Price > 0)
                                    nRepairCost = (int)Math.Ceiling((Price * (item.MaximDurability - item.Durability) / item.MaximDurability) * QualityMultipier);

                                nRepairCost = Math.Max(1, nRepairCost);
                                if (client.Player.Money >= nRepairCost)
                                {
                                    client.Player.Money -= (uint)nRepairCost;
                                    client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);

                                    item.Durability = item.MaximDurability;
                                    item.Mode = Role.Flags.ItemMode.Update;
                                    item.Send(client, stream);
                                }
                            }
                            else if (item.Durability == 0)
                            {
                                if (client.Inventory.Remove(1088001, 5, stream))
                                {
                                    item.Durability = item.MaximDurability;
                                    item.Mode = Role.Flags.ItemMode.Update;
                                    item.Send(client, stream);
                                }
                            }
                        }
                        break;
                    }
                case ItemUsuageID.ShowWarehouseMoney:
                    {
                        dwParam = (ulong)client.Player.WHMoney;

                        client.Send(stream.ItemUsageCreate(action, id, dwParam, timestamp, dwParam2, dwParam3, dwparam4));
                        break;
                    }
                case ItemUsuageID.DepositWarehouse:
                    {
                        if (client.Player.Money > (long)dwParam)
                        {

                            client.Player.WHMoney += (long)dwParam;
                            client.Player.Money -= (uint)dwParam;
                            client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);
                            client.Send(stream.ItemUsageCreate(action, id, dwParam, timestamp, dwParam2, dwParam3, dwparam4));
                        }
                        break;
                    }
                case ItemUsuageID.WarehouseWithdraw:
                    {
                        if (client.Player.WHMoney >= (long)dwParam)
                        {
                            client.Player.Money += (uint)dwParam;
                            client.Player.WHMoney -= (long)dwParam;
                            client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);
                            client.Player.SendUpdate(stream, client.Player.WHMoney, MsgUpdate.DataType.WHMoney);
                            client.Send(stream.ItemUsageCreate(action, id, dwParam, timestamp, dwParam2, dwParam3, dwparam4));
                        }
                        break;
                    }
                case ItemUsuageID.SocketTalismanWithCPs:
                    {
                        MsgGameItem Talisman = null;
                        if (client.TryGetItem(id, out Talisman))
                        {
                            uint price = 0;
                            if (Talisman.SocketOne == Role.Flags.Gem.NoSocket)
                            {
                                //double procent = (Talisman.SocketProgress * 25600 / 2048000);
                                //if (100 - procent < 25)
                                //    return;
                                //price = (uint)(procent * 55);
                                price = (uint)(7 * (8000 - Talisman.SocketProgress) / 10);
                            }
                            else if (Talisman.SocketTwo == Role.Flags.Gem.NoSocket)
                            {
                                price = (uint)(7 * (20000 - Talisman.SocketProgress) / 10);
                            }
                            else
                                return;
                            if (client.Player.ConquerPoints >= price)
                            {
                                client.Player.ConquerPoints -= price;

                                if (Talisman.SocketOne == Role.Flags.Gem.NoSocket)
                                    Talisman.SocketOne = Role.Flags.Gem.EmptySocket;
                                else if (Talisman.SocketTwo == Role.Flags.Gem.NoSocket)
                                    Talisman.SocketTwo = Role.Flags.Gem.EmptySocket;
                                Talisman.SocketProgress = 0;
                                Talisman.Mode = Role.Flags.ItemMode.Update;
                                Talisman.Send(client, stream);
                            }
                        }
                        break;
                    }
                case ItemUsuageID.SocketTalismanWithItem:
                    {
                        MsgGameItem Talisman = null;
                        if (client.TryGetItem(id, out Talisman))
                        {
                            if (!Database.ItemType.IsTalisman(Talisman.ITEM_ID))
                                return;
                            //  foreach (var itemUID in args)
                            {
                                MsgGameItem Item = null;
                                if (client.Inventory.TryGetItem((uint)dwParam, out Item))
                                {
                                    if (Item.ITEM_ID / 1000 == Talisman.ITEM_ID / 1000 || Item.Bound == 1 || Talisman.SocketTwo != Role.Flags.Gem.NoSocket)
                                        return;

                                    uint Points = 0;
                                    switch (Item.ITEM_ID % 10)
                                    {
                                        case 6: Points += 5; break;
                                        case 7: Points += 10; break;
                                        case 8: Points += 40; break;
                                        case 9: Points += 1000; break;
                                    }

                                    int position = Database.ItemType.ItemPosition(Item.ITEM_ID);
                                    switch (position)
                                    {
                                        case 0: return;
                                        case 4:
                                        case 5:
                                            if (Item.ITEM_ID % 10 >= 8)
                                            {
                                                if (Item.SocketOne != Role.Flags.Gem.NoSocket)
                                                    Points += 160;
                                                if (Item.SocketTwo != Role.Flags.Gem.NoSocket)
                                                    Points += 800;
                                            }
                                            break;
                                        default:
                                            if (Item.ITEM_ID % 10 >= 8)
                                            {
                                                if (Item.SocketOne != Role.Flags.Gem.NoSocket)
                                                    Points += 2000;
                                                if (Item.SocketTwo != Role.Flags.Gem.NoSocket)
                                                    Points += 6000;
                                            }
                                            break;
                                    }
                                    Talisman.SocketProgress += Points;
                                    if (Talisman.SocketOne == Role.Flags.Gem.NoSocket)
                                    {
                                        if (Talisman.SocketProgress >= 8000)
                                        {
                                            Talisman.SocketProgress -= 8000;
                                            Talisman.SocketOne = Role.Flags.Gem.EmptySocket;
                                        }
                                    }
                                    if (Talisman.SocketOne != Role.Flags.Gem.NoSocket)
                                    {
                                        if (Talisman.SocketProgress >= 20000)
                                        {
                                            Talisman.SocketProgress = 0;
                                            Talisman.SocketTwo = Role.Flags.Gem.EmptySocket;
                                        }
                                    }
                                    Talisman.Mode = Role.Flags.ItemMode.Update;
                                    Talisman.Send(client, stream).Update(Item, Role.Instance.AddMode.REMOVE, stream);
                                }
                            }
                        }
                        break;
                    }
                case ItemUsuageID.SellItem:
                    {
                        uint ShopUID = id;
                        uint ItemUID = (uint)dwParam;
                        Game.MsgNpc.Npc obj;
                        if (client.Map.SearchNpcInScreen(ShopUID, client.Player.X, client.Player.Y, out obj))
                        {
                            Game.MsgServer.MsgGameItem Item;
                            if (client.Inventory.TryGetItem(ItemUID, out Item))
                            {
                                if (Item.Locked > 0)
                                {
                                    client.SendSysMesage("This item is Inscribed in your guild, or this items is locked.");
                                    break;
                                }
                                Database.ItemType.DBItem DBItem = null;
                                if (Database.Server.ItemsBase.TryGetValue(Item.ITEM_ID, out DBItem))
                                {
                                    int prince = (int)(DBItem.GoldWorth / 3);
                                    if (Item.Durability > 0 && Item.Durability < Item.MaximDurability)
                                        prince = (prince * Item.Durability) / Item.MaximDurability;

                                    if (Item.Durability > 0 && Item.Durability <= Item.MaximDurability)
                                    {
                                        client.Inventory.Update(Item, Role.Instance.AddMode.REMOVE, stream);
                                        client.Player.Money += (uint)prince;
                                        client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);
                                    }
                                    else
                                    {
                                        client.Inventory.Update(Item, Role.Instance.AddMode.REMOVE, stream);
                                    }
                                }
                            }

                        }
                        break;
                    }
                case ItemUsuageID.BuyItem:
                    {
                        uint ShopUID = id;
                        uint ItemID = (uint)dwParam;
                        uint Counts = dwParam2;
                        if (ShopUID == 0)
                            return;
                        Game.MsgNpc.Npc obj;

                        if (client.Map.SearchNpcInScreen(ShopUID, client.Player.X, client.Player.Y, out obj) /* || ShopUID == 2888 */)
                        {
                            Database.Shops.ShopFile.Shop shop = new Database.Shops.ShopFile.Shop();
                            if (!Database.Shops.ShopFile.Shops.TryGetValue(ShopUID, out shop))
                                shop = null;
                            if (shop != null && shop.UID != 0)
                            {
                                if (dwparam4 == 2)
                                {
                                    //if (!shop.BoundItems.Contains(ItemID))
                                    //    return;
                                    Database.ItemType.DBItem DBItem = null;
                                    if (Database.Server.ItemsBase.TryGetValue(ItemID, out DBItem))
                                    {
                                        uint Amount = Counts > 0 ? Counts : 1;

                                        while (Amount > 0)
                                        {
                                            switch (shop.MoneyType)
                                            {
                                                case Database.Shops.ShopFile.MoneyType.ConquerPoints:
                                                    {
                                                        if (DBItem.ConquerPointsWorth >= 1)
                                                        {
                                                            if (client.Player.BoundConquerPoints >= DBItem.ConquerPointsWorth)
                                                            {
                                                                byte plus = 0;
                                                                if (DBItem.ID >= 730001 && DBItem.ID <= 730008)
                                                                    plus = (byte)(DBItem.ID % 10);
                                                                client.Player.BoundConquerPoints -= (int)DBItem.ConquerPointsWorth;
                                                                client.Inventory.Add(DBItem.ID, plus, DBItem, stream, true);

                                                            }
                                                        }
                                                        break;
                                                    }
                                            }
                                            Amount--;
                                        }
                                    }
                                }
                                else
                                {
                                    if (!shop.Items.Contains(ItemID))
                                        return;
                                    Database.ItemType.DBItem DBItem = null;
                                    if (Database.Server.ItemsBase.TryGetValue(ItemID, out DBItem))
                                    {
                                        uint Amount = Counts > 0 ? Counts : 1;

                                        while (Amount > 0)
                                        {
                                            switch (shop.MoneyType)
                                            {
                                                case Database.Shops.ShopFile.MoneyType.ConquerPoints:
                                                    {
                                                        if (DBItem.ConquerPointsWorth >= 1)
                                                        {
                                                            if (client.Player.ConquerPoints >= DBItem.ConquerPointsWorth)
                                                            {
                                                                byte plus = 0;
                                                                if (DBItem.ID >= 730001 && DBItem.ID <= 730008)
                                                                    plus = (byte)(DBItem.ID % 10);
                                                                client.Player.ConquerPoints -= DBItem.ConquerPointsWorth;
                                                                client.Inventory.Add(DBItem.ID, plus, DBItem, stream);

                                                            }
                                                        }
                                                        break;
                                                    }
                                                case Database.Shops.ShopFile.MoneyType.Gold:
                                                    {
                                                        if (DBItem.GoldWorth >= 1)
                                                        {
                                                            if (client.Player.Money >= DBItem.GoldWorth)
                                                            {
                                                                client.Player.Money -= DBItem.GoldWorth;
                                                                client.Inventory.Add(DBItem.ID, 0, DBItem, stream);
                                                                client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);
                                                            }
                                                        }
                                                        break;
                                                    }
                                            }
                                            Amount--;
                                        }
                                    }
                                }

                            }
                        }
                        break;
                    }
                #region PING
                case ItemUsuageID.Ping:
                    {
                        client.Send(stream.ItemUsageCreate(action, id, dwParam, timestamp, dwParam2, dwParam3, dwparam4));
                        break;
                    }
                #endregion
                case ItemUsuageID.Unequip:
                    {
                        uint Position = (uint)dwParam;

                        if (client.Equipment.Remove((Role.Flags.ConquerItem)Position, stream))
                        {
                            if (client.OnAutoAttack)
                                client.OnAutoAttack = false;
                            if (client.Player.Mining) client.Player.Mining = false;
                            //lock (client.ItemSyncRoot)
                            client.Equipment.QueryEquipment();
                        }
                        break;
                    }
                case ItemUsuageID.SetEquipPosition:
                case ItemUsuageID.Equip:
                    {
                        if (dwParam < 20)
                            //lock (client.ItemSyncRoot)
                            EquipItem(client, stream, id, (uint)dwParam, timestamp, dwParam2, dwParam3, dwparam4);
                        //else
                        //AlternanteEquipItem(client, stream, id, (uint)dwParam, timestamp, dwParam2, dwParam3, dwparam4);
                        break;
                    }
                default:
                    {
                        // Console.WriteLine(" [" + client.Player.Name + "] MsgItemUsuagePacket  not find -> " + id);
                        break;
                    }
            }
        }
        public unsafe static void EquipItem(Client.GameClient client, ServerSockets.Packet stream, uint id, uint dwParam1, uint timestamp, uint dwParam2, uint dwParam3, uint dwparam4)
        {
            uint Position = dwParam1;
            client.OnAutoAttack = false;
            Game.MsgServer.MsgGameItem item = null;
            if (client.Inventory.TryGetItem(id, out item))
            {
                if (client.Player.Mining) client.Player.Mining = false;

                switch (item.ITEM_ID)
                {
                    case 1200000:
                    case 1200001:
                    case 1200002:
                        {
                            UseItem(item, client, stream);
                            return;
                        }
                    default:
                        {
                            if (Position == 17 && Database.ItemType.ItemPosition(item.ITEM_ID) == 0)
                            {
                                UseItem(item, client, stream);
                                break;
                            }
                            else if (Position == 0 && Database.ItemType.ItemPosition(item.ITEM_ID) == 0)
                            {
                                UseItem(item, client, stream);
                                break;
                            }
                            break;
                        }
                }
                //if (Database.ItemType.ItemPosition(item.ITEM_ID) == (ushort)Role.Flags.ConquerItem.Garment)
                //{
                //    if (client.Player.SpecialGarment != 0)
                //    {
                //        client.SendSysMesage("Item can't be unequiped during the event.");
                //        return;
                //    }
                //}

                if (client.Player.Class >= 50 && client.Player.Class <= 55 && !Database.ItemType.IsKatana(item.ITEM_ID) && Position == 4)
                {
                    client.SendSysMesage("Sorry, you can't equpiment only Katana because it's the main weapon.");
                    return;

                }
                if (client.Player.Class >= 50 && client.Player.Class <= 55 && !Database.ItemType.IsKatana(item.ITEM_ID) && Position == 5)
                {
                    client.SendSysMesage("Sorry, you can't equpiment only Katana because it's the main weapon.");
                    return;
                }

                bool can2hand = false;
                bool can2wpn = false;
                if (client.Player.Class >= 11 && client.Player.Class <= 85 || client.Player.Class >= 160 && client.Player.Class <= 165)
                    can2hand = true;
                if (client.Player.Class >= 11 && client.Player.Class <= 15 || client.Player.Class >= 20 && client.Player.Class <= 25 || client.Player.Class >= 51 && client.Player.Class <= 145
                    || client.Player.Class >= 160 && client.Player.Class <= 165)
                    can2wpn = true;

                if (!Database.ItemType.Equipable(item, client))
                    return;
                ushort po = Database.ItemType.ItemPosition(item.ITEM_ID);

                if (Database.ItemType.IsShield(item.ITEM_ID) && !(client.Player.Class >= 21 && client.Player.Class <= 25))
                    return;
                if (Position == 5)
                {
                    bool isPrevArch = (client.Player.Reborn > 0 && (client.Player.FirstClass == 45 || client.Player.SecondClass == 45));

                    if (!(Database.AtributesStatus.IsArcher(client.Player.Class)) && !isPrevArch)
                    {
                        if (Database.ItemType.IsTwoHand(client.Equipment.RightWeapon))
                        {
                            if (Database.ItemType.IsShield(item.ITEM_ID) == false)
                                return;
                        }
                    }
                }
                if (po == (ushort)Role.Flags.ConquerItem.Tower || po == (ushort)Role.Flags.ConquerItem.Fan)
                    return;

                // if (po == (ushort)Role.Flags.ConquerItem.Garment || po == (ushort)Role.Flags.ConquerItem.SteedMount)
                //     return;
                if (po == 5)
                {
                    Position = 5;
                    if (!can2hand && !can2wpn)
                        return;
                    if (!client.Equipment.FreeEquip(Role.Flags.ConquerItem.RightWeapon))
                    {
                        Game.MsgServer.MsgGameItem RightWeapon;
                        if (client.Equipment.TryGetEquip(Role.Flags.ConquerItem.RightWeapon, out RightWeapon))
                        {
                            if (RightWeapon.ITEM_ID / 1000 != 500 && Database.ItemType.IsArrow(item.ITEM_ID))
                                return;
                        }
                    }
                    else
                        return;
                }
                else if (po == 0) return;
                if (Database.ItemType.ItemPosition(item.ITEM_ID) == 4)
                {
                    if (Position == 5)
                        if (!can2hand || !can2wpn)
                            return;
                }
                bool twohand = Database.ItemType.IsTwoHand(item.ITEM_ID);
                if (!twohand && Position == 4)
                {
                    Game.MsgServer.MsgGameItem LeftWeapon;
                    if (client.Equipment.TryGetEquip(Role.Flags.ConquerItem.LeftWeapon, out LeftWeapon))
                    {
                        if (client.Inventory.HaveSpace(1))
                        {
                            if (Database.ItemType.IsArrow(LeftWeapon.ITEM_ID))
                                client.Equipment.Remove(Role.Flags.ConquerItem.LeftWeapon, stream);
                            else
                            {
                                Game.MsgServer.MsgGameItem RightWeapon;
                                if (client.Equipment.TryGetEquip(Role.Flags.ConquerItem.RightWeapon, out RightWeapon))
                                {
                                    if (Database.ItemType.IsTwoHand(RightWeapon.ITEM_ID))
                                        client.Equipment.Remove(Role.Flags.ConquerItem.RightWeapon, stream);
                                }
                            }
                        }
                    }
                }

                if (client.Equipment.FreeEquip((Role.Flags.ConquerItem)Position))
                {
                    bool spaceinventory = true;
                    if (twohand)
                    {
                        if (client.Inventory.HaveSpace(1))
                        {
                            client.Equipment.Remove(Role.Flags.ConquerItem.LeftWeapon, stream);
                        }
                        else
                        {
                            spaceinventory = false;
                        }
                    }
                    if (spaceinventory)
                    {
                        client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                        item.Position = (ushort)Position;
                        client.Equipment.Add(item, stream);
                        item.Mode = Role.Flags.ItemMode.Update;
                        item.Send(client, stream);
                    }
                }
                else//if is item on character
                {
                    bool spaceinventory = true;
                    if (twohand)
                    {
                        if (client.Inventory.HaveSpace(1))
                        {
                            client.Equipment.Remove(Role.Flags.ConquerItem.LeftWeapon, stream);
                        }
                        else
                            spaceinventory = false;
                    }
                    if (spaceinventory)
                    {
                        client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                        item.Position = (ushort)Position;
                        item.Mode = Role.Flags.ItemMode.AddItem;
                        //item.Send(client);
                        client.Equipment.Remove((Role.Flags.ConquerItem)Position, stream);
                        client.Equipment.Add(item, stream);
                    }
                }
                client.Equipment.QueryEquipment();
                client.Send(stream.ItemUsageCreate(ItemUsuageID.SetEquipPosition, item.UID, Position, 0, 0, 0, 0));

            }
        }

        public static void UseItem(Game.MsgServer.MsgGameItem item, Client.GameClient client, ServerSockets.Packet stream)
        {
            if (!client.Player.Alive)
                return;
            if (DateTime.Now < client.ItemStamp.AddMilliseconds(200))
                return;
            client.ItemStamp = DateTime.Now;
            Database.ItemType.DBItem DBItem;
            if (Database.Server.ItemsBase.TryGetValue(item.ITEM_ID, out DBItem))
            {
                if (item.ITEM_ID >= 726001 && item.ITEM_ID <= 726108 || item.ITEM_ID == 722559// item box.
                || item.ITEM_ID >= 721177 && item.ITEM_ID <= 721202)
                {
                    if (Database.HouseTable.InHouse(client.Player.Map) && client.Player.DynamicID == client.Player.UID/*that check if is you house*/)
                    {
                        NpcServerQuery Furniture = new NpcServerQuery();
                        if (item.ITEM_ID == 722559)
                            Furniture.NpcType = Role.Flags.NpcType.Talker;
                        else
                            Furniture.NpcType = Role.Flags.NpcType.Furniture;

                        Furniture.Action = MsgNpc.NpcServerReplay.Mode.Cursor;
                        var Npc = Database.NpcServer.GetNpc(item.ITEM_ID);
                        if (Npc != null)
                        {
                            client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);

                            Furniture.Mesh = Npc.Mesh;
                            Furniture.ID = (MsgNpc.NpcID)Npc.UID;
                            client.MoveNpcMesh = Furniture.Mesh;
                            client.MoveNpcUID = (uint)Furniture.ID;
                            client.Send(stream.NpcServerCreate(Furniture));
                        }
                    }
                    else
                    {
#if Arabic
                          client.SendSysMesage("You have to be in your own house to be able to display it.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#else
                        client.SendSysMesage("You have to be in your own house to be able to display it.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#endif

                    }
                    return;
                }
                if (item.ITEM_ID >= 1000000 && item.ITEM_ID <= 1000040
                    || item.ITEM_ID >= 1002000 && item.ITEM_ID < 1002030
                    || item.ITEM_ID == 1002050
                    || item.ITEM_ID == 725065 || item.ITEM_ID == 1003010)
                {
                    if (DateTime.Now > client.Player.MedicineStamp.AddMilliseconds(100) && !Program.NoDrugMap.Contains(client.Player.Map))
                    {
                        if (client.Player.ContainFlag(MsgUpdate.Flags.PoisonStar))
                            return;
                        if (client.Player.HitPoints == client.Status.MaxHitpoints)
                            return;
                        client.Player.HitPoints = Math.Min(client.Player.HitPoints + DBItem.ItemHP, (int)client.Status.MaxHitpoints);
                        client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                        client.Player.MedicineStamp = DateTime.Now;
                    }
                }
                else if (item.ITEM_ID >= 1001000 && item.ITEM_ID <= 1001040
                    || item.ITEM_ID == 1002030 || item.ITEM_ID == 1002040
                    || item.ITEM_ID == 725066 || item.ITEM_ID == 1004010)
                {
                    if (client.Player.ContainFlag(MsgUpdate.Flags.PoisonStar))
                        return;
                    if (client.Player.Mana == client.Status.MaxMana)
                        return;
                    client.Player.Mana = (ushort)Math.Min(client.Player.Mana + DBItem.ItemMP, (int)client.Status.MaxMana);
                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                }
                else if (item.ITEM_ID >= 725000 && item.ITEM_ID <= 725044 || item.ITEM_ID == 1060101
                    || item.ITEM_ID == 721158 || item.ITEM_ID == 721157 || item.ITEM_ID == 3007568 || item.ITEM_ID == 3007567
                    || item.ITEM_ID == 3007566 || item.ITEM_ID >= 3006017 && item.ITEM_ID <= 3006020 || item.ITEM_ID == Database.ItemType.DragonBall || item.ITEM_ID == Database.ItemType.Meteor)//skills books
                {
                    switch (item.ITEM_ID)
                    {
                        //aquigeraritem

                        case Database.ItemType.DragonBall:
                            {
                                if (client.Inventory.HaveSpace(1))
                                {
                                    if (client.Inventory.Contain(Database.ItemType.DragonBall, 10))
                                    {
                                        client.Inventory.Remove(Database.ItemType.DragonBall, 10, stream);
                                        client.Inventory.Add(stream, Database.ItemType.DragonBallScroll, 1);
                                    }
                                }
                                else
                                {
                                    client.SendSysMesage("please free some space from your inventory");
                                }
                                break;
                            }
                        case Database.ItemType.Meteor:
                            {
                                if (client.Inventory.HaveSpace(1))
                                {
                                    if (client.Inventory.Contain(Database.ItemType.Meteor, 10))
                                    {
                                        client.Inventory.Remove(Database.ItemType.Meteor, 10, stream);
                                        client.Inventory.Add(stream, Database.ItemType.MeteorScroll, 1);
                                    }
                                }
                                else
                                {
                                    client.SendSysMesage("please free some space from your inventory");
                                }
                                break;
                            }
                        case Database.ItemType.SoulAroma:
                            {
                                if (client.Inventory.HaveSpace(1))
                                {
                                    if (client.Inventory.Contain(Database.ItemType.SoulAroma, 10))
                                    {
                                        client.Inventory.Remove(Database.ItemType.SoulAroma, 10, stream);
                                        client.Inventory.Add(stream, Database.ItemType.SoulAromaBag, 1);
                                    }
                                }
                                else
                                {
                                    client.SendSysMesage("please free some space from your inventory");
                                }
                                break;
                            }
                        case Database.ItemType.DreamGrass:
                            {
                                if (client.Inventory.HaveSpace(1))
                                {
                                    if (client.Inventory.Contain(Database.ItemType.DreamGrass, 10))
                                    {
                                        client.Inventory.Remove(Database.ItemType.DreamGrass, 10, stream);
                                        client.Inventory.Add(stream, Database.ItemType.DreamGrassBag, 1);
                                    }
                                }
                                else
                                {
                                    client.SendSysMesage("please free some space from your inventory");
                                }
                                break;
                            }
                        case Database.ItemType.Moss:
                            {
                                if (client.Inventory.HaveSpace(1))
                                {
                                    if (client.Inventory.Contain(Database.ItemType.Moss, 10))
                                    {
                                        client.Inventory.Remove(Database.ItemType.Moss, 10, stream);
                                        client.Inventory.Add(stream, Database.ItemType.MossBag, 1);
                                    }
                                }
                                else
                                {
                                    client.SendSysMesage("please free some space from your inventory");
                                }
                                break;
                            }



                        case 725018:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.MySpells.Add(stream, 1380, 0, 0, 0, 0);
                                break;
                            }
                        case 725019:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.MySpells.Add(stream, 1385, 0, 0, 0, 0);
                                break;
                            }
                        case 725020:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.MySpells.Add(stream, 1390, 0, 0, 0, 0);
                                break;
                            }
                        case 725021:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.MySpells.Add(stream, 1395, 0, 0, 0, 0);
                                break;
                            }
                        case 725022:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.MySpells.Add(stream, 1400, 0, 0, 0, 0);
                                break;
                            }
                        case 725023:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.MySpells.Add(stream, 1405, 0, 0, 0, 0);
                                break;
                            }
                        case 725024:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.MySpells.Add(stream, 1410, 0, 0, 0, 0);
                                break;
                            }
                        case 1060101://fireofheal
                            {
                                break;





                                if (client.Player.Level < 84)
                                {
#if Arabic
                                     client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~84.~Please~train~harder.", "I~see.");
#else
                                    client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~84.~Please~train~harder.", "I~see.");
#endif

                                    break;
                                }
                                if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.FireofHell))
                                {
                                    if (Database.AtributesStatus.IsFire(client.Player.Class))
                                    {
                                        client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                        client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.FireofHell);
#if Arabic
                                          client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.FireofHell.ToString() + ".", "I~see.");
#else
                                        client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.FireofHell.ToString() + ".", "I~see.");
#endif

                                    }
                                    else
                                    {
#if Arabic
                                          client.CreateDialog(stream, "Sorry,this spell is just for Fire Taoist`s.", "I~see.");
#else
                                        client.CreateDialog(stream, "Sorry,this spell is just for Fire Taoist`s.", "I~see.");
#endif

                                    }
                                }
                                else
                                {
#if Arabic
                                      client.CreateDialog(stream, "You already have this spell.", "I~see.");
#else
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#endif

                                }
                                break;
                            }
                        case 725016://nightdevil
                            {
                                if (client.Player.Level < 90)
                                {
#if Arabic
                                       client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~90.~Please~train~harder.", "I~see.");
#else
                                    client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~90.~Please~train~harder.", "I~see.");
#endif

                                    break;
                                }
                                if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.NightDevil))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.NightDevil);
#if Arabic
                                    client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.NightDevil.ToString() + ".", "I~see.");
#else
                                    client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.NightDevil.ToString() + ".", "I~see.");
#endif

                                }
                                else
                                {
#if Arabic
                                       client.CreateDialog(stream, "You already have this spell.", "I~see.");
#else
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#endif

                                }
                                break;
                            }
                        case 721157://dragon tail
                            {
                                if (client.Player.Level < 40)
                                {
#if Arabic
                                      client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~40.~Please~train~harder.", "I~see.");
#else
                                    client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~40.~Please~train~harder.", "I~see.");
#endif

                                    break;
                                }
                                if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.DragonTail))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.DragonTail);
#if Arabic
                                    client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.DragonTail.ToString() + ".", "I~see.");
#else
                                    client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.DragonTail.ToString() + ".", "I~see.");
#endif

                                }
                                else
                                {
#if Arabic
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#else
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#endif

                                }
                                break;
                            }
                        case 721158://ViperFang
                            {
                                if (client.Player.Level < 40)
                                {
#if Arabic
                                     client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~40.~Please~train~harder.", "I~see.");
#else
                                    client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~40.~Please~train~harder.", "I~see.");
#endif

                                    break;
                                }
                                if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.ViperFang))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.ViperFang);
#if Arabic
                                    client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.ViperFang.ToString() + ".", "I~see.");
#else
                                    client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.ViperFang.ToString() + ".", "I~see.");
#endif

                                }
                                else
                                {
#if Arabic
                                      client.CreateDialog(stream, "You already have this spell.", "I~see.");
#else
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#endif

                                }
                                break;
                            }
                        case 725005://fastblader
                            {
                                if (client.Player.Level < 40)
                                {
#if Arabic
                                             client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~40.~Please~train~harder.", "I~see.");
#else
                                    client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~40.~Please~train~harder.", "I~see.");
#endif

                                    break;
                                }
                                if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.FastBlader))
                                {
                                    if (client.MyProfs.CheckProf((ushort)Database.MagicType.WeaponsType.Blade, 2))
                                    {
                                        client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                        client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.FastBlader);
#if Arabic
                                        
                                        client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.FastBlader.ToString() + ".", "I~see.");
#else

                                        client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.FastBlader.ToString() + ".", "I~see.");
#endif
                                    }
                                    else
                                    {
#if Arabic
                                        client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~skill~before~you~practice~your~blade~to~level~5.~Please~train~harder.", "I~see.");
#else
                                        client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~skill~before~you~practice~your~blade~to~level~2.~Please~train~harder.", "I~see.");
#endif

                                    }
                                }
                                else
                                {
#if Arabic
                                       client.CreateDialog(stream, "You already have this spell.", "I~see.");
#else
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#endif

                                }
                                break;
                            }
                        case 725010://screensword
                            {
                                if (client.Player.Level < 40)
                                {
#if Arabic
                                    client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~40.~Please~train~harder.", "I~see.");
#else
                                    client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~40.~Please~train~harder.", "I~see.");
#endif

                                    break;
                                }
                                if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.ScrenSword))
                                {
                                    if (client.MyProfs.CheckProf((ushort)Database.MagicType.WeaponsType.Sword, 2))
                                    {
                                        client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                        client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.ScrenSword);
#if Arabic
                                              client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.ScrenSword.ToString() + ".", "I~see.");
#else
                                        client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.ScrenSword.ToString() + ".", "I~see.");
#endif

                                    }
                                    else
                                    {
#if Arabic
                                        client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~skill~before~you~practice~your~sword~to~level~5.~Please~train~harder.", "I~see.");
#else
                                        client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~skill~before~you~practice~your~sword~to~level~2.~Please~train~harder.", "I~see.");
#endif

                                    }
                                }
                                else
                                {
#if Arabic
                                     client.CreateDialog(stream, "You already have this spell.", "I~see.");
#else
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#endif

                                }
                                break;
                            }
                        case 725029://pheonix
                            {
                                if (client.Player.Level < 30)
                                {
#if Arabic
                                       client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~30.~Please~train~harder.", "I~see.");
#else
                                    client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~30.~Please~train~harder.", "I~see.");
#endif

                                    break;
                                }
                                if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Phoenix))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Phoenix);
#if Arabic
                                     client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.Phoenix.ToString() + ".", "I~see.");
#else
                                    client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.Phoenix.ToString() + ".", "I~see.");
#endif

                                }
                                else
                                {
#if Arabic
                                      client.CreateDialog(stream, "You already have this spell.", "I~see.");
#else
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#endif

                                }
                                break;
                            }
                        case 725042://rage
                            {
                                if (client.Player.Level < 30)
                                {
#if Arabic
                                     client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~30.~Please~train~harder.", "I~see.");
#else
                                    client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~30.~Please~train~harder.", "I~see.");
#endif

                                    break;
                                }
                                if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Rage))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Rage);
#if Arabic
                                    client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.Rage.ToString() + ".", "I~see.");
#else
                                    client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.Rage.ToString() + ".", "I~see.");
#endif

                                }
                                else
                                {
#if Arabic
                                     client.CreateDialog(stream, "You already have this spell.", "I~see.");
#else
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#endif

                                }
                                break;
                            }
                        case 725030://boom
                            {
                                if (client.Player.Level < 30)
                                {
#if Arabic
                                      client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~30.~Please~train~harder.", "I~see.");
#else
                                    client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~30.~Please~train~harder.", "I~see.");
#endif

                                    break;
                                }
                                if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Boom))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Boom);
#if Arabic
                                      client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.Boom.ToString() + ".", "I~see.");
#else
                                    client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.Boom.ToString() + ".", "I~see.");
#endif

                                }
                                else
                                {
#if Arabic
                                      client.CreateDialog(stream, "You already have this spell.", "I~see.");
#else
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#endif

                                }
                                break;
                            }
                        case 725040://seizer
                            {
                                if (client.Player.Level < 30)
                                {
#if Arabic
                                       client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~30.~Please~train~harder.", "I~see.");
#else
                                    client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~30.~Please~train~harder.", "I~see.");
#endif

                                    break;
                                }
                                if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Seizer))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Seizer);
#if Arabic
                                        client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.Seizer.ToString() + ".", "I~see.");
#else
                                    client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.Seizer.ToString() + ".", "I~see.");
#endif

                                }
                                else
                                {
#if Arabic
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#else
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#endif
                                }
                                break;
                            }
                        case 725041://Earthquake
                            {
                                if (client.Player.Level < 30)
                                {
#if Arabic
                                         client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~30.~Please~train~harder.", "I~see.");
#else
                                    client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~30.~Please~train~harder.", "I~see.");
#endif

                                    break;
                                }
                                if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Earthquake))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Earthquake);
#if Arabic
                                      client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.Earthquake.ToString() + ".", "I~see.");
#else
                                    client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.Earthquake.ToString() + ".", "I~see.");
#endif

                                }
                                else
                                {
#if Arabic
                                        client.CreateDialog(stream, "You already have this spell.", "I~see.");
#else
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#endif

                                }
                                break;
                            }
                        case 725043://celestial
                            {
                                if (client.Player.Level < 30)
                                {
#if Arabic
                                     client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~30.~Please~train~harder.", "I~see.");
#else
                                    client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~30.~Please~train~harder.", "I~see.");
#endif

                                    break;
                                }
                                if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Celestial))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Celestial);
#if Arabic
                                          client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.Celestial.ToString() + ".", "I~see.");
#else
                                    client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.Celestial.ToString() + ".", "I~see.");
#endif

                                }
                                else
                                {
#if Arabic
                                       client.CreateDialog(stream, "You already have this spell.", "I~see.");
#else
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#endif

                                }
                                break;
                            }
                        case 725044://roamer
                            {
                                if (client.Player.Level < 30)
                                {
#if Arabic
                                      client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~30.~Please~train~harder.", "I~see.");
#else
                                    client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~30.~Please~train~harder.", "I~see.");
#endif

                                    break;
                                }
                                if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Roamer))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Roamer);
#if Arabic
                                    client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.Roamer.ToString() + ".", "I~see.");
#else
                                    client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.Roamer.ToString() + ".", "I~see.");
#endif

                                }
                                else
                                {
#if Arabic
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#else
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#endif

                                }
                                break;
                            }
                        case 725013://penetarion
                            {
                                if (client.Player.Level < 30)
                                {
#if Arabic
                                     client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~30.~Please~train~harder.", "I~see.");
#else
                                    client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~30.~Please~train~harder.", "I~see.");
#endif

                                    break;
                                }
                                if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Penetration))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Penetration);
#if Arabic
                                     client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.Penetration.ToString() + ".", "I~see.");
#else
                                    client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.Penetration.ToString() + ".", "I~see.");
#endif

                                }
                                else
                                {
#if Arabic
                                           client.CreateDialog(stream, "You already have this spell.", "I~see.");
#else
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#endif

                                }
                                break;
                            }
                        case 725026://snow
                            {
                                if (client.Player.Level < 30)
                                {
#if Arabic
                                          client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~30.~Please~train~harder.", "I~see.");
#else
                                    client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~30.~Please~train~harder.", "I~see.");
#endif

                                    break;
                                }
                                if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Snow))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Snow);
#if Arabic
                                          client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.Snow.ToString() + ".", "I~see.");
#else
                                    client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.Snow.ToString() + ".", "I~see.");
#endif

                                }
                                else
                                {
#if Arabic
                                     client.CreateDialog(stream, "You already have this spell.", "I~see.");
#else
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#endif

                                }
                                break;
                            }
                        case 725012://speed Gun
                            {
                                if (client.Player.Level < 30)
                                {
#if Arabic
                                      client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~30.~Please~train~harder.", "I~see.");
#else
                                    client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~30.~Please~train~harder.", "I~see.");
#endif

                                    break;
                                }
                                if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.SpeedGun))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.SpeedGun);
#if Arabic
                                      client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.SpeedGun.ToString() + ".", "I~see.");
#else
                                    client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.SpeedGun.ToString() + ".", "I~see.");
#endif

                                }
                                else
                                {
#if Arabic
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#else
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#endif

                                }
                                break;
                            }
                        case 725011://WideStrike
                            {
                                if (client.Player.Level < 30)
                                {
#if Arabic
                                       client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~30.~Please~train~harder.", "I~see.");
#else
                                    client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~30.~Please~train~harder.", "I~see.");
#endif

                                    break;
                                }
                                if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.WideStrike))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.WideStrike);
#if Arabic
                                      client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.WideStrike.ToString() + ".", "I~see.");
#else
                                    client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.WideStrike.ToString() + ".", "I~see.");
#endif

                                }
                                else
                                {
#if Arabic
                                       client.CreateDialog(stream, "You already have this spell.", "I~see.");
#else
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#endif

                                }
                                break;
                            }
                        case 725027:// StrandedMonster
                            {
                                if (client.Player.Level < 30)
                                {
#if Arabic
                                      client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~30.~Please~train~harder.", "I~see.");
#else
                                    client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~30.~Please~train~harder.", "I~see.");
#endif

                                    break;
                                }
                                if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.StrandedMonster))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.StrandedMonster);
#if Arabic
                                           client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.StrandedMonster.ToString() + ".", "I~see.");
#else
                                    client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.StrandedMonster.ToString() + ".", "I~see.");
#endif

                                }
                                else
                                {
#if Arabic
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#else
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#endif

                                }
                                break;
                            }
                        case 725014://halt
                            {
                                if (client.Player.Level < 30)
                                {
#if Arabic
                                    client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~30.~Please~train~harder.", "I~see.");
#else
                                    client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~30.~Please~train~harder.", "I~see.");
#endif

                                    break;
                                }
                                if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Halt))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Halt);
#if Arabic
                                    client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.Halt.ToString() + ".", "I~see.");
#else
                                    client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.Halt.ToString() + ".", "I~see.");
#endif

                                }
                                else
                                {
#if Arabic
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#else
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#endif

                                }
                                break;
                            }
                        case 725031://bores
                            {
                                if (client.Player.Level < 30)
                                {
#if Arabic
                                     client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~30.~Please~train~harder.", "I~see.");
#else
                                    client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~30.~Please~train~harder.", "I~see.");
#endif

                                    break;
                                }
                                if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Boreas))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Boreas);
#if Arabic
                                    client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.Boreas.ToString() + ".", "I~see.");
#else
                                    client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.Boreas.ToString() + ".", "I~see.");
#endif

                                }
                                else
                                {
#if Arabic
                                     client.CreateDialog(stream, "You already have this spell.", "I~see.");
#else
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#endif

                                }

                                break;
                            }
                        case 725000://thunder.
                            {
                                if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Thunder))
                                {
                                    if (client.Player.Spirit >= 20)
                                    {
                                        client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                        client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Thunder);
#if Arabic
                                        client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.Thunder.ToString() + ".", "I~see.");
#else
                                        client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.Thunder.ToString() + ".", "I~see.");
#endif

                                    }
                                    else
                                    {
#if Arabic
                                        client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Spirit~is~20.~Please~train~harder.", "I~see.");
#else
                                        client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Spirit~is~20.~Please~train~harder.", "I~see.");
#endif

                                    }
                                }
                                else
                                {
#if Arabic
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#else
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#endif

                                }
                                break;
                            }
                        case 725001://fire
                            {
                                if (client.Player.Level < 40)
                                {
#if Arabic
                                     client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~40.~Please~train~harder.", "I~see.");
#else
                                    client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~40.~Please~train~harder.", "I~see.");
#endif
                                    break;
                                }
                                if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Fire))
                                {
                                    //if (Database.AtributesStatus.IsTaoist(client.Player.Class))
                                    {
                                        if (client.MySpells.CheckSpell((ushort)Role.Flags.SpellID.Thunder, 4))
                                        {
                                            if (client.Player.Spirit >= 80)
                                            {
                                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                                client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Fire);
#if Arabic
                                                  client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.Fire.ToString() + ".", "I~see.");
#else
                                                client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.Fire.ToString() + ".", "I~see.");
#endif

                                            }
                                            else
                                            {
#if Arabic
                                                client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Spirit~is~80.~Please~train~harder.", "I~see.");
#else
                                                client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Spirit~is~80.~Please~train~harder.", "I~see.");
#endif

                                            }
                                        }
                                        else
                                        {
#if Arabic
                                                client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~you~practice~Thunder~to~level~4.", "I~see.");
#else
                                            client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~you~practice~Thunder~to~level~4.", "I~see.");
#endif

                                        }
                                    }

                                }
                                else
                                {
#if Arabic
                                      client.CreateDialog(stream, "You already have this spell.", "I~see.");
#else
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#endif

                                }
                                break;
                            }
                        case 725002://tornado.
                            {
                                if (client.Player.Level < 90)
                                {
#if Arabic
                                      client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~90.~Please~train~harder.", "I~see.");
#else
                                    client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~90.~Please~train~harder.", "I~see.");
#endif

                                    break;
                                }
                                if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Tornado))
                                {
                                    if (Database.AtributesStatus.IsFire(client.Player.Class))
                                    {
                                        if (client.MySpells.CheckSpell((ushort)Role.Flags.SpellID.Fire, 3))
                                        {
                                            if (client.Player.Spirit >= 160)
                                            {
                                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                                client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Tornado);
#if Arabic
                                                  client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.Tornado.ToString() + ".", "I~see.");
#else
                                                client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.Tornado.ToString() + ".", "I~see.");
#endif

                                            }
                                            else
                                            {
#if Arabic
                                                 client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Spirit~is~160.~Please~train~harder.", "I~see.");
#else
                                                client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Spirit~is~160.~Please~train~harder.", "I~see.");
#endif

                                            }
                                        }
                                        else
                                        {
#if Arabic
                                            client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~you~practice~Fire~to~level~3.", "I~see.");
#else
                                            client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~you~practice~Fire~to~level~3.", "I~see.");
#endif

                                        }
                                    }
                                    else
                                    {
#if Arabic
                                           client.CreateDialog(stream, "Sorry,this spell is just for Fire Taoist.", "I~see.");
#else
                                        client.CreateDialog(stream, "Sorry,this spell is just for Fire Taoist.", "I~see.");
#endif

                                    }
                                }
                                else
                                {
#if Arabic
                                     client.CreateDialog(stream, "You already have this spell.", "I~see.");
#else
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#endif

                                }
                                break;
                            }
                        case 725003://cure
                            {

                                if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Cure))
                                {

                                    if (client.Player.Spirit >= 30)
                                    {
                                        client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                        client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Cure);
#if Arabic
                                            client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.Cure.ToString() + ".", "I~see.");
#else
                                        client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.Cure.ToString() + ".", "I~see.");
#endif

                                    }
                                    else
                                    {
#if Arabic
                                               client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Spirit~is~30.~Please~train~harder.", "I~see.");
#else
                                        client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Spirit~is~30.~Please~train~harder.", "I~see.");
#endif

                                    }
                                }
                                else
                                {
#if Arabic
                                           client.CreateDialog(stream, "You already have this spell.", "I~see.");
#else
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#endif

                                }
                                break;
                            }
                        case 725015://divinehare
                            {
                                if (client.Player.Level < 54)
                                {
#if Arabic
                                    client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~54.~Please~train~harder.", "I~see.");
#else
                                    client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~54.~Please~train~harder.", "I~see.");
#endif

                                    break;
                                }
                                if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.DivineHare))
                                {

                                    if (Database.AtributesStatus.IsWater(client.Player.Class))
                                    {
                                        client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                        client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.DivineHare);
#if Arabic
                                        client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.DivineHare.ToString() + ".", "I~see.");
#else
                                        client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.DivineHare.ToString() + ".", "I~see.");
#endif

                                    }
                                    else
                                    {
#if Arabic
                                            client.CreateDialog(stream, "Sorry,this spell is just for Water Taoist.", "I~see.");
#else
                                        client.CreateDialog(stream, "Sorry,this spell is just for Water Taoist.", "I~see.");
#endif

                                    }
                                }
                                else
                                {
#if Arabic
                                     client.CreateDialog(stream, "You already have this spell.", "I~see.");
#else
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#endif

                                }
                                break;
                            }
                        case 725004://lighiting
                            {
                                if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Lightning))
                                {
                                    if (Database.AtributesStatus.IsTaoist(client.Player.Class))
                                    {
                                        if (client.Player.Spirit >= 25)
                                        {
                                            client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                            client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Lightning);
#if Arabic
                                                client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.Lightning.ToString() + ".", "I~see.");
#else
                                            client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.Lightning.ToString() + ".", "I~see.");
#endif

                                        }
                                        else
                                        {
#if Arabic
                                                client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Spirit~is~25.~Please~train~harder.", "I~see.");
#else
                                            client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Spirit~is~25.~Please~train~harder.", "I~see.");
#endif

                                        }
                                    }
                                    else
                                    {
#if Arabic
                                              client.CreateDialog(stream, "Sorry,this spell is just for Taoist`s.", "I~see.");
#else
                                        client.CreateDialog(stream, "Sorry,this spell is just for Taoist`s.", "I~see.");
#endif

                                    }
                                }
                                else
                                {
#if Arabic
                                     client.CreateDialog(stream, "You already have this spell.", "I~see.");
#else
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#endif

                                }
                                break;
                            }
                        case 725028://SpeedLightning
                            {
                                if (client.Player.Level < 70)
                                {
#if Arabic
                                    client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~70.~Please~train~harder.", "I~see.");
#else
                                    client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~70.~Please~train~harder.", "I~see.");
#endif

                                    break;
                                }
                                if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.SpeedLightning))
                                {
                                    if (Database.AtributesStatus.IsTaoist(client.Player.Class))
                                    {
                                        if (client.Player.Spirit >= 25)
                                        {
                                            client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                            client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.SpeedLightning);
#if Arabic
                                            client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.SpeedLightning.ToString() + ".", "I~see.");
#else
                                            client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.SpeedLightning.ToString() + ".", "I~see.");
#endif

                                        }
                                        else
                                        {
#if Arabic
                                             client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Spirit~is~25.~Please~train~harder.", "I~see.");
#else
                                            client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Spirit~is~25.~Please~train~harder.", "I~see.");
#endif

                                        }
                                    }
                                    else
                                    {
#if Arabic
                                             client.CreateDialog(stream, "Sorry,this spell is just for Taoist`s.", "I~see.");
#else
                                        client.CreateDialog(stream, "Sorry,this spell is just for Taoist`s.", "I~see.");
#endif

                                    }
                                }
                                else
                                {
#if Arabic
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#else
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#endif

                                }
                                break;
                            }
                        case 725025://fly moon
                            {
                                if (client.Player.Level < 40)
                                {
#if Arabic
                                       client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~40.~Please~train~harder.", "I~see.");
#else
                                    client.CreateDialog(stream, "Sorry,~you~cannot~learn~this~spell~before~your~Level~is~40.~Please~train~harder.", "I~see.");
#endif

                                    break;
                                }
                                if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.FlyingMoon))
                                {
                                    if (Database.AtributesStatus.IsWarrior(client.Player.Class))
                                    {
                                        client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                        client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.FlyingMoon);
#if Arabic
                                         client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.FlyingMoon.ToString() + ".", "I~see.");
#else
                                        client.CreateDialog(stream, "You~have~learned~" + Role.Flags.SpellID.FlyingMoon.ToString() + ".", "I~see.");
#endif

                                    }
                                    else
                                    {
#if Arabic
                                         client.CreateDialog(stream, "Sorry,this spell is just for Warrior`s.", "I~see.");
#else
                                        client.CreateDialog(stream, "Sorry, this spell is just for Warrior`s.", "I~see.");
#endif

                                    }
                                }
                                else
                                {
#if Arabic
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#else
                                    client.CreateDialog(stream, "You already have this spell.", "I~see.");
#endif

                                }
                                break;
                            }
                    }
                }
                else if (Database.ItemType.IsMoneyBag(DBItem.ID))
                {
                    #region MoneyBags
                    switch (item.ITEM_ID)
                    {
                        case 3005945:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.Money += 200000000;
                                client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);
#if Arabic
                                                               client.SendSysMesage("You received 200000000 silver for opening the HonorableMoneyBag's.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#else
                                client.SendSysMesage("You received 200000000 silver for opening the HonorableMoneyBag's.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#endif

                                break;
                            }


                        case 3008452:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.Money += 1000000;
                                client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);
#if Arabic
                                                               client.SendSysMesage("You received 1000000 silver for opening the MillionSilverBag's.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#else
                                client.SendSysMesage("You received 1000000 silver for opening the MillionSilverBag's.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#endif

                                break;
                            }
                        case 723713:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.Money += 300000;
                                client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);
#if Arabic
                                                                  client.SendSysMesage("You received 300000 silver for opening the MoneyBag's.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#else
                                client.SendSysMesage("You received 300000 silver for opening the MoneyBag's.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#endif

                                break;
                            }
                        case 723714:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.Money += 800000;
                                client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);
#if Arabic
                                              client.SendSysMesage("You received 800000 silver for opening the MoneyBag's.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#else
                                client.SendSysMesage("You received 800000 silver for opening the MoneyBag's.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#endif

                                break;
                            }
                        case 723715:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.Money += 1200000;
                                client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);
#if Arabic
                                                   client.SendSysMesage("You received 1200000 silver for opening the MoneyBag's.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#else
                                client.SendSysMesage("You received 1200000 silver for opening the MoneyBag's.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#endif

                                break;
                            }
                        case 723716:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.Money += 1800000;
                                client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);
#if Arabic
                                                client.SendSysMesage("You received 1800000 silver for opening the MoneyBag's.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#else
                                client.SendSysMesage("You received 1800000 silver for opening the MoneyBag's.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#endif

                                break;
                            }
                        case 723717:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.Money += 5000000;
                                client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);
#if Arabic

#else

#endif
                                client.SendSysMesage("You received 5000000 silver for opening the MoneyBag's.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
                                break;
                            }
                        case 723718:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.Money += 20000000;
                                client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);
#if Arabic
                                                  client.SendSysMesage("You received 20000000 silver for opening the MoneyBag's.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#else
                                client.SendSysMesage("You received 20000000 silver for opening the MoneyBag's.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#endif

                                break;
                            }
                        case 723719:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.Money += 25000000;
                                client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);
#if Arabic
                                               client.SendSysMesage("You received 25000000 silver for opening the MoneyBag's.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#else
                                client.SendSysMesage("You received 25000000 silver for opening the MoneyBag's.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#endif

                                break;
                            }
                        case 723720:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.Money += 80000000;
                                client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);
#if Arabic
                                              client.SendSysMesage("You received 80000000 silver for opening the MoneyBag's.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#else
                                client.SendSysMesage("You received 80000000 silver for opening the MoneyBag's.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#endif

                                break;
                            }
                        case 723721:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.Money += 100000000;
                                client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);
#if Arabic
                                             client.SendSysMesage("You received 100000000 silver for opening the MoneyBag's.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#else
                                client.SendSysMesage("You received 100000000 silver for opening the MoneyBag's.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#endif

                                break;
                            }
                        case 723722:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.Money += 300000000;
                                client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);
#if Arabic
                                             client.SendSysMesage("You received 300000000 silver for opening the MoneyBag's.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#else
                                client.SendSysMesage("You received 300000000 silver for opening the MoneyBag's.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#endif

                                break;
                            }
                        case 723723:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.Money += 500000000;
                                client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);
#if Arabic
                                             client.SendSysMesage("You received 500000000 silver for opening the MoneyBag's.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#else
                                client.SendSysMesage("You received 500000000 silver for opening the MoneyBag's.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#endif

                                break;
                            }
                    }
                    #endregion
                }
                /*  else if (item.ITEM_ID >= 3005107 && item.ITEM_ID <= 3005135 || item.ITEM_ID == 3001264 || item.ITEM_ID == 3003126
                      || item.ITEM_ID == 728919)
                  {

                      switch (item.ITEM_ID)
                      {
                          case 3005107:
                              {
                                  client.Player.ConquerPoints += 20;

                                  client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                  break;
                              }
                          case 3005108:
                              {
                                  client.Player.ConquerPoints += 30;

                                  client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                  break;
                              }
                          case 3005109:
                              {
                                  client.Player.ConquerPoints += 50;

                                  client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                  break;
                              }
                          case 3005110:
                              {
                                  client.Player.ConquerPoints += 80;

                                  client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                  break;
                              }
                          case 3005111:
                              {
                                  client.Player.ConquerPoints += 100;

                                  client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                  break;
                              }
                          case 3005112:
                              {
                                  client.Player.ConquerPoints += 200;

                                  client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                  break;
                              }
                          case 3005113:
                              {
                                  client.Player.ConquerPoints += 300;

                                  client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                  break;
                              }
                          case 3005114:
                              {
                                  client.Player.ConquerPoints += 500;

                                  client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                  break;
                              }
                          case 3005115:
                              {
                                  client.Player.ConquerPoints += 800;

                                  client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                  break;
                              }
                          case 3005116:
                              {
                                  client.Player.ConquerPoints += 1000;

                                  client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                  break;
                              }
                          case 3005117:
                              {
                                  client.Player.Money += 5000;
                                  client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);
                                  client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                  break;
                              }
                          case 3005118:
                              {
                                  client.Player.Money += 10000;
                                  client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);
                                  client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                  break;
                              }
                          case 3005119:
                              {
                                  client.Player.Money += 20000;
                                  client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);
                                  client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                  break;
                              }
                          case 3005120:
                              {
                                  client.Player.Money += 50000;
                                  client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);
                                  client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                  break;
                              }
                          case 3005121:
                              {
                                  client.Player.Money += 100000;
                                  client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);
                                  client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                  break;
                              }
                          case 3005122:
                              {
                                  client.Player.Money += 200000;
                                  client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);
                                  client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                  break;
                              }

                          case 3005130:
                              {
                                  uint[] Items = new uint[]
                                  {
                                      (uint)(700000 + (uint)Role.Flags.Gem.RefinedDragonGem)
                                      ,(uint)(700000 + (uint)Role.Flags.Gem.RefinedFuryGem)
                                      ,(uint)(700000 + (uint)Role.Flags.Gem.RefinedGloryGem)
                                      ,(uint)(700000 + (uint)Role.Flags.Gem.RefinedKylinGem)
                                      ,(uint)(700000 + (uint)Role.Flags.Gem.RefinedMoonGem)
                                      ,(uint)(700000 + (uint)Role.Flags.Gem.RefinedPhoenixGem)
                                      ,(uint)(700000 + (uint)Role.Flags.Gem.RefinedRainbowGem)
                                      ,(uint)(700000 + (uint)Role.Flags.Gem.RefinedThunderGem)
                                      ,(uint)(700000 + (uint)Role.Flags.Gem.RefinedTortoiseGem)
                                      ,(uint)(700000 + (uint)Role.Flags.Gem.RefinedVioletGem)
                                  };
                                  uint ItemID = Items[(uint)Program.GetRandom.Next(0, Items.Length)];
                                  client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                  client.Inventory.Add(stream, ItemID, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                  break;
                              }
                          case 3005131:
                              {
                                  uint[] Items = new uint[]
                                  {
                                    726083
                                      ,726078
                                      ,726080
                                      ,721179
                                      ,726076
                                      ,726082
                                      ,721183
                                      ,726108
                                      ,726107
                                      ,726087
                                      ,726096
                                      ,726095
                                      ,726072
                                      ,726064
                                      ,726073
                                      ,726097
                                     ,726098
                                     ,726086
                                     ,726104
                                     ,726100,726103,726086,726099,726106
                                  };

                                  uint ItemID = Items[(uint)Program.GetRandom.Next(0, Items.Length)];
                                  client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                  client.Inventory.Add(stream, ItemID, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                  break;
                              }
                          case 3005132:
                              {
                                  uint[] Items = new uint[]
                                  {
                                      (uint)(700000 + (uint)Role.Flags.Gem.SuperDragonGem)
                                      ,(uint)(700000 + (uint)Role.Flags.Gem.SuperFuryGem)
                                      ,(uint)(700000 + (uint)Role.Flags.Gem.SuperGloryGem)
                                      ,(uint)(700000 + (uint)Role.Flags.Gem.SuperKylinGem)
                                      ,(uint)(700000 + (uint)Role.Flags.Gem.SuperMoonGem)
                                      ,(uint)(700000 + (uint)Role.Flags.Gem.SuperPhoenixGem)
                                      ,(uint)(700000 + (uint)Role.Flags.Gem.SuperRainbowGem)
                                      ,(uint)(700000 + (uint)Role.Flags.Gem.SuperThunderGem)
                                      ,(uint)(700000 + (uint)Role.Flags.Gem.SuperTortoiseGem)
                                      ,(uint)(700000 + (uint)Role.Flags.Gem.SuperVioletGem)
                                  };
                                  uint ItemID = Items[(uint)Program.GetRandom.Next(0, Items.Length)];
                                  client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                  client.Inventory.Add(stream, ItemID, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                  break;
                              }
                          case 3005133:
                              {
                                  var PurificationArray = Database.ItemType.PurificationItems[4];
                                  var Position = Program.GetRandom.Next(0, PurificationArray.Values.Count);
                                  var ReceiveItem = PurificationArray.Values.ToArray()[Position];
                                  client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                  client.Inventory.Add(stream, ReceiveItem.ID);
                                  break;
                              }
                          case 3005134:
                              {
                                  if (client.Inventory.HaveSpace(1))
                                  {
                                      client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                      client.Inventory.Add(stream, 755099, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                      client.Inventory.Add(stream, 751099, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                  }
                                  else
                                  {
#if Arabic
                                         client.SendSysMesage("Please make one space in you bag.");
#else
                                      client.SendSysMesage("Please make one space in you bag.");
#endif

                                  }
                                  break;
                              }
                      }
                  }*/
                else //Other Items
                {
                    switch (item.ITEM_ID)
                    {
                        case 720374:
                            {
                                if (Program.FreePkMap.Contains(client.Player.Map))
                                {
                                    client.SendSysMesage("You can not use this feature in this map.");
                                    break;
                                }
                                client.ActiveNpc = 987977854;
                                Game.MsgNpc.Dialog dialog = new Game.MsgNpc.Dialog(client, stream);

                                if (client.Player.ExpireVip <= DateTime.Now)
                                {
                                    dialog.Text("Your VIP has expired. Please purchase VIP to continue.\n");
                                    dialog.AddOption("Purchase VIP", 4); // Opção para compra de VIP
                                }
                                else
                                {
                                    // Apenas mostra o tempo de VIP restante
                                    TimeSpan Time = client.Player.ExpireVip - DateTime.Now;
                                    dialog.Text(string.Format("VIP Time Left: {0} Days {1} Hours {2} Minutes {3} Seconds.", Time.Days, Time.Hours, Time.Minutes, Time.Seconds));

                                    // Mantém a opção de configurar itens de caça
                                    dialog.AddOption("Set Hunting Items.", 3);
                                }

                                dialog.AddAvatar(0);
                                dialog.FinalizeDialog();
                                break;
                            }
                        case Database.ItemType.MoonBox:
                            {
                                if (client.Inventory.HaveSpace(1))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);

                                    if (Role.Core.RateDouble(5))
                                        client.Inventory.Add(stream, 720027, 1);
                                    else if (Role.Core.RateDouble(0.1))
                                        client.Inventory.Add(stream, 723584, 1);
                                    else if (Role.Core.RateDouble(100))
                                        client.Inventory.Add(stream, Database.ItemType.Meteor, 1);
                                    else if (Role.Core.RateDouble(0.0001))
                                        client.Inventory.Add(stream, Database.ItemType.DragonBall, 1);

                                    client.SendSysMesage("You've successfully opened the MoonBox! Check your inventory!");

                                }
                                else
                                {
                                    client.SendSysMesage("Please clear room in your inventory! You need at least 3 spaces!");
                                }
                                break;
                            }
                        case 721169:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);

                                int rnd = Role.Core.Random.Next(0, 2);
                                var listGarments = new List<uint>()
                                {
                                    193115,193625,193565,192495,192435,192425,192345,192310,187735,192185,192165,192125,188965,188915,188675,188655,
                                    188575,188545,188495,188295,188285,188255,188265,188175,188165,184385,184345,183485,183475,183405,188155,183465,
                                    188155,187825,187775,187505,187475,187465,187325,187455,187575
                                };
                                if (rnd == 0)
                                    client.Inventory.Add(stream, listGarments[Role.Core.Random.Next(0, listGarments.Count)], 1, 0, 0, 0,
                                        Role.Flags.Gem.NoSocket,
                                        Role.Flags.Gem.NoSocket, item.Bound == 1 ? true : false);
                                break;
                            }
                        case 721168:
                            {
                                if (client.Inventory.HaveSpace(12))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.Add(stream, 721169, 12);
                                }
                                else
                                    client.SendSysMesage("You need 12 empty spaces");

                                break;
                            }

                        case 722178:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                SurpriseBox.GetReward(client, stream);
                                break;
                            }
                        case 720757:
                            {
                                if (client.Inventory.HaveSpace(3))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.AddItemWitchStack(722136, 0, 30, stream);
                                }
                                else
                                    client.SendSysMesage("You need 30 free space.");
                                break;
                            }
                        case 723583:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                switch (client.Player.Body)
                                {
                                    case 1003: client.Player.Body = 1004; break;
                                    case 1004: client.Player.Body = 1003; break;
                                    case 2001: client.Player.Body = 2002; break;
                                    case 2002: client.Player.Body = 2001; break;
                                }
                                break;
                            }
                        case 723753:
                            {
                                if (!client.Inventory.HaveSpace(30))
                                {
                                    client.SendSysMesage("Please make 30 more spaces in your inventory.");
                                    break;
                                }
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.AddItemWitchStack(722136, 0, 100, stream, true);
                                client.Inventory.Add(stream, 117089, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//earring
                                client.Inventory.Add(stream, 201009, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                client.Inventory.Add(stream, 202009, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);

                                client.Inventory.Add(stream, 1080001, 1);
                                client.Inventory.Add(stream, 1088001, 1);
                                client.Inventory.Add(stream, Database.ItemType.MoonBox, 1);
                                client.Inventory.Add(stream, 1072031, 5);


                                if (Database.AtributesStatus.IsTrojan(client.Player.Class))
                                {
                                    client.Inventory.Add(stream, 130089, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                    client.Inventory.Add(stream, 410209, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//blade
                                    client.Inventory.Add(stream, 480209, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                    client.Inventory.Add(stream, 150219, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//ring
                                    client.Inventory.Add(stream, 120189, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//neck
                                    client.Inventory.Add(stream, 160219, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//boots
                                }
                                else if (Database.AtributesStatus.IsWarrior(client.Player.Class))
                                {
                                    client.Inventory.Add(stream, 131089, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                    client.Inventory.Add(stream, 561209, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//wand
                                    client.Inventory.Add(stream, 150219, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//ring
                                    client.Inventory.Add(stream, 120189, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//neck
                                    client.Inventory.Add(stream, 160219, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//boots
                                }
                                else if (Database.AtributesStatus.IsArcher(client.Player.Class))
                                {
                                    client.Inventory.Add(stream, 133089, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//armor
                                    client.Inventory.Add(stream, 500209, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//bow
                                    client.Inventory.Add(stream, 150219, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//ring
                                    client.Inventory.Add(stream, 120189, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//neck
                                    client.Inventory.Add(stream, 160219, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//boots
                                }
                                else if (Database.AtributesStatus.IsNinja(client.Player.Class))
                                {
                                    client.Inventory.Add(stream, 135089, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                    client.Inventory.Add(stream, 601219, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                    client.Inventory.Add(stream, 601219, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                    client.Inventory.Add(stream, 150219, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//ring
                                    client.Inventory.Add(stream, 120189, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//neck
                                    client.Inventory.Add(stream, 160219, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//boots
                                }
                                else if (Database.AtributesStatus.IsMonk(client.Player.Class))
                                {
                                    client.Inventory.Add(stream, 136089, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                    client.Inventory.Add(stream, 610209, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                    client.Inventory.Add(stream, 610209, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                    client.Inventory.Add(stream, 150219, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//ring
                                    client.Inventory.Add(stream, 120189, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//neck
                                    client.Inventory.Add(stream, 160219, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//boots
                                }
                                else if (Database.AtributesStatus.IsTaoist(client.Player.Class))
                                {
                                    client.Inventory.Add(stream, 134089, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                    client.Inventory.Add(stream, 421219, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                    client.Inventory.Add(stream, 160219, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//boots
                                    client.Inventory.Add(stream, 121189, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//bag
                                    client.Inventory.Add(stream, 152209, 1, 6, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//bag
                                    //152066
                                }
                                client.CreateBoxDialog("Welcome to OrigensCO!");
                                break;
                            }
                        case 723584:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                MsgServer.MsgGameItem Coat;
                                if (client.Equipment.TryGetEquip(Role.Flags.ConquerItem.Armor, out Coat))
                                {
                                    Coat.Color = Role.Flags.Color.Black;
                                    Coat.Mode = Role.Flags.ItemMode.Update;
                                    Coat.Send(client, stream);
                                    client.Player.View.SendView(client.Player.GetArray(stream, false), false);
                                }
                                break;
                            }
                        #region itemtype.txt
                        case 720891:
                            {

                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 1088000, 3);
                                break;
                            }
                        case 720884:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 1088000, 5);
                                break;
                            }
                        case 729277:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 183475, 10);
                                break;
                            }
                        case 3000000:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 187405, 0, 0, 1);
                                client.Inventory.Add(stream, 729277);
                                break;
                            }
                        case 3000001:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 187415, 0, 0, 1);
                                client.Inventory.Add(stream, 729277);
                                break;
                            }
                        case 3000002:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 187425, 0, 0, 1);
                                client.Inventory.Add(stream, 729277);
                                break;
                            }
                        #endregion
                        case 3004280:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 721423, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Skill~C.Strike~Pack~and~received~1~Skill~C.Strike~(Elite)~Material~(B).", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 721424, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Skill~C.Strike~Pack~and~received~1~Skill~C.Strike~(Super)~Material~(B).", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004295, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Skill~C.Strike~Pack~and~received~1~Skill~C.Strike~(Sacred)~Material~(B).", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004279:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 721393, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Penetration~Pack~and~received~1~Penetration~(Elite)~Material~(B).", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 721394, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Penetration~Pack~and~received~1~Penetration~(Super)~Material~(B).", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004294, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Penetration~Pack~and~received~1~Penetration~(Sacred)~Material~(B).", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004278:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 721376, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Detoxication~Pack~and~received~1~Detoxication~(Elite)~Material~(B).", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 721377, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Detoxication~Pack~and~received~1~Detoxication~(Super)~Material~(B).", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004293, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Detoxication~Pack~and~received~1~Detoxication~(Sacred)~Material~(B).", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004277:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 721383, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Penetration~Pack~and~received~1~Penetration~(Elite)~Material~(B).", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 721384, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Penetration~Pack~and~received~1~Penetration~(Super)~Material~(B).", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004292, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Penetration~Pack~and~received~1~Penetration~(Sacred)~Material~(B).", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004276:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 721351, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Detoxication~Pack~and~received~1~Detoxication~(Elite)~Material~(B).", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 721352, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Detoxication~Pack~and~received~1~Detoxication~(Super)~Material~(B).", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004291, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Detoxication~Pack~and~received~1~Detoxication~(Sacred)~Material~(B).", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004275:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 721496, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Detoxication~Pack~and~received~1~Detoxication~(Elite)~Material~(B).", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 721497, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Detoxication~Pack~and~received~1~Detoxication~(Super)~Material~(B).", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004290, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Detoxication~Pack~and~received~1~Detoxication~(Sacred)~Material~(B).", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004274:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 721491, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Detoxication~Pack~and~received~1~Detoxication~(Elite)~Material~(B).", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 721492, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Detoxication~Pack~and~received~1~Detoxication~(Super)~Material~(B).", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004289, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Detoxication~Pack~and~received~1~Detoxication~(Sacred)~Material~(B).", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004273:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 721486, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Detoxication~Pack~and~received~1~Detoxication~(Elite)~Material~(B).", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 721487, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Detoxication~Pack~and~received~1~Detoxication~(Super)~Material~(B).", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004288, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Detoxication~Pack~and~received~1~Detoxication~(Sacred)~Material~(B).", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004272:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 721361, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Block~Pack~and~received~1~Block~(Elite)~Material~(B).", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 721362, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Block~Pack~and~received~1~Block~(Super)~Material~(B).", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004287, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Block~Pack~and~received~1~Block~(Sacred)~Material~(B).", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004271:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 721462, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Breakthrough~Pack~and~received~1~Breakthrough~(Elite)~Material~(B).", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 721463, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Breakthrough~Pack~and~received~1~Breakthrough~(Super)~Material~(B).", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004286, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Breakthrough~Pack~and~received~1~Breakthrough~(Sacred)~Material~(B).", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004270:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 721457, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Breakthrough~Pack~and~received~1~Breakthrough~(Elite)~Material~(B).", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 721458, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Breakthrough~Pack~and~received~1~Breakthrough~(Super)~Material~(B).", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004285, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Breakthrough~Pack~and~received~1~Breakthrough~(Sacred)~Material~(B).", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004269:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 721452, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Breakthrough~Pack~and~received~1~Breakthrough~(Elite)~Material~(B).", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 721453, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Breakthrough~Pack~and~received~1~Breakthrough~(Super)~Material~(B).", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004284, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Breakthrough~Pack~and~received~1~Breakthrough~(Sacred)~Material~(B).", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004268:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 721413, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Critical~Strike~Pack~and~received~1~Critical~Strike~(Elite)~Material~(B).", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 721414, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Critical~Strike~Pack~and~received~1~Critical~Strike~(Super)~Material~(B).", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004283, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Critical~Strike~Pack~and~received~1~Critical~Strike~(Sacred)~Material~(B).", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004267:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 721408, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Critical~Strike~Pack~and~received~1~Critical~Strike~(Elite)~Material~(B).", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 721409, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Critical~Strike~Pack~and~received~1~Critical~Strike~(Super)~Material~(B).", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004282, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Critical~Strike~Pack~and~received~1~Critical~Strike~(Sacred)~Material~(B).", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004266:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 721403, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Critical~Strike~Pack~and~received~1~Critical~Strike~(Elite)~Material~(B).", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 721404, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Critical~Strike~Pack~and~received~1~Critical~Strike~(Super)~Material~(B).", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004281, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Critical~Strike~Pack~and~received~1~Critical~Strike~(Sacred)~Material~(B).", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004226:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 725210, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~M-Defense~Pack~and~received~1~M-Defense~(Elite)~Material.", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 725211, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~M-Defense~Pack~and~received~1~M-Defense~(Super)~Material.", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004155, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~M-Defense~Pack~and~received~1~M-Defense~(Sacred)~Material.", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004225:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 725205, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~M-Defense~Pack~and~received~1~M-Defense~(Elite)~Material.", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 725206, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~M-Defense~Pack~and~received~1~M-Defense~(Super)~Material.", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004157, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~M-Defense~Pack~and~received~1~M-Defense~(Sacred)~Material.", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004224:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 725200, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~M-Defense~Pack~and~received~1~M-Defense~(Elite)~Material.", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 725201, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~M-Defense~Pack~and~received~1~M-Defense~(Super)~Material.", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004146, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~M-Defense~Pack~and~received~1~M-Defense~(Sacred)~Material.", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004223:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 725195, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~M-Defense~Pack~and~received~1~M-Defense~(Elite)~Material.", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 725196, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~M-Defense~Pack~and~received~1~M-Defense~(Super)~Material.", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004136, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~M-Defense~Pack~and~received~1~M-Defense~(Sacred)~Material.", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004222:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 724438, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Immunity~Pack~and~received~1~Immunity~(Elite)~Material.", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 724439, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Immunity~Pack~and~received~1~Immunity~(Super)~Material.", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004149, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Immunity~Pack~and~received~1~Immunity~(Sacred)~Material.", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004221:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 724371, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Counteraction~Pack~and~received~1~Counteraction~(Elite)~Material.", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 724372, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Counteraction~Pack~and~received~1~Counteraction~(Super)~Material.", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004151, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Counteraction~Pack~and~received~1~Counteraction~(Sacred)~Material.", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004220:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 724481, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Counteraction~Pack~and~received~1~Counteraction~(Elite)~Material.", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 724482, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Counteraction~Pack~and~received~1~Counteraction~(Super)~Material.", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004160, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Counteraction~Pack~and~received~1~Counteraction~(Sacred)~Material.", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004219:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 724366, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Breakthrough~Pack~and~received~1~Breakthrough~(Elite)~Material.", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 724367, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Breakthrough~Pack~and~received~1~Breakthrough~(Super)~Material.", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004164, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Breakthrough~Pack~and~received~1~Breakthrough~(Sacred)~Material.", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004218:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 724471, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Breakthrough~Pack~and~received~1~Breakthrough~(Elite)~Material.", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 724472, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Breakthrough~Pack~and~received~1~Breakthrough~(Super)~Material.", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004163, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Breakthrough~Pack~and~received~1~Breakthrough~(Sacred)~Material.", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004217:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 724428, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Skill~C.Strike~Pack~and~received~1~Skill~C.Strike~(Elite)~Material.", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 724429, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Skill~C.Strike~Pack~and~received~1~Skill~C.Strike~(Super)~Material.", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004148, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Skill~C.Strike~Pack~and~received~1~Skill~C.Strike~(Sacred)~Material.", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004216:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 724418, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Critical~Strike~Pack~and~received~1~Critical~Strike~(Elite)~Material.", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 724419, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Critical~Strike~Pack~and~received~1~Critical~Strike~(Super)~Material.", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004158, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Critical~Strike~Pack~and~received~1~Critical~Strike~(Sacred)~Material.", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004215:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 724388, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Penetration~Pack~and~received~1~Penetration~(Elite)~Material.", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 724389, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Penetration~Pack~and~received~1~Penetration~(Super)~Material.", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004154, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Penetration~Pack~and~received~1~Penetration~(Sacred)~Material.", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004214:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 724356, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Block~Pack~and~received~1~Block~(Elite)~Material.", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 724357, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Block~Pack~and~received~1~Block~(Super)~Material.", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004144, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Block~Pack~and~received~1~Block~(Sacred)~Material.", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004213:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 724476, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Counteraction~Pack~and~received~1~Counteraction~(Elite)~Material.", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 724477, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Counteraction~Pack~and~received~1~Counteraction~(Super)~Material.", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004142, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Counteraction~Pack~and~received~1~Counteraction~(Sacred)~Material.", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004212:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 724443, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Intensification~Pack~and~received~1~Intensification~(Elite)~Material.", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 724444, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Intensification~Pack~and~received~1~Intensification~(Super)~Material.", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004140, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Intensification~Pack~and~received~1~Intensification~(Sacred)~Material.", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004211:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 724433, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Immunity~Pack~and~received~1~Immunity~(Elite)~Material.", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 724434, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Immunity~Pack~and~received~1~Immunity~(Super)~Material.", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004139, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Immunity~Pack~and~received~1~Immunity~(Sacred)~Material.", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004210:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 724376, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Detoxication~Pack~and~received~1~Detoxication~(Elite)~Material.", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 724377, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Detoxication~Pack~and~received~1~Detoxication~(Super)~Material.", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004166, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Detoxication~Pack~and~received~1~Detoxication~(Sacred)~Material.", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004209:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 724383, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Penetration~Pack~and~received~1~Penetration~(Elite)~Material.", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 724384, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Penetration~Pack~and~received~1~Penetration~(Super)~Material.", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004145, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Penetration~Pack~and~received~1~Penetration~(Sacred)~Material.", MsgMessage.ChatMode.System);
                                    }
                                }

                                break;
                            }
                        case 3004208:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 724351, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Detoxication~Pack~and~received~1~Detoxication~(Elite)~Material.", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 724352, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Detoxication~Pack~and~received~1~Detoxication~(Super)~Material.", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004165, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Detoxication~Pack~and~received~1~Detoxication~(Sacred)~Material.", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004207:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 724496, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Detoxication~Pack~and~received~1~Detoxication~(Elite)~Material.", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 724497, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Detoxication~Pack~and~received~1~Detoxication~(Super)~Material.", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004152, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Detoxication~Pack~and~received~1~Detoxication~(Sacred)~Material.", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004206:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 724491, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Detoxication~Pack~and~received~1~Detoxication~(Elite)~Material.", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 724492, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Detoxication~Pack~and~received~1~Detoxication~(Super)~Material.", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004161, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Detoxication~Pack~and~received~1~Detoxication~(Sacred)~Material.", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004205:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 724486, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Detoxication~Pack~and~received~1~Detoxication~(Elite)~Material.", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 724487, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Detoxication~Pack~and~received~1~Detoxication~(Super)~Material.", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004143, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Detoxication~Pack~and~received~1~Detoxication~(Sacred)~Material.", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004204:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 724361, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Block~Pack~and~received~1~Block~(Elite)~Material.", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 724362, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Block~Pack~and~received~1~Block~(Super)~Material.", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004153, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Block~Pack~and~received~1~Block~(Sacred)~Material.", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004203:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 724462, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Breakthrough~Pack~and~received~1~Breakthrough~(Elite)~Material.", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 724463, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Breakthrough~Pack~and~received~1~Breakthrough~(Super)~Material.", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004150, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Breakthrough~Pack~and~received~1~Breakthrough~(Sacred)~Material.", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004202:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 724457, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Breakthrough~Pack~and~received~1~Breakthrough~(Elite)~Material.", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 724458, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Breakthrough~Pack~and~received~1~Breakthrough~(Super)~Material.", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004159, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Breakthrough~Pack~and~received~1~Breakthrough~(Sacred)~Material.", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004201:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 724452, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Breakthrough~Pack~and~received~1~Breakthrough~(Elite)~Material.", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 724453, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Breakthrough~Pack~and~received~1~Breakthrough~(Super)~Material.", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004141, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Breakthrough~Pack~and~received~1~Breakthrough~(Sacred)~Material.", MsgMessage.ChatMode.System);
                                    }
                                }

                                break;
                            }
                        case 3004200:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 724413, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Critical~Strike~Pack~and~received~1~Critical~Strike~(Elite)~Material.", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 724414, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Critical~Strike~Pack~and~received~1~Critical~Strike~(Super)~Material.", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004156, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Critical~Strike~Pack~and~received~1~Critical~Strike~(Sacred)~Material.", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004199:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 724408, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Critical~Strike~Pack~and~received~1~Critical~Strike~(Elite)~Material.", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 724409, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Critical~Strike~Pack~and~received~1~Critical~Strike~(Super)~Material.", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004137, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Critical~Strike~Pack~and~received~1~Critical~Strike~(Sacred)~Material.", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004198:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 724403, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Critical~Strike~Pack~and~received~1~Critical~Strike~(Elite)~Material.", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 724404, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Critical~Strike~Pack~and~received~1~Critical~Strike~(Super)~Material.", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004147, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Critical~Strike~Pack~and~received~1~Critical~Strike~(Sacred)~Material.", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                        case 3004197:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 724423, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("You~opened~the~Prime~Skill~C.Strike~Pack~and~received~1~Skill~C.Strike~(Elite)~Material.", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 724424, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Skill~C.Strike~Pack~and~received~1~Skill~C.Strike~(Super)~Material.", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 3004138, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                        client.SendSysMesage("You~opened~the~Prime~Skill~C.Strike~Pack~and~received~1~Skill~C.Strike~(Sacred)~Material.", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }




                        case 1060030:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.HairColor = 3;
                                client.Player.SendUpdate(stream, client.Player.Hair, MsgUpdate.DataType.HairStyle);
                                break;
                            }
                        case 1060040:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.HairColor = 9;
                                client.Player.SendUpdate(stream, client.Player.Hair, MsgUpdate.DataType.HairStyle);
                                break;
                            }
                        case 1060050:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.HairColor = 8;
                                client.Player.SendUpdate(stream, client.Player.Hair, MsgUpdate.DataType.HairStyle);
                                break;
                            }
                        case 1060060:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.HairColor = 7;
                                client.Player.SendUpdate(stream, client.Player.Hair, MsgUpdate.DataType.HairStyle);
                                break;
                            }
                        case 1060070:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.HairColor = 6;
                                client.Player.SendUpdate(stream, client.Player.Hair, MsgUpdate.DataType.HairStyle);
                                break;
                            }
                        case 1060080:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.HairColor = 5;
                                client.Player.SendUpdate(stream, client.Player.Hair, MsgUpdate.DataType.HairStyle);
                                break;
                            }
                        case 1060090:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.HairColor = 4;
                                client.Player.SendUpdate(stream, client.Player.Hair, MsgUpdate.DataType.HairStyle);
                                break;
                            }
                        case 725067:
                            {

                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.AddItemWitchStack(725065, 0, 5, stream);
                                break;
                            }
                        case 725068:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.AddItemWitchStack(725066, 0, 5, stream);
                                break;
                            }
                        case 723467:
                            {
                                client.ActiveNpc = (uint)Game.MsgNpc.NpcID.GoldPrizeToken;
                                Game.MsgNpc.NpcHandler.GoldPrizeToken(client, stream, 0, "", 0);
                                break;
                            }
                        case 723751:
                            {
                                if (item.Bound <= 0)
                                {
                                    item.Bound = 1;
                                }
                                switch (item.Bound)
                                {
                                    case 1:
                                        {
                                            if (!client.Inventory.HaveSpace(17))
                                            {
                                                client.SendSysMesage("Please make 17 spaces in your inventory!");
                                                break;
                                            }
                                            if (client.Player.Level >= 5)
                                            {
                                                item.Bound = 2;
                                                client.Inventory.Add(stream, Database.ItemType.ExpBall, 2, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                                client.Inventory.Add(stream, Database.ItemType.ExperiencePotion, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                                client.Inventory.Add(stream, 1000000, 10, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                                client.Inventory.Add(stream, 1072031, 5, 0, 0, 0, 0, 0, false);
                                            }
                                            else
                                            {
                                                client.SendSysMesage("You can't open this package before you reach Level 5!", MsgMessage.ChatMode.TopLeftSystem);
                                            }
                                            break;
                                        }
                                    case 2:
                                        {
                                            if (!client.Inventory.HaveSpace(2))
                                            {
                                                client.SendSysMesage("Please make 2 spaces in your inventory!");
                                                break;
                                            }
                                            if (client.Player.Level >= 10)
                                            {
                                                item.Bound = 3;
                                                // client.Inventory.Add(stream, Database.ItemType.Meteor, 5, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                                client.Inventory.Add(stream, Database.ItemType.ExperiencePotion, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                            }
                                            else
                                            {
                                                client.SendSysMesage("You can't open this package before you reach level 10!", MsgMessage.ChatMode.TopLeftSystem);
                                            }
                                            break;
                                        }
                                    case 3:
                                        {
                                            if (!client.Inventory.HaveSpace(8))
                                            {
                                                client.SendSysMesage("Please make 8 spaces in your inventory!");
                                                break;
                                            }
                                            if (client.Player.Level >= 15)
                                            {
                                                item.Bound = 4;
                                                client.Inventory.Add(stream, Database.ItemType.ExperiencePotion, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                                client.Inventory.Add(stream, 721626, 3, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                                client.Inventory.Add(stream, 721625, 3, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                            }
                                            else
                                            {
                                                client.SendSysMesage("You can't open this package before you reach level 15!", MsgMessage.ChatMode.TopLeftSystem);
                                            }
                                            break;
                                        }
                                    case 4:
                                        {
                                            if (!client.Inventory.HaveSpace(6))// wepons 
                                            {
                                                client.SendSysMesage("Please make 6 spaces in your inventory!");
                                                break;
                                            }
                                            if (client.Player.Level >= 20)
                                            {
                                                item.Bound = 5;
                                                if (Database.AtributesStatus.IsArcher(client.Player.Class))
                                                    client.Inventory.Add(stream, 500019, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                                if (Database.AtributesStatus.IsTrojan(client.Player.Class))
                                                    client.Inventory.Add(stream, 410039, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                                if (Database.AtributesStatus.IsWarrior(client.Player.Class))
                                                    client.Inventory.Add(stream, 561039, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                                if (Database.AtributesStatus.IsTaoist(client.Player.Class))
                                                    client.Inventory.Add(stream, 421029, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);

                                                client.Inventory.Add(stream, 1002000, 5, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                            }
                                            else
                                            {
                                                client.SendSysMesage("You can't open this package before you reach level 20!", MsgMessage.ChatMode.TopLeftSystem);
                                            }
                                            break;
                                        }
                                    case 5:
                                        {
                                            if (!client.Inventory.HaveSpace(2))// armor 
                                            {
                                                client.SendSysMesage("Please make 2 spaces in your inventory!");
                                                break;
                                            }
                                            if (client.Player.Level >= 30)
                                            {
                                                item.Bound = 6;
                                                if (Database.AtributesStatus.IsArcher(client.Player.Class))
                                                    client.Inventory.Add(stream, 133018, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                                if (Database.AtributesStatus.IsTrojan(client.Player.Class))
                                                    client.Inventory.Add(stream, 130018, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                                if (Database.AtributesStatus.IsWarrior(client.Player.Class))
                                                    client.Inventory.Add(stream, 131018, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                                if (Database.AtributesStatus.IsTaoist(client.Player.Class))
                                                    client.Inventory.Add(stream, 134018, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                            }
                                            else
                                            {
                                                client.SendSysMesage("You can't open this package before you reach level 30!", MsgMessage.ChatMode.TopLeftSystem);
                                            }
                                            break;
                                        }

                                    case 6:
                                        {
                                            if (!client.Inventory.HaveSpace(2))// rINGS 
                                            {
                                                client.SendSysMesage("Please make 2 spaces in your inventory!");
                                                break;
                                            }
                                            if (client.Player.Level >= 35)
                                            {
                                                item.Bound = 7;
                                                if (Database.AtributesStatus.IsArcher(client.Player.Class))
                                                    client.Inventory.Add(stream, 150058, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                                if (Database.AtributesStatus.IsTrojan(client.Player.Class))
                                                    client.Inventory.Add(stream, 150058, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                                if (Database.AtributesStatus.IsWarrior(client.Player.Class))
                                                    client.Inventory.Add(stream, 150058, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                                if (Database.AtributesStatus.IsTaoist(client.Player.Class))
                                                    client.Inventory.Add(stream, 152048, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                            }
                                            else
                                            {
                                                client.SendSysMesage("You can't open this package before you reach level 35!", MsgMessage.ChatMode.TopLeftSystem);
                                            }
                                            break;
                                        }
                                    case 7:
                                        {
                                            if (!client.Inventory.HaveSpace(2))// Head 
                                            {
                                                client.SendSysMesage("Please make 2 spaces in your inventory!");
                                                break;
                                            }
                                            if (client.Player.Level >= 40)
                                            {
                                                item.Bound = 8;
                                                if (Database.AtributesStatus.IsArcher(client.Player.Class))
                                                {
                                                    client.Inventory.Add(stream, 113018, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                                    client.Inventory.Add(stream, 120068, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                                }
                                                if (Database.AtributesStatus.IsTrojan(client.Player.Class))
                                                {
                                                    client.Inventory.Add(stream, 118038, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                                    client.Inventory.Add(stream, 120068, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);

                                                }
                                                if (Database.AtributesStatus.IsWarrior(client.Player.Class))
                                                {
                                                    client.Inventory.Add(stream, 111038, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                                    client.Inventory.Add(stream, 120068, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);

                                                }
                                                if (Database.AtributesStatus.IsTaoist(client.Player.Class))
                                                {
                                                    client.Inventory.Add(stream, 114038, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                                    client.Inventory.Add(stream, 121068, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                                }
                                                client.Inventory.Add(stream, 160078, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                            }
                                            else
                                            {
                                                client.SendSysMesage("You can't open this package before you reach level 40!", MsgMessage.ChatMode.TopLeftSystem);
                                            }
                                            break;
                                        }
                                    case 8:
                                        {
                                            if (!client.Inventory.HaveSpace(2))// Nec and boot 
                                            {
                                                client.SendSysMesage("Please make 2 spaces in your inventory!");
                                                break;
                                            }
                                            if (client.Player.Level >= 50)
                                            {
                                                item.Bound = 9;
                                                client.Inventory.Add(stream, Database.ItemType.ExperiencePotion, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                            }
                                            else
                                            {
                                                client.SendSysMesage("You can't open this package before you reach level 50!", MsgMessage.ChatMode.TopLeftSystem);
                                            }
                                            break;
                                        }
                                    case 9:
                                        {
                                            if (!client.Inventory.HaveSpace(4))
                                            {
                                                client.SendSysMesage("Please make 2 spaces in your inventory!");
                                                break;
                                            }
                                            if (client.Player.Level >= 70)
                                            {
                                                item.Bound = 10;
                                                client.Inventory.Add(stream, Database.ItemType.ExperiencePotion, 2, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                            }
                                            else
                                            {
                                                client.SendSysMesage("You can't open this package before you reach level 70!", MsgMessage.ChatMode.TopLeftSystem);
                                            }
                                            break;
                                        }

                                }
                                if (item.Bound < 10)
                                {
                                    item.Mode = Role.Flags.ItemMode.Update;
                                    item.Send(client, stream);
                                }
                                else
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                }

                                break;
                            }
                        case 723912:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (client.Player.Level < 130)
                                    client.GainExpBall(200, true, Role.Flags.ExperienceEffect.angelwing);
                                break;
                            }
                        case 1060020:
                            {
                                if (client.Player.ContainFlag(MsgUpdate.Flags.FlashingName) || client.Player.ContainFlag(MsgUpdate.Flags.BlackName))
                                    break;
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (!Program.BlockTeleportMap.Contains(client.Player.Map) || Program.ArenaMaps.ContainsValue(client.Player.DynamicID))
                                    client.Teleport(428, 378, 1002);

                                break;
                            }
                        case 1060021:
                            {
                                if (client.Player.ContainFlag(MsgUpdate.Flags.FlashingName) || client.Player.ContainFlag(MsgUpdate.Flags.BlackName))
                                    break;
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (!Program.BlockTeleportMap.Contains(client.Player.Map) || Program.ArenaMaps.ContainsValue(client.Player.DynamicID))
                                    client.Teleport(500, 650, 1000);

                                break;
                            }
                        case 1060022:
                            {
                                if (client.Player.ContainFlag(MsgUpdate.Flags.FlashingName) || client.Player.ContainFlag(MsgUpdate.Flags.BlackName))
                                    break;
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (!Program.BlockTeleportMap.Contains(client.Player.Map) || Program.ArenaMaps.ContainsValue(client.Player.DynamicID))
                                    client.Teleport(565, 562, 1020);

                                break;
                            }
                        case 1060023:
                            {
                                if (client.Player.ContainFlag(MsgUpdate.Flags.FlashingName) || client.Player.ContainFlag(MsgUpdate.Flags.BlackName))
                                    break;
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (!Program.BlockTeleportMap.Contains(client.Player.Map) || Program.ArenaMaps.ContainsValue(client.Player.DynamicID))
                                    client.Teleport(188, 264, 1011);
                                break;
                            }
                        case 1060024:
                            {
                                if (client.Player.ContainFlag(MsgUpdate.Flags.FlashingName) || client.Player.ContainFlag(MsgUpdate.Flags.BlackName))
                                    break;
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (!Program.BlockTeleportMap.Contains(client.Player.Map) || Program.ArenaMaps.ContainsValue(client.Player.DynamicID))
                                    client.Teleport(717, 571, 1015);
                                break;
                            }
                        case 1060039:
                            {
                                if (client.Player.ContainFlag(MsgUpdate.Flags.FlashingName) || client.Player.ContainFlag(MsgUpdate.Flags.BlackName))
                                    break;
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (!Program.BlockTeleportMap.Contains(client.Player.Map) || Program.ArenaMaps.ContainsValue(client.Player.DynamicID))
                                    client.Teleport(535, 558, 1075);
                                break;
                            }
                        case 1060025:
                            {
                                if (client.Player.ContainFlag(MsgUpdate.Flags.FlashingName) || client.Player.ContainFlag(MsgUpdate.Flags.BlackName))
                                    break;
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (!Program.BlockTeleportMap.Contains(client.Player.Map) || Program.ArenaMaps.ContainsValue(client.Player.DynamicID))
                                    client.Teleport(428, 378, 1002);

                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (!Program.BlockTeleportMap.Contains(client.Player.Map) || Program.ArenaMaps.ContainsValue(client.Player.DynamicID))
                                    client.Teleport(405, 655, 1002);
                                break;
                            }
                        case 1060026:
                            {
                                if (client.Player.ContainFlag(MsgUpdate.Flags.FlashingName) || client.Player.ContainFlag(MsgUpdate.Flags.BlackName))
                                    break;

                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (!Program.BlockTeleportMap.Contains(client.Player.Map) || Program.ArenaMaps.ContainsValue(client.Player.DynamicID))
                                    client.Teleport(65, 252, 1002);
                                break;
                            }
                        case 1060027:
                            {
                                if (client.Player.ContainFlag(MsgUpdate.Flags.FlashingName) || client.Player.ContainFlag(MsgUpdate.Flags.BlackName))
                                    break;
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (!Program.BlockTeleportMap.Contains(client.Player.Map) || Program.ArenaMaps.ContainsValue(client.Player.DynamicID))
                                    client.Teleport(443, 372, 1002);
                                break;
                            }
                        case 1060028:
                            {
                                if (client.Player.ContainFlag(MsgUpdate.Flags.FlashingName) || client.Player.ContainFlag(MsgUpdate.Flags.BlackName))
                                    break;
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (!Program.BlockTeleportMap.Contains(client.Player.Map) || Program.ArenaMaps.ContainsValue(client.Player.DynamicID))
                                    client.Teleport(537, 767, 1011);
                                break;
                            }
                        case 1060029:
                            {
                                if (client.Player.ContainFlag(MsgUpdate.Flags.FlashingName) || client.Player.ContainFlag(MsgUpdate.Flags.BlackName))
                                    break;
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (!Program.BlockTeleportMap.Contains(client.Player.Map) || Program.ArenaMaps.ContainsValue(client.Player.DynamicID))
                                    client.Teleport(734, 448, 1011);
                                break;
                            }
                        case 1060038:
                            {
                                if (client.Player.ContainFlag(MsgUpdate.Flags.FlashingName) || client.Player.ContainFlag(MsgUpdate.Flags.BlackName))
                                    break;
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (!Program.BlockTeleportMap.Contains(client.Player.Map) || Program.ArenaMaps.ContainsValue(client.Player.DynamicID))
                                    client.Teleport(64, 426, 1011);
                                break;
                            }
                        case 1060031:
                            {
                                if (client.Player.ContainFlag(MsgUpdate.Flags.FlashingName) || client.Player.ContainFlag(MsgUpdate.Flags.BlackName))
                                    break;
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (!Program.BlockTeleportMap.Contains(client.Player.Map) || Program.ArenaMaps.ContainsValue(client.Player.DynamicID))
                                    client.Teleport(819, 602, 1020);
                                break;
                            }
                        case 1060032:
                            {
                                if (client.Player.ContainFlag(MsgUpdate.Flags.FlashingName) || client.Player.ContainFlag(MsgUpdate.Flags.BlackName))
                                    break;
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (!Program.BlockTeleportMap.Contains(client.Player.Map) || Program.ArenaMaps.ContainsValue(client.Player.DynamicID))
                                    client.Teleport(492, 726, 1020);
                                break;
                            }
                        case 1060033:
                            {
                                if (client.Player.ContainFlag(MsgUpdate.Flags.FlashingName) || client.Player.ContainFlag(MsgUpdate.Flags.BlackName))
                                    break;
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (!Program.BlockTeleportMap.Contains(client.Player.Map) || Program.ArenaMaps.ContainsValue(client.Player.DynamicID))
                                    client.Teleport(105, 396, 1020);
                                break;
                            }
                        case 1060034:
                            {
                                if (client.Player.ContainFlag(MsgUpdate.Flags.FlashingName) || client.Player.ContainFlag(MsgUpdate.Flags.BlackName))
                                    break;
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (!Program.BlockTeleportMap.Contains(client.Player.Map) || Program.ArenaMaps.ContainsValue(client.Player.DynamicID))
                                    client.Teleport(226, 201, 1000);
                                break;
                            }
                        case 1060035:
                            {
                                if (client.Player.ContainFlag(MsgUpdate.Flags.FlashingName) || client.Player.ContainFlag(MsgUpdate.Flags.BlackName))
                                    break;
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (!Program.BlockTeleportMap.Contains(client.Player.Map) || Program.ArenaMaps.ContainsValue(client.Player.DynamicID))
                                    client.Teleport(797, 547, 1000);
                                break;
                            }
                        case 1060037:
                            {
                                if (client.Player.ContainFlag(MsgUpdate.Flags.FlashingName) || client.Player.ContainFlag(MsgUpdate.Flags.BlackName))
                                    break;
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (!Program.BlockTeleportMap.Contains(client.Player.Map) || Program.ArenaMaps.ContainsValue(client.Player.DynamicID))
                                    client.Teleport(476, 365, 1001);
                                break;
                            }
                        case 3001318:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.Money += 500000;
                                client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);
#if Arabic
                                  client.SendSysMesage("You received 500,000 silver for opening the GloryMoneyBag's.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#else
                                client.SendSysMesage("You received 500,000 silver for opening the GloryMoneyBag's.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#endif

                                break;
                            }
                        case 780000:
                            {
                                if (DateTime.Now > client.Player.ExpireVip)
                                {
                                    client.Player.ExpireVip = DateTime.Now;
                                    client.Player.ExpireVip = client.Player.ExpireVip.AddDays(3);
                                }
                                else client.Player.ExpireVip = client.Player.ExpireVip.AddDays(3);
                                client.Player.VipLevel = 6;
                                client.Player.SendUpdate(stream, client.Player.VipLevel, MsgUpdate.DataType.VIPLevel);
                                client.Player.UpdateVip(stream);
                                client.SendSysMesage("Vip 3 Days Actived.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.yellow);
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                break;
                            }
                        case 780010:
                            {
                                if (DateTime.Now > client.Player.ExpireVip)
                                {
                                    client.Player.ExpireVip = DateTime.Now;
                                    client.Player.ExpireVip = client.Player.ExpireVip.AddDays(30);
                                }
                                else client.Player.ExpireVip = client.Player.ExpireVip.AddDays(30);
                                client.Player.VipLevel = 6;
                                client.Player.SendUpdate(stream, client.Player.VipLevel, MsgUpdate.DataType.VIPLevel);
                                client.Player.UpdateVip(stream);
                                Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Player: " + client.Player.Name + " - claimed VIP 30 Days #16#16", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                break;
                            }

                        case 780020:
                            {
                                // Verifica se o jogador já é VIP 6
                                if (client.Player.VipLevel == 6)
                                {
                                    client.SendSysMesage("You already have VIP 6, which is higher than VIPMining (VIP 4)!");
                                    break; // Sai do case sem aplicar o VIP 4
                                }

                                // Se não for VIP 6, prossegue com a lógica de adicionar o VIP 4
                                if (DateTime.Now > client.Player.ExpireVip)
                                {
                                    client.Player.ExpireVip = DateTime.Now;
                                    client.Player.ExpireVip = client.Player.ExpireVip.AddDays(30);
                                }
                                else
                                {
                                    client.Player.ExpireVip = client.Player.ExpireVip.AddDays(30);
                                }

                                client.Player.VipLevel = 6;
                                client.Player.SendUpdate(stream, client.Player.VipLevel, MsgUpdate.DataType.VIPLevel);
                                client.Player.UpdateVip(stream);
                                client.SendSysMesage("Congratulations! You've received VIPMining (30 Days)!");
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                break;
                            }

                        case 3001322:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.Money += 25000000;
                                client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);
#if Arabic
                                   client.SendSysMesage("You received 25,000,000 silver for opening the SplendidMoneyBox's.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#else
                                client.SendSysMesage("You received 25,000,000 silver for opening the SplendidMoneyBox's.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#endif

                                break;
                            }
                        //case 721117://change to new item
                        //    {
                        //        client.ActiveNpc = (uint)Game.MsgNpc.NpcID.VIPBook;
                        //        Game.MsgNpc.NpcHandler.VIPBook(client, stream, 0, "", 0);
                        //        break;
                        //    }
                        case 721540://AncestorBox
                            {
                                if (client.Inventory.HaveSpace(4))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    switch (Program.GetRandom.Next(0, 7))
                                    {
                                        case 0:
                                            {
                                                client.Inventory.Add(stream, Database.ItemType.MeteorScroll, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                                break;
                                            }
                                        case 1:
                                            {
                                                client.Inventory.Add(stream, Database.ItemType.PowerExpBall, 2, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                                break;
                                            }
                                        case 2:
                                            {
                                                client.Inventory.Add(stream, Database.ItemType.MoonBox, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                                break;
                                            }
                                        case 3:
                                            {
                                                client.Inventory.Add(stream, Database.ItemType.LotteryTick, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                                break;
                                            }
                                        case 4:
                                            {
                                                client.Inventory.Add(stream, Database.ItemType.DragonBall, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                                break;
                                            }
                                        case 5:
                                            {
                                                client.Inventory.Add(stream, Database.ItemType.CleanWater, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                                break;
                                            }
                                        case 6:
                                            {
                                                client.Inventory.Add(stream, Database.ItemType.CelestialStone, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                                break;
                                            }
                                    }
                                }
                                else
                                    client.SendSysMesage("Please prepare at least four slots in your inventory.");

                                break;
                            }
                        case 720598://dragonpill
                            {
                                if (DateTime.Now < client.Player.LastDragonPill.AddHours(24))
                                {
                                    client.CreateDialog(stream, "You`re already used DragonPill once today. You can use it next time on " + client.Player.LastDragonPill.AddHours(24)
                                        , "I~see");
                                    return;
                                }
                                if (client.Player.Map == 2056)
                                {
                                    if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 322, 331) <= 18)
                                    {
                                        if (!client.Map.ContainMobID(20060))
                                        {
                                            client.Player.LastDragonPill = DateTime.Now;
                                            client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                            Database.Server.AddMapMonster(stream, client.Map, 20060, client.Player.X, client.Player.Y, 1, 1, 1, client.Player.DynamicID, true, MsgFloorItem.MsgItemPacket.EffectMonsters.EarthquakeAndNight);
                                        }
                                        else
                                        {
#if Arabic
                                             client.CreateDialog(stream, "Sorry,but the monster is already spawned..", "I~see.");
#else
                                            client.CreateDialog(stream, "Sorry,but the monster is already spawned..", "I~see.");
#endif

                                        }
                                    }
                                    else
                                    {
#if Arabic
                                                  client.CreateDialog(stream, "The Dragon Pill is used to summon the Terato Dragon. You can use it in the Frozen Grotto Floor 6 (532,394) or the Wind Plain (517,742).", "I~see.");
                                 
#else
                                        client.CreateDialog(stream, "The Dragon Pill is used to summon the Terato Dragon. You can use it in the Frozen Grotto Floor 6 (322,331).", "I~see.");

#endif
                                    }
                                }
                                else client.CreateDialog(stream, "The Dragon Pill is used to summon the Terato Dragon. You can use it in the Frozen Grotto Floor 6 (322,331).", "I~see.");

                                //else if (client.Player.Map == 1002)
                                //{
                                //    if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 566, 793) <= 18)
                                //    {
                                //        if (!client.Map.ContainMobID(20060))
                                //        {
                                //            client.Player.LastDragonPill = DateTime.Now;
                                //            client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                //            Database.Server.AddMapMonster(stream, client.Map, 20060, client.Player.X, client.Player.Y, 1, 1, 1, client.Player.DynamicID, true, MsgFloorItem.MsgItemPacket.EffectMonsters.EarthquakeAndNight);
                                //        }
                                //        else
                                //        {
                                //            client.CreateDialog(stream, "Sorry,but the monster is already spawned..", "I~see.");
                                //        }
                                //    }
                                //    else
                                //    {
                                //        client.CreateDialog(stream, "The Dragon Pill is used to summon the Terato Dragon. You can use it in the Frozen Grotto Floor 6 (322,331) or the Wind Plain (566, 793).", "I~see.");

                                //    }
                                //}
                                break;
                            }
                        case 720599://shadow jade  fg5
                            {
                                if (client.Player.Map == 2055 || client.Player.Map == 2056)
                                {

#if Arabic
                                     client.Player.MessageBox("Do you want to speak with the F5 Grotto Miner now?", new Action<Client.GameClient>(p =>
                                        {
                                            p.Teleport(540, 573, 2055);
                                            p.CreateBoxDialog("You were send to the F5 Grotto Miner.");
                                        }
                                        ), null, 0);
#else
                                    client.Player.MessageBox("Do you want to speak with the F5 Grotto Miner now?", new Action<Client.GameClient>(p =>
                                    {
                                        p.Teleport(540, 573, 2055);
                                        p.CreateBoxDialog("You were send to the F5 Grotto Miner.");
                                    }
                                       ), null, 0);
#endif

                                }
                                else
                                {
#if Arabic
                                     client.CreateBoxDialog("Sorry,  use that in F5 AND F6.");
#else
                                    client.CreateBoxDialog("Sorry,  use that in F5 AND F6.");
#endif

                                }
                                break;
                            }
                        case 720842:
                            {
                                if (client.Player.Level >= 99)
                                {
                                    if (client.MyHouse != null && client.Player.DynamicID == client.Player.UID)
                                    {
                                        if (!client.Map.ContainMobID(6643, client.Player.UID))
                                        {
                                            client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                            Database.Server.AddMapMonster(stream, client.Map, 6643, client.Player.X, client.Player.Y, 1, 1, 1, client.Player.DynamicID, true, MsgFloorItem.MsgItemPacket.EffectMonsters.EarthquakeAndNight);
                                        }
                                        else
                                        {
#if Arabic
                                            client.CreateDialog(stream, "Sorry,but the monster is already spawned..", "I~see.");
#else
                                            client.CreateDialog(stream, "Sorry,but the monster is already spawned..", "I~see.");
#endif

                                        }
                                    }
                                    else
                                    {
#if Arabic
                                          client.CreateDialog(stream, "You have to be in your own house to be able to display it", "I~see.");
#else
                                        client.CreateDialog(stream, "You have to be in your own house to be able to display it", "I~see.");
#endif

                                    }
                                }
                                else
                                {
#if Arabic
                                      client.CreateDialog(stream, "Sorry,~you~cannot~open~this~monster~before~your~Level~is~99.~Please~train~harder.", "I~see.");
#else
                                    client.CreateDialog(stream, "Sorry,~you~cannot~open~this~monster~before~your~Level~is~99.~Please~train~harder.", "I~see.");
#endif

                                }
                                break;
                            }
                        case 720030://firework
                            {
                                if (client.Player.Stamina >= 100)
                                {
                                    //client.Player.SendString(MsgStringPacket.StringID.Fireworks, true, new string[1] { "1123" });

                                    client.Player.SendString(stream, MsgStringPacket.StringID.Effect, true, "firework-like");
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Player.Stamina -= 100;
                                    client.Player.SendUpdate(stream, client.Player.Stamina, MsgUpdate.DataType.Stamina);
                                }
                                break;
                            }
                        case 720031://endleslove
                            {
                                if (client.Player.Stamina >= 100)
                                {
                                    client.Player.SendString(stream, MsgStringPacket.StringID.Effect, true, "firework-1love");
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Player.Stamina -= 100;
                                    client.Player.SendUpdate(stream, client.Player.Stamina, MsgUpdate.DataType.Stamina);
                                }
                                break;
                            }
                        case 720032://mywish
                            {
                                if (client.Player.Stamina >= 100)
                                {
                                    client.Player.SendString(stream, MsgStringPacket.StringID.Effect, true, "firework-2love");
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Player.Stamina -= 100;
                                    client.Player.SendUpdate(stream, client.Player.Stamina, MsgUpdate.DataType.Stamina);
                                }
                                break;
                            }
                        case 720021:
                            {
                                client.UseItem = item.ITEM_ID;

                                NpcServerQuery Furniture = new NpcServerQuery();
                                Furniture.NpcType = Role.Flags.NpcType.Talker;
                                Furniture.Action = MsgNpc.NpcServerReplay.Mode.Cursor;

                                var conductor = MsgTournaments.MsgSchedules.GuildWar.GuildConductors[MsgNpc.NpcID.TeleGuild1];
                                Furniture.Mesh = (ushort)(conductor.Npc.Mesh != 0 ? conductor.Npc.Mesh : 1457);
                                Furniture.ID = MsgNpc.NpcID.TeleGuild1;

                                client.MoveNpcMesh = Furniture.Mesh;
                                client.MoveNpcUID = (uint)Furniture.ID;
                                client.Send(stream.NpcServerCreate(Furniture));
                                break;
                            }
                        case 720022:
                            {
                                client.UseItem = item.ITEM_ID;

                                NpcServerQuery Furniture = new NpcServerQuery();
                                Furniture.NpcType = Role.Flags.NpcType.Talker;
                                Furniture.Action = MsgNpc.NpcServerReplay.Mode.Cursor;

                                var conductor = MsgTournaments.MsgSchedules.GuildWar.GuildConductors[MsgNpc.NpcID.TeleGuild2];
                                Furniture.Mesh = (ushort)(conductor.Npc.Mesh != 0 ? conductor.Npc.Mesh : 1467);
                                Furniture.ID = MsgNpc.NpcID.TeleGuild2;

                                client.MoveNpcMesh = Furniture.Mesh;
                                client.MoveNpcUID = (uint)Furniture.ID;
                                client.Send(stream.NpcServerCreate(Furniture));
                                break;
                            }
                        case 720023:
                            {
                                client.UseItem = item.ITEM_ID;

                                NpcServerQuery Furniture = new NpcServerQuery();
                                Furniture.NpcType = Role.Flags.NpcType.Talker;
                                Furniture.Action = MsgNpc.NpcServerReplay.Mode.Cursor;

                                var conductor = MsgTournaments.MsgSchedules.GuildWar.GuildConductors[MsgNpc.NpcID.TeleGuild3];
                                Furniture.Mesh = (ushort)(conductor.Npc.Mesh != 0 ? conductor.Npc.Mesh : 1477);
                                Furniture.ID = MsgNpc.NpcID.TeleGuild3;

                                client.MoveNpcMesh = Furniture.Mesh;
                                client.MoveNpcUID = (uint)Furniture.ID;
                                client.Send(stream.NpcServerCreate(Furniture));
                                break;
                            }
                        case 720024:
                            {
                                client.UseItem = item.ITEM_ID;

                                NpcServerQuery Furniture = new NpcServerQuery();
                                Furniture.NpcType = Role.Flags.NpcType.Talker;
                                Furniture.Action = MsgNpc.NpcServerReplay.Mode.Cursor;

                                var conductor = MsgTournaments.MsgSchedules.GuildWar.GuildConductors[MsgNpc.NpcID.TeleGuild4];
                                Furniture.Mesh = (ushort)(conductor.Npc.Mesh != 0 ? conductor.Npc.Mesh : 1487);
                                Furniture.ID = MsgNpc.NpcID.TeleGuild4;

                                client.MoveNpcMesh = Furniture.Mesh;
                                client.MoveNpcUID = (uint)Furniture.ID;
                                client.Send(stream.NpcServerCreate(Furniture));
                                break;
                            }
                        case 721261://bomb
                            {
                                if (client.Player.Map == 1038)
                                {
                                    if (client.Player.View.Contains(MsgTournaments.MsgSchedules.GuildWar.Furnitures[Role.SobNpc.StaticMesh.LeftGate]))
                                    {
                                        client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                        MsgTournaments.MsgSchedules.GuildWar.Bomb(stream, client, Role.SobNpc.StaticMesh.LeftGate);
                                        break;
                                    }
                                    if (client.Player.View.Contains(MsgTournaments.MsgSchedules.GuildWar.Furnitures[Role.SobNpc.StaticMesh.RightGate]))
                                    {
                                        client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                        MsgTournaments.MsgSchedules.GuildWar.Bomb(stream, client, Role.SobNpc.StaticMesh.RightGate);
                                        break;
                                    }
                                }
                                else
                                {
#if Arabic
                                        client.SendSysMesage("Use this on gate in Guild Arena");
#else
                                    client.SendSysMesage("Use this on gate in Guild Arena");
#endif

                                }
                                break;
                            }
                        case 720020://statue scroll
                            {
                                client.UseItem = item.ITEM_ID;


                                NpcServerQuery npc = new NpcServerQuery();
                                npc.ID = (MsgNpc.NpcID)3;
                                npc.Mesh = 1130;
                                npc.Action = MsgNpc.NpcServerReplay.Mode.Cursor;
                                npc.NpcType = (Role.Flags.NpcType)9;
                                client.Send(stream.NpcServerCreate(npc));

                                break;
                            }
                        case 720027://MeteorScroll
                            {
                                if (client.Inventory.HaveSpace(9))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    if (item.Bound == 1)
                                    {
                                        client.Inventory.Add(stream, 1088001, 10, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                    }
                                    else
                                        client.Inventory.Add(stream, 1088001, 10, 0);
                                }
                                else
                                {
                                    client.SendSysMesage("Please make 10 more spaces in your inventory.");
                                    client.SendSysMesage("Please make 10 more spaces in your inventory.");
                                }
                                break;
                            }

                        case 720028://dragonballscroll
                            {
                                if (client.Inventory.HaveSpace(9))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.Add(stream, 1088000, 10, 0);
                                }
                                else
                                {
                                    client.SendSysMesage("Please make 9 more spaces in your inventory.");
                                    client.SendSysMesage("Please make 9 more spaces in your inventory.");
                                }
                                break;
                            }

                        case 720372:
                            {
                                if (client.Inventory.HaveSpace(10))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.Add(stream, 722725, 10, 0);
                                }
                                else
                                {
                                    client.SendSysMesage("Please make 10 more spaces in your inventory.");
                                }
                                break;
                            }  // SoulAromaBag
                        case 720373:
                            {
                                if (client.Inventory.HaveSpace(10))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.Add(stream, 722724, 10, 0);
                                }
                                else
                                {
                                    client.SendSysMesage("Please make 4 more spaces in your inventory.");
                                }
                                break;
                            }  // DreamGrassBag
                        case 720375:
                            {
                                if (client.Inventory.HaveSpace(10))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.Add(stream, 722723, 10, 0);
                                }
                                else
                                {
                                    client.SendSysMesage("Please make 4 more spaces in your inventory.");
                                }
                                break;
                            }  // MossBag




                        case 723711://MeteorTearScroll
                            {
                                if (client.Inventory.HaveSpace(4))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.Add(stream, 1088002, 5, 0);
                                }
                                else
                                {
#if Arabic
                                    client.SendSysMesage("Please make 4 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 4 more spaces in your inventory.");
#endif

                                }
                                break;
                            }
                        case 723726://LifeFruit
                            {
                                if (DateTime.Now > client.Player.MedicineStamp.AddMilliseconds(300) && !Program.NoDrugMap.Contains(client.Player.Map))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Player.HitPoints = (int)client.Status.MaxHitpoints;
                                    client.Player.Mana = (ushort)client.Status.MaxMana;
                                    client.Player.MedicineStamp = DateTime.Now;
                                }
                                break;
                            }
                        case 720011://
                            {
                                if (client.Inventory.HaveSpace(3))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.Add(stream, 1002000, 3, 0);
                                }
                                else
                                {
                                    client.SendSysMesage("Please make 3 more spaces in your inventory.");
                                }
                                break;
                            }
                        case 720012://
                            {
                                if (client.Inventory.HaveSpace(3))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.Add(stream, 1002010, 3, 0);
                                }
                                else
                                {
                                    client.SendSysMesage("Please make 3 more spaces in your inventory.");
                                }
                                break;
                            }
                        case 720013://
                            {
                                if (client.Inventory.HaveSpace(3))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.Add(stream, 1002020, 3, 0);
                                }
                                else
                                {
                                    client.SendSysMesage("Please make 3 more spaces in your inventory.");
                                }
                                break;
                            }
                        case 723725://LifeFruitBasket
                            {
                                if (client.Inventory.HaveSpace(9))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.Add(stream, 723726, 10, 0);
                                }
                                else
                                {
#if Arabic
                                     client.SendSysMesage("Please make 9 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 9 more spaces in your inventory.");
#endif

                                }
                                break;
                            }
                        case 729022://+2 stone packet
                            {
                                if (client.Inventory.HaveSpace(9))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.Add(stream, 730002, 10, 2);
                                }
                                else
                                {
#if Arabic
                                         client.SendSysMesage("Please make 9 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 9 more spaces in your inventory.");
#endif

                                }
                                break;
                            }
                        case 729023://+3 stone packet
                            {
                                if (client.Inventory.HaveSpace(9))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.Add(stream, 730003, 10, 3);
                                }
                                else
                                {
#if Arabic
                                         client.SendSysMesage("Please make 9 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 9 more spaces in your inventory.");
#endif

                                }
                                break;
                            }
                        case 727060:
                            {
                                if (client.Inventory.HaveSpace(4))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                }
                                else
                                {
#if Arabic
                                         client.SendSysMesage("Please make 4 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 4 more spaces in your inventory.");
#endif

                                }
                                break;
                            }
                        case 727061:
                            {
                                if (client.Inventory.HaveSpace(4))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                }
                                else
                                {
#if Arabic
                                         client.SendSysMesage("Please make 4 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 4 more spaces in your inventory.");
#endif

                                }
                                break;
                            }
                        case 727062:
                            {
                                if (client.Inventory.HaveSpace(4))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.Add(stream, (uint)(700000 + (uint)Role.Flags.Gem.NormalKylinGem), 5);
                                }
                                else
                                {
#if Arabic
                                         client.SendSysMesage("Please make 4 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 4 more spaces in your inventory.");
#endif

                                }
                                break;
                            }
                        case 727063:
                            {
                                if (client.Inventory.HaveSpace(4))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.Add(stream, (uint)(700000 + (uint)Role.Flags.Gem.NormalRainbowGem), 5);
                                }
                                else
                                {
#if Arabic
                                         client.SendSysMesage("Please make 4 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 4 more spaces in your inventory.");
#endif

                                }
                                break;
                            }
                        case 727064:
                            {
                                if (client.Inventory.HaveSpace(4))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.Add(stream, (uint)(700000 + (uint)Role.Flags.Gem.NormalFuryGem), 5);
                                }
                                else
                                {
#if Arabic
                                         client.SendSysMesage("Please make 4 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 4 more spaces in your inventory.");
#endif

                                }
                                break;
                            }
                        case 727065:
                            {
                                if (client.Inventory.HaveSpace(4))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.Add(stream, (uint)(700000 + (uint)Role.Flags.Gem.NormalDragonGem), 5);
                                }
                                else
                                {
#if Arabic
                                         client.SendSysMesage("Please make 4 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 4 more spaces in your inventory.");
#endif

                                }
                                break;
                            }
                        case 727066:
                            {
                                if (client.Inventory.HaveSpace(4))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.Add(stream, (uint)(700000 + (uint)Role.Flags.Gem.NormalPhoenixGem), 5);
                                }
                                else
                                {
#if Arabic
                                         client.SendSysMesage("Please make 4 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 4 more spaces in your inventory.");
#endif

                                }
                                break;
                            }
                        case 727067:
                            {
                                if (client.Inventory.HaveSpace(4))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.Add(stream, (uint)(700000 + (uint)Role.Flags.Gem.NormalVioletGem), 5);
                                }
                                else
                                    client.SendSysMesage("Your Inventory Is Full , you need 4 spaces!");
                                break;
                            }
                        case 727068:
                            {
                                if (client.Inventory.HaveSpace(4))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.Add(stream, (uint)(700000 + (uint)Role.Flags.Gem.NormalMoonGem), 5);
                                }
                                else
                                {
#if Arabic
                                         client.SendSysMesage("Please make 4 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 4 more spaces in your inventory.");
#endif

                                }
                                break;
                            }
                        case 727069:
                            {
                                if (client.Inventory.HaveSpace(4))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                }
                                else
                                {
#if Arabic
                                         client.SendSysMesage("Please make 4 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 4 more spaces in your inventory.");
#endif

                                }
                                break;
                            }
                        case 727464:
                            {
                                if (client.Inventory.HaveSpace(4))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    if (item.Bound == 1)
                                    {
                                        client.Inventory.Add(stream, Database.ItemType.ExpBall, 5, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                    }
                                    else
                                        client.Inventory.Add(stream, Database.ItemType.ExpBall, 5);
                                }
                                else
                                {
#if Arabic
                                         client.SendSysMesage("Please make 4 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 4 more spaces in your inventory.");
#endif

                                }
                                break;
                            }
                        case 723712://+1 stone packet
                            {
                                if (client.Inventory.HaveSpace(4))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    if (item.Bound == 1)
                                    {
                                        client.Inventory.Add(stream, 730001, 5, 1, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                    }
                                    else
                                        client.Inventory.Add(stream, 730001, 5, 1);
                                }
                                else
                                {
#if Arabic
                                         client.SendSysMesage("Please make 4 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 4 more spaces in your inventory.");
#endif

                                }
                                break;
                            }
                        case 720128:
                        case 723727:
                            {
                                if (client.Player.Map >= 6000 && client.Player.Map <= 6004)
                                {
                                    client.SendSysMesage("You can`t use this in jail.", MsgMessage.ChatMode.System);
                                    break;
                                }
                                if (client.Player.PKPoints > 30)
                                {
                                    client.Player.PKPoints -= 30;
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
#if Arabic
                                      client.SendSysMesage("You've removed 30 Pk Points.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#else
                                    client.SendSysMesage("You've removed 30 Pk Points.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#endif

                                }
                                else
                                {
#if Arabic
                                             client.SendSysMesage("You can not use this item for now since your Pk points are less than 30.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
                            
#else
                                    client.SendSysMesage("You can not use this item for now since your Pk points are less than 30.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);

#endif
                                }
                                break;
                            }
                        case 720393:
                            {

                                client.SendSysMesage("From now on, you can get triple experience for the next two hours.");
                                client.Player.RateExp = 3;
                                client.Player.DExpTime = 3600 * 2;
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.CreateExtraExpPacket(stream);
                                break;
                            }
                        case Database.ItemType.DoubleExp:
                            {
                                client.Player.RateExp = 2;
                                client.Player.DExpTime = 1800;
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.CreateExtraExpPacket(stream);
#if Arabic
                                client.SendSysMesage("You get double exp. time of one hour.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#else
                                client.SendSysMesage("You get double exp. time of one hour.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.yellow);
#endif

                                break;
                            }
                        case 3000254:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 200407, 1, 0, 1);
                                break;
                            }
                        case 3000266:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 200446, 1, 0, 1);
                                break;
                            }
                        case 3000257:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 200464, 1, 0, 1);
                                break;
                            }
                        case 3000263:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 200418, 1, 0, 1);
                                break;
                            }
                        case 3000265:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 200420, 1, 0, 1);
                                break;
                            }
                        case 3000267:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 200427, 1, 0, 1);
                                break;
                            }
                        case 3000268:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 200431, 1, 0, 1);
                                break;
                            }
                        case 3000256:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 200411, 1, 0, 1);
                                break;
                            }
                        case 3000258:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 200413, 1, 0, 1);
                                break;
                            }
                        case 3000259:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 200414, 1, 0, 1);
                                break;
                            }
                        case 3000260:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 200415, 1, 0, 1);
                                break;
                            }
                        case 3000261:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 200416, 1, 0, 1);
                                break;
                            }
                        case 3000262:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 200417, 1, 0, 1);
                                break;
                            }
                        case 3000264:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 200419, 1, 0, 1);
                                break;
                            }
                        case 3000269:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 200438, 1, 0, 1);
                                break;
                            }
                        case 3000270:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 200444, 1, 0, 1);
                                break;
                            }
                        case 3000271:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 200445, 1, 0, 1);
                                break;
                            }
                        case Database.ItemType.ExpBall:
                            {
                                if (client.Player.Level >= 130)
                                {
                                    // client.SendSysMesage("You are high level ! ", MsgMessage.ChatMode.TopLeft, MsgMessage.MsgColor.red);
                                    break;
                                }
                                if (client.Player.ExpBallUsed < 10)
                                {
                                    client.Player.ExpBallUsed++;
                                    client.GainExpBall(600, false, Role.Flags.ExperienceEffect.angelwing);
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                }
                                else
                                {
#if Arabic
                                     client.SendSysMesage("You can use only ten exp balls a day. Try tomorrow.", MsgMessage.ChatMode.TopLeft, MsgMessage.MsgColor.red);
#else
                                    client.SendSysMesage("You can only use 10 ExpBalls a day.", MsgMessage.ChatMode.TopLeft, MsgMessage.MsgColor.red);
#endif

                                }
                                break;
                            }
                        case 722136:
                        case Database.ItemType.ExpBall2:
                            {
                                if (client.Player.Level >= 130)
                                {
                                    client.SendSysMesage("You're already max level.", MsgMessage.ChatMode.System);
                                    break;
                                }

                                //client.SendSysMesage("Congratulations! You have obtained the experience worth 1 Exp Ball.", MsgMessage.ChatMode.System);
                                if (item.ITEM_ID == 722136)
                                    client.GainExpBall(1200, false, Role.Flags.ExperienceEffect.angelwing);
                                else
                                    client.GainExpBall(600, false, Role.Flags.ExperienceEffect.angelwing);

                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                break;
                            }
                        case 723744:
                        case Database.ItemType.PowerExpBall:
                            {
                                //break;
                                if (client.Player.Level < 130)
                                {
                                    var nextlevel = Database.Server.LevelInfo[Database.DBLevExp.Sort.User][(byte)(client.Player.Level)];
                                    ulong exp = (ulong)(nextlevel.Experience * 10 / 100);
                                    client.Player.Experience += exp + 1;
                                    client.Player.SendUpdate(stream, (long)client.Player.Experience, MsgUpdate.DataType.Experience);

                                    if (client.Player.Experience >= nextlevel.Experience)
                                    {
                                        ushort level = (ushort)(client.Player.Level + 1);
                                        client.UpdateLevel(stream, level, true);
                                    }
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                }
                                else
                                {
                                    client.SendSysMesage("You're already max level.", MsgMessage.ChatMode.System);
                                }
                                break;
                            }

                        case 729979:
                        case 729980:
                        case 729981:
                            {
                                client.Inventory.Remove(729981, 1, stream);
                                client.Inventory.Remove(729980, 1, stream);
                                client.Inventory.Remove(729979, 1, stream);
                                client.Teleport(535, 477, 1768);
                                ActionQuery action = new ActionQuery();
                                action.ObjId = client.Player.UID;
                                action.Type = ActionType.CountDown;
                                action.dwParam = (uint)300;
                                client.Send(stream.ActionCreate(&action));
                                client.Player.TaskQuestTimer = DateTime.Now.AddSeconds(300);
                                break;
                            }
                        case 729983:
                        case 729984:
                        case 729985:
                        case 729986:
                        case 729987:
                        case 729988:
                        case 729989:
                            {
                                if (client.Inventory.Contain(729983, 1) && client.Inventory.Contain(729984, 1) && client.Inventory.Contain(729985, 1)
                                 && client.Inventory.Contain(729986, 1) && client.Inventory.Contain(729987, 1) && client.Inventory.Contain(729988, 1)
                                 && client.Inventory.Contain(729989, 1))
                                {
                                    client.Inventory.Remove(729983, 1, stream);
                                    client.Inventory.Remove(729984, 1, stream);
                                    client.Inventory.Remove(729985, 1, stream);
                                    client.Inventory.Remove(729986, 1, stream);
                                    client.Inventory.Remove(729987, 1, stream);
                                    client.Inventory.Remove(729988, 1, stream);
                                    client.Inventory.Remove(729989, 1, stream);
                                    client.Inventory.Add(stream, 729982);
                                    client.CreateBoxDialog("I will pass that exam! For the students, passing this exam means a guaranteed position and great credit to their family.");
                                    client.Teleport(554, 600, 1020);
                                    break;
                                }
                                else
                                {
#if Arabic
                                     client.CreateBoxDialog("Well, you are short of powders.");
#else
                                    client.CreateBoxDialog("Well, you are short of powders.");
#endif

                                }
                                break;
                            }
                        case 720968:
                            {
                                if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 280, 442) <= 4 && client.Player.Map == 1000)
                                {

                                    client.Player.SendString(stream, MsgStringPacket.StringID.Effect, true, "relive1");
                                    client.CreateBoxDialog("You~put~the~Antidote~in~the~Moon~Spring~and~the~water~became~clear!");
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                }
                                else
                                {
                                    client.CreateBoxDialog("Get~closer~to~put~the~Antidote.");
                                }

                                break;
                            }
                        case 720938:
                            {

                                if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 293, 450) <= 12 && client.Player.Map == 1000)
                                {

                                    client.Player.SendString(stream, MsgStringPacket.StringID.Effect, true, "relive1");
                                    client.CreateBoxDialog("You`ve~filled~the~Emerald~Bottle~with~water.~Now~go~report~to~Taoist~Yu.");
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                }
                                else
                                {
                                    client.CreateBoxDialog("You`ll~need~to~collect~water~on~the~platform~in~the~Moon~Spring.");
                                }
                                break;
                            }
                        case 720966:
                            {
                                if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 292, 453) <= 3 && client.Player.Map == 1000)
                                {
                                    client.Player.AddFlag(MsgUpdate.Flags.Poisoned, 5, true);
                                    client.Player.SendString(stream, MsgStringPacket.StringID.Effect, true, "relive1");
                                    client.CreateBoxDialog("You~worshipped~the~god~of~water.~Go~report~to~the~Taoist.");
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                }
                                else
                                {
                                    client.CreateBoxDialog("Get~closer~to~use~the~offering.");
                                }
                                break;
                            }
                        case 720967:
                            {
                                if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 292, 453) <= 15 && client.Player.Map == 1000)
                                {

                                    client.Player.SendString(stream, MsgStringPacket.StringID.Effect, true, "relive1");
                                    client.CreateBoxDialog("You~drank~the~Moon~Spring~and~instantly~felt~dizzy.~Go~report~to~Spring~Soldier~Zhao.");
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                }
                                else
                                {
                                    client.CreateBoxDialog("Get~closer~to~use~the~offering.");
                                }
                                break;
                            }
                        case 720985:
                        case 723724:
                            {
                                client.Player.TransformInfo = new Role.ClientTransform(client.Player);
                                client.Player.TransformInfo.CreateTransform(stream, (uint)client.Player.HitPoints, Database.Tranformation.GetRandomTransform(), 60, 1360);
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                break;
                            }
                        #region PrayingStone(S)
                        case 1200000:
                            {
                                client.Player.AddHeavenBlessing(stream, (int)(3 * 24 * 60 * 60));
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                break;
                            }
                        #endregion
                        #region PrayingStone(M)
                        case 1200001:
                            {
                                client.Player.AddHeavenBlessing(stream, (int)(7 * 24 * 60 * 60));
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                break;
                            }
                        #endregion
                        #region PrayingStone(L)
                        case 1200002:
                            {
                                client.Player.AddHeavenBlessing(stream, (int)(30 * 24 * 60 * 60));
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                break;
                            }
                        #endregion
                        case 720173:
                            {
                                client.Player.AddHeavenBlessing(stream, (int)(5 * 60 * 60));
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
#if Arabic
                                 client.SendSysMesage("Congratulation! " + client.Player.Name + " got 5 hours of Heaven Blessing.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#else
                                client.SendSysMesage("Congratulation! " + client.Player.Name + " got 5 hours of Heaven Blessing.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#endif

                                break;
                            }
                        case 728898:
                            {
                                if (client.Inventory.Contain(728898, 8))
                                {
                                    if (!client.Inventory.HaveSpace(1))
                                    {
                                        client.CreateBoxDialog("Please make 1 more space in your inventory");
                                        break;
                                    }
                                    client.Inventory.Remove(728898, 8, stream);

                                    client.Inventory.Add(stream, 723714);
                                    client.Player.SendString(stream, MsgStringPacket.StringID.Effect, false, "zf2-e104");
                                    client.CreateBoxDialog("You~received~a~Class2~Money~Bag~after~you~combined~8~Tiny~Class2~Money~Pouches!");
                                }
                                else
                                    client.CreateBoxDialog("You~need~to~have~8~Tiny~Class2~Money~Pouches~to~combine~and~get~a~Class2~Money~Bag.");
                                break;
                            }
                        case 728899:
                            {
                                if (client.Inventory.Contain(728899, 6))
                                {
                                    if (!client.Inventory.HaveSpace(1))
                                    {
                                        client.CreateBoxDialog("Please make 1 more space in your inventory");
                                        break;
                                    }
                                    client.Inventory.Remove(728899, 6, stream);
                                    client.Inventory.Add(stream, 720880);
                                    client.Player.SendString(stream, MsgStringPacket.StringID.Effect, false, "zf2-e104");
                                    client.CreateBoxDialog("You~received~a~Horse~Racing~Points~Pack(3K)~after~you~combined~6~Tiny~Horse~Racing~Points~Bags!");

                                }
                                else
                                    client.CreateBoxDialog("You~need~to~have~6~Tiny~Horse~Racing~Points~Bags~to~combine~and~get~a~Horse~Racing~Points~Pack(3K).");
                                break;
                            }
                        case 729508:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.AddFlag(MsgUpdate.Flags.XPList, 20, true);
                                client.Player.SendString(stream, Game.MsgServer.MsgStringPacket.StringID.Effect, true, new string[1] { "xp" });
                                break;
                            }
                        case 720837:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.TransformInfo = new Role.ClientTransform(client.Player);
                                client.Player.TransformInfo.CreateTransform(stream, 817, 201, (int)60);
                                break;
                            }
                        case 720172:
                            {
                                if (client.Player.Level == 140)
                                {
                                    client.SendSysMesage("As your character has reached the highest level, you cannot use this.", MsgMessage.ChatMode.System);
                                    break;
                                }
                                client.GainExpBall(300, false, Role.Flags.ExperienceEffect.angelwing);
#if Arabic
                                  client.SendSysMesage("Congratulation! You have obtained the experience worth 1 EXPBallScrap.", MsgMessage.ChatMode.System);
#else
                                client.SendSysMesage("Congratulation! You have obtained the experience worth 1 EXPBallScrap.", MsgMessage.ChatMode.System);
#endif

                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                break;
                            }
                        case 728900:
                            {
                                if (!client.Inventory.HaveSpace(1))
                                {
                                    client.CreateBoxDialog("Please make 1 more space in your inventory");
                                    break;
                                }
                                if (client.Inventory.Contain(728900, 8))
                                {
                                    client.Inventory.Remove(728900, 8, stream);
                                    client.Inventory.Add(stream, 723713);
                                    client.Player.SendString(stream, MsgStringPacket.StringID.Effect, true, "zf2-e104");
                                    client.CreateBoxDialog("You~received~a~Class1~Money~Bag~after~you~combined~8~Class1~Money~Pouches!");
                                }
                                else
                                    client.CreateBoxDialog("You~need~to~have~8~Class1~Money~Pouches~to~combine~and~get~a~Class1~Money~Bag.");

                                break;
                            }
                        case 728897:
                            {
                                if (client.Inventory.Contain(728897, 15))
                                {
                                    if (!client.Inventory.HaveSpace(1))
                                    {
                                        client.CreateBoxDialog("Please make 1 more space in your inventory");
                                        break;
                                    }
                                    client.Inventory.Remove(728897, 15, stream);
                                    client.Inventory.Add(stream, Database.ItemType.ExpBall2);
                                    client.Player.SendString(stream, MsgStringPacket.StringID.Effect, false, "zf2-e104");
                                    client.CreateBoxDialog("You~received~an~EXP~Ball~after~you~combined~15~Special~EXP~Ball~Scraps!");
                                }
                                else
                                {
                                    client.CreateBoxDialog("You~need~to~have~15~Special~EXP~Ball~Scraps~to~combine~and~get~a~EXP~Ball.");
                                }

                                break;
                            }
                        case 720840:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                Database.Server.AddMapMonster(stream, client.Map, 8303, client.Player.X, client.Player.Y, 5, 5, 1);

                                break;
                            }
                        case 728237:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (Database.AtributesStatus.IsTrojan(client.Player.Class))
                                    client.Inventory.Add(stream, 130027, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                else if (Database.AtributesStatus.IsWarrior(client.Player.Class))
                                    client.Inventory.Add(stream, 131027, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                else if (Database.AtributesStatus.IsArcher(client.Player.Class))
                                    client.Inventory.Add(stream, 133027, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                else if (Database.AtributesStatus.IsNinja(client.Player.Class))
                                    client.Inventory.Add(stream, 135027, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                else if (Database.AtributesStatus.IsMonk(client.Player.Class))
                                    client.Inventory.Add(stream, 136027, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                else if (Database.AtributesStatus.IsPirate(client.Player.Class))
                                    client.Inventory.Add(stream, 139027, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                else if (Database.AtributesStatus.IsTaoist(client.Player.Class))
                                    client.Inventory.Add(stream, 134027, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
#if Arabic
                                  client.CreateBoxDialog("You~received~a~Level~32~Unique~Armor!");
#else
                                client.CreateBoxDialog("You~received~a~Level~32~Unique~Armor!");
#endif

                                break;
                            }
                        case 721700:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                var map = Database.Server.ServerMaps[1013];
                                var dinamic = map.GenerateDynamicID();
                                Database.Server.AddMapMonster(stream, map, 14333, 30, 30, 18, 18, 13, dinamic, false);
                                client.Teleport(50, 50, 1013, dinamic);
#if Arabic
                                 client.CreateBoxDialog("You`ve~taken~the~toxin~and~found~yourself~in~a~strange~place.");
#else
                                client.CreateBoxDialog("You`ve~taken~the~toxin~and~found~yourself~in~a~strange~place.");
#endif

                                break;
                            }
                        case 728244:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 160118);
#if Arabic
                                   client.CreateBoxDialog("You~opened~the~pack~and~received~an~Elite~Crocodile~Boots!");
#else
                                client.CreateBoxDialog("You~opened~the~pack~and~received~an~Elite~Crocodile~Boots!");
#endif

                                break;
                            }
                        case 721701:
                            {
                                if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 795, 471) < 18)
                                {
#if Arabic
                                     if (client.Player.View.ContainMobInScreen("VenomousApe"))
                                    {
                                        Game.MsgNpc.Dialog dialog = new Game.MsgNpc.Dialog(client, stream);
                                        dialog.AddText("Please kill all VenomousApe`s.")
                                            .FinalizeDialog();
                                        break;
                                    }
#else
                                    if (client.Player.View.ContainMobInScreen("VenomousApe"))
                                    {
                                        Game.MsgNpc.Dialog dialog = new Game.MsgNpc.Dialog(client, stream);
                                        dialog.AddText("Please kill all VenomousApe`s.")
                                            .FinalizeDialog();
                                        break;
                                    }
#endif

                                    client.Player.QuestMultiple = 1;
                                    Database.Server.AddMapMonster(stream, client.Map, 14334, client.Player.X, client.Player.Y, 7, 7, 7, 0, true);
                                }
                                else
                                {
#if Arabic
                                      Game.MsgNpc.Dialog dialog = new Game.MsgNpc.Dialog(client, stream);
                                    dialog.AddText("Wrong place. You may only use the Metamorphic Vial in Love Canyon (795,471).")
                                        .FinalizeDialog();
#else
                                    Game.MsgNpc.Dialog dialog = new Game.MsgNpc.Dialog(client, stream);
                                    dialog.AddText("Wrong place. You may only use the Metamorphic Vial in Love Canyon (795,471).")
                                        .FinalizeDialog();
#endif

                                }
                                break;
                            }
                        case 728238:
                            {
                                client.ActiveNpc = (uint)Game.MsgNpc.NpcID.Level43UniqueRingPack;
                                Game.MsgNpc.Dialog dialog = new Game.MsgNpc.Dialog(client, stream);
#if Arabic
                                                                dialog.AddText("Take a good look and pick one.")
                                    .AddOption("Unique~Ivory~Heavy~Ring.", 1)
                                    .AddOption("Unique~Amethyst~Ring.", 2)
                                    .FinalizeDialog();
#else
                                dialog.AddText("Take a good look and pick one.")
                                    .AddOption("Unique~Ivory~Heavy~Ring.", 1)
                                    .AddOption("Unique~Amethyst~Ring.", 2)
                                    .FinalizeDialog();
#endif

                                break;
                            }
                        case 729971:
                            {
                                Game.MsgNpc.Dialog dialog = new Game.MsgNpc.Dialog(client, stream);
#if Arabic
                                 dialog.AddText("We sincerely welcome you to join us and admire the Spun Gold Armor recently found by Cloud the Lustful.")
                                    .FinalizeDialog();
#else
                                dialog.AddText("We sincerely welcome you to join us and admire the Spun Gold Armor recently found by Cloud the Lustful.")
                                   .FinalizeDialog();
#endif

                                break;
                            }
                        case 729972:
                            {
                                if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 100, 430) < 18)
                                {
                                    Database.Server.AddMapMonster(stream, client.Map, 1403, client.Player.X, client.Player.Y, 1, 1, 1, client.Player.DynamicID, true, MsgFloorItem.MsgItemPacket.EffectMonsters.EarthquakeAndNight);
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                }
                                else
                                {
                                    Game.MsgNpc.Dialog dialog = new Game.MsgNpc.Dialog(client, stream);
#if Arabic
 dialog.AddText("Visit Zhao Jian (200,325), disguise yourself as a young girl, then use the offering at the Altar (100,430).")
                                        .FinalizeDialog();

#else
                                    dialog.AddText("Visit Zhao Jian (200,325), disguise yourself as a young girl, then use the offering at the Altar (100,430).")
                                       .FinalizeDialog();

#endif

                                }
                                break;
                            }
                        case 729973:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 729974);
                                break;
                            }
                        case 3001027:
                            {
                                if (!client.Inventory.HaveSpace(10))
                                {
#if Arabic
                                       client.CreateBoxDialog("Please make 10 more spaces in your inventory.");
#else
                                    client.CreateBoxDialog("Please make 10 more spaces in your inventory.");
#endif

                                    break;

                                }
                                client.Inventory.Add(stream, Database.ItemType.ExpBall, 10, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
#if Arabic
                                  client.CreateBoxDialog("You've received 10 Exp Balls(B).");
#else
                                client.CreateBoxDialog("You've received 10 Exp Balls(B).");
#endif

                                break;
                            }
                        case 3002307:
                            {

                                if (client.Inventory.Contain(3002307, 3))
                                {
                                    if (!client.Inventory.HaveSpace(1))
                                    {
#if Arabic
                                            client.CreateBoxDialog("Please make 1 more space in your inventory.");
#else
                                        client.CreateBoxDialog("Please make 1 more space in your inventory.");
#endif

                                        break;
                                    }
                                    client.Inventory.Remove(3002307, 3, stream);
                                    client.Inventory.Add(stream, 3002308);
#if Arabic
                                    client.SendSysMesage("You've received HeavenKey.", MsgMessage.ChatMode.System);
#else
                                    client.SendSysMesage("You've received HeavenKey.", MsgMessage.ChatMode.System);
#endif

                                }
                                else
                                {
#if Arabic
                                    
                                    client.CreateBoxDialog("Failed to combine. You don`t have 3 Heaven Ket Scraps.");
#else

                                    client.CreateBoxDialog("Failed to combine. You don`t have 3 Heaven Ket Scraps.");
#endif
                                }
                                break;
                            }
                        case 729307:
                        case 729308:
                        case 729309:
                        case 729310:
                        case 729306:
                            {
#if Arabic
                                   client.Player.AddPick(stream, "Pick", 2);
#else
                                client.Player.AddPick(stream, "Pick", 2);
#endif

                                break;
                            }
                        case 3005160:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 1200000);
                                break;
                            }
                        case 721625://attackpotin 30
                            {
                                if (client.Player.Level > 100)
                                {
#if Arabic
                                         client.SendSysMesage("sorry, the max level is 100, for active this.");
#else
                                    client.SendSysMesage("Sorry, you need level 100 to use this.");
#endif

                                    break;
                                }
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.ActiveAttackPotion(30);
                                break;
                            }
                        case 721626://defensepotin 30
                            {
                                if (client.Player.Level > 100)
                                {
#if Arabic
                                          client.SendSysMesage("sorry, the max level is 100, for active this.");
#else
                                    client.SendSysMesage("Sorry, you need level 100 to use this.");
#endif

                                    break;
                                }
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.ActiveDefensePotion(30);
                                break;
                            }
                        case 3000587:
                            {
                                if (!client.Inventory.HaveSpace(10))
                                {
#if Arabic
                                     client.SendSysMesage("Please make 10 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 10 more spaces in your inventory.");
#endif

                                    break;
                                }
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 117026, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//earring
                                if (Database.AtributesStatus.IsTrojan(client.Player.Class))
                                {
                                    client.Inventory.Add(stream, 130026, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                    client.Inventory.Add(stream, 410056, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//blade
                                    client.Inventory.Add(stream, 480056, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                    client.Inventory.Add(stream, 150056, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//ring
                                    client.Inventory.Add(stream, 120046, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//neck
                                    client.Inventory.Add(stream, 160056, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//boots
                                }
                                else if (Database.AtributesStatus.IsWarrior(client.Player.Class))
                                {
                                    client.Inventory.Add(stream, 131026, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                    client.Inventory.Add(stream, 561056, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//wand
                                    client.Inventory.Add(stream, 150056, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//ring
                                    client.Inventory.Add(stream, 120046, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//neck
                                    client.Inventory.Add(stream, 160056, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//boots
                                }
                                else if (Database.AtributesStatus.IsArcher(client.Player.Class))
                                {
                                    client.Inventory.Add(stream, 133016, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//armor
                                    client.Inventory.Add(stream, 500046, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//bow
                                    client.Inventory.Add(stream, 150056, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//ring
                                    client.Inventory.Add(stream, 120046, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//neck
                                    client.Inventory.Add(stream, 160056, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//boots
                                }
                                else if (Database.AtributesStatus.IsNinja(client.Player.Class))
                                {
                                    client.Inventory.Add(stream, 135026, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                    client.Inventory.Add(stream, 601056, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                    client.Inventory.Add(stream, 601056, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                    client.Inventory.Add(stream, 150056, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//ring
                                    client.Inventory.Add(stream, 120046, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//neck
                                    client.Inventory.Add(stream, 160056, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//boots
                                }
                                else if (Database.AtributesStatus.IsMonk(client.Player.Class))
                                {
                                    client.Inventory.Add(stream, 136026, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                    client.Inventory.Add(stream, 610056, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                    client.Inventory.Add(stream, 610056, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                    client.Inventory.Add(stream, 150056, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//ring
                                    client.Inventory.Add(stream, 120046, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//neck
                                    client.Inventory.Add(stream, 160056, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//boots
                                }
                                else if (Database.AtributesStatus.IsPirate(client.Player.Class))
                                {
                                    client.Inventory.Add(stream, 139026, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                    client.Inventory.Add(stream, 611056, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//rapier
                                    client.Inventory.Add(stream, 612056, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//pistol
                                    client.Inventory.Add(stream, 150056, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//ring
                                    client.Inventory.Add(stream, 120046, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//neck
                                    client.Inventory.Add(stream, 160056, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//boots

                                }
                                else if (Database.AtributesStatus.IsTaoist(client.Player.Class))
                                {
                                    client.Inventory.Add(stream, 134026, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                    client.Inventory.Add(stream, 421046, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                    client.Inventory.Add(stream, 160056, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//boots
                                    client.Inventory.Add(stream, 121046, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//bag
                                    client.Inventory.Add(stream, 152066, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);//bag
                                    //152066
                                }
#if Arabic
                                client.CreateBoxDialog("You've received a suit of Level 30 Refined equipment.");
#else
                                client.CreateBoxDialog("You've received a suit of Level 30 Refined equipment.");
#endif

                                break;
                            }
                        case 3001289:
                            {
                                if (client.Inventory.HaveSpace(3))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.Add(stream, Database.ItemType.MoonBox, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                    client.Inventory.Add(stream, 1080001, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                    client.Inventory.Add(stream, 721259, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);

                                }
                                else
                                {
#if Arabic
                                     client.SendSysMesage("Please make 3 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 3 more spaces in your inventory.");
#endif

                                }
                                break;
                            }
                        case 3001266:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.Money += 100000;
                                client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);

                                break;
                            }
                        case 3000781:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.AddItemWitchStack(Database.ItemType.DoubleExp, 0, 10, stream, true);
                                break;
                            }
                        case 3000782:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.AddItemWitchStack(723727, 0, 10, stream, true);
                                break;
                            }
                        case 720844:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                Database.Server.AddMapMonster(stream, client.Map, 1, client.Player.X, client.Player.Y, 1, 1, 1, client.Player.DynamicID, true, MsgFloorItem.MsgItemPacket.EffectMonsters.EarthquakeAndNight);
                                break;
                            }
                        case 750000:
                            {
                                if (client.DemonExterminator != null)
                                {
#if Arabic
                                      client.SendSysMesage("You currently have " + client.DemonExterminator.HuntKills + "KOs of the required " + item.Durability + ". Please keep in mind the jar will sometimes list incorrect amounts.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.yellow);
#else
                                    client.SendSysMesage("You currently have " + client.DemonExterminator.HuntKills + "KOs of the required " + item.Durability + ". Please keep in mind the jar will sometimes list incorrect amounts.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.yellow);
#endif

                                }
                                break;
                            }

                        case 3100011:
                            {
                                if (!client.Inventory.HaveSpace(1))
                                {
#if Arabic
                                    client.SendSysMesage("Please make 1 more space in inventory.");
#else
                                    client.SendSysMesage("Please make 1 more space in inventory.");
#endif

                                    break;
                                }
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                byte rand = (byte)Program.GetRandom.Next(0, 7);
                                switch (rand)
                                {
                                    default:
                                        {
                                            client.Inventory.Add(stream, 3600023, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
#if Arabic
                                              client.CreateBoxDialog("You received EminentExploitPack.");
#else
                                            client.CreateBoxDialog("You received EminentExploitPack.");
#endif

                                            break;
                                        }
                                }
                                break;
                            }
                        case 3100014:
                            {
                                if (!client.Inventory.HaveSpace(2))
                                {
#if Arabic
                                      client.SendSysMesage("Please make 2 more space in inventory.");
#else
                                    client.SendSysMesage("Please make 2 more space in inventory.");
#endif

                                    break;
                                }
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);

                                client.Inventory.Add(stream, 3600023, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                client.Inventory.Add(stream, 3100012, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
#if Arabic

#else

#endif
                                client.CreateBoxDialog("You received EminentExploitPack and SeniorSign-inBox.");
                                break;
                            }
                        case 3100015:
                            {
                                if (!client.Inventory.HaveSpace(2))
                                {
#if Arabic
                                     client.SendSysMesage("Please make 2 more space in inventory.");
#else
                                    client.SendSysMesage("Please make 2 more space in inventory.");
#endif

                                    break;
                                }
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 730003, 1, 3, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                client.Inventory.Add(stream, 3100012, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
#if Arabic
                                  client.CreateBoxDialog("You received EminentExploitPack, +3Stone(B) and 800ChiPoints.");
#else
                                client.CreateBoxDialog("You received EminentExploitPack, +3Stone(B).");
#endif

                                break;
                            }
                        case 3100016:
                            {
                                if (!client.Inventory.HaveSpace(2))
                                {
#if Arabic
                                      client.SendSysMesage("Please make 2 more space in inventory.");
#else
                                    client.SendSysMesage("Please make 2 more space in inventory.");
#endif

                                    break;
                                }
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 3100012, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                client.Inventory.Add(stream, Database.ItemType.DragonBall, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
#if Arabic
                                  client.CreateBoxDialog("You received EminentExploitPack, DragonBall(B) and 1000ChiPoints.");
#else
                                client.CreateBoxDialog("You received EminentExploitPack, DragonBall(B) and 1000ChiPoints.");
#endif

                                break;
                            }
                        case 3100017:
                            {
                                if (!client.Inventory.HaveSpace(3))
                                {
#if Arabic
                                      client.SendSysMesage("Please make 3 more space in inventory.");
#else
                                    client.SendSysMesage("Please make 3 more space in inventory.");
#endif

                                    break;
                                }
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 3009000, 4, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                client.Inventory.Add(stream, 3009001, 2, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                client.Inventory.Add(stream, 3100012, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
#if Arabic
                                                                client.CreateBoxDialog("You received 4TwilightStarStones, 2BrightStarStones, a SeniorSign-inBox.");
#else
                                client.CreateBoxDialog("You received 4TwilightStarStones, 2BrightStarStones, a SeniorSign-inBox.");
#endif

                                break;
                            }

                        case 720861:
                            {
                                if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 231, 431) < 18 && client.Player.Map == 1020)
                                {
#if Arabic
                                        if (client.Player.View.ContainMobInScreen("SnakemanElder") == false)
                                        Database.Server.AddMapMonster(stream, client.Map, 14337, client.Player.X, client.Player.Y, 8, 8, 1, 0, true, MsgFloorItem.MsgItemPacket.EffectMonsters.Night);
                           
#else
                                    if (client.Player.View.ContainMobInScreen("SnakemanElder") == false)
                                        Database.Server.AddMapMonster(stream, client.Map, 14337, client.Player.X, client.Player.Y, 8, 8, 1, 0, true, MsgFloorItem.MsgItemPacket.EffectMonsters.Night);

#endif
                                }
                                else
                                {
#if Arabic
                                     client.SendSysMesage("Go to territory of the Apes at(231, 431) to use this item!", MsgMessage.ChatMode.System);
#else
                                    client.SendSysMesage("Go to territory of the Apes at(231, 431) to use this item!", MsgMessage.ChatMode.System);
#endif

                                }
                                break;
                            }
                        case 728246:
                            {



                                if (client.Player.Class <= 15)
                                {
                                    if (client.Inventory.HaveSpace(1))
                                    {
                                        client.Inventory.Remove(728246, 1, stream);
                                        client.Inventory.Add(stream, 722136, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                        client.Inventory.Add(stream, 130078, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                        client.CreateBoxDialog("You~opened~the~pack~and~received~an~Elite~Sacred~Armor!");
                                    }
                                    else
                                    {
                                        client.CreateBoxDialog("Please~prepare~one~slot~in~your~inventory.");
                                    }
                                }
                                else
                                {
                                    if (client.Player.Class <= 25)
                                    {
                                        if (client.Inventory.HaveSpace(1))
                                        {
                                            client.Inventory.Remove(728246, 1, stream);
                                            client.Inventory.Add(stream, 722136, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                            client.Inventory.Add(stream, 131078, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                            client.CreateBoxDialog("You~opened~the~pack~and~received~an~Elite~Lion~Armor!");
                                        }
                                        else
                                        {
                                            client.CreateBoxDialog("Please~prepare~one~slot~in~your~inventory.");
                                        }
                                    }
                                    else
                                    {
                                        if (client.Player.Class <= 45)
                                        {
                                            if (client.Inventory.HaveSpace(1))
                                            {
                                                client.Inventory.Remove(728246, 1, stream);
                                                client.Inventory.Add(stream, 722136, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                                client.Inventory.Add(stream, 133068, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                                client.CreateBoxDialog("You~opened~the~pack~and~received~an~Elite~Shark~Coat!");
                                            }
                                            else
                                            {
                                                client.CreateBoxDialog("Please~prepare~one~slot~in~your~inventory.");
                                            }
                                        }
                                        else
                                        {
                                            if (client.Player.Class <= 55)
                                            {
                                                if (client.Inventory.HaveSpace(1))
                                                {
                                                    client.Inventory.Remove(728246, 1, stream);
                                                    client.Inventory.Add(stream, 722136, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                                    client.Inventory.Add(stream, 135078, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                                    client.CreateBoxDialog("You~opened~the~pack~and~received~an~Elite~Bear~Vest!");
                                                }
                                                else
                                                {
                                                    client.CreateBoxDialog("Please~prepare~one~slot~in~your~inventory.");
                                                }
                                            }
                                            else
                                            {
                                                if (client.Player.Class <= 65)
                                                {
                                                    if (client.Inventory.HaveSpace(1))
                                                    {
                                                        client.Inventory.Remove(728246, 1, stream);
                                                        client.Inventory.Add(stream, 722136, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                                        client.Inventory.Add(stream, 136078, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                                        client.CreateBoxDialog("You~opened~the~pack~and~received~an~Elite~Tough~Frock!");
                                                    }
                                                    else
                                                    {
                                                        client.CreateBoxDialog("Please~prepare~one~slot~in~your~inventory.");
                                                    }
                                                }
                                                else
                                                {
                                                    if (client.Player.Class <= 75)
                                                    {
                                                        if (client.Inventory.HaveSpace(1))
                                                        {
                                                            client.Inventory.Remove(728246, 1, stream);
                                                            client.Inventory.Add(stream, 722136, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                                            client.Inventory.Add(stream, 139078, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                                            client.CreateBoxDialog("You~opened~the~pack~and~received~an~Elite~Pirate~clothing~(L87)!");
                                                        }
                                                        else
                                                        {
                                                            client.CreateBoxDialog("Please~prepare~one~slot~in~your~inventory.");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (client.Player.Class <= 85)
                                                        {
                                                            if (client.Inventory.HaveSpace(1))
                                                            {
                                                                client.Inventory.Remove(728246, 1, stream);
                                                                client.Inventory.Add(stream, 722136, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                                                client.Inventory.Add(stream, 138078, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                                                client.CreateBoxDialog("You~received~an~Level~87~Elite~Yellow~Combat~Suit~and~an~EXP~Ball!");
                                                            }
                                                            else
                                                            {
                                                                client.CreateBoxDialog("Please~prepare~one~slot~in~your~inventory.");
                                                            }
                                                        }
                                                        else if (client.Player.Class <= 145)
                                                        {
                                                            if (client.Inventory.HaveSpace(1))
                                                            {
                                                                client.Inventory.Remove(728246, 1, stream);
                                                                client.Inventory.Add(stream, 722136, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                                                client.Inventory.Add(stream, 134078, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                                                client.CreateBoxDialog("You~opened~the~pack~and~received~an~Elite~Full~Frock!");
                                                            }
                                                            else
                                                            {
                                                                client.CreateBoxDialog("Please~prepare~one~slot~in~your~inventory.");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (client.Inventory.HaveSpace(1))
                                                            {
                                                                client.Inventory.Remove(728246, 1, stream);
                                                                client.Inventory.Add(stream, 722136, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                                                client.Inventory.Add(stream, 170078, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                                                client.CreateBoxDialog("You~opened~the~pack~and~received~an~Elite~WindWalker~Armor!");
                                                            }
                                                            else
                                                            {
                                                                client.CreateBoxDialog("Please~prepare~one~slot~in~your~inventory.");
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        case 720862:
                            {
                                if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 360, 696) < 18 && client.Player.Map == 1020)
                                {
#if Arabic
        if (client.Player.View.ContainMobInScreen("HeresyElder") == false)
                                        Database.Server.AddMapMonster(stream, client.Map, 14338, client.Player.X, client.Player.Y, 8, 8, 1, 0, true, MsgFloorItem.MsgItemPacket.EffectMonsters.Night);

#else
                                    if (client.Player.View.ContainMobInScreen("HeresyElder") == false)
                                        Database.Server.AddMapMonster(stream, client.Map, 14338, client.Player.X, client.Player.Y, 8, 8, 1, 0, true, MsgFloorItem.MsgItemPacket.EffectMonsters.Night);

#endif

                                }
                                else
                                {
#if Arabic
                                       client.SendSysMesage("Go to territory of the Apes at(360,696) to use this item!", MsgMessage.ChatMode.System);
#else
                                    client.SendSysMesage("Go to territory of the Apes at(360,696) to use this item!", MsgMessage.ChatMode.System);
#endif

                                }

                                break;
                            }
                        case 723855:// +1MaroonSteedPack
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.AddSteed(stream, 300000, 1, 1, false, 0, 150, 255);
#if Arabic
                                      client.SendSysMesage("You received (+1) MaroonSteed.", MsgMessage.ChatMode.System);
#else
                                client.SendSysMesage("You received (+1) MaroonSteed.", MsgMessage.ChatMode.System);
#endif

                                break;
                            }
                        case 720609:// +3MaroonSteedPack
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.AddSteed(stream, 300000, 1, 3, false, 0, 150, 255);
#if Arabic
                                      client.SendSysMesage("You received (+3) MaroonSteed.", MsgMessage.ChatMode.System);
#else
                                client.SendSysMesage("You received (+3) MaroonSteed.", MsgMessage.ChatMode.System);
#endif

                                break;
                            }
                        case 723856:// +1WhiteSteedPack
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.AddSteed(stream, 300000, 1, 1, false, 150, 255, 0);
#if Arabic
                                client.SendSysMesage("You received (+1) WhiteSteed.", MsgMessage.ChatMode.System);
#else
                                client.SendSysMesage("You received (+1) WhiteSteed.", MsgMessage.ChatMode.System);
#endif

                                break;
                            }
                        case 720610:// +3WhiteSteedPack
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.AddSteed(stream, 300000, 1, 3, false, 150, 255, 0);
                                client.SendSysMesage("You received (+3) WhiteSteed.", MsgMessage.ChatMode.System);
                                break;
                            }
                        case 723859:// +1BlackSteedPack 
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.AddSteed(stream, 300000, 1, 1, false, 255, 0, 150);
#if Arabic
                                  client.SendSysMesage("You received (+1) BlackSteed.", MsgMessage.ChatMode.System);
#else
                                client.SendSysMesage("You received (+1) BlackSteed.", MsgMessage.ChatMode.System);
#endif

                                break;
                            }

                        case 723860:// +3MaroonSteedPack
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.AddSteed(stream, 300000, 1, 3, false, 0, 150, 255);
#if Arabic
                                 client.SendSysMesage("You received (+3) MaroonSteed.", MsgMessage.ChatMode.System);
#else
                                client.SendSysMesage("You received (+3) MaroonSteed.", MsgMessage.ChatMode.System);
#endif

                                break;
                            }
                        case 723861:// +3WhiteSteedPack
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.AddSteed(stream, 300000, 1, 3, false, 150, 255, 0);
#if Arabic
                                  client.SendSysMesage("You received (+3) WhiteSteed.", MsgMessage.ChatMode.System);
#else
                                client.SendSysMesage("You received (+3) WhiteSteed.", MsgMessage.ChatMode.System);
#endif

                                break;
                            }
                        case 720611:
                        case 723862:// +3BlackSteedPack 
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.AddSteed(stream, 300000, 1, 3, false, 255, 0, 150);
#if Arabic
                                    client.SendSysMesage("You received (+3) BlackSteed.", MsgMessage.ChatMode.System);
#else
                                client.SendSysMesage("You received (+3) BlackSteed.", MsgMessage.ChatMode.System);
#endif

                                break;
                            }

                        case 723863:// +6MaroonSteedPack
                            {

                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.AddSteed(stream, 300000, 1, 6, item.Bound > 0 ? true : false, 0, 150, 255);
#if Arabic
                                 client.SendSysMesage("You received (+6) MaroonSteed.", MsgMessage.ChatMode.System);
#else
                                client.SendSysMesage("You received (+6) MaroonSteed.", MsgMessage.ChatMode.System);
#endif

                                break;
                            }
                        case 723864:// +6WhiteSteedPack
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.AddSteed(stream, 300000, 1, 6, false, 150, 255, 0);
#if Arabic
                                  client.SendSysMesage("You received (+6) WhiteSteed.", MsgMessage.ChatMode.System);
#else
                                client.SendSysMesage("You received (+6) WhiteSteed.", MsgMessage.ChatMode.System);
#endif

                                break;
                            }
                        case 723865:// +6BlackSteedPack 
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.AddSteed(stream, 300000, 1, 6, false, 255, 0, 150);
#if Arabic
                                    client.SendSysMesage("You received (+6) BlackSteed.", MsgMessage.ChatMode.System);
#else
                                client.SendSysMesage("You received (+6) BlackSteed.", MsgMessage.ChatMode.System);
#endif

                                break;
                            }
                        case 729536:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                uint amount = (uint)Program.GetRandom.Next(20, 90);
                                client.Player.ConquerPoints += amount;
#if Arabic
                                 client.SendSysMesage("You`ve received " + amount.ToString() + " Conquer Points.");
#else
                                client.SendSysMesage("You`ve received " + amount.ToString() + " Conquer Points.");
#endif

                                break;
                            }
                        case 720665:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.ConquerPoints += 60;
                                client.Player.SendString(stream, MsgStringPacket.StringID.Effect, false, "eidolon");
                                client.SendSysMesage("You used the Cute CP Pack and received 60 CPs.");
                                break;
                            }

                        ////////////////////



                        case 3001405:
                            {
                                if (client.Inventory.HaveSpace(1))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.Add(stream, Database.ItemType.MoonBox, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                    client.CreateBoxDialog("You received 1 MoonBox.");

                                }
                                else
                                {
#if Arabic
                                     client.SendSysMesage("Please make 1 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 1 more spaces in your inventory.");
#endif

                                }
                                break;
                            }



                        case 722454:
                            {
                                if (!client.Inventory.HaveSpace(5))
                                {
#if Arabic
                                     client.CreateBoxDialog("Please make 5 more spaces in your inventory.");
#else
                                    client.CreateBoxDialog("Please make 5 more spaces in your inventory.");
#endif
                                    break;
                                }
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.AddItemWitchStack(Database.ItemType.ExpBall2, 0, 5, stream);
                                client.Inventory.AddItemWitchStack(723903, 0, 6, stream);
                                client.Inventory.Add(stream, 730002, 3, 2);
#if Arabic
                                client.CreateBoxDialog("You received 5 Exp Ball, 6 Saddles and 3 (+2)Stone!");
#else
                                client.CreateBoxDialog("You received 5 Exp Ball, 6 Saddles and 3 (+2)Stone!");
#endif
                                break;
                            }
                        case 722459:
                            {
                                if (!client.Inventory.HaveSpace(5))
                                {
#if Arabic
                                     client.CreateBoxDialog("Please make 5 more spaces in your inventory.");
#else
                                    client.CreateBoxDialog("Please make 5 more spaces in your inventory.");
#endif
                                    break;
                                }
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.AddItemWitchStack(Database.ItemType.ExpBall2, 0, 3, stream);//1
                                client.Inventory.AddItemWitchStack(723903, 0, 5, stream);//2
                                client.Inventory.Add(stream, 730002, 2, 2);
#if Arabic
                                client.CreateBoxDialog("You received 3 Exp Ball, 5 Saddles and 2 (+2)Stone!");
#else
                                client.CreateBoxDialog("You received 3 Exp Ball, 5 Saddles and 2 (+2)Stone!");
#endif
                                break;
                            }
                        case 720993:
                        case 711458:
                            {
                                if (client.Inventory.Contain(720993, 1) && client.Inventory.Contain(711458, 1))
                                {
                                    client.Inventory.Remove(720993, 1, stream);
                                    client.Inventory.Remove(711458, 1, stream);
                                    client.Inventory.Add(stream, 711459);
                                    client.SendSysMesage("The Hawk Blood revealed the Blank Paper. It`s a treasure map!", MsgMessage.ChatMode.System);
                                }
                                break;
                            }
                        case 720995:
                            {
                                client.Player.AddPick(stream, "Poison", 2);
                                break;
                            }
                        case 720994:
                            {
                                if (client.Inventory.Contain(711479, 1))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.Remove(711479, 1, stream);
                                    client.Inventory.Add(stream, 720995);
                                    client.SendSysMesage("You used the Poison Liquid on the Salted Fish. When the liquid diffuses into the Salted Fish completely, the Poison Fish small good still.", MsgMessage.ChatMode.System);
                                }

                                break;
                            }
                        case 720987:
                        case 720988:
                        case 720989:
                            {
                                if (client.Inventory.Contain(720987, 1) && client.Inventory.Contain(720988, 1) && client.Inventory.Contain(720989, 1))
                                {
                                    client.Inventory.Remove(720987, 1, stream);
                                    client.Inventory.Remove(720988, 1, stream);
                                    client.Inventory.Remove(720989, 1, stream);
                                    client.Inventory.Add(stream, 711452);
                                }
                                break;
                            }
                        case 720962:
                            {
                                if (!client.Inventory.HaveSpace(1))
                                {
                                    client.CreateBoxDialog("Please make 1 more space in your inventory.");
                                    break;
                                }
                                if (client.Inventory.Contain(711401, 1))
                                {
                                    client.CreateBoxDialog("You`ve~collected~enough~Desert~Herbs.");
                                    break;
                                }
                                client.Inventory.Add(stream, 711401);
                                client.CreateBoxDialog("You`ve~received~a~Desert~Herb!");
                                break;
                            }
                        case 720974:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 711348);
                                client.CreateBoxDialog("You`ve~collect~a~bucket~of~water.~Hurry~to~deliver~it~to~Han~Cheng.");
                                break;
                            }
                        case 722464:
                            {
                                if (!client.Inventory.HaveSpace(5))
                                {
#if Arabic
                                     client.CreateBoxDialog("Please make 5 more spaces in your inventory.");
#else
                                    client.CreateBoxDialog("Please make 5 more spaces in your inventory.");
#endif
                                    break;
                                }
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.AddItemWitchStack(Database.ItemType.ExpBall2, 0, 3, stream);//1
                                client.Inventory.AddItemWitchStack(723903, 0, 4, stream);//2
                                client.Inventory.Add(stream, 730001, 3, 1);
#if Arabic
                                client.CreateBoxDialog("You received 3 Exp Ball, 4 Saddles and 3 (+1)Stone!");
#else
                                client.CreateBoxDialog("You received 3 Exp Ball, 4 Saddles and 3 (+1)Stone!");
#endif
                                break;
                            }
                        case 722469:
                            {
                                if (!client.Inventory.HaveSpace(5))
                                {
#if Arabic
                                     client.CreateBoxDialog("Please make 5 more spaces in your inventory.");
#else
                                    client.CreateBoxDialog("Please make 5 more spaces in your inventory.");
#endif
                                    break;
                                }
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.AddItemWitchStack(Database.ItemType.ExpBall2, 0, 2, stream);//1
                                client.Inventory.AddItemWitchStack(723903, 0, 3, stream);//2
                                client.Inventory.Add(stream, 730001, 2, 1);
#if Arabic
                                client.CreateBoxDialog("You received 2 Exp Ball, 3 Saddles and 2 (+1)Stone!");
#else
                                client.CreateBoxDialog("You received 2 Exp Ball, 3 Saddles and 2 (+1)Stone!");
#endif
                                break;
                            }
                        case 722474:
                            {
                                if (!client.Inventory.HaveSpace(5))
                                {
#if Arabic
                                     client.CreateBoxDialog("Please make 5 more spaces in your inventory.");
#else
                                    client.CreateBoxDialog("Please make 5 more spaces in your inventory.");
#endif
                                    break;
                                }
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.AddItemWitchStack(Database.ItemType.ExpBall2, 0, 1, stream);//1
                                client.Inventory.AddItemWitchStack(723903, 0, 3, stream);//2
                                client.Inventory.Add(stream, 730001, 1, 1);
#if Arabic
                                client.CreateBoxDialog("You received 1 Exp Ball, 3 Saddles and 1 (+1)Stone!");
#else
                                client.CreateBoxDialog("You received 1 Exp Ball, 3 Saddles and 1 (+1)Stone!");
#endif
                                break;
                            }


                        case 3001133: //HopeCPPack
                            {

                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.ConquerPoints += 5;
                                client.CreateBoxDialog("You have received 5 ConquerPoints.");
                                break;
                            }



                        case 3001134: //MascotCPPack
                            {

                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.ConquerPoints += 10;
                                client.CreateBoxDialog("You have received 10 ConquerPoints.");
                                break;
                            }



                        case 3001135: //MammonCPPack
                            {

                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.ConquerPoints += 20;
                                client.CreateBoxDialog("You have received 20 ConquerPoints.");
                                break;
                            }



                        case 3001136: //DeityCPPack
                            {

                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.ConquerPoints += 500;
                                client.CreateBoxDialog("You have received 500 ConquerPoints.");
                                break;
                            }

                        case 3001065: //+3BlackSteedPack
                            {

                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.AddSteed(stream, 300000, 1, 3, true, 255, 0, 150);
                                client.CreateBoxDialog("You have received  +3 Black Steed(Bound).");
                                break;
                            }


                        case 3001060: //+1MaroonSteedPack 
                            {

                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.AddSteed(stream, 300000, 1, 1, true, 0, 150, 255);
                                client.CreateBoxDialog("You have received  +1 Maroon Steed(Bound).");
                                break;
                            }


                        case 3001061: //+1WhiteSteedPack 
                            {

                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.AddSteed(stream, 300000, 1, 1, true, 150, 255, 0);
                                client.CreateBoxDialog("You have received  +1 White Steed(Bound).");
                                break;
                            }




                        case 3001062: //+1BlackSteedPack
                            {

                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.AddSteed(stream, 300000, 1, 1, true, 255, 0, 150);
                                client.CreateBoxDialog("You have received  +1 Black Steed(Bound).");
                                break;
                            }


                        case 3001063: //+3MaroonSteedPack 
                            {

                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.AddSteed(stream, 300000, 1, 3, true, 0, 150, 255);
                                client.CreateBoxDialog("You have received  +3 Maroon Steed(Bound).");
                                break;
                            }



                        case 3001064: //+3WhiteSteedPack 
                            {

                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.AddSteed(stream, 300000, 1, 3, true, 150, 255, 0);
                                client.CreateBoxDialog("You have received  +3 White Steed(Bound).");
                                break;
                            }


                        case 729462: //6,999CPPack(B)
                            {

                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.BoundConquerPoints += 6999;
                                client.CreateBoxDialog("You have received 6,999 (B)ConquerPoints.");
                                break;
                            }


                        case 729463: //4,999CPPack(B)
                            {

                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.BoundConquerPoints += 4999;
                                client.CreateBoxDialog("You have received 4,999 (B)ConquerPoints.");
                                break;
                            }





                        case 729464: //2,999CPPack(B)
                            {

                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.BoundConquerPoints += 2999;
                                client.CreateBoxDialog("You have received 2,999 (B)ConquerPoints.");
                                break;
                            }


                        case 729465: //599CPPack(B)
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.BoundConquerPoints += 599;
                                client.CreateBoxDialog("You have received 599 (B)ConquerPoints.");
                                break;
                            }


                        case 729466: //99CPPack(B)
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.BoundConquerPoints += 99;
                                client.CreateBoxDialog("You have received 99 (B)ConquerPoints.");
                                break;
                            }



                        case 3200522: //60CPsPack(B)
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.BoundConquerPoints += 60;
                                client.CreateBoxDialog("You have received 60 (B)ConquerPoints.");
                                break;
                            }

                        case 3200523: //125CPsPack(B)
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.BoundConquerPoints += 125;
                                client.CreateBoxDialog("You have received 125 (B)ConquerPoints.");
                                break;
                            }


                        case 3200524: //320CPsPack(B)
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.BoundConquerPoints += 320;
                                client.CreateBoxDialog("You have received 320 (B)ConquerPoints.");
                                break;
                            }


                        case 3200525: //530CPsPack(B)
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.BoundConquerPoints += 530;
                                client.CreateBoxDialog("You have received 530 (B)ConquerPoints.");
                                break;
                            }



                        case 3200526: //1075CPsPack(B)
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.BoundConquerPoints += 1075;
                                client.CreateBoxDialog("You have received 1075 (B)ConquerPoints.");
                                break;
                            }


                        case 3200527: //2050CPsPack(B)
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.BoundConquerPoints += 2050;
                                client.CreateBoxDialog("You have received 2050 (B)ConquerPoints.");
                                break;
                            }



                        case 3200528: //4200 CPsPack(B)
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.BoundConquerPoints += 4200;
                                client.CreateBoxDialog("You have received 4200 (B)ConquerPoints.");
                                break;
                            }



                        case 3200700: //20 CPsPack(B)
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.BoundConquerPoints += 20;
                                client.CreateBoxDialog("You have received 20 (B)ConquerPoints.");
                                break;
                            }


                        case 3200701: //300 CPsPack(B)
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.BoundConquerPoints += 300;
                                client.CreateBoxDialog("You have received 300 (B)ConquerPoints.");
                                break;
                            }


                        case 3200702: //500 CPsPack(B)
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.BoundConquerPoints += 500;
                                client.CreateBoxDialog("You have received 500 (B)ConquerPoints.");
                                break;
                            }



                        case 3200703: //5000 CPsPack(B)
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.BoundConquerPoints += 5000;
                                client.CreateBoxDialog("You have received 5000 (B)ConquerPoints.");
                                break;
                            }


                        case 3200704: //50 CPsPack(B)
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.BoundConquerPoints += 50;
                                client.CreateBoxDialog("You have received 50 (B)ConquerPoints.");
                                break;
                            }



                        case 3200705: //600 CPsPack(B)
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.BoundConquerPoints += 600;
                                client.CreateBoxDialog("You have received 600 (B)ConquerPoints.");
                                break;
                            }

                        case 720698:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.GainExpBall(3200);
                                client.Player.SendString(stream, MsgStringPacket.StringID.Effect, false, "eidolon");
                                client.CreateBoxDialog("You used the Moon Pill and received the EXP worth 8 and 1/3 EXP Balls!");
                                break;
                            }

                        case 720692:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.GainExpBall(3000, true, Role.Flags.ExperienceEffect.angelwing);
                                client.Player.SendString(stream, MsgStringPacket.StringID.Effect, false, "eidolon");
                                client.CreateBoxDialog("You used the Wind Pill and received the EXP worth 5 EXP Balls!");


                                break;
                            }

                        case 720686:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.GainExpBall(300);
                                client.Player.SendString(stream, MsgStringPacket.StringID.Effect, false, "eidolon");
                                client.CreateBoxDialog("You used the Mystery Pill and received EXP worth 3 and 1/3 EXP Balls!");

                                break;
                            }



                        case 3200322:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (client.Player.Level < 137)
                                {
                                    client.CreateBoxDialog("You received~1250~minutes~of~EXP");

                                    client.GainExpBall(12500, true, Role.Flags.ExperienceEffect.angelwing);

                                }
                                break;
                            }

                        case 3200323:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (client.Player.Level < 137)
                                {
                                    client.CreateBoxDialog("You received~1125~minutes~of~EXP");

                                    client.GainExpBall(11250, true, Role.Flags.ExperienceEffect.angelwing);

                                }
                                break;
                            }


                        case 3200324:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (client.Player.Level < 137)
                                {
                                    client.CreateBoxDialog("You received~875~minutes~of~EXP");

                                    client.GainExpBall(8750, true, Role.Flags.ExperienceEffect.angelwing);

                                }
                                break;
                            }



                        case 3200325:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (client.Player.Level < 137)
                                {
                                    client.CreateBoxDialog("You received~500~minutes~of~EXP");

                                    client.GainExpBall(5000, true, Role.Flags.ExperienceEffect.angelwing);

                                }

                                break;
                            }


                        case 3200330:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (client.Player.Level < 137)
                                {
                                    client.CreateBoxDialog("You received~125~minutes~of~EXP");

                                    client.GainExpBall(1250, true, Role.Flags.ExperienceEffect.angelwing);

                                }
                                break;
                            }


                        case 3200331:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (client.Player.Level < 137)
                                {
                                    client.CreateBoxDialog("You received~250~minutes~of~EXP");

                                    client.GainExpBall(2050, true, Role.Flags.ExperienceEffect.angelwing);

                                }

                                break;
                            }


                        case 3200332:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (client.Player.Level < 137)
                                {
                                    client.CreateBoxDialog("You received~375~minutes~of~EXP");

                                    client.GainExpBall(3750, true, Role.Flags.ExperienceEffect.angelwing);

                                }
                                break;
                            }


                        case 3200333:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (client.Player.Level < 137)
                                {
                                    client.CreateBoxDialog("You received~150~minutes~of~EXP");

                                    client.GainExpBall(1500, true, Role.Flags.ExperienceEffect.angelwing);

                                }
                                break;
                            }
                        case 3200334:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (client.Player.Level < 137)
                                {
                                    client.CreateBoxDialog("You received~300~minutes~of~EXP");

                                    client.GainExpBall(3000, true, Role.Flags.ExperienceEffect.angelwing);

                                }
                                break;
                            }

                        case 3200335:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (client.Player.Level < 137)
                                {
                                    client.CreateBoxDialog("You received~450~minutes~of~EXP");

                                    client.GainExpBall(4500, true, Role.Flags.ExperienceEffect.angelwing);

                                }

                                break;
                            }


                        case 3200336:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (client.Player.Level < 137)
                                {
                                    client.CreateBoxDialog("You received~600~minutes~of~EXP");

                                    client.GainExpBall(6000, true, Role.Flags.ExperienceEffect.angelwing);

                                }
                                break;
                            }

                        case 3200337:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (client.Player.Level < 137)
                                {
                                    client.CreateBoxDialog("You received~300~minutes~of~EXP");

                                    client.GainExpBall(3000, true, Role.Flags.ExperienceEffect.angelwing);

                                }
                                break;
                            }



                        case 3200338:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (client.Player.Level < 137)
                                {
                                    client.CreateBoxDialog("You received~600~minutes~of~EXP");

                                    client.GainExpBall(6000, true, Role.Flags.ExperienceEffect.angelwing);

                                }
                                break;
                            }




                        case 3200339:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (client.Player.Level < 137)
                                {
                                    client.CreateBoxDialog("You received~900~minutes~of~EXP");

                                    client.GainExpBall(9000, true, Role.Flags.ExperienceEffect.angelwing);

                                }
                                break;
                            }


                        case 3200340:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (client.Player.Level < 137)
                                {
                                    client.CreateBoxDialog("You received~180~minutes~of~EXP");

                                    client.GainExpBall(1800, true, Role.Flags.ExperienceEffect.angelwing);

                                }
                                break;
                            }





                        case 3200341:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (client.Player.Level < 137)
                                {
                                    client.CreateBoxDialog("You received~240~minutes~of~EXP");

                                    client.GainExpBall(2400, true, Role.Flags.ExperienceEffect.angelwing);

                                }
                                break;
                            }

                        case 3200344:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (client.Player.Level < 137)
                                {
                                    client.CreateBoxDialog("You received~30~minutes~of~EXP");

                                    client.GainExpBall(300, true, Role.Flags.ExperienceEffect.angelwing);

                                }
                                break;
                            }

                        case 3200345:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (client.Player.Level < 137)
                                {
                                    client.CreateBoxDialog("You received~90~minutes~of~EXP");

                                    client.GainExpBall(900, true, Role.Flags.ExperienceEffect.angelwing);

                                }
                                break;
                            }

                        case 3200346:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                if (client.Player.Level < 137)
                                {
                                    client.CreateBoxDialog("You received~120~minutes~of~EXP");

                                    client.GainExpBall(1200, true, Role.Flags.ExperienceEffect.angelwing);

                                }
                                break;
                            }
                        case 3008992:
                            {
                                if (!client.Inventory.HaveSpace(1))
                                {
                                    client.CreateBoxDialog("Please make 1 more space in your inventory.");
                                    break;
                                }
                                client.Inventory.Remove(3008992, 1, stream);
                                byte rand = (byte)Program.GetRandom.Next(0, 5);
                                switch (rand)
                                {
                                    case 0:
                                        {
                                            client.Inventory.AddItemWitchStack(3008727, 0, 1, stream);
                                            client.SendSysMesage("The Reasure of Dragon turned into a flash of light, and you received 1 Blazing CP Fragment!", MsgMessage.ChatMode.System);
                                            break;
                                        }
                                    case 1:
                                        {
                                            client.Inventory.Add(stream, 3009000);
                                            client.SendSysMesage("The Reasure of Dragon turned into a flash of light, and you received 2 Twilight Star Stones!", MsgMessage.ChatMode.System);
                                            break;
                                        }
                                    case 4:
                                        {
                                            client.Inventory.AddItemWitchStack(3006284, 0, 1, stream);
                                            client.SendSysMesage("The Reasure of Dragon turned into a flash of light, and you received 1 Mystery Dew Scrap!", MsgMessage.ChatMode.System);
                                            break;
                                        }
                                }
                                break;
                            }
                        case 3006284:
                            {
                                if (!client.Inventory.HaveSpace(1))
                                {
                                    client.CreateBoxDialog("Please make 1 more space in your inventory.");
                                    break;
                                }
                                if (client.Inventory.Contain(3006284, 10))
                                {
                                    client.Inventory.Add(stream, 3001045);
                                    client.CreateBoxDialog("You successfully combined 10*MysteryDewScrap into 1 MysteryDew.");
                                }
                                else
                                    client.CreateBoxDialog("Failed to combine. You should have at least 10 Mystery Dew Scraps to combine.");
                                break;
                            }
                        case 721316:
                            {
                                if (!client.Inventory.HaveSpace(1))
                                {
#if Arabic
                                     client.CreateBoxDialog("Please make 1 more spaces in your inventory.");
#else
                                    client.CreateBoxDialog("Please make 1 more spaces in your inventory.");
#endif
                                    break;
                                }
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.AddItemWitchStack(Database.ItemType.ExpBall2, 0, 3, stream); ;
#if Arabic
                                  client.CreateBoxDialog("You received 3 Exp Balls.");
#else
                                client.CreateBoxDialog("You received 3 Exp Balls.");
#endif
                                break;
                            }



                        case 721317:
                            {
                                if (!client.Inventory.HaveSpace(1))
                                {
#if Arabic
                                     client.CreateBoxDialog("Please make 1 more spaces in your inventory.");
#else
                                    client.CreateBoxDialog("Please make 1 more spaces in your inventory.");
#endif
                                    break;
                                }
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.AddItemWitchStack(Database.ItemType.ExpBall2, 0, 3, stream);
#if Arabic
                                  client.CreateBoxDialog("You received 3 Exp Balls.");
#else
                                client.CreateBoxDialog("You received 3 Exp Balls.");
#endif
                                break;
                            }





                        case 3003132:
                            {
                                if (!client.Inventory.HaveSpace(1))
                                {
#if Arabic
                                     client.CreateBoxDialog("Please make 1 more spaces in your inventory.");
#else
                                    client.CreateBoxDialog("Please make 1 more spaces in your inventory.");
#endif
                                    break;
                                }
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 3003124, 1);

#if Arabic
                                 client.CreateBoxDialog("You received 1 Favored~Training~Pill");
#else
                                client.CreateBoxDialog("You received 1 Favored~Training~Pill");
#endif
                                break;
                            }


                        case 3003133:
                            {
                                if (!client.Inventory.HaveSpace(2))
                                {
#if Arabic
                                     client.CreateBoxDialog("Please make 2 more spaces in your inventory.");
#else
                                    client.CreateBoxDialog("Please make 2 more spaces in your inventory.");
#endif
                                    break;
                                }
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 3003124, 2);

#if Arabic
                                 client.CreateBoxDialog("You received 1 Favored~Training~Pill");
#else
                                client.CreateBoxDialog("You received 2 Favored~Training~Pill");
#endif
                                break;
                            }

                        case 3003134:
                            {
                                if (!client.Inventory.HaveSpace(1))
                                {
#if Arabic
                                     client.CreateBoxDialog("Please make 1 more spaces in your inventory.");
#else
                                    client.CreateBoxDialog("Please make 1 more spaces in your inventory.");
#endif
                                    break;
                                }
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.AddItemWitchStack(3003124, 0, 5, stream);

#if Arabic
                                 client.CreateBoxDialog("You received 5 Favored~Training~Pill");
#else
                                client.CreateBoxDialog("You received 5 Favored~Training~Pill");
#endif
                                break;
                            }

                        case 3003135:
                            {
                                if (!client.Inventory.HaveSpace(2))
                                {
#if Arabic
                                     client.CreateBoxDialog("Please make 1 more spaces in your inventory.");
#else
                                    client.CreateBoxDialog("Please make 2 more spaces in your inventory.");
#endif
                                    break;
                                }
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.AddItemWitchStack(3003124, 0, 10, stream);

#if Arabic
                                 client.CreateBoxDialog("You received 10 Favored~Training~Pill");
#else
                                client.CreateBoxDialog("You received 10 Favored~Training~Pill");
#endif
                                break;
                            }



                        case 3003136:
                            {
                                if (!client.Inventory.HaveSpace(4))
                                {
#if Arabic
                                     client.CreateBoxDialog("Please make 4 more spaces in your inventory.");
#else
                                    client.CreateBoxDialog("Please make 4 more spaces in your inventory.");
#endif
                                    break;
                                }
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.AddItemWitchStack(3003124, 0, 20, stream);

#if Arabic
                                 client.CreateBoxDialog("You received 20 Favored~Training~Pill");
#else
                                client.CreateBoxDialog("You received 20 Favored~Training~Pill");
#endif
                                break;
                            }



                        case 3003137:
                            {
                                if (!client.Inventory.HaveSpace(5))
                                {
#if Arabic
                                     client.CreateBoxDialog("Please make 5 more spaces in your inventory.");
#else
                                    client.CreateBoxDialog("Please make 5 more spaces in your inventory.");
#endif
                                    break;
                                }
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.AddItemWitchStack(3003124, 0, 50, stream);

#if Arabic
                                 client.CreateBoxDialog("You received 50 Favored~Training~Pill");
#else
                                client.CreateBoxDialog("You received 50 Favored~Training~Pill");
#endif
                                break;
                            }



                        case 720892:
                            {
                                if (!client.Inventory.HaveSpace(3))
                                {
#if Arabic
                                     client.CreateBoxDialog("Please make 3 more spaces in your inventory.");
#else
                                    client.CreateBoxDialog("Please make 3 more spaces in your inventory.");
#endif
                                    break;
                                }

                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 700073, 3);

#if Arabic
                                      client.CreateBoxDialog("You received 3~Super~Tortoise~Gems.");
#else
                                client.CreateBoxDialog("You received 3~Super~Tortoise~Gems.");
#endif
                                break;
                            }








                        case 27832:
                            {
                                if (client.Inventory.HaveSpace(1))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.AddItemWitchStack(723342, 0, 8, stream);
                                    client.CreateBoxDialog("You received 8~Modesty~Books!");

                                }
                                else
                                {
#if Arabic
                                     client.SendSysMesage("Please make 1 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 1 more spaces in your inventory.");
#endif

                                }
                                break;
                            }


                        case 727832:
                            {
                                if (client.Inventory.HaveSpace(1))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.AddItemWitchStack(723342, 0, 8, stream);
                                    client.CreateBoxDialog("You received 8~Modesty~Books!");

                                }
                                else
                                {
#if Arabic
                                     client.SendSysMesage("Please make 1 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 1 more spaces in your inventory.");
#endif

                                }
                                break;
                            }


                        case 727833:
                            {
                                if (client.Inventory.HaveSpace(1))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.AddItemWitchStack(723342, 0, 6, stream);
                                    client.CreateBoxDialog("You received 6~Modesty~Books!");

                                }
                                else
                                {
#if Arabic
                                     client.SendSysMesage("Please make 1 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 1 more spaces in your inventory.");
#endif

                                }
                                break;
                            }

                        case 727834:
                            {
                                if (client.Inventory.HaveSpace(1))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.AddItemWitchStack(723342, 0, 4, stream);
                                    client.CreateBoxDialog("You received 4~Modesty~Books!");

                                }
                                else
                                {
#if Arabic
                                     client.SendSysMesage("Please make 1 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 1 more spaces in your inventory.");
#endif

                                }
                                break;
                            }


                        case 727831:
                            {
                                if (client.Inventory.HaveSpace(1))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.AddItemWitchStack(723342, 0, 10, stream);
                                    client.CreateBoxDialog("You received 10~Modesty~Books!");

                                }
                                else
                                {
#if Arabic
                                     client.SendSysMesage("Please make 1 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 1 more spaces in your inventory.");
#endif

                                }
                                break;
                            }

                        case 720668:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.GainExpBall(100);
                                client.CreateBoxDialog("You used the Magic Ball and received EXP worth 1/6 of an EXP Ball from Demon Box.");
                                break;
                            }


                        case 720669:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.GainExpBall(500);
                                client.CreateBoxDialog("You used the Super Ball and received EXP worth 5/6 of an EXP Ball.");
                                break;
                            }


                        case 720670:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.GainExpBall(150);
                                client.Player.SendString(stream, MsgStringPacket.StringID.Effect, false, "eidolon");
                                client.CreateBoxDialog("You used the Ultra Ball and received EXP worth 1 and 2/3 EXP Balls!");
                                break;
                            }
                        case 3100097:
                            {
                                if (client.Inventory.HaveSpace(1))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);


                                    if (client.Player.Class >= 10 && client.Player.Class <= 15)
                                    {
                                        client.Inventory.Add(stream, 118008, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                        client.CreateBoxDialog("You received 1 GuardCoronet(Elite).");
                                    }

                                    if (client.Player.Class >= 20 && client.Player.Class <= 25)
                                    {
                                        client.Inventory.Add(stream, 111008, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                        client.CreateBoxDialog("You received 1 IronHelmet(Elite).");
                                    }


                                    if (client.Player.Class >= 40 && client.Player.Class <= 45)
                                    {
                                        client.Inventory.Add(stream, 113008, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                        client.CreateBoxDialog("You received 1 BadgerHat(Elite).");
                                    }


                                    if (client.Player.Class >= 50 && client.Player.Class <= 55)
                                    {
                                        client.Inventory.Add(stream, 123008, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                        client.CreateBoxDialog("You received 1 CottonHood(Elite).");
                                    }

                                    if (client.Player.Class >= 60 && client.Player.Class <= 65)
                                    {
                                        client.Inventory.Add(stream, 143008, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                        client.CreateBoxDialog("You received 1 BronzeHeadband(Elite).");
                                    }


                                    if (client.Player.Class >= 70 && client.Player.Class <= 75)
                                    {
                                        client.Inventory.Add(stream, 145008, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                        client.CreateBoxDialog("You received 1 TidalHood(Elite).");
                                    }



                                    if (client.Player.Class >= 80 && client.Player.Class <= 85)
                                    {
                                        client.Inventory.Add(stream, 148008, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                        client.CreateBoxDialog("You received 1 LinenHood(Elite).");
                                    }

                                    if (client.Player.Class >= 101 && client.Player.Class <= 145)
                                    {
                                        client.Inventory.Add(stream, 114008, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                        client.CreateBoxDialog("You received 1 DestinyCap(Elite).");
                                    }



                                }
                                else
                                {
#if Arabic
                                     client.SendSysMesage("Please make 1 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 1 more spaces in your inventory.");
#endif

                                }
                                break;
                            }


                        case 3100098:
                            {
                                if (client.Inventory.HaveSpace(2))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);


                                    if (client.Player.Class >= 10 && client.Player.Class <= 15)
                                    {
                                        client.Inventory.Add(stream, 420018, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true); //sword
                                        client.Inventory.Add(stream, 410028, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true); //blade
                                        client.CreateBoxDialog("You received 1 SpringSword(Elite) and 1 DemonBlade(Elite).");
                                    }

                                    if (client.Player.Class >= 20 && client.Player.Class <= 25)
                                    {
                                        client.Inventory.Add(stream, 561028, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                        client.CreateBoxDialog("You received 1 IronwoodWand(Elite).");
                                    }


                                    if (client.Player.Class >= 40 && client.Player.Class <= 45)
                                    {
                                        client.Inventory.Add(stream, 500018, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                        client.CreateBoxDialog("You received 1 HuntingBow(Elite).");
                                    }


                                    if (client.Player.Class >= 50 && client.Player.Class <= 55)
                                    {
                                        client.Inventory.Add(stream, 601028, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                        client.Inventory.Add(stream, 601028, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                        client.CreateBoxDialog("You received 2 SteelKatana(Elite).");
                                    }

                                    if (client.Player.Class >= 60 && client.Player.Class <= 65)
                                    {
                                        client.Inventory.Add(stream, 610028, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                        client.Inventory.Add(stream, 610028, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                        client.CreateBoxDialog("You received 2 SandalwoodPrayerBeads(Elite).");
                                    }


                                    if (client.Player.Class >= 70 && client.Player.Class <= 75)
                                    {
                                        client.Inventory.Add(stream, 612028, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true); //TidePistol
                                        client.Inventory.Add(stream, 611028, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                        client.CreateBoxDialog("You received 1 TidePistol(Elite) and 1 ValorRapier(Elite).");
                                    }



                                    if (client.Player.Class >= 80 && client.Player.Class <= 85)
                                    {
                                        client.Inventory.Add(stream, 617028, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                        client.Inventory.Add(stream, 617028, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                        client.CreateBoxDialog("You received 2 JujubeWoodNunchaku(Elite).");
                                    }

                                    if (client.Player.Class >= 101 && client.Player.Class <= 145)
                                    {
                                        client.Inventory.Add(stream, 421028, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                        client.Inventory.Add(stream, 561028, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                        client.CreateBoxDialog("You received 1 IronwoodWand(Elite) and 1 JustBacksword(Elite).");
                                    }



                                }
                                else
                                {
#if Arabic
                                     client.SendSysMesage("Please make 2 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 2 more spaces in your inventory.");
#endif

                                }
                                break;
                            }




                        case 3100100:
                            {
                                if (client.Inventory.HaveSpace(1))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);


                                    if (client.Player.Class >= 10 && client.Player.Class <= 15)
                                    {
                                        client.Inventory.Add(stream, 130008, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                        client.CreateBoxDialog("You received 1 LeatherArmor(Elite).");
                                    }

                                    if (client.Player.Class >= 20 && client.Player.Class <= 25)
                                    {
                                        client.Inventory.Add(stream, 131008, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                        client.CreateBoxDialog("You received 1 OxhideArmor(Elite).");
                                    }


                                    if (client.Player.Class >= 40 && client.Player.Class <= 45)
                                    {
                                        client.Inventory.Add(stream, 133008, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                        client.CreateBoxDialog("You received 1 DeerskinCoat(Elite).");
                                    }


                                    if (client.Player.Class >= 50 && client.Player.Class <= 55)
                                    {
                                        client.Inventory.Add(stream, 135008, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                        client.CreateBoxDialog("You received 1 LowerNinjaVest(Elite).");
                                    }

                                    if (client.Player.Class >= 60 && client.Player.Class <= 65)
                                    {
                                        client.Inventory.Add(stream, 136008, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                        client.CreateBoxDialog("You received 1 BurlapFrock(Elite).");
                                    }


                                    if (client.Player.Class >= 70 && client.Player.Class <= 75)
                                    {
                                        client.Inventory.Add(stream, 139008, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                        client.CreateBoxDialog("You received 1 RecruitCoat(Elite).");
                                    }



                                    if (client.Player.Class >= 80 && client.Player.Class <= 85)
                                    {
                                        client.Inventory.Add(stream, 138008, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                        client.CreateBoxDialog("You received 1 CombatSuit(Elite).");
                                    }

                                    if (client.Player.Class >= 101 && client.Player.Class <= 145)
                                    {
                                        client.Inventory.Add(stream, 134008, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                        client.CreateBoxDialog("You received 1 TaoRobe(Elite).");
                                    }



                                }
                                else
                                {
#if Arabic
                                     client.SendSysMesage("Please make 1 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 1 more spaces in your inventory.");
#endif

                                }
                                break;
                            }





                        case 3100102:
                            {
                                if (client.Inventory.HaveSpace(1))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);



                                    client.Inventory.Add(stream, 120028, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                    client.CreateBoxDialog("You received 1 HeartNecklace(Elite).");

                                }
                                else
                                {
#if Arabic
                                     client.SendSysMesage("Please make 1 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 1 more spaces in your inventory.");
#endif

                                }
                                break;
                            }


                        case 3100099:
                            {
                                if (client.Inventory.HaveSpace(1))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);



                                    client.Inventory.Add(stream, 152018, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                    client.CreateBoxDialog("You received 1 PeachBracelet(Elite).");

                                }
                                else
                                {
#if Arabic
                                     client.SendSysMesage("Please make 1 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 1 more spaces in your inventory.");
#endif

                                }
                                break;
                            }




                        case 721203:
                            {
                                if (client.Player.Level > 50)
                                {
                                    if (client.Inventory.HaveSpace(4))
                                    {
                                        client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);



                                        client.Inventory.Add(stream, 143009, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                        client.Inventory.Add(stream, 136009, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                        client.Inventory.Add(stream, 610019, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                        client.Inventory.Add(stream, 610019, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                        client.CreateBoxDialog("You received some gifts specially for your class.");

                                    }
                                    else
                                    {
#if Arabic
                                      client.CreateBoxDialog("Please make 4 more spaces in your inventory.");
#else

                                        client.CreateBoxDialog("Please make 4 more spaces in your inventory.");
#endif

                                    }
                                }
                                else
                                {
#if Arabic
                                      client.CreateBoxDialog("You must be atleast level 50 to let you know my secrets.");
#else
                                    client.CreateBoxDialog("You must be atleast level 50 to let you know my secrets.");
#endif

                                }
                                break;
                            }







                        case 3100101:
                            {
                                if (client.Inventory.HaveSpace(1))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);



                                    client.Inventory.Add(stream, 160018, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                    client.CreateBoxDialog("You received 1 OxhideBoots(Elite).");

                                }
                                else
                                {
#if Arabic
                                     client.SendSysMesage("Please make 1 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 1 more spaces in your inventory.");
#endif

                                }
                                break;
                            }



                        case 723826:
                            {
                                if (client.Inventory.HaveSpace(6))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);



                                    client.Inventory.Add(stream, 160138, 0, 0, 3, 0, Role.Flags.Gem.EmptySocket, Role.Flags.Gem.NoSocket, false); //bots -3 & elite (B)
                                    client.Inventory.Add(stream, 723712, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); // +1 Pack - non bound
                                    client.Inventory.AddItemWitchStack(3001283, 0, 3, stream);   // 3 ExpBalls
                                    client.Inventory.AddItemWitchStack(723711, 0, 5, stream);  //// MeteorTearPack - 5
                                    client.CreateBoxDialog("You received~1~Socket~Lv70EliteBoots(-3%),~a~+1~StonePack,~3~EXPBalls(F),~a~7~Days`~Blessing~Stone(F),~and~5~Meteor~Tear~Packs.");

                                }
                                else
                                {
#if Arabic
                                     client.SendSysMesage("Please make 6 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 6 more spaces in your inventory.");
#endif

                                }
                                break;
                            }




                        case 723827:
                            {
                                if (client.Inventory.HaveSpace(6))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);



                                    client.Inventory.Add(stream, 120128, 0, 0, 3, 0, Role.Flags.Gem.EmptySocket, Role.Flags.Gem.NoSocket, false); //Necklace -3 & elite 
                                    client.Inventory.Add(stream, 723712, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); // +1 Pack - non bound
                                    client.Inventory.AddItemWitchStack(3001283, 0, 3, stream);   // 3 ExpBalls
                                    client.Inventory.AddItemWitchStack(723711, 0, 5, stream);  //// MeteorTearPack - 5
                                    client.CreateBoxDialog("You received~1~Socket~Lv67~Elite~Necklace(-3%),~a~+1~StonePack,~3~EXPBalls(F),~a~7~Days`~Blessing~Stone(F),~and~5~Meteor~Tear~Packs.");

                                }
                                else
                                {
#if Arabic
                                     client.SendSysMesage("Please make 6 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 6 more spaces in your inventory.");
#endif

                                }
                                break;
                            }



                        case 723828:
                            {
                                if (client.Inventory.HaveSpace(6))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);



                                    client.Inventory.Add(stream, 150138, 0, 0, 3, 0, Role.Flags.Gem.EmptySocket, Role.Flags.Gem.NoSocket, false); //Ring -3 & elite 
                                    client.Inventory.Add(stream, 723712, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); // +1 Pack - non bound
                                    client.Inventory.AddItemWitchStack(3001283, 0, 3, stream);   // 3 ExpBalls
                                    client.Inventory.AddItemWitchStack(723711, 0, 5, stream);  //// MeteorTearPack - 5
                                    client.CreateBoxDialog("You received~1~Socket~Lv70~EliteRing(-3%),~a~+1~StonePack,~3~EXPBalls(F),~a~7~Days`~Blessing~Stone(F),~and~5~Meteor~Tear~Packs.");

                                }
                                else
                                {
#if Arabic
                                     client.SendSysMesage("Please make 6 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 6 more spaces in your inventory.");
#endif

                                }
                                break;
                            }


                        case 723829:
                            {
                                if (client.Inventory.HaveSpace(6))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);



                                    client.Inventory.Add(stream, 121128, 0, 0, 3, 0, Role.Flags.Gem.EmptySocket, Role.Flags.Gem.NoSocket, false); //Bag -3 & elite 
                                    client.Inventory.Add(stream, 723712, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); // +1 Pack - non bound
                                    client.Inventory.AddItemWitchStack(3001283, 0, 3, stream);   // 3 ExpBalls
                                    client.Inventory.AddItemWitchStack(723711, 0, 5, stream);  //// MeteorTearPack - 5
                                    client.CreateBoxDialog("You received~1~Socket~Lv67~Elite~Bag(-3%),~a~+1~StonePack,~3~EXPBalls(F),~a~7~Days`~Blessing~Stone(F),~and~5~Meteor~Tear~Packs.");

                                }
                                else
                                {
#if Arabic
                                     client.SendSysMesage("Please make 6 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 6 more spaces in your inventory.");
#endif

                                }
                                break;
                            }


                        case 723830:
                            {
                                if (client.Inventory.HaveSpace(6))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);



                                    client.Inventory.Add(stream, 152148, 0, 0, 3, 0, Role.Flags.Gem.EmptySocket, Role.Flags.Gem.NoSocket, false); //Bracelet -3 & elite 
                                    client.Inventory.Add(stream, 723712, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); // +1 Pack - non bound
                                    client.Inventory.AddItemWitchStack(3001283, 0, 3, stream);   // 3 ExpBalls
                                    client.Inventory.AddItemWitchStack(723711, 0, 5, stream);  //// MeteorTearPack - 5
                                    client.CreateBoxDialog("You received~1~Socket~Lv75EliteBracelet(-3%),~a~+1~StonePack,~3~EXPBalls(F),~a~7~Days`~Blessing~Stone(F),~and~5~Meteor~Tear~Packs.");

                                }
                                else
                                {
#if Arabic
                                     client.SendSysMesage("Please make 6 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 6 more spaces in your inventory.");
#endif

                                }
                                break;
                            }


                        case 723831:
                            {
                                if (client.Inventory.HaveSpace(6))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);



                                    client.Inventory.Add(stream, 117068, 0, 0, 3, 0, Role.Flags.Gem.EmptySocket, Role.Flags.Gem.NoSocket, false); //Earings -3 & elite 
                                    client.Inventory.Add(stream, 723712, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); // +1 Pack - non bound
                                    client.Inventory.AddItemWitchStack(3001283, 0, 3, stream);   // 3 ExpBalls
                                    client.Inventory.AddItemWitchStack(723711, 0, 5, stream);  //// MeteorTearPack - 5
                                    client.CreateBoxDialog("You received~1~Socket~Lv67~EliteEarrings(-3%),~a~+1~StonePack,~3~EXPBalls(F),~a~7~Days`~Blessing~Stone(F),~and~5~Meteor~Tear~Packs.");

                                }
                                else
                                {
#if Arabic
                                     client.SendSysMesage("Please make 6 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 6 more spaces in your inventory.");
#endif

                                }
                                break;
                            }

                        //
                        case 720706:
                            {
                                if (client.Inventory.HaveSpace(4))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.Add(stream, 561138, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //ironwand
                                    client.Inventory.Add(stream, 131068, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //lightarmour
                                    client.Inventory.Add(stream, 117068, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //HeartofOcean
                                    client.Inventory.AddItemWitchStack(723712, 0, 1, stream);
                                    client.CreateBoxDialog("You received an~Elite~L70~Iron~Wand,~an~Elite~L70~Light~Armor,~an~Elite~L67~Heart~of~Ocean~and~3~+1~Stones.");

                                }
                                else
                                {
                                    client.SendSysMesage("Please make 4 more spaces in your inventory.");
                                }
                                break;
                            }
                        case 723792:  //MetScrollPack
                            {
                                if (client.Inventory.HaveSpace(9))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.Add(stream, 720027, 10, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //MetScrollPack
                                    client.SendSysMesage("Congratulations! You just got 10 MetScrollPack ..!");

                                }
                                else
                                {
                                    client.SendSysMesage("Please make 9 more spaces in your inventory.");
                                }
                                break;
                            }

                        case 723793:  //DbScrollPack
                            {
                                if (client.Inventory.HaveSpace(9))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.Add(stream, 720028, 10, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //DbScrollPack
                                    client.SendSysMesage("Congratulations! You just got 10 DbScrollPack ..!");

                                }
                                else
                                {
                                    client.SendSysMesage("Please make 9 more spaces in your inventory.");
                                }
                                break;
                            }

                        case 720707:
                            {
                                if (client.Inventory.HaveSpace(4))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);



                                    client.Inventory.Add(stream, 420138, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //SharkSword
                                    client.Inventory.Add(stream, 130068, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //RageArmor
                                    client.Inventory.Add(stream, 118068, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //WarCoronet
                                    client.Inventory.AddItemWitchStack(723712, 0, 1, stream);
                                    client.CreateBoxDialog("You received an~Elite~L70~Shark~Sword,~an~Elite~L70~Rage~Armor,~an~Elite~L67~War~Coronet~and~3~+1~Stones.");

                                }
                                else
                                {
#if Arabic
                                     client.SendSysMesage("Please make 4 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 4 more spaces in your inventory.");
#endif

                                }
                                break;
                            }



                        case 720708:
                            {
                                if (client.Inventory.HaveSpace(4))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);



                                    client.Inventory.Add(stream, 500128, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //GooseBow
                                    client.Inventory.Add(stream, 133048, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //ApeCoat
                                    client.Inventory.Add(stream, 113048, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //ApeHat
                                    client.Inventory.AddItemWitchStack(723712, 0, 1, stream);
                                    client.CreateBoxDialog("You received an~Elite~L70~Goose~Bow,~an~Elite~L67~Ape~Coat,~an~Elite~L72~Ape~Hat~and~3~+1~Stones.");

                                }
                                else
                                {
#if Arabic
                                     client.SendSysMesage("Please make 4 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 4 more spaces in your inventory.");
#endif

                                }
                                break;
                            }

                        case 720709:
                            {
                                if (client.Inventory.HaveSpace(4))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);



                                    client.Inventory.Add(stream, 601138, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //RainKatana
                                    client.Inventory.Add(stream, 135068, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //TigerVest
                                    client.Inventory.Add(stream, 112068, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //BloodEvil
                                    client.Inventory.AddItemWitchStack(723712, 0, 1, stream);
                                    client.CreateBoxDialog("You received an~Elite~L70~Rain~Katana,~an~Elite~L70~TigerVest,~an~Elite~L67~BloodVeil~and~3~+1~Stones.");

                                }
                                else
                                {
#if Arabic
                                     client.SendSysMesage("Please make 4 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 4 more spaces in your inventory.");
#endif

                                }
                                break;
                            }


                        case 720710:
                            {
                                if (client.Inventory.HaveSpace(4))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);



                                    client.Inventory.Add(stream, 421138, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //GreatBacksword
                                    client.Inventory.Add(stream, 134068, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //CraneVestment
                                    client.Inventory.Add(stream, 114068, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //Sharkcap
                                    client.Inventory.AddItemWitchStack(723712, 0, 1, stream);
                                    client.CreateBoxDialog("You received an~Elite~L70~Great~Backsword,~an~Elite~L70~Crane~Vestment,~an~Elite~L67~Shark~Cap~and~3~+1~Stones.");

                                }
                                else
                                {
#if Arabic
                                     client.SendSysMesage("Please make 4 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 4 more spaces in your inventory.");
#endif

                                }
                                break;
                            }

                        case 720711:
                            {
                                if (client.Inventory.HaveSpace(4))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);



                                    client.Inventory.Add(stream, 120128, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //PlatinaNecklace
                                    client.Inventory.Add(stream, 150138, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //PearlRing
                                    client.Inventory.Add(stream, 160138, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //SnakeskinBoots
                                    client.Inventory.AddItemWitchStack(723712, 0, 1, stream);
                                    client.CreateBoxDialog("You received an~Elite~L67~Platina~Necklace,~an~Elite~L70~Pearl~Ring,~an~Elite~L70~Snakeskin~Boots~and~3~+1~Stones");

                                }
                                else
                                {
#if Arabic
                                     client.SendSysMesage("Please make 4 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 4 more spaces in your inventory.");
#endif

                                }
                                break;
                            }



                        case 720712:
                            {
                                if (client.Inventory.HaveSpace(4))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);



                                    client.Inventory.Add(stream, 152128, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //BoneBracelet
                                    client.Inventory.Add(stream, 121128, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //AmbergrisBag
                                    client.Inventory.Add(stream, 160138, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //SnakeskinBoots
                                    client.Inventory.AddItemWitchStack(723712, 0, 1, stream);
                                    client.CreateBoxDialog("You received an~Elite~L65~Bone~Bracelet,~an~Elite~L67~Ambergris~Bag,~an~Elite~L70~Snakeskin~Boots~and~3~+1~Stones");

                                }
                                else
                                {
#if Arabic
                                     client.SendSysMesage("Please make 4 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 4 more spaces in your inventory.");
#endif

                                }
                                break;
                            }


                        case 720704:
                            {
                                if (client.Inventory.HaveSpace(5))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);



                                    client.Inventory.Add(stream, 150179, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //CrystalRing
                                    client.Inventory.Add(stream, 120159, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //BasaltNecklace
                                    client.Inventory.Add(stream, 160179, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //LeopardBoots
                                    client.Inventory.AddItemWitchStack(3005654, 0, 2, stream);
                                    client.CreateBoxDialog("You received a~Super~L90~Crystal~Ring,~a~Super~L82~Basalt~Necklace,~a~Super~L90~Leopard~Boots~and~3~+2~Stones.");

                                }
                                else
                                {
#if Arabic
                                     client.SendSysMesage("Please make 5 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 5 more spaces in your inventory.");
#endif

                                }
                                break;
                            }


                        case 720705:
                            {
                                if (client.Inventory.HaveSpace(5))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);



                                    client.Inventory.Add(stream, 152169, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //JadeBracelet
                                    client.Inventory.Add(stream, 121159, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //JadeBag
                                    client.Inventory.Add(stream, 160179, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //LeopardBoots
                                    client.Inventory.AddItemWitchStack(3005654, 0, 2, stream);
                                    client.CreateBoxDialog("You received a~Super~L85~Jade~Bracelet,~a~Super~L82~Jade~Bag,~a~Super~L90~Leopard~Boots~and~3~+2~Stones.");

                                }
                                else
                                {
#if Arabic
                                     client.SendSysMesage("Please make 5 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 5 more spaces in your inventory.");
#endif

                                }
                                break;
                            }




                        case 720699:
                            {
                                if (client.Inventory.HaveSpace(5))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);



                                    client.Inventory.Add(stream, 561179, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //ExorcisingWand
                                    client.Inventory.Add(stream, 131079, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //LionArmor
                                    client.Inventory.Add(stream, 111079, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //DragonHelmet
                                    client.Inventory.AddItemWitchStack(3005654, 0, 2, stream);
                                    client.CreateBoxDialog("You received a~Super~L90~Exorcising~Wand,~a~Super~L87~Lion~Armor,~a~Super~L82~Dragon~Helmet~and~3~+2~Stones.");

                                }
                                else
                                {
#if Arabic
                                     client.SendSysMesage("Please make 5 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 5 more spaces in your inventory.");
#endif

                                }
                                break;
                            }


                        case 720700:
                            {
                                if (client.Inventory.HaveSpace(5))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);



                                    client.Inventory.Add(stream, 420179, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //LongSword
                                    client.Inventory.Add(stream, 130079, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //SacredArmor
                                    client.Inventory.Add(stream, 118079, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //HerculesCoronet
                                    client.Inventory.AddItemWitchStack(3005654, 0, 2, stream);
                                    client.CreateBoxDialog("You received a~Super~L90~Long~Sword,~a~Super~L87~Sacred~Armor,~a~Super~L82~Hercules~Coronet~and~3~+2~Stones.");

                                }
                                else
                                {
#if Arabic
                                     client.SendSysMesage("Please make 5 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 5 more spaces in your inventory.");
#endif

                                }
                                break;
                            }




                        case 720701:
                            {
                                if (client.Inventory.HaveSpace(5))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);



                                    client.Inventory.Add(stream, 500169, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //StarBow
                                    client.Inventory.Add(stream, 133069, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //SharkCoat
                                    client.Inventory.Add(stream, 113059, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //MartenHat
                                    client.Inventory.AddItemWitchStack(3005654, 0, 2, stream);
                                    client.CreateBoxDialog("You received a~Super~L90~Star~Bow,~a~Super~L87~Shark~Coat,~a~Super~L82~Marten~Hat~and~3~+2~Stones.");

                                }
                                else
                                {
#if Arabic
                                     client.SendSysMesage("Please make 5 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 5 more spaces in your inventory.");
#endif

                                }
                                break;
                            }


                        case 720702:
                            {
                                if (client.Inventory.HaveSpace(5))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);



                                    client.Inventory.Add(stream, 601179, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //IseKatana
                                    client.Inventory.Add(stream, 135079, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //BearVest
                                    client.Inventory.Add(stream, 112079, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //StealthVeil
                                    client.Inventory.AddItemWitchStack(3005654, 0, 2, stream);
                                    client.CreateBoxDialog("You received a~Super~L90~Ise~Katana,~a~Super~L87~Bear~Vest,~a~Super~L82~Stealth~Veil~and~3~+2~Stones.");

                                }
                                else
                                {
#if Arabic
                                     client.SendSysMesage("Please make 5 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 5 more spaces in your inventory.");
#endif

                                }
                                break;
                            }


                        case 720703:
                            {
                                if (client.Inventory.HaveSpace(5))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);



                                    client.Inventory.Add(stream, 421179, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //WarBacksword
                                    client.Inventory.Add(stream, 134079, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //FullFrock
                                    client.Inventory.Add(stream, 114079, 0, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, false); //DragonCap
                                    client.Inventory.AddItemWitchStack(3005654, 0, 2, stream);
                                    client.CreateBoxDialog("You received a~Super~L90~War~Backsword,~a~Super~L87~Full~Frock,~a~Super~L82~Dragon~Cap~and~3~+2~Stones.");

                                }
                                else
                                {
#if Arabic
                                     client.SendSysMesage("Please make 5 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 5 more spaces in your inventory.");
#endif

                                }
                                break;
                            }


                        case 3005654://+2StonePack
                            {
                                if (client.Inventory.HaveSpace(2))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    if (item.Bound == 1)
                                    {
                                        client.Inventory.Add(stream, 730002, 2, 1, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                    }
                                    else
                                        client.Inventory.Add(stream, 730002, 2, 1);
                                }
                                else
                                {
#if Arabic
                                         client.SendSysMesage("Please make 2 more spaces in your inventory.");
#else
                                    client.SendSysMesage("Please make 2 more spaces in your inventory.");
#endif

                                }
                                break;
                            }
                        case 721952:
                            {
                                if (client.Inventory.Contain(721953, 1))
                                {
                                    if (client.Inventory.Contain(1, 710583))
                                    {
                                        client.SendSysMesage("You've already obtained a Moon Jade.", MsgMessage.ChatMode.System);
                                        break;
                                    }
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.Remove(721953, 1, stream);
                                    if (client.Player.Rate(40))
                                    {
                                        client.Inventory.Add(stream, 710583);//si itemul
                                        client.SendSysMesage("A Maple Stone and Ocean Stone have been fused into a Moon Jade!", MsgMessage.ChatMode.System);
                                    }
                                    else
                                        client.SendSysMesage("Failed to fuse! The Maple Stone and Ocean Stone disapperead!", MsgMessage.ChatMode.System);
                                }
                                else
                                    client.SendSysMesage("If you have a Maple Stone and an Ocean Stone, right click the Leaf Stone to fuse them info a Sun Jade.", MsgMessage.ChatMode.System);
                                break;
                            }
                        case 721953://ocean Stone
                            {
                                if (client.Inventory.Contain(721951, 1))
                                {
                                    if (client.Inventory.Contain(1, 710584))
                                    {
                                        client.SendSysMesage("You've already obtained a Sun Jade.", MsgMessage.ChatMode.System);
                                        break;
                                    }
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.Remove(721951, 1, stream);
                                    if (client.Player.Rate(40))
                                    {
                                        client.Inventory.Add(stream, 710584);//si itemul
                                        client.SendSysMesage("A Leaf Stone and Ocean Stone have been fused into a Sun Jade!", MsgMessage.ChatMode.System);
                                    }
                                    else
                                        client.SendSysMesage("Failed to fuse! The leaf Stone and Ocean Stone disapperead!", MsgMessage.ChatMode.System);
                                }
                                else
                                    client.SendSysMesage("If you have a Leaf Stone and an Ocean Stone, right click the Leaf Stone to fuse them info a Sun Jade.", MsgMessage.ChatMode.System);

                                break;
                            }
                        case 721951://leaf Stone
                            {
                                if (client.Inventory.Contain(721952, 1))
                                {

                                    if (client.Inventory.Contain(1, 710582))
                                    {
                                        client.SendSysMesage("You've already obtained a Rune Jade.", MsgMessage.ChatMode.System);
                                        break;
                                    }
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.Remove(721952, 1, stream);



                                    if (client.Player.Rate(40))
                                    {
                                        client.Inventory.Add(stream, 710582);
                                        client.SendSysMesage("A Leaf Stone and Maple Stone have been fused into a Rune Jade!", MsgMessage.ChatMode.System);
                                    }
                                    else
                                        client.SendSysMesage("Failed to fuse! The leaf Stone and Maple Stone disapperead!", MsgMessage.ChatMode.System);
                                }
                                else
                                    client.SendSysMesage("If you have a Leaf Stone and an Maple Stone, right click the Leaf Stone to fuse them info a Sun Jade.", MsgMessage.ChatMode.System);
                                break;
                            }
                        case 728242:
                            {
                                if (client.Inventory.HaveSpace(1))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.Add(stream, Database.ItemType.ExpBall2);
                                    if (Database.AtributesStatus.IsTaoist(client.Player.Class))
                                    {
                                        client.CreateBoxDialog("You~opened~the~pack~and~received~an~Elite~Jade~Bracelet,~a~ExpBall(Event)!");
                                        client.Inventory.Add(stream, 152168, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 150178, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                        client.CreateBoxDialog("You~opened~the~pack~and~received~an~Elite~Crystal~Ring,~a~ExpBall(Event)!");
                                    }
                                }
                                else
                                {
                                    client.CreateBoxDialog("Please~prepare~one~slot~in~your~inventory.");
                                }
                                break;
                            }
                        case 728241:
                            {
                                if (client.Inventory.HaveSpace(1))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.Add(stream, Database.ItemType.ExpBall2);
                                    if (Database.AtributesStatus.IsTaoist(client.Player.Class))
                                    {
                                        client.CreateBoxDialog("You~opened~the~pack~and~received~an~Elite~Jade~Bag,~a~ExpBall(Event)!");
                                        client.Inventory.Add(stream, 121158, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 120158, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, true);
                                        client.CreateBoxDialog("You~opened~the~pack~and~received~an~Elite~Basalt~Necklace,~a~ExpBall(Event)!");
                                    }
                                }
                                else
                                {
                                    client.CreateBoxDialog("Please~prepare~one~slot~in~your~inventory.");
                                }

                                break;
                            }
                        case 3005836:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.Money += 50000000;
                                client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);
#if Arabic
                                                   client.SendSysMesage("You received 50000000 silver for opening the MediumSilverBag.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#else
                                client.SendSysMesage("You received 50000000 silver for opening the MediumSilverBag.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#endif
                                break;
                            }
                        case 3005837:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Player.Money += 100000000;
                                client.Player.SendUpdate(stream, client.Player.Money, MsgUpdate.DataType.Money);
#if Arabic
                                                   client.SendSysMesage("You received 100000000 silver for opening the BigSilverBag.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#else
                                client.SendSysMesage("You received 100000000 silver for opening the BigSilverBag.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
#endif
                                break;
                            }
                        case 3600025:
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                {
                                    client.GainExpBall(1000);
                                    client.SendSysMesage("You received 100-min EXP!", MsgMessage.ChatMode.System);
                                }
                                break;
                            }
                        case 3200000:
                            {
                                if (!client.Inventory.HaveSpace(3))
                                {
                                    client.CreateBoxDialog("Please make 3 more spaces in your inventory.");
                                    break;
                                }
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 3007108);
                                client.Inventory.Add(stream, 3200334);
                                client.Inventory.Add(stream, 729549);
                                client.Inventory.AddItemWitchStack(3002926, 0, 3, stream);
                                break;
                            }
                        case 729549:
                            {
                                if (!client.Inventory.HaveSpace(3))
                                {
                                    client.CreateBoxDialog("Please make 3 more spaces in your inventory.");
                                    break;
                                }
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.AddItemWitchStack(1003010, 0, 20, stream);
                                break;
                            }
                        case 3008994:
                            {

                                if (client.Inventory.Contain(3008994, 10))
                                {
                                    client.Inventory.Remove(3008994, 10, stream);
                                    client.Inventory.Add(stream, Database.ItemType.DragonBall);
                                    client.SendSysMesage("You've successfully made a DragonBall.", MsgMessage.ChatMode.System);
                                }
                                break;
                            }
                        case 3600031:
                            {
                                if (!client.Inventory.HaveSpace(2))
                                {
                                    client.CreateBoxDialog("Please make 2 more spaces in your inventory.");
                                    break;
                                }
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Inventory.Add(stream, 3009000, 3);
                                client.SendSysMesage("You've received x3 TwilightStarStone.", MsgMessage.ChatMode.System);
                                break;
                            }

                        case 721874:
                            {
                                if (client.Inventory.Contain(711518, 1))
                                {
                                    client.Inventory.Remove(711518, 1, stream);
                                    client.Inventory.Remove(721874, 1, stream);
                                    client.Inventory.Add(stream, 721872);
                                    client.CreateBoxDialog("You`ve~obtained~the~Buddha~Relic.~Hurry~up~and~give~it~to~Louis.");
                                }
                                else
                                {
                                    client.CreateBoxDialog("You~don`t~have~a~Jinx~Tomb~Bat~Heart!");
                                }
                                break;
                            }
                        case 721920:
                            {
                                if (client.Player.Map == 1001)
                                {
                                    if (DateTime.Now > client.Player.FerventPill.AddSeconds(300))
                                    {
                                        client.Player.AddFlag(MsgUpdate.Flags.Poisoned, 300, true, 1);
                                        client.Player.SendString(stream, MsgStringPacket.StringID.Effect, false, "900x100");
                                        client.SendSysMesage("Fervent Pill is in effect now. But you will lose 10 percent of current HP every 10 seconds.");
                                    }
                                    else
                                    {
                                        client.SendSysMesage("Fervent Pill will work for 1 minute every time you use it. It is still effective now. You don`t need to use it again.");
                                    }
                                }
                                else
                                {
                                    client.SendSysMesage("You need to use it in Mystic Castle.");
                                }
                                break;
                            }
                        case 721921:
                            {
                                if (Role.Core.GetDistance(client.Player.X, client.Player.Y, 102, 292) <= 7 && client.Player.Map == 1001)
                                {
                                    client.Player.AddMapEffect(stream, 102, 292, "zf2-e027");
                                    client.SendSysMesage("You used the Ghost Mirror and a trap was set by it. Now herd the Mad Bull here and kill it to capture its ghost.");

                                }
                                else
                                {
                                    client.SendSysMesage("You need to use the Ghost Mirror at Heaven Well (102,292) in Mystic Castle.");
                                }
                                break;
                            }
                        case 728249:
                            {
                                if (client.Inventory.HaveSpace(1))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.Add(stream, 722136, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.Inventory.Add(stream, 160199, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.CreateBoxDialog("You~opened~the~pack~and~received~a~Super~Kylin~Boots!");
                                }
                                else
                                {
                                    client.CreateBoxDialog("Please~prepare~one~slot~in~your~inventory.");
                                }
                                break;
                            }
                        case 3300179:
                            {
                                if (client.Inventory.HaveSpace(1) == false)
                                {
                                    client.CreateBoxDialog("Please make 1 more space in your inventory.");
                                    break;
                                }
                                client.CreateBoxDialog("You've received 10 SpiritofLegend!");
                                client.Inventory.AddItemWitchStack(3300056, 0, 10, stream);
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                break;
                            }

                        case 3008101:
                            {
                                if (!client.Inventory.HaveSpace(1))
                                {
                                    client.CreateBoxDialog("Please make 1 more space in your inventory.");
                                    break;
                                }
                                if (client.Player.OpenHousePack < 3)
                                {
                                    client.Player.OpenHousePack += 1;
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.AddItemWitchStack(3008100, 0, 1, stream, false);
                                    client.SendSysMesage("You received a Class 6 House Pack!", MsgMessage.ChatMode.System);
                                }
                                else
                                    client.SendSysMesage("You've already opened such a pack for 3 times today. Please retry tomorrow.", MsgMessage.ChatMode.System);

                                break;
                            }
                        case 3004259://banshe
                            {
                                if (client.Player.Level >= 99)
                                {
                                    if (client.MyHouse != null && client.Player.DynamicID == client.Player.UID)
                                    {
                                        if (!client.Map.ContainMobID(20070, client.Player.UID))
                                        {
                                            client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                            Database.Server.AddMapMonster(stream, client.Map, 20070, client.Player.X, client.Player.Y, 1, 1, 1, client.Player.DynamicID, true, MsgFloorItem.MsgItemPacket.EffectMonsters.EarthquakeAndNight);
                                        }
                                        else
                                        {
#if Arabic
                                            client.CreateDialog(stream, "Sorry,but the monster is already spawned..", "I~see.");
#else
                                            client.CreateDialog(stream, "Sorry,but the monster is already spawned..", "I~see.");
#endif

                                        }
                                    }
                                    else
                                    {
                                        client.CreateDialog(stream, "This item can only be used in your house.", "Got~it.");


                                    }
                                }
                                else
                                {
#if Arabic
                                      client.CreateDialog(stream, "Sorry,~you~cannot~open~this~monster~before~your~Level~is~99.~Please~train~harder.", "I~see.");
#else
                                    client.CreateDialog(stream, "Sorry,~you~cannot~open~this~monster~before~your~Level~is~99.~Please~train~harder.", "I~see.");
#endif

                                }
                                break;
                            }
                        case 3008100:
                            {
                                client.SendSysMesage("This packet contains all kinds of necessary materials needed to build a Class 6 house.", MsgMessage.ChatMode.System);
                                break;
                            }
                        case 721799:
                            {
                                if (Role.Core.GetDistance(55, 109, client.Player.X, client.Player.Y) <= 10 && client.Player.Map == 1792)
                                {
                                    client.Inventory.Remove(721799, 1, stream);
                                    Game.MsgNpc.Npc np = Game.MsgNpc.Npc.Create();
                                    np.UID = 4584;
                                    np.NpcType = Role.Flags.NpcType.Talker;
                                    np.Mesh = 3920;
                                    np.Map = 1792;
                                    np.X = 55;
                                    np.Y = 109;
                                    Database.Server.ServerMaps[np.Map].AddNpc(np);
                                    client.Player.View.Role(false);
                                    client.Player.AddMapEffect(stream, 55, 109, "eddy");
                                }
                                else
                                    client.SendSysMesage("Please burn the Incense in Swan Lake (55,109).");
                                break;
                            }
                        case 721806:
                            {
                                if (client.Player.Map == 1794)
                                {
                                    client.Teleport(557, 649, 1000);
                                    client.Inventory.Remove(721806, 1, stream);
                                    client.SendSysMesage("You`ve come back to Desert City. Now take the Posion Fang to the Kunlun Wanderer (490,618).");
                                }
                                else
                                    client.SendSysMesage("It can be used in the Viper Cave only.");
                                break;
                            }
                        case 721801:
                            {
                                Dialog data = new Dialog(client, stream);
                                data.AddText("The Tang Silk has been dyed with Sacred Water. A complicated poem appears on the silk. Now take this Poem Silk to Sugar Tang in");
                                data.AddText("~Bird Island (685,599). This poem was written by her grandma and a man called Mr. Loneliness. She may know something.");
                                data.AddOption("Gotcha.");
                                data.FinalizeDialog();
                                break;
                            }

                        case 3005368:
                            {
                                if (client.Inventory.HaveSpace(1))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    client.Inventory.Add(stream, 188915, 1, 0, 1, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.Inventory.Add(stream, 200517, 1, 0, 1, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket, item.Bound >= 1 ? true : false);
                                    client.SendSysMesage("Congratulations!~You~received~a~Gold~Cloth~(Saint)~and~a~King~of~Scorpions~(Saint)!");
                                    Role.Core.SendGlobalMessage(stream, "Congratulations!~" + client.Player.Name + "~received~a~Gold~Cloth~(Saint)~and~a~King~of~Scorpions~(Saint)!", MsgMessage.ChatMode.System);
                                    client.Player.SendString(stream, MsgStringPacket.StringID.Effect, false, "accession");
                                }
                                else
                                {
                                    client.SendSysMesage("Your~inventory~is~full.");
                                }
                                break;
                            }

                        case 724132:
                            {
                                client.Inventory.Remove(724132, 1, stream);
                                if (Role.Core.Rate(60, 100))
                                {
                                    client.Inventory.Add(stream, 724392);
                                    client.SendSysMesage("You've opened the Superior Bell Pack and got the Unique Bell.", MsgMessage.ChatMode.System);
                                }
                                else
                                {
                                    if (Role.Core.Rate(30, 40))
                                    {
                                        client.Inventory.Add(stream, 724393);
                                        client.SendSysMesage("You've opened the Superior Bell Pack and got the Elite Bell.", MsgMessage.ChatMode.System);
                                    }
                                    else
                                    {
                                        client.Inventory.Add(stream, 724394);
                                        client.SendSysMesage("You've opened the Superior Bell Pack and got the Super Bell.", MsgMessage.ChatMode.System);
                                    }
                                }
                                break;
                            }
                    }
                }
            }
        }
    }
}
