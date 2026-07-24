using COServer.Game.MsgServer;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace COServer.Game.MsgMonster
{
    public class ActionHandler
    {
        public MSRandom Random = new MSRandom();
        public Random SystemRandom = new Random();

        public bool Rate(int value)
        {
            return value > Random.Next() % 100;
        }

        public void ExecuteAction(Role.Player client, Game.MsgMonster.MonsterRole monster)
        {
            switch (monster.State)
            {
                case MobStatus.Idle:
                    {
                        monster.Target = null;
                        CheckTarget(client, monster); break;
                    }
                case MobStatus.SearchTarget: SearchTarget(client, monster); break;
                case MobStatus.Attacking: AttackingTarget(client, monster); break;
            }
        }
        public unsafe void CheckGuardPosition(Role.Player client, Game.MsgMonster.MonsterRole monster)
        {

            if (monster.Alive)
            {
                if (monster.X != monster.RespawnX || monster.Y != monster.RespawnY)
                {
                    if (Time32.Now > monster.MoveStamp.AddMilliseconds(300))
                    {
                        monster.MoveStamp = Time32.Now;
                        {
                            Role.Flags.ConquerAngle dir = GetAngle(monster.X, monster.Y, monster.RespawnX, monster.RespawnY);
                            ushort WalkX = monster.X; ushort WalkY = monster.Y;
                            IncXY(dir, ref WalkX, ref WalkY);

                            client.Owner.Map.View.MoveTo<Role.IMapObj>(monster, WalkX, WalkY);

                            monster.X = WalkX;
                            monster.Y = WalkY;

                            WalkQuery walk = new WalkQuery()
                            {
                                Direction = (byte)dir,
                                Running = 1,
                                UID = monster.UID
                            };
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();

                                monster.Send(stream.MovementCreate(&walk));
                            }
                        }
                    }
                }
            }
        }
        public unsafe bool JumpPos(Client.GameClient client, MonsterRole monster)
        {
            if ((client.Player.ContainFlag(MsgUpdate.Flags.FlashingName) || client.Player.ContainFlag(MsgUpdate.Flags.BlackName))
                && monster.Alive && Role.Core.GetDistance(monster.X, monster.Y, monster.RespawnX, monster.RespawnY) < 16)
            {
                short distance = Role.Core.GetDistance(monster.X, monster.Y, client.Player.X, client.Player.Y);
                if (distance >= 2)
                {
                    ushort X = (ushort)(client.Player.X + 2);// + Program.GetRandom.Next(2));
                    ushort Y = (ushort)(client.Player.Y + 2);//+ Program.GetRandom.Next(2));
                    if (!client.Map.ValidLocation(X, Y))
                    {
                        X = client.Player.X;
                        Y = client.Player.Y;
                    }


                    var data = new ActionQuery()
                    {
                        Type = ActionType.Jump,
                        dwParam = (uint)((Y << 16) | X),
                        wParam1 = X,
                        wParam2 = Y,
                        ObjId = monster.UID

                    };

                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        if (stream != null)
                        {
                            client.Player.View.SendView(stream.ActionCreate(&data), true);
                            monster.Facing = Role.Core.GetAngle(monster.X, monster.Y, X, Y);
                            monster.Action = Role.Flags.ConquerAction.Jump;
                            client.Map.View.MoveTo<Role.IMapObj>(monster, X, Y);
                            monster.X = X;
                            monster.Y = Y;
                            //                        if (!CheckRespouseDamage(client.Player, monster))
                            //{
                            //    uint Damage = MagicAttack(client, monster);
                            //    MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                            //               , 0, client.Player.X, client.Player.Y, (ushort)monster.Family.SpellId, 0, 0);
                            //    SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(client.Player.UID, Damage));
                            //    SpellPacket.SetStream(stream);
                            //    SpellPacket.Send(monster);
                            //    CheckForOponnentDead(client.Player, Damage, monster);
                            //}
                            //monster.UpdateMonsterView(null, stream);

                        }
                    }
                }
                return true;
            }
            else
            {
                if (monster.Alive)
                {
                    //if ((!client.Player.ContainFlag(MsgUpdate.Flags.FlashingName) || !client.Player.ContainFlag(MsgUpdate.Flags.BlackName)))
                    { 
                        short distance = Role.Core.GetDistance(monster.X, monster.Y, monster.RespawnX, monster.RespawnY);
                        if (distance >= 15)
                        {
                            if (monster.X != monster.RespawnX || monster.Y != monster.RespawnY)
                            {
                                if (!client.Map.ValidLocation(monster.RespawnX, monster.RespawnY))
                                {
                                    monster.RespawnX = client.Player.X;
                                    monster.RespawnY = client.Player.Y;
                                }
                                monster.X = monster.RespawnX;
                                monster.Y = monster.RespawnY;

                                var data = new ActionQuery()
                                {
                                    Type = ActionType.Jump,
                                    dwParam = (uint)((monster.RespawnY << 16) | monster.RespawnX),
                                    wParam1 = monster.RespawnX,
                                    wParam2 = monster.RespawnY,
                                    ObjId = monster.UID
                                };

                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();
                                    if (stream != null)
                                    {
                                        monster.Facing = Role.Core.GetAngle(monster.X, monster.Y, monster.RespawnX, monster.RespawnY);
                                        monster.Action = Role.Flags.ConquerAction.Jump;
                                        client.Map.View.MoveTo<Role.IMapObj>(monster, monster.RespawnX, monster.RespawnY);
                                        client.Player.View.SendView(stream.ActionCreate(&data), true);
                                    }
                                }
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            return false;
        }

        public unsafe bool GuardAttackPlayer(Role.Player client, Game.MsgMonster.MonsterRole monster)
        {
            if (!monster.Alive)
                return false;
            if (client.ContainFlag(MsgServer.MsgUpdate.Flags.FlashingName) && client.Alive)
            {

                short distance = MonsterView.GetDistance(client.X, client.Y, monster.X, monster.Y);
                if (distance < monster.Family.AttackRange)
                {

                    if (!CheckRespouseDamage(client, monster))
                    {

                        uint Damage = MagicAttack(client.Owner, monster);
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();

                            /*  if ((monster.Family.Settings & MsgMonster.MonsterSettings.Guard) == MsgMonster.MonsterSettings.Guard)
                                  Damage = 1000;*/
                            MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                , 0, client.X, client.Y, (ushort)monster.Family.SpellId, 0, 0);
                            SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(client.UID, Damage));
                            SpellPacket.SetStream(stream);
                            SpellPacket.Send(monster);
                        }
                        CheckForOponnentDead(client, Damage, monster);
                        return true;
                    }
                }
            }
            return false;
        }
        public unsafe bool GuardAttackMonster(Role.GameMap map, Game.MsgMonster.MonsterRole attacked, Game.MsgMonster.MonsterRole monster)
        {
            if (!monster.Alive)
                return false;
            if (attacked.Alive)
            {
                short distance = MonsterView.GetDistance(attacked.X, attacked.Y, monster.X, monster.Y);
                if (distance < monster.Family.AttackRange)
                {
                    uint Damage = MagicAttack(attacked, monster);
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();

                        MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                            , 0, attacked.X, attacked.Y, (ushort)monster.Family.SpellId, 0, 0);
                        SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(attacked.UID, Damage));
                        SpellPacket.SetStream(stream);
                        SpellPacket.Send(monster);

                        if (Damage >= attacked.HitPoints)
                        {
                            map.SetMonsterOnTile(attacked.X, attacked.Y, false);
                            attacked.Dead(stream, null, monster.UID, map);
                        }
                        else
                            attacked.HitPoints -= Damage;

                    }
                    return true;
                }
            }
            return false;
        }
        public bool ExtraBoss(Game.MsgMonster.MonsterRole monster)
        {
            return monster.Family.MaxHealth > 300000 && monster.Family.MaxHealth < 7000000;
        }
        public unsafe void AttackingTarget(Role.Player client, Game.MsgMonster.MonsterRole monster)
        {
            if (!monster.Alive)
                return;
            short distance = MonsterView.GetDistance(monster.Target.X, monster.Target.Y, monster.X, monster.Y);

            if (monster.Boss == 1 && monster.HitPoints >= 2000000)
                monster.Family.AttackRange = 18;

            if (distance > monster.Family.AttackRange || monster.Target == null || !monster.Target.Alive 
                /*|| monster.Target.ContainFlag(MsgServer.MsgUpdate.Flags.Fly)*/
                || !monster.Target.Owner.Socket.Alive)
            {
                monster.State = MobStatus.SearchTarget;
            }
            else
            {
                if (Time32.Now > monster.AttackSpeed.AddMilliseconds(monster.Family.AttackSpeed))
                {
                    monster.AttackSpeed = Time32.Now;
                    if (ExtraBoss(monster))
                    {
                        if (!CheckRespouseDamage(client, monster))
                        {
                            ushort SpellID = 1001;
                            if (monster.Level < 80)
                                SpellID = 1001;
                            else if (monster.Level < 60)
                                SpellID = 1001;
                            

                            uint Damage = MagicAttack(monster.Target.Owner, monster);

                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();

                                MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                    , 0, monster.Target.X, monster.Target.Y, (ushort)SpellID, 0, 0);
                                SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(monster.Target.UID, Damage));
                                SpellPacket.SetStream(stream);
                                SpellPacket.Send(monster);
                            }

                            CheckForOponnentDead(monster.Target, Damage, monster);
                        }
                        return;
                    }
                    if (monster.Boss == 0 && monster.Family.SpellId == 0)
                    {
                        if (!CheckRespouseDamage(client, monster))
                        {
                            uint Damage = PhysicalAttack(monster.Target.Owner, monster);
                            Damage = CheckDodge(monster.Target.Owner.Status.Dodge) ? Damage : 0;
                            if (Damage >= 1)
                            {
                                MsgServer.AttackHandler.CheckAttack.CheckItems.RespouseDurability(monster.Target.Owner);
                            }
                            InteractQuery action = new InteractQuery()
                            {
                                AtkType = MsgAttackPacket.AttackID.Physical,
                                X = monster.Target.X,
                                Y = monster.Target.Y,
                                UID = monster.UID,
                                OpponentUID = monster.Target.UID,
                                Damage = (int)Damage
                            };
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                monster.Send(stream.InteractionCreate(&action));
                            }

                            CheckForOponnentDead(monster.Target, Damage, monster);
                        }
                    }
                    else if (monster.Family.SpellId != 0 && monster.Boss == 0)
                    {
                        if (!CheckRespouseDamage(client, monster))
                        {
                            uint Damage = MagicAttack(monster.Target.Owner, monster);
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();

                                MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                    , 0, monster.Target.X, monster.Target.Y, (ushort)monster.Family.SpellId, 0, 0);
                                SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(monster.Target.UID, Damage));
                                SpellPacket.SetStream(stream);
                                SpellPacket.Send(monster);
                            }

                            CheckForOponnentDead(monster.Target, Damage, monster);
                        }
                    }
                    else if (monster.Boss != 0)
                    {
                        if (!monster.Target.Alive || Role.Core.GetDistance(monster.X, monster.Y, monster.Target.X, monster.Target.Y) > 18)
                        {
                            monster.State = MobStatus.SearchTarget;
                            return;
                        }
                        switch (monster.Family.ID)
                        {
                            /// ancdevail
                            case 9111:
                                 {
                                     List<ushort> Spells = new List<ushort>() { 1002, 1120, 1290};
                                     ushort rand = (byte)Program.GetRandom.Next(0, Spells.Count);
                                     switch (Spells[rand])
                                     {
                                         case 1120:
                                         case 1002:
                                         case 1290:
                                             {
                                                 using (var rec = new ServerSockets.RecycledPacket())
                                                 {
                                                     var stream = rec.GetStream();

                                                     MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                                                        , 0, monster.Target.X, monster.Target.Y, (ushort)Spells[rand], 0, 0);

                                                     foreach (var targent in monster.View.Roles(client.Owner.Map, Role.MapObjectType.Player))
                                                     {
                                                         if (!targent.Alive)
                                                             continue;
                                                         var player = targent as Role.Player;
                                                         if (Role.Core.GetDistance(monster.X, monster.Y, player.X, player.Y) <= 7)
                                                         {
                                                             uint Damage = MagicAttack(player.Owner, monster);
                                                             SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(player.UID, Damage
                                                                 ));
                                                             CheckForOponnentDead(player, Damage, monster);

                                                             //if (Rate(5) && !player.ContainFlag(MsgServer.MsgUpdate.Flags.Freeze))
                                                             //    player.AddFlag(MsgServer.MsgUpdate.Flags.Freeze, 3, true);
                                                         }
                                                     }
                                                     SpellPacket.SetStream(stream);
                                                     SpellPacket.Send(monster);
                                                 }
                                                 break;
                                             }

                                     }
                                     break;
                                 }
                          
                            case 20070://SnowBanshee
                                {
                                    List<ushort> Spells = new List<ushort>() { 1125, 1290, 3090 };
                                    ushort rand = (byte)Program.GetRandom.Next(0, Spells.Count);
                                    uint Damage = MagicAttack(monster.Target.Owner, monster);
                                    if (rand == 0)
                                        Damage = (uint)Game.MsgServer.AttackHandler.Calculate.Base.MulDiv((int)Damage, 130, 100);
                                    switch (Spells[rand])
                                    {
                                        //case 3090:
                                        //    {
                                        //        using (var rec = new ServerSockets.RecycledPacket())
                                        //        {
                                        //            var stream = rec.GetStream();

                                        //            MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                        //                               , 0, monster.Target.X, monster.Target.Y, (ushort)Spells[rand], 5, 0);
                                        //            monster.Target.SendString(stream, MsgStringPacket.StringID.Effect, true, "zf2-e267");
                                        //            foreach (var targent in monster.View.Roles(client.Owner.Map, Role.MapObjectType.Player))
                                        //            {
                                        //                if (!targent.Alive)
                                        //                    continue;
                                        //                var player = targent as Role.Player;
                                        //                if (Role.Core.GetDistance(monster.Target.X, monster.Target.Y, player.X, player.Y) <= 8)
                                        //                {
                                        //                    SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(player.UID, Damage
                                        //                        ));
                                        //                }
                                        //            }
                                        //            SpellPacket.SetStream(stream);
                                        //            SpellPacket.Send(monster);
                                        //        }
                                        //        break;
                                        //    }
                                        default:
                                            {
                                                using (var rec = new ServerSockets.RecycledPacket())
                                                {
                                                    var stream = rec.GetStream();
                                                    if (Spells[rand] == 3090)
                                                    {
                                                        monster.Target.SendString(stream, MsgStringPacket.StringID.Effect, true, "zf2-e267");
                                                    }
                                                    else if (Spells[rand] == 1290)
                                                    {
                                                        monster.Target.SendString(stream, MsgStringPacket.StringID.Effect, true, "_p_19_daboluo110");
                                                    }
                                                    MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                                                                  , 0, monster.Target.X, monster.Target.Y, (ushort)Spells[rand], 0, 0);
                                                    SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(monster.Target.UID, Damage));

                                                    SpellPacket.SetStream(stream);
                                                    SpellPacket.Send(monster);

                                                }
                                                break;
                                            }
                                    }

                                    CheckForOponnentDead(monster.Target, Damage, monster);
                                    break;
                                }
                            case 20060://DRAGON
                                {
                                    List<ushort> Spells = new List<ushort>() { 1290, 3090, 10183 };
                                    ushort rand = (byte)Program.GetRandom.Next(0, Spells.Count);
                                    uint Damage = MagicAttack(monster.Target.Owner, monster);
                                    if (rand == 0)
                                        Damage = (uint)Game.MsgServer.AttackHandler.Calculate.Base.MulDiv((int)Damage, 130, 100);
                                    switch (Spells[rand])
                                    {
                                        case 10183:
                                            {
                                                using (var rec = new ServerSockets.RecycledPacket())
                                                {
                                                    var stream = rec.GetStream();

                                                    MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                                                       , 0, monster.Target.X, monster.Target.Y, (ushort)Spells[rand], 0, 0);

                                                    foreach (var targent in monster.View.Roles(client.Owner.Map, Role.MapObjectType.Player))
                                                    {
                                                        if (!targent.Alive)
                                                            continue;
                                                        var player = targent as Role.Player;
                                                        if (Role.Core.GetDistance(monster.Target.X, monster.Target.Y, player.X, player.Y) <= 8)
                                                        {
                                                            SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(player.UID, Damage
                                                                ));
                                                        }
                                                    }
                                                    SpellPacket.SetStream(stream);
                                                    SpellPacket.Send(monster);
                                                }
                                                break;
                                            }
                                        default:
                                            {
                                                using (var rec = new ServerSockets.RecycledPacket())
                                                {
                                                    var stream = rec.GetStream();
                                                    if (Spells[rand] == 3090)
                                                    {
                                                        monster.Target.SendString(stream, MsgStringPacket.StringID.Effect, true, "change");
                                                    }
                                                    else if (Spells[rand] == 1290)
                                                    {
                                                        monster.Target.SendString(stream, MsgStringPacket.StringID.Effect, true, "_p_19_daboluo110");
                                                    }
                                                    MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                                                                  , 0, monster.Target.X, monster.Target.Y, (ushort)Spells[rand], 0, 0);
                                                    SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(monster.Target.UID, Damage));

                                                    SpellPacket.SetStream(stream);
                                                    SpellPacket.Send(monster);

                                                }
                                                break;
                                            }
                                    }

                                    CheckForOponnentDead(monster.Target, Damage, monster);
                                    break;
                                }
                            case 3822://Raikou
                                {
                                    List<ushort> Spells = new List<ushort>() { 5001, 6004, 3090,1234 };
                                    ushort rand = (byte)Program.GetRandom.Next(0, Spells.Count);
                                    uint Damage = MagicAttack(monster.Target.Owner, monster);
                                    if (rand == 0)
                                        Damage = (uint)Game.MsgServer.AttackHandler.Calculate.Base.MulDiv((int)Damage, 130, 100);
                                    switch (Spells[rand])
                                    {
                                        case 1234:
                                            {
                                                using (var rec = new ServerSockets.RecycledPacket())
                                                {
                                                    var stream = rec.GetStream();

                                                    foreach (var targent in monster.View.Roles(client.Owner.Map, Role.MapObjectType.Player))
                                                    {
                                                        if (!targent.Alive)
                                                            continue;
                                                        var player = targent as Role.Player;
                                                        if (Role.Core.GetDistance(monster.Target.X, monster.Target.Y, player.X, player.Y) <= 8)
                                                        {
                                                            if (!player.ContainFlag(MsgUpdate.Flags.Fly))
                                                            {
                                                                byte Todir = (byte)(7 - (Math.Floor(BaseFunc.PointDirecton(monster.X, monster.Y, player.X, player.Y) / 45 % 8)) - 1 % 8);
                                                                byte Dirc = (byte)((byte)Todir % 8);
                                                                if (Dirc == 0)
                                                                    player.Y += 5;
                                                                else if (Dirc == 2)//nw
                                                                    player.X -= 5;

                                                                else if (Dirc == 4)//NE
                                                                    player.Y -= 5;
                                                                else if (Dirc == 6)
                                                                    player.X += 5;

                                                                else if (Dirc == 1)
                                                                {
                                                                    player.Y -= 5;
                                                                    player.X += 5;
                                                                }
                                                                else if (Dirc == 3)
                                                                {
                                                                    player.Y -= 5;
                                                                    player.X += 5;

                                                                }
                                                                else if (Dirc == 7)
                                                                {
                                                                    player.Y += 7;
                                                                    player.X += 7;

                                                                }
                                                                var Map = Database.Server.ServerMaps[1787];
                                                                if (Map.ValidLocation(player.X, player.Y))
                                                                {
                                                                    ActionQuery query = new ActionQuery()
                                                                    {
                                                                        Type = ActionType.FlashStep,
                                                                        ObjId = player.UID,
                                                                        wParam1 = player.X,
                                                                        wParam2 = player.Y
                                                                    };
                                                                    monster.Send(stream.ActionCreate(&query));
                                                                }
                                                                
                                                            }
                                                        }
                                                    }
                                                }
                                                break;
                                            }
                                        default:
                                            {
                                                using (var rec = new ServerSockets.RecycledPacket())
                                                {
                                                    var stream = rec.GetStream();
                                                    if (Spells[rand] == 3090)
                                                    {
                                                        monster.Target.SendString(stream, MsgStringPacket.StringID.Effect, true, "break_accept");
                                                    }
                                                    MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                                                                  , 0, monster.Target.X, monster.Target.Y, (ushort)Spells[rand], 0, 0);
                                                    SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(monster.Target.UID, Damage));

                                                    SpellPacket.SetStream(stream);
                                                    SpellPacket.Send(monster);

                                                }
                                                break;
                                            }
                                    }

                                    CheckForOponnentDead(monster.Target, Damage, monster);
                                    break;
                                }
                            case 20160://ThrillingSpook
                                {
                                    List<ushort> Spells = new List<ushort>() { 1290, 3090, 1234 };
                                    ushort rand = (byte)Program.GetRandom.Next(0, Spells.Count);
                                    uint Damage = MagicAttack(monster.Target.Owner, monster);
                                    if (rand == 0)
                                        Damage = (uint)Game.MsgServer.AttackHandler.Calculate.Base.MulDiv((int)Damage, 130, 100);
                                    switch (Spells[rand])
                                    {
                                        case 1234://Whirlpool
                                            {
                                               
                                                using (var rec = new ServerSockets.RecycledPacket())
                                                {
                                                    var stream = rec.GetStream();

                                                    MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                                                       , 0, monster.Target.X, monster.Target.Y, (ushort)Spells[rand], 0, 0);

                                                    foreach (var targent in monster.View.Roles(client.Owner.Map, Role.MapObjectType.Player))
                                                    {
                                                        if (!targent.Alive)
                                                            continue;
                                                        var player = targent as Role.Player;
                                                        if (Role.Core.GetDistance(monster.Target.X, monster.Target.Y, player.X, player.Y) <= 8)
                                                        {
                                                            uint pct = 5 / 100;
                                                            uint hpredc = (uint)(player.HitPoints * pct);
                                                            if (hpredc == 0)
                                                                hpredc = 500;
                                                            monster.Target.SendString(stream, MsgStringPacket.StringID.Effect, true, "zf2-e121");
                                                            SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(player.UID, hpredc
                                                                ));
                                                        }
                                                    }
                                                    SpellPacket.SetStream(stream);
                                                    SpellPacket.Send(monster);
                                                }
                                                break;
                                            }
                                        default:
                                            {
                                                using (var rec = new ServerSockets.RecycledPacket())
                                                {
                                                    var stream = rec.GetStream();
                                                    if (Spells[rand] == 3090)
                                                    {
                                                        monster.Target.SendString(stream, MsgStringPacket.StringID.Effect, true, "colorstar");
                                                    }
                                                    else if (Spells[rand] == 1290)
                                                    {
                                                        monster.Target.SendString(stream, MsgStringPacket.StringID.Effect, true, "fam_exp_special");
                                                    }
                                                    MsgServer.MsgSpellAnimation SpellPacket = new MsgServer.MsgSpellAnimation(monster.UID
                                                                                  , 0, monster.Target.X, monster.Target.Y, (ushort)Spells[rand], 0, 0);
                                                    SpellPacket.Targets.Enqueue(new MsgServer.MsgSpellAnimation.SpellObj(monster.Target.UID, Damage));

                                                    SpellPacket.SetStream(stream);
                                                    SpellPacket.Send(monster);

                                                }
                                                break;
                                            }
                                    }

                                    CheckForOponnentDead(monster.Target, Damage, monster);
                                    break;
                                }

                        }
                        //bosses
                    }
                }
            }
        }
        public unsafe void SearchTarget(Role.Player client, Game.MsgMonster.MonsterRole monster)
        {
            if (monster == null)
                return;
            if (!monster.Alive)
                return;
            try
            {
                if (monster.Target != null)
                {
                    if (!monster.Target.Alive || monster.Target.ContainFlag(MsgServer.MsgUpdate.Flags.Fly)
                        || !monster.Target.Owner.Socket.Alive)
                    {
                        monster.State = MobStatus.Idle;
                        return;
                    }
                }
                if (monster.Target == null)
                {
                    monster.State = MobStatus.Idle;
                    return;
                }
                if (monster.Family == null)
                    return;
                short distance = MonsterView.GetDistance(monster.Target.X, monster.Target.Y, monster.X, monster.Y);
                if (distance <= monster.Family.AttackRange || (monster.Boss == 1 && monster.HitPoints > 2000000 && distance <= 18))
                {
                    monster.State = MobStatus.Attacking;
                }
                else
                {
                    monster.State = MobStatus.Idle;
                }
                if (distance <= monster.Family.ViewRange && monster.Target != null && monster.Target.Alive)
                {

                    try
                    {
                        if (Time32.Now > monster.MoveStamp.AddMilliseconds(monster.Family.MoveSpeed))
                        {
                            monster.MoveStamp = Time32.Now;

                            bool Walk = Random.Next() % 100 < 70;
                            if (Walk)
                            {/*
                              * 05:22:57] System.NullReferenceException: Object reference not set to an instance of an object.
   at COServer.Game.MsgMonster.ActionHandler.SearchTarget(Player client, MonsterRole monster) in C:\Users\Jason\Desktop\COServer\COServer\COServer\Game\MsgMonster\ActionHandler.cs:line 902
                                */
                                Role.Flags.ConquerAngle dir = GetAngle(monster.X, monster.Y, monster.Target.X, monster.Target.Y);
                                ushort WalkX = monster.X; ushort WalkY = monster.Y;
                                IncXY(dir, ref WalkX, ref WalkY);
                                var Map = monster.Target.Owner.Map;
                                if (Map.ValidLocation(WalkX, WalkY) && !Map.MonsterOnTile(WalkX, WalkY))
                                {
                                    Map.SetMonsterOnTile(monster.X, monster.Y, false);
                                    Map.SetMonsterOnTile(WalkX, WalkY, true);

                                    Map.View.MoveTo<Role.IMapObj>(monster, WalkX, WalkY);

                                    monster.X = WalkX; monster.Y = WalkY;


                                    WalkQuery action = new WalkQuery()
                                    {
                                        Direction = (byte)dir,
                                        Running = 1,
                                        UID = monster.UID
                                    };
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        monster.Send(stream.MovementCreate(&action));
                                    }
                                }
                                else
                                {
                                    dir = (Role.Flags.ConquerAngle)(Random.Next() % 8);
                                    WalkX = monster.X; WalkY = monster.Y;
                                    IncXY(dir, ref WalkX, ref WalkY);

                                    if (Map.ValidLocation(WalkX, WalkY) && !Map.MonsterOnTile(WalkX, WalkY))
                                    {
                                        Map.SetMonsterOnTile(monster.X, monster.Y, false);
                                        Map.SetMonsterOnTile(WalkX, WalkY, true);

                                        Map.View.MoveTo<Role.IMapObj>(monster, WalkX, WalkY);

                                        monster.X = WalkX; 
                                        monster.Y = WalkY;


                                        WalkQuery action = new WalkQuery()
                                        {
                                            Direction = (byte)dir,
                                            Running = 1,
                                            UID = monster.UID
                                        };
                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            var stream = rec.GetStream();
                                            monster.Send(stream.MovementCreate(&action));

                                            monster.UpdateMonsterView(monster.Target.View, stream);
                                        }
                                    }
                                }
                            }

                        }

                    }
                    catch (Exception e) { Console.WriteLine(e.ToString()); }
                }
                else
                    monster.State = MobStatus.Idle;
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        public void CheckTarget(Role.Player client, Game.MsgMonster.MonsterRole monster)
        {
            if (!monster.Alive)
                return;
            if (monster.Target == null && !client.ContainFlag(MsgServer.MsgUpdate.Flags.Fly))
            {
                short distance = MonsterView.GetDistance(client.X, client.Y, monster.X, monster.Y);
                if (distance <= monster.Family.ViewRange && client.Alive)
                {
                    var targ = monster.View.GetTarget(client.Owner.Map, Role.MapObjectType.Player);
                    if (targ != null)
                    {
                        monster.Target = targ as Role.Player;
                        if (!monster.Target.ContainFlag(MsgServer.MsgUpdate.Flags.Fly))
                        {
                            monster.State = MobStatus.SearchTarget;
                        }
                        else
                        {
                            foreach (var OtherTarget in monster.View.Roles(client.Owner.Map, Role.MapObjectType.Player))
                            {
                                var obj = OtherTarget as Role.Player;
                                if (!obj.ContainFlag(MsgServer.MsgUpdate.Flags.Fly))
                                {
                                    monster.Target = obj;
                                    monster.State = MobStatus.SearchTarget;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                short distance = MonsterView.GetDistance(client.X, client.Y, monster.X, monster.Y);
                if (monster.Target == null)
                {
                    var targ = monster.View.GetTarget(client.Owner.Map, Role.MapObjectType.Player);
                    if (targ != null)
                    {
                        monster.Target = targ as Role.Player;
                        if (!monster.Target.ContainFlag(MsgServer.MsgUpdate.Flags.Fly) || monster.Boss == 1)
                        {
                            monster.State = MobStatus.SearchTarget;
                        }
                        else
                        {
                            foreach (var OtherTarget in monster.View.Roles(client.Owner.Map, Role.MapObjectType.Player))
                            {
                                var obj = OtherTarget as Role.Player;
                                if (!obj.ContainFlag(MsgServer.MsgUpdate.Flags.Fly))
                                {
                                    monster.Target = obj;
                                    monster.State = MobStatus.SearchTarget;
                                }
                            }
                        }
                    }
                }
                if (monster.Target != null && (distance > monster.Family.ViewRange || monster.Target.ContainFlag(MsgServer.MsgUpdate.Flags.Fly)
                    || monster.Target.Owner.Socket == null || !monster.Target.Owner.Socket.Alive))
                {
                    monster.Target = null;
                }
                else if (monster.Target == null || monster.Target.Alive)
                    monster.State = MobStatus.SearchTarget;
            }
        }
        public bool CheckDodge(uint Dodge)
        {
            bool allow = true;
            uint Noumber = (uint)Random.Next() % 150;
            if (Noumber > 60)
                Noumber = (uint)Random.Next() % 150;
            if (Noumber < Dodge)
                allow = false;
            return allow;
        }
        public void CheckForOponnentDead(Role.Player Player, uint Damage, MonsterRole monster)
        {
            if (Player.Alive == false)
                return;

            if (Player.ActivePick)
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    Player.RemovePick(stream);
                }
            }
            if (!Player.Owner.Socket.Alive)
                return;
            if (Damage >= Player.HitPoints)
            {

                ushort X = Player.X;
                ushort Y = Player.Y;
                Player.Dead(null, X, Y, monster.UID);
            }
            else
            {
                if (Player.Action == Role.Flags.ConquerAction.Sit)
                {
                    if (Player.Stamina >= 10)
                        Player.Stamina -= 10;
                    else
                        Player.Stamina = 0;

                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        Player.SendUpdate(stream, Player.Stamina, MsgServer.MsgUpdate.DataType.Stamina);
                    }


                    Player.Action = Role.Flags.ConquerAction.None;
                }

                Player.HitPoints -= (int)Damage;
            }
        }
        public unsafe bool CheckRespouseDamage(Role.Player player, MonsterRole Monster)
        {
            if (MsgServer.AttackHandler.Calculate.Base.Success(10))//Rate(30))//Lower Reflect rate. Player reflect rate is too high.
            {
                if (player.ContainReflect)
                {
                    Game.MsgServer.MsgSpellAnimation.SpellObj DmgObj = new Game.MsgServer.MsgSpellAnimation.SpellObj();
                    DmgObj.Damage = PhysicalAttack(player.Owner, Monster);
                    DmgObj.Damage /= 10;
                    if (DmgObj.Damage == 0)
                        DmgObj.Damage = 1;

                    InteractQuery action = new InteractQuery()
                    {
                        Damage = (int)DmgObj.Damage,
                        ResponseDamage = DmgObj.Damage,
                        AtkType = MsgAttackPacket.AttackID.Reflect,
                        X = player.X,
                        Y = player.Y,
                        OpponentUID = Monster.UID,
                        UID = player.UID

                    };
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();

                        Monster.Send(stream.InteractionCreate(&action));

                        Game.MsgServer.AttackHandler.ReceiveAttack.Monster.Execute(stream, DmgObj, player.Owner, Monster);
                    }
                    return true;


                }

                if (player.ActivateCounterKill)
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        if (player.ContainFlag(MsgServer.MsgUpdate.Flags.ShurikenVortex))
                            return false;
                        Database.MagicType.Magic DBSpell;
                        Game.MsgServer.MsgSpell ClientSpell;
                        if (player.Owner.MySpells.ClientSpells.TryGetValue((ushort)Role.Flags.SpellID.CounterKill, out ClientSpell))
                        {
                            Dictionary<ushort, Database.MagicType.Magic> DBSpells;
                            if (Database.Server.Magic.TryGetValue((ushort)Role.Flags.SpellID.CounterKill, out DBSpells))
                            {
                                if (DBSpells.TryGetValue(ClientSpell.Level, out DBSpell))
                                {
                                    Game.MsgServer.MsgSpellAnimation.SpellObj DmgObj = new Game.MsgServer.MsgSpellAnimation.SpellObj();
                                    Game.MsgServer.AttackHandler.Calculate.Physical.OnMonster(player, Monster, DBSpell, out DmgObj);

                                    //update spell
                                    if (ClientSpell.Level < DBSpells.Count - 1)
                                    {
                                        int SpecialRate = 1;
                                        if (player.Level < 120)
                                            SpecialRate = 100;
                                        ClientSpell.Experience += (int)(DmgObj.Damage * Program.ServerConfig.ExpRateSpell * SpecialRate);
                                        if (ClientSpell.Experience > DBSpells[ClientSpell.Level].Experience)
                                        {
                                            ClientSpell.Level++;
                                            ClientSpell.Experience = 0;
                                        }
                                        player.Send(stream.SpellCreate(ClientSpell));
                                    }

                                    InteractQuery action = new InteractQuery()
                                    {
                                        ResponseDamage = DmgObj.Damage,
                                        AtkType = MsgAttackPacket.AttackID.Scapegoat,
                                        X = player.X,
                                        Y = player.Y,
                                        OpponentUID = Monster.UID,
                                        UID = player.UID

                                    };

                                    Monster.Send(stream.InteractionCreate(&action));


                                    Game.MsgServer.AttackHandler.ReceiveAttack.Monster.Execute(stream, DmgObj, player.Owner, Monster, true);
                                }
                            }
                        }
                    }
                    return true;
                }
            }
            return false;
        }
        public uint MagicAttack(MonsterRole attacked, MonsterRole monster)
        {
           
            uint power = (uint)monster.Family.MaxAttack;
            if (power > attacked.Family.Defense)
                power -= attacked.Family.Defense;
            else power = 1;
            return power;
        }
        public uint MagicAttack(Client.GameClient client, MonsterRole monster)
        {
            if (!client.Socket.Alive)
                return 0;

            if (client.ProjectManager)
                return 1;
            if (client.Player.ContainFlag(MsgUpdate.Flags.ShurikenVortex))
                return 1;
            MsgServer.AttackHandler.CheckAttack.CheckGemEffects.CheckRespouseDamage(client);
            uint power = 0;
            if (monster.Name == "Guard2") 
                power = 200;
            else if (monster.Boss > 0)
            {
                power = (uint)Program.GetRandom.Next(monster.Family.MinAttack / 2, monster.Family.MaxAttack / 3);
            }
            else if (monster.Name == "AncientDevil")
            {
                power = (uint)Program.GetRandom.Next(monster.Family.MinAttack, monster.Family.MaxAttack);
            }
            else if (monster.Name == "UltimatePluto")
            {
                power = (uint)Program.GetRandom.Next(monster.Family.MinAttack, monster.Family.MaxAttack);
            }
            else if (monster.Family.MinAttack >= 900 && monster.Family.MaxAttack >= 900)
            {
                power = (uint)(monster.Family.MaxAttack / 10);
}
            else
            {
                power = (uint)monster.Family.MinAttack;//(uint)Program.GetRandom.Next(monster.Family.MinAttack, monster.Family.MaxAttack);
            }


            if (power > client.Status.MDefence)
                power -= client.Status.MDefence;
            else power = 1;

            if (power > client.Status.MagicDefence)
                power -= client.Status.MagicDefence;
            else power = 1;


            if (client.Player.Reborn == 1)
                power = (uint)Math.Round((double)(power * 0.7));
            else if (client.Player.Reborn == 2)
                power = (uint)Math.Round((double)(power * 0.5));

            power = DecreaseBless(power, (uint)client.Status.ItemBless);

            if (power > client.Status.MagicDamageDecrease)
                power -= client.Status.MagicDamageDecrease;
            else power = 1;

            power -= (power * 30) / 100;

            if (power == 0)
                power = 1;

            return power;
        }
        public uint PhysicalAttack(Client.GameClient client, MonsterRole monster)
        {
          
            if (!client.Socket.Alive)
                return 0;
            if (client.Player.ContainFlag(MsgUpdate.Flags.ShurikenVortex))
                return 1;
            if (client.ProjectManager)
                return 1;
            MsgServer.AttackHandler.CheckAttack.CheckGemEffects.CheckRespouseDamage(client);

            uint power = 0;
            if (monster.Family.MaxAttack > monster.Family.MinAttack)//look here we return number btw min attack and max attack of the monster
                power = (uint)SystemRandom.Next(monster.Family.MinAttack, monster.Family.MaxAttack);
            else
                power = (uint)SystemRandom.Next(monster.Family.MaxAttack, monster.Family.MinAttack);
            //
         //   power += (uint)(power * 50 / 100); //here we take the damage we have taken up and x 100 % then /100 if the number is 173 *100 25k /100 = 259 

            power = DecreaseBless(power, (uint)client.Status.ItemBless);

            if (power > client.AjustDefense)
                power -= client.AjustDefense;
            else
                power = 1;

            power -= (power * 30) / 100;

            if (power == 0)
                power = 1;

            return power;
        }
        public static uint PhysicalAttackPet(Client.GameClient client, MonsterRole monster)
        {

            if (!client.Socket.Alive)
                return 0;
            if (client.Player.ContainFlag(MsgUpdate.Flags.ShurikenVortex))
                return 1;
            //if (client.ProjectManager)
            //    return 1;
            MsgServer.AttackHandler.CheckAttack.CheckGemEffects.CheckRespouseDamage(client);

            uint power = 0;
            if (client.Pet.monster.Family.MaxAttack > client.Pet.monster.Family.MinAttack)
                power = (uint)Program.GetRandom.Next(client.Pet.monster.Family.MinAttack, client.Pet.monster.Family.MaxAttack);
            else
                power = (uint)Program.GetRandom.Next(client.Pet.monster.Family.MaxAttack, client.Pet.monster.Family.MinAttack);

            power += (uint)(power * Program.ServerConfig.PhysicalDamage / 100);

           // power = DecreaseBless(power, (uint)client.Status.ItemBless);

            if (power > monster.Family.Defense)
                power -= monster.Family.Defense;
            else
                power = 1;


            return power;
        }

        public static uint PhysicalAttackPet(Client.GameClient client, Client.GameClient target)
        {

            if (!client.Socket.Alive)
                return 0;
            if (client.Player.ContainFlag(MsgUpdate.Flags.ShurikenVortex))
                return 1;
            if (client.Pet == null) return 0;

            //if (client.ProjectManager)
            //    return 1;
            MsgServer.AttackHandler.CheckAttack.CheckGemEffects.CheckRespouseDamage(client);

            uint power = 0;
            if (client.Pet.monster.Family.MaxAttack > client.Pet.monster.Family.MinAttack)
                power = (uint)Program.GetRandom.Next(client.Pet.monster.Family.MinAttack, client.Pet.monster.Family.MaxAttack);
            else
                power = (uint)Program.GetRandom.Next(client.Pet.monster.Family.MaxAttack, client.Pet.monster.Family.MinAttack);

           // power += (uint)(power * Program.ServerConfig.PhysicalDamage / 100);

            power = new ActionHandler().DecreaseBless(power, (uint)target.Status.ItemBless);


            if (target.Player.Reborn == 1)
                power = (uint)Math.Round((double)(power * 0.7));
            else if (target.Player.Reborn == 2)
                power = (uint)Math.Round((double)(power * 0.5));

            if (power > target.Status.Defence)
                power -= target.Status.Defence;

            if (power > target.Status.MDefence)
                power -= target.Status.MDefence;

            if (power > 300)// 5las ya 3m mustafa msh lazm fzlaka 
            {
                power /= 3;
            }
            return  power ;// (uint)new Random().Next(100, 1000);
        }

        public uint DecreaseBless(uint Damage, uint bless)
        {
            uint power = Damage;
            power = (power * bless) / 100;
            power = Damage - power;
            return power;
        }
        public static Role.Flags.ConquerAngle GetAngle(ushort X, ushort Y, ushort X2, ushort Y2)
        {
            double direction = 0;

            double AddX = X2 - X;
            double AddY = Y2 - Y;
            double r = (double)Math.Atan2(AddY, AddX);

            if (r < 0) r += (double)Math.PI * 2;

            direction = 360 - (r * 180 / (double)Math.PI);

            byte Dir = (byte)((7 - (Math.Floor(direction) / 45 % 8)) - 1 % 8);
            return (Role.Flags.ConquerAngle)(byte)((int)Dir % 8);
        }
        public static void IncXY(Role.Flags.ConquerAngle Facing, ref ushort x, ref ushort y)
        {
            sbyte xi, yi;
            xi = yi = 0;
            switch (Facing)
            {
                case Role.Flags.ConquerAngle.North: xi = -1; yi = -1; break;
                case Role.Flags.ConquerAngle.South: xi = 1; yi = 1; break;
                case Role.Flags.ConquerAngle.East: xi = 1; yi = -1; break;
                case Role.Flags.ConquerAngle.West: xi = -1; yi = 1; break;
                case Role.Flags.ConquerAngle.NorthWest: xi = -1; break;
                case Role.Flags.ConquerAngle.SouthWest: yi = 1; break;
                case Role.Flags.ConquerAngle.NorthEast: yi = -1; break;
                case Role.Flags.ConquerAngle.SouthEast: xi = 1; break;
            }
            x = (ushort)(x + xi);
            y = (ushort)(y + yi);
        }
    }
}
