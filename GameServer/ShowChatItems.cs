using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COServer
{
    public class ShowChatItems
    {
        public static void ShowFakeItem(Client.GameClient user, ServerSockets.Packet stream, uint ItemID, ushort Durability)
        {
            Game.MsgServer.MsgGameItem GameItem = new Game.MsgServer.MsgGameItem();
            GameItem.UID = 1;
            GameItem.Durability = GameItem.MaximDurability = Durability;
            GameItem.ITEM_ID = ItemID;
            GameItem.Mode = Role.Flags.ItemMode.ChatItem;
            GameItem.Send(user, stream);
        }
        public class Item
        {
            public DateTime Stamp = new DateTime();
            public Game.MsgServer.MsgGameItem aItem = null;
        }
        public System.SafeDictionary<uint, Item> Items;

        public ShowChatItems()
        {
            Items = new System.SafeDictionary<uint, Item>();
        }

        public void Add(Game.MsgServer.MsgGameItem GameItem)
        {
            if (!Items.ContainsKey(GameItem.UID))
            {
                Items.Add(GameItem.UID, new Item()
                {
                    Stamp = DateTime.Now.AddHours(5),
                    aItem = GameItem
                });
            }
        }

        public void Work()
        {
            try
            {
                List<uint> remover = new List<uint>();

                foreach (var item in Items.GetValues())
                {
                    if (DateTime.Now > item.Stamp)
                    {
                        remover.Add(item.aItem.UID);
                    }
                }

                foreach (var rem in remover)
                    Items.Remove(rem);
            }

            catch (Exception e)
            {
                Console.WriteException(e);
            }
        }
    }
}
