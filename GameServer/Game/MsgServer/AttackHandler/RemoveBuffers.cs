using System;
using System.Collections.Generic;

namespace COServer.Game.MsgServer.AttackHandler
{
    public class RemoveBuffers
    {
        public unsafe static void Execute(Client.GameClient user, InteractQuery Attack, ServerSockets.Packet stream, Dictionary<ushort, Database.MagicType.Magic> DBSpells)
        {
            Database.MagicType.Magic DBSpell;
            MsgSpell ClientSpell;
            if (CheckAttack.CanUseSpell.Verified(Attack, user, DBSpells, out ClientSpell, out DBSpell))
            {
                switch (ClientSpell.ID)
                {
                    case (ushort)Role.Flags.SpellID.Compassion:
                        {
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                            , 0, Attack.X, Attack.Y, ClientSpell.ID
                            , ClientSpell.Level, ClientSpell.UseSpellSoul);

                            if (user.Team != null)
                            {
                                foreach (var target in user.Team.GetMembers())
                                {
                                    if (Role.Core.GetDistance(user.Player.X, user.Player.Y, target.Player.X, target.Player.Y) < 18)
                                    {
                                        target.Player.RemoveFlag(MsgUpdate.Flags.Poisoned);
                                        target.Player.RemoveFlag(MsgUpdate.Flags.PoisonStar);
                                        MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(target.Player.UID, 0));
                                    }
                                }
                            }

                            MsgSpell.SetStream(stream); MsgSpell.Send(user);

                            Updates.UpdateSpell.CheckUpdate(stream, user, Attack, 1000, DBSpells);

                            break;
                        }
                    case (ushort)Role.Flags.SpellID.Tranquility:
                        {

                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                           , 0, Attack.X, Attack.Y, ClientSpell.ID
                           , ClientSpell.Level, ClientSpell.UseSpellSoul);

                            if (user.Player.UID == Attack.OpponentUID)
                            {
                                user.Player.RemoveFlag(MsgUpdate.Flags.Poisoned);
                                user.Player.RemoveFlag(MsgUpdate.Flags.PoisonStar);
                                MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID, 0));
                            }
                            else
                            {
                                Role.IMapObj target;
                                if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Player))
                                {
                                    Role.Player attacked = target as Role.Player;
                                    if (attacked.ContainFlag(MsgUpdate.Flags.SoulShackle))
                                        attacked.SendUpdate(stream, MsgUpdate.Flags.SoulShackle, 0, 0, ClientSpell.Level, MsgUpdate.DataType.SoulShackle, false);
                                    attacked.RemoveFlag(MsgUpdate.Flags.SoulShackle);
                                    attacked.RemoveFlag(MsgUpdate.Flags.Poisoned);
                                    attacked.RemoveFlag(MsgUpdate.Flags.PoisonStar);
                                    MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(attacked.UID, 0));

                                }
                            }


                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);

                            Updates.UpdateSpell.CheckUpdate(stream, user, Attack, 1000, DBSpells);
                            break;
                        }
                    case (ushort)Role.Flags.SpellID.Serenity:
                        {
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                            , 0, Attack.X, Attack.Y, ClientSpell.ID
                            , ClientSpell.Level, ClientSpell.UseSpellSoul);

                            if (user.Player.UID == Attack.OpponentUID)
                            {
                                user.Player.RemoveFlag(MsgUpdate.Flags.SoulShackle);
                                user.Player.RemoveFlag(MsgUpdate.Flags.Frightened);
                                user.Player.RemoveFlag(MsgUpdate.Flags.Freeze);
                                user.Player.RemoveFlag(MsgUpdate.Flags.Dizzy);
                                user.Player.RemoveFlag(MsgUpdate.Flags.Poisoned);
                                user.Player.RemoveFlag(MsgUpdate.Flags.PoisonStar);
                                MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID, 0));
                            }
                            else
                            {
                                Role.IMapObj target;
                                if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Player))
                                {
                                    Role.Player attacked = target as Role.Player;
                                    attacked.RemoveFlag(MsgUpdate.Flags.Frightened);
                                    attacked.RemoveFlag(MsgUpdate.Flags.Freeze);
                                    attacked.RemoveFlag(MsgUpdate.Flags.Dizzy);
                                    attacked.RemoveFlag(MsgUpdate.Flags.Poisoned);
                                    attacked.RemoveFlag(MsgUpdate.Flags.PoisonStar);
                                    MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(attacked.UID, 0));

                                }
                            }

                            MsgSpell.SetStream(stream); MsgSpell.Send(user);

                            Updates.UpdateSpell.CheckUpdate(stream, user, Attack, 1000, DBSpells);
                            break;
                        }
                }
            }
        }
    }
}
