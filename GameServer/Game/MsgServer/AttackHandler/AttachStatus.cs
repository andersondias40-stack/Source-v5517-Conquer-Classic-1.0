using System;
using System.Collections.Generic;

namespace COServer.Game.MsgServer.AttackHandler
{
    public class AttachStatus
    {
        public unsafe static void Execute(Client.GameClient user, InteractQuery Attack, ServerSockets.Packet stream,
            Dictionary<ushort, Database.MagicType.Magic> DBSpells)
        {
            Database.MagicType.Magic DBSpell;
            MsgSpell ClientSpell;
            if (CheckAttack.CanUseSpell.Verified(Attack, user, DBSpells, out ClientSpell, out DBSpell))
            {
                switch (ClientSpell.ID)
                {
                    case (ushort)Role.Flags.SpellID.Intensify:
                        {
                            user.Player.IntensifyActive = DateTime.Now;
                            Attack.SpellID = ClientSpell.ID;
                            Attack.SpellLevel = ClientSpell.Level;
                            user.Player.View.SendView(stream.InteractionCreate(&Attack), true);
                            user.Player.IntensifyStamp = Time32.Now;
                            user.Player.InUseIntensify = true;
                            user.Player.IntensifyDamage = (int)DBSpell.Damage;
                            Updates.UpdateSpell.CheckUpdate(stream, user, Attack, DBSpell.Duration, DBSpells);

                            break;
                        }
                    case (ushort)Role.Flags.SpellID.PoisonStar:
                        {
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                            , 0, Attack.X, Attack.Y, ClientSpell.ID
                            , ClientSpell.Level, ClientSpell.UseSpellSoul);


                            Role.IMapObj target;
                            if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Player))
                            {

                                Role.Player attacked = target as Role.Player;
                                if (!CheckAttack.CanAttackPlayer.Verified(user, attacked, DBSpell))
                                    return;
                                var rate = (((user.Player.BattlePower - attacked.BattlePower) + 10) * 7);
                                if ((attacked.BattlePower - user.Player.BattlePower) >= 10)
                                    rate = 10; // Pity Success rate.
                                if (user.Player.BattlePower >= attacked.BattlePower) rate = 100;
                                if (Calculate.Base.Success(rate))
                                {
                                    attacked.AddSpellFlag(MsgUpdate.Flags.PoisonStar, (int)DBSpell.Duration, true);
                                    MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(attacked.UID, DBSpell.Duration));
                                }
                                else
                                {

                                    var clientobj = new MsgSpellAnimation.SpellObj(attacked.UID, MsgSpell.SpellID);
                                    clientobj.Hit = 0;
                                    MsgSpell.Targets.Enqueue(clientobj);
                                }

                            }
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);

                            Updates.UpdateSpell.CheckUpdate(stream, user, Attack, 250, DBSpells);

                            break;
                        }
                    case (ushort)Role.Flags.SpellID.Stigma:
                        {
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID,
                                0, Attack.X, Attack.Y, ClientSpell.ID,
                                ClientSpell.Level, ClientSpell.UseSpellSoul);

                            if (user.Player.UID == Attack.OpponentUID)
                            {
                                if (!user.Player.ContainFlag(MsgUpdate.Flags.Shield))
                                {
                                    user.Player.AddSpellFlag(MsgUpdate.Flags.Stigma, (int)DBSpell.Duration, true);
                                    user.SendSysMesage($"Your attack will increase for the next {(int)DBSpell.Duration} seconds.", MsgMessage.ChatMode.TopLeftSystem);
                                }
                            }
                            else
                            {
                                Role.IMapObj target;
                                if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Monster))
                                {
                                    MsgMonster.MonsterRole attacked = target as MsgMonster.MonsterRole;
                                    attacked.AddSpellFlag(MsgUpdate.Flags.Stigma, (int)DBSpell.Duration, true);
                                }
                                else if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Player))
                                {
                                    Role.Player attacked = target as Role.Player;
                                    if (!attacked.ContainFlag(MsgUpdate.Flags.Shield))
                                    {
                                        attacked.AddSpellFlag(MsgUpdate.Flags.Stigma, (int)DBSpell.Duration, true);
                                        attacked.Owner.SendSysMesage($"Your attack will increase for the next {(int)DBSpell.Duration} seconds.", MsgMessage.ChatMode.TopLeftSystem);
                                    }
                                }
                            }

                            Updates.UpdateSpell.CheckUpdate(stream, user, Attack, DBSpell.Duration, DBSpells);
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);

                            // 🔊 Envia o som junto com a skill Stigma
                            Game.MsgServer.MsgStringPacket soundPacket = new Game.MsgServer.MsgStringPacket();
                            soundPacket.ID = MsgServer.MsgStringPacket.StringID.Sound;
                            soundPacket.Strings = new string[2];
                            soundPacket.Strings[0] = "sound\\magic020.wav"; // caminho do som
                            soundPacket.Strings[1] = "1"; // flag de execução
                            var streamSound = stream.StringPacketCreate(soundPacket);
                            user.Send(streamSound);

                            break;
                        }
                    case (ushort)Role.Flags.SpellID.MagicShield:
                        {
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                            , 0, Attack.X, Attack.Y, ClientSpell.ID
                            , ClientSpell.Level, ClientSpell.UseSpellSoul);

                            if (user.Player.UID == Attack.OpponentUID)
                            {
                                if (!user.Player.ContainFlag(MsgUpdate.Flags.Shield))
                                {
                                    user.Player.AddSpellFlag(MsgUpdate.Flags.Shield, (int)DBSpell.Duration, true);
                                    user.SendSysMesage($"Your defence will increase for the next {(int)DBSpell.Duration} seconds.", MsgMessage.ChatMode.TopLeftSystem);
                                    user.Player.AzureShieldLevel = (byte)ClientSpell.Level;
                                    switch (ClientSpell.Level)
                                    {
                                        case 0:
                                            user.Player.AzureShieldDefence = 100;
                                            break;
                                        case 1:
                                            user.Player.AzureShieldDefence = 250;
                                            break;
                                        case 2:
                                            user.Player.AzureShieldDefence = 500;
                                            break;
                                        case 3:
                                            user.Player.AzureShieldDefence = 800;
                                            break;
                                        case 4:
                                            user.Player.AzureShieldDefence = 1000;
                                            break;
                                    }
                                    MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID, 0));
                                    MsgSpell.TargetSend(user, Attack, DBSpell.Duration, DBSpells, false, stream);

                                }
                            }
                            else
                            {
                                Role.IMapObj target;
                                if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Monster))
                                {
                                    MsgMonster.MonsterRole attacked = target as MsgMonster.MonsterRole;
                                    if (!attacked.ContainFlag(MsgUpdate.Flags.Shield))
                                    {
                                        attacked.AddSpellFlag(MsgUpdate.Flags.Shield, (int)DBSpell.Duration, true);
                                        MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(attacked.UID, 0));
                                        MsgSpell.TargetSend(user, Attack, DBSpell.Duration, DBSpells, false, stream);
                                    }
                                }
                                else if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Player))
                                {
                                    Role.Player attacked = target as Role.Player;
                                    if (!attacked.ContainFlag(MsgUpdate.Flags.Shield))
                                    {
                                        attacked.AddSpellFlag(MsgUpdate.Flags.Shield, (int)DBSpell.Duration, true);
                                        attacked.Owner.SendSysMesage($"Your attack will increase for the next {(int)DBSpell.Duration} seconds.", MsgMessage.ChatMode.TopLeftSystem);
                                        attacked.AzureShieldLevel = (byte)ClientSpell.Level;
                                        switch (ClientSpell.Level)
                                        {
                                            case 0:
                                                attacked.AzureShieldDefence = 100;
                                                break;
                                            case 1:
                                                attacked.AzureShieldDefence = 250;
                                                break;
                                            case 2:
                                                attacked.AzureShieldDefence = 500;
                                                break;
                                            case 3:
                                                attacked.AzureShieldDefence = 800;
                                                break;
                                            case 4:
                                                attacked.AzureShieldDefence = 1000;
                                                break;
                                        }
                                        MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(attacked.UID, 0));
                                        MsgSpell.TargetSend(user, Attack, DBSpell.Duration, DBSpells, false, stream);
                                    }
                                }
                            }

                            //Updates.UpdateSpell.CheckUpdate(stream, user, Attack, DBSpell.Duration, DBSpells);
                            //MsgSpell.SetStream(stream);
                            //MsgSpell.Send(user);

                            break;
                        }
                    case (ushort)Role.Flags.SpellID.StarofAccuracy:
                        {
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                            , 0, Attack.X, Attack.Y, ClientSpell.ID
                            , ClientSpell.Level, ClientSpell.UseSpellSoul);

                            if (user.Player.UID == Attack.OpponentUID)
                            {
                                if (!user.Player.ContainFlag(MsgUpdate.Flags.Shield))
                                {
                                    user.Player.AddSpellFlag(MsgUpdate.Flags.StarOfAccuracy, (int)DBSpell.Duration, true);
                                    MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID, 0));
                                }
                            }
                            else
                            {
                                Role.IMapObj target;
                                //if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Monster))
                                //{
                                //    MsgMonster.MonsterRole attacked = target as MsgMonster.MonsterRole;
                                //    attacked.AddSpellFlag(MsgUpdate.Flags.StarOfAccuracy, (int)DBSpell.Duration, true);
                                //    MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(attacked.UID, 0));
                                //}
                                //else 
                                if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Player))
                                {

                                    Role.Player attacked = target as Role.Player;
                                    if (!attacked.ContainFlag(MsgUpdate.Flags.Shield))
                                    {
                                        attacked.AddSpellFlag(MsgUpdate.Flags.StarOfAccuracy, (int)DBSpell.Duration, true);
                                        MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(attacked.UID, 0));
                                    }

                                }
                            }
                            Updates.UpdateSpell.CheckUpdate(stream, user, Attack, DBSpell.Duration, DBSpells);
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);

                            break;
                        }
                    case (ushort)Role.Flags.SpellID.Invisibility:
                        {
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                            , 0, Attack.X, Attack.Y, ClientSpell.ID
                            , ClientSpell.Level, ClientSpell.UseSpellSoul);

                            if (user.Player.UID == Attack.OpponentUID)
                            {
                                user.Player.AddSpellFlag(MsgUpdate.Flags.Invisibility, (int)DBSpell.Duration, true);
                                MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID, 0));
                            }
                            else
                            {
                                Role.IMapObj target;
                                //if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Monster))
                                //{
                                //    MsgMonster.MonsterRole attacked = target as MsgMonster.MonsterRole;
                                //    attacked.AddSpellFlag(MsgUpdate.Flags.Invisibility, (int)DBSpell.Duration, true);
                                //    MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(attacked.UID, 0));
                                //}
                                //else 
                                if (user.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.Player))
                                {

                                    Role.Player attacked = target as Role.Player;
                                    if (!attacked.ContainFlag(MsgUpdate.Flags.Shield))
                                    {
                                        attacked.AddSpellFlag(MsgUpdate.Flags.Invisibility, (int)DBSpell.Duration, true);
                                        MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(attacked.UID, 0));
                                    }

                                }
                            }

                            Updates.UpdateSpell.CheckUpdate(stream, user, Attack, DBSpell.Duration, DBSpells);
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);
                            break;
                        }

                    case (ushort)Role.Flags.SpellID.Shield:
                        {
                            if (user.Player.ContainFlag(MsgUpdate.Flags.XPList) == false)
                                break;
                            if (user.Player.ContainFlag(MsgUpdate.Flags.Superman))//11. Disable Superman while having XP Shield.
                                break;
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                            , 0, Attack.X, Attack.Y, ClientSpell.ID
                            , ClientSpell.Level, ClientSpell.UseSpellSoul);

                            user.Player.RemoveFlag(MsgUpdate.Flags.XPList);
                            if (!user.Player.ContainFlag(MsgUpdate.Flags.Shield))
                                user.Player.AddFlag(MsgUpdate.Flags.Shield, (int)DBSpell.Duration, true);
                            // user.Player.AzureShieldDefence = 1000;
                            MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID, DBSpell.Duration));
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);

                            break;
                        }
                    case (ushort)Role.Flags.SpellID.Accuracy:
                        {
                            if (user.Player.ContainFlag(MsgUpdate.Flags.XPList) == false)
                                break;
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                            , 0, Attack.X, Attack.Y, ClientSpell.ID
                            , ClientSpell.Level, ClientSpell.UseSpellSoul);

                            user.Player.RemoveFlag(MsgUpdate.Flags.XPList);
                            user.Player.AddFlag(MsgUpdate.Flags.StarOfAccuracy, (int)DBSpell.Duration, true);

                            MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID, 0));
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);


                            break;
                        }
                    case (ushort)Role.Flags.SpellID.XpFly:
                        {
                            //if (Game.MsgTournaments.MsgSchedules.CurrentTournament.Process == MsgTournaments.ProcesType.Alive)
                            //{
                            //    if (Game.MsgTournaments.MsgSchedules.CurrentTournament.Type == MsgTournaments.TournamentType.TreasureThief)
                            //    {
                            //        var tournament = Game.MsgTournaments.MsgSchedules.CurrentTournament as Game.MsgTournaments.MsgTreasureChests;
                            //        if (tournament.InTournament(user))
                            //        {
                            //            return;
                            //        }
                            //    }
                            //}
                            if ((MsgTournaments.MsgSchedules.CurrentTournament.Process == MsgTournaments.ProcesType.Alive))
                            {
                                if (MsgTournaments.MsgSchedules.CurrentTournament.InTournament(user))
                                {
                                    return;
                                }
                            }
                            if (user.Player.ContainFlag(MsgUpdate.Flags.XPList) == false)
                                break;
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                            , 0, Attack.X, Attack.Y, ClientSpell.ID
                            , ClientSpell.Level, ClientSpell.UseSpellSoul);

                            if (user.Player.OnTransform
                                || user.Player.ContainFlag(MsgUpdate.Flags.Ride)
                                || user.Player.ContainFlag(MsgUpdate.Flags.Shield))
                            {
                                //user.SendSysMesage("You can't use this skill right now!");
                                break;
                            }
                            if (user.Player.ContainFlag(MsgUpdate.Flags.Fly))
                                user.Player.UpdateFlag(MsgUpdate.Flags.Fly, (int)DBSpell.Duration, true, 0);
                            else
                                user.Player.AddFlag(MsgUpdate.Flags.Fly, (int)DBSpell.Duration, true);

                            user.Player.RemoveFlag(MsgUpdate.Flags.XPList);


                            MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID, DBSpell.Duration));
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);

                            break;
                        }
                    case (ushort)Role.Flags.SpellID.Fly:
                        {
                            //if (Game.MsgTournaments.MsgSchedules.CurrentTournament.Process == MsgTournaments.ProcesType.Alive)
                            //{
                            //    if (Game.MsgTournaments.MsgSchedules.CurrentTournament.Type == MsgTournaments.TournamentType.TreasureThief)
                            //    {
                            //    var tournament = Game.MsgTournaments.MsgSchedules.CurrentTournament as Game.MsgTournaments.MsgTreasureChests;
                            //        if (tournament.InTournament(user))
                            //        {
                            //            return;
                            //        }
                            //    }
                            //}
                            if ((MsgTournaments.MsgSchedules.CurrentTournament.Process == MsgTournaments.ProcesType.Alive))
                            {
                                if (MsgTournaments.MsgSchedules.CurrentTournament.InTournament(user))
                                {
                                    return;
                                }
                            }
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                            , 0, Attack.X, Attack.Y, ClientSpell.ID
                            , ClientSpell.Level, ClientSpell.UseSpellSoul);

                            if (user.Player.OnTransform || user.Player.ContainFlag(MsgUpdate.Flags.Ride) || user.Player.ContainFlag(MsgUpdate.Flags.Shield))
                            {
                                // user.SendSysMesage("You can't use this skill right now!");
                                break;
                            }
                            if (user.Player.Level >= 100 && ClientSpell.Level == 0)
                            {
                                ClientSpell.Level = 1;
                                if (user.Player.ContainFlag(MsgUpdate.Flags.Fly))
                                    user.Player.UpdateFlag(MsgUpdate.Flags.Fly, 60, true, 0);
                                else
                                    user.Player.AddFlag(MsgUpdate.Flags.Fly, 60, true);

                            }
                            else
                            {
                                if (user.Player.ContainFlag(MsgUpdate.Flags.Fly))
                                    user.Player.UpdateFlag(MsgUpdate.Flags.Fly, (int)DBSpell.Duration, true, 0);
                                else
                                    user.Player.AddFlag(MsgUpdate.Flags.Fly, (int)DBSpell.Duration, true);

                            }

                            MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID, DBSpell.Duration));
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);

                            break;
                        }
                    case (ushort)Role.Flags.SpellID.Bless:
                        {
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                            , 0, Attack.X, Attack.Y, ClientSpell.ID
                            , ClientSpell.Level, ClientSpell.UseSpellSoul);


                            user.Player.AddFlag(MsgUpdate.Flags.CastPray, Role.StatusFlagsBigVector32.PermanentFlag, true);

                            MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID, 0));
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);


                            break;
                        }
                    case (ushort)Role.Flags.SpellID.FatalStrike:
                        {
                            if (user.Player.Map == 1038 || user.Player.Map == 3868)
                            {
                                //   user.SendSysMesage("You can't use this skill right now!");
                                break;

                            }
                            if (user.Player.ContainFlag(MsgUpdate.Flags.XPList) == false)
                                break;
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                            , 0, Attack.X, Attack.Y, ClientSpell.ID
                            , ClientSpell.Level, ClientSpell.UseSpellSoul);

                            user.Player.RemoveFlag(MsgUpdate.Flags.XPList);
                            user.Player.OpenXpSkill(MsgUpdate.Flags.FatalStrike, 60);

                            MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID, 0));
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);


                            break;
                        }
                    case (ushort)Role.Flags.SpellID.Cyclone:
                        {
                            if (user.Player.ContainFlag(MsgUpdate.Flags.XPList) == false || user.Player.ContainFlag(MsgUpdate.Flags.Shield)
                                || user.Player.Map == 1780)
                                break;
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                             , 0, Attack.X, Attack.Y, ClientSpell.ID
                             , ClientSpell.Level, ClientSpell.UseSpellSoul);

                            user.Player.RemoveFlag(MsgUpdate.Flags.XPList);
                            user.Player.OpenXpSkill(MsgUpdate.Flags.Cyclone, 30);

                            MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID, 0));
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);


                            break;
                        }
                    case (ushort)Role.Flags.SpellID.Superman:
                        {
                            if (user.Player.ContainFlag(MsgUpdate.Flags.XPList) == false /*|| user.Player.ContainFlag(MsgUpdate.Flags.Shield)*/)
                                break;
                            MsgSpellAnimation MsgSpell = new MsgSpellAnimation(user.Player.UID
                          , 0, Attack.X, Attack.Y, ClientSpell.ID
                           , ClientSpell.Level, ClientSpell.UseSpellSoul);

                            user.Player.RemoveFlag(MsgUpdate.Flags.XPList);
                            user.Player.OpenXpSkill(MsgUpdate.Flags.Superman, 30);

                            MsgSpell.Targets.Enqueue(new MsgSpellAnimation.SpellObj(user.Player.UID, 0));
                            MsgSpell.SetStream(stream);
                            MsgSpell.Send(user);

                            break;
                        }
                }
            }
        }
    }
}
