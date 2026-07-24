using COServer.Game.MsgServer;

namespace COServer.Role
{
    public class StaticRole : IMapObj
    {
        public Role.StatusFlagsBigVector32 BitVector;
        public StaticRole(ushort x, ushort y, string _Namee = "Flag", uint FlagMesh = 513)
        {
            AllowDynamic = false;
            Name = _Namee;
            X = x;
            Y = y;
            UID = (uint)((x * 1000) + y);
            Mesh = FlagMesh;
            Level = 1;
            ObjType = MapObjectType.StaticRole;
            BitVector = new Role.StatusFlagsBigVector32(32 * 2);
        }
        public Role.GameMap GMap
        {

            get { return Database.Server.ServerMaps[Map]; }
        }
        public bool AllowDynamic { get; set; }
        public uint IndexInScreen { get; set; }
        public bool IsTrap() { return false; }
        public unsafe string Name { get; set; }
        public unsafe uint Mesh { get; set; }
        public unsafe uint UID { get; set; }
        public unsafe byte Level { get; set; }
        public unsafe uint HitPoints { get; set; }
        public uint Map { get; set; }
        public uint DynamicID { get; set; }
        public Role.MapObjectType ObjType { get; set; }
        public ushort X { get; set; }
        public ushort Y { get; set; }
        public unsafe Role.Flags.ConquerAction Action { get; set; }
        public unsafe Role.Flags.ConquerAngle Facing { get; set; }

        public uint SetBy = 0;
        public bool Viable = false;

        public bool QuestionMark { get { return Mesh == 767; } }

        public void DoFrozenTrap(uint setter)
        {
            SetBy = setter;
            Viable = true;
            Mesh = 761;
            Name = "Frozen Trap";
            Level = 1;
        }

        public bool AddFlag(Game.MsgServer.MsgUpdate.Flags Flag, int Seconds, bool RemoveOnDead, int StampSeconds = 0)
        {
            if (!BitVector.ContainFlag((int)Flag))
            {
                BitVector.TryAdd((int)Flag, Seconds, RemoveOnDead, StampSeconds);
                UpdateFlagOffset();
                return true;
            }
            return false;
        }
        public bool RemoveFlag(Game.MsgServer.MsgUpdate.Flags Flag, Role.GameMap map)
        {
            if (BitVector.ContainFlag((int)Flag))
            {
                BitVector.TryRemove((int)Flag);
                UpdateFlagOffset();
                return true;
            }
            return false;
        }
        private unsafe void UpdateFlagOffset(bool SendScreem = true)
        {
            if (SendScreem)
                SendUpdate(BitVector.bits, Game.MsgServer.MsgUpdate.DataType.StatusFlag);
        }
        public unsafe void SendUpdate(uint[] Value, Game.MsgServer.MsgUpdate.DataType datatype)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();

                Game.MsgServer.MsgUpdate packet = new Game.MsgServer.MsgUpdate(stream, UID, 1);
                stream = packet.Append(stream, datatype, Value);
                stream = packet.GetArray(stream);
                Send(stream);
            }
        }
        public bool UpdateFlag(Game.MsgServer.MsgUpdate.Flags Flag, int Seconds, bool SetNewTimer, int MaxTime)
        {
            return BitVector.UpdateFlag((int)Flag, Seconds, SetNewTimer, MaxTime);
        }

        public unsafe bool MoveTo(ushort _x, ushort _y)
        {
            if (_x == X && _y == Y)
            {
                return false;
            }
            Role.Flags.ConquerAngle dir = Role.Core.GetAngle(X, Y, _x, _y);
            ushort WalkX = X; ushort WalkY = Y;
            Role.Core.IncXY(dir, ref WalkX, ref WalkY);

            // ushort WalkX = (ushort)(X + Game.MsgServer.MsgMovement.DeltaMountX[(byte)dir]);
            // ushort WalkY = (ushort)(Y + Game.MsgServer.MsgMovement.DeltaMountY[(byte)dir]);


            GMap.View.MoveTo<Role.IMapObj>(this, WalkX, WalkY);
            X = WalkX;
            Y = WalkY;
            /*       var action = new ActionQuery()
                   {
                       ObjId = UID,
                       wParam5 =5,
                       dwParam3 = Map,
                       wParam1 = WalkX,
                       wParam2 = WalkY,
                       Type =  ActionType.Jump,
                       Fascing = (ushort)dir
                   };*/
            WalkQuery walk = new WalkQuery()
            {
                Direction = (byte)dir,
                Running = 1,//2
                UID = UID
            };
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                //Send(stream.ActionCreate(&action));
                Send(stream.MovementCreate(&walk));
            }
            return true;
        }


        public unsafe void SendString(ServerSockets.Packet stream, Game.MsgServer.MsgStringPacket.StringID id, params string[] args)
        {
            Game.MsgServer.MsgStringPacket packet = new Game.MsgServer.MsgStringPacket();
            packet.ID = id;
            packet.UID = UID;

            packet.Strings = args;
            Send(stream.StringPacketCreate(packet));
        }
        public unsafe ServerSockets.Packet GetArray(ServerSockets.Packet stream, bool WindowsView)
        {
            stream.InitWriter();
            stream.Write(UID);//8
            stream.Write(Mesh); //4

            stream.ZeroFill(4);
            
            for (uint x = 0; x < BitVector.bits.Length; x++)
                stream.Write((uint)BitVector.bits[x]);//16

            stream.ZeroFill(30);

            stream.Write(X);//54
            stream.Write(Y);//56
            stream.Write((byte)Facing);//58
            stream.Write((ushort)Action);//59
            stream.ZeroFill(1);
            stream.Write((ushort)Level);//62
            stream.ZeroFill(31);//27
            stream.Write(Name, string.Empty);//
            stream.Finalize(Game.GamePackets.SpawnPlayer);//111

            return stream;

        }

        public bool Alive { get { return HitPoints > 0; } }

        public void RemoveRole(IMapObj obj)
        {

        }

        public unsafe void Send(ServerSockets.Packet msg)
        {
            foreach (var obj in GMap.View.Roles(MapObjectType.Player, X, Y, p => CanSee(p)))
                obj.Send(msg);
        }
        public bool CanSee(Role.IMapObj obj)
        {
            if (obj.Map != Map)
                return false;
            if (obj.DynamicID != DynamicID)
                return false;
            if (obj.UID == UID)
                return false;
            return Role.Core.GetDistance(obj.X, obj.Y, X, Y) <= 18;
        }
    }
}
