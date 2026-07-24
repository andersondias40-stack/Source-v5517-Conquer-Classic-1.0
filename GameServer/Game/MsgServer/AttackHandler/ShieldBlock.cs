using System;
using System.Collections.Generic;

namespace COServer.Game.MsgServer.AttackHandler
{
    public class ShieldBlock
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

                user.Player.ShieldBlockDamage = (uint)DBSpell.Damage;
                user.Player.ShieldBlockEnd = DateTime.Now.AddSeconds(DBSpell.Duration);
                user.Player.AddSpellFlag(MsgUpdate.Flags.ShieldBlock, (int)DBSpell.Duration, true);
                user.Player.SendUpdate(stream, Game.MsgServer.MsgUpdate.Flags.ShieldBlock, DBSpell.Duration
        , (uint)DBSpell.Damage, ClientSpell.Level, Game.MsgServer.MsgUpdate.DataType.AzureShield, true);

                MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID,0));
                MsgSpell.SetStream(stream);
                MsgSpell.Send(user);

                ClientSpell.Experience += (int)(10 * Program.ServerConfig.ExpRateSpell);
                if (ClientSpell.Experience > DBSpells[ClientSpell.Level].Experience && ClientSpell.Level != 4)
                {
                    ClientSpell.Level++;
                    ClientSpell.Experience = 0;
                }
                user.Send(stream.SpellCreate(ClientSpell));
                user.MySpells.ClientSpells[ClientSpell.ID] = ClientSpell;
            }
        }
    }
}
