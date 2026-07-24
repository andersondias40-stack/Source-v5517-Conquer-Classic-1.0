using System;
using System.Collections.Generic;

namespace COServer.Game.MsgServer.AttackHandler.Updates
{
    public class GetWeaponSpell
    {
        public static void CheckExtraEffects(Client.GameClient client, ServerSockets.Packet stream)
        {
            if (client.Equipment.RingEffect != Role.Flags.ItemEffect.None || client.Equipment.RightWeaponEffect == Role.Flags.ItemEffect.Stigma)
            {
                if (Calculate.Base.Success(20))
                {
                    if (!client.Player.ContainFlag(MsgUpdate.Flags.Stigma))
                    {
                        MsgSpellAnimation MsgSpell = new MsgSpellAnimation(client.Player.UID
                   , 0, client.Player.X, client.Player.Y, (ushort)Role.Flags.SpellID.Stigma
                   , 4, 0);


                        client.Player.AddSpellFlag(MsgUpdate.Flags.Stigma, 20, true);
                        MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(client.Player.UID, 0));


                        MsgSpell.SetStream(stream);
                        MsgSpell.Send(client);
                    }
                }
            }
            if (client.Equipment.NecklaceEffect != Role.Flags.ItemEffect.None || client.Equipment.RightWeaponEffect == Role.Flags.ItemEffect.Shield)
            {
                if (Calculate.Base.Success(20))
                {
                    if (!client.Player.ContainFlag(MsgUpdate.Flags.Shield))
                    {
                        MsgSpellAnimation MsgSpell = new MsgSpellAnimation(client.Player.UID
                   , 0, client.Player.X, client.Player.Y, (ushort)Role.Flags.SpellID.Shield
                   , 4, 0);


                        client.Player.AddSpellFlag(MsgUpdate.Flags.Shield, 15, true);
                        MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(client.Player.UID, 0));


                        MsgSpell.SetStream(stream);
                        MsgSpell.Send(client);
                    }
                }
            }

        }
        public unsafe static bool Check(InteractQuery Attack, ServerSockets.Packet stream, Client.GameClient client, Role.IMapObj Target)
        {
            if (Calculate.Base.Success(35))
            {
                if (client.Equipment.RightWeapon != 0)
                {
                    if ((client.Equipment.RightWeaponEffect != Role.Flags.ItemEffect.None
                        || client.Equipment.LeftWeaponEffect != Role.Flags.ItemEffect.None)
                        && Target.ObjType != Role.MapObjectType.SobNpc)
                    {
                        if (Calculate.Base.Success(20) && !client.Player.ContainFlag(MsgUpdate.Flags.FatalStrike))
                        {
                            client.Player.AttackStamp = new Time32();

                            InteractQuery AttackPaket = new InteractQuery();

                            AttackPaket.OpponentUID = Attack.OpponentUID;
                            AttackPaket.UID = Attack.UID;
                            AttackPaket.X = Target.X;
                            AttackPaket.Y = Target.Y;

                            if (client.Equipment.RightWeaponEffect == Role.Flags.ItemEffect.Poison
                                || client.Equipment.LeftWeaponEffect == Role.Flags.ItemEffect.Poison)
                                AttackPaket.SpellID = (ushort)Role.Flags.SpellID.Poison;

                            else if (client.Equipment.RightWeaponEffect == Role.Flags.ItemEffect.MP)
                                AttackPaket.SpellID = (ushort)Role.Flags.SpellID.EffectMP;

                            else if (client.Equipment.RingEffect == Role.Flags.ItemEffect.MP)
                                AttackPaket.SpellID = (ushort)Role.Flags.SpellID.EffectMP;


                            client.Player.RandomSpell = AttackPaket.SpellID;

                            AttackPaket.AtkType = MsgAttackPacket.AttackID.Magic;

                            MsgServer.MsgAttackPacket.ProcescMagic(client, stream, AttackPaket);

                            return true;
                        }

                    }

                }
            }
            if (/*Calculate.Base.Success(35) &&*/ DateTime.Now > client.WepSkill.AddSeconds(2))//1. Weapon skills aren't activating much now.
            {
            jump:
                if (client.Equipment.RightWeapon != 0)
                {
                    {
                        uint rightWeapon = client.Equipment.RightWeapon;

                        ushort wep1subyte = (ushort)(rightWeapon / 1000), wep2subyte = 0;

                        bool doWep1Spell = false, doWep2Spell = false;

                        ushort wep1spellid = 0, wep2spellid = 0;

                        if (wep1subyte == 421)
                            wep1subyte--;

                        if (Database.Server.WeaponSpells.ContainsKey(wep1subyte))
                            wep1spellid = Database.Server.WeaponSpells[wep1subyte];

                        Database.MagicType.Magic wep1spell = null, wep2spell = null;

                        if (client.MySpells.ClientSpells.ContainsKey(wep1spellid) && Database.Server.Magic.ContainsKey(wep1spellid))
                        {
                            wep1spell = Database.Server.Magic[wep1spellid][client.MySpells.ClientSpells[wep1spellid].Level];
                            if (wep1spell != null)
                            {
                                if ((wep1spell.Rate > 20))
                                    doWep1Spell = Calculate.Base.Success(wep1spell.Rate - 10);
                                else
                                    doWep1Spell = Calculate.Base.Success(wep1spell.Rate);
                            }
                        }

                        if (!doWep1Spell)
                        {
                            if (client.Equipment.LeftWeapon != 0)
                            {
                                uint leftWeapon = client.Equipment.LeftWeapon;
                                wep2subyte = (ushort)(leftWeapon / 1000);
                                if (Database.Server.WeaponSpells.ContainsKey(wep2subyte))
                                    wep2spellid = Database.Server.WeaponSpells[wep2subyte];

                                if (client.MySpells.ClientSpells.ContainsKey(wep2spellid) && Database.Server.Magic.ContainsKey(wep2spellid))
                                {
                                    wep2spell = Database.Server.Magic[wep2spellid][client.MySpells.ClientSpells[wep2spellid].Level];
                                    if (wep2spell != null)
                                    {
                                        if (wep2spell.Rate > 20)
                                            doWep2Spell = Calculate.Base.Success(wep2spell.Rate - 10);
                                        else
                                            doWep2Spell = Calculate.Base.Success(wep2spell.Rate);
                                    }
                                }


                                //else
                                //{
                                //    doWep1Spell = true;
                                //}
                            }
                            //else
                            //{
                            //    doWep1Spell = true;
                            //}

                        }

                        if (!client.Player.OnTransform)
                        {
                            if (doWep1Spell)
                            {
                                if (!client.MySpells.ClientSpells.ContainsKey(wep1spellid))
                                    return false;

                                if (client.Player.ContainFlag(MsgUpdate.Flags.FatalStrike))
                                {
                                    client.Shift(Target.X, Target.Y, stream);
                                }

                                if (Target.ObjType != Role.MapObjectType.Monster)
                                    MsgServer.MsgAttackPacket.CreateAutoAtack(Attack, client);
                                else
                                {
                                    MsgMonster.MonsterRole attacked = Target as MsgMonster.MonsterRole;
                                    if (attacked.Family.ID != 4145)
                                        MsgServer.MsgAttackPacket.CreateAutoAtack(Attack, client);

                                }
                                InteractQuery AttackPaket = new InteractQuery();
                                AttackPaket.OpponentUID = Attack.OpponentUID;
                                AttackPaket.UID = Attack.UID;
                                AttackPaket.X = Target.X;
                                AttackPaket.Y = Target.Y;
                                AttackPaket.SpellID = wep1spellid;
                                client.Player.RandomSpell = wep1spellid;
                                AttackPaket.AtkType = MsgAttackPacket.AttackID.Magic;
                                MsgServer.MsgAttackPacket.ProcescMagic(client, stream, AttackPaket, true);
                                //if (Target is MsgMonster.MonsterRole mo)
                                //{
                                //    if (mo.Family.ID == 41299)
                                //    {
                                //        goto jump;
                                //    }
                                //}
                                client.WepSkill = DateTime.Now;
                                return true;
                            }
                            if (doWep2Spell)
                            {
                                if (!client.MySpells.ClientSpells.ContainsKey(wep2spellid))
                                    return false;

                                if (client.Player.ContainFlag(MsgUpdate.Flags.FatalStrike))
                                {
                                    if (Target.ObjType == Role.MapObjectType.Monster)
                                        client.Shift(Target.X, Target.Y, stream);
                                }

                                if (Target.ObjType != Role.MapObjectType.Monster)
                                    MsgServer.MsgAttackPacket.CreateAutoAtack(Attack, client);
                                else
                                {
                                    MsgMonster.MonsterRole attacked = Target as MsgMonster.MonsterRole;
                                    if (attacked.Family.ID != 4145)
                                        MsgServer.MsgAttackPacket.CreateAutoAtack(Attack, client);

                                }

                                InteractQuery AttackPaket = new InteractQuery();
                                AttackPaket.OpponentUID = Attack.OpponentUID;
                                AttackPaket.UID = Attack.UID;
                                AttackPaket.X = Target.X;
                                AttackPaket.Y = Target.Y;
                                AttackPaket.SpellID = wep2spellid;
                                AttackPaket.AtkType = MsgAttackPacket.AttackID.Magic;
                                client.Player.RandomSpell = wep2spellid;
                                MsgServer.MsgAttackPacket.ProcescMagic(client, stream, AttackPaket, true);
                                //if (Target is MsgMonster.MonsterRole mo)
                                //{
                                //     if (mo.Family.ID == 41299)
                                //     {
                                //         goto jump;
                                //     }
                                // }
                                client.WepSkill = DateTime.Now;
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}
