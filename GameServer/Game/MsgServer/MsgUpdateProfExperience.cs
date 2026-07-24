namespace COServer.Game.MsgServer
{
    public unsafe static partial class MsgBuilder
    {
        public static unsafe ServerSockets.Packet UpdateProfExperienceCreate(this ServerSockets.Packet stream, uint Experience, uint UID, uint ID)
        {
            stream.InitWriter();

            stream.Write(Experience);
            stream.Write(UID);
            stream.Write(ID);
            stream.ZeroFill(8);//unknow

            stream.Finalize(GamePackets.UpgradeSpellExperience);
            return stream;
        }
    }
}
