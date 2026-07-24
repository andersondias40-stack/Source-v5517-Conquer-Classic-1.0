using System.Collections.Generic;

namespace COServer.Game.MsgServer.AttackHandler
{
    public class Oblivion
    {
        public unsafe static void Execute(Client.GameClient user, InteractQuery Attack, ServerSockets.Packet stream, Dictionary<ushort, Database.MagicType.Magic> DBSpells)
        {
            Database.MagicType.Magic DBSpell;
            MsgSpell ClientSpell;
            if (CheckAttack.CanUseSpell.Verified(Attack, user, DBSpells, out ClientSpell, out DBSpell))
            {
                MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                            , 0, Attack.X, Attack.Y, ClientSpell.ID
                            , ClientSpell.Level, ClientSpell.UseSpellSoul);

                user.Player.RemoveFlag(MsgUpdate.Flags.XPList);
                user.Player.OpenXpSkill(MsgUpdate.Flags.Oblivion, 30);
                user.Player.OblivionMobs = 0;
                MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID,0));
                MsgSpell.SetStream(stream);
                MsgSpell.Send(user);
            }
        }
    }
}
