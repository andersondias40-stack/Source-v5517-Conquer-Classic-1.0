using COServer.Game.MsgServer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace COServer.Role.Instance
{

    public class Inventory
    {
        private const byte File_Size = 40;

        public ConcurrentDictionary<uint, Game.MsgServer.MsgGameItem> ClientItems = new ConcurrentDictionary<uint, Game.MsgServer.MsgGameItem>();

        public int GetCountItem(uint ItemID)
        {
            int count = 0;
            foreach (var DataItem in ClientItems.Values)
            {
                if (DataItem.ITEM_ID == ItemID)
                {
                    count += DataItem.StackSize > 1 ? DataItem.StackSize : 1;
                }
            }
            return count;
        }
        public bool VerifiedUpdateItem(List<uint> ItemsUIDS, uint ID, byte count, out Queue<Game.MsgServer.MsgGameItem> Items)
        {
            Queue<Game.MsgServer.MsgGameItem> ExistItems = new Queue<Game.MsgServer.MsgGameItem>();
            foreach (var DataItem in ClientItems.Values)
            {
                if (DataItem.ITEM_ID == ID)
                {
                    if (ItemsUIDS.Contains(DataItem.UID))
                    {
                        count--;
                        ItemsUIDS.Remove(DataItem.UID);
                        ExistItems.Enqueue(DataItem);
                    }
                }
            }
            Items = ExistItems;
            return ItemsUIDS.Count == 0 && count == 0;
        }

        private Client.GameClient Owner;
        public Inventory(Client.GameClient _own)
        {
            Owner = _own;
        }

        public void AddDBItem(Game.MsgServer.MsgGameItem item)
        {
            ClientItems.TryAdd(item.UID, item);
        }

        public void AddReturnedItem(ServerSockets.Packet stream, uint ID, byte count = 1, byte plus = 0, byte bless = 0, byte Enchant = 0
            , Role.Flags.Gem sockone = Flags.Gem.NoSocket
             , Role.Flags.Gem socktwo = Flags.Gem.NoSocket, bool bound = false, Role.Flags.ItemEffect Effect = Flags.ItemEffect.None, ushort StackSize = 0)
        {

            byte x = 0;
            for (; x < count;)
            {
                x++;
                Database.ItemType.DBItem DbItem;
                if (Database.Server.ItemsBase.TryGetValue(ID, out DbItem))
                {

                    Game.MsgServer.MsgGameItem ItemDat = new Game.MsgServer.MsgGameItem();
                    ItemDat.UID = Database.Server.ITEM_Counter.Next;
                    ItemDat.ITEM_ID = ID;
                    ItemDat.Effect = Effect;
                    ItemDat.StackSize = StackSize;
                    ItemDat.Durability = ItemDat.MaximDurability = DbItem.Durability;
                    ItemDat.Plus = plus;
                    ItemDat.Bless = bless;
                    ItemDat.Enchant = Enchant;
                    ItemDat.SocketOne = sockone;
                    ItemDat.SocketTwo = socktwo;
                    ItemDat.Color = (Role.Flags.Color)Program.GetRandom.Next(3, 9);
                    ItemDat.Bound = (byte)(bound ? 1 : 0);
                    ItemDat.Mode = Flags.ItemMode.AddItemReturned;
                    ItemDat.WH_ID = ushort.MaxValue;
                    //ItemDat.RemainingTime = (DbItem.StackSize > 1) ? 0 : uint.MaxValue;
                    Owner.Warehouse.AddItem(ItemDat, ushort.MaxValue);

                    ItemDat.Send(Owner, stream);
                }
            }
        }


        public bool HaveSpace(byte count)
        {
            return (ClientItems.Count + count) <= File_Size;
        }

        public bool TryGetItem(uint UID, out Game.MsgServer.MsgGameItem item)
        {
            return ClientItems.TryGetValue(UID, out item);
        }
        public bool SearchItemByID(uint ID, out Game.MsgServer.MsgGameItem item)
        {
            foreach (var msg_item in ClientItems.Values)
            {
                if (msg_item.ITEM_ID == ID)
                {
                    item = msg_item;
                    return true;
                }
            }
            item = null;
            return false;
        }

        public bool SearchItemByID(uint ID, byte count, out List<Game.MsgServer.MsgGameItem> Items)
        {
            byte increase = 0;
            Items = new List<Game.MsgServer.MsgGameItem>();
            foreach (var msg_item in ClientItems.Values)
            {
                if (msg_item.ITEM_ID == ID)
                {
                    Items.Add(msg_item);
                    increase++;
                    if (increase == count)
                    {
                        return true;
                    }
                }
            }
            Items = null;
            return false;
        }
        public bool Contain(uint ID, uint Amount, byte bound = 0)
        {
            if (ID == Database.ItemType.Meteor/* || ID == Database.ItemType.MeteorTear*/)//MeteorTearحل 
            {
                uint count = 0;
                foreach (var item in ClientItems.Values)
                {
                    if (item.ITEM_ID == Database.ItemType.Meteor/*
                        || item.ITEM_ID == Database.ItemType.MeteorTear*/)
                    {
                        if (item.Bound == bound)
                        {
                            count += item.StackSize;
                            if (count >= Amount)
                                return true;
                        }
                    }
                }
            }
            else if (ID == Database.ItemType.MoonBox || ID == 723087)//execept for bound
            {
                uint count = 0;
                foreach (var item in ClientItems.Values)
                {
                    if (item.ITEM_ID == ID)
                    {
                        count += item.StackSize;
                        if (count >= Amount)
                            return true;
                    }
                }
            }
            else
            {
                uint count = 0;
                foreach (var item in ClientItems.Values)
                {
                    if (item.ITEM_ID == ID)
                    {
                        if (item.Bound == bound)
                        {
                            count += item.StackSize;
                            if (count >= Amount)
                                return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool Remove(uint ID, uint count, ServerSockets.Packet stream)
        {
            if (Contain(ID, count) || Contain(ID, count, 1))
            {
                if (ID == Database.ItemType.Meteor || ID == Database.ItemType.MeteorTear)
                {
                    byte removed = 0;
                    for (byte x = 0; x < count; x++)
                    {
                        foreach (var item in ClientItems.Values)
                        {
                            if (item.ITEM_ID == Database.ItemType.Meteor
                         || item.ITEM_ID == Database.ItemType.MeteorTear)
                            {
                                try
                                {
                                    Update(item, AddMode.REMOVE, stream);
                                }
                                catch (Exception e)
                                {
                                    Console.SaveException(e);
                                }
                                removed++;
                                if (removed == count)
                                    break;
                            }
                        }
                        if (removed == count)
                            break;
                    }
                }
                else
                {
                    byte removed = 0;
                    for (byte x = 0; x < count; x++)
                    {
                        foreach (var item in ClientItems.Values)
                        {
                            if (item.ITEM_ID == ID)
                            {
                                try
                                {
                                    Update(item, AddMode.REMOVE, stream);
                                }
                                catch (Exception e)
                                {
                                    Console.SaveException(e);
                                }
                                removed++;
                                if (removed == count)
                                    break;
                            }
                        }
                        if (removed == count)
                            break;
                    }
                }
                return true;
            }
            return false;
        }
        public bool AddSteed(ServerSockets.Packet stream, uint ID, byte count = 1, byte plus = 0, bool bound = false, byte ProgresGreen = 0, byte ProgresBlue = 0, byte ProgresRed = 0)
        {
            if (count == 0)
                count = 1;
            if (HaveSpace(count))
            {
                for (byte x = 0; x < count; x++)
                {
                    Database.ItemType.DBItem DbItem;
                    if (Database.Server.ItemsBase.TryGetValue(ID, out DbItem))
                    {
                        Game.MsgServer.MsgGameItem ItemDat = new Game.MsgServer.MsgGameItem();
                        ItemDat.UID = Database.Server.ITEM_Counter.Next;
                        ItemDat.ITEM_ID = ID;

                        ItemDat.ProgresGreen = ProgresGreen;
                        ItemDat.Enchant = ProgresBlue;
                        ItemDat.Bless = ProgresRed;
                        ItemDat.SocketProgress = (uint)(ProgresGreen | (ProgresBlue << 8) | (ProgresRed << 16));
                        ItemDat.Durability = ItemDat.MaximDurability = DbItem.Durability;
                        ItemDat.Plus = plus;
                        ItemDat.Color = (Role.Flags.Color)Program.GetRandom.Next(3, 9);
                        ItemDat.Bound = (byte)(bound ? 1 : 0);
                        try
                        {
                            if (!Update(ItemDat, AddMode.ADD, stream))
                                return false;
                        }
                        catch (Exception e)
                        {
                            Console.SaveException(e);
                        }
                        if (x >= count)
                            return true;
                    }
                }
            }
            return false;
        }
        public bool Add(ServerSockets.Packet stream, uint ID, byte count = 1, byte plus = 0, byte bless = 0, byte Enchant = 0
            , Role.Flags.Gem sockone = Flags.Gem.NoSocket
             , Role.Flags.Gem socktwo = Flags.Gem.NoSocket, bool bound = false, Role.Flags.ItemEffect Effect = Flags.ItemEffect.None, bool SendMessage = false
            , string another_text = "", int DaysActive = 0)
        {
            if (count == 0)
                count = 1;
            if (HaveSpace(count))
            {
                byte x = 0;
                for (; x < count;)
                {
                    x++;
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
                        //ItemDat.RemainingTime = (DbItem.StackSize > 1) ? 0 : uint.MaxValue;

                        // Console.WriteLine(Database.ItemType.GetItemPoints(DbItem, ItemDat));
                        if (SendMessage)
                        {
                            Owner.CreateBoxDialog("You~received~a~" + DbItem.Name + "" + another_text);
                        }

                        try
                        {
                            if (!Update(ItemDat, AddMode.ADD, stream))
                                return false;
                        }
                        catch (Exception e)
                        {
                            Console.SaveException(e);
                        }

                    }
                }
                if (x >= count)
                    return true;
            }

            return false;
        }

        public bool AddItemTime(ServerSockets.Packet stream, uint ID, byte count = 1, byte plus = 0, byte bless = 0, byte Enchant = 0
           , Role.Flags.Gem sockone = Flags.Gem.NoSocket
            , Role.Flags.Gem socktwo = Flags.Gem.NoSocket, bool bound = false,
            Role.Flags.ItemEffect Effect = Flags.ItemEffect.None, bool SendMessage = false
           , string another_text = "", int days = 0, int hours = 0, int mins = 0)
        {
            if (count == 0)
                count = 1;
            if (HaveSpace(count))
            {
                byte x = 0;
                for (; x < count;)
                {
                    x++;
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

                        if (days != 0)
                        {
                            ItemDat.Activate = 1;
                            ItemDat.EndDate = DateTime.Now.AddDays(days);
                            Owner.SendSysMesage($"You've got {Database.Server.ItemsBase.GetItemName(ItemDat.ITEM_ID)} for {days} days!", MsgMessage.ChatMode.Talk);

                        }
                        else if (hours != 0)
                        {
                            ItemDat.Activate = 1;
                            ItemDat.EndDate = DateTime.Now.AddHours(hours);
                            Owner.SendSysMesage($"You've got {Database.Server.ItemsBase.GetItemName(ItemDat.ITEM_ID)} for {hours} hours!", MsgMessage.ChatMode.Talk);
                        }
                        else if (mins != 0)
                        {
                            ItemDat.Activate = 1;
                            ItemDat.EndDate = DateTime.Now.AddMinutes(mins);
                            Owner.SendSysMesage($"You've got {Database.Server.ItemsBase.GetItemName(ItemDat.ITEM_ID)} for {mins} minutes!", MsgMessage.ChatMode.Talk);
                        }


                        if (SendMessage)
                        {
                            Owner.CreateBoxDialog("You~received~a~" + DbItem.Name + "" + another_text);
                        }

                        try
                        {
                            if (!Update(ItemDat, AddMode.ADD, stream))
                                return false;
                        }
                        catch (Exception e)
                        {
                            Console.SaveException(e);
                        }

                    }
                }
                if (x >= count)
                    return true;
            }

            return false;
        }
        public bool AddItemWitchStack(uint ID, byte Plus, ushort amount, ServerSockets.Packet stream, bool bound = false)
        {
            //return Add(stream, ID, (byte)amount, Plus, 0, 0, Flags.Gem.NoSocket, Flags.Gem.NoSocket, bound);
            Database.ItemType.DBItem DbItem;
            if (Database.Server.ItemsBase.TryGetValue(ID, out DbItem))
            {

                if (DbItem.StackSize > 0)
                {
                    byte _bound = 0;
                    if (bound)
                        _bound = 1;
                    foreach (var item in ClientItems.Values)
                    {

                        if (item.ITEM_ID == ID && item.Bound == _bound)
                        {
                            if (item.StackSize + amount <= DbItem.StackSize)
                            {
                                item.Mode = Flags.ItemMode.Update;
                                item.StackSize += amount;
                                if (bound)
                                    item.Bound = 1;
                                //item.RemainingTime = (DbItem.StackSize > 1) ? 0 : uint.MaxValue;
                                item.Send(Owner, stream);

                                return true;
                            }
                        }
                    }

                    if (amount > DbItem.StackSize)
                    {
                        if (HaveSpace((byte)((amount / DbItem.StackSize))))// 10
                        {
                            while (amount >= DbItem.StackSize)
                            {
                                Game.MsgServer.MsgGameItem ItemDat = new Game.MsgServer.MsgGameItem();
                                ItemDat.UID = Database.Server.ITEM_Counter.Next;
                                ItemDat.ITEM_ID = ID;
                                ItemDat.Durability = ItemDat.MaximDurability = DbItem.Durability;
                                ItemDat.Plus = Plus;
                                ItemDat.StackSize += DbItem.StackSize;
                                ItemDat.Color = (Role.Flags.Color)Program.GetRandom.Next(3, 9);
                                //ItemDat.RemainingTime = (DbItem.StackSize > 1) ? 0 : uint.MaxValue;
                                if (bound)
                                    ItemDat.Bound = 1;
                                try
                                {
                                    Update(ItemDat, AddMode.ADD, stream);
                                }
                                catch (Exception e)
                                {
                                    Console.SaveException(e);
                                }
                                amount -= DbItem.StackSize;

                            }
                            if (amount > 0 && amount < DbItem.StackSize)
                            {
                                Game.MsgServer.MsgGameItem ItemDat = new Game.MsgServer.MsgGameItem();
                                ItemDat.UID = Database.Server.ITEM_Counter.Next;
                                ItemDat.ITEM_ID = ID;
                                ItemDat.Durability = ItemDat.MaximDurability = DbItem.Durability;
                                ItemDat.Plus = Plus;
                                ItemDat.StackSize += amount;
                                //ItemDat.RemainingTime = (DbItem.StackSize > 1) ? 0 : uint.MaxValue;
                                ItemDat.Color = (Role.Flags.Color)Program.GetRandom.Next(3, 9);
                                if (bound)
                                    ItemDat.Bound = 1;
                                try
                                {
                                    Update(ItemDat, AddMode.ADD, stream);
                                }
                                catch (Exception e)
                                {
                                    Console.SaveException(e);
                                }
                            }
                            return true;
                        }
                        else
                        {
                            while (amount >= DbItem.StackSize)
                            {
                                AddReturnedItem(stream, ID, 1, Plus, 0, 0, Flags.Gem.NoSocket, Flags.Gem.NoSocket, bound, Flags.ItemEffect.None, DbItem.StackSize);
                                amount -= DbItem.StackSize;
                            }
                            if (amount > 0 && amount < DbItem.StackSize)
                            {
                                AddReturnedItem(stream, ID, 1, Plus, 0, 0, Flags.Gem.NoSocket, Flags.Gem.NoSocket, bound, Flags.ItemEffect.None, amount);
                            }
                            return true;
                        }
                    }
                    else
                    {
                        if (HaveSpace(1))
                        {
                            Game.MsgServer.MsgGameItem ItemDat = new Game.MsgServer.MsgGameItem();
                            ItemDat.UID = Database.Server.ITEM_Counter.Next;
                            ItemDat.ITEM_ID = ID;
                            ItemDat.Durability = ItemDat.MaximDurability = DbItem.Durability;
                            ItemDat.Plus = Plus;
                            ItemDat.StackSize = amount;
                            ItemDat.Color = (Role.Flags.Color)Program.GetRandom.Next(3, 9);
                            //ItemDat.RemainingTime = (DbItem.StackSize > 1) ? 0 : uint.MaxValue;
                            if (bound)
                                ItemDat.Bound = 1;
                            try
                            {
                                Update(ItemDat, AddMode.ADD, stream);
                            }
                            catch (Exception e)
                            {
                                Console.SaveException(e);
                            }
                            return true;
                        }
                    }
                }
                for (int count = 0; count < amount; count++)
                    Add(ID, Plus, DbItem, stream, bound);
                return true;
            }
            return false;
        }
        public bool RemoveStackItem(uint UID, ushort Count, ServerSockets.Packet stream)
        {
            Game.MsgServer.MsgGameItem ItemDat;
            if (ClientItems.TryGetValue(UID, out ItemDat))
            {
                if (ItemDat.StackSize > Count)
                {
                    ItemDat.StackSize -= Count;
                    ItemDat.Mode = Flags.ItemMode.Update;
                    ItemDat.Send(Owner, stream);
                }
                else
                {
                    ItemDat.StackSize = 1;
                    Update(ItemDat, AddMode.REMOVE, stream);
                    return true;
                }
            }
            else
            {

                foreach (var item in ClientItems.Values)
                {
                    if (0 == Count)
                        break;
                    if (item.ITEM_ID == UID)
                    {
                        if (item.StackSize > Count)
                        {
                            item.StackSize -= Count;
                            item.Mode = Flags.ItemMode.Update;
                            item.Send(Owner, stream);
                            Count = 0;
                        }
                        else
                        {
                            Count -= item.StackSize;
                            item.StackSize = 1;
                            Update(item, AddMode.REMOVE, stream);
                        }
                    }
                }
            }
            return false;
        }
        public bool Add(uint ID, byte Plus, Database.ItemType.DBItem ITEMDB, ServerSockets.Packet stream, bool bound = false)
        {
            if (ITEMDB.StackSize > 0)
            {
                byte _bound = 0;
                if (bound)
                    _bound = 1;
                foreach (var item in ClientItems.Values)
                {

                    if (item.ITEM_ID == ID && item.Bound == _bound)
                    {
                        if (item.StackSize < ITEMDB.StackSize)
                        {
                            item.Mode = Flags.ItemMode.Update;
                            item.StackSize++;
                            //item.RemainingTime = (ITEMDB.StackSize > 1) ? 0 : uint.MaxValue;
                            if (bound)
                                item.Bound = 1;
                            item.Send(Owner, stream);

                            return true;
                        }
                    }
                }
            }
            if (HaveSpace(1))
            {
                Game.MsgServer.MsgGameItem ItemDat = new Game.MsgServer.MsgGameItem();
                ItemDat.UID = Database.Server.ITEM_Counter.Next;
                ItemDat.ITEM_ID = ID;
                ItemDat.Durability = ItemDat.MaximDurability = ITEMDB.Durability;
                ItemDat.Plus = Plus;
                ItemDat.Color = (Role.Flags.Color)Program.GetRandom.Next(3, 9);
                //ItemDat.RemainingTime = (ITEMDB.StackSize > 1) ? 0 : uint.MaxValue;
                if (bound)
                    ItemDat.Bound = 1;
                try
                {
                    Update(ItemDat, AddMode.ADD, stream);
                }
                catch (Exception e)
                {
                    Console.SaveException(e);
                }
                return true;
            }
            return false;

        }
        public bool Add(Game.MsgServer.MsgGameItem ItemDat, Database.ItemType.DBItem ITEMDB, ServerSockets.Packet stream)
        {
            if (ITEMDB.StackSize > 0)
            {
                foreach (var item in ClientItems.Values)
                {
                    if (item.ITEM_ID == ItemDat.ITEM_ID)
                    {
                        if (item.StackSize < ITEMDB.StackSize)
                        {
                            item.Mode = Flags.ItemMode.Update;
                            item.StackSize++;
                            //ItemDat.RemainingTime = (ITEMDB.StackSize > 1) ? 0 : uint.MaxValue;
                            item.Send(Owner, stream);
                            return true;
                        }
                    }
                }
            }
            if (HaveSpace(1))
            {
                //ItemDat.RemainingTime = (ITEMDB.StackSize > 1) ? 0 : uint.MaxValue;
                Update(ItemDat, AddMode.ADD, stream);
                return true;
            }
            return false;

        }
        public bool AddItemWitchStack(Game.MsgServer.MsgGameItem ItemDat, byte amount, ServerSockets.Packet stream)
        {
            Database.ItemType.DBItem DbItem;
            if (Database.Server.ItemsBase.TryGetValue(ItemDat.ITEM_ID, out DbItem))
            {
                for (int count = 0; count < amount; count++)
                    Add(ItemDat, DbItem, stream);
                return true;
            }
            return false;
        }
        public unsafe bool Update(Game.MsgServer.MsgGameItem ItemDat, AddMode mode, ServerSockets.Packet stream, bool Removefull = false)
        {

            if (HaveSpace(1) || mode == AddMode.REMOVE)
            {
                string logs = "[Item]" + Owner.Player.Name + " [" + mode + "] [" + ItemDat.UID + "]" + ItemDat.ITEM_ID + " plus [" + ItemDat.Plus + "]";
                Database.ServerDatabase.LoginQueue.Enqueue(logs);
                switch (mode)
                {
                    case AddMode.ADD:
                        {
                            CheakUp(ItemDat);
                            if (ItemDat.StackSize == 0)
                                ItemDat.StackSize = 1;
                            ItemDat.Position = 0;
                            ItemDat.Mode = Flags.ItemMode.AddItem;
                            ItemDat.Send(Owner, stream);
                            break;
                        }
                    case AddMode.MOVE:
                        {
                            CheakUp(ItemDat);
                            ItemDat.Position = 0;
                            ItemDat.Mode = Flags.ItemMode.AddItem;
                            ItemDat.Send(Owner, stream);
                            break;
                        }
                    case AddMode.REMOVE:
                        {
                            if (ItemDat.StackSize > 1 && ItemDat.Position < 40 && !Removefull)
                            {
                                ItemDat.StackSize -= 1;
                                ItemDat.Mode = Flags.ItemMode.Update;
                                ItemDat.Send(Owner, stream);
                                break;
                            }
                            Game.MsgServer.MsgGameItem item;
                            if (ClientItems.TryRemove(ItemDat.UID, out item))
                            {
                                Owner.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsuageID.RemoveInventory, item.UID, 0, 0, 0, 0, 0));
                            }
                            break;
                        }
                }
                if (ItemDat.ITEM_ID == 750000)
                {
                    Owner.DemonExterminator.ItemUID = ItemDat.UID;
                    if (mode == AddMode.REMOVE)
                        Owner.DemonExterminator.ItemUID = 0;
                }
                /* try
                 {
                     StackTrace stackTrace = new StackTrace();           // get call stack
                     StackFrame[] stackFrames = stackTrace.GetFrames();  // get method calls (frames)

                     string data = "[CallStack]" + Owner.Player.Name + " " + ItemDat.ITEM_ID + " "+ItemDat.UID+" \n";
                     // write call stack method names
                     foreach (StackFrame stackFrame in stackFrames)
                     {
                         data += stackFrame.GetMethod().Name + " " + stackFrame.GetMethod().DeclaringType.Name + " ";   
                  
                     }

                     data += Environment.StackTrace;

                     Database.ServerDatabase.LoginQueue.Enqueue(data);
                  
                 }
                 catch (Exception e)
                 {
                     MyConsole.SaveException(e);
                 }*/

                return true;

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
                    ItemDat.UID = Database.Server.ITEM_Counter.Next;
                while
                  (ClientItems.TryAdd(ItemDat.UID, ItemDat) == false);
            }
        }

        public bool CheckMeteors(byte count, bool Removethat, ServerSockets.Packet stream)
        {

            if (Contain(1088001, count))
            {
                if (Removethat)
                    Remove(1088001, count, stream);
                return true;
            }
            else
            {
                byte Counter = 0;
                var RemoveThis = new Dictionary<uint, Game.MsgServer.MsgGameItem>();
                var MyMetscrolls = GetMyMetscrolls();
                var MyMeteors = GetMyMeteors();
                foreach (var GameItem in MyMetscrolls.Values)
                {
                    Counter += 10;
                    RemoveThis.Add(GameItem.UID, GameItem);
                    if (Counter >= count)
                        break;
                }
                if (Counter >= count)
                {
                    byte needSpace = (byte)(Counter - count);
                    if (HaveSpace(needSpace))
                    {
                        if (Removethat)
                        {
                            Add(stream, 1088001, needSpace);
                        }
                    }
                    else
                    {
                        Counter -= 10;
                        RemoveThis.Remove(RemoveThis.Values.First().UID);
                        byte needmetsss = (byte)(count - Counter);
                        if (needmetsss <= MyMeteors.Count)
                        {
                            foreach (var GameItem in MyMeteors.Values)
                            {
                                Counter += 1;
                                RemoveThis.Add(GameItem.UID, GameItem);
                                if (Counter >= count)
                                    break;
                            }
                            if (Removethat)
                            {
                                foreach (var GameItem in RemoveThis.Values)
                                    Update(GameItem, AddMode.REMOVE, stream);
                            }
                        }
                        else
                            return false;
                    }
                    if (Removethat)
                    {
                        foreach (var GameItem in RemoveThis.Values)
                            Update(GameItem, AddMode.REMOVE, stream);
                    }
                    return true;
                }
                foreach (var GameItem in MyMeteors.Values)
                {
                    Counter += 1;
                    RemoveThis.Add(GameItem.UID, GameItem);
                    if (Counter >= count)
                        break;
                }
                if (Counter >= count)
                {
                    if (Removethat)
                    {
                        foreach (var GameItem in RemoveThis.Values)
                            Update(GameItem, AddMode.REMOVE, stream);
                    }
                    return true;
                }
            }

            return false;
        }
        private Dictionary<uint, Game.MsgServer.MsgGameItem> GetMyMetscrolls()
        {
            var array = new Dictionary<uint, Game.MsgServer.MsgGameItem>();
            foreach (var GameItem in ClientItems.Values)
            {
                if (GameItem.ITEM_ID == 720027)
                {
                    if (!array.ContainsKey(GameItem.UID))
                        array.Add(GameItem.UID, GameItem);
                }
            }
            return array;
        }
        private Dictionary<uint, Game.MsgServer.MsgGameItem> GetMyMeteors()
        {
            var array = new Dictionary<uint, Game.MsgServer.MsgGameItem>();
            foreach (var GameItem in ClientItems.Values)
            {
                if (GameItem.ITEM_ID == Database.ItemType.Meteor || GameItem.ITEM_ID == Database.ItemType.MeteorTear)
                {
                    if (!array.ContainsKey(GameItem.UID))
                        array.Add(GameItem.UID, GameItem);
                }
            }
            return array;
        }


        public void ShowALL(ServerSockets.Packet stream)
        {
            foreach (var msg_item in ClientItems.Values)
            {
                msg_item.Mode = Flags.ItemMode.AddItem;
                msg_item.Send(Owner, stream);
            }
        }
        public void Clear(ServerSockets.Packet stream)
        {
            var dictionary = ClientItems.Values.ToArray();
            foreach (var msg_item in dictionary)
                Update(msg_item, AddMode.REMOVE, stream, true);
        }
    }
}
