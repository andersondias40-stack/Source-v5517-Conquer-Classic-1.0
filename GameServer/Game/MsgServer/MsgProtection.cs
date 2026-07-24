using System;
using System.Collections.Generic;
using System.Text;

namespace COServer.Game.MsgServer
{
    public partial class MsgProtection
    {
        [PacketAttribute(1801)]
        public unsafe static void Loader(Client.GameClient user, ServerSockets.Packet packet)
        {
        }
        }
    }
