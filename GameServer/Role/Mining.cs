using COServer.Database;
using COServer.Game.MsgServer;
using System;
using System.Collections.Generic;
using System.Text;
using static COServer.Database.ItemType;

namespace COServer.Role
{
    public class Mining
    {
        public unsafe static void Mine(ServerSockets.Packet stream, Client.GameClient client)
        {
            if (!client.Player.Alive)
            {
                client.Player.Mining = false;
                return;
            }
            if (!client.Map.TypeStatus.HasFlag(Role.MapTypeFlags.MineEnable))
            {
                client.Player.Mining = false;
                return;
            }
            Game.MsgServer.MsgGameItem Item;
            if (!client.Equipment.TryGetEquip(Role.Flags.ConquerItem.RightWeapon, out Item))
            {
                client.SendSysMesage("You have to wear PickAxe to start mining.");
                client.Player.Mining = false;
                return;
            }
            if (Item == null) return;
            if (!Database.ItemType.IsPickAxe(Item.ITEM_ID) && !Database.ItemType.IsHoe(Item.ITEM_ID))
            {
                client.SendSysMesage("You have to wear PickAxe or Hoe to start mining.");
                client.Player.Mining = false;
                return;
            }
            if (!client.Inventory.HaveSpace(1))
            {
                client.SendSysMesage("Your inventory is full. You can't mine anymore items.");
                client.Player.Mining = false;
                return;
            }
            ActionQuery a = new ActionQuery()
            {
                ObjId = client.Player.UID,
                Type = ActionType.Mining,
            };
            client.Send(stream.ActionCreate(&a));
            client.Player.View.SendView(stream.ActionCreate(&a), false);

            if (!Role.Core.RateDouble(30))
            {
                return;
            }
            switch (client.Player.Map)
            {
                case 6000://jails
                    {
                        Mine(stream, 700001, 700011, 700021, 700031, 700041, 700051, 700061, 1072010, 1072012, 1072024, 1072031, client);
                        break;
                    }
                case 1028://twincity minecave
                    {
                        Mine(stream, 700001, 700011, 700021, 700031, 700041, 700051, 700061, 1072010, 1072012, 1072024, 1072031, client);
                        break;
                    }
                case 1025://pc mine 1st floor
                case 1503://pc mine 2nd floor left side
                case 1502://pc mine 2nd floor right side
                    {
                        Mine(stream, 700001, 700011, 700021, 700031, 700041, 700051, 700061, 1072010, 1072012, 1072024, 1072031, client);
                        break;
                    }
                case 1027://DesertMine
                case 1026://ApeMine
                    {
                        Mine(stream, 700001, 700011, 700021, 700031, 700041, 700051, 700061, 1072010, 1072012, 1072024, 1072031, client);
                        break;
                    }
                case 1029:
                    {
                        Mine(stream, 700001, 700011, 700021, 700031, 700041, 700051, 700061, 1072010, 1072012, 1072024, 1072031, client);
                        break;
                    }
                default:
                    {
                        client.SendSysMesage("You can't mine here. You must go inside a mine.");
                        client.Player.Mining = false;
                        break;
                    }
            }
        }
        public static string GetItemName(uint ID)
        {
            DBItem item;
            if (Server.ItemsBase.TryGetValue(ID, out item))
            {
                return item.Name;
            }
            return "";

        }
        private static void Mine(ServerSockets.Packet stream, uint GemID, uint GemID2, uint GemID3, uint GemID4, uint GemID5, uint GemID6, uint GemID7, uint Ore1, uint Ore2, uint Ore3, uint Ore4, Client.GameClient client)
        {
            double i = 0;
            bool IsSuperGem = false;
            bool IsRefindGem = false;
            string playerName = client.Player.Name;

            string itemName = GetItemName(GemID); // Nome original do item

            DateTime minedAt = DateTime.Now;
            if (client.Player.Map == 1028) i = 0.02;

            if (GemID != 0 && Role.Core.RateDouble(Global.MINING_DROP_GEMS + i)) // ores type 2
            {
                if (Role.Core.RateDouble(Global.MINING_DROP_GEMS_REFIND))
                {
                    IsRefindGem = true;
                    GemID += 1;
                }
                else if (Role.Core.RateDouble(Global.MINING_DROP_GEMS_SUPER))
                {
                    GemID += 2;
                    IsSuperGem = true;
                }

                itemName = GetItemName(GemID);


                if (IsSuperGem)
                {
                    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Congratulations! " + client.Player.Name + " has found a Super " + itemName + ".", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
                    client.SendSysMesage($"You've gained a {itemName}", MsgMessage.ChatMode.TopLeft);
                }
                else if (IsRefindGem)
                {
                    client.SendSysMesage($"You have gained a {itemName}", MsgMessage.ChatMode.TopLeft);
                    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Congratulations! " + client.Player.Name + " has found a Refined " + itemName + ".", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
                }
                else
                {
                    client.SendSysMesage($"You've gained a {itemName}", MsgMessage.ChatMode.TopLeft);
                }

                client.Inventory.Add(stream, GemID, 1);
                COServer.Database.MiningRepository.InsertMinedItem(client.Player.Name, itemName, DateTime.Now);

                return;
            }

            if (GemID2 != 0 && Role.Core.RateDouble(Global.MINING_DROP_GEMS + i))//ores type 2
            {
                if (Role.Core.RateDouble(Global.MINING_DROP_GEMS_REFIND))
                {
                    IsRefindGem = true;
                    GemID2 += 1;
                }
                else if (Role.Core.RateDouble(Global.MINING_DROP_GEMS_SUPER))
                {
                    GemID2 += 2;
                    IsSuperGem = true;
                }

                // Obtenha o nome do item após as modificações
                itemName = GetItemName(GemID2);

                if (IsSuperGem)
                {
                    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Congratulations! " + client.Player.Name + " has found a Super " + GetItemName(GemID2) + ".", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
                    client.SendSysMesage($"You've gained a {GetItemName(GemID2)}", MsgMessage.ChatMode.TopLeft);
                }
                else if (IsRefindGem)
                {
                    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Congratulations! " + client.Player.Name + " has found a Refined " + GetItemName(GemID2) + ".", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
                    client.SendSysMesage($"You've gained a {GetItemName(GemID2)}", MsgMessage.ChatMode.TopLeft);
                }
                else
                {
                    client.SendSysMesage($"You've gained a {GetItemName(GemID2)}", MsgMessage.ChatMode.TopLeft);
                }
                client.Inventory.Add(stream, GemID2, 1);
                COServer.Database.MiningRepository.InsertMinedItem(client.Player.Name, itemName, DateTime.Now);
                return;
            }

            if (GemID3 != 0 && Role.Core.RateDouble(Global.MINING_DROP_GEMS + i))//ores type 2
            {

                if (Role.Core.RateDouble(Global.MINING_DROP_GEMS_REFIND))
                {
                    IsRefindGem = true;
                    GemID3 += 1;
                }
                else if (Role.Core.RateDouble(Global.MINING_DROP_GEMS_SUPER))
                {
                    GemID3 += 2;
                    IsSuperGem = true;
                }

                // Obtenha o nome do item após as modificações
                itemName = GetItemName(GemID3);

                if (IsSuperGem)
                {
                    client.SendSysMesage($"You've gained a {GetItemName(GemID3)}", MsgMessage.ChatMode.TopLeft);
                    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Congratulations! " + client.Player.Name + " has found a Super " + GetItemName(GemID3) + ".", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));

                }
                else if (IsRefindGem)
                {
                    client.SendSysMesage($"You've gained a {GetItemName(GemID3)}", MsgMessage.ChatMode.TopLeft);
                    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Congratulations! " + client.Player.Name + " has found a Refined " + GetItemName(GemID3) + ".", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
                }
                else
                {
                    client.SendSysMesage($"You've gained a {GetItemName(GemID3)}", MsgMessage.ChatMode.TopLeft);
                }
                client.Inventory.Add(stream, GemID3, 1);
                COServer.Database.MiningRepository.InsertMinedItem(client.Player.Name, itemName, DateTime.Now);
                return;
            }

            if (GemID4 != 0 && Role.Core.RateDouble(Global.MINING_DROP_GEMS + i))//ores type 2
            {

                if (Role.Core.RateDouble(Global.MINING_DROP_GEMS_REFIND))
                {
                    IsRefindGem = true;
                    GemID4 += 1;
                }
                else if (Role.Core.RateDouble(Global.MINING_DROP_GEMS_SUPER))
                {
                    GemID4 += 2;
                    IsSuperGem = true;
                }

                // Obtenha o nome do item após as modificações
                itemName = GetItemName(GemID4);

                if (IsSuperGem)
                {
                    client.SendSysMesage($"You've gained a {GetItemName(GemID4)}", MsgMessage.ChatMode.TopLeft);
                    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Congratulations! " + client.Player.Name + " has found a Super " + GetItemName(GemID4) + ".", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));

                }
                else if (IsRefindGem)
                {
                    client.SendSysMesage($"You've gained a {GetItemName(GemID4)}", MsgMessage.ChatMode.TopLeft);
                    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Congratulations! " + client.Player.Name + " has found a Refined " + GetItemName(GemID4) + ".", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
                }
                else
                {
                    client.SendSysMesage($"You've gained a {GetItemName(GemID4)}", MsgMessage.ChatMode.TopLeft);
                }
                client.Inventory.Add(stream, GemID4, 1);
                COServer.Database.MiningRepository.InsertMinedItem(client.Player.Name, itemName, DateTime.Now);
                return;
            }

            if (GemID5 != 0 && Role.Core.RateDouble(Global.MINING_DROP_GEMS + i)) // ores type 2
            {
                if (Role.Core.RateDouble(Global.MINING_DROP_GEMS_REFIND))
                {
                    IsRefindGem = true;
                    GemID5 += 1;
                }
                else if (Role.Core.RateDouble(Global.MINING_DROP_GEMS_SUPER))
                {
                    GemID5 += 2;
                    IsSuperGem = true;
                }

                itemName = GetItemName(GemID5);

                if (IsSuperGem)
                {
                    client.SendSysMesage($"You've gained a {itemName}", MsgMessage.ChatMode.TopLeft);
                    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Congratulations! " + client.Player.Name + " has found a Super " + itemName + ".", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
                }
                else if (IsRefindGem)
                {
                    client.SendSysMesage($"You've gained a {itemName}", MsgMessage.ChatMode.TopLeft);
                    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Congratulations! " + client.Player.Name + " has found a Refined " + itemName + ".", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
                }
                else
                {
                    client.SendSysMesage($"You've gained a {itemName}", MsgMessage.ChatMode.TopLeft);
                }

                client.Inventory.Add(stream, GemID5, 1);
                COServer.Database.MiningRepository.InsertMinedItem(client.Player.Name, itemName, DateTime.Now);
                return;
            }

            if (GemID6 != 0 && Role.Core.RateDouble(Global.MINING_DROP_GEMS + i)) // ores type 2
            {
                if (Role.Core.RateDouble(Global.MINING_DROP_GEMS_REFIND))
                {
                    IsRefindGem = true;
                    GemID6 += 1;
                }
                else if (Role.Core.RateDouble(Global.MINING_DROP_GEMS_SUPER))
                {
                    GemID6 += 2;
                    IsSuperGem = true;
                }

                itemName = GetItemName(GemID6);

                if (IsSuperGem)
                {
                    client.SendSysMesage($"You've gained a {itemName}", MsgMessage.ChatMode.TopLeft);
                    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Congratulations! " + client.Player.Name + " has found a Super " + itemName + ".", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
                }
                else if (IsRefindGem)
                {
                    client.SendSysMesage($"You've gained a {itemName}", MsgMessage.ChatMode.TopLeft);
                    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Congratulations! " + client.Player.Name + " has found a Refined " + itemName + ".", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
                }
                else
                {
                    client.SendSysMesage($"You've gained a {itemName}", MsgMessage.ChatMode.TopLeft);
                }

                client.Inventory.Add(stream, GemID6, 1);
                COServer.Database.MiningRepository.InsertMinedItem(client.Player.Name, itemName, DateTime.Now);
                return;
            }

            if (GemID7 != 0 && Role.Core.RateDouble(Global.MINING_DROP_GEMS + i)) // ores type 2
            {
                if (Role.Core.RateDouble(Global.MINING_DROP_GEMS_REFIND))
                {
                    IsRefindGem = true;
                    GemID7 += 1;
                }
                else if (Role.Core.RateDouble(Global.MINING_DROP_GEMS_SUPER))
                {
                    GemID7 += 2;
                    IsSuperGem = true;
                }

                itemName = GetItemName(GemID7);

                if (IsSuperGem)
                {
                    client.SendSysMesage($"You've gained a {itemName}", MsgMessage.ChatMode.TopLeft);
                    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Congratulations! " + client.Player.Name + " has found a Super " + itemName + ".", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
                }
                else if (IsRefindGem)
                {
                    client.SendSysMesage($"You've gained a {itemName}", MsgMessage.ChatMode.TopLeft);
                    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Congratulations! " + client.Player.Name + " has found a Refined " + itemName + ".", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
                }
                else
                {
                    client.SendSysMesage($"You've gained a {itemName}", MsgMessage.ChatMode.TopLeft);
                }

                client.Inventory.Add(stream, GemID7, 1);
                COServer.Database.MiningRepository.InsertMinedItem(client.Player.Name, itemName, DateTime.Now);
                return;
            }


            if (Ore1 != 0 && Role.Core.RateDouble(25)) // ores type 1
            {
                Ore1 += (uint)Program.GetRandom.Next(0, 9);

                if (client.Player.VipLevel >= 4 && client.Player.SkipBadOre)
                    return;

                client.Inventory.Add(stream, Ore1, 1);
                client.SendSysMesage($"You've gained a {GetItemName(Ore1)}", MsgMessage.ChatMode.TopLeft);
                return;
            }

            if (Ore2 != 0 && Role.Core.RateDouble(0.55)) // ores type 2
            {
                Ore2 += (uint)Program.GetRandom.Next(0, 9);

                if (client.Player.VipLevel >= 4 && client.Player.SkipBadOre)
                    return;

                client.Inventory.Add(stream, Ore2, 1);
                client.SendSysMesage($"You've gained a {GetItemName(Ore2)}", MsgMessage.ChatMode.TopLeft);
                return;
            }

            if (Ore3 != 0 && Role.Core.RateDouble(10)) // ores type 3
            {
                Ore3 += (uint)Program.GetRandom.Next(0, 9);

                if (client.Player.VipLevel >= 4 && client.Player.SkipBadOre)
                    return;

                client.Inventory.Add(stream, Ore3, 1);
                client.SendSysMesage($"You've gained a {GetItemName(Ore3)}", MsgMessage.ChatMode.TopLeft);
                return;
            }

            if (Ore4 != 0 && Role.Core.RateDouble(0.042)) // ores type 4
            {
                Ore4 += (uint)Program.GetRandom.Next(0, 9);

                if (client.Player.VipLevel >= 4 && client.Player.SkipBadOre)
                    return;

                client.Inventory.Add(stream, Ore4, 1);
                client.SendSysMesage($"You've gained a {GetItemName(Ore4)}", MsgMessage.ChatMode.TopLeft);
                return;
            }

            // Drop controlado pela constante Global.MINING_DROP_EUXENITE
            if (Role.Core.RateDouble(Global.MINING_DROP_EUXENITE))
            {
                client.Inventory.Add(stream, Database.ItemType.EuxeniteOre, 1);
                client.SendSysMesage($"You've gained a {GetItemName(Database.ItemType.EuxeniteOre)}", MsgMessage.ChatMode.TopLeft);
                return;
            }
        }

    }
}