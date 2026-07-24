using System.Collections.Generic;

namespace COServer.Game.MsgServer.AttackHandler
{
    public class Toxic
    {
        public unsafe static void Execute(Client.GameClient user, InteractQuery Attack, ServerSockets.Packet stream, Dictionary<ushort, Database.MagicType.Magic> DBSpells)
        {
          /*  Database.MagicType.Magic DBSpell;
            MsgSpell ClientSpell;
            if (CheckAttack.CanUseSpell.Verified(Attack, user, DBSpells, out ClientSpell, out DBSpell))
            {
                switch (ClientSpell.ID)
                {
                    case (ushort)Role.Flags.SpellID.ToxicFog:
                        {
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                                , 0, Attack.X, Attack.Y, ClientSpell.ID
                                , ClientSpell.Level, ClientSpell.UseSpellSoul);
                            uint Damage = 0;
                            foreach (Role.IMapObj target in user.Player.View.Roles(Role.MapObjectType.Monster))
                            {
                                MsgMonster.MonsterRole attacked = target as MsgMonster.MonsterRole;
                                if (Calculate.Base.GetDistance(Attack.X, Attack.Y, attacked.X, attacked.Y) < 5)
                                {
                                    if (CheckAttack.CanAttackMonster.Verified(user, attacked, DBSpell))
                                    {
                                        if (attacked.Boss == 0)
                                        {
                                            attacked.PoisonLevel = (byte)(ClientSpell.Level + ClientSpell.UseSpellSoul);
                                            Damage = 1000;
                                            attacked.AddSpellFlag(MsgUpdate.Flags.Poisoned, (int)DBSpell.Duration, true, 1);
                                        }
                                    }
                                }
                            }
                            foreach (Role.IMapObj targer in user.Player.View.Roles(Role.MapObjectType.Player))
                            {
                                var attacked = targer as Role.Player;
                                if (Calculate.Base.GetDistance(Attack.X, Attack.Y, attacked.X, attacked.Y) < 5)
                                {
                                    if (CheckAttack.CanAttackPlayer.Verified(user, attacked, DBSpell) && attacked.HitPoints > 10)
                                    {
                                        if (attacked.Owner.Status.Detoxication >= 100)
                                            continue;
                                        var rate = (((user.Player.BattlePower - attacked.BattlePower) + 10) * 7);
                                        if ((attacked.BattlePower - user.Player.BattlePower) >= 10)
                                            rate = 10; // Pity Success rate.
                                        if (user.Player.BattlePower >= attacked.BattlePower) rate = 100;
                                        double dex = (100 - attacked.Owner.Status.Detoxication) / 100.0;

                                        var Removepercent = (DBSpell.Damage / 100) * dex;

                                        if (Role.Core.Rate((int)rate))
                                        {
                                            attacked.PoisonLevel = Removepercent;
                                            Damage = 1000;
                                            attacked.AddSpellFlag(MsgUpdate.Flags.Poisoned, (int)DBSpell.Duration, true, 2);
                                        }
                                    }
                                }
                            }

                            Updates.UpdateSpell.CheckUpdate(stream, user, Attack, Damage, DBSpells);
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);

                            break;
                        }

                }
            }*/
        }
    }
}
