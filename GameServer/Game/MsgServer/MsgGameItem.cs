using System;
using System.Collections.Generic;

namespace COServer.Game.MsgServer
{
    public class MsgGameItem
    {
        public MsgGameItem()
        {
        }

        public bool IsMaxPlus
        {
            get { return Plus >= 9; }
        }

        public unsafe Role.Instance.Inventory Send(Client.GameClient client, ServerSockets.Packet stream)
        {
            if (Mode == Role.Flags.ItemMode.Update)
            {

                string logs = "[Item]" + client.Player.Name + " update [" + UID + "]" + ITEM_ID + " plus [" + Plus + "] s1[" + SocketOne + "]s2[" + SocketTwo + "]";
                Database.ServerDatabase.LoginQueue.Enqueue(logs);
            }
            Database.ItemType.DBItem DBItem;
            if (MaximDurability == 0)
            {
                if (Database.Server.ItemsBase.TryGetValue(ITEM_ID, out DBItem))
                    MaximDurability = DBItem.Durability;
            }
            ushort position = Database.ItemType.ItemPosition(ITEM_ID);
            if (Plus > 0)
            {
                if (position == 0)
                    Plus = 0;
            }
            if (ITEM_ID >= 730001 && ITEM_ID <= 730008)
                Plus = (byte)(ITEM_ID % 10);

            client.Send(ItemCreate(stream, this));
            SendItemLocked(client, stream);

            return client.Inventory;
        }
        public void SendItemLocked(Client.GameClient client, ServerSockets.Packet stream)
        {
            if (Locked == 2)
            {
                if (client.Player.OnMyOwnServer)
                {
                    if (UnLockTimer == 0)
                    {
                        Locked = 0;
                        Mode = Role.Flags.ItemMode.Update;
                        client.Send(ItemCreate(stream, this));
                    }
                    else
                    {
                        if (DateTime.Now > Role.Core.GetTimer(UnLockTimer))
                        {
                            Locked = 0;
                            Mode = Role.Flags.ItemMode.Update;
                            client.Send(ItemCreate(stream, this));
                        }
                        else
                        {
                            client.Send(stream.ItemLockCreate(UID, MsgItemLock.TypeLock.UnlockDate, 0, (uint)UnLockTimer));
                        }
                    }
                }
            }
        }
        public ServerSockets.Packet ItemCreate(ServerSockets.Packet stream, MsgGameItem item)
        {
            stream.InitWriter();

            stream.Write(item.UID);//4
            stream.Write(item.ITEM_ID);//8
            stream.Write(item.Durability);//12
            stream.Write(item.MaximDurability);//14
            stream.Write((ushort)item.Mode);//16
            stream.Write(item.Position);//18
            stream.Write(item.SocketProgress);//20

            stream.Write((byte)item.SocketOne);//24

            stream.Write((byte)item.SocketTwo);//25
            stream.Write((ushort)item.Effect);//26
            stream.Write(item.Plus);//28
            stream.Write(item.Bless);//29
            stream.Write((byte)item.Bound);//30
            stream.Write(item.Enchant);//31
            //stream.Write(item.Suspicious);//31 ???
            //stream.Write((uint)99999);
            //stream.Write((byte)123);
           stream.ZeroFill(6);//38
            stream.Write((byte)item.Locked);//38
            stream.ZeroFill(1);//38
            stream.Write((uint)item.Color);//40
            stream.Write((uint)item.PlusProgress);//44
            stream.Finalize(Game.GamePackets.Item);

            return stream;
        }
        public ServerSockets.Packet ItemCreateViwer(ServerSockets.Packet stream, MsgGameItem item, uint PlayerUID)
        {
            stream.InitWriter();
            stream.Write(PlayerUID);
            stream.Write(item.ITEM_ID);//8
            stream.Write(item.Durability);//12
            stream.Write(item.MaximDurability);//14
            stream.Write((ushort)item.Mode);//16
            stream.Write(item.Position);//18
            stream.Write(item.SocketProgress);//20
            stream.Write((byte)item.SocketOne);//24

            stream.Write((byte)item.SocketTwo);//25
            stream.Write((ushort)item.Effect);//26
            stream.Write(item.Plus);//28
            stream.Write(item.Bless);//29
            stream.Write((byte)item.Bound);//30
            stream.Write(item.Enchant);//31
            stream.ZeroFill(6);//38
            stream.Write((byte)item.Locked);//38
            stream.ZeroFill(1);//38
            stream.Write((uint)item.Color);//40
            stream.Write((uint)item.PlusProgress);//44

            stream.Finalize(Game.GamePackets.Item);

            return stream;
        }
        
        public bool IsWeapon
        {
            get
            {
                return (Database.ItemType.ItemPosition(ITEM_ID) == (ushort)Role.Flags.ConquerItem.RightWeapon
                    || Database.ItemType.ItemPosition(ITEM_ID) == (ushort)Role.Flags.ConquerItem.LeftWeapon) 
                    && !Database.ItemType.IsArrow(ITEM_ID);
            }
        }
        public bool IsEquip
        {
            get
            {
                return Database.ItemType.ItemPosition(ITEM_ID) != 0;
            }
        }
        public uint UID;
        public uint ITEM_ID;
        public ushort Durability;
        public ushort MaximDurability;
        public Role.Flags.ItemMode Mode;
        public ushort Position;
        public uint SocketProgress;

        public Role.Flags.Gem SocketOne;
        public Role.Flags.Gem SocketTwo;
        public ushort padding;
        public Role.Flags.ItemEffect Effect;
        public byte Plus;
        public byte Bless;
        public byte Bound;
        public byte Enchant;//36 // Steed  -> ProgresBlue 
        public uint ProgresGreen;//39 // for steed
        public byte Suspicious;
        public byte Locked;
        public Role.Flags.Color Color;
        public uint PlusProgress;//52
        public ushort StackSize;//68
        public uint WH_ID;
        public int UnLockTimer;
        public uint Activate;
        public DateTime EndDate = DateTime.FromBinary(0);
    }
}
