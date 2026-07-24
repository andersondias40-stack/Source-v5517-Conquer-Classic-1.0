//using COServer.Database;
//using COServer.Game.MsgServer;
//using COServer.Role;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace COServer.Game
//{

//    public class AutoHunting2
//    {
//        private static Random RobotRandom = new Random();
//        private static ushort[] SkillRobotTrojan = new ushort[] { 1045, 1046, 1115 };
//        private static ushort[] SkillRobotArcher = new ushort[] { 8001 };
//        private static ushort[] SkillRobotNinja = new ushort[] { 6000 };
//        private static ushort[] SkillRobotMonk = new ushort[] { 10381, 10415 };
//        private static ushort[] SkillRobotWater = new ushort[] { 1000 };
//        private static ushort[] SkillRobotFire = new ushort[] { 1002 };
//        private static ushort[] SkillRobotAttacked = new ushort[] { 6000, 10381, 10415, 1000, 1002 };
//        private static ushort[] SkillXPRobot = new ushort[] { 1110 };

//        public static unsafe void JumpRobot(Client.GameClient client)
//        {
//            try
//            {
//                if (client != null && client.Player.Robot && client.Map != null && client.Player.View != null && client.Player != null && client.Player.HitPoints > 0)
//                {
//                    if (DateTime.Now >= client.Player.RobotAttack.AddMilliseconds(10000))
//                    {
//                        int Count = client.Player.View.Roles(Role.MapObjectType.Monster).Count();
//                        if (Count > 0)
//                        {
//                            if (!client.Player.ContainFlag(MsgUpdate.Flags.FatalStrike))
//                            {
//                                var cordes = client.Map.RandomJump(client.Player.X, client.Player.Y, 16);
//                                ushort X = cordes.Item1, Y = cordes.Item2;
                                

//                                using (var rec = new ServerSockets.RecycledPacket())
//                                {
//                                    var stream = rec.GetStream();
//                                    InterActionWalk inter = new InterActionWalk()
//                                    {
//                                        Mode = MsgInterAction.Action.Jump,
//                                        X = X,
//                                        Y = Y,
//                                        UID = client.Player.UID,
//                                    };
//                                    client.Send(stream.InterActionWalk(&inter));
//                                    client.Player.DirectionChange = 0;
//                                    client.Player.LastMove = DateTime.Now;
//                                    client.Player.RobotAttack = DateTime.Now;
//                                }
//                            }
//                        }
//                        else
//                        {
//                            var cordes = client.Map.RandomJump(client.Player.X, client.Player.Y, 16);
//                            ushort X = cordes.Item1, Y = cordes.Item2;

//                            using (var rec = new ServerSockets.RecycledPacket())
//                            {
//                                var stream = rec.GetStream();
//                                InterActionWalk inter = new InterActionWalk()
//                                {
//                                    Mode = MsgInterAction.Action.Jump,
//                                    X = X,
//                                    Y = Y,
//                                    UID = client.Player.UID,
//                                };
//                                client.Send(stream.InterActionWalk(&inter));
//                                client.Player.LastMove = DateTime.Now;
//                            }
//                            return;

//                            if (client.Player.DirectionChange > 10)
//                            {
//                                foreach (var Obj in client.Map.View.GetAllMapRoles(Role.MapObjectType.Monster))
//                                {
//                                    var entity = Obj as MsgMonster.MonsterRole;
//                                    if (entity.HitPoints > 0 && !entity.Name.Contains("Guard"))
//                                    {
//                                        MsgServer.AttackHandler.Algoritms.InLineAlgorithm Line = new MsgServer.AttackHandler.Algoritms.InLineAlgorithm(client.Player.X, Obj.X, client.Player.Y, Obj.Y, client.Map, 15, 0);

//                                        X = (ushort)Line.lcoords[(int)(Line.lcoords.Count() - 1)].X; Y = (ushort)Line.lcoords[(int)(Line.lcoords.Count() - 1)].Y;
//                                        if (client.Map.AddGroundItem(ref X, ref Y))
//                                        {
//                                            using (var rec = new ServerSockets.RecycledPacket())
//                                            {
//                                                var stream = rec.GetStream();
//                                                InterActionWalk inter = new InterActionWalk()
//                                                {
//                                                    Mode = MsgInterAction.Action.Jump,
//                                                    X = X,
//                                                    Y = Y,
//                                                    UID = client.Player.UID,
//                                                };
//                                                client.Send(stream.InterActionWalk(&inter));
//                                                client.Player.LastMove = DateTime.Now;
//                                            }
//                                            return;
//                                        }
//                                    }
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//            catch (Exception e)
//            {
//                Console.WriteException(e);
//            }
//        }
//        public static unsafe void SkillRobot(Client.GameClient client)
//        {
//            try
//            {
//                if (client != null && client.Player.Robot && client.Map != null && client.Player.View != null && client.Player != null && client.Player.HitPoints > 0)
//                {
//                    foreach (Role.IMapObj Obj in client.Player.View.Roles(Role.MapObjectType.Monster))
//                    {
//                        if (Role.Core.GetDistance(Obj.X, Obj.Y, client.Player.X, client.Player.Y) > 8) continue;
//                        var entity = Obj as MsgMonster.MonsterRole;
//                        if (entity.HitPoints > 0 && !entity.ContainFlag(MsgUpdate.Flags.Ghost) && !entity.Name.Contains("Guard"))
//                        {
//                            ushort SpellID = 0;
//                            if (client.Player.Class >= 10 && client.Player.Class <= 15) SpellID = SkillRobotTrojan[RobotRandom.Next(SkillRobotTrojan.Length)];
//                            if (client.Player.Class >= 40 && client.Player.Class <= 45) SpellID = SkillRobotArcher[RobotRandom.Next(SkillRobotArcher.Length)];
//                            if (client.Player.Class >= 50 && client.Player.Class <= 55) SpellID = SkillRobotNinja[RobotRandom.Next(SkillRobotNinja.Length)];
//                            if (client.Player.Class >= 60 && client.Player.Class <= 65) SpellID = SkillRobotMonk[RobotRandom.Next(SkillRobotMonk.Length)];
//                            if (client.Player.Class >= 130 && client.Player.Class <= 135) SpellID = SkillRobotWater[RobotRandom.Next(SkillRobotWater.Length)];
//                            if (client.Player.Class >= 140 && client.Player.Class <= 145) SpellID = SkillRobotFire[RobotRandom.Next(SkillRobotFire.Length)];

//                            if (!client.Player.ContainFlag(MsgUpdate.Flags.Cyclone) && !client.Player.ContainFlag(MsgUpdate.Flags.FatalStrike) && client.Player.ContainFlag(MsgUpdate.Flags.XPList))
//                            {
//                                List<ushort> SkillsXP = new List<ushort>();
//                                ushort SkillXP = 0;
//                                for (int i = 0; i < SkillXPRobot.Length; i++)
//                                {
//                                    if (client.MySpells.ClientSpells.ContainsKey(SkillXPRobot[i]))
//                                        SkillsXP.Add(SkillXPRobot[i]);
//                                }
//                                if (SkillsXP.Count > 0)
//                                {
//                                    SkillXP = SkillsXP[(ushort)RobotRandom.Next(SkillsXP.Count)];
//                                    if (SkillXP != 0)
//                                    {
//                                        using (var rec = new ServerSockets.RecycledPacket())
//                                        {
//                                            var stream = rec.GetStream();

//                                            InteractQuery action = new InteractQuery()
//                                            {
//                                                AtkType = MsgAttackPacket.AttackID.Magic,
//                                                UID = client.Player.UID,
//                                                OpponentUID = client.Player.UID,
//                                                X = client.Player.X,
//                                                Y = client.Player.Y,
//                                                Damage = (int)SkillXP
//                                            };
//                                            MsgAttackPacket.Process(client, action);
//                                        }
//                                    }
//                                }
//                            }
//                            if (!client.Player.ContainFlag(MsgUpdate.Flags.FatalStrike))//here
//                            {
//                                Dictionary<ushort, Database.MagicType.Magic> Spells;
//                                if (Database.Server.Magic.TryGetValue(SpellID, out Spells))
//                                {
//                                    MsgSpell ClientSpell;
//                                    if (client.MySpells.ClientSpells.TryGetValue(SpellID, out ClientSpell))
//                                    {
//                                        Database.MagicType.Magic spell;
//                                        if (Spells.TryGetValue(ClientSpell.Level, out spell))
//                                        {
//                                            if (SpellID != 0 && spell != null && spell.UseStamina <= client.Player.Stamina && spell.UseMana <= client.Player.Mana)
//                                            {
//                                                if (client.Player.Rate(50))
//                                                {
//                                                    if (!(client.Player.RobotX == client.Player.X && client.Player.RobotY == client.Player.Y) || client.Player.RobotX == 0 && client.Player.RobotY == 0)
//                                                    {
//                                                        client.Player.RobotX = client.Player.X;
//                                                        client.Player.RobotY = client.Player.Y;
//                                                        client.Player.RobotAttack = DateTime.Now;
//                                                    }

//                                                    using (var rec = new ServerSockets.RecycledPacket())
//                                                    {
//                                                        var stream = rec.GetStream();
//                                                        InteractQuery action = new InteractQuery();
//                                                        action.AtkType = MsgAttackPacket.AttackID.Magic;
//                                                        action.UID = client.Player.UID;
//                                                        if (SkillRobotAttacked.Contains(SpellID))
//                                                            action.OpponentUID = Obj.UID;
//                                                        action.X = Obj.X;
//                                                        action.Y = Obj.Y;
//                                                        action.Damage = (int)SpellID;
//                                                        action.SpellID = (ushort)SpellID;
//                                                        MsgAttackPacket.Process(client, action);
//                                                    }
//                                                    return;
//                                                }
//                                            }
//                                        }
//                                    }
//                                }
//                            }
//                            if (Role.Core.GetDistance(Obj.X, Obj.Y, client.Player.X, client.Player.Y) <= 2 || client.Player.ContainFlag(MsgUpdate.Flags.FatalStrike))
//                            {
//                                if (client.Player.Class != 135 && client.Player.Class != 145)
//                                {
//                                    if (!(client.Player.RobotX == client.Player.X && client.Player.RobotY == client.Player.Y) || client.Player.RobotX == 0 && client.Player.RobotY == 0)
//                                    {
//                                        client.Player.RobotX = client.Player.X;
//                                        client.Player.RobotY = client.Player.Y;
//                                        client.Player.RobotAttack = DateTime.Now;
//                                    }
//                                    using (var rec = new ServerSockets.RecycledPacket())
//                                    {
//                                        var stream = rec.GetStream();
//                                        InteractQuery action = new InteractQuery();
//                                        action.AtkType = MsgAttackPacket.AttackID.Physical;
//                                        if (client.Player.Class == 45)
//                                            action.AtkType = MsgAttackPacket.AttackID.Archer;
//                                        action.UID = client.Player.UID;
//                                        action.OpponentUID = Obj.UID;
//                                        action.X = Obj.X;
//                                        action.Y = Obj.Y;
//                                        MsgAttackPacket.Process(client, action);
//                                    }
//                                    return;
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//            catch (Exception e)
//            {
//                Console.WriteException(e);
//            }
//        }
//        public static unsafe void ReviveRobot(Client.GameClient client)
//        {
//            try
//            {
//                if (client != null && client.Map != null && client.Player.View != null && client.Player != null)
//                {
//                    #region Revive
//                    if (client.Player.ContainFlag(MsgUpdate.Flags.Ghost) && Time32.Now > client.Player.DeadStamp.AddSeconds(20))
//                    {
//                        client.Player.Action = Flags.ConquerAction.None;
//                        client.Player.TransformationID = 0;
//                        client.Player.RemoveFlag(MsgUpdate.Flags.Dead);
//                        client.Player.RemoveFlag(MsgUpdate.Flags.Ghost);
//                        client.Player.HitPoints = (int)client.Status.MaxHitpoints;
//                    }
//                    #endregion
//                    if (client.Player.HitPoints > 0)
//                    {
//                        #region Hitpoints
//                        if (!client.Player.ContainFlag(MsgUpdate.Flags.Ghost) && client.Player.HitPoints < client.Status.MaxHitpoints)
//                            client.Player.HitPoints = Math.Min(client.Player.HitPoints + 3000, (int)client.Status.MaxHitpoints);
//                        #endregion
//                        #region Mana
//                        if (!client.Player.ContainFlag(MsgUpdate.Flags.Ghost) && client.Player.Mana < client.Status.MaxMana)
//                            client.Player.Mana = (ushort)Math.Min(client.Player.Mana + 3000, client.Status.MaxMana);
//                        #endregion
//                    }
//                }
//            }
//            catch (Exception e)
//            {
//                Console.WriteException(e);
//            }
//        }
//    }
//}
