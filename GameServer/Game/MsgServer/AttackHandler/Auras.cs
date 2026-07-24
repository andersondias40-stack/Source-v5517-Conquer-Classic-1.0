using System.Collections.Generic;

namespace COServer.Game.MsgServer.AttackHandler
{
    public class Auras
    {
        public unsafe static void Execute(Client.GameClient user, InteractQuery Attack, ServerSockets.Packet stream, Dictionary<ushort, Database.MagicType.Magic> DBSpells)
        {
            Database.MagicType.Magic DBSpell;
            MsgSpell ClientSpell;
            if (CheckAttack.CanUseSpell.Verified(Attack, user, DBSpells, out ClientSpell, out DBSpell))
            {
                switch (ClientSpell.ID)
                {
                    case (ushort)Role.Flags.SpellID.MagicDefender:
                        {
                            if (user.Player.ContainFlag(MsgUpdate.Flags.Ride))
                                user.Player.RemoveFlag(MsgUpdate.Flags.Ride);

                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                                        , user.Player.UID, Attack.X, Attack.Y, ClientSpell.ID
                                        , ClientSpell.Level, ClientSpell.UseSpellSoul);
                            if (!user.Player.ContainFlag(MsgUpdate.Flags.MagicDefender))
                            {
                                user.Player.AddFlag(MsgUpdate.Flags.MagicDefender, (int)DBSpell.Duration, true);
                                user.Player.SendUpdate(stream, Game.MsgServer.MsgUpdate.Flags.MagicDefender, DBSpell.Duration
           , 0, ClientSpell.Level, Game.MsgServer.MsgUpdate.DataType.AzureShield, true);
                            }
                            else
                            {
                                user.Player.RemoveFlag(MsgUpdate.Flags.MagicDefender);
                                user.Player.SendUpdate(stream, Game.MsgServer.MsgUpdate.Flags.MagicDefender, 0
               , 0, ClientSpell.Level, Game.MsgServer.MsgUpdate.DataType.AzureShield, true);
                            }

                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);
                            Updates.UpdateSpell.CheckUpdate(stream, user, Attack, 200, DBSpells);


                            break;
                        }
                }
            }
        }
    }
}
