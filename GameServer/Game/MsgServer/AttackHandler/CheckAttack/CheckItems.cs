using System;

namespace COServer.Game.MsgServer.AttackHandler.CheckAttack
{
    public class CheckItems
    {
        //tq
        public static void AttackDurability(Client.GameClient client, ServerSockets.Packet stream)
        {
            if (client.Player.Rate(3))
            {
                bool dura_zero = false;
                 
                foreach (var item in client.Equipment.CurentEquip)
                {
                    if (item != null)
                    {
                        if (item.Position == (ushort)Role.Flags.ConquerItem.RightWeapon
                            || item.Position == (ushort)Role.Flags.ConquerItem.LeftWeapon
                            || item.Position == (ushort)Role.Flags.ConquerItem.Ring
                            || item.Position == (ushort)Role.Flags.ConquerItem.Fan)
                        {
                            if (item.Position == (ushort)Role.Flags.ConquerItem.LeftWeapon)
                            {
                                if (Database.ItemType.IsArrow(item.ITEM_ID))
                                    continue;
                            }

                            byte durability = (byte)Program.GetRandom.Next(1, Math.Max(1, (int)(item.Durability / 1000)));

                            durability -= (byte)((uint)durability * client.GemValues(Role.Flags.Gem.NormalKylinGem) / 100);

                            if (item.Durability < 100)
                            {
                                if ((item.Durability % 10) == 0)
                                {
                                    client.SendSysMesage($"{Database.Server.ItemsBase.GetItemName(item.ITEM_ID)} has been severely damaged. Please repair it soon, otherwise, it will be gone.", MsgMessage.ChatMode.TopLeft);
                                    client.Player.SendString(stream, MsgStringPacket.StringID.Effect, true, "EquipBroken");
                                }
                            }
                            else if (item.Durability < 200)
                            {
                                if (item.Durability % 10 == 0)
                                {
                                    client.SendSysMesage($"Durability of {Database.Server.ItemsBase.GetItemName(item.ITEM_ID)}  is too low. Please repair it soon to prevent further damaging.", MsgMessage.ChatMode.TopLeft);
                                    client.Player.SendString(stream, MsgStringPacket.StringID.Effect, true, "EquipBroken");
                                }
                            }
                            if (item.Durability > durability)
                            {
                                item.Durability -= durability;

                            }
                            else
                            {
                                item.Durability = 0;
                                dura_zero = true;
                            }
                            item.Mode = Role.Flags.ItemMode.Update;
                      //      client.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsuageID.UpdateDurability, item.Durability, 0, 0, 0, 0, 0));
                            item.Send(client, stream);
                            item.Mode = Role.Flags.ItemMode.None;

                        }
                    }
                }
                if (dura_zero)
                    client.Equipment.QueryEquipment();
            }
        }
        public static void RespouseDurability(Client.GameClient client)
        {
            if (client.Player.Rate(1))
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();

                    bool dura_zero = false;
                    foreach (var item in client.Equipment.CurentEquip)
                    {
                        if (item != null)
                        {
                            if (item.Position == (ushort)Role.Flags.ConquerItem.Armor
                                || item.Position == (ushort)Role.Flags.ConquerItem.Necklace
                                || item.Position == (ushort)Role.Flags.ConquerItem.Boots
                                 || item.Position == (ushort)Role.Flags.ConquerItem.Head
                                 || item.Position == (ushort)Role.Flags.ConquerItem.Tower)
                            {
                                byte durability = (byte)Program.GetRandom.Next(2, Math.Max(2, (int)(item.Durability / 1000)));


                                if (item.Durability < 100)
                                {
                                    if ((item.Durability % 10) == 0)
                                    {
                                        client.SendSysMesage($"{Database.Server.ItemsBase.GetItemName(item.ITEM_ID)} has been severely damaged. Please repair it soon, otherwise, it will be gone.", MsgMessage.ChatMode.TopLeft);
                                        client.Player.SendString(stream, MsgStringPacket.StringID.Effect, true, "EquipBroken");
                                    }
                                }
                                else if (item.Durability < 200)
                                {
                                    if (item.Durability % 10 == 0)
                                    {
                                        client.SendSysMesage($"Durability of {Database.Server.ItemsBase.GetItemName(item.ITEM_ID)}  is too low. Please repair it soon to prevent further damaging.", MsgMessage.ChatMode.TopLeft);
                                        client.Player.SendString(stream, MsgStringPacket.StringID.Effect, true, "EquipBroken");
                                    }
                                }
                                if (item.Durability > durability)
                                    item.Durability -= durability;
                                else
                                {
                                    item.Durability = 0;
                                    dura_zero = true;
                                }
                                item.Mode = Role.Flags.ItemMode.Update;


                                item.Send(client, stream);
                            }

                        }
                    }
                    if (dura_zero)
                        client.Equipment.QueryEquipment();
                }
            }
        }
    }
}
