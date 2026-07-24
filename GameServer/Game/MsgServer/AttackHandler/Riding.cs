using System;
using System.Collections.Generic;

namespace COServer.Game.MsgServer.AttackHandler
{
    public class Riding
    {
        public unsafe static void Execute(Client.GameClient user, InteractQuery Attack, ServerSockets.Packet stream, Dictionary<ushort, Database.MagicType.Magic> DBSpells)
        {
            Database.MagicType.Magic DBSpell;
            MsgSpell ClientSpell;
            if (CheckAttack.CanUseSpell.Verified(Attack, user, DBSpells, out ClientSpell, out DBSpell))
            {
                if (user.Player.ContainFlag(MsgUpdate.Flags.Fly) || user.Player.OnTransform)
                {
                    user.SendSysMesage("You can`t use this skill right now !");
                    return;
                }
                if (user.Player.Map == 1860 || user.Player.Map == 1005 || user.Player.Map == 1601 || user.Player.Map == 1036 || user.Player.Map == 2071 || user.Player.Map == 1764 || user.Player.Map == 1858 || user.Player.Map == 8881 || user.Player.Map == 8880 || user.Player.Map == 1038 || user.Player.Map == 700 || MsgTournaments.MsgSchedules.CurrentTournament.InTournament(user))
                {
                    user.SendSysMesage("You can't use this skill on this map.");
                    return;
                }

                MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                                 , 0, Attack.X, Attack.Y, ClientSpell.ID
                                 , ClientSpell.Level, ClientSpell.UseSpellSoul);

                MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID,0));
                MsgSpell.SetStream(stream);
                MsgSpell.Send(user);

            }
        }
    }
}
