using System.Linq;

namespace COServer
{
    public unsafe class SendGlobalPacket
    {
        public unsafe void Enqueue(ServerSockets.Packet data)
        {
            var array = Database.Server.GamePoll.Values.ToArray();
            foreach (var user in Database.Server.GamePoll.Values)
            {
                user.Send(data);
            }
        }
    }
}
