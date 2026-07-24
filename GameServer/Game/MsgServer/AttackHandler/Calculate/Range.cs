using System;
using System.Collections.Generic;

namespace COServer.Game.MsgServer.AttackHandler.Calculate
{
    public class Range
    {
        public static void OnMonster(Role.Player player, MsgMonster.MonsterRole monster, Database.MagicType.Magic DBSpell, out MsgSpellAnimation.SpellObj SpellObj, byte MultipleDamage = 0)
        {

            SpellObj = new MsgSpellAnimation.SpellObj(monster.UID, 0);
            if (monster.IsFloor)
            {
                SpellObj.Damage = 1;
                return;
            }
            int Damage = (int)Base.GetDamage(player.Owner.Status.MaxAttack, player.Owner.Status.MinAttack);

            Damage = (int)player.Owner.AjustAttack((uint)Damage);
            if (player.Level > monster.Level)
                Damage *= 2;
            if (MultipleDamage != 0)
            {
                Damage = Damage * MultipleDamage;
            }
            if (DBSpell != null)
            {
                Damage = Base.MulDiv((int)Damage, (int)((DBSpell != null) ? DBSpell.Damage > 30000 ? DBSpell.Damage -= 30000 : DBSpell.Damage : Program.ServerConfig.PhysicalDamage), 100);
            }
            else
            {
                Damage = Base.MulDiv((int)Damage, 100, 100);
                //  Damage = (int)Base.BigMulDiv((int)Damage, Client.GameClient.DefaultDefense2, player.Owner.GetDefense2());
            }

            //var rawDefense = monster.Family.Defense;

            //Damage = Math.Max(0, Damage - rawDefense);

            if (monster.Name.Contains("Guard"))
                monster.Family.Defense2 = 1000;
            Damage = (int)Base.BigMulDiv(Damage, monster.Family.Defense2, Client.GameClient.DefaultDefense);
            Damage = Base.MulDiv((int)Damage, (int)(100 - (int)(monster.Family.Dodge * 0.4)), 100);

            // if (monster.Boss == 0)
            {
                Damage = Base.CalcDamageUser2Monster(Damage, monster.Family.Defense, player.Level, monster.Level, true);
                Damage = Base.AdjustMinDamageUser2Monster(Damage, player.Owner);

            }

            Damage = (int)Calculate.Base.CalculateExtraAttack((uint)Damage, player.Owner.Status.PhysicalDamageIncrease, 0);
            //if (monster.Family.Defense2 == 0)
            //    Damage = 1;


            SpellObj.Damage = (uint)Math.Max(1, Damage);
            //  MyConsole.WriteLine("My Range Damage 1 -> Monster " + SpellObj.Damage.ToString());
#if TEST
            MyConsole.WriteLine("My Range Damage -> Monster " + SpellObj.Damage.ToString());
#endif

            if (monster.Boss == 0)
            {
                if (player.ContainFlag(MsgUpdate.Flags.Superman))
                    SpellObj.Damage *= 10;
            }
            if (player.ContainFlag(MsgUpdate.Flags.MagicShield) || player.ContainFlag(MsgUpdate.Flags.Shield))
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

            if (monster.Family.ID == 20211)
                SpellObj.Damage = 1;
            if (monster.Family.ID == 4145)
            {
                player.Owner.OnAutoAttack = false;
                SpellObj.Damage = 100000;
            }
            if (Damage > 0 && player.BlessTime > 0 && Role.Core.PercentSuccess(Global.LUCKY_TIME_CRIT_RATE_RANGED))
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
        public static void OnPlayer(Role.Player player, Role.Player target, Database.MagicType.Magic DBSpell, out MsgSpellAnimation.SpellObj SpellObj, int increasedmg = 0)
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
            var nDef = target.Owner.AjustDefense;




            int nDamage = target.Owner.AdjustWeaponDamage((int)(nAttack - nDef));
            if (Database.ItemType.IsBow(player.RightWeaponId) && nDamage >= 1000)// grbt el bow? asl kda el  grb dmg el archer kda xdd
                nDamage /= 5;
            //if (DBSpell != null)
            //{
            //    if (DBSpell.DamagePersent != 0)
            //        nDamage = (int)((nDamage * DBSpell.DamagePersent));
            //}
            if (target.Reborn == 1)
                nDamage = (int)Math.Round((double)(nDamage * 0.7));
            else if (target.Reborn == 2)
                nDamage = (int)Math.Round((double)(nDamage * 0.5));

            nDamage = (int)Math.Round((double)(nDamage * (1.00 - (target.Owner.Status.ItemBless * 0.01))));

            if (target.Owner.Status.Damage > 0)
                nDamage = (int)Math.Round(nDamage * Math.Max(1.00 - target.Owner.Status.Damage, 0.50));

            if (nDamage <= 0) nDamage = 7;

            //nDamage = (int)Calculate.Base.CalculateExtraAttack((uint)nDamage, player.Owner.Status.PhysicalDamageIncrease, target.Owner.Status.PhysicalDamageDecrease);

            if (nDamage <= 0)
                nDamage = 1;
            if (player.InUseIntensify)
            {
                if (player.IntensifyActive.AddMilliseconds(5300) < DateTime.Now)
                {
                    if (!player.ContainFlag(MsgUpdate.Flags.Intensify))
                        player.AddSpellFlag(MsgUpdate.Flags.Intensify, 20, true);

                    Game.MsgServer.MsgSpell ClientSpell;
                    if (player.Owner.MySpells.ClientSpells.TryGetValue((ushort)Role.Flags.SpellID.Intensify, out ClientSpell))
                    {
                        Dictionary<ushort, Database.MagicType.Magic> DBSpells;
                        if (Database.Server.Magic.TryGetValue((ushort)Role.Flags.SpellID.Intensify, out DBSpells))
                        {
                            Database.MagicType.Magic spell;

                            if (DBSpells.TryGetValue(ClientSpell.Level, out spell))
                            {
                                switch (ClientSpell.Level)
                                {
                                    //case 0:
                                    //    nDamage *= 1;
                                    //    break;

                                    //case 1:
                                    //    nDamage *= 1;
                                    //    break;
                                    //case 2:
                                    //    nDamage *= 2;
                                    //    break;
                                    //case 3:
                                    //    nDamage *= 3;
                                    //    break;
                                    default:
                                        nDamage *= 2;
                                        break;

                                }
                            }

                        }
                    }
                    if (player.ContainFlag(MsgUpdate.Flags.Intensify))
                        player.RemoveFlag(MsgUpdate.Flags.Intensify);
                    player.InUseIntensify = false;
                }

            }
            /*   if (player.InRapidFire)
               {
                   Game.MsgServer.MsgSpell ClientSpell;
                   if (player.Owner.MySpells.ClientSpells.TryGetValue((ushort)Role.Flags.SpellID.RapidFire, out ClientSpell))
                   {
                       Dictionary<ushort, Database.MagicType.Magic> DBSpells;
                       if (Database.Server.Magic.TryGetValue((ushort)Role.Flags.SpellID.RapidFire, out DBSpells))
                       {
                           Database.MagicType.Magic spell;
                           if (DBSpells.TryGetValue(ClientSpell.Level, out spell))
                           {
                               float tPower = Database.Server.Magic[(ushort)Role.Flags.SpellID.RapidFire][4].Damage;
                               if (tPower > 30000)
                               {
                                   tPower = (tPower - 30000) / 100f;
                                   nDamage = (int)(nDamage * tPower);
                               }
                               else
                                   nDamage += (short)tPower;
                              /* switch (ClientSpell.Level)
                               {

                                   case 0:
                                       nDamage += (int)(nDamage * 0.50f);
                                       break;
                                   case 1:
                                       nDamage += (int)(nDamage * 0.70f);
                                       break;
                                   case 2:
                                       nDamage += (int)(nDamage * 0.90f);
                                       break;
                                   case 3:
                                       nDamage += (int)(nDamage * 1.0f);
                                       break;
                                   case 4:
                                       nDamage += (int)(nDamage * 1.2f);
                                       break;
                                   case 5:
                                       nDamage += (int)(nDamage * 1.5f);
                                       break;
                               }*/
            /*  }
          }
      }
      player.InRapidFire = false;

  }
  */
            /*  if (player.ContainFlag(MsgUpdate.Flags.Stigma) && !player.OnTransform)
              {
                 // if (nDamage > player.StagimaAttack)
                  {
                      nDamage += (int)(nDamage *1.15); //player.StagimaAttack;

                  }
              }*/
            if (player.ContainFlag(MsgUpdate.Flags.Stigma) && !player.OnTransform)
            {
                float tPower = Database.Server.Magic[1095][4].Damage;
                if (tPower > 30000)
                {
                    tPower = (tPower - 30000) / 100f;
                    nDamage = (int)(nDamage * tPower);
                }
                else
                    nDamage += (short)tPower;

            }
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



            if (nDamage > 0 && player.BlessTime > 0 && Role.Core.PercentSuccess(Global.LUCKY_TIME_CRIT_RATE_RANGED))
            {
                nDamage *= 2;
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var msg = rec.GetStream();
                    player.SendString(msg, MsgStringPacket.StringID.Effect, true, "LuckyGuy");
                }
                //player.Owner.SendSysMesage("Lucky Strike: You had inflict double damage on the target.", MsgMessage.ChatMode.Action);
            }
            if (target.Owner.Equipment.ShieldID != 0)
            {
                //SpellObj.Effect |= MsgAttackPacket.AttackEffect.Block;
                if (nDamage > target.Owner.Status.ShieldDefenece)
                {
                    nDamage -= (int)target.Owner.Status.ShieldDefenece;
                }

            }
            SpellObj.Damage = (uint)Math.Max(1, nDamage);

            MsgSpellAnimation.SpellObj InRedirect;
            if (BackDmg.Calculate(player, target, DBSpell, SpellObj.Damage, out InRedirect))
                SpellObj = InRedirect;
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

            if (Damage > 0 && player.BlessTime > 0 && Role.Core.PercentSuccess(Global.LUCKY_TIME_CRIT_RATE_RANGED))
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

    }
}
