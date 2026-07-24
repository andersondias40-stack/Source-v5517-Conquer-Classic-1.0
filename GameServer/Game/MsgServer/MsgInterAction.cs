using System.Runtime.InteropServices;

namespace COServer.Game.MsgServer
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct InterActionWalk
    {
        public MsgInterAction.Action Mode;//4
        public ushort X;
        public ushort Y;
        public uint UID;
    }

    public static class MsgInterAction
    {
        public enum Action : ushort
        {
            Walk = 1,
            Jump = 2
        }

        public static unsafe ServerSockets.Packet InterActionWalk(this ServerSockets.Packet stream, InterActionWalk* pQuery)
        {

            stream.InitWriter();
            stream.WriteUnsafe(pQuery, sizeof(InterActionWalk));
            stream.Finalize(GamePackets.InterAction);
            return stream;
        }

    }
}
