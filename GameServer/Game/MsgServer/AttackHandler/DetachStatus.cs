using System.Collections.Generic;

namespace COServer.Game.MsgServer.AttackHandler
{
    public class DetachStatus
    {
        public unsafe static void Execute(Client.GameClient user, InteractQuery Attack, ServerSockets.Packet stream, Dictionary<ushort, Database.MagicType.Magic> DBSpells)
        {
            Database.MagicType.Magic DBSpell;
            MsgSpell ClientSpell;
            if (CheckAttack.CanUseSpell.Verified(Attack, user, DBSpells, out ClientSpell, out DBSpell))
            {
                switch (ClientSpell.ID)
                {
                    case (ushort)Role.Flags.SpellID.ArcherBane:
                        {
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                            , 0, Attack.X, Attack.Y, ClientSpell.ID
                            , ClientSpell.Level, ClientSpell.UseSpellSoul);


                            Role.IMapObj target;
                            if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Player))
                            {
                                Role.Player attacked = target as Role.Player;
                                if (attacked.ContainFlag(MsgUpdate.Flags.Fly))
                                {
                                    if (CheckAttack.CanAttackPlayer.Verified(user, attacked, DBSpell))
                                    {
                                        //if (Calculate.Base.Success(70))
                                        {
                                            attacked.RemoveFlag(MsgUpdate.Flags.Fly);
                                            MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(attacked.UID, 30));
                                        }
                                    }
                                    //else
                                    //{
                                    //    var clientobj = new MsgSpellAnimation.SpellObj(attacked.UID, MsgSpell.SpellID);
                                    //    clientobj.Hit = 0;
                                    //    MsgSpell.Targets.Enqueue(clientobj);
                                    //}
                                }
                            }
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);

                            Updates.UpdateSpell.CheckUpdate(stream, user, Attack, 250, DBSpells);

                            break;
                        }
                    case (ushort)Role.Flags.SpellID.Revive:
                        {
                            if (user.Player.Name.Contains("[GM]"))
                                break;
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                            , 0, Attack.X, Attack.Y, ClientSpell.ID
                            , ClientSpell.Level, ClientSpell.UseSpellSoul);

                            user.Player.RemoveFlag(MsgUpdate.Flags.XPList);

                            Role.IMapObj target;
                            if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Player))
                            {
                                Role.Player attacked = target as Role.Player;

                                if (!attacked.Alive)
                                {
                                    MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(attacked.UID,0));
                                    attacked.Revive(stream);
                                }
                                else
                                {
                                    user.SendSysMesage("You can`t revive this player, he's not dead.");
                                    break;
                                }
                            }


                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);

                            Updates.UpdateSpell.CheckUpdate(stream, user, Attack, DBSpell.Duration, DBSpells);

                            break;
                        }
                    case (ushort)Role.Flags.SpellID.Pray:
                        {
                            if (user.Player.Map == 700 || user.Player.Map == 1005)
                            {
                                user.SendSysMesage("This spell isn't working on this map..");
                                break;
                            }
                            if (user.Player.Name.Contains("[GM]"))
                                break;
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                            , 0, Attack.X, Attack.Y, ClientSpell.ID
                            , ClientSpell.Level, ClientSpell.UseSpellSoul);



                            Role.IMapObj target;
                            if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Player))
                            {
                                Role.Player attacked = target as Role.Player;
                                if (!attacked.Alive)
                                {
                                    MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(attacked.UID,0));
                                    attacked.Revive(stream);
                                }
                                else
                                {
                                    user.Player.Mana += DBSpell.UseMana;
                                    user.SendSysMesage("You can`t revive this player, he's not dead.");
                                    break;
                                }
                            }
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);
                            Updates.UpdateSpell.CheckUpdate(stream, user, Attack, DBSpell.Duration, DBSpells);
                            break;
                        }
                }
            }
        }
    }
}
