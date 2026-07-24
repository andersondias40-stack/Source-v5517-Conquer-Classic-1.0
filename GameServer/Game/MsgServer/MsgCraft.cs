using System;
using System.Collections.Generic;
using System.Text;

namespace COServer.Game.MsgServer
{
    public static class MsgCraft
    {
        public enum Action : byte
        {
            Open = 0
        }
        public static unsafe void CraftItem(this ServerSockets.Packet stream, out uint UID, out Action mode)
        {
            UID = stream.ReadUInt32();
            mode = (Action)stream.ReadInt8();

        }
        [PacketAttribute(1028)]
        public unsafe static void MsgCraftItem(Client.GameClient Client, ServerSockets.Packet stream)
        {
            //Int32[] item;
            //uint reward = 0;
            //for (int i = 0; i < BitConverter.ToInt16(Packet.Buffer, 10); i++)
            //{
            //    item = new Int32[BitConverter.ToInt16(Packet.Buffer, 10)];
            //    item[i] = BitConverter.ToInt32(Packet.Buffer, 12 + (i * 4));
            //    ItemDataPacket Item = Client.GetInventoryItemByUID((uint)item[i]);
            //    if (Item.ID != 0)
            //    {
            //        reward += Kernel.ItemData[Item.ID].GoldPrice / 3;
            //        Client.RemoveInventory((uint)item[i]);
            //    }
            //}
            //Client.Send(new ChatPacket("You've sold items for " + reward + " silvers!", ChatType.TopLeft));
            //Client.Money += reward;


        }
    }
}