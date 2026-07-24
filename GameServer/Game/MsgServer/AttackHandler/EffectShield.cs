using System.Collections.Generic;

namespace COServer.Game.MsgServer.AttackHandler
{
    public class EffectShield
    {
        public unsafe static void Execute(InteractQuery Attack, Client.GameClient user, ServerSockets.Packet stream, Dictionary<ushort, Database.MagicType.Magic> DBSpells)
        {

            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                , 0, user.Player.X, user.Player.Y, (ushort)Role.Flags.SpellID.Shield
                , 4, 0);

            if (!user.Player.ContainFlag(MsgUpdate.Flags.Shield))
            {
                user.Player.AddSpellFlag(MsgUpdate.Flags.Shield, 40, true);
                MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID,0));
            }




            MsgSpell.SetStream(stream);
            MsgSpell.Send(user);
        }
    }
}
