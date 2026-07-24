using COServer.EventsLib;

namespace COServer.Game.MsgFloorItem
{
    public unsafe static partial class MsgBuilder
    {
        public static unsafe void GetItemPacket(this ServerSockets.Packet stream, out uint uid)
        {
            //   uint stamp = stream.ReadUInt32();
            uid = stream.ReadUInt32();
        }
        public static unsafe ServerSockets.Packet ItemPacketCreate(this ServerSockets.Packet stream, MsgItemPacket Item)
        {
            stream.InitWriter();
            stream.Write(Item.m_UID);//8
            stream.Write(Item.m_ID);//12
            stream.Write(Item.m_X);//16
            stream.Write(Item.m_Y);//18
            stream.Write((ushort)Item.m_Color);//Item.m_Color);
            stream.Write((byte)Item.DropType);//22
            stream.Finalize(GamePackets.FloorMap);
            return stream;
        }
    }

    public unsafe class MsgItemPacket
    {
        public enum EffectMonsters : uint
        {
            None = 0,
            EarthquakeLeftRight = 1,
            EarthquakeUpDown = 2,
            Night = 4,
            EarthquakeAndNight = 5
        }

        public const uint
            DBShowerEffect = 17;


        public uint m_UID;
        public uint m_ID;
        public ushort m_X;
        public ushort m_Y;
        public ushort MaxLife;
        public MsgDropID DropType;
        public uint Life;
        public byte m_Color;
        public byte m_Color2;
        public uint ItemOwnerUID;
        public byte DontShow;
        public uint GuildID;
        public byte FlowerType;
        public ulong Timer;
        public string Name;
        public uint UnKnow;
        public byte Plus;



        public ushort OwnerX;
        public ushort OwnerY;

        public static MsgItemPacket Create()
        {
            MsgItemPacket item = new MsgItemPacket();
            return item;
        }

        [PacketAttribute(GamePackets.FloorMap)]
        public unsafe static void FloorMap(Client.GameClient client, ServerSockets.Packet packet)
        {
            if (client.InTrade)
                return;
            if (!client.Player.OnMyOwnServer)
                return;

            uint m_UID;

            packet.GetItemPacket(out m_UID);

            MsgFloorItem.MsgItem MapItem;
            if (client.Map.View.TryGetObject<MsgFloorItem.MsgItem>(m_UID, Role.MapObjectType.Item, client.Player.X, client.Player.Y, out MapItem))
            {
                if (MapItem.ToMySelf)
                {
                    if (!MapItem.ExpireMySelf)
                    {
                        if (MapItem.ItemOwner != client.Player.UID)
                        {
                            if (client.Team != null)
                            {
                                if (MapItem.Typ != MsgItem.ItemType.Money &&
                                    (!client.Team.IsTeamMember(MapItem.ItemOwner) || !client.Team.PickupItems))
                                {
                                    client.SendSysMesage("You have to wait a little bit before you can pick up any items dropped from monsters killed by other players.");
                                    return;
                                }
                                else if (MapItem.Typ == MsgItem.ItemType.Money)
                                {
                                    if (!client.Team.PickupMoney)
                                    {
                                        client.SendSysMesage("You have to wait a little bit before you can pick up any items dropped from monsters killed by other players.");
                                        return;
                                    }
                                }
                            }
                            else if (client.Team == null)
                            {
                                if (MapItem.Typ == MsgItem.ItemType.Money)
                                {
                                    client.SendSysMesage("You have to wait a little bit before you can pick up any items dropped from monsters killed by other players.");
                                    return;
                                }
                                else
                                {
                                    client.SendSysMesage("You have to wait a little bit before you can pick up any items dropped from monsters killed by other players.");
                                    return;
                                }
                            }
                        }
                    }
                }
                if (Role.Core.GetDistance(client.Player.X, client.Player.Y, MapItem.MsgFloor.m_X, MapItem.MsgFloor.m_Y) <= 5)
                {
                    switch (MapItem.Typ)
                    {

                        case MsgItem.ItemType.Money:
                            {

                                client.Player.Money += MapItem.Gold;
                                client.Player.SendUpdate(packet, client.Player.Money, MsgServer.MsgUpdate.DataType.Money);
                                MapItem.SendAll(packet, MsgDropID.Remove);
                                client.Map.cells[MapItem.MsgFloor.m_X, MapItem.MsgFloor.m_Y] &= ~Role.MapFlagType.Item;
                                client.Map.View.LeaveMap<Role.IMapObj>(MapItem);
                                client.SendSysMesage("You've picked up " + MapItem.Gold + " gold.");
                                break;
                            }
                        case MsgItem.ItemType.Item:
                            {
                                Database.ItemType.DBItem DBItem;
                                if (client.Inventory.HaveSpace(1))
                                {
                                    if (Database.Server.ItemsBase.TryGetValue(MapItem.MsgFloor.m_ID, out DBItem))
                                    {

                                      
                                        client.Map.cells[MapItem.MsgFloor.m_X, MapItem.MsgFloor.m_Y] &= ~Role.MapFlagType.Item;
                                        if (MapItem.ItemBase.StackSize > 1)
                                        {
                                            if (MapItem.MsgFloor.m_ID != 710100 && MapItem.MsgFloor.m_ID != 722741)
                                            {
                                                client.Inventory.Update(MapItem.ItemBase, Role.Instance.AddMode.ADD, packet);
                                            }
                                        }
                                        else
                                        {
                                            if (MapItem.MsgFloor.m_ID != 710100 && MapItem.MsgFloor.m_ID != 722741)
                                            {
                                                client.Inventory.Add(MapItem.ItemBase, DBItem, packet);
                                            }
                                        }

                                        if (MapItem.MsgFloor.m_ID == 710100 || MapItem.MsgFloor.m_ID == 722741)
                                        {
                                            if (MapItem.MsgFloor.m_ID == 710100)
                                            {
                                                if (client.TeamColor == EventsLib.CTBTeam.Red)
                                                    client.SendSysMesage("You can't pick up the bag of your own team!", (MsgServer.MsgMessage.ChatMode)2021);
                                                else if (client.TeamColor == EventsLib.CTBTeam.Blue)
                                                {
                                                    EventsLib.EventManager.ctb.Broadcast(client.Player.Name + " from the BlueTeam has picked up the RedBag! Be careful!", BroadCastLoc.Map);
                                                    client.Map.View.LeaveMap<Role.IMapObj>(MapItem);
                                                    MapItem.SendAll(packet, MsgDropID.Remove);
                                                    client.HasBag = true;
                                                    EventsLib.CaptureTheBag.Red = false;
                                                    EventsLib.CaptureTheBag.RedOnFloor = false;
                                                    client.Player.AddFlag(MsgServer.MsgUpdate.Flags.Flashy, 60 * 60 * 24 * 30, false);
                                                }
                                            }
                                            else if (MapItem.MsgFloor.m_ID == 722741)
                                            {
                                                if (client.TeamColor == EventsLib.CTBTeam.Red)
                                                {
                                                    EventsLib.EventManager.ctb.Broadcast(client.Player.Name + " from the RedTeam has picked up the BlueBag! Be careful!", BroadCastLoc.Map);
                                                    client.Map.View.LeaveMap<Role.IMapObj>(MapItem);
                                                    MapItem.SendAll(packet, MsgDropID.Remove);
                                                    client.HasBag = true;
                                                    EventsLib.CaptureTheBag.Blue = false;
                                                    EventsLib.CaptureTheBag.BlueOnFloor = false;
                                                    client.Player.AddFlag(MsgServer.MsgUpdate.Flags.Flashy, 60 * 60 * 24 * 30, false);
                                                }
                                                else if (client.TeamColor == EventsLib.CTBTeam.Blue)
                                                    client.SendSysMesage("You can't pick up the bag of your own team!", (MsgServer.MsgMessage.ChatMode)2021);
                                            }
                                        }
                                        else
                                        {
                                            client.Map.View.LeaveMap<Role.IMapObj>(MapItem);
                                            MapItem.SendAll(packet, MsgDropID.Remove);
                                            client.SendSysMesage("You've picked up a " + DBItem.Name + ".");
                                        }
                                    }
                                }
                                break;
                            }
                        case MsgItem.ItemType.Cps:
                            {
                                Database.ItemType.DBItem DBItem;
                                if (Database.Server.ItemsBase.TryGetValue(MapItem.MsgFloor.m_ID, out DBItem))
                                {
                                    client.Inventory.Add(MapItem.ItemBase, DBItem, packet);
                                    MapItem.SendAll(packet, MsgDropID.Remove);
                                    client.Map.cells[MapItem.MsgFloor.m_X, MapItem.MsgFloor.m_Y] &= ~Role.MapFlagType.Item;
                                    client.Map.View.LeaveMap<Role.IMapObj>(MapItem);
                                    break;
                                }
                                break;
                            }
                    }
                }
            }
        }
    }
}
