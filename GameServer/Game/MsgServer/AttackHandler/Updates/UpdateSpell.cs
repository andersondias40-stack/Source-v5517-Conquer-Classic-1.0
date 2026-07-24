using COServer.Game.MsgServer.AttackHandler.Calculate;
using System;
using System.Collections.Generic;
using static COServer.Game.MsgServer.AttackHandler.Calculate.Base;

namespace COServer.Game.MsgServer.AttackHandler.Updates
{
    public class UpdateSpell
    {
        public unsafe static void CheckUpdate(ServerSockets.Packet stream, Client.GameClient client, InteractQuery Attack, uint Damage, Dictionary<ushort, Database.MagicType.Magic> DBSpells)
        {
            // Removido o retorno imediato se o dano for 0
            // if (Damage == 0) return; 
            if (Attack.SpellID == 30000)
                return;

            if (DBSpells != null)
            {
                MsgSpell ClientSpell;
                if (client.MySpells.ClientSpells.TryGetValue(Attack.SpellID, out ClientSpell))
                {
                    ushort firstlevel = ClientSpell.Level;

                    if (ClientSpell.Level < DBSpells.Count - 1)
                    {
                        // Tratamento de dano específico para Phoenix
                        if (ClientSpell.ID == (ushort)Role.Flags.SpellID.Phoenix)
                        {
                            Damage = 1; // Considera o dano como 1 para experiência
                        }
                        else
                        {
                            if (client.GemValues(Role.Flags.Gem.NormalMoonGem) > 0)
                            {
                                Damage += Damage * client.GemValues(Role.Flags.Gem.NormalMoonGem) / 100;
                            }

                            switch (ClientSpell.ID)
                            {
                                case (ushort)Role.Flags.SpellID.Tornado:
                                    Damage *= 100;
                                    break;
                                case (ushort)Role.Flags.SpellID.Meditation:
                                    Damage += 1000;
                                    break;
                                case (ushort)Role.Flags.SpellID.Stigma:
                                case (ushort)Role.Flags.SpellID.DivineHare:
                                case (ushort)Role.Flags.SpellID.Penetration:
                                case (ushort)Role.Flags.SpellID.Intensify:
                                case (ushort)Role.Flags.SpellID.Golem:
                                case (ushort)Role.Flags.SpellID.NightDevil:
                                case (ushort)Role.Flags.SpellID.WaterElf:
                                case (ushort)Role.Flags.SpellID.SummonGuard:
                                    Damage = 1; // Para outras skills, mantém o dano como 1 se necessário
                                    break;
                                default:
                                    Damage /= 3;
                                    break;
                            }
                        }

                        if (client.Player.Level >= DBSpells[ClientSpell.Level].NeedLevel)
                        {
                            ClientSpell.Experience += (int)(Damage * Program.ServerConfig.ExpRateSpell);
                            if (ClientSpell.Experience > DBSpells[ClientSpell.Level].Experience)
                            {
                                ClientSpell.PreviousLevel = (byte)ClientSpell.Level;
                                ClientSpell.Level++;
                                ClientSpell.Experience = 0;
                            }
                            if (ClientSpell.PreviousLevel != 0 && ClientSpell.PreviousLevel >= ClientSpell.Level)
                            {
                                ClientSpell.Level = ClientSpell.PreviousLevel;
                            }
                            try
                            {
                                if (ClientSpell.Level > firstlevel)
                                    client.SendSysMesage("You have just leveled your skill " + DBSpells[ClientSpell.Level].Name + ".", MsgMessage.ChatMode.System);
                            }
                            catch (Exception e) { Console.WriteLine(e.ToString()); }
                            client.Send(stream.SpellCreate(ClientSpell));
                        }
                    }
                }
            }
            else if (Attack.AtkType == MsgAttackPacket.AttackID.Physical || Attack.AtkType == MsgAttackPacket.AttackID.Archer || Attack.AtkType == MsgAttackPacket.AttackID.Magic)
            {
                uint ProfRightWeapon = client.Equipment.RightWeapon / 1000;
                uint PorfLeftWeapon = client.Equipment.LeftWeapon / 1000;
                if (ProfRightWeapon != 0)
                    client.MyProfs.CheckUpdate(ProfRightWeapon, Damage, stream);

                if (PorfLeftWeapon != 0)
                    client.MyProfs.CheckUpdate(PorfLeftWeapon, Damage / 2, stream);
            }
        }
    }
}
