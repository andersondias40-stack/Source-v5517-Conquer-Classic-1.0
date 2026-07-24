using System.Collections.Generic;

namespace COServer.Game.MsgServer.AttackHandler
{
    public class DispatchXp
    {
        public unsafe static void Execute(Client.GameClient user, InteractQuery Attack, ServerSockets.Packet stream, Dictionary<ushort, Database.MagicType.Magic> DBSpells)
        {
            Database.MagicType.Magic DBSpell;
            MsgSpell ClientSpell;
            if (CheckAttack.CanUseSpell.Verified(Attack, user, DBSpells, out ClientSpell, out DBSpell))
            {
                {
                    if (!user.Player.RemoveFlag(MsgUpdate.Flags.XPList))
                        return;
                    MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                            , 0, Attack.X, Attack.Y, ClientSpell.ID
                            , ClientSpell.Level, ClientSpell.UseSpellSoul);


                    bool IncreaseExp = false;
                    if (user.Player.UID == Attack.OpponentUID)
                    {
                        IncreaseExp = true;
                        user.Player.XPCount += 20;
                        MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID,0));
                    }
                    else
                    {
                        Role.IMapObj target;
                        if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Player))
                        {
                            IncreaseExp = true;
                            Role.Player attacked = target as Role.Player;
                            attacked.XPCount += 20;
                            MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(attacked.UID,0));
                        }
                    }

                    if (user.Team != null)
                    {
                        IncreaseExp = true;
                        foreach (var target in user.Team.GetMembers())
                        {
                            if (target.Player.UID == user.Player.UID)
                                continue;
                            if (Role.Core.GetDistance(user.Player.X, user.Player.Y, target.Player.X, target.Player.Y) < 18)
                            {
                                target.Player.XPCount += 20;
                                MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(target.Player.UID,0));
                            }
                        }
                    }

                    MsgSpell.SetStream(stream);
                    MsgSpell.Send(user);

                    if (IncreaseExp)
                        Updates.UpdateSpell.CheckUpdate(stream, user, Attack, 200, DBSpells);
                }
            }
        }
    }
}
