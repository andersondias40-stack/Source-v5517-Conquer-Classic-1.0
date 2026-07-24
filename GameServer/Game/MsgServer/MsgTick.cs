
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace COServer.Game.MsgServer
{
    public unsafe static partial class MsgBuilder
    {
        public static unsafe ServerSockets.Packet MsgTickCreate(this ServerSockets.Packet stream, Client.GameClient user)
        {
            stream.InitWriter();
            stream.Write(user.Player.UID);
            stream.Finalize(GamePackets.MsgTick);
            return stream;
        }
    }
    public static unsafe class MsgTick
    {
        //[PacketAttribute(GamePackets.MsgTick)]
        private unsafe static void Process(Client.GameClient client, ServerSockets.Packet stream)
        {// coded by jason
            if (client == null) return;
            client.ServersidePing = (Time32.Now - client._ServersidePing).AllMilliseconds;

            client.øServersidePing += client.ServersidePing;
            client.ΩServersidePing++;
            client.ƒServersidePing = client.øServersidePing / client.ΩServersidePing;

            client.WaitingPing = false;
        }
    }
}
