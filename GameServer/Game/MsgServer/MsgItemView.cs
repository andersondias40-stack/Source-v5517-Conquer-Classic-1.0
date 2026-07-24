namespace COServer.Game.MsgServer
{
    public static unsafe partial class MsgBuilder
    {
        public static unsafe ServerSockets.Packet ItemViewCreate(this ServerSockets.Packet stream, uint PlayerUID, uint Cost, MsgGameItem DateItem, MsgItemView.ActionMode Mode)
        {
            stream.InitWriter();
            if (Mode == MsgItemView.ActionMode.ViewEquip)
            {
                stream.Write(PlayerUID);
                stream.Write(DateItem.ITEM_ID);//8
                stream.Write(DateItem.Durability);//12
                stream.Write(DateItem.MaximDurability);//14
                stream.Write((ushort)DateItem.Mode);//16
                stream.Write(DateItem.Position);//18
                stream.Write(DateItem.SocketProgress);//20
                stream.Write((byte)DateItem.SocketOne);//24

                stream.Write((byte)DateItem.SocketTwo);//25
                stream.Write((ushort)DateItem.Effect);//26
                stream.Write(DateItem.Plus);//28
                stream.Write(DateItem.Bless);//29
                stream.Write((byte)DateItem.Bound);//30
                stream.Write(DateItem.Enchant);//31
                stream.ZeroFill(6);//38
                stream.Write((byte)DateItem.Locked);//38
                stream.ZeroFill(1);//38
                stream.Write((uint)DateItem.Color);//40
                stream.Write((uint)DateItem.PlusProgress);//44
            }
            else// venditems !
            {
                stream.Write(DateItem.UID);//4
                stream.Write(PlayerUID);//8
                stream.Write(Cost);//12
                stream.Write(DateItem.ITEM_ID);//16
                stream.Write(DateItem.Durability);//20
                stream.Write(DateItem.MaximDurability);//22
                stream.Write((ushort)Mode);//24
                                           //stream.ZeroFill(4);
                stream.Write(DateItem.Position);
                stream.Write(DateItem.SocketProgress);
                //    stream.Write((uint)0);//unknow 28
                stream.Write((byte)DateItem.SocketOne);//32
                stream.Write((byte)DateItem.SocketTwo);//33
                stream.Write((ushort)DateItem.Effect);//32
                stream.Write(DateItem.Plus);//33
                stream.Write(DateItem.Bless);//34
                stream.Write(DateItem.Enchant);//36

                stream.Write(DateItem.ProgresGreen);//40 tailms?
                                                    //   stream.Write((ushort)DateItem.Suspicious);//50
                stream.ZeroFill(3);

                stream.Write((ushort)DateItem.Locked);//48

                stream.Write((uint)DateItem.Color);//52
                stream.Write((ushort)DateItem.PlusProgress);//56
            }
            stream.Finalize(1108);

            return stream;
        }

    }

    public class MsgItemView
    {
        public enum ActionMode : ushort
        {
            Gold = 1,
            CPs = 3,
            ViewEquip = 4
        }
    }
}
