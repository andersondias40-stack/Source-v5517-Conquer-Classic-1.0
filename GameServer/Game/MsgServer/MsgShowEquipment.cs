namespace COServer.Game.MsgServer
{
    public unsafe static partial class MsgBuilder
    {
        public static unsafe ServerSockets.Packet ShowEquipmentCreate(this ServerSockets.Packet stream, MsgShowEquipment item)
        {
            stream.InitWriter();

            stream.Write((uint)item.UID);
            stream.Write(item.wParam);//8
            stream.Write((uint)44);//12 -- 44 is the show action i think
            //stream.Write(0);//16
            //stream.Write(0);//24
            //stream.Write(0);//28

            stream.Write(item.Head);//32
            stream.Write(item.Necklace);
            stream.Write(item.Armor);
            stream.Write(item.RightWeapon);
            stream.Write(item.LeftWeapon);
            stream.Write(item.Ring);
            stream.Write(item.Bottle);
            stream.Write(item.Boots);

            stream.Write(item.Garment);
            //stream.Write(item.RightWeaponAccessory);
            //stream.Write(item.LeftWeaponAccessory);
            //stream.Write(item.SteedMount);
            //stream.Write(item.RidingCrop);
            //stream.Write(item.Wing);


            stream.Finalize(GamePackets.Usage);

            return stream;
        }

    }
    public class MsgShowEquipment
    {
        public const ushort Show = 46;

        public uint wParam;
        public ushort UID;
        public uint Head;
        public uint Necklace;
        public uint Armor;
        public uint RightWeapon;
        public uint LeftWeapon;
        public uint Ring;
        public uint Bottle;
        public uint Boots;
        public uint Garment;
    }
}
