using COServer.Client;
using COServer.Database;
using COServer.Game.MsgMonster;
using COServer.Game.MsgServer;
using System;
using static COServer.Game.MsgServer.MsgPetInfo;

namespace COServer.Role
{
    public unsafe class MonsterPet
    {
        public void RemoveThat(Client.GameClient _Owner)
        {

            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();

                ActionQuery action = new ActionQuery()
                {
                    ObjId = this.monster.UID,
                    Type = ActionType.RemoveEntity
                };
                Owner.Player.View.SendView(stream.ActionCreate(&action), true);
                _Owner.Pet = null;
            }

        }
        public Game.MsgMonster.MonsterFamily Family;
        public MonsterRole monster;
        public Client.GameClient Owner;
        public Time32 AttackStamp = new Time32();

        public MonsterPet(Role.Player role, string Name, ServerSockets.Packet stream)
        {
            Owner = role.Owner;
            Owner.Pet = this;
            Family = new Game.MsgMonster.MonsterFamily();
            Family.SpellId = Server.Pets.ReadUInt16(Name, "SpellID", 0);
            Family.Level = Server.Pets.ReadUInt16(Name, "Level", 0);
            Family.MaxAttack = Server.Pets.ReadInt32(Name, "Attack", 0);
            Family.MinAttack = Family.MaxAttack;
            Family.Mesh = Server.Pets.ReadUInt16(Name, "Mesh", 0);
            Family.MaxHealth = Server.Pets.ReadInt32(Name, "Hitpoints", 0);
            Family.Defense = Server.Pets.ReadUInt16(Name, "Defence", 0);
            Family.AttackRange = Server.Pets.ReadSByte(Name, "AttackRange", 0);
            Family.Name = Server.Pets.ReadString(Name, "Name", "ERROR");
            Family.MapID = role.Map;
            monster = new MonsterRole(Family, Family.ID, string.Empty, Owner.Map);
            monster.ObjType = MapObjectType.Monster;
            monster.UID = 700000 + (role.UID - 1000000);
            monster.Name = Family.Name;
            monster.Level = (byte)Family.Level;
            monster.Mesh = Family.Mesh;
            monster.HitPoints = (uint)Family.MaxHealth;
            monster.X = Owner.Player.X;
            monster.Y = Owner.Player.Y;
            if (monster.HitPoints > 0)
            {
                Game.MsgServer.MsgUpdate Upd = new Game.MsgServer.MsgUpdate(stream, monster.UID, 2);
                stream = Upd.Append(stream, Game.MsgServer.MsgUpdate.DataType.MaxHitpoints, Family.MaxHealth);
                stream = Upd.Append(stream, Game.MsgServer.MsgUpdate.DataType.Hitpoints, monster.HitPoints);
                stream = Upd.GetArray(stream);
                Owner.Send(stream);
            }
            ActionQuery action = new ActionQuery()
            {
                ObjId = monster.UID,
                Type = ActionType.ReviveMonster,
                wParam1 = Owner.Player.X,
                wParam2 = Owner.Player.Y
            };
            monster.Send(stream.ActionCreate(&action));
            monster.Send(monster.GetArray(stream, false));
            SendView(stream);
        }
        public void SendView(ServerSockets.Packet stream)
        {
            Owner.Player.View.SendView(GetArray(stream, false), true);
        }

        public unsafe ServerSockets.Packet GetArray(ServerSockets.Packet stream, bool view)
        {
           
            stream.InitWriter();
            stream.Write(monster.Mesh);
            stream.Write(monster.UID);//8

            stream.Write(0);
            for (uint x = 0; x < monster.BitVector.bits.Length; x++)
                stream.Write((uint)monster.BitVector.bits[x]);//16 -- 18

            stream.ZeroFill(28);//12 -- 46
            stream.Write((ushort)monster.HitPoints);//48
            stream.ZeroFill(4);
            //stream.Write((ushort)Level);//62
            stream.Write(monster.X);
            stream.Write(monster.Y);//60

            //stream.Write((ushort)0);
            stream.Write((byte)monster.Facing);
            stream.Write((byte)monster.Action);//59

            stream.ZeroFill(2);//reborn 68
            //  stream.ZeroFill(1+3);//reborn 68

            stream.Write((byte)monster.Level);//62

            //stream.ZeroFill(89);//86

            //stream.Write((byte)Boss);//175 
            stream.ZeroFill(32);//138
            string[] arraystring = new string[]
                {
                    monster.Name, string.Empty, string.Empty, string.Empty
                };
            stream.Write(arraystring);
            stream.Finalize(Game.GamePackets.SpawnPlayer);
            //    MyConsole.PrintPacketAdvanced(stream.Memory, stream.Size);

            return stream;
        }

        public void Attach(ServerSockets.Packet stream)
        {

            Owner.Pet.monster.GMap.View.EnterMap<MonsterRole>(monster);
            //Owner.Player.View.SendView(stream, false);

            //user.Player.View.SendView(stream, false);
        }



        public static sbyte[] XDir = new sbyte[] { 0, -1, -1, -1, 0, 1, 1, 1 };
        public static sbyte[] YDir = new sbyte[] { 1, 1, 0, -1, -1, -1, 0, 1 };
        public static sbyte[] XDir2 = new sbyte[] { 0, -2, -2, -2, 0, 2, 2, 2, -1, -2, -2, -1, 1, 2, 2, 1, -1, -2, -2, -1, 1, 2, 2, 1 };
        public static sbyte[] YDir2 = new sbyte[] { 2, 2, 0, -2, -2, -2, 0, 2, 2, 1, -1, -2, -2, -1, 1, 2, 2, 1, -1, -2, -2, -1, 1, 2 };

        public IMapObj Target { get; set; }

        public bool Move(Flags.ConquerAngle Direction)
        {
            ushort _X = monster.X, _Y = monster.Y;
            monster.Facing = Direction;
            int dir = ((int)Direction) % XDir.Length;
            sbyte xi = XDir[dir], yi = YDir[dir];
            _X = (ushort)(monster.X + xi);
            _Y = (ushort)(monster.Y + yi);
            Core.IncXY((Flags.ConquerAngle)dir, ref _X, ref _Y);
            if (monster.GMap.ValidLocation(_X, _Y) && !monster.GMap.MonsterOnTile(_X, _Y))
            {
                monster.GMap.SetMonsterOnTile(monster.X, monster.Y, false);
                monster.GMap.SetMonsterOnTile(_X, _Y, true);

                monster.GMap.View.MoveTo<Role.IMapObj>(monster, _X, _Y);
                monster.X = _X;
                monster.Y = _Y;
                return true;
            }
            return false;
        }
        public void DeAtach(ServerSockets.Packet stream)
        {
            ActionQuery action = new ActionQuery()
            {
                ObjId = monster.UID,
                Type = ActionType.RemoveEntity,
            };
            Owner.Send(stream.ActionCreate(&action));
            Owner.Player.View.SendView(stream.ActionCreate(&action), false);
            monster.GMap.View.LeaveMap<MonsterRole>(monster);
            Owner.Pet = null;
        }

        public void Attack(uint TagertUID, GameClient client, ServerSockets.Packet stream)
        {

            InteractQuery action = new InteractQuery()
            {
                UID = monster.UID,
                AtkType = MsgAttackPacket.AttackID.Physical
            };



        /*    Role.IMapObj target;
            if (client.Pet.Owner.Player.View.TryGetValue(TagertUID, out target, Role.MapObjectType.Player))
            {
                if (Game.MsgServer.AttackHandler.Calculate.Base.GetDistance(client.Player.X, client.Player.Y, target.X, target.Y) <= 3)
                {
                    Role.Player attacked = target as Role.Player;
                    if (Game.MsgServer.AttackHandler.CheckAttack.CanAttackPlayer.Verified(client, attacked, null))
                    {
                        // if(attacked.UID == client.Player.UID)

                        MsgSpellAnimation.SpellObj AnimationObj;
                        Game.MsgServer.AttackHandler.Calculate.Physical.OnPlayer(client.Player, attacked, null, out AnimationObj);
                        AnimationObj.Damage = (uint)(AnimationObj.Damage);
                        action.OpponentUID = target.UID;

                        action.Damage = (int)AnimationObj.Damage;

                        client.Player.View.SendView(stream.InteractionCreate(&action), true);

                        Game.MsgServer.AttackHandler.ReceiveAttack.Player.Execute(AnimationObj, client, attacked);
                    }
                }
            }
            if (client.Pet.Owner.Player.View.TryGetValue(TagertUID, out target, Role.MapObjectType.Monster))
            {

                if (Game.MsgServer.AttackHandler.Calculate.Base.GetDistance(client.Pet.monster.X, client.Pet.monster.Y, target.X, target.Y) <= 8)
                {
                    Game.MsgMonster.MonsterRole attacked = target as Game.MsgMonster.MonsterRole;
                    if (Game.MsgServer.AttackHandler.CheckAttack.CanAttackMonster.Verified(client.Pet.Owner, attacked, null))
                    {
                        MsgSpellAnimation.SpellObj AnimationObj;

                        Game.MsgServer.AttackHandler.Calculate.Physical.OnMonster(client.Pet.Owner.Player, attacked, null, out AnimationObj);

                        AnimationObj.Damage = (uint)(AnimationObj.Damage);
                        action.OpponentUID = target.UID;

                        action.Damage = (int)AnimationObj.Damage;

                        client.Player.View.SendView(stream.InteractionCreate(&action), true);


                       Game.MsgServer.AttackHandler.ReceiveAttack.Monster.Execute(stream, AnimationObj, client, attacked);
                    }
                }

            }

            if (client.Pet.Owner.Player.View.TryGetValue(TagertUID, out target, Role.MapObjectType.SobNpc))
            {
                if (Game.MsgServer.AttackHandler.Calculate.Base.GetDistance(client.Player.X, client.Player.Y, target.X, target.Y) <= 3)
                {
                    var attacked = target as Role.SobNpc;
                    if (Game.MsgServer.AttackHandler.CheckAttack.CanAttackNpc.Verified(client, attacked, null))
                    {
                        MsgSpellAnimation.SpellObj AnimationObj;
                        Game.MsgServer.AttackHandler.Calculate.Physical.OnNpcs(client.Player, attacked, null, out AnimationObj);

                        AnimationObj.Damage = (uint)(AnimationObj.Damage);
                        action.OpponentUID = target.UID;

                        action.Damage = (int)AnimationObj.Damage;

                        client.Player.View.SendView(stream.InteractionCreate(&action), true);


                        Game.MsgServer.AttackHandler.ReceiveAttack.Npc.Execute(stream, AnimationObj, client, attacked);
                    }
                }
            }*/
        }
    }

}