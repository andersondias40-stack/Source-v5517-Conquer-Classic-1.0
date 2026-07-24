namespace COServer.Game.MsgNpc
{
    public unsafe static class NpcReply
    {
        public enum InteractTypes : byte
        {
            Dialog = 1,
            Option = 2,
            Input = 3,
            Avatar = 4,
            MessageBox = 6,
            Finish = 100
        }



        public static unsafe ServerSockets.Packet NpcReplyCreate(this ServerSockets.Packet stream, InteractTypes interactType
            , string text
            , ushort InputMaxLength
            , byte OptionID
            , bool display = true)
        {
            stream.InitWriter();
            stream.Write(0);
            stream.Write(InputMaxLength);
            stream.Write((byte)OptionID);
            stream.Write((byte)interactType);
            if (display)
                stream.Write(text);

            stream.Finalize(GamePackets.NpcServerRequest);
            return stream;
        }
    }
}
