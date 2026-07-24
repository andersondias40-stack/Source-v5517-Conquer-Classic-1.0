using COServer.Client;
using COServer.ServerSockets;
using System;
using System.Collections.Generic;
using System.IO;

namespace COServer.Database
{
    public class Lottery
    {
        public static List<uint> RateH = new List<uint>()
        {
            723584,723700,723583,723727,1088001,1088002,1088000,1080001,1072031,751001,751003,751009,751099,751999,710001
        };
        public static List<uint> RateH2 = new List<uint>()
        {
            720027,720028,725018,725019,725020,725021,725022,725023,725024,420019,410029,160019,152019,151019,150019,134009,133009,131009,130009,121029,120029,117009
        };
        public static List<uint> RateH3 = new List<uint>()
        {
            730001,191905,181655,700001,700011,700021,700031,700041,700051,700061,730003
        };
        public static List<uint> RateH4 = new List<uint>()
        {
            700002,700012,700022,700032,700042,700052,700062,730003
        };
        public static List<uint> RateH5 = new List<uint>()
        {
            2100025,2100045,722057,700072,730004
        };
        //public static list<uint> super1socitems = new list<uint>()
        //{
        //   empty.
        //};
        //public static list<uint> super2socitems = new list<uint>()
        //{
        //   empty.
        //};
        //public static list<uint> supernosocitems = new list<uint>()
        //{
        //   empty.
        //};
        //public static list<uint> eliteplus8items = new list<uint>()
        //{
        //   empty.
        //};
        public static void GetRandomPrize(GameClient Client, Packet stream)
        //{
        // if (Role.Core.Rate(0.0002))
        // {
        //     uint Id = ElitePlus8Items[Role.Core.Random.Next(0, ElitePlus8Items.Count)];
        //     Client.Inventory.Add(stream, Id, 1, 8);
        //     Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Congratulations! " + Client.Player.Name + " won a +8" + Server.ItemsBase[Id].Name + " in Lottery.", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
        //     Program.Plus8++;
        //  }
        //else if (Role.Core.Rate(0.0002))
        //{
        //    uint Id = Super2SocItems[Role.Core.Random.Next(0, Super2SocItems.Count)];
        //    Client.Inventory.Add(stream, Id, 1, 0, 0, 0, Role.Flags.Gem.EmptySocket, Role.Flags.Gem.EmptySocket);
        //    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Congratulations! " + Client.Player.Name + " won a Super-2soc." + Server.ItemsBase[Id].Name + " in lottery.", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
        //    Program.Super2Soc++;
        //}
        //else if (Role.Core.Rate(0.001))
        // {
        //    uint Id = Super1SocItems[Role.Core.Random.Next(0, Super1SocItems.Count)];
        //    Client.Inventory.Add(stream, Id, 1, 0, 0, 0, Role.Flags.Gem.EmptySocket);
        //    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Congratulations! " + Client.Player.Name + " won a Super-1soc." + Server.ItemsBase[Id].Name + " in lottery.", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
        //    Program.Super1Soc++;
        // }
        // else if (Role.Core.Rate(0.005))
        // {
        //    uint Id = SuperNoSocItems[Role.Core.Random.Next(0, SuperNoSocItems.Count)];
        //    Client.Inventory.Add(stream, Id, 1);
        //    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Congratulations! " + Client.Player.Name + " won a Super" + Server.ItemsBase[Id].Name + " in lottery.", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
        //    Program.SuperNoSoc++;
        //}
            { 
                if (Role.Core.Rate(0.005))
            {
                if (Client.ProjectManager)
                    Client.SendSysMesage("Group5", Game.MsgServer.MsgMessage.ChatMode.TopLeft);
                uint Id = RateH5[Role.Core.Random.Next(0, RateH5.Count)];
                Client.Inventory.Add(stream, Id, 1);
                Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Congratulations! " + Client.Player.Name + " won " + Server.ItemsBase[Id].Name + " in lottery.", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
                Program.DiscordAPILotery.Enqueue("``Congratulations! ["+ Client.Player.Name + "] won " + Server.ItemsBase[Id].Name + " in lottery.``");
            }
            else if (Role.Core.Rate(0.010))
            {
                if (Client.ProjectManager)
                    Client.SendSysMesage("Group4", Game.MsgServer.MsgMessage.ChatMode.TopLeft);
               uint Id = RateH4[Role.Core.Random.Next(0, RateH4.Count)];
                Client.Inventory.Add(stream, Id, 1);
                Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Congratulations! " + Client.Player.Name + " won " + Server.ItemsBase[Id].Name + " in lottery.", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
                Program.DiscordAPILotery.Enqueue("``Congratulations! [" + Client.Player.Name + "] won " + Server.ItemsBase[Id].Name + " in lottery.``");
            }
            else if (Role.Core.Rate(0.015))
            {
                if (Client.ProjectManager)
                    Client.SendSysMesage("Group3", Game.MsgServer.MsgMessage.ChatMode.TopLeft);
                uint Id = RateH3[Role.Core.Random.Next(0, RateH3.Count)];
                Client.Inventory.Add(stream, Id, 1);
                Program.DiscordAPILotery.Enqueue("``Congratulations! [" + Client.Player.Name + "] won " + Server.ItemsBase[Id].Name + " in lottery.``");
            }
            else if (Role.Core.Rate(10))
            {
                if (Client.ProjectManager)
                    Client.SendSysMesage("Group2", Game.MsgServer.MsgMessage.ChatMode.TopLeft);
                uint Id = RateH2[Role.Core.Random.Next(0, RateH2.Count)];
                Client.Inventory.Add(stream, Id, 1);
               
            }
            else
            {
                if (Client.ProjectManager)
                    Client.SendSysMesage("Group1", Game.MsgServer.MsgMessage.ChatMode.TopLeft);
                uint Id = RateH[Role.Core.Random.Next(0, RateH.Count)];
                Client.Inventory.Add(stream, Id, 1);
               
            }
        }
        //static string ItemsSuper = "";
        //static string ItemsElite = "";
        //static string ItemsSuper1Soc = "";
        //static string ItemsSuper2Soc = "";
        //public static void TestLoad()
        //{
        //    using (var reader = new StreamReader("Lott.txt"))
        //    {
        //        string[] lines = reader.ReadToEnd().Split(new string[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
        //        foreach (var item in lines)
        //        {
        //            string name = "";
        //            if (item.StartsWith("Elite+8"))
        //            {
        //                name = item.Replace("Elite+8", "");
        //                var Id = ItemType.GetItemIdByName(name).Split('-');
        //                if (Id[0] != "0")
        //                {
        //                    foreach (var ip in Id)
        //                        if (ip.EndsWith("8"))
        //                        {
        //                            ItemsElite += ip + ",";
        //                            break;
        //                        }
        //                }
        //            }
        //            else if (item.StartsWith("Super2-Soc."))
        //            {
        //                name = item.Replace("Super2-Soc.", "");
        //                var Id = ItemType.GetItemIdByName(name).Split('-');
        //                if (Id[0] != "0")
        //                {
        //                    foreach (var ip in Id)
        //                        if (ip.EndsWith("9"))
        //                        {
        //                            ItemsSuper2Soc += ip + ",";
        //                            break;
        //                        }
        //                }
        //            }
        //            else if (item.StartsWith("Super1-Soc."))
        //            {
        //                name = item.Replace("Super1-Soc.", "");
        //                var Id = ItemType.GetItemIdByName(name).Split('-');
        //                if (Id[0] != "0")
        //                {
        //                    foreach (var ip in Id)
        //                        if (ip.EndsWith("9"))
        //                        {
        //                            ItemsSuper1Soc += ip + ",";
        //                            break;
        //                        }
        //                }
        //            }
        //            else if (item.StartsWith("Super"))
        //            {
        //                name = item.Replace("Super", "");
        //                var Id = ItemType.GetItemIdByName(name).Split('-');
        //                if (Id[0] != "0")
        //                {
        //                    foreach (var ip in Id)
        //                        if (ip.EndsWith("9"))
        //                        {
        //                            ItemsSuper += ip + ",";
        //                            break;
        //                        }
        //                }
        //            }
        //        }
        //        //using (var writer = new StreamWriter("lott2.txt"))
        //        //{
        //        //    foreach (var item in Items)
        //        //        writer.WriteLine(item);
        //        //    writer.Close();
        //        //}
        //    }
        //}
        public static void TestLoad()
        {
            List<string> e = new List<string>();
            using (var reader = new StreamReader("garments.txt"))
            {
                string[] lines = reader.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in lines)
                {
                    var data = item.Split(' ');
                    uint uid = uint.Parse(ItemType.GetItemIdByName(data[0]));
                    e.Add($"{uid} {data[1]} item other new");

                }
                using (var writer = new StreamWriter("lott2.txt"))
                {
                    foreach (var item in e)
                        writer.WriteLine(item);
                    writer.Close();
                }
            }
        }

    }
}
