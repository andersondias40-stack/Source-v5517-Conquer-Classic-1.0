using System;
using System.Linq;
using System.Collections.Generic;

namespace COServer.Game.MsgServer.AttackHandler.CheckAttack
{
    public class CanUseSpell
    {
        private static ushort[] MagicSkills = new ushort[]
        {//Magic spells in TG should 
            1000,
            1001,
            1002
        };
        public unsafe static bool Verified(InteractQuery Attack, Client.GameClient client, Dictionary<ushort, Database.MagicType.Magic> DBSpells
            , out MsgSpell ClientSpell, out Database.MagicType.Magic Spell)
        {
            try
            {

                //anti proxy --------------------------
                if (Database.MagicType.RandomSpells.Contains((Role.Flags.SpellID)Attack.SpellID))
                {
                    if (client.Player.RandomSpell != Attack.SpellID)
                    {
                        ClientSpell = default(MsgSpell);
                        Spell = default(Database.MagicType.Magic);
                        return false;
                    }
                    client.Player.RandomSpell = 0;
                }
                if (Attack.UID < 1000000)
                {
                    ClientSpell = default(MsgSpell);
                    Spell = default(Database.MagicType.Magic); return true;
                }
                //-------------------------------------
                if (client.MySpells.ClientSpells.TryGetValue(Attack.SpellID, out ClientSpell))
                {
                    if (DBSpells.TryGetValue(ClientSpell.Level, out Spell))
                    {
                        if (Program.SsFbMap.Contains(client.Player.Map))
                        {
                            if (Spell.ID != 1045 && Spell.ID != 1046)
                            {
                                client.SendSysMesage("You have to use manual linear skills(FastBlade/ScentSword)", MsgMessage.ChatMode.Talk, MsgMessage.MsgColor.white, false);
                                return false;
                            }
                        }
                        if (!EventsLib.EventManager.ExecuteSkill((uint)Attack.AtkType, Spell.ID, client))
                        {
                            if (Spell.ID != 1045 && Spell.ID != 1046)
                                return false;
                        }
                        if (client.Player.Map == 1039)
                        {
                            Role.IMapObj target;
                            if (client.Player.View.TryGetValue(Attack.OpponentUID, out target, Role.MapObjectType.SobNpc))
                            {
                                var attacked = target as Role.SobNpc;
                                if (MagicSkills.Contains(Attack.SpellID))
                                {
                                    if (attacked.Mesh != (Role.SobNpc.StaticMesh)707)
                                    {
                                        ClientSpell = default(MsgSpell);
                                        Spell = default(Database.MagicType.Magic);
                                        return false;
                                    }
                                }
                                else
                                {
                                    if (attacked.Mesh == (Role.SobNpc.StaticMesh)707)
                                    {
                                        ClientSpell = default(MsgSpell);
                                        Spell = default(Database.MagicType.Magic);
                                        return false;
                                    }
                                }
                            }
                        }


                        if (Spell.Type == Database.MagicType.MagicSort.DirectAttack || Spell.Type == Database.MagicType.MagicSort.Attack && Spell.ID != 1002)
                        {

                            if (!client.IsInSpellRange(Attack.OpponentUID, Spell.Range))
                            {
                                ClientSpell = default(MsgSpell);
                                Spell = default(Database.MagicType.Magic);
                                return false;
                            }
                        }

                        uint IncreaseSpellStamina = 0;//constant
                        if (client.Player.ContainFlag(MsgUpdate.Flags.ScurvyBomb))
                            IncreaseSpellStamina = (uint)(client.Player.UseStamina + 5);
                        if (client.Player.Map == 700 && client.Player.DynamicID == 0)//Lottery //Players can pk inside Lottery and use summon guards. we need to stop
                        {
                            ClientSpell = default(MsgSpell);
                            Spell = default(Database.MagicType.Magic);
                            return false;
                        }
                        if (client.Player.Map != 1039)
                            {
                            if (Spell.UseStamina + IncreaseSpellStamina > client.Player.Stamina)
                                return false;
                            else
                            {
                                if ((ushort)(Spell.UseStamina + IncreaseSpellStamina) > 0)
                                {
                                    client.Player.Stamina -= (ushort)(Spell.UseStamina + IncreaseSpellStamina);
                                    using (var rec = new ServerSockets.RecycledPacket())
                                    {
                                        var stream = rec.GetStream();
                                        client.Player.SendUpdate(stream, client.Player.Stamina, MsgUpdate.DataType.Stamina);
                                    }
                                }
                            }

                            if (Spell.UseMana > client.Player.Mana)
                                return false;
                            else
                            {
                                if (Spell.UseMana > 0)
                                {
                                    if (Attack.UID > 1000000)
                                        client.Player.Mana -= Spell.UseMana;
                                }
                            }
                        }
                        if (Spell.UseArrows > 0 && Spell.ID >= 8000 && Spell.ID <= 9875)
                        {

                            if (!client.Equipment.FreeEquip(Role.Flags.ConquerItem.LeftWeapon))
                            {
                                Game.MsgServer.MsgGameItem arrow = null;
                                client.Equipment.TryGetEquip(Role.Flags.ConquerItem.LeftWeapon, out arrow);
                                if (arrow.Durability <= 0)//< Spell.UseArrows)                                                                  
                                    return false;
                                else
                                {
                                    if (client.Player.Map != 1039)
                                    {
                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            var stream = rec.GetStream();

                                            arrow.Durability -= (ushort)Math.Min(arrow.Durability, Spell.UseArrows);
                                            client.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsuageID.UpdateArrowCount, arrow.UID, arrow.Durability, 0, 0, 0, 0));
                                            if (arrow.Durability <= 0/*Spell.UseArrows */|| arrow.Durability > 10000)
                                                ReloadArrows(client.Equipment.TryGetEquip(Role.Flags.ConquerItem.LeftWeapon), client, stream);

                                        }
                                    }

                                }
                            }
                            else
                                return false;
                        }

                        return true;
                    }
                }

                ClientSpell = default(MsgSpell);
                Spell = default(Database.MagicType.Magic);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                ClientSpell = default(MsgSpell);
                Spell = default(Database.MagicType.Magic);
                return false;
            }
            return false;
        }

        public static unsafe void ReloadArrows(MsgGameItem arrow, Client.GameClient client, ServerSockets.Packet stream)
        {
            if (client.Player.Class < 40 || client.Player.Class > 45)
                return;
            if (client.Equipment.FreeEquip(Role.Flags.ConquerItem.LeftWeapon))
                return;
            if (client.Equipment.TryGetEquip(Role.Flags.ConquerItem.RightWeapon).ITEM_ID / 1000 != 500)
                return;
            client.Equipment.DestoyArrow(Role.Flags.ConquerItem.LeftWeapon, stream);
            uint id = 1050002;
            if (arrow != null)
                id = arrow.ITEM_ID;
            if (client.Inventory.Contain(id, 1))
            {
                MsgGameItem newArrow;
                client.Inventory.SearchItemByID(id, out newArrow);
                newArrow.Position = 5;
                client.Inventory.Update(newArrow, Role.Instance.AddMode.REMOVE, stream);
                client.Equipment.Add(newArrow, stream);
                client.Equipment.QueryEquipment();
                client.SendSysMesage("Arrows Reloaded.", MsgMessage.ChatMode.TopLeft);
                //  client.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsuageID.UpdateArrowCount, newArrow.UID, newArrow.Durability, 0, 0, 0, 0));

            }
            else if (!client.Inventory.Contain(id, 1))
            {
                client.SendSysMesage("Can't reload arrows, you are out of " + Database.Server.ItemsBase[arrow.ITEM_ID].Name + "s!", MsgMessage.ChatMode.TopLeft);
                client.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsuageID.UpdateArrowCount, 0, 0, 0, 0, 0, 0));
            }

        }
    }
}
