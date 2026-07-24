using System;
using System.Collections.Generic;

namespace COServer.Game.MsgServer
{
    public static unsafe partial class MsgBuilder
    {
        public static unsafe void GetUpdateItem(this ServerSockets.Packet stream, out MsgUpdateItem.ActionType Action, out uint ItemUID, out List<uint> items)
        {
            Action = (MsgUpdateItem.ActionType)stream.ReadUInt8();
            byte FullCount = stream.ReadUInt8();
            ushort padding = stream.ReadUInt16();
            ItemUID = stream.ReadUInt32();
            uint Count = stream.ReadUInt32();

            items = new List<uint>();
            if (Action != MsgUpdateItem.ActionType.UpdateLevel && Action != MsgUpdateItem.ActionType.UpdateQuality)
            {

                items.Add(Count);
                for (byte x = 0; x < FullCount - 2; x++)
                {
                    items.Add(stream.ReadUInt32());
                }
            }
            else
            {
                for (byte x = 0; x < Count; x++)
                    items.Add(stream.ReadUInt32());
            }
        }
    }


    public struct MsgUpdateItem
    {
        public enum ActionType : byte
        {
            Plus = 0,
            CurrentSteed = 2,
            NewSteed = 3,
            ChanceUpgrade = 4,
            UpdateLevel = 6,
            UpdateQuality = 7
        }
        [PacketAttribute(GamePackets.Compose)]
        public unsafe static void Compose(Client.GameClient client, ServerSockets.Packet stream)
        {

            MsgUpdateItem.ActionType Action; uint ItemUID;
            List<uint> ItemsUIDS;


            //MsgUpdateItem* PacketCompose = (MsgUpdateItem*)packet.Pointer;

            uint dwParam1 = 2;

            stream.GetUpdateItem(out Action, out ItemUID, out ItemsUIDS);



            switch (Action)
            {
                case ActionType.UpdateLevel:
                    {




                        if (ItemsUIDS.Count == 0)
                            break;

                        MsgGameItem DataItem;
                        if (client.TryGetItem(ItemUID, out DataItem))
                        {
                            ushort Position = Database.ItemType.ItemPosition(DataItem.ITEM_ID);
                            //anti proxy --------------------
                            if (!Database.ItemType.AllowToUpdate((Role.Flags.ConquerItem)Position))
                            {
                                client.SendSysMesage("This item's level can't be upgraded anymore.");
                                return;
                            }
                            //------------------------
                            MsgGameItem itemuse;
                            if (client.Inventory.ClientItems.TryGetValue(ItemsUIDS[0], out itemuse))
                            {
                                if (itemuse.ITEM_ID == Database.ItemType.DragonBall)
                                {
                                    Database.ItemType.DBItem DBItem;
                                    if (Database.Server.ItemsBase.TryGetValue(DataItem.ITEM_ID, out DBItem))
                                    {
                                        bool succesed = false;
                                        uint nextItemId = Database.Server.ItemsBase.UpdateItem(DataItem.ITEM_ID, out succesed);

                                        if ((DBItem.Level >= 70 && Database.ItemType.Equipable(nextItemId, client) == false)
                                              && (Database.ItemType.ItemPosition(DataItem.ITEM_ID) == (ushort)Role.Flags.ConquerItem.RightWeapon
                                              || Database.ItemType.ItemPosition(DataItem.ITEM_ID) == (ushort)Role.Flags.ConquerItem.LeftWeapon))
                                        {
                                            client.CreateBoxDialog("You can`t upgrade this item.");
                                        }
                                        else
                                        {
                                            dwParam1 = 1;
                                            uint oldid = DataItem.ITEM_ID;
                                            DataItem.ITEM_ID = Database.Server.ItemsBase.UpdateItem(DataItem.ITEM_ID, out succesed);
                                            DataItem.Mode = Role.Flags.ItemMode.Update;
                                            DataItem.Send(client, stream);//.Update(itemuse, Role.Instance.AddMode.REMOVE,stream);
                                            if (succesed && oldid != DataItem.ITEM_ID)
                                            {
                                                client.Inventory.Update(itemuse, Role.Instance.AddMode.REMOVE, stream);
                                            }
                                            else
                                            {
                                                client.SendSysMesage("This item's level can't be upgraded anymore.");
                                            }
                                        }
                                    }
                                }
                                else if (itemuse.ITEM_ID == Database.ItemType.Meteor || itemuse.ITEM_ID == Database.ItemType.MeteorScroll)
                                {
                                    if (client.Inventory.CheckMeteors((byte)ItemsUIDS.Count, false, stream))
                                    {
                                        Database.ItemType.DBItem DBItem;
                                        if (Database.Server.ItemsBase.TryGetValue(DataItem.ITEM_ID, out DBItem))
                                        {
                                            bool succesed = false;

                                            uint nextItemId = Database.Server.ItemsBase.UpdateItem(DataItem.ITEM_ID, out succesed);

                                            if ((DBItem.Level >= 70 && Database.ItemType.Equipable(nextItemId, client) == false)
                                             && (Database.ItemType.ItemPosition(DataItem.ITEM_ID) == (ushort)Role.Flags.ConquerItem.RightWeapon
                                             || Database.ItemType.ItemPosition(DataItem.ITEM_ID) == (ushort)Role.Flags.ConquerItem.LeftWeapon))
                                            {
                                                client.CreateBoxDialog("You can`t upgrade this item.");
                                            }
                                            else
                                            {
                                                if (Database.ItemType.UpItemMeteors(DataItem.ITEM_ID, (uint)ItemsUIDS.Count))
                                                {
                                                    dwParam1 = 1;
                                                    DataItem.ITEM_ID = Database.Server.ItemsBase.UpdateItem(DataItem.ITEM_ID, out succesed);
                                                    DataItem.Mode = Role.Flags.ItemMode.Update;
                                                    DataItem.Send(client, stream);

                                                }
#if TEST
                                            Console.WriteLine("Update Item " + DBItem.Name + " id = " + Database.ItemType.GetLevel(DataItem.ITEM_ID));
#endif

                                                client.Inventory.CheckMeteors((byte)ItemsUIDS.Count, true, stream);
                                            }
                                        }
                                    }
                                }
                                if (DataItem.Position != 0)
                                    client.Equipment.QueryEquipment();
                            }
                            client.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsuageID.UpgradeMeteor, ItemUID, dwParam1, 0, 0, 0, 0));

                        }
                        break;
                    }
                case ActionType.UpdateQuality:
                    {


                        if (ItemsUIDS.Count == 0)
                            break;
                        MsgGameItem DataItem;
                        if (client.TryGetItem(ItemUID, out DataItem))
                        {
                            ushort Position = Database.ItemType.ItemPosition(DataItem.ITEM_ID);
                            //anti proxy --------------------
                            if (Position != (ushort)Role.Flags.ConquerItem.Fan
                                && Position != (ushort)Role.Flags.ConquerItem.Tower)
                            {
                                if (!Database.ItemType.AllowToUpdate((Role.Flags.ConquerItem)Position))
                                {
                                    client.SendSysMesage("This item's quality can't be upgraded anymore.");
                                    return;
                                }
                            }
                            //------------------------
                            Queue<MsgGameItem> UseItems = new Queue<MsgGameItem>();
                            bool EmbedUpdate = false;
                            for (int x = 0; x < ItemsUIDS.Count; x++)
                            {
                                MsgGameItem itemuse;
                                if (client.Inventory.ClientItems.TryGetValue(ItemsUIDS[x], out itemuse))
                                {
                                    UseItems.Enqueue(itemuse);
                                    EmbedUpdate = true;
                                }
                                else { EmbedUpdate = false; break; }
                            }
                            if (EmbedUpdate && UseItems.Count > 0)
                            {
                                var CheckItem = UseItems.Dequeue();
                                if (CheckItem.ITEM_ID == Database.ItemType.DragonBall)
                                {
                                    Database.ItemType.DBItem DBItem;
                                    if (Database.Server.ItemsBase.TryGetValue(DataItem.ITEM_ID, out DBItem))
                                    {
                                        if (Database.ItemType.UpQualityDB(DataItem.ITEM_ID, (uint)(UseItems.Count + 1)))
                                        {
                                            dwParam1 = 1;
                                            if (DataItem.ITEM_ID % 10 < 5)
                                                DataItem.ITEM_ID += 5 - DataItem.ITEM_ID % 10;
                                            DataItem.ITEM_ID++;
                                            DataItem.Mode = Role.Flags.ItemMode.Update;
                                            DataItem.Send(client, stream);
                                        }


                                        client.Inventory.Update(CheckItem, Role.Instance.AddMode.REMOVE, stream);
                                        while (UseItems.Count > 0)
                                            client.Inventory.Update(UseItems.Dequeue(), Role.Instance.AddMode.REMOVE, stream);
                                    }
                                }
                            }
                            if (DataItem.Position != 0)
                                client.Equipment.QueryEquipment();
                        }

                        client.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsuageID.UpgradeDragonball, ItemUID, dwParam1, 0, 0, 0, 0));


                        break;
                    }
                default:
                    {

                        MsgGameItem DataItem;
                        if (client.TryGetItem(ItemUID, out DataItem))
                        {
                            ushort Position = Database.ItemType.ItemPosition(DataItem.ITEM_ID);
                            //anti proxy --------------------
                            if (Position != (ushort)Role.Flags.ConquerItem.Fan
                                && Position != (ushort)Role.Flags.ConquerItem.Tower)
                            {
                                if (!Database.ItemType.AllowToUpdate((Role.Flags.ConquerItem)Position))
                                {
                                    client.SendSysMesage("This item's plus can't be upgraded anymore.");
                                    return;
                                }
                            }
                            //------------------------
                            Queue<MsgGameItem> UseItems = new Queue<MsgGameItem>();
                            bool EmbedUpdate = false;
                            for (int x = 0; x < ItemsUIDS.Count; x++)
                            {
                                MsgGameItem itemuse;
                                if (client.Inventory.ClientItems.TryGetValue(ItemsUIDS[x], out itemuse))
                                {
                                    // 🔒 Bloqueia item já em +9 de entrar no slot
                                    if (itemuse.IsMaxPlus)
                                    {
                                        client.SendSysMesage("Highest bonus level.");

                                        // Força o cliente a atualizar/remover o item do slot
                                        itemuse.PlusProgress = 0;
                                        itemuse.Mode = Role.Flags.ItemMode.Update;
                                        itemuse.Send(client, stream);

                                        // Pacote extra para limpar slot
                                        client.Send(stream.ItemLockCreate(itemuse.UID, MsgItemLock.TypeLock.UnlockDate, 0, 0));

                                        EmbedUpdate = false;
                                        break; // não deixa entrar na fila
                                    }

                                    UseItems.Enqueue(itemuse);
                                    EmbedUpdate = true;
                                }
                                else
                                {
                                    EmbedUpdate = false;
                                    break;
                                }
                            }

                            if (EmbedUpdate && UseItems.Count > 0)
                            {
                                switch (Action)
                                {
                                    case ActionType.ChanceUpgrade:
                                        {
                                            if (!DataItem.IsMaxPlus && DataItem.PlusProgress != 0)
                                            {
                                                byte oldplus = DataItem.Plus;

                                                double percent = (double)DataItem.PlusProgress / (double)Database.ItemType.ComposePlusPoints(DataItem.Plus);

                                                if (Role.Core.Rate(percent) && new Random().Next(1, 100) < 10)
                                                {
                                                    DataItem.Plus++;
                                                }

                                                DataItem.PlusProgress = 0;

                                                if (DataItem.Plus >= 9)
                                                {
                                                    DataItem.PlusProgress = 0;
                                                    DataItem.Mode = Role.Flags.ItemMode.Update;
                                                    DataItem.Send(client, stream);
                                                    client.Send(stream.ItemLockCreate(DataItem.UID, MsgItemLock.TypeLock.UnlockDate, 0, 0));
                                                }

                                                if (oldplus != DataItem.Plus && DataItem.Plus >= 6)
                                                {
                                                    client.Map.SendSysMesage("Congratulations, " + client.Player.Name +
                                                        " has upgraded his/her " + Database.Server.ItemsBase[DataItem.ITEM_ID].Name +
                                                        " to +" + DataItem.Plus + (DataItem.IsMaxPlus ? " (MAX)!" : " and " + DataItem.PlusProgress + " in progress!"));
                                                }
                                            }
                                        }
                                        break;

                                    case ActionType.CurrentSteed:
                                    case ActionType.Plus:
                                        {
                                            while (UseItems.Count > 0)
                                            {
                                                byte oldplus = DataItem.Plus;
                                                var Stone = UseItems.Dequeue();

                                                if (DataItem.Plus >= 9)
                                                {
                                                    DataItem.PlusProgress = 0;
                                                    DataItem.Mode = Role.Flags.ItemMode.Update;
                                                    DataItem.Send(client, stream);
                                                    client.Send(stream.ItemLockCreate(DataItem.UID, MsgItemLock.TypeLock.UnlockDate, 0, 0));
                                                    continue;
                                                }

                                                DataItem.PlusProgress += Database.ItemType.StonePlusPoints(Stone.Plus);
                                                DataItem.PlusProgress += Stone.PlusProgress;

                                                while (DataItem.PlusProgress >= Database.ItemType.ComposePlusPoints(DataItem.Plus) && DataItem.Plus < 9)
                                                {
                                                    DataItem.PlusProgress -= Database.ItemType.ComposePlusPoints(DataItem.Plus);
                                                    DataItem.Plus++;
                                                }

                                                if (DataItem.Plus >= 9)
                                                {
                                                    DataItem.PlusProgress = 0;
                                                    DataItem.Mode = Role.Flags.ItemMode.Update;
                                                    DataItem.Send(client, stream);
                                                    client.Send(stream.ItemLockCreate(DataItem.UID, MsgItemLock.TypeLock.UnlockDate, 0, 0));
                                                }

                                                DataItem.Mode = Role.Flags.ItemMode.Update;
                                                DataItem.Send(client, stream).Update(Stone, Role.Instance.AddMode.REMOVE, stream);

                                                if (oldplus != DataItem.Plus && DataItem.Plus >= 2)
                                                {
                                                    string congratsMessage =
                                                        $"```diff\n" +
                                                        $"+ 🔥 {client.Player.Name} upgraded {Database.Server.ItemsBase[DataItem.ITEM_ID].Name} to +{DataItem.Plus}" +
                                                        (DataItem.Plus >= 9 ? " (MAX)\n" : $"\n+ {DataItem.PlusProgress} in progress!\n") +
                                                        $"```";

                                                    client.Map.SendSysMesage(congratsMessage);
                                                    Program.DiscordAPIplus.Enqueue(congratsMessage);
                                                }
                                            }
                                        }
                                        break;

                                    case ActionType.NewSteed:
                                        {
                                            while (UseItems.Count > 0)
                                            {
                                                var Stone = UseItems.Dequeue();

                                                if (DataItem.Plus >= 9)
                                                {
                                                    DataItem.PlusProgress = 0;
                                                    DataItem.Mode = Role.Flags.ItemMode.Update;
                                                    DataItem.Send(client, stream);
                                                    client.Send(stream.ItemLockCreate(DataItem.UID, MsgItemLock.TypeLock.UnlockDate, 0, 0));
                                                    continue;
                                                }

                                                DataItem.PlusProgress += Database.ItemType.StonePlusPoints(Stone.Plus);
                                                while (DataItem.PlusProgress >= Database.ItemType.ComposePlusPoints(DataItem.Plus) && DataItem.Plus < 9)
                                                {
                                                    DataItem.PlusProgress -= Database.ItemType.ComposePlusPoints(DataItem.Plus);
                                                    DataItem.Plus++;
                                                }

                                                if (DataItem.Plus >= 9)
                                                {
                                                    DataItem.PlusProgress = 0;
                                                    DataItem.Mode = Role.Flags.ItemMode.Update;
                                                    DataItem.Send(client, stream);
                                                    client.Send(stream.ItemLockCreate(DataItem.UID, MsgItemLock.TypeLock.UnlockDate, 0, 0));
                                                }

                                                // lógica de cores e sockets
                                                int color1 = (int)DataItem.SocketProgress;
                                                int color2 = (int)Stone.SocketProgress;

                                                int G1 = color1 & 0xFF;
                                                int G2 = color2 & 0xFF;
                                                int B1 = (color1 >> 8) & 0xFF;
                                                int B2 = (color2 >> 8) & 0xFF;
                                                int R1 = (color1 >> 16) & 0xFF;
                                                int R2 = (color2 >> 16) & 0xFF;

                                                byte ProgresGreen = (byte)((int)Math.Floor(0.9 * G1) + (int)Math.Floor(0.1 * G2) + 1);
                                                byte ProgresBlue = (byte)((int)Math.Floor(0.9 * B1) + (int)Math.Floor(0.1 * B2) + 1);
                                                byte ProgresRed = (byte)((int)Math.Floor(0.9 * R1) + (int)Math.Floor(0.1 * R2) + 1);

                                                DataItem.ProgresGreen = ProgresGreen;
                                                DataItem.Enchant = ProgresBlue;
                                                DataItem.Bless = ProgresRed;

                                                DataItem.SocketProgress = (uint)(ProgresBlue | (ProgresGreen << 8) | (ProgresRed << 16));

                                                DataItem.Mode = Role.Flags.ItemMode.Update;
                                                DataItem.Send(client, stream).Update(Stone, Role.Instance.AddMode.REMOVE, stream);
                                            }
                                        }
                                        break;
                                }

                                if (DataItem.Position != 0)
                                    client.Equipment.QueryEquipment();
                            }
                        }

                        break;
                    }

            }
        }
    }
}
