using COServer.Game.MsgNpc;
using COServer.Game.MsgServer;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace COServer.Role.Instance
{
    public class Warehouse
    {
        public const byte Max_Count = 40;//40

        public static bool IsWarehouse(Game.MsgNpc.NpcID ID)
        {
            return (ID == Game.MsgNpc.NpcID.WHTwin || ID == Game.MsgNpc.NpcID.wHPheonix
                              || ID == Game.MsgNpc.NpcID.WHMarket || ID == Game.MsgNpc.NpcID.WHBird
                              || ID == Game.MsgNpc.NpcID.WHDesert || ID == Game.MsgNpc.NpcID.WHApe
                              || ID == Game.MsgNpc.NpcID.WHPoker || ID == Game.MsgNpc.NpcID.WHStone
                              || ID == Game.MsgNpc.NpcID.WHMarket2 || ID == Game.MsgNpc.NpcID.WHMarket3
                              || ID == Game.MsgNpc.NpcID.WHMarket4 || ID == Game.MsgNpc.NpcID.WHMarket5
                              || ID == Game.MsgNpc.NpcID.WHMarket6
                              || ID == (Game.MsgNpc.NpcID)ushort.MaxValue);
        }


        public byte WHMaxSpace()
        {
            return Max_Count;//(byte)((User.Player.VipLevel != 0) ? 80 : Max_Count);
        }


        public bool HaveItemsInBanks()
        {

            foreach (var bank in ClientItems.Values)
            {
                if (bank.Count > 0)
                    return true;
            }
            return false;
        }

        public ConcurrentDictionary<uint, ConcurrentDictionary<uint, Game.MsgServer.MsgGameItem>> ClientItems;
        public List<uint> IsShow = new List<uint>();
        public Client.GameClient User;
        public Warehouse(Client.GameClient client)
        {
            ClientItems = new ConcurrentDictionary<uint, ConcurrentDictionary<uint, Game.MsgServer.MsgGameItem>>();
            User = client;
        }

        public void SendReturnedItems(ServerSockets.Packet stream)
        {
            ConcurrentDictionary<uint, Game.MsgServer.MsgGameItem> wh_items;
            if (ClientItems.TryGetValue(ushort.MaxValue, out wh_items))
            {
                foreach (var item in wh_items.Values)
                {
                    item.Mode = Flags.ItemMode.AddItemReturned;
                    item.Send(User, stream);
                }
            }
        }


        public bool AddItem(Game.MsgServer.MsgGameItem DataItem, uint NpcID)
        {
            if (!ClientItems.ContainsKey(NpcID))
                ClientItems.TryAdd(NpcID, new ConcurrentDictionary<uint, Game.MsgServer.MsgGameItem>());

            if (ClientItems[NpcID].TryAdd(DataItem.UID, DataItem))
            {
                DataItem.WH_ID = NpcID;
                return true;
            }
            return false;
        }
        public bool AddVIPItem(Game.MsgServer.MsgGameItem DataItem, out string NpcName)
        {
            NpcName = "";

            if (DataItem.UID == 0)
                DataItem.UID = Database.Server.ITEM_Counter.Next;

            if (!ClientItems.ContainsKey((uint)NpcID.WHTwin))
                ClientItems.TryAdd((uint)NpcID.WHTwin, new ConcurrentDictionary<uint, Game.MsgServer.MsgGameItem>());

            if (!ClientItems.ContainsKey((uint)NpcID.wHPheonix))
                ClientItems.TryAdd((uint)NpcID.wHPheonix, new ConcurrentDictionary<uint, Game.MsgServer.MsgGameItem>());

            if (!ClientItems.ContainsKey((uint)NpcID.WHMarket))
                ClientItems.TryAdd((uint)NpcID.WHMarket, new ConcurrentDictionary<uint, Game.MsgServer.MsgGameItem>());

            if (!ClientItems.ContainsKey((uint)NpcID.WHMarket2))
                ClientItems.TryAdd((uint)NpcID.WHMarket2, new ConcurrentDictionary<uint, Game.MsgServer.MsgGameItem>());

            if (!ClientItems.ContainsKey((uint)NpcID.WHMarket3))
                ClientItems.TryAdd((uint)NpcID.WHMarket3, new ConcurrentDictionary<uint, Game.MsgServer.MsgGameItem>());

            if (!ClientItems.ContainsKey((uint)NpcID.WHMarket4))
                ClientItems.TryAdd((uint)NpcID.WHMarket4, new ConcurrentDictionary<uint, Game.MsgServer.MsgGameItem>());

            if (!ClientItems.ContainsKey((uint)NpcID.WHMarket5))
                ClientItems.TryAdd((uint)NpcID.WHMarket5, new ConcurrentDictionary<uint, Game.MsgServer.MsgGameItem>());

            if (!ClientItems.ContainsKey((uint)NpcID.WHMarket6))
                ClientItems.TryAdd((uint)NpcID.WHMarket6, new ConcurrentDictionary<uint, Game.MsgServer.MsgGameItem>());

            if (!ClientItems.ContainsKey((uint)NpcID.WHBird))
                ClientItems.TryAdd((uint)NpcID.WHBird, new ConcurrentDictionary<uint, Game.MsgServer.MsgGameItem>());

            if (!ClientItems.ContainsKey((uint)NpcID.WHDesert))
                ClientItems.TryAdd((uint)NpcID.WHDesert, new ConcurrentDictionary<uint, Game.MsgServer.MsgGameItem>());

            if (!ClientItems.ContainsKey((uint)NpcID.WHApe))
                ClientItems.TryAdd((uint)NpcID.WHApe, new ConcurrentDictionary<uint, Game.MsgServer.MsgGameItem>());

            if (ClientItems[(uint)NpcID.WHTwin].Count < Max_Count)
            {
                if (ClientItems[(uint)NpcID.WHTwin].TryAdd(DataItem.UID, DataItem))
                {
                    DataItem.WH_ID = (uint)NpcID.WHTwin;
                    NpcName = "Twin City";
                    User.SendSysMesage($"You've successfully transferred {Database.Server.ItemsBase[DataItem.ITEM_ID].Name} into {NpcName} Warehouse.");
                    return true;
                }
            }
            else if(ClientItems[(uint)NpcID.wHPheonix].Count < Max_Count)
            {
                if (ClientItems[(uint)NpcID.wHPheonix].TryAdd(DataItem.UID, DataItem))
                {
                    DataItem.WH_ID = (uint)NpcID.wHPheonix;
                    NpcName = "Phoenix Castle";
                    User.SendSysMesage($"You've successfully transferred {Database.Server.ItemsBase[DataItem.ITEM_ID].Name} into {NpcName} Warehouse.");
                    return true;
                }
            }
            else if (ClientItems[(uint)NpcID.WHMarket].Count < Max_Count)
            {
                if (ClientItems[(uint)NpcID.WHMarket].TryAdd(DataItem.UID, DataItem))
                {
                    DataItem.WH_ID = (uint)NpcID.WHMarket;
                    NpcName = "Market";
                    User.SendSysMesage($"You've successfully transferred {Database.Server.ItemsBase[DataItem.ITEM_ID].Name} into {NpcName} Warehouse.");
                    return true;
                }
            }
            else if (ClientItems[(uint)NpcID.WHMarket2].Count < Max_Count)
            {
                if (ClientItems[(uint)NpcID.WHMarket2].TryAdd(DataItem.UID, DataItem))
                {
                    DataItem.WH_ID = (uint)NpcID.WHMarket2;
                    NpcName = "Market";
                    User.SendSysMesage($"You've successfully transferred {Database.Server.ItemsBase[DataItem.ITEM_ID].Name} into {NpcName} Warehouse.");
                    return true;
                }
            }
            else if (ClientItems[(uint)NpcID.WHMarket3].Count < Max_Count)
            {
                if (ClientItems[(uint)NpcID.WHMarket3].TryAdd(DataItem.UID, DataItem))
                {
                    DataItem.WH_ID = (uint)NpcID.WHMarket3;
                    NpcName = "Market";
                    User.SendSysMesage($"You've successfully transferred {Database.Server.ItemsBase[DataItem.ITEM_ID].Name} into {NpcName} Warehouse.");
                    return true;
                }
            }
            else if (ClientItems[(uint)NpcID.WHMarket4].Count < Max_Count)
            {
                if (ClientItems[(uint)NpcID.WHMarket4].TryAdd(DataItem.UID, DataItem))
                {
                    DataItem.WH_ID = (uint)NpcID.WHMarket4;
                    NpcName = "Market";
                    User.SendSysMesage($"You've successfully transferred {Database.Server.ItemsBase[DataItem.ITEM_ID].Name} into {NpcName} Warehouse.");
                    return true;
                }
            }
            else if (ClientItems[(uint)NpcID.WHMarket5].Count < Max_Count)
            {
                if (ClientItems[(uint)NpcID.WHMarket5].TryAdd(DataItem.UID, DataItem))
                {
                    DataItem.WH_ID = (uint)NpcID.WHMarket5;
                    NpcName = "Market";
                    User.SendSysMesage($"You've successfully transferred {Database.Server.ItemsBase[DataItem.ITEM_ID].Name} into {NpcName} Warehouse.");
                    return true;
                }
            }
            else if (ClientItems[(uint)NpcID.WHMarket6].Count < Max_Count)
            {
                if (ClientItems[(uint)NpcID.WHMarket6].TryAdd(DataItem.UID, DataItem))
                {
                    DataItem.WH_ID = (uint)NpcID.WHMarket6;
                    NpcName = "Market";
                    User.SendSysMesage($"You've successfully transferred {Database.Server.ItemsBase[DataItem.ITEM_ID].Name} into {NpcName} Warehouse.");
                    return true;
                }
            }
            else if (ClientItems[(uint)NpcID.WHBird].Count < Max_Count)
            {
                if (ClientItems[(uint)NpcID.WHBird].TryAdd(DataItem.UID, DataItem))
                {
                    DataItem.WH_ID = (uint)NpcID.WHBird;
                    NpcName = "Bird Island";
                    User.SendSysMesage($"You've successfully transferred {Database.Server.ItemsBase[DataItem.ITEM_ID].Name} into {NpcName} Warehouse.");
                    return true;
                }
            }
            else if (ClientItems[(uint)NpcID.WHDesert].Count < Max_Count)
            {
                if (ClientItems[(uint)NpcID.WHDesert].TryAdd(DataItem.UID, DataItem))
                {
                    DataItem.WH_ID = (uint)NpcID.WHDesert;
                    NpcName = "Desert City";
                    User.SendSysMesage($"You've successfully transferred {Database.Server.ItemsBase[DataItem.ITEM_ID].Name} into {NpcName} Warehouse.");
                    return true;
                }
            }
            else if (ClientItems[(uint)NpcID.WHApe].Count < Max_Count)
            {
                if (ClientItems[(uint)NpcID.WHApe].TryAdd(DataItem.UID, DataItem))
                {
                    DataItem.WH_ID = (uint)NpcID.WHApe;
                    NpcName = "Ape City";
                    User.SendSysMesage($"You've successfully transferred {Database.Server.ItemsBase[DataItem.ITEM_ID].Name} into {NpcName} Warehouse.");
                    return true;
                }
            }

            return false;
        }
        public unsafe bool RemoveItemScroll(uint UID, uint NpcID, ServerSockets.Packet stream)
        {
            if (ClientItems.ContainsKey(NpcID))
            {

                Game.MsgServer.MsgGameItem item;
                if (ClientItems[NpcID].TryRemove(UID, out item))
                {
                    item.Position = 0;
                    item.WH_ID = 0;
                    User.Inventory.Update(item, AddMode.REMOVE, stream);

                    return true;
                }

            }
            return false;
        }

        public unsafe bool RemoveItem(uint UID, uint NpcID, ServerSockets.Packet stream)
        {
            if (ClientItems.ContainsKey(NpcID))
            {
                if (User.Inventory.HaveSpace(1))
                {
                    Game.MsgServer.MsgGameItem item;
                    if (ClientItems[NpcID].TryRemove(UID, out item))
                    {
                        item.Position = 0;
                        item.WH_ID = 0;
                        User.Inventory.Update(item, AddMode.ADD, stream);
                        return true;
                    }
                }
                else
                {
                    User.SendSysMesage("Your inventory is full!");
                }
            }
            return false;
        }
        public unsafe void Show(uint NpcID, Game.MsgServer.MsgWarehouse.DepositActionID Action, ServerSockets.Packet stream)
        {
            if (ClientItems.ContainsKey(NpcID) /*&& !IsShow.Contains(NpcID)*/)
            {
                //IsShow.Add(NpcID);

                Dictionary<int, List<Game.MsgServer.MsgGameItem>> Queues = new Dictionary<int, List<Game.MsgServer.MsgGameItem>>();
                Queues.Add(0, new List<Game.MsgServer.MsgGameItem>());

                int count = 0;
                var Array = ClientItems[NpcID].Values.ToArray();
                for (uint x = 0; x < Array.Length; x++)
                {
                    //if (x % 8 == 0)
                    //{
                    //count++;
                    //Queues.Add(count, new List<Game.MsgServer.MsgGameItem>());
                    //}
                    Queues[count].Add(Array[x]);
                }

                foreach (var aray in Queues.Values)
                {
                    stream.WarehouseCreate(NpcID, Action, 0, WHMaxSpace(), aray.Count);

                    foreach (var item in aray)
                    {
                        stream.AddItemWarehouse(item);

                    }
                    User.Send(stream.FinalizeWarehouse());
                }
            }
        }
    }
}
