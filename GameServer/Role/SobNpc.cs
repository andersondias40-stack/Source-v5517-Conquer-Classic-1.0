using COServer.Game.MsgServer;

namespace COServer.Role
{

    public unsafe class SobNpc : IMapObj
    {
        public enum StaticMesh : ushort
        {
            Vendor = 406,
            LeftGate = 241,
            OpenLeftGate = 251,
            RightGate = 277,
            OpenRightGate = 287,
            Pole = 1137,
            SuperGuildWarPole = 31220
        }

        public Role.Statue statue = null;
        public bool AllowDynamic { get; set; }
        public Role.StatusFlagsBigVector32 BitVector;
        public uint IndexInScreen { get; set; }
        public bool IsStatue
        {
            get { return statue != null; }
        }
        public SobNpc(Role.Statue _statue)
        {
            statue = _statue;
            BitVector = new StatusFlagsBigVector32(32 * 1);
        }


        public SobNpc()
        {
            AllowDynamic = false;
            BitVector = new StatusFlagsBigVector32(32 * 1);
        }
        public const byte SeedDistrance = 19;//17
        public bool IsTrap() { return false; }
        public uint UID { get; set; }
        public int MaxHitPoints { get; set; }
        int Hit;
        public int HitPoints
        {
            get { return Hit; }
            set
            {
                Hit = value;
            }

        }
        public string Name { get; set; }
        public ushort X { get; set; }
        public ushort Y { get; set; }
        public StaticMesh Mesh;
        public Flags.NpcType Type;
        public ushort Sort;
        public uint Map { get; set; }
        public uint DynamicID { get; set; }

        public bool Alive { get { return HitPoints > 0; } }
        public MapObjectType ObjType { get; set; }

        public Client.GameClient OwnerVendor;

        public void RemoveRole(IMapObj obj)
        {

        }
        public unsafe void Send(byte[] packet)
        {

        }
        public unsafe void Send(ServerSockets.Packet msg)
        {

        }
        public bool AddFlag(Game.MsgServer.MsgUpdate.Flags Flag, int Seconds, bool RemoveOnDead, int StampSeconds = 0, uint showamount = 0, uint amount = 0)
        {
            if (!BitVector.ContainFlag((int)Flag))
            {
                BitVector.TryAdd((int)Flag, Seconds, RemoveOnDead, StampSeconds);
                UpdateFlagScreen();
                return true;
            }
            return false;
        }
        public bool RemoveFlag(Game.MsgServer.MsgUpdate.Flags Flag)
        {
            if (BitVector.ContainFlag((int)Flag))
            {
                BitVector.TryRemove((int)Flag);
                UpdateFlagScreen();

                return true;
            }
            return false;
        }
        public bool ContainFlag(Game.MsgServer.MsgUpdate.Flags Flag)
        {
            return BitVector.ContainFlag((int)Flag);
        }
        public void UpdateFlagScreen()
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();

                Game.MsgServer.MsgUpdate upd = new Game.MsgServer.MsgUpdate(stream, UID, 1);
                stream = upd.Append(stream, MsgUpdate.DataType.StatusFlag, BitVector.bits);
                stream = upd.GetArray(stream);

                foreach (var user in Database.Server.GamePoll.Values)
                {
                    if (user.Player.Map == Map)
                        user.Send(stream);
                }
            }
        }

        public unsafe void Die(ServerSockets.Packet stream, Client.GameClient killer)
        {
            if (HitPoints == 0)
                return;
            //if (killer.OnAutoAttack)
            //    killer.OnAutoAttack = false;
            if (Map == 1039|| UID >= 7811 && UID <= 7814)
            {
                HitPoints = MaxHitPoints;
                InteractQuery action = new InteractQuery()
                {
                    UID = killer.Player.UID,
                    X = X,
                    Y = Y,
                    AtkType = MsgAttackPacket.AttackID.Death,
                    KillCounter = killer.Player.KillCounter,
                    SpellID = (ushort)(Database.ItemType.IsBow(killer.Equipment.RightWeapon) ? 5 : 1),
                    OpponentUID = UID,
                };
                killer.Player.View.SendView(stream.InteractionCreate(&action), true);


                Game.MsgServer.MsgUpdate upd = new Game.MsgServer.MsgUpdate(stream, UID, 2);
                stream = upd.Append(stream, Game.MsgServer.MsgUpdate.DataType.Hitpoints, (long)HitPoints);
                stream = upd.Append(stream, Game.MsgServer.MsgUpdate.DataType.MaxHitpoints, (long)MaxHitPoints);
                stream = upd.GetArray(stream);
                killer.Player.View.SendView(stream, true);
                return;
            }
            if (IsStatue)
            {
                HitPoints = 0;
                Role.Statue.RemoveStatue(stream, killer, UID, this);
                return;
            }

            if (UID == Game.MsgTournaments.MsgSchedules.GuildWar.Furnitures[StaticMesh.RightGate].UID)
            {
                if (Game.MsgTournaments.MsgSchedules.GuildWar.Winner != null)
                {
                    Instance.Guild guild;
                    if (Instance.Guild.GuildPoll.TryGetValue(Game.MsgTournaments.MsgSchedules.GuildWar.Winner.GuildID, out guild))
                    {
                        guild.SendMessajGuild("[Guild War] The right gate has been breached!");
                    }
                }
                Mesh = StaticMesh.OpenRightGate;
                Game.MsgServer.MsgUpdate upd = new Game.MsgServer.MsgUpdate(stream, UID, 1);
                stream = upd.Append(stream, Game.MsgServer.MsgUpdate.DataType.Mesh, (long)Mesh);
                stream = upd.GetArray(stream);
                foreach (var client in Database.Server.GamePoll.Values)
                {
                    if (client.Player.Map == Map)
                    {
                        if (Role.Core.GetDistance(client.Player.X, client.Player.Y, X, Y) <= Role.SobNpc.SeedDistrance)
                        {
                            client.Send(stream);
                        }

                    }
                }
            }
            if (UID == Game.MsgTournaments.MsgSchedules.GuildWar.Furnitures[StaticMesh.LeftGate].UID)
            {
                if (Game.MsgTournaments.MsgSchedules.GuildWar.Winner != null)
                {
                    Instance.Guild guild;
                    if (Instance.Guild.GuildPoll.TryGetValue(Game.MsgTournaments.MsgSchedules.GuildWar.Winner.GuildID, out guild))
                    {
                        guild.SendMessajGuild("[Guild War] The left gate has been breached!");
                    }
                }
                Mesh = StaticMesh.OpenLeftGate;

                Game.MsgServer.MsgUpdate upd = new Game.MsgServer.MsgUpdate(stream, UID, 1);
                stream = upd.Append(stream, Game.MsgServer.MsgUpdate.DataType.Mesh, (long)Mesh);
                stream = upd.GetArray(stream);
                foreach (var client in Database.Server.GamePoll.Values)
                {
                    if (client.Player.Map == Map)
                    {
                        if (Role.Core.GetDistance(client.Player.X, client.Player.Y, X, Y) <= Role.SobNpc.SeedDistrance)
                        {
                            client.Send(stream);
                        }

                    }
                }
            }

            if (UID == 890)
            {
                uint Damage = (uint)HitPoints;
                if (HitPoints > 0)
                {
                    HitPoints = 0;
                }
                //Game.MsgTournaments.MsgSchedules.CityWar.CurentWar.UpdateScore(killer.Player, Damage);
            }

            else if (UID == Game.MsgTournaments.MsgSchedules.GuildWar.Furnitures[StaticMesh.Pole].UID)
            {
                uint Damage = (uint)HitPoints;
                if (HitPoints > 0)
                {
                    HitPoints = 0;
                }
                Game.MsgTournaments.MsgSchedules.GuildWar.UpdateScore(killer.Player, Damage);
            }
            #region _ExtremeFlagWar
            else if (UID == Game.MsgTournaments.MsgSchedules._ExtremeFlagWar.Furnitures[StaticMesh.Pole].UID)
            {
                uint Damage = (uint)HitPoints;
                if (HitPoints > 0)
                {
                    HitPoints = 0;
                }
                Game.MsgTournaments.MsgSchedules._ExtremeFlagWar.UpdateScore(killer.Player, Damage);
            }
            #endregion
            #region _EliteGuildWar
            else if (UID == Game.MsgTournaments.MsgSchedules._EliteGuildWar.Furnitures[StaticMesh.Pole].UID)
            {
                uint Damage = (uint)HitPoints;
                if (HitPoints > 0)
                {
                    HitPoints = 0;
                }
                Game.MsgTournaments.MsgSchedules._EliteGuildWar.UpdateScore(killer.Player, Damage);
            }
            #endregion
            #region _FirePoleWar
            //else if (UID == Game.MsgTournaments.MsgSchedules._FirePoleWar.Furnitures[StaticMesh.Pole].UID)
            //{
            //    uint Damage = (uint)HitPoints;
            //    if (HitPoints > 0)
            //    {
            //        HitPoints = 0;
            //    }
            //    Game.MsgTournaments.MsgSchedules._FirePoleWar.UpdateScore(killer.Player, Damage);
            //}
            #endregion
            else if (HitPoints > 0)
            {
                HitPoints = 0;
            }

        }
        public unsafe void SendString(ServerSockets.Packet stream, Game.MsgServer.MsgStringPacket.StringID id, params string[] args)
        {
            Game.MsgServer.MsgStringPacket packet = new Game.MsgServer.MsgStringPacket();
            packet.ID = id;
            packet.UID = UID;
            packet.Strings = args;

            SendScrennPacket(stream.StringPacketCreate(packet));
        }

        public unsafe void SendScrennPacket(ServerSockets.Packet packet)
        {
            foreach (var client in Database.Server.GamePoll.Values)
            {
                if (client.Player.Map == Map)
                {
                    if (client.Player.GetMyDistance(X, Y) < SeedDistrance)
                    {
                        client.Send(packet);
                    }
                }
            }
        }
        public unsafe ServerSockets.Packet GetArray(ServerSockets.Packet stream, bool view)
        {
            // for gw statue
            if (statue != null)
            {
                if (statue.StatuePacket != null && statue.Static)
                {
                    stream.Seek(0);
                    fixed (byte* ptr = statue.StatuePacket)
                    {
                        stream.memcpy(stream.Memory, ptr, statue.StatuePacket.Length);
                    }
                    stream.Size = statue.StatuePacket.Length;
                    return stream;
                }
                stream.InitWriter();

                //  stream.Write(Time32.Now.Value);
                //stream.Write((uint)(statue.user.Player.TransformationID * 10000000 + statue.user.Player.Face * 10000 + statue.user.Player.Body));
                stream.Write((uint)(statue.user.Player.Mesh));
                stream.Write(UID);//8
                if (statue.user.Player.MyGuild != null)
                {
                    stream.Write((ushort)statue.user.Player.GuildID);//12 ushort
                    stream.ZeroFill(1);// guild bransh maybe?
                    stream.Write((byte)statue.user.Player.GuildRank);//15
                }
                else
                    stream.ZeroFill(4);//15 total

                for (uint x = 0; x < statue.user.Player.BitVector.bits.Length; x++)
                    stream.Write((uint)statue.user.Player.BitVector.bits[x]);//16

                stream.Write(statue.user.Player.HeadId);//24

               
                stream.Write(statue.user.Player.GarmentId);//28
                stream.Write(statue.user.Player.ArmorId);//32

                stream.Write(statue.user.Player.LeftWeaponId);//36
                stream.Write(statue.user.Player.RightWeaponId);//40
                stream.ZeroFill(4);
                stream.Write(HitPoints / 2900);//48
                stream.Write(statue.user.Player.Hair);//52
                stream.Write(X);//54
                stream.Write(Y);//56
                if (statue.Static)
                    stream.Write((byte)0);
                else
                    stream.Write((byte)statue.user.Player.Angle);

                if (statue.Static)
                    stream.Write((ushort)Role.Flags.ConquerAction.Sit);
                else
                    stream.Write((ushort)statue.Action);

                stream.Write((byte)statue.user.Player.Reborn);//61
                stream.Write((ushort)statue.user.Player.Level);//62
                stream.Write((byte)0);//64
                stream.Write(statue.user.Player.ExtraBattlePower);//65
                stream.ZeroFill(8);
                stream.Write((statue.user.Player.FlowerRank + 10000));//77
                stream.Write((uint)statue.user.Player.NobilityRank);//81
                stream.Write(statue.user.Player.ColorArmor);//85
                stream.Write(statue.user.Player.ColorShield);//87
                stream.Write(statue.user.Player.ColorHelment);//89
                stream.Write(statue.user.Player.QuizPoints);//quiz points 91
                stream.Write(statue.user.Player.Name, string.Empty, string.Empty);
                stream.Finalize(Game.GamePackets.SpawnPlayer);
                if (statue.StatuePacket == null && statue.Static)
                {
                    statue.StatuePacket = new byte[stream.Size];
                    int size = stream.Size;
                    fixed (byte* ptr = statue.StatuePacket)
                    {
                        stream.memcpy(ptr, stream.Memory, size);
                    }
                }
                return stream;
            }
            stream.InitWriter();

            stream.Write(UID);
            stream.Write(MaxHitPoints);
            stream.Write(HitPoints);
            stream.Write(X);
            stream.Write(Y);//18
            stream.Write((ushort)Mesh);//20
            stream.Write((ushort)Type);//22
            stream.Write((ushort)Sort);//24
            //stream.ZeroFill(1);
            if (Name != "")
            {
                // stream.Write((byte)1);
                if (Name != null)
                {
                    if (Name.Length > 16)
                        Name = Name.Substring(0, 16);

                    stream.Write(Name);
                }
            }
            stream.Finalize(Game.GamePackets.SobNpcs);

            return stream;


        }
    }
}
