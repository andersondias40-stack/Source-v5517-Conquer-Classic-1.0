using System;

namespace COServer.Database
{
    public class ClientItems
    {
        public struct DBItem
        {
            public uint UID;
            public uint ITEM_ID;
            public ushort Durability;
            public ushort MaximDurability;
            public ushort Position;
            public uint SocketProgress;
            public Role.Flags.Gem SocketOne;
            public Role.Flags.Gem SocketTwo;
            public Role.Flags.ItemEffect Effect;
            public byte Plus;
            public byte Bless;
            public byte Bound;
            public byte Enchant;
            public byte Suspicious;
            public byte Locked;
            public uint PlusProgress;
            public ushort StackSize;
            public uint WH_ID;
            public Role.Flags.Color Color;
            public int UnLockTimer;
            public long Expiration;
            public uint Activate;

            public DBItem GetDBItem(Game.MsgServer.MsgGameItem DataItem)
            {
                UID = DataItem.UID;
                ITEM_ID = DataItem.ITEM_ID;
                Durability = DataItem.Durability;
                MaximDurability = DataItem.MaximDurability;
                Position = DataItem.Position;
                SocketProgress = DataItem.SocketProgress;
                SocketOne = DataItem.SocketOne;
                SocketTwo = DataItem.SocketTwo;
                Effect = DataItem.Effect;
                Plus = DataItem.Plus;
                Bless = DataItem.Bless;
                Bound = DataItem.Bound;
                Enchant = DataItem.Enchant;
                Suspicious = DataItem.Suspicious;
                Locked = DataItem.Locked;
                PlusProgress = DataItem.PlusProgress;
                StackSize = DataItem.StackSize;
                UnLockTimer = DataItem.UnLockTimer;
                WH_ID = DataItem.WH_ID;
                Color = DataItem.Color;
                Activate = DataItem.Activate;
                Expiration = DataItem.EndDate == DateTime.FromBinary(0) ? 0 : DataItem.EndDate.ToBinary();
                return this;
            }

            public Game.MsgServer.MsgGameItem GetDataItem()
            {
                Game.MsgServer.MsgGameItem DataItem = new Game.MsgServer.MsgGameItem();
                DataItem.UID = UID;
                DataItem.ITEM_ID = ITEM_ID;
                DataItem.Durability = Durability;
                DataItem.MaximDurability = MaximDurability;
                DataItem.Position = Position;
                DataItem.SocketProgress = SocketProgress;
                DataItem.SocketOne = SocketOne;
                DataItem.SocketTwo = SocketTwo;
                DataItem.Effect = Effect;
                DataItem.Plus = Plus;
                DataItem.Bless = Bless;
                DataItem.Bound = Bound;
                DataItem.Enchant = Enchant;
                DataItem.Suspicious = Suspicious;
                DataItem.Locked = Locked;
                DataItem.PlusProgress = PlusProgress;
                DataItem.StackSize = StackSize;
                DataItem.UnLockTimer = UnLockTimer;
                DataItem.WH_ID = WH_ID;
                DataItem.Color = Color;
                DataItem.Activate = Activate;
                DataItem.EndDate = DateTime.FromBinary(Expiration);
                return DataItem;
            }

        }
    }
}