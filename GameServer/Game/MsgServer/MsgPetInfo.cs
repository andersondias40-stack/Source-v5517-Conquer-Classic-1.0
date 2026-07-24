using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace COServer.Game.MsgServer
{
    public static unsafe class MsgPetInfo
    {
        public unsafe struct PetActionQuery
        {
            public uint UID;
            public uint Model;
            public uint Callpet; // AI Type?
            public ushort X;
            public ushort Y;
            public fixed sbyte Name[16];
        }
        public static unsafe ServerSockets.Packet PetsViewCreate(this ServerSockets.Packet stream,
            uint UID, uint Model, uint Callpet, ushort x, ushort y, string name)
        {
            stream.InitWriter();
            stream.Write(UID);
            stream.Write(Model);
            stream.Write(Callpet);
            stream.Write(x);
            stream.Write(y);
            stream.Write(name);
            stream.Finalize(2035);
            return stream;
        }
        public static unsafe ServerSockets.Packet Create(this ServerSockets.Packet stream, PetActionQuery* pQuery)
        {
            stream.InitWriter();
            stream.WriteUnsafe(pQuery, sizeof(PetActionQuery));
            stream.Finalize(GamePackets.MsgPetInfo);
            return stream;
        }
    }
}