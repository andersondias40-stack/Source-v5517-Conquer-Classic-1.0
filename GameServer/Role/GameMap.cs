using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.IO;
using COServer.Game.MsgServer;
using COServer.Game.MsgFloorItem;
using static COServer.Role.Flags;
using System.Windows.Forms;

namespace COServer.Role
{
    public class MapView
    {
        const int CELLS_PER_BLOCK = 18;



        private Counter CounterMovement = new Counter(1);

        public ViewPtr[,] m_setBlock;

        private int Width, Height;

        private int GetWidthOfBlock() { return (Width - 1) / CELLS_PER_BLOCK + 1; }
        private int GetHeightOfBlock() { return (Height - 1) / CELLS_PER_BLOCK + 1; }

        public MapView(int _Width, int _Height)
        {
            Width = _Width;
            Height = _Height;

            m_setBlock = new ViewPtr[GetWidthOfBlock(), GetHeightOfBlock()];
            for (int x = 0; x < GetWidthOfBlock(); x++)
                for (int y = 0; y < GetHeightOfBlock(); y++)
                    m_setBlock[x, y] = new ViewPtr();
        }

        private int Block(int nPos)
        {
            return nPos / CELLS_PER_BLOCK;
        }
        private ViewPtr BlockSet(int nPosX, int nPosY) { return m_setBlock[Block(nPosX), Block(nPosY)]; }

        public bool MoveTo<T>(T obj, int nNewPosX, int nNewPosY)
            where T : IMapObj
        {

            int nOldPosX = obj.X;
            int nOldPosY = obj.Y;
            if ((nOldPosX >= 0 && nOldPosX < Width) == false)
                return false;
            if ((nOldPosY >= 0 && nOldPosY < Height) == false)
                return false;
            if ((nNewPosX >= 0 && nNewPosX < Width) == false)
                return false;
            if ((nNewPosY >= 0 && nNewPosY < Height) == false)
                return false;

            if (Block(nOldPosX) == Block(nNewPosX) && Block(nOldPosY) == Block(nNewPosY))
                return false;

            BlockSet(nOldPosX, nOldPosY).RemoveObject<T>(obj);
            BlockSet(nNewPosX, nNewPosY).AddObject<T>(obj);

            if (obj.ObjType == MapObjectType.Player)
                obj.IndexInScreen = CounterMovement.Next;

            return true;
        }

        public bool EnterMap<T>(T obj)
            where T : IMapObj
        {
            if ((obj.X >= 0 && obj.X < Width) == false)
                return false;
            if ((obj.Y >= 0 && obj.Y < Height) == false)
                return false;

            BlockSet(obj.X, obj.Y).AddObject<T>(obj);

            if (obj.ObjType == MapObjectType.Player)
                obj.IndexInScreen = CounterMovement.Next;

            return true;
        }
        public bool LeaveMap<T>(T obj)
             where T : IMapObj
        {
            if ((obj.X >= 0 && obj.X < Width) == false)
                return false;
            if ((obj.Y >= 0 && obj.Y < Height) == false)
                return false;

            BlockSet(obj.X, obj.Y).RemoveObject<T>(obj);

            return true;
        }


        public IEnumerable<IMapObj> Roles(MapObjectType typ, int X, int Y, Predicate<IMapObj> P = null)
        {

            for (int x = Math.Max(Block(X) - 1, 0); x <= Block(X) + 1 && x < GetWidthOfBlock(); x++)
                for (int y = Math.Max(Block(Y) - 1, 0); y <= Block(Y) + 1 && y < GetHeightOfBlock(); y++)
                {
                    var list = m_setBlock[x, y].GetObjects(typ);
                    if (list != null)
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            if (i >= list.Count)
                                break;
                            var element = list[i];
                            if (element != null)
                            {
                                if (P != null)
                                {
                                    if (P(element))
                                        yield return element;
                                }
                                else if (element != null)
                                    yield return element;
                            }
                        }
                    }
                }
        }
        public int CountRoles(MapObjectType typ, int X, int Y)
        {
            int count = 0;
            for (int x = Math.Max(Block(X) - 1, 0); x <= Block(X) + 1 && x < GetWidthOfBlock(); x++)
                for (int y = Math.Max(Block(Y) - 1, 0); y <= Block(Y) + 1 && y < GetHeightOfBlock(); y++)
                {
                    var list = m_setBlock[x, y].GetObjects(typ);
                    count += list.Count;
                }
            return count;
        }
        public IEnumerable<IMapObj> GetAllMapRoles(MapObjectType typ, Predicate<IMapObj> P = null)
        {
            for (int x = 0; x < GetWidthOfBlock(); x++)
                for (int y = 0; y < GetHeightOfBlock(); y++)
                {
                    var list = m_setBlock[x, y].GetObjects(typ);
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (i >= list.Count)
                            break;
                        var element = list[i];
                        if (element != null)
                        {
                            if (P != null)
                            {
                                if (P(element))
                                    yield return element;
                            }
                            else if (element != null)
                                yield return element;
                        }
                    }
                }
        }
        public int GetAllMapRolesCount(MapObjectType typ, Predicate<IMapObj> P = null)
        {
            return GetAllMapRoles(typ, P).Count();
        }
        public T GetMapObject<T>(MapObjectType typ, uint UID, Predicate<IMapObj> P = null)
        {
            foreach (var obj in GetAllMapRoles(typ, P))
                if (obj.UID == UID)
                    return (T)obj;
            return default(T);
        }
        public bool MapContain(MapObjectType typ, uint UID, Predicate<IMapObj> P = null)
        {
            foreach (var obj in GetAllMapRoles(typ, P))
                if (obj.UID == UID)
                    return true;
            return false;
        }
        public void ClearMap(MapObjectType typ)
        {
            for (int x = 0; x < GetWidthOfBlock(); x++)
                for (int y = 0; y < GetHeightOfBlock(); y++)
                {
                    m_setBlock[x, y].Clear(typ);
                }
        }
        public bool TryGetObject<T>(uint UID, MapObjectType typ, int X, int Y, out T obj)
            where T : IMapObj
        {
            for (int x = Math.Max(Block(X) - 1, 0); x <= Block(X) + 1 && x < GetWidthOfBlock(); x++)
                for (int y = Math.Max(Block(Y) - 1, 0); y <= Block(Y) + 1 && y < GetHeightOfBlock(); y++)
                {
                    var list = m_setBlock[x, y];
                    if (list.TryGetObject<T>(typ, UID, out obj))
                        return true;

                }
            obj = default(T);
            return false;
        }
        public bool Contain(uint UID, int X, int Y)
        {
            for (int x = Math.Max(Block(X) - 1, 0); x <= Block(X) + 1 && x < GetWidthOfBlock(); x++)
                for (int y = Math.Max(Block(Y) - 1, 0); y <= Block(Y) + 1 && y < GetHeightOfBlock(); y++)
                {
                    var list = m_setBlock[x, y];
                    for (int i = 0; i < (int)MapObjectType.Count; i++)
                        if (list.ContainObject((MapObjectType)i, UID))
                            return true;

                }
            return false;
        }
    }
    public class ViewPtr
    {
        private MyList<Role.IMapObj>[] Objects;
        public ViewPtr()
        {
            Objects = new MyList<IMapObj>[(int)MapObjectType.Count];
            for (int x = 0; x < (int)MapObjectType.Count; x++)
                Objects[x] = new MyList<IMapObj>();
        }


        public void AddObject<T>(T obj)
             where T : IMapObj
        {

            Objects[(int)obj.ObjType].Add(obj);
        }

        public void RemoveObject<T>(T obj)
            where T : IMapObj
        {
            Objects[(int)obj.ObjType].Remove(obj);
        }


        public bool ContainObject(MapObjectType obj_t, uint UID)
        {
            for (int x = 0; x < Objects[(int)obj_t].Count; x++)
            {
                var list = Objects[(int)obj_t];
                if (x >= list.Count)
                    break;
                if (list[x].UID == UID)
                    return true;
            }
            return false;
        }

        public bool TryGetObject<T>(MapObjectType obj_t, uint UID, out T obj)
        {
            for (int x = 0; x < Objects[(int)obj_t].Count; x++)
            {
                var list = Objects[(int)obj_t];
                if (x >= list.Count)
                    break;
                if (list[x].UID == UID)
                {
                    obj = (T)list[x];
                    return true;
                }
            }
            obj = default(T);
            return false;
        }
        public MyList<IMapObj> GetObjects(MapObjectType typ)
        {
            return Objects[(int)typ];
        }

        public void Clear(MapObjectType typ)
        {
            Objects[(int)typ].Clear();
        }
    }

    public class Portal
    {
        public ushort MapID { get; set; }
        public ushort X { get; set; }
        public ushort Y { get; set; }

        public ushort Destiantion_MapID { get; set; }
        public ushort Destiantion_X { get; set; }
        public ushort Destiantion_Y { get; set; }
    }
    [Flags]
    public enum MapFlagType : byte
    {
        None = 0,
        Valid = 1 << 0,
        Monster = 1 << 1,
        Item = 1 << 2,
        Player = 1 << 3

    }
    [Flags]
    public enum MapTypeFlags
    {
        Normal = 0,
        PkField = 1 << 0,
        ChangeMapDisable = 1 << 1,
        RecordDisable = 1 << 2,
        PkDisable = 1 << 3,
        BoothEnable = 1 << 4,
        TeamDisable = 1 << 5,
        TeleportDisable = 1 << 6,
        GuildMap = 1 << 7,
        PrisonMap = 1 << 8,
        FlyDisable = 1 << 9,
        Family = 1 << 10,
        MineEnable = 1 << 11,
        FreePk = 1 << 12,
        NeverWound = 1 << 13,
        DeadIsland = 1 << 14
    }
    public class GameMap
    {
        public uint RecordSteedRace = 0;

        private Dictionary<uint, Role.Player> _offlinePlayers = new Dictionary<uint, Role.Player>();

        public static sbyte[] XDir = new sbyte[]
        {
            -1, -2, -2, -1, 1, 2, 2, 1,
             0, -2, -2, -2, 0, 2, 2, 2,
            -1, -2, -2, -1, 1, 2, 2, 1,
             0, -1, -1, -1, 0, 1, 1, 1,
        };
        public static sbyte[] YDir = new sbyte[]
        {
            2,  1, -1, -2, -2, -1, 1, 2,
            2,  2,  0, -2, -2, -2, 0, 2,
            2,  1, -1, -2, -2, -1, 1, 2,
            1,  1,  0, -1, -1, -1, 0, 1
        };

        // Adiciona um jogador offline à lista
        public void AddOfflinePlayer(Role.Player player)
        {
            lock (_offlinePlayers)
            {
                if (!_offlinePlayers.ContainsKey(player.UID))
                {
                    _offlinePlayers[player.UID] = player;
                    Console.WriteLine($"[{DateTime.Now}] {player.Name} adicionado à lista de offline visíveis.");
                }
            }
        }

        // Remove um jogador offline da lista
        public void RemoveOfflinePlayer(uint uid)
        {
            lock (_offlinePlayers)
            {
                if (_offlinePlayers.ContainsKey(uid))
                {
                    _offlinePlayers.Remove(uid);
                    Console.WriteLine($"[{DateTime.Now}] Jogador UID {uid} removido da lista de offline visíveis.");
                }
            }
        }

        // Retorna a lista de jogadores offline visíveis
        public List<Role.Player> GetOfflinePlayers()
        {
            lock (_offlinePlayers)
            {
                return _offlinePlayers.Values.ToList();
            }
        }


        public static bool IsGate(uint UID)
        {
            return UID == 516076 || UID == 516077 || UID == 516074 || UID == 516075 || UID == 516078 || UID == 516079 || UID == 516080;
        }
        public static bool IsFrozengrotoMaps(uint Map)
        {
            return Map == 1762 || Map == 1927 || Map == 1999 || Map == 2054 || Map == 2055 || Map == 2056;
        }
        public static bool IsMineCave(uint Map)
        {
            return Map == 1028;
        }
        private static List<ushort> UsingMaps = new List<ushort>()
        {
            601,//offlineTG
            700,//arena map,
                //map
            1000,//Desert
            1001,//MysticCastel
            1002,//TwinCity
            1004,//Prommoter
            1005,//Arena
            1006,//Steeding TC
            1008,//color you armors/heah
            1010,//bird vilage
            1011,//PhoenixCastle
            1013,//HalkingCave
            1015,//BirdIsland
            1020,//ApeMoutain
            1036,//Market
            //1039,//TrainingGrounds
            1511,//buy mobila
            1038,//GuildWar
            2068,//elitepk map
            6001,//GuildWarJaill
            1098,1099,2080,601,3024,//house id`s
            1351,1352,1353,1354,//lab`s
            1762,//fg1
            1927,//fg2
            1999,//fg3
            2054,//fg4
            2055,//fg5
            2056,//fg6
            1700,//2nd reborn quest !!!
            3056,//pestera
            1039,//
            6000,//jail
            2057,
            Game.MsgTournaments.MsgClassPKWar.MapID,
            10137,
            10166,
            1767,
            1212,
            1300,
            1571,
            1572,
        };

        public List<Portal> Portals = new List<Portal>();

        public unsafe void SendSysMesage(string Messaj, Game.MsgServer.MsgMessage.ChatMode ChatType = Game.MsgServer.MsgMessage.ChatMode.TopLeft
           , Game.MsgServer.MsgMessage.MsgColor color = Game.MsgServer.MsgMessage.MsgColor.white)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();

                var Packet = new Game.MsgServer.MsgMessage(Messaj, color, ChatType).GetArray(stream);
                foreach (var client in Users)
                    client.Send(Packet);
            }
        }

        public string Name = "";

        public uint BaseID = 0;
        public MapFlagType[,] cells { get; set; }
        public System.Drawing.Size bounds;
        public Game.MsgMonster.MobCollection MonstersColletion;

        public MapView View;

        public bool AddStaticRole(StaticRole role)
        {
            if (View.EnterMap<StaticRole>(role))
            {
                SetFlagNpc(role.X, role.Y);
                return true;
            }
            return false;
        }
        public bool RemoveStaticRole(Role.IMapObj obj)
        {

            if (View.LeaveMap<Role.IMapObj>(obj))
            {
                RemoveFlagNpc(obj.X, obj.Y);
                return true;
            }
            return false;
        }

        public Game.MsgNpc.Npc Magnolia = null;
        public void AddMagnolia(ServerSockets.Packet stream, uint Quality)
        {
            bool Location = false;

            if (Magnolia != null)
            {
                if (Magnolia.X == 99)
                    Location = true;
                RemoveNpc(Magnolia, stream);
            }
            Magnolia = Game.MsgNpc.Npc.Create();
            if (Location)
            {
                Magnolia.UID = 999900;
                Magnolia.X = 106;
                Magnolia.Y = 99;
            }
            else
            {
                Magnolia.UID = 999901;
                Magnolia.X = 99;
                Magnolia.Y = 112;
            }
            Magnolia.ObjType = MapObjectType.Npc;
            Magnolia.NpcType = Flags.NpcType.Talker;
            uint mesh = 0;
            if (Quality % 10 == 7)
                mesh = 10;
            else if (Quality % 10 == 8)
                mesh = 20;
            if (Quality % 10 == 9)
                mesh = 30;
            if (Quality % 10 == 0)
                mesh = 40;
            Magnolia.Mesh = (ushort)(19340 + mesh);
            Magnolia.Map = this.ID;
            AddNpc(Magnolia);
        }

        public string GetMobLoc(uint ID)
        {
            foreach (var monster in View.GetAllMapRoles(MapObjectType.Monster))
            {
                var mob = monster as Game.MsgMonster.MonsterRole;
                if (mob.Family != null)
                    if (mob.Family.ID == ID)
                    {
                        return "(" + mob.X + "," + mob.Y + ")";
                    }
            }
            return "";
        }
        public void GenerateSectorTraps(ushort x, ushort y, int type)
        {
            if (View.CountRoles(MapObjectType.Item, x, y) < 6)
            {
                ushort newx = (ushort)Program.GetRandom.Next(1, 18);
                ushort newy = (ushort)Program.GetRandom.Next(1, 18);
                newx += x;
                newy += y;
                if (IsFlagPresent(newx, newy, MapFlagType.Item) == false && IsFlagPresent(newx, newy, MapFlagType.Valid))
                {
                    var Item = new Game.MsgFloorItem.MsgItem(null, newx, newy, Game.MsgFloorItem.MsgItem.ItemType.Effect, 0, 0, ID, 0, false, this, 60 * 60 * 1000);
                    Item.MsgFloor.m_ID = (uint)type;
                    Item.MsgFloor.m_Color = 2;
                    Item.MsgFloor.DropType = Game.MsgFloorItem.MsgDropID.Effect;
                    cells[newx, newy] |= MapFlagType.Item;
                    View.EnterMap<Role.IMapObj>(Item);


                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        Item.SendAll(stream, MsgDropID.Effect);
                    }
                }
            }
        }
        public void RemoveTrap(ushort x, ushort y, Role.IMapObj item)
        {

            View.LeaveMap<Role.IMapObj>(item);
            cells[item.X, item.Y] &= ~MapFlagType.Item;
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                var ittem = item as Game.MsgFloorItem.MsgItem;
                ittem.SendAll(stream, MsgDropID.RemoveEffect);
            }

        }
        public ConcurrentDictionary<uint, Game.MsgNpc.Npc> soldierRemains = new ConcurrentDictionary<uint, Game.MsgNpc.Npc>();
        public void CheckUpSoldierReamins(Time32 Now)
        {
            List<Game.MsgNpc.Npc> remove = new List<Game.MsgNpc.Npc>();
            foreach (var npc in soldierRemains.Values)
            {
                if (ID == 1000)
                {
                    if (Now > npc.Respawn)
                    {
                        npc.X = (ushort)Program.GetRandom.Next(624 - 32, 624 + 32);
                        npc.Y = (ushort)Program.GetRandom.Next(477 - 32, 477 + 32);
                        AddNpc(npc);
                        remove.Add(npc);
                    }
                }
                else if (ID == 1015)
                {
                    if (npc.UID == 8551)
                    {
                        npc.X = (ushort)Program.GetRandom.Next(551 - 32, 551 + 32);
                        npc.Y = (ushort)Program.GetRandom.Next(342 - 32, 342 + 32);
                        AddNpc(npc);
                        remove.Add(npc);
                    }
                    else
                    {
                        npc.X = (ushort)Program.GetRandom.Next(454 - 90, 454 + 90);
                        npc.Y = (ushort)Program.GetRandom.Next(574 - 90, 574 + 90);
                        AddNpc(npc);
                        remove.Add(npc);
                    }
                }
            }
            foreach (var npc in remove)
            {
                Game.MsgNpc.Npc rem;
                soldierRemains.TryRemove(npc.UID, out rem);
            }
        }

        public void AddNpc(Game.MsgNpc.Npc npc)
        {
            if (!View.MapContain(MapObjectType.Npc, npc.UID))
            {
                View.EnterMap<Role.IMapObj>(npc);
                SetFlagNpc(npc.X, npc.Y);
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    npc.Send(stream);
                }
            }
        }
        public unsafe void RemoveNpc(Game.MsgNpc.Npc npc, ServerSockets.Packet stream)
        {
            if (View.MapContain(MapObjectType.Npc, npc.UID))
            {
                View.LeaveMap<Role.IMapObj>(npc);
                RemoveFlagNpc(npc.X, npc.Y);


                ActionQuery action;

                action = new ActionQuery()
                {
                    ObjId = npc.UID,
                    Type = ActionType.RemoveEntity
                };

                foreach (var client in View.Roles(MapObjectType.Player, npc.X, npc.Y))
                {
                    if (Core.GetDistance(client.X, client.Y, npc.X, npc.Y) <= Game.MsgNpc.Npc.SeedDistance)
                    {
                        client.Send(stream.ActionCreate(&action));
                    }
                }
            }

        }
        public bool ValidLocation(ushort X, ushort Y)
        {
            if (bounds.Width > X && this.bounds.Height > Y)
            {
               // Console.WriteLine(cells[X, Y]);
                return (cells[X, Y] & MapFlagType.Valid) == MapFlagType.Valid;
            }
            return false;
        }
        public bool MonsterOnTile(ushort X, ushort Y)
        {
            if (bounds.Width > X && this.bounds.Height > Y)
            {
                return (cells[X, Y] & MapFlagType.Monster) == MapFlagType.Monster;
            }
            return false;
        }
        public void SetMonsterOnTile(ushort X, ushort Y, bool Value)
        {
            try
            {
                if (Value)
                    cells[X, Y] |= MapFlagType.Monster;
                else
                    cells[X, Y] &= ~MapFlagType.Monster;
            }
            catch (Exception e)
            {
                Console.WriteException(e);
                Console.WriteLine("Problem monsters on map " + ID.ToString());
            }
        }
        public bool SearchNpcInScreen(uint UID, ushort X, ushort Y, out Game.MsgNpc.Npc obj)
        {
            if (View.TryGetObject<Game.MsgNpc.Npc>(UID, MapObjectType.Npc, X, Y, out obj))
            {
                return Core.GetDistance(X, Y, obj.X, obj.Y) < Game.MsgNpc.Npc.SeedDistance;
            }
            obj = default(Game.MsgNpc.Npc);
            return false;
        }


        public uint ID { get; private set; }
        public GameMap(int width, int height, int m_id)
        {
            Clients = new ConcurrentDictionary<uint, Client.GameClient>();
            this.cells = new MapFlagType[width, height];
            this.bounds = new System.Drawing.Size(width, height);

            this.ID = (uint)m_id;
        }

        public static Counter DinamicIDS = new Counter(10000001);

        public uint GenerateDynamicID()
        {
            return DinamicIDS.Next;
        }
        public static void CreateDynamicMap(ushort mapneed, uint ownerid, bool Event)
        {
            string MapPath;
            if (MapContents.TryGetValue(mapneed, out MapPath))
            {
                LoadMap((ushort)ownerid, MapPath, mapneed);
                Console.WriteLine($"CreateDynamicMap ID[{mapneed}] Dynamic[{ownerid}].");
            }
            else Console.WriteLine($"\tMap not found: ID[{mapneed}] Dynamic[{ownerid}].");
        }
        //reviver character
        public ushort Reborn_Map = 0;
        public ushort Reborn_X = 0;
        public ushort Reborn_Y = 0;

        public static Dictionary<int, string> MapContents = new Dictionary<int, string>();
        public static bool CheckMap(uint ID)
        {
            /*#if TEST
                        if (!Database.Server.ServerMaps.ContainsKey(ID))
                            Database.Server.ServerMaps.Add(ID, new GameMap(100, 100, ID));
                        return true;
            #endif*/
            if (!Database.Server.ServerMaps.ContainsKey(ID))
            {
                try
                {
                    LoadMap((int)ID, MapContents[(int)ID]);
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Console.WriteLine("MapID = " + ID + " not exist.");
                    return false;
                }
            }
            return true;
        }
        public MapTypeFlags TypeStatus { get; set; }
        public static void LoadMaps()
        {
            Database.Server.ServerMaps.Clear(); // Limpa o dicionário antes de carregar
            using (var gamemap = new BinaryReader(new FileStream(Path.Combine(Program.ServerConfig.CO2Folder, "ini/gamemap.dat"), FileMode.Open)))
            {
                var amount = gamemap.ReadInt32();
                for (var i = 0; i < amount; i++)
                {

                    var id = gamemap.ReadInt32();
                    var fileName = Encoding.ASCII.GetString(gamemap.ReadBytes(gamemap.ReadInt32()));
                    var puzzleSize = gamemap.ReadInt32();
                    MapContents[id] = fileName.Replace(".7z", ".dmap");
                }
            }
            foreach (var folded in MapContents)
            {

                int id = folded.Key;
                var mapFile = folded.Value;
                LoadMap(id, mapFile);

                // Força mineração nos mapas de mina
                uint[] mineMaps = { 1025, 1026, 1027, 1028, 1029, 1030, 1031, 1032, 1500, 1501, 1502, 1503, 5000 };
                if (mineMaps.Contains((uint)id) && Database.Server.ServerMaps.ContainsKey((uint)id))
                {
                    Database.Server.ServerMaps[(uint)id].TypeStatus |= Role.MapTypeFlags.MineEnable;
                    Console.WriteLine($"[Mining Enabled] MapID {id} agora permite mineração.");
                }

                if (folded.Key == 1780)
                {
                    LoadMap(3830, mapFile, 1780);
                    LoadMap(3831, mapFile, 1780);
                    LoadMap(3832, mapFile, 1780);
                    LoadMap(3834, mapFile, 1780);
                }
                if (folded.Key == 3825)
                {
                    LoadMap(3826, mapFile, 3825);
                    LoadMap(3827, mapFile, 3825);
                    LoadMap(3828, mapFile, 3825);
                    LoadMap(3829, mapFile, 3825);
                }
                if (folded.Key == 2071)
                {
                    LoadMap(160, mapFile, 2071);
                    LoadMap(161, mapFile, 2071);
                    LoadMap(162, mapFile, 2071);
                }
                if (folded.Key == 3825)
                    LoadMap(3833, mapFile, 3825);
                if (folded.Key == 1765)
                    LoadMap(1818, mapFile, 1765);
                if (folded.Key == 1082)
                    LoadMap(1052, mapFile, 1082);
                if (folded.Key == 1004)
                {
                    LoadMap(6072, mapFile, 1004);
                    LoadMap(1782, mapFile, 1004);
                    LoadMap(1783, mapFile, 1004);
                }
                if (folded.Key == 601)
                {
                    LoadMap(1784, mapFile, 601);
                }
                if (folded.Key == 1004)
                {
                    LoadMap(1767, mapFile, 1004);
                    LoadMap(9999, mapFile, 1004);
                }
                if (folded.Key == 700)
                {
                    LoadMap(50, mapFile, 700);
                    LoadMap(51, mapFile, 700);
                    LoadMap(52, mapFile, 700);
                    LoadMap(53, mapFile, 700);
                    LoadMap(5550, mapFile, 700);
                }
                if (folded.Key == 1028)
                {
                    LoadMap(1794, mapFile, 1028);
                }
                if (folded.Key == 1014)
                    LoadMap(1792, mapFile, 1014);
                if (folded.Key == 1765)
                    LoadMap(1791, mapFile, 1765);
                if (folded.Key == 10088)
                {
                    LoadMap(44455, mapFile, 10088);
                    LoadMap(44456, mapFile, 10088);
                    LoadMap(44457, mapFile, 10088);
                }
                if (folded.Key == 10090)
                {
                    LoadMap(44460, mapFile, 10090);
                    LoadMap(44461, mapFile, 10090);
                    LoadMap(44462, mapFile, 10090);
                    LoadMap(44463, mapFile, 10090);
                }
            }
            Console.WriteLine("\tLoaded " + Database.Server.ServerMaps.Count + "maps");
            GC.Collect();

        }

        public uint MapColor = 0;




        public int[,] FloorType;
        public int[,] Altitude;
        public static void LoadMap(int id, string mapFile, uint baseid = 0)
        {
            try
            {
                GameMap ourInst;
                using (var rdr = new BinaryReader(new FileStream(Path.Combine(Program.ServerConfig.CO2Folder, mapFile), FileMode.Open)))
                {
                    rdr.ReadBytes(268);
                    ourInst = new GameMap(rdr.ReadInt32(), rdr.ReadInt32(), id);
                    ourInst.MonstersColletion = new Game.MsgMonster.MobCollection((uint)id);
                    ourInst.View = new MapView(ourInst.bounds.Width, ourInst.bounds.Height);
                    ourInst.MonstersColletion = new Game.MsgMonster.MobCollection((uint)id);
                    ourInst.BaseID = baseid;
                    if (id == 1038)
                    {
                        ourInst.FloorType = new int[ourInst.bounds.Width, ourInst.bounds.Height];
                        ourInst.Altitude = new int[ourInst.bounds.Width, ourInst.bounds.Height];
                    }

                    for (int y = 0; y < ourInst.bounds.Height; y++)
                    {
                        for (int x = 0; x < ourInst.bounds.Width; x++)
                        {
                            ourInst.cells[x, y] = (rdr.ReadInt16() == 0) ? MapFlagType.Valid : MapFlagType.None;
                            if (id == 1038)
                            {
                                ourInst.FloorType[x, y] = rdr.ReadInt16();
                                ourInst.Altitude[x, y] = rdr.ReadInt16();
                            }
                            else
                            {
                                rdr.ReadInt16();
                                rdr.ReadInt16();
                            }
                        }
                        rdr.ReadInt32();
                    }
                }
                if (!Database.Server.ServerMaps.ContainsKey((ushort)id))
                {
                    Database.Server.ServerMaps.Add((ushort)id, ourInst);
                }
                else
                {
                    Console.WriteLine($"Mapa {id} já existe em ServerMaps, ignorando adição");
                }

                int info = baseid != 0 ? (int)baseid : (int)id;

                if (File.Exists(Program.ServerConfig.DbLocation + "maps\\" + info + ".ini"))
                {
                    WindowsAPI.IniFile reader = new WindowsAPI.IniFile("\\maps\\" + info + ".ini");
                    ourInst.TypeStatus = (MapTypeFlags)reader.ReadUInt32("info", "type", 0);
                    ourInst.Reborn_X = reader.ReadUInt16("info", "portal0_x", 0);
                    ourInst.Reborn_Y = reader.ReadUInt16("info", "portal0_y", 0);
                    ourInst.Reborn_Map = reader.ReadUInt16("info", "reborn_map", 0);
                    ourInst.RecordSteedRace = reader.ReadUInt16("info", "race_record", 0);
                    ourInst.MapColor = reader.ReadUInt32("info", "color", 0);
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("\tMap not found: " + id + " - " + mapFile + "");
            }
            catch (Exception e)
            {
                Console.WriteException(e);
            }
        }
        private bool Update = false;
        private Client.GameClient[] Users = new Client.GameClient[0];
        public Client.GameClient[] Values
        {
            get
            {
                if (Update)
                {
                    Users = Clients.Values.ToArray();
                    Update = false;
                }
                return Users;
            }
            set { }
        }
        private ConcurrentDictionary<uint, Client.GameClient> Clients;
        public void Enquer(Client.GameClient client)
        {
            if (Clients.TryAdd(client.Player.UID, client))
            {
                View.EnterMap<Role.IMapObj>(client.Player);
                client.Map = this;
                Update = true;

                // Envia jogadores offline visíveis ao novo jogador
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    foreach (var offlinePlayer in GetOfflinePlayers())
                    {
                        client.Send(offlinePlayer.GetArray(stream, false));
                    }
                }
            }
        }
        public void Denquer(Client.GameClient client)
        {
            Client.GameClient aclient;
            if (Clients.TryRemove(client.Player.UID, out aclient))
            {
                View.LeaveMap<Role.IMapObj>(client.Player);

                Update = true;
            }
        }
        public void SendToRange(ServerSockets.Packet stream,ushort x, ushort y)
        {
            foreach (var pUser in Values)
            {
                if (Role.Core.InScreen(x, y, pUser.Player.X, pUser.Player.Y))
                {
                    pUser.Send(stream);
                }
            }
        }
        public void SetFlagNpc(ushort x, ushort y)
        {
            cells[x, y] = MapFlagType.None;

            ushort limy = (ushort)Math.Min(this.bounds.Height - 1, y + 1);
            ushort limx = (ushort)Math.Min(this.bounds.Width - 1, x + 1);
            ushort xstart = (ushort)Math.Max(x - 1, 0);

            for (ushort ay = (ushort)Math.Max(y - 1, 0); ay <= limy; ay++)
            {
                for (ushort ax = xstart; ax <= limx; ax++)
                {
                    cells[ax, ay] = MapFlagType.None;
                }
            }
        }
        public void SetGateFlagNpc(ushort x, ushort y)
        {
            cells[x, y] = MapFlagType.None;

            ushort limy = (ushort)Math.Min(this.bounds.Height - 2, y + 2);
            ushort limx = (ushort)Math.Min(this.bounds.Width - 2, x + 2);
            ushort xstart = (ushort)Math.Max(x - 2, 0);

            for (ushort ay = (ushort)Math.Max(y - 2, 0); ay <= limy; ay++)
            {
                for (ushort ax = xstart; ax <= limx; ax++)
                {
                    cells[ax, ay] = MapFlagType.None;
                }
            }
        }
        public void RemoveFlagNpc(ushort x, ushort y)
        {
            cells[x, y] = MapFlagType.Valid;

            ushort limy = (ushort)Math.Min(this.bounds.Height - 1, y + 1);
            ushort limx = (ushort)Math.Min(this.bounds.Width - 1, x + 1);
            ushort xstart = (ushort)Math.Max(x - 1, 0);

            for (ushort ay = (ushort)Math.Max(y - 1, 0); ay <= limy; ay++)
            {
                for (ushort ax = xstart; ax <= limx; ax++)
                {
                    cells[ax, ay] = MapFlagType.Valid;
                }
            }
        }
        public bool ContainMobID(uint ID, uint Dynamic = 0)
        {
            foreach (var monster in View.GetAllMapRoles(MapObjectType.Monster))
            {
                var mob = monster as Game.MsgMonster.MonsterRole;
                if (mob.Family != null)
                    if (mob.Family.ID == ID)
                    {
                        if (Dynamic == 0)
                            return true;
                        else
                            return Dynamic == monster.DynamicID;
                    }
            }
            return false;
        }

        public object SyncRoot = new object();
        public void GetRandCoord(ref ushort x, ref ushort y)
        {
            lock (SyncRoot)
            {
                do
                {
                    x = (ushort)Program.GetRandom.Next(20, (ushort)(bounds.Width - 1));
                    y = (ushort)Program.GetRandom.Next(20, (ushort)(bounds.Height - 1));
                }
                while ((cells[x, y] & MapFlagType.Valid) != MapFlagType.Valid);
            }
        }

        public bool IsFlagPresent(int x, int y, MapFlagType flag)
        {
            if (x > 0 && y > 0 && x < bounds.Width && y < bounds.Height)
                return (cells[x, y] & flag) == flag;
            return false;
        }
        public bool EnqueueItem(Game.MsgFloorItem.MsgItem item)
        {
            return View.EnterMap<Role.IMapObj>(item);
        }
        public bool IsValidFlagNpc(ushort x, ushort y)
        {
            ushort limy = (ushort)Math.Min(this.bounds.Height - 1, y + 1);
            ushort limx = (ushort)Math.Min(this.bounds.Width - 1, x + 1);
            ushort xstart = (ushort)Math.Max(x - 1, 0);

            for (ushort ay = (ushort)Math.Max(y - 1, 0); ay <= limy; ay++)
            {
                for (ushort ax = xstart; ax <= limx; ax++)
                {
                    if (!this.IsFlagPresent(x, y, MapFlagType.Valid))
                        return false;
                }
            }
            return true;
        }
        public bool AddGuildTeleporterItem(ref ushort x, ref ushort y)
        {
            if (IsValidFlagNpc(x, y))
            {
                ushort limy = (ushort)Math.Min(this.bounds.Height - 6, y + 6);
                ushort limx = (ushort)Math.Min(this.bounds.Width - 6, x + 6);
                ushort xstart = (ushort)Math.Max(x - 6, 0);
                ushort ystart = (ushort)Math.Max(y - 6, 0);

                for (ushort ay = ystart; ay <= limy; ay++)
                {
                    for (ushort ax = xstart; ax <= limx; ax++)
                    {
                        if (IsValidFlagNpc(ax, ay))
                        {
                            x = ax;
                            y = ay;

                            cells[ax, ay] |= MapFlagType.Item;

                            return true;
                        }
                    }
                }
                x = 0;
                y = 0;
                return false;
            }

            cells[x, y] |= MapFlagType.Item;
            return true;
        }
        public bool AddGroundItem(ref ushort x, ref ushort y, byte Range = 0)
        {
            if (this.IsFlagPresent(x, y, MapFlagType.Item) || !this.IsFlagPresent(x, y, MapFlagType.Valid))
            {
                ushort limy = (ushort)Math.Min(this.bounds.Height - (1 + Range), y + (1 + Range));
                ushort limx = (ushort)Math.Min(this.bounds.Width - (1 + Range), x + (1 + Range));
                ushort xstart = (ushort)Math.Max(x - (1 + Range), 0);
                ushort ystart = (ushort)Math.Max(y - (1 + Range), 0);

                for (ushort ay = ystart; ay <= limy; ay++)
                {
                    for (ushort ax = xstart; ax <= limx; ax++)
                    {
                        if (!this.IsFlagPresent(ax, ay, MapFlagType.Item))
                        {
                            if (this.IsFlagPresent(ax, ay, MapFlagType.Valid))
                            {
                                x = ax;
                                y = ay;

                                cells[ax, ay] |= MapFlagType.Item;

                                return true;
                            }
                        }
                    }
                }
                x = 0;
                y = 0;
                return false;
            }

            cells[x, y] |= MapFlagType.Item;
            return true;
        }
     
        public Tuple<ushort, ushort> RandomJump(int _x, int _y, int radius)
        {
            int times = 10000;
            int x = _x + Core.Random.Sign() * Core.Random.Next(radius),
                y = _y + Core.Random.Sign() * Core.Random.Next(radius);
            while (times-- > 0)
            {
                if (!ValidLocation((ushort)x, (ushort)y))
                {
                    x = _x + Core.Random.Sign() * Core.Random.Next(radius);
                    y = _y + Core.Random.Sign() * Core.Random.Next(radius);
                }
                else break;
            }
            return new Tuple<ushort, ushort>((ushort)x, (ushort)y);
        }

    }
}
