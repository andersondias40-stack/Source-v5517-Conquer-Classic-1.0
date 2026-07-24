using System.Collections.Generic;
using System.IO;

namespace COServer.Database
{
    public class NpcServer
    {
        public class Furniture
        {
            public string Name;
            public uint ItemID;
            public int MoneyCost;
            public uint UID;
            public ushort Mesh;
        }
        public static Dictionary<uint, Furniture> FurnitureInformations = new Dictionary<uint, Furniture>();

        public static Furniture GetNpc(uint ItemID)
        {
            foreach (var npc in FurnitureInformations.Values)
            {
                if (npc.ItemID == ItemID)
                {
                    return npc;
                }
            }
            return null;
        }
        public static Furniture GetNpcFromMesh(uint mesh)
        {
            foreach (var npc in FurnitureInformations.Values)
            {
                if (npc.Mesh == mesh)
                {
                    return npc;
                }
            }
            return null;
        }
        internal static void LoadSobNpcs()
        {
            string[] baseText = File.ReadAllLines(Program.ServerConfig.DbLocation + "SobNpcs.txt");
            foreach (var bas_line in baseText)
            {
                Database.DBActions.ReadLine line = new DBActions.ReadLine(bas_line, ',');
                Role.SobNpc npc = new Role.SobNpc();
                npc.ObjType = Role.MapObjectType.SobNpc;
                npc.UID = line.Read((uint)0);
                npc.Name = line.Read("");
                npc.Type = (Role.Flags.NpcType)line.Read((ushort)0);
                npc.Mesh = (Role.SobNpc.StaticMesh)line.Read((ushort)0);
                npc.Map = line.Read((ushort)0);
                npc.X = line.Read((ushort)0);
                npc.Y = line.Read((ushort)0);
                npc.HitPoints = line.Read((int)0);
                npc.MaxHitPoints = line.Read((int)0);
                npc.Sort = line.Read((ushort)0);
                if (npc.Map == 1039)
                    npc.MaxHitPoints = npc.HitPoints = 10000;
                if (line.Read((byte)0) == 0)
                    npc.Name = null;
                if (Server.ServerMaps.ContainsKey(npc.Map))
                {
                    Server.ServerMaps[npc.Map].View.EnterMap<Role.IMapObj>(npc);

                    if (Role.GameMap.IsGate(npc.UID))
                        Server.ServerMaps[npc.Map].SetGateFlagNpc(npc.X, npc.Y);
                    else
                        Server.ServerMaps[npc.Map].SetFlagNpc(npc.X, npc.Y);
                }
            }

            Game.MsgTournaments.MsgSchedules.GuildWar.CreateFurnitures();
            Game.MsgTournaments.MsgSchedules.GuildWar.Load();
        }
        public static void LoadServerTraps()
        {
            if (System.IO.File.Exists(Program.ServerConfig.DbLocation + "Traps.txt"))
            {
                using (System.IO.StreamReader read = System.IO.File.OpenText(Program.ServerConfig.DbLocation + "Traps.txt"))
                {
                    while (true)
                    {
                        string aline = read.ReadLine();
                        if (aline != null && aline != "")
                        {
                            string[] line = aline.Split(',');

                            uint ID = uint.Parse(line[3]);
                            ushort map = ushort.Parse(line[0]);
                            if (ID == 1063 && (map == 1002 || map == 1010 || map == 1011 || map == 1020 || map == 1000 || map == 1015))
                                continue;
                            var Item = new Game.MsgFloorItem.MsgItem(null, ushort.Parse(line[1]), ushort.Parse(line[2]), Game.MsgFloorItem.MsgItem.ItemType.Effect, 0, 0, ushort.Parse(line[0]), 0, false, Database.Server.ServerMaps[ushort.Parse(line[0])], 60 * 60 * 1000);
                            Item.MsgFloor.m_ID = ID;
                            Item.MsgFloor.m_Color = 2;
                            Item.MsgFloor.DropType = Game.MsgFloorItem.MsgDropID.Effect;
                            if (line.Length > 4)
                                Item.AllowDynamic = byte.Parse(line[4]) == 1;
                            Item.GMap.View.EnterMap<Role.IMapObj>(Item);

                        }
                        else
                            break;
                    }
                }
            }
        }
        public static void LoadNpcs()
        {
            uint Count = 0;
            if (System.IO.File.Exists(Program.ServerConfig.DbLocation + "npcs.txt"))
            {
                using (System.IO.StreamReader read = System.IO.File.OpenText(Program.ServerConfig.DbLocation + "npcs.txt"))
                {
                    while (true)
                    {
                        string aline = read.ReadLine();
                        if (aline != null && aline != "")
                        {

                            string[] line = aline.Split(',');
                            Game.MsgNpc.Npc np = Game.MsgNpc.Npc.Create();
                            np.UID = uint.Parse(line[0]);
                            np.NpcType = (Role.Flags.NpcType)byte.Parse(line[1]);
                            np.Mesh = ushort.Parse(line[2]);
                            np.Map = ushort.Parse(line[3]);
                            np.X = ushort.Parse(line[4]);
                            np.Y = ushort.Parse(line[5]);
                           
                            if (np.Mesh == 42580)
                                continue;
                            if (np.UID == 16851 || np.UID == 168052 || np.UID == 113)
                            {
                                np.AllowDynamic = true;
                            }
                            else if (line.Length > 6)
                                    np.Name = line[6];
                            else if (line.Length > 7)
                                np.AllowDynamic = ushort.Parse(line[7]) == 1;

                            if (Server.ServerMaps.ContainsKey(np.Map))
                            {
                                Count++;
                                Server.ServerMaps[np.Map].AddNpc(np);
                            }

                        }
                        else
                            break;
                    }
                    Console.WriteLine("Loading " + Count + " Npcs");
                }
            }
            if (System.IO.File.Exists(Program.ServerConfig.DbLocation + "furnitures.txt"))
            {
                using (StreamReader read = File.OpenText(Program.ServerConfig.DbLocation + "furnitures.txt"))
                {
                    //read.ReadLine();//first line
                    while (true)
                    {
                        string aline = read.ReadLine();
                        if (aline != null)
                        {

                            string[] line = aline.Split(',');
                            Game.MsgNpc.Npc np = Game.MsgNpc.Npc.Create();
                            np.UID = uint.Parse(line[0]);
                            np.NpcType = (Role.Flags.NpcType)byte.Parse(line[1]);
                            np.Mesh = ushort.Parse(line[2]);
                            np.Map = ushort.Parse(line[3]);
                            np.X = ushort.Parse(line[4]);
                            np.Y = ushort.Parse(line[5]);

                            Furniture furnit = new Furniture();
                            furnit.Name = line[6];
                            furnit.ItemID = uint.Parse(line[7]);
                            furnit.UID = np.UID;
                            furnit.Mesh = np.Mesh;
                            furnit.MoneyCost = int.Parse(line[8]);
                            if (!FurnitureInformations.ContainsKey(np.UID))
                                FurnitureInformations.Add(np.UID, furnit);

                            if (Server.ServerMaps.ContainsKey(np.Map))
                            {
                                Server.ServerMaps[np.Map].AddNpc(np);
                            }
                        }
                        else
                            break;
                    }
                }
            }
        }
    }
}
