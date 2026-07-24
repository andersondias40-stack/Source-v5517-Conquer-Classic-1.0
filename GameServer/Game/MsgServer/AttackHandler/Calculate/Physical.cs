using System;

namespace COServer.Game.MsgServer.AttackHandler.Calculate
{
    public class Physical
    {
        public static void OnMonster(Role.Player player, MsgMonster.MonsterRole monster, Database.MagicType.Magic DBSpell, out MsgSpellAnimation.SpellObj SpellObj, byte MultipleDamage = 0)
        {

            SpellObj = new MsgSpellAnimation.SpellObj(monster.UID, 0);
            if (monster.IsFloor)
            {
                SpellObj.Damage = 2;
                return;
            }
            if (DBSpell == null)
            {
                if (Base.Dodged(player.Owner, monster))
                {
                    SpellObj.Damage = 0;
                    return;
                }

            }
            int Damage = (int)Base.GetDamage(player.Owner.Status.MaxAttack, player.Owner.Status.MinAttack);

            Damage = (int)player.Owner.AjustAttack((uint)Damage);

            if (monster.Name == "Guard2")
                Damage = 1;

            if (DBSpell != null && DBSpell.Damage < 10 && DBSpell.ID != 10490)
                DBSpell.Damage = 10;

            if (MultipleDamage != 0)
            {
                Damage = Damage * MultipleDamage;
            }
            if (DBSpell != null)
                Damage = Base.MulDiv((int)Damage, (int)((DBSpell != null) ? DBSpell.Damage > 30000 ? DBSpell.Damage -= 30000 : DBSpell.Damage : Program.ServerConfig.PhysicalDamage), 100);

            Damage = Base.AdjustMinDamageUser2Monster(Damage, player.Owner);
            Damage = Base.CalcDamageUser2Monster(Damage, monster.Family.Defense, player.Level, monster.Level, false);
            if (monster.Name.Contains("Guard"))
                monster.Family.Defense2 = 1000;

            Damage = (int)Base.BigMulDiv(Damage, monster.Family.Defense2, Client.GameClient.DefaultDefense);

            if (player.ContainFlag(MsgUpdate.Flags.Superman))
                Damage = (int)(Damage * 5);

            if ((monster.Family.Settings & MsgMonster.MonsterSettings.Guard) == MsgMonster.MonsterSettings.Guard)
                SpellObj.Damage /= 10;
            if (player.ContainFlag(MsgUpdate.Flags.Shield))
            {
                if (SpellObj.Damage > 300)
                {
                    SpellObj.Damage -= 300;
                }
                else
                {
                    SpellObj.Damage = 1;
                }
            }
            if (player.ContainFlag(MsgUpdate.Flags.MagicShield))
            {
                if (SpellObj.Damage > player.AzureShieldDefence)
                {
                    SpellObj.Damage -= player.AzureShieldDefence;

                }
                else
                {
                    player.AzureShieldDefence -= (ushort)SpellObj.Damage;
                    SpellObj.Damage = 1;
                }
            }

            //if (player.Owner.ProjectManager)
            //    SpellObj.Damage = 10000000;
            if (Damage > 0 && player.BlessTime > 0 && Role.Core.PercentSuccess(Global.LUCKY_TIME_CRIT_RATE_PHYSICAL))
            {
                Damage *= 2;
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var msg = rec.GetStream();
                    player.SendString(msg, MsgStringPacket.StringID.Effect, true, "LuckyGuy");
                }
                //player.Owner.SendSysMesage("Lucky Strike: You had inflict double damage on the target.", MsgMessage.ChatMode.Action);
            }
            SpellObj.Damage = (uint)Math.Max(1, Damage);
        }
        public static void OnPlayer(Role.Player player, Role.Player target, Database.MagicType.Magic DBSpell, out MsgSpellAnimation.SpellObj SpellObj, bool StackOver = false, int IncreaseAttack = 0)
        {
            SpellObj = new MsgSpellAnimation.SpellObj(target.UID, 0);
            if (target.ContainFlag(MsgUpdate.Flags.ShurikenVortex))
            {
                SpellObj.Damage = 1;
                return;
            }
            if (DBSpell == null)
            {
                if (Base.Dodged(player.Owner, target.Owner))
                {
                    SpellObj.Damage = 0;
                    return;
                }
            }

            int nAttack = (int)Base.GetDamage(player.Owner.Status.MaxAttack, player.Owner.Status.MinAttack);


            #region DbSpellPower
            if (DBSpell != null)
            {
                float tPower = DBSpell != null ? DBSpell.Damage : 100;
                if (tPower > 30000)
                {
                    tPower = (float)(Math.Max(tPower - 30000, 0) * 0.01);
                    nAttack = (int)(nAttack * tPower);
                }
                else
                    nAttack += (short)tPower;
                //if (DBSpell.DamagePersent != 0)
                //    nDamage = (int)((nDamage * DBSpell.DamagePersent));
            }
            #endregion
            //nAttack = (int)(Math.Min(nAttack, 100000) * 1.00);
            var nDef = target.Owner.AjustDefense;


            int nDamage = target.Owner.AdjustWeaponDamage((int)(nAttack - nDef));

            if (player.ContainFlag(MsgUpdate.Flags.Stigma))
            {
                if (nDamage > player.StagimaAttack)
                {
                    nDamage += player.StagimaAttack;

                }
            }

            if (target.Reborn == 1)
                nDamage = (int)Math.Round((double)(nDamage * 0.7));
            else if (target.Reborn == 2)
                nDamage = (int)Math.Round((double)(nDamage * 0.8));//

            nDamage = (int)Math.Round(nDamage * (1f - (target.Owner.Status.ItemBless * 0.01)));

            if (target.Owner.Status.Damage > 0)
                nDamage = (int)Math.Round(nDamage * Math.Max(1.00 - target.Owner.Status.Damage, 0.90));

            //  nDamage = (int)Calculate.Base.CalculateExtraAttack((uint)nDamage, player.Owner.Status.PhysicalDamageIncrease, target.Owner.Status.PhysicalDamageDecrease);

            //   nDamage = (int)(nDamage * 1.2);
            if (nDamage <= 0)
                nDamage = 1;


            if (target.ContainFlag(MsgUpdate.Flags.MagicShield) || target.ContainFlag(MsgUpdate.Flags.Shield))
            {
                if (nDamage > target.AzureShieldDefence)
                {
                    nDamage -= target.AzureShieldDefence;

                }
                else
                {
                    target.AzureShieldDefence -= (ushort)SpellObj.Damage;
                    SpellObj.Damage = 1;
                }
            }


            if (target.Owner.Equipment.ShieldID != 0)
            {
                //SpellObj.Effect |= MsgAttackPacket.AttackEffect.Block;
                if (nDamage > target.Owner.Status.ShieldDefenece)
                {
                    nDamage -= (int)target.Owner.Status.ShieldDefenece;
                }

            }

            if (nDamage > 0 && player.BlessTime > 0 && Role.Core.PercentSuccess(Global.LUCKY_TIME_CRIT_RATE_PHYSICAL))
            {
                nDamage *= 2;
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var msg = rec.GetStream();
                    player.SendString(msg, MsgStringPacket.StringID.Effect, true, "LuckyGuy");
                }
                //player.Owner.SendSysMesage("Lucky Strike: You had inflict double damage on the target.", MsgMessage.ChatMode.Action);
            }

            SpellObj.Damage = (uint)Math.Max(1, nDamage);
            if (CheckAttack.BlockRefect.CanUseReflect(player.Owner))
            {
                MsgSpellAnimation.SpellObj InRedirect;
                if (BackDmg.Calculate(player, target, DBSpell, SpellObj.Damage, out InRedirect))
                    SpellObj = InRedirect;
            }
            if (target.Name.Contains("[PM]") || target.Name.Contains("[GM]"))
            {
                SpellObj.Damage = 1;
            }

        }
        public static void OnNpcs(Role.Player player, Role.SobNpc target, Database.MagicType.Magic DBSpell, out MsgSpellAnimation.SpellObj SpellObj)
        {
            SpellObj = new MsgSpellAnimation.SpellObj(target.UID, 0);
            int Damage = (int)Base.GetDamage(player.Owner.Status.MaxAttack, player.Owner.Status.MinAttack);
            Damage = (int)player.Owner.AjustAttack((uint)Damage);

            Damage = Base.MulDiv((int)Damage, (int)((DBSpell != null) ? DBSpell.Damage > 30000 ? DBSpell.Damage -= 30000 : DBSpell.Damage : Program.ServerConfig.PhysicalDamage), 100);
            Damage = (int)Base.BigMulDiv((int)Damage, Client.GameClient.DefaultDefense, player.Owner.GetDefense2());

            SpellObj.Damage = (uint)Math.Max(1, Damage);

            SpellObj.Damage = Calculate.Base.CalculateExtraAttack(SpellObj.Damage, player.Owner.Status.PhysicalDamageIncrease, 0);
            if (target.ContainFlag(MsgUpdate.Flags.AzureShield))
                SpellObj.Damage = 100;
            if (Damage > 0 && player.BlessTime > 0 && Role.Core.PercentSuccess(Global.LUCKY_TIME_CRIT_RATE_PHYSICAL))
            {
                Damage *= 2;
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var msg = rec.GetStream();
                    player.SendString(msg, MsgStringPacket.StringID.Effect, true, "LuckyGuy");
                }
                //player.Owner.SendSysMesage("Lucky Strike: You had inflict double damage on the target.", MsgMessage.ChatMode.Action);
            }
        }
    }
}
