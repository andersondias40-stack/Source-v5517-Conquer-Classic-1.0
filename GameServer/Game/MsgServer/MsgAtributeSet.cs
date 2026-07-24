namespace COServer.Game.MsgServer
{
    public unsafe static partial class MsgBuilder
    {

        public static unsafe void GetAtributeSet(this ServerSockets.Packet stream, out uint Str, out uint Agi, out uint vit, out uint spi)
        {
            Str = (byte)stream.ReadUInt8();//4
            Agi = (byte)stream.ReadUInt8();
            vit = (byte)stream.ReadUInt8();
            spi = (byte)stream.ReadUInt8();
        }
        public static unsafe ServerSockets.Packet AtributeSetCreate(this ServerSockets.Packet stream, uint Str, uint Agi, uint vit, uint spi)
        {
            stream.InitWriter();

            //   stream.Write(Time32.Now.Value);
            //stream.Write((uint)0);//unknow
            stream.Write(Str);
            stream.Write((byte)Agi);
            stream.Write((byte)vit);
            stream.Write((byte)spi);

            stream.Finalize(GamePackets.AtributeSet);
            return stream;
        }
    }

    public unsafe struct MsgAtributeSet
    {
        [PacketAttribute(GamePackets.AtributeSet)]
        private static void Process(Client.GameClient user, ServerSockets.Packet stream)
        {

            uint Str;
            uint Agi;
            uint Vit;
            uint Spi;

            stream.GetAtributeSet(out Str, out Agi, out Vit, out Spi);

            if (user.Player.Atributes == 0)
                return;

            uint TotalStatPoints = Str + Agi + Vit + Spi;

            if (user.Player.Atributes >= TotalStatPoints)
            {
                user.Player.Strength += (ushort)Str;
                user.Player.Vitality += (ushort)Vit;
                user.Player.Spirit += (ushort)Spi;
                user.Player.Agility += (ushort)Agi;
                user.Player.Atributes -= (ushort)TotalStatPoints;

                user.Send(stream.AtributeSetCreate(Str, Agi, Vit, Spi));

                user.Equipment.QueryEquipment(false);
            }
        }
    }
}
