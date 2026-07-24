using System;

namespace COServer.Game.MsgServer.AttackHandler
{
    public static unsafe partial class MsgBuilder
    {

        public static unsafe void GetEmbedSocket(this ServerSockets.Packet stream, out uint ItemUID, out uint GemUID
            , out MsgEmbedSocket.ActionSlot Slot, out MsgEmbedSocket.ActionMode Mode)
        {
            uint unknow1 = stream.ReadUInt32();
            //uint unknow2 = stream.ReadUInt32();
            ItemUID = stream.ReadUInt32();
            GemUID = stream.ReadUInt32();
            Slot = (MsgEmbedSocket.ActionSlot)stream.ReadUInt16();
            Mode = (MsgEmbedSocket.ActionMode)stream.ReadUInt16();
        }
    }
    public struct MsgEmbedSocket
    {
        public enum ActionSlot : ushort
        {
            SlotOne = 1,
            SlotTwo = 2
        }
        public enum ActionMode : ushort
        {
            Add = 0,
            Remove = 1,
        }

        [PacketAttribute(GamePackets.EmbedSocket)]
        public unsafe static void EmbedSocket(Client.GameClient user, ServerSockets.Packet stream)
        {
            uint ItemUID;
            uint GemUID;
            ActionSlot Slot;
            ActionMode Mode;

            stream.GetEmbedSocket(out ItemUID, out GemUID, out Slot, out Mode);

            MsgGameItem DataItem;
            if (user.TryGetItem(ItemUID, out DataItem))
            {
                ushort Position = Database.ItemType.ItemPosition(DataItem.ITEM_ID);
                //anti-proxy-----------------------
                if (Position != (ushort)Role.Flags.ConquerItem.Fan && Position != (ushort)Role.Flags.ConquerItem.Tower)
                {
                    if (!Database.ItemType.AllowToUpdate((Role.Flags.ConquerItem)Position))
                    {
                        user.SendSysMesage("Sorry, you can't make a socket in this item.");
                        return;
                    }
                }
                //------------------------
                switch (Mode)
                {
                    case ActionMode.Add:
                        {
                            MsgGameItem Gem;
                            if (user.TryGetItem(GemUID, out Gem))
                            {

                                if (Enum.IsDefined(typeof(Role.Flags.Gem), (byte)(Gem.ITEM_ID % 1000)))
                                {
                                    Role.Flags.Gem GemType = (Role.Flags.Gem)(Gem.ITEM_ID % 1000);

                                    //anti proxy------------------
                                    if (Position == (ushort)Role.Flags.ConquerItem.Fan)
                                    {
                                        if (!Database.ItemType.CheckAddGemFan(GemType))
                                            return;
                                    }
                                    if (Position == (ushort)Role.Flags.ConquerItem.Tower)
                                    {
                                        if (!Database.ItemType.CheckAddGemTower(GemType))
                                            return;
                                    }
                                    if (Position != (ushort)Role.Flags.ConquerItem.Fan && Position != (ushort)Role.Flags.ConquerItem.Tower)
                                    {
                                        if (Database.ItemType.CheckAddGemFan(GemType) || Database.ItemType.CheckAddGemTower(GemType))
                                            return;
                                    }
                                    //------------------------

                                    switch (Slot)
                                    {
                                        case ActionSlot.SlotOne:
                                            {
                                                if (DataItem.SocketOne == Role.Flags.Gem.EmptySocket)
                                                {
                                                    DataItem.SocketOne = GemType;
                                                    DataItem.Mode = Role.Flags.ItemMode.Update;
                                                    DataItem.Send(user, stream).Update(Gem, Role.Instance.AddMode.REMOVE, stream);
                                                    if (DataItem.Position != 0)
                                                        user.Equipment.QueryEquipment();
                                                }
                                                break;
                                            }
                                        case ActionSlot.SlotTwo:
                                            {

                                                if ((byte)DataItem.SocketOne > 0 && (byte)DataItem.SocketOne < 255)
                                                {
                                                    if (DataItem.SocketTwo == Role.Flags.Gem.EmptySocket)
                                                    {
                                                        DataItem.SocketTwo = GemType;
                                                        DataItem.Mode = Role.Flags.ItemMode.Update;
                                                        DataItem.Send(user, stream).Update(Gem, Role.Instance.AddMode.REMOVE, stream);
                                                        if (DataItem.Position != 0)
                                                            user.Equipment.QueryEquipment();
                                                    }
                                                }
                                                else
                                                {
                                                    goto case ActionSlot.SlotOne;
                                                }
                                                break;
                                            }

                                    }

                                }
                            }

                            break;
                        }
                    case ActionMode.Remove:
                        {
                            switch (Slot)
                            {
                                case ActionSlot.SlotOne:
                                    {
                                        if (DataItem.SocketOne != Role.Flags.Gem.NoSocket)
                                        {
                                            DataItem.SocketOne = Role.Flags.Gem.EmptySocket;//1. ArtisanCloud in PC has a gem issue. Check GemIssue video on desktop.
                                            //if (DataItem.SocketTwo != Role.Flags.Gem.NoSocket && DataItem.SocketTwo != Role.Flags.Gem.EmptySocket)
                                            //{
                                            //    DataItem.SocketOne = DataItem.SocketTwo;
                                            //    DataItem.SocketTwo = Role.Flags.Gem.EmptySocket;
                                            //}
                                            DataItem.Mode = Role.Flags.ItemMode.Update;
                                            DataItem.Send(user, stream);
                                            if (DataItem.Position != 0)
                                                user.Equipment.QueryEquipment();
                                        }
                                        break;
                                    }
                                case ActionSlot.SlotTwo:
                                    {
                                        if (DataItem.SocketTwo != Role.Flags.Gem.NoSocket && DataItem.SocketTwo != Role.Flags.Gem.EmptySocket)
                                        {
                                            DataItem.SocketTwo = Role.Flags.Gem.EmptySocket;
                                            DataItem.Mode = Role.Flags.ItemMode.Update;
                                            DataItem.Send(user, stream);
                                            if (DataItem.Position != 0)
                                                user.Equipment.QueryEquipment();
                                        }
                                        else if (DataItem.SocketOne != Role.Flags.Gem.NoSocket)
                                        {
                                            DataItem.SocketOne = Role.Flags.Gem.EmptySocket;
                                            DataItem.Mode = Role.Flags.ItemMode.Update;
                                            DataItem.Send(user, stream);
                                            if (DataItem.Position != 0)
                                                user.Equipment.QueryEquipment();
                                        }
                                        break;
                                    }
                            }
                            break;
                        }

                }
            }
        }
    }
}
