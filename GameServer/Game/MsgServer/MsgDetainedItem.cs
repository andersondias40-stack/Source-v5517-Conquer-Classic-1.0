using System.Runtime.InteropServices;

namespace COServer.Game.MsgServer
{
    public unsafe static partial class MsgBuilder
    {


        public static unsafe ServerSockets.Packet DetainedItemCreate(this ServerSockets.Packet stream, MsgDetainedItem item)
        {
            stream.InitWriter();

            stream.Write(item.UID);
            stream.Write(item.ItemUID);
            stream.Write(item.ItemID);
            stream.Write(item.Durability);
            stream.Write(item.MaximDurability);
            stream.Write((uint)item.Action);
            stream.Write(item.SocketProgress);
            stream.Write((byte)item.SocketOne);
            stream.Write((byte)item.SocketTwo);
            //stream.Write((ushort)0);
            stream.Write((ushort)item.Effect);
            //stream.Write((byte)0);
            //stream.Write((ushort)0);
            stream.Write(item.Plus);
            stream.Write(item.Bless);
            stream.Write((byte)(item.Bound ? 1 : 0));
            stream.Write(item.Enchant);
            stream.Write(item.PlusProgres);
            stream.Write(item.Suspicious);
            stream.Write(item.Lock);
            stream.Write((uint)item.ItemColor);
            stream.Write(item.OwnerUID);
            stream.Write(item.OwnerName, 16);
            stream.Write(item.GainerUID);
            stream.Write(item.GainerName, 16);
            stream.Write(item.Date);
            stream.Write(item.RewardConquerPoints);
            stream.Write(item.ConquerPointsCost);
            stream.Write(item.DaysLeft);
            stream.Finalize(GamePackets.DetainedItem);
            return stream;
        }

    }
    [StructLayout(LayoutKind.Explicit, Size = 238)]
    public unsafe struct MsgDetainedItem
    {
        public enum ContainerType : uint
        {
            DetainPage = 13369344,//0, 
            ClaimPage = 13369345,//1,
            RewardCps = 13369346 //2
        }
        [FieldOffset(0)]
        public ushort Length;
        [FieldOffset(2)]
        public ushort PacketID;
        [FieldOffset(4)]
        public uint UID;
        [FieldOffset(8)]
        public uint ItemUID;
        [FieldOffset(12)]
        public uint ItemID;
        [FieldOffset(16)]
        public ushort Durability;
        [FieldOffset(18)]
        public ushort MaximDurability;
        [FieldOffset(20)]
        public ContainerType Action;
        [FieldOffset(24)]
        public uint SocketProgress;
        [FieldOffset(28)]
        public Role.Flags.Gem SocketOne;
        [FieldOffset(29)]
        public Role.Flags.Gem SocketTwo;
        [FieldOffset(30)]
        public Role.Flags.ItemEffect Effect;
        [FieldOffset(32)]//32
        public byte Plus;
        [FieldOffset(33)]//33
        public byte Bless;
        [FieldOffset(34)]//34
        public bool Bound;
        [FieldOffset(35)]//35
        public byte Enchant;
        [FieldOffset(36)]//36
        public uint PlusProgres;
        [FieldOffset(40)]//40
        public ushort Suspicious;
        [FieldOffset(42)]//42
        public ushort Lock;
        [FieldOffset(44)]//44
        public Role.Flags.Color ItemColor;
        [FieldOffset(48)]
        public uint OwnerUID;
        [FieldOffset(52)]
        private fixed sbyte szOwnerName[16];
        [FieldOffset(68)]
        public uint GainerUID;
        [FieldOffset(72)]
        private fixed sbyte szGainerName[16];
        [FieldOffset(88)]
        public int Date;//time when lose item
        [FieldOffset(92)]
        public int RewardConquerPoints;
        [FieldOffset(96)]
        public int ConquerPointsCost;
        [FieldOffset(100)]
        public uint DaysLeft;
        public static MsgDetainedItem Create(MsgGameItem GameItem)
        {
            MsgDetainedItem item = new MsgDetainedItem();
            item.Length = 112;
            item.PacketID = GamePackets.DetainedItem;

            item.ItemUID = GameItem.UID;
            item.ItemID = GameItem.ITEM_ID;
            item.Durability = GameItem.Durability;
            item.MaximDurability = GameItem.MaximDurability;
            item.SocketProgress = GameItem.SocketProgress;
            item.SocketOne = GameItem.SocketOne;
            item.SocketTwo = GameItem.SocketTwo;
            item.Effect = GameItem.Effect;
            item.Plus = GameItem.Plus;
            item.Bless = GameItem.Bless;
            item.Bound = GameItem.Bound == 1;
            item.Enchant = GameItem.Enchant;
            item.Suspicious = GameItem.Suspicious;
            item.Lock = GameItem.Locked;
            item.ItemColor = GameItem.Color;
            item.PlusProgres = GameItem.PlusProgress;

            return item;
        }
        public static MsgGameItem CopyTo(MsgDetainedItem Item)
        {
            MsgGameItem GameItem = new MsgGameItem();
            GameItem.UID = Item.ItemUID;
            GameItem.ITEM_ID = Item.ItemID;
            GameItem.Bless = Item.Bless;
            GameItem.Bound = (byte)(Item.Bound ? 1 : 0);
            GameItem.Color = Item.ItemColor;
            GameItem.Durability = Item.Durability;
            GameItem.Effect = Item.Effect;
            GameItem.Enchant = Item.Enchant;
            GameItem.MaximDurability = Item.MaximDurability;
            GameItem.Plus = Item.Plus;
            GameItem.PlusProgress = Item.PlusProgres;
            GameItem.SocketOne = Item.SocketOne;
            GameItem.SocketProgress = Item.SocketProgress;
            GameItem.SocketTwo = Item.SocketTwo;
            GameItem.Suspicious = (byte)Item.Suspicious;

            return GameItem;
        }

        public unsafe string OwnerName
        {
            get { fixed (sbyte* bp = szOwnerName) { return new string(bp); } }
            set
            {
                string ip = value;
                fixed (sbyte* bp = szOwnerName)
                {
                    for (int i = 0; i < ip.Length; i++)
                        bp[i] = (sbyte)ip[i];
                }
            }
        }
        public unsafe string GainerName
        {
            get { fixed (sbyte* bp = szGainerName) { return new string(bp); } }
            set
            {
                string ip = value;
                fixed (sbyte* bp = szGainerName)
                {
                    for (int i = 0; i < ip.Length; i++)
                        bp[i] = (sbyte)ip[i];
                }
            }
        }
        public unsafe Role.Instance.Inventory Send(Client.GameClient client, ServerSockets.Packet stream)
        {
            if (Action == ContainerType.RewardCps)
            {
                OwnerUID = 500;
                ItemID = 0;
                ItemColor = 0;
                Durability = 0;
                MaximDurability = 0;
                SocketOne = Role.Flags.Gem.NoSocket;
                SocketTwo = Role.Flags.Gem.NoSocket;
                Effect = Role.Flags.ItemEffect.None;
                Plus = 0;
                Bless = 0;
                Enchant = 0;
                SocketProgress = 0;
                Bound = false;
                Lock = 0;
                client.Send(stream.DetainedItemCreate(this));
                return client.Inventory;
            }
            var item = this;
            client.Send(stream.DetainedItemCreate(this));

            return client.Inventory;
        }
    }
}
