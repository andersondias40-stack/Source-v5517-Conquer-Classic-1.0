using System;

namespace COServer.Game.MsgServer.AttackHandler.Calculate
{
    public class Magic
    {
        public static void OnMonster(Role.Player player, MsgMonster.MonsterRole monster, Database.MagicType.Magic DBSpell, out MsgSpellAnimation.SpellObj SpellObj)
        {


            SpellObj = new MsgSpellAnimation.SpellObj(monster.UID, 0);


            if (monster.IsFloor)
            {
                SpellObj.Damage = 1;
                return;
            }

            SpellObj.Damage += (uint)player.Owner.Status.MagicAttack;

            if (DBSpell != null)
                SpellObj.Damage += (uint)DBSpell.Damage;//(uint)((SpellObj.Damage * DBSpell.Damage) / 100);
            if (player.Level >= monster.Level)
                SpellObj.Damage = (uint)(SpellObj.Damage * 1.8);

            //if (SpellObj.Damage > monster.Family.Defense)
            //    SpellObj.Damage -= monster.Family.Defense;
            //else
            //    SpellObj.Damage = 1;

            if (monster.Name == "Guard2")
                monster.Family.Defense = 500;
            SpellObj.Damage = (uint)Base.CalcDamageUser2Monster((int)SpellObj.Damage, monster.Family.Defense, player.Level, monster.Level, false);
            //SpellObj.Damage = (uint)Base.AdjustMinDamageUser2Monster((int)SpellObj.Damage, player.Owner);
            SpellObj.Damage = Base.CalculateExtraAttack(SpellObj.Damage, player.Owner.Status.MagicDamageIncrease, 0);
            if (monster.Name.Contains("Guard"))
                monster.Family.Defense2 = 10000;
            SpellObj.Damage = (uint)Base.BigMulDiv(SpellObj.Damage, monster.Family.Defense2, Client.GameClient.DefaultDefense);

            SpellObj.Damage += player.Owner.Status.MagicDamageIncrease;

            //if (monster.Family.Defense2 == 0)
            //    SpellObj.Damage = 1;

            //if ((monster.Family.Settings & MsgMonster.MonsterSettings.Guard) == MsgMonster.MonsterSettings.Guard)
            //    SpellObj.Damage = 2000;
            if ((monster.Family.Settings & MsgMonster.MonsterSettings.Guard) == MsgMonster.MonsterSettings.Guard)
                SpellObj.Damage /= 10;
            if (monster.Family.ID == 20211)
                SpellObj.Damage = 1;
            if (SpellObj.Damage > 0 && player.BlessTime > 0 && Role.Core.PercentSuccess(Global.LUCKY_TIME_CRIT_RATE_MAGIC))
            {
                SpellObj.Damage *= 2;
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var msg = rec.GetStream();
                    player.SendString(msg, MsgStringPacket.StringID.Effect, true, "LuckyGuy");
                }
                //player.Owner.SendSysMesage("Lucky Strike: You had inflict double damage on the target.", MsgMessage.ChatMode.Action);
            }
            if (monster.Name == "Guard2")
                SpellObj.Damage /= 3;

        }
        public static void OnPlayer(Role.Player player, Role.Player target, Database.MagicType.Magic DBSpell, out MsgSpellAnimation.SpellObj SpellObj)
        {
            SpellObj = new MsgSpellAnimation.SpellObj(target.UID, 0);
            if (target.ContainFlag(MsgUpdate.Flags.ShurikenVortex))
            {
                SpellObj.Damage = 1;
                return;
            }
            int nAtk = (int)player.Owner.Status.MagicAttack;
            // int nDef = (int)target.Owner.Status.MagicDefence* (1+;Damage = attacker.Mattack > attacked.Mdef ? attacker.Mattack - attacked.Mdef : 1;

            if (DBSpell != null)
            {
                float tPower = DBSpell.Damage;
                if (tPower > 30000)
                {
                    tPower = (tPower - 30000) / 100f;
                    nAtk = (int)(nAtk * tPower);
                }
                else
                    nAtk += (short)tPower;
            }

            int nDamage = nAtk > target.Owner.Status.MDefence ? nAtk - (int)target.Owner.Status.MDefence : 1;
            //int nDamage = (int)(((nAtk * 0.75) * (1 - (target.Owner.Status.MDefence * 0.01))) - target.Owner.Status.MagicDefence);


            if (target.Reborn == 1)
                nDamage = (int)Math.Round((double)(nDamage * 0.7));
            else if (target.Reborn == 2)
                nDamage = (int)Math.Round((double)(nDamage * 0.5));
            nDamage = (int)Math.Round((double)(nDamage * (1.00 - (target.Owner.Status.ItemBless * 0.01))));

            if (target.Owner.Status.Damage > 0)
                nDamage = (int)Math.Round(nDamage * Math.Max(1.00 - target.Owner.Status.Damage, 0.50));

            //     nDamage = (int)Calculate.Base.CalculateExtraAttack((uint)nDamage, player.Owner.Status.MagicDamageIncrease, target.Owner.Status.MagicDamageDecrease);

            if (nDamage > 0 && player.BlessTime > 0 && Role.Core.PercentSuccess(Global.LUCKY_TIME_CRIT_RATE_MAGIC))
            {
                nDamage *= 2;
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var msg = rec.GetStream();
                    player.SendString(msg, MsgStringPacket.StringID.Effect, true, "LuckyGuy");
                }
                //player.Owner.SendSysMesage("Lucky Strike: You had inflict double damage on the target.", MsgMessage.ChatMode.Action);
            }
            if (nDamage > target.Owner.Status.MagicDefence)
            {
                nDamage -= (int)target.Owner.Status.MagicDefence;
            }
            if (target.Name.Contains("[PM]") || target.Name.Contains("[GM]"))
            {
                nDamage = 1;
            }

            SpellObj.Damage = (uint)Math.Max(1, (int)(nDamage));//0.55
            if (CheckAttack.BlockRefect.CanUseReflect(player.Owner))
            {
                MsgSpellAnimation.SpellObj InRedirect;
                if (BackDmg.Calculate(player, target, DBSpell, SpellObj.Damage, out InRedirect))
                    SpellObj = InRedirect;
            }

        }

        public static void OnNpcs(Role.Player player, Role.SobNpc target, Database.MagicType.Magic DBSpell, out MsgSpellAnimation.SpellObj SpellObj)
        {
            SpellObj = new MsgSpellAnimation.SpellObj(target.UID, 0);

            SpellObj.Damage = player.Owner.Status.MagicAttack;
            SpellObj.Damage = (uint)Base.BigMulDiv((int)SpellObj.Damage, Client.GameClient.DefaultDefense, player.Owner.GetDefense2());
            SpellObj.Damage = Calculate.Base.CalculateExtraAttack(SpellObj.Damage, player.Owner.Status.MagicDamageIncrease, 0);

            if (target.ContainFlag(MsgUpdate.Flags.AzureShield))
                SpellObj.Damage = 100;
            if (SpellObj.Damage > 0 && player.BlessTime > 0 && Role.Core.PercentSuccess(Global.LUCKY_TIME_CRIT_RATE_MAGIC))
            {
                SpellObj.Damage *= 2;
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
