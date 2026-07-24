using COServer.Game.MsgFloorItem;
using COServer.Game.MsgMonster;
using COServer.Game.MsgServer;
using COServer.Role;
using System;
using System.Generic;
using System.Linq;
using System.Threading;
using static COServer.Role.Flags;

namespace COServer.Client
{
    public class PoolProcesses
    {
        private static bool Valid(GameClient client)
        {
            if (!client.Socket.Alive || client.Player == null
                || client == null || client.Player.View == null || !client.FullLoading)
            {
                return false;
            }
            return true;
        }
        public static unsafe void AutoHuntThread(Client.GameClient client)
        {
            if (!client.FullLoading)
                return;
            if (!Valid(client))
                return;

            if (client.OnAutoHunt)
            {
                var Now = Time32.Now;


            }
        }
        public static unsafe void AiThread(Client.GameClient client)
        {
            if (!client.FullLoading)
                return;
            if (!Valid(client))
                return;
            var Now = Time32.Now;

            if (client.Pet != null && client.Pet.monster != null)
            {
                short distance = Core.GetDistance(client.Pet.monster.X, client.Pet.monster.Y, client.Player.X, client.Player.Y);
                if (distance >= 8)
                {
                    ushort X = (ushort)(client.Player.X + Program.GetRandom.Next(2, 5));
                    ushort Y = (ushort)(client.Player.Y + Program.GetRandom.Next(2, 5));
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
                        ObjId = client.Pet.monster.UID

                    };

                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        if (stream != null)
                        {
                            client.Player.View.SendView(stream.ActionCreate(&data), true);
                            client.Pet.monster.Facing = Role.Core.GetAngle(client.Pet.monster.X, client.Pet.monster.Y, X, Y);
                            client.Pet.monster.Action = Role.Flags.ConquerAction.Jump;
                            client.Map.View.MoveTo<Role.IMapObj>(client.Pet.monster, X, Y);
                            client.Pet.monster.X = X;
                            client.Pet.monster.Y = Y;
                            client.Pet.monster.UpdateMonsterView(null, stream);
                        }
                    }
                }
                else if (distance > 4)
                {
                    var facing = Role.Core.GetAngle(client.Pet.monster.X, client.Pet.monster.Y, client.Player.X, client.Player.Y);
                    if (!client.Pet.Move(facing))
                    {
                        var x = client.Pet.monster.X;
                        var y = client.Pet.monster.Y;
                        facing = (Flags.ConquerAngle)Program.GetRandom.Next(7);
                        if (client.Pet.Move(facing))
                        {
                            client.Pet.monster.Facing = facing;
                            var move = new WalkQuery()
                            {
                                Direction = (uint)facing,
                                UID = client.Pet.monster.UID,
                                Running = 1
                            };

                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if (stream != null)
                                {
                                    client.Player.View.SendView(stream.MovementCreate(&move), true);
                                    client.Map.View.MoveTo<Role.IMapObj>(client.Pet.monster, x, y);
                                    client.Pet.monster.X = x;
                                    client.Pet.monster.Y = y;
                                    client.Pet.monster.Facing = facing;

                                    client.Player.View.Role(false, stream.MovementCreate(&move));
                                    client.Pet.monster.UpdateMonsterView(null, stream);
                                }
                            }
                        }
                    }
                    else
                    {
                        client.Pet.monster.Facing = facing;
                        var x = client.Pet.monster.X;
                        var y = client.Pet.monster.Y;
                        var move = new WalkQuery()
                        {
                            Direction = (uint)facing,
                            UID = client.Pet.monster.UID,
                            Running = 1
                        };

                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            if (stream != null)
                            {
                                client.Player.View.SendView(stream.MovementCreate(&move), true);
                                client.Map.View.MoveTo<Role.IMapObj>(client.Pet.monster, x, y);
                                client.Pet.monster.X = x;
                                client.Pet.monster.Y = y;
                                client.Pet.monster.Facing = facing;

                                client.Player.View.Role(false, stream.MovementCreate(&move));
                                client.Pet.monster.UpdateMonsterView(null, stream);
                            }
                        }
                    }
                }
                else
                {
                    var monster = client.Pet;
                    if (monster.Target != null && monster.Target.UID != client.Player.UID)
                    {

                        short dis = MonsterView.GetDistance(monster.monster.X, monster.monster.Y, monster.Target.X, monster.Target.Y);
                        if (dis <= 13)
                        {
                            if (monster.Target.Alive)
                            {
                                using (var rec = new ServerSockets.RecycledPacket())
                                {
                                    var stream = rec.GetStream();

                                    var SpellPacket = new MsgSpellAnimation(monster.monster.UID, 0, monster.Target.X, monster.Target.Y, (ushort)monster.Family.SpellId, 0, 0);
                                    uint Damage = 0;
                                    if (monster.Target is MonsterRole)
                                    {
                                        var tar = monster.Target as MonsterRole;
                                        Damage = ActionHandler.PhysicalAttackPet(monster.Owner, tar);
                                        Game.MsgServer.AttackHandler.ReceiveAttack.Monster.ExecutePet(stream, (uint)Damage, monster.Owner, tar);
                                        Game.MsgServer.AttackHandler.Updates.IncreaseExperience.Up(stream, monster.Owner, Damage);

                                    }
                                    else if (monster.Target is Player)
                                    {
                                        var tar = monster.Target as Player;
                                        Damage = ActionHandler.PhysicalAttackPet(monster.Owner, tar.Owner);
                                        Game.MsgServer.AttackHandler.ReceiveAttack.Player.ExecutePet((int)Damage, monster.Owner, tar);
                                    }
                                    else if (monster.Target is SobNpc)
                                    {
                                        var tar = monster.Target as SobNpc;
                                        Damage = (uint)monster.monster.Family.MaxAttack * 2;
                                        Game.MsgServer.AttackHandler.ReceiveAttack.Npc.Execute(stream, new MsgSpellAnimation.SpellObj(monster.Target.UID, Damage), monster.Owner, tar);
                                    }
                                    SpellPacket.Targets.Enqueue(new MsgSpellAnimation.SpellObj(monster.Target.UID, Damage));

                                    SpellPacket.SetStream(stream);
                                    SpellPacket.Send(monster.Owner);
                                    SpellPacket.Send(monster.monster);
                                }
                            }
                            else monster.monster.Target = null;
                        }
                        //else monster.monster.Target = null;
                    }
                    //else
                    //{
                    //    var array = client.Player.View.Roles(MapObjectType.Monster);
                    //    //if (array != null)
                    //    //    foreach (var obj in array)
                    //    //    {
                    //    //        var target = obj as MonsterRole;
                    //    //        short dis = MonsterView.GetDistance(monster.monster.X, monster.monster.Y, target.X, target.Y);
                    //    //        if (dis <= 13)
                    //    //        {
                    //    //            if (target.Alive)
                    //    //            {
                    //    //                using (var rec = new ServerSockets.RecycledPacket())
                    //    //                {
                    //    //                    var stream = rec.GetStream();

                    //    //                    var SpellPacket = new MsgSpellAnimation(monster.monster.UID, 0, target.X, target.Y, (ushort)monster.Family.SpellId, 0, 0);
                    //    //                    uint Damage = ActionHandler.PhysicalAttackPet(monster.Owner, target);
                    //    //                    SpellPacket.Targets.Enqueue(new MsgSpellAnimation.SpellObj(target.UID, Damage));
                    //    //                    Game.MsgServer.AttackHandler.ReceiveAttack.Monster.ExecutePet(stream, (uint)Damage, monster.Owner, target);
                    //    //                    SpellPacket.SetStream(stream);
                    //    //                    SpellPacket.Send(monster.Owner);
                    //    //                    SpellPacket.Send(monster.monster);
                    //    //                    break;
                    //    //                }
                    //    //            }
                    //    //            else monster.monster.Target = null;
                    //    //        }
                    //    //    }
                    //    //else
                    //    {
                    //        var npcarray = client.Player.View.Roles(MapObjectType.SobNpc);
                    //        if (array != null)
                    //            foreach (var obj in npcarray)
                    //            {
                    //                var npc = obj as SobNpc;
                    //                short dis = MonsterView.GetDistance(monster.monster.X, monster.monster.Y, npc.X, npc.Y);
                    //                if (dis <= 13)
                    //                {
                    //                    if (npc.Alive)
                    //                    {
                    //                        using (var rec = new ServerSockets.RecycledPacket())
                    //                        {
                    //                            var stream = rec.GetStream();

                    //                            var SpellPacket = new MsgSpellAnimation(monster.monster.UID, 0, npc.X, npc.Y, (ushort)monster.Family.SpellId, 0, 0);
                    //                            uint Damage = (uint)monster.monster.Family.MaxAttack;
                    //                            MsgSpellAnimation.SpellObj spellObj = new MsgSpellAnimation.SpellObj(npc.UID, Damage);
                    //                            SpellPacket.Targets.Enqueue(spellObj);
                    //                            Game.MsgServer.AttackHandler.ReceiveAttack.Npc.Execute(stream, spellObj, monster.Owner, npc);
                    //                            SpellPacket.SetStream(stream);
                    //                            SpellPacket.Send(monster.Owner);
                    //                            SpellPacket.Send(monster.monster);
                    //                            break;
                    //                        }
                    //                    }
                    //                    else monster.monster.Target = null;
                    //                }
                    //            }
                    //    }
                    //}
                }

            }
        }
        public static unsafe void CheckItemTime(Client.GameClient client)
        {
            //return;
            try
            {
                if (!client.FullLoading)
                    return;
                if (!Valid(client))
                    return;
                if (client.Player.PlayerHasItemTime)
                {
                    var Now = Time32.Now;

                    foreach (var item in client.AllMyTimeItems())
                    {
                        if (DateTime.Now < item.EndDate)
                            continue;
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            if (client.Inventory.ClientItems.ContainsKey(item.UID))
                            {
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                continue;
                            }
                            if (client.Equipment.ClientItems.ContainsKey(item.UID))
                            {
                                Role.Flags.ConquerItem position = (Role.Flags.ConquerItem)Database.ItemType.ItemPosition(item.ITEM_ID);
                                client.Equipment.Remove(position, stream);
                                client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                client.Equipment.QueryEquipment();
                            }
                            foreach (var Wh in client.Warehouse.ClientItems)
                            {
                                foreach (var item2 in Wh.Value.Values)
                                {
                                    if (item2.UID == item.UID)
                                    {
                                        client.Warehouse.RemoveItem(item2.UID, Wh.Key, stream);
                                        client.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream);
                                    }
                                }
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteException(ex);
            }
        }
        public static unsafe void CheckSecond(Client.GameClient client)
        {

            //if (!client.FullLoading)
            //    return;
            if (!Valid(client))
                return;
            var Timer = Time32.Now;
            //  DateTime timer = DateTime.Now;


            #region Check Seconds
            if (client.Player.Map == 601)
            {
                if (client.Map != null)
                    if (!client.Map.ValidLocation(client.Player.X, client.Player.Y))
                    {
                        client.Teleport(64, 56, 601);
                    }
            }
            if (client.Player.VipLevel == 6 && !client.Player.ContainFlag(MsgUpdate.Flags.TopNinja))
            {
                client.Player.AddFlag(MsgUpdate.Flags.TopNinja, Role.StatusFlagsBigVector32.PermanentFlag, false);
            }
            else if (client.Player.VipLevel < 6 && client.Player.ContainFlag(MsgUpdate.Flags.TopPirate))
            {
                client.Player.AddFlag(MsgUpdate.Flags.TopPirate, Role.StatusFlagsBigVector32.PermanentFlag, false);
            }
            if (client.Player.Merchant == 1 && client.Player.MerchantApplicationEnd <= DateTime.Now)
            {
                client.Player.Merchant = 255;
            }
            if (DateTime.Now > client.Player.ExpireVip)
            {
                if (client.Player.VipLevel > 1)
                {
                    client.Player.VipLevel = 0;
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        client.Player.SendUpdate(stream, client.Player.VipLevel, Game.MsgServer.MsgUpdate.DataType.VIPLevel);

                        client.Player.UpdateVip(stream);
                    }
                }
            }
            if (client.Player.ExpProtection > 0)
                client.Player.ExpProtection -= 1;

            Database.VoteSystem.CheckUp(client);

            if (client.Player.OnDefensePotion)
            {
                if (Timer > client.Player.OnDefensePotionStamp)
                {
                    client.Player.OnDefensePotion = false;
                }
            }
            if (client.Player.OnAttackPotion)
            {
                if (Timer > client.Player.OnAttackPotionStamp)
                {
                    client.Player.OnAttackPotion = false;
                }
            }
            if (client.Player.X == 0 || client.Player.Y == 0)
            {
                client.Teleport(428, 378, 1002);
            }
            if (client.Player.DExpTime > 0)
            {
                client.Player.DExpTime -= 1;
                if (client.Player.DExpTime == 0)
                    client.Player.RateExp = 1;
            }

            #endregion
            if (client.Player.HeavenBlessing > 0)
            {
                if (client.Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.HeavenBlessing))
                {
                    if (Timer > client.Player.HeavenBlessTime)
                    {
                        client.Player.RemoveFlag(Game.MsgServer.MsgUpdate.Flags.HeavenBlessing);
                        client.Player.HeavenBlessing = 0;
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            client.Player.SendUpdate(stream, 0, Game.MsgServer.MsgUpdate.DataType.HeavensBlessing);
                            client.Player.SendUpdate(stream, Game.MsgServer.MsgUpdate.OnlineTraining.Remove, Game.MsgServer.MsgUpdate.DataType.OnlineTraining);

                            client.Player.Stamina = (ushort)Math.Min((int)client.Player.Stamina, 100);
                            client.Player.SendUpdate(stream, client.Player.Stamina, Game.MsgServer.MsgUpdate.DataType.Stamina);
                        }
                    }

                    if (client.Player.Map != 601 && client.Player.Map != 1039)
                    {
                        if (Timer > client.Player.ReceivePointsOnlineTraining)
                        {
                            client.Player.ReceivePointsOnlineTraining = Timer.AddMinutes(1);
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                client.Player.SendUpdate(stream, Game.MsgServer.MsgUpdate.OnlineTraining.IncreasePoints, Game.MsgServer.MsgUpdate.DataType.OnlineTraining);//+10
                            }
                        }
                        if (Timer > client.Player.OnlineTrainingTime)
                        {
                            client.Player.OnlineTrainingPoints += 100000;
                            client.Player.OnlineTrainingTime = Timer.AddMinutes(10);
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                client.Player.SendUpdate(stream, Game.MsgServer.MsgUpdate.OnlineTraining.ReceiveExperience, Game.MsgServer.MsgUpdate.DataType.OnlineTraining);
                            }
                        }
                    }
                }
            }

        }
        public static unsafe void XPCounter(Client.GameClient client)
        {

            if (!Valid(client))
                return;
            var Timer = Time32.Now;

            if (client.Player.PKPoints > 0)
            {
                if (Timer > client.Player.PkPointsStamp.AddMinutes(6))
                {
                    client.Player.PKPoints -= 1;
                    client.Player.PkPointsStamp = Time32.Now;
                }
            }
            if (Timer > client.Player.XPListStamp.AddSeconds(4) && client.Player.Alive)
            {
                client.Player.XPListStamp = Timer.AddSeconds(4);
                if (!client.Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.XPList))
                {
                    client.Player.XPCount++;
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();

                        client.Player.SendUpdate(stream, client.Player.XPCount, MsgUpdate.DataType.XPCircle);
                        if (client.Player.XPCount >= 100)
                        {
                            client.Player.XPCount = 0;
                            client.Player.AddFlag(Game.MsgServer.MsgUpdate.Flags.XPList, 20, true);
                        }
                    }
                }
            }

            if (client.Player.InUseIntensify)
            {
                if (client.Player.IntensifyActive.AddMilliseconds(5300) < DateTime.Now)
                {
                    if (!client.Player.ContainFlag(MsgUpdate.Flags.Intensify))
                        client.Player.AddSpellFlag(MsgUpdate.Flags.Intensify, 20, true);
                }
                if (Timer > client.Player.IntensifyStamp.AddSeconds(15))
                {
                    if (!client.Player.Intensify)
                    {
                        if (client.Player.ContainFlag(MsgUpdate.Flags.Intensify))
                            client.Player.RemoveFlag(MsgUpdate.Flags.Intensify);
                        client.Player.Intensify = true;
                        client.Player.InUseIntensify = false;
                    }
                }
            }
        }
        public static unsafe void CheckItems(Client.GameClient client)
        {
            try
            {
                //if (!client.FullLoading)
                //    return;
                if (!Valid(client))
                    return;
                var Now = Time32.Now;

                //using (var rec = new ServerSockets.RecycledPacket())
                //{
                //    var stream = rec.GetStream();
                //    client._ServersidePing = Now;
                //    client.Send(stream.MsgTickCreate(client));
                //    client.WaitingPing = true;
                //}
                /*if (!client.Fake)
                {
                    if (Now > client.JumpStamp)
                    {
                        client.Player.dummyX = client.Player.dummyX2 = client.Player.X;
                        client.Player.dummyY = client.Player.dummyY2 = client.Player.Y;
                        client.JumpStamp.Value = Now.Value + 700;// JUMP;
                    }

                }*/

                foreach (var item in client.Player.View.Roles(Role.MapObjectType.Item))
                {
                    if (item.Alive == false)
                    {
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            var PItem = item as Game.MsgFloorItem.MsgItem;
                            if (PItem.IsTrap())
                            {

                                //if (PItem.ItemBase.ITEM_ID == Game.MsgFloorItem.MsgItemPacket.DBShowerEffect)
                                //{
                                //    if (Game.MsgTournaments.MsgSchedules.CurrentTournament.Type == Game.MsgTournaments.TournamentType.DBShower)
                                //    {
                                //        var tournament = Game.MsgTournaments.MsgSchedules.CurrentTournament as MsgDBShower;
                                //        tournament.DropDragonBall(PItem.X, PItem.Y, stream);
                                //    }
                                //}
                                PItem.SendAll(stream, MsgDropID.RemoveEffect);
                            }
                            else
                                PItem.SendAll(stream, MsgDropID.Remove);
                            client.Map.View.LeaveMap<Role.IMapObj>(PItem);
                        }
                    }
                    else if (item.IsTrap())
                    {
                        var FloorItem = item as Game.MsgFloorItem.MsgItem;
                        if (FloorItem.ItemBase == null)
                            continue;
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteException(e);
            }

        }
        public static unsafe void AutoAttackCallback(Client.GameClient client)
        {
            try
            {
                //if (!client.FullLoading)
                //    return;
                if (!Valid(client))
                    return;
                var timer = Time32.Now;

                if (client.Player.ContainFlag(MsgUpdate.Flags.Dead) && !client.Player.ContainFlag(MsgUpdate.Flags.Ghost))//client.Player.Alive == false && client.Player.CompleteLogin)
                {
                    if (client.Player.Alive == false && client.Player.CompleteLogin)
                    {
                        if (timer > client.Player.GhostStamp.AddMilliseconds(500))
                        {
                            client.Player.AddFlag(Game.MsgServer.MsgUpdate.Flags.Ghost, Role.StatusFlagsBigVector32.PermanentFlag, true);
                            if (client.Player.Body % 10 < 3)
                                client.Player.TransformationID = 99;
                            else
                                client.Player.TransformationID = 98;
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                client.Send(stream.MapStatusCreate(client.Player.Map, client.Map.ID, (uint)client.Map.TypeStatus));
                            }
                        }
                    }
                }
                if (client.OnAutoAttack && client.Player.Alive)
                {
                    if (client.Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Dizzy))
                    {
                        client.OnAutoAttack = false;
                        return;
                    }

                    InteractQuery action = new InteractQuery();
                    action = InteractQuery.ShallowCopy(client.AutoAttack);
                    if (action.SpellID >= 1000 && action.SpellID <= 1002 && action.AtkType == MsgAttackPacket.AttackID.Magic)
                    {
                        if (client.Player.Mana < 100)
                        {
                            client.OnAutoAttack = false;
                            return;
                        }
                    }
                    if (action.SpellID == 1005 && action.OpponentUID < 1000000)
                    {
                        client.OnAutoAttack = false;
                        return;
                    }
                    MsgAttackPacket.Process(client, action);
                }
            }
            catch (Exception e)
            {
                Console.WriteException(e);
            }

        }
        public unsafe static void StaminaCallback(Client.GameClient client)
        {
            if (!Valid(client))
                return;
            var Timer = Time32.Now;
            if (client.Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Fly))
                return;
            if (client.Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Ghost))
                return;

            byte MaxStamina = (byte)(client.Player.HeavenBlessing > 0 ? 150 : 100);

            if (client.Player.Stamina < MaxStamina)
            {
                // Obtém a quantidade de stamina adicional que o jogador pode receber
                ushort addstamin = client.Player.GetAddStamina();

                // Se a stamina atual é 0, não permita que a stamina seja setada para o máximo
                if (client.Player.Stamina == 0 && addstamin >= MaxStamina)
                {
                    // Opcional: Mantenha a stamina em 0 ou ajuste para um valor razoável
                    // client.Player.Stamina = 1; // Exemplo para evitar ir para 0
                }
                else
                {
                    // Atualiza a stamina do jogador, garantindo que não exceda a stamina máxima
                    client.Player.Stamina = (ushort)Math.Min(client.Player.Stamina + addstamin, MaxStamina);
                }

                // Cria um pacote reciclado para enviar a atualização da stamina ao cliente
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    // Obtém o stream do pacote reciclado
                    var stream = rec.GetStream();

                    // Envia a atualização da stamina para o jogador
                    client.Player.SendUpdate(stream, client.Player.Stamina, Game.MsgServer.MsgUpdate.DataType.Stamina);
                }
            }
        }
        public unsafe static void CharacterCallback(Client.GameClient client)
        {
            try
            {
                if (!client.FullLoading)
                    return;
                if (!Valid(client))
                    return;
                var Timer = Time32.Now;
                DateTime Now64 = DateTime.Now;
                #region GuildBeast
                if (Now64.Hour == 20 && Now64.Minute == 00 && client.Player.SpawnGuildBeast)
                {
                    var Map = Database.Server.ServerMaps[1038];

                    if (!Map.ContainMobID(3120))
                    {
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Warning! The GuildBeast has appeared in the GuildCastle! Everyone shall gather their weapons and fight it!", "ALLUSERS", "Server", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                            Database.Server.AddMapMonster(stream, Map, 3120, 87, 77, 1, 1, 1);
                        }
                    }
                }
                else if (Now64.Hour == 21 && Now64.Minute == 00 && client.Player.SpawnGuildBeast)
                {
                    var Map = Database.Server.ServerMaps[1038];
                    if (Map.GetMobLoc(3120) != null)
                    {
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();

                            var Array = Map.View.GetAllMapRoles(Role.MapObjectType.Monster);
                            foreach (var map_mob in Array)
                            {
                                if (map_mob.Map == 1038 && map_mob.Alive)
                                {
                                    var Monster = (map_mob as Game.MsgMonster.MonsterRole);
                                    if (Monster.Name == "GuildBeast")
                                    {
                                        Monster.Dead(stream, client, 3120, Monster.GMap);
                                        //ActionQuery action = new ActionQuery()
                                        //{
                                        //    ObjId = Monster.UID,
                                        //    Type = ActionType.RemoveEntity
                                        //};

                                        //Monster.Send(stream.ActionCreate(&action));
                                        client.Player.SpawnGuildBeast = false;
                                        client.Player.GuildBeastClaimd = true;// can guild leader claim it
                                        //  Monster.Dead(stream, client, 3120, Map);
                                    }
                                }

                            }

                        }
                    }

                }

                #endregion
                //jason

                if (client.Player.Mining)
                {
                    #region Mining
                    if (Timer >= client.Player.NextMine.AddSeconds(3))
                    {
                        client.Player.NextMine = Timer;
                        using (var recycle = new ServerSockets.RecycledPacket())
                        {
                            var stream = recycle.GetStream();
                            Role.Mining.Mine(stream, client);
                        }
                    }
                    #endregion
                }
                client.EffectStatus?.CheckUp();
                if (Database.ItemType.IsTwoHand(client.Equipment.RightWeapon))// need to check it
                {
                    if (client.Equipment.LeftWeapon != 0 && ((Database.ItemType.IsShield(client.Equipment.LeftWeapon)
                        || Database.ItemType.IsArrow(client.Equipment.LeftWeapon)) == false))
                    {
                        if (client.Inventory.HaveSpace(1))
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                client.Equipment.LeftWeapon = 0;
                                Console.WriteLine("Done clearing.");
                            }
                        }
                    }
                }
                if (DateTime.Now > client.LastOnlineStamp.AddMinutes(25))
                {
                    // Atualiza o timestamp do último login para o horário atual
                    client.LastOnlineStamp = DateTime.Now;

                    // Verifica se o jogador está no mapa 1002
                    if (client.Player.Map == 1002)
                    {
                        // Incrementa os pontos online do jogador em 1
                        client.Player.OnlinePoints += 1;

                        // Envia uma mensagem ao jogador informando que ele ganhou 1 pontos online
                        client.SendSysMesage($"You have earned 1 Online Points.");
                    }
                    else
                    {
                        // Incrementa os pontos online do jogador em 1
                        client.Player.OnlinePoints += 1;

                        // Envia uma mensagem ao jogador informando que ele ganhou 1 pontos online
                        // por não estar no mapa especial
                        client.SendSysMesage($"You have 1 Online Points.");
                    }
                }
            }

            catch (Exception e)
            {
                Console.WriteException(e);
            }

        }
        public unsafe static void BuffersCallback(Client.GameClient client)
        {
            try
            {
                if (!client.FullLoading)
                    return;
                if (!Valid(client))
                    return;

                var Timer = Time32.Now;
                if (Program.ArenaMaps.ContainsValue(client.Player.DynamicID))
                {
                    client.SendSysMesage("Accuracy Rates.", MsgMessage.ChatMode.FirstRightCorner);
                    foreach (var player in client.Map.Values.Where(e => e.Player.DynamicID == client.Player.DynamicID))
                    {
                        client.SendSysMesage($"{player.Player.Name} {Math.Round((double)(player.Player.Hits * 100.0 / Math.Max(1, player.Player.TotalHits)), 2)} % " +
                        $" Hits {player.Player.Hits} Miss {player.Player.TotalHits - player.Player.Hits} CC {player.Player.MaxChains}",
                        MsgMessage.ChatMode.ContinueRightCorner);

                        if (player.Player.Betting > 0 && client.Player.Betting > 0)
                        {
                            if (player.Player.Hits >= 100)
                            {
                                player.Player.ConquerPoints += (uint)(client.Player.Betting + player.Player.Betting);
                                player.SendSysMesage($"You won in the Betting Room and got {client.Player.Betting + player.Player.Betting} CPs.");
                                Program.ArenaMaps.Remove((uint)player.Player.Betting);
                                client.Teleport(429, 378, 1002);
                                player.Teleport(429, 378, 1002);
                                player.Player.Betting = 0;
                                client.Player.Betting = 0;
                            }
                            else if (client.Player.Hits >= 100)
                            {
                                client.Player.ConquerPoints += (uint)(client.Player.Betting + player.Player.Betting);
                                client.SendSysMesage($"You won in the Betting Room and got {client.Player.Betting + player.Player.Betting} CPs.");
                                Program.ArenaMaps.Remove((uint)client.Player.Betting);
                                player.Teleport(429, 378, 1002);
                                client.Teleport(429, 378, 1002);
                                player.Player.Betting = 0;
                                client.Player.Betting = 0;
                            }
                        }

                    }
                }

                #region Anti bot

                if (Timer < client.Player.LastAttack.AddSeconds(5))
                {

                    if (client.MobsKilled >= 1000)
                    {
                        if (Timer > client.Player.KillCountCaptchaStamp.AddMinutes(10))
                        {
                            if (!client.Player.WaitingKillCaptcha)
                            {
                                if (client.Player.Map != 1039) // Verifica se o jogador NÃO está no mapa 1039
                                {
                                    client.Player.KillCountCaptchaStamp = Time32.Now;
                                    client.Player.WaitingKillCaptcha = true;
                                    client.ActiveNpc = 9999997;
                                    client.Player.KillCountCaptcha = Role.Core.Random.Next(10, 100).ToString();

                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        Game.MsgNpc.Dialog dialog = new Game.MsgNpc.Dialog(client, stream);
                                        dialog.Text("Input the current text: " + client.Player.KillCountCaptcha + " to verify your humanity.");
                                        dialog.AddInput("Captcha message:", (byte)client.Player.KillCountCaptcha.Length);
                                        dialog.Option("No thank you.", 255);
                                        dialog.AddAvatar(39);
                                        dialog.FinalizeDialog();
                                        //client.Send(stream);
                                    }
                                }
                            }
                            else
                            {
                                client.Teleport(429, 378, 1002);
                            }
                        }
                    }
                }
                #endregion
                if ((client.Player.Map == 1005 || client.Player.Map == 6000)
                        && client.Player.DynamicID == 0
                        && !client.Player.Alive
                        && Timer > client.Player.DeadStamp.AddSeconds(6))
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        client.Player.Revive(stream);

                        // Verificar o mapa e definir as coordenadas apropriadas
                        if (client.Player.Map == 1005)
                        {
                            client.Teleport(050, 050, 1005);
                        }
                        else if (client.Player.Map == 6000)
                        {
                            client.Teleport(029, 072, 6000); // Exemplo de coordenadas para o mapa 6000
                        }
                    }
                }

                if (client.Player.BlockMovementCo && DateTime.Now > client.Player.BlockMovement)
                {
                    client.Player.Protect = Time32.Now.AddSeconds(1);
                    client.Player.BlockMovementCo = false;
                    client.SendSysMesage("You`re free to move now, you have 1 second to jump away.");
                }
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    if (client.Player.RemoveAfter && DateTime.Now > client.Player.RemoveStamp)
                    {
                        client.Send(stream.GarUsge());
                        client.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsuageID.RemoveInventory, 1, 0, 0, 0, 0, 0));
                        client.Player.RemoveAfter = false;
                    }
                }
                foreach (var flag in client.Player.BitVector.GetFlags())
                {
                    if (flag.Expire(Timer))
                    {
                        if (flag.Key == (int)Game.MsgServer.MsgUpdate.Flags.Cursed)
                        {
                            client.Player.CursedTimer = 0;
                            client.Player.RemoveFlag(MsgUpdate.Flags.Cursed);
                        }
                        else
                        {
                            if (flag.Key == (int)Game.MsgServer.MsgUpdate.Flags.Superman || flag.Key == (int)Game.MsgServer.MsgUpdate.Flags.Cyclone)
                            {
                                Role.KOBoard.KOBoardRanking.AddItem(new Role.KOBoard.Entry() { UID = client.Player.UID, Name = client.Player.Name, Points = (uint)client.Player.KillCounter }, true);
                            }
                            client.Player.RemoveFlag((Game.MsgServer.MsgUpdate.Flags)flag.Key);
                        }

                    }
                    else if (flag.Key == (int)Game.MsgServer.MsgUpdate.Flags.Poisoned)
                    {
                        if (flag.CheckInvoke(Timer))
                        {
                            int damage = 200;// (int)Game.MsgServer.AttackHandler.Calculate.Base.CalculatePoisonDamageFog((uint)client.Player.HitPoints, client.Player.PoisonLevel);

                            if (client.Player.HitPoints == 1)
                            {
                                damage = 0;
                                goto jump;
                            }
                            client.Player.HitPoints = Math.Max(1, (int)(client.Player.HitPoints - damage));

                        jump:

                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();

                                InteractQuery action = new InteractQuery()
                                {
                                    Damage = damage,
                                    AtkType = MsgAttackPacket.AttackID.Physical,
                                    X = client.Player.X,
                                    Y = client.Player.Y,
                                    OpponentUID = client.Player.UID
                                };
                                client.Player.View.SendView(stream.InteractionCreate(&action), true);
                            }

                        }
                    }
                    else if (flag.Key == (int)Game.MsgServer.MsgUpdate.Flags.ShurikenVortex)
                    {
                        if (flag.CheckInvoke(Timer))
                        {
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();

                                InteractQuery action = new InteractQuery()
                                {
                                    UID = client.Player.UID,
                                    X = client.Player.X,
                                    Y = client.Player.Y,
                                    SpellID = (ushort)Role.Flags.SpellID.ShurikenEffect,
                                    AtkType = MsgAttackPacket.AttackID.Magic
                                };

                                MsgAttackPacket.ProcescMagic(client, stream.InteractionCreate(&action), action);
                            }
                        }
                    }
                    else if (flag.Key == (int)Game.MsgServer.MsgUpdate.Flags.RedName
                        || flag.Key == (int)Game.MsgServer.MsgUpdate.Flags.BlackName
                        && client.Player.Map != 6000)
                    {
                        if (flag.CheckInvoke(Timer))
                        {
                            if (client.Player.PKPoints > 0)
                                client.Player.PKPoints -= 1;

                            client.Player.PkPointsStamp = Time32.Now;
                        }
                    }
                    else if (flag.Key == (int)Game.MsgServer.MsgUpdate.Flags.Cursed)
                    {
                        if (flag.CheckInvoke(Timer))
                        {
                            if (client.Player.CursedTimer > 0)
                                client.Player.CursedTimer -= 1;
                            else
                            {
                                client.Player.CursedTimer = 0;
                                client.Player.RemoveFlag(Game.MsgServer.MsgUpdate.Flags.Cursed);
                            }
                        }
                    }
                }
                if (client.Player.OnTransform)
                {
                    if (client.Player.TransformInfo != null)
                    {
                        if (client.Player.TransformInfo.CheckUp(Timer))
                            client.Player.TransformInfo = null;
                    }
                }
                if (client.Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Praying))
                {
                    if (client.Player.BlessTime < 7200000 - 1000)
                    {
                        if (Timer > client.Player.CastPrayStamp.AddSeconds(1))
                        {
                            bool have = false;
                            foreach (var ownerpraying in client.Player.View.Roles(Role.MapObjectType.Player))
                            {
                                if (Role.Core.GetDistance(client.Player.X, client.Player.Y, ownerpraying.X, ownerpraying.Y) <= 2)
                                {
                                    var target = ownerpraying as Role.Player;
                                    if (target.ContainFlag(MsgUpdate.Flags.CastPray))
                                    {
                                        have = true;
                                        break;
                                    }
                                }
                            }
                            if (!have)
                                client.Player.RemoveFlag(MsgUpdate.Flags.Praying);
                            client.Player.CastPrayStamp = new Time32(Timer.AllMilliseconds);
                            client.Player.BlessTime += 1000;
                        }
                    }
                    else
                        client.Player.BlessTime = 3100000;
                }
                if (client.Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.CastPray))
                {
                    if (client.Player.BlessTime < 7200000 - 3000)
                    {
                        if (Timer > client.Player.CastPrayStamp.AddSeconds(1))
                        {
                            client.Player.CastPrayStamp = new Time32(Timer.AllMilliseconds);
                            client.Player.BlessTime += 3000;
                        }
                    }
                    else
                        client.Player.BlessTime = 7200000;

                    if (Timer > client.Player.CastPrayActionsStamp.AddSeconds(1))
                    {
                        client.Player.CastPrayActionsStamp = new Time32(Timer.AllMilliseconds);
                        foreach (var obj in client.Player.View.Roles(Role.MapObjectType.Player))
                        {
                            if (Role.Core.GetDistance(client.Player.X, client.Player.Y, obj.X, obj.Y) <= 3)
                            {
                                var Target = obj as Role.Player;
                                if (Target.Reborn < 2)
                                {
                                    if (!Target.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Praying))
                                    {
                                        Target.AddFlag(Game.MsgServer.MsgUpdate.Flags.Praying, Role.StatusFlagsBigVector32.PermanentFlag, true);

                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            var stream = rec.GetStream();
                                            ActionQuery action = new ActionQuery()
                                            {
                                                ObjId = client.Player.UID,
                                                dwParam = (uint)client.Player.Action,
                                                Timestamp = (int)obj.UID
                                            };
                                            client.Player.View.SendView(stream.ActionCreate(&action), true);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (client.Player.BlessTime > 0)
                {
                    if (!client.Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.CastPray) && !client.Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Praying))
                    {

                        if (Timer > client.Player.CastPrayStamp.AddSeconds(1))
                        {
                            if (client.Player.BlessTime > 1000)
                                client.Player.BlessTime -= 1000;
                            else
                                client.Player.BlessTime = 0;
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                client.Player.SendUpdate(stream, client.Player.BlessTime, Game.MsgServer.MsgUpdate.DataType.LuckyTimeTimer);
                            }
                            client.Player.CastPrayStamp = new Time32(Timer.AllMilliseconds);
                        }
                    }
                }



                if (client.Team != null)
                {
                    if (client.Team.AutoInvite == true && client.Player.Map != 1036 && client.Team.CkeckToAdd())//7. When you create a Team, Auto Invite is enabled automatically without it being selected.
                    {
                        if (Timer > client.Team.InviteTimer.AddSeconds(10))
                        {
                            client.Team.InviteTimer = Timer;
                            foreach (var obj in client.Player.View.Roles(Role.MapObjectType.Player))
                            {
                                if (!client.Team.SendInvitation.Contains(obj.UID))
                                {
                                    client.Team.SendInvitation.Add(obj.UID);

                                    if ((obj as Role.Player).Owner.Team == null)
                                    {
                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            var stream = rec.GetStream();
                                            stream.TeamCreate(MsgTeam.TeamTypes.InviteRequest, client.Player.UID);
                                            obj.Send(stream);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (client.Team.TeamLider(client))
                    {
                        if (Timer > client.Team.UpdateLeaderLocationStamp.AddSeconds(4))
                        {
                            client.Team.UpdateLeaderLocationStamp = Timer;
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();

                                ActionQuery action = new ActionQuery()
                                {
                                    ObjId = client.Player.UID,
                                    dwParam = 1015,
                                    Type = ActionType.LocationTeamLieder,
                                    wParam1 = client.Team.Leader.Player.X,
                                    wParam2 = client.Team.Leader.Player.Y
                                };
                                client.Team.SendTeam(stream.ActionCreate(&action), client.Player.UID, client.Player.Map);
                            }
                        }
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteException(e);
            }

        }
    }
}
