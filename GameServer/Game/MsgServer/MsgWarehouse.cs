namespace COServer.Game.MsgServer
{
    public static unsafe partial class MsgBuilder
    {
        public static unsafe void GetWarehouse(this ServerSockets.Packet stream, out uint NpcID, out MsgWarehouse.DepositActionID Action, out uint ItemUID)
        {
            NpcID = stream.ReadUInt32();
            Action = (MsgWarehouse.DepositActionID)stream.ReadUInt32();
            ItemUID = stream.ReadUInt32();
            //uint file_size = stream.ReadUInt32();
            //  Console.WriteLine("ItemUID " + stream.Position);

        }
        public static unsafe ServerSockets.Packet WarehouseCreate(this ServerSockets.Packet stream, uint NpcID, MsgWarehouse.DepositActionID Action, uint ItemUID, int File_Size, int count)
        {
            stream.InitWriter();
            stream.Write(NpcID);
            stream.Write((uint)Action);

            stream.Write(count);
            //stream.Write(ItemUID);

            return stream;
        }
        public static unsafe ServerSockets.Packet AddItemWarehouse(this ServerSockets.Packet stream, Game.MsgServer.MsgGameItem item)
        {
            //stream.SeekBackwards(4);
            stream.Write(item.UID);//16
            stream.Write(item.ITEM_ID);//20
            stream.ZeroFill(1); //unknown 
            stream.Write((byte)(item.SocketOne));//25
            stream.Write((byte)(item.SocketTwo));//26
            stream.Write((ushort)item.Effect); //27
            stream.Write(item.Plus);//29
            stream.Write(item.Bless);//30
            stream.Write(item.Bound);//31
            stream.Write((ushort)(item.Enchant));//32
                                                 // 34
            stream.ZeroFill(2);
            stream.Write((ushort)0);//36
            stream.Write(item.Locked); // locked
            stream.Write((byte)item.Color);//39
            //stream.Write(item.SocketProgress);//40

            return stream;
        }
        public static unsafe ServerSockets.Packet FinalizeWarehouse(this ServerSockets.Packet stream)
        {
            stream.Finalize(GamePackets.Warehause);
            return stream;
        }
    }
    public class MsgWarehouse
    {
        public enum DepositActionID : ushort
        {
            Show = 2560,
            DepositItem = 2561,
            WithdrawItem = 2562,

            Show_WH_House = 5120,
            DepositItem_WH_House = 5121,
            WithdrawItem_WH_House = 5122,

            ShashShow = 7680,
            ShashDepositItem = 7681,
            ShashWithdrawItem = 7682,

            ShowInventorySash = 10240,
            InventorySashDepositItem = 10241,
            InventorySashWithdrawItem = 10242,
        }



        [PacketAttribute(GamePackets.Warehause)]
        public unsafe static void HandlerWarehause(Client.GameClient client, ServerSockets.Packet stream)
        {
            uint NpcID;
            MsgWarehouse.DepositActionID Action;
            uint ItemUID;
            stream.GetWarehouse(out NpcID, out Action, out ItemUID);
            switch (Action)
            {
                case DepositActionID.ShashDepositItem:
                    {
                        /// if (client.Player.UID == NpcID)
                        {
                            MsgGameItem item;
                            if (client.Inventory.TryGetItem(ItemUID, out item))
                            {
                                if (client.Warehouse.AddItem(item, NpcID))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream, true);


                                    stream.WarehouseCreate(NpcID, Action, 0, 0, 1);

                                    stream.AddItemWarehouse(item);

                                    client.Send(stream.FinalizeWarehouse());

                                    item.SendItemLocked(client, stream);
                                }
                            }
                        }
                        break;
                    }
                case DepositActionID.DepositItem_WH_House:
                case DepositActionID.DepositItem:
                    {

                        if (Role.Instance.Warehouse.IsWarehouse((MsgNpc.NpcID)NpcID) || client.Player.UID == client.Player.DynamicID || client.Player.UID == NpcID)
                        {
                            MsgGameItem item;
                            if (client.Inventory.TryGetItem(ItemUID, out item))
                            {
                                if (client.Warehouse.AddItem(item, NpcID))
                                {
                                    client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream, true);


                                    stream.WarehouseCreate(NpcID, Action, 0, 0, 1);

                                    stream.AddItemWarehouse(item);

                                    client.Send(stream.FinalizeWarehouse());

                                    item.SendItemLocked(client, stream);
                                }
                            }
                        }
                        break;
                    }
                case DepositActionID.ShashShow:
                case DepositActionID.ShowInventorySash:
                    {
                        //  if (client.Player.UID == NpcID)
                        {
                            client.Warehouse.Show(NpcID, Action, stream);
                        }
                        break;
                    }
                case DepositActionID.Show_WH_House:
                case DepositActionID.Show:
                    {
                        if (Role.Instance.Warehouse.IsWarehouse((MsgNpc.NpcID)NpcID) 
                            || client.Player.UID == client.Player.DynamicID)
                        {
                            client.Warehouse.Show(NpcID, Action, stream);
                        }
                        break;
                    }
                case DepositActionID.ShashWithdrawItem:
                case DepositActionID.InventorySashWithdrawItem:
                    {
                        //   if (client.Player.UID == NpcID)
                        {
                            if (client.Warehouse.RemoveItem(ItemUID, NpcID, stream))
                            {
                                stream.WarehouseCreate(NpcID, Action, ItemUID, 0, 0);

                                client.Send(stream.FinalizeWarehouse());
                            }
                        }
                        break;
                    }
                case DepositActionID.WithdrawItem_WH_House:
                case DepositActionID.WithdrawItem:
                    {
                        if (Role.Instance.Warehouse.IsWarehouse((MsgNpc.NpcID)NpcID) || client.Player.UID == client.Player.DynamicID)
                        {
                            if (client.Warehouse.RemoveItem(ItemUID, NpcID, stream))
                            {
                                stream.WarehouseCreate(NpcID, Action, ItemUID, 0, (int)ItemUID);

                                client.Send(stream.FinalizeWarehouse());
                            }
                        }
                        break;
                    }
            }
        }


    }
}
