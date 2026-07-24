using COServer.Client;
using System;
using System.Collections.Generic;

namespace COServer.Game.MsgServer
{
    public class SuperDrop
    {
        public static List<uint> SuperDropItems = new List<uint>()
        {
            113013, 114023, 117003, 118003, 120003, 121003, 130020, 131013,
            133003, 134003, 141003, 142003, 150003, 152013, 160013, 410003,
            420003, 421003, 500003, 561003,
            Database.ItemType.Meteor
        };

        public static MsgFloorItem.MsgItem GenerateSuperDropItem(
            ServerSockets.Packet stream, Client.GameClient killer, Role.GameMap map,
            ushort x, ushort y, uint dynamicId)
        {
            uint itemId = SuperDropItems[Program.GetRandom.Next(0, SuperDropItems.Count)];
            MsgGameItem dataItem = new MsgGameItem();
            dataItem.ITEM_ID = itemId;
            dataItem.Durability = 100;
            dataItem.MaximDurability = 100;

            dataItem.Color = Role.Flags.Color.Red;

            // Determina se será Plus (99%) ou Bless (1%)
            int attributeChance = Program.GetRandom.Next(0, 100);
            if (attributeChance < 99) // 99% de chance de ter Plus
            {
                if (Role.Core.Rate(100)) 
                {
                    dataItem.Plus = 1;
                }
            }
            else // 1% de chance de ter Bless
            {
                int blessType = Program.GetRandom.Next(0, 100);
                if (blessType < 99) // 99% de chance de Bless 1 dentro do 1%
                {
                    dataItem.Bless = 1;
                }
                else // 1% de chance de Bless 3 dentro do 1%
                {
                    dataItem.Bless = 3;
                }
            }

            // Criar o MsgFloorItem.MsgItem com todos os 11 parâmetros
            MsgFloorItem.MsgItem dropItem = new MsgFloorItem.MsgItem(
                dataItem,           // 1: MsgGameItem
                x,                  // 2: ushort (X)
                y,                  // 3: ushort (Y)
                MsgFloorItem.MsgItem.ItemType.Item, // 4: ItemType
                1,                  // 5: uint (amount)
                dynamicId,          // 6: uint (DynamicID)
                map.ID,             // 7: uint (mapId)
                killer.Player.UID,  // 8: uint (OwnerUID)
                true,               // 9: bool (pickable)
                map,                // 10: GameMap (_map)
                0                   // 11: int (assumido como 0)
            );

            return dropItem;
        }
    }
}