using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static COServer.Game.MsgServer.MsgFlower;

namespace COServer.Game.MsgServer
{

    public static unsafe partial class MsgBuilder
    {

        public static void GetFlower(this ServerSockets.Packet stream, out MsgFlower.FlowerAction action
           , out uint UID, out uint ItemUID, out uint SendAmount, out FlowersType flowersType)
        {
            action = (FlowerAction)stream.ReadUInt32();
            UID = stream.ReadUInt32();
            ItemUID = stream.ReadUInt32();
            SendAmount = 0;
            flowersType = 0;
            var list = stream.ReadStringList();
            if (list.Length > 0)
            {
                string[] ValueSplit = list[0].Split(' ');
                UID = uint.Parse(ValueSplit[0]);
                SendAmount = uint.Parse(ValueSplit[1]);
                flowersType = (FlowersType)int.Parse(ValueSplit[2]);
            }
        }

        public static ServerSockets.Packet FlowerCreate(this ServerSockets.Packet stream, MsgFlower.FlowerAction action,
            string SenderName, string ReceiverName, uint SendAmount, MsgFlower.FlowersType FlowerTyp)
        {
            stream.InitWriter();
            stream.Write((uint)action);
            stream.Write(0);
            stream.Write(0);
            string[] Format = { $"{SenderName} {ReceiverName} {SendAmount} {(uint)FlowerTyp}" };
            stream.Write(Format);
            stream.Finalize(GamePackets.FlowerPacket);
            return stream;
        }

        public static ServerSockets.Packet FlowerIconCreate(this ServerSockets.Packet stream, MsgFlower.FlowerAction action, Role.Instance.Flowers flowers)
        {
            stream.InitWriter();
            stream.Write((uint)action);
            stream.Write(0);
            stream.Write(0);
            stream.Write(flowers.Format);
            stream.Finalize(GamePackets.FlowerPacket);
            return stream;
        }
    }

    public unsafe struct MsgFlower
    {
        public enum FlowerAction
        {
            FlowerSend,
            FlowerIcon,
        }
        public enum FlowersType : uint
        {
            RedRoses = 0,
            Lilies = 1,
            Orchids = 2,
            Tulips = 3,
        }
        public enum FlowerEffect : uint
        {
            None = 0,
            RedRoses = 1,
            Lilies = 2,
            Orchids = 3,
            Tulips = 4,
        }

        [PacketAttribute(GamePackets.FlowerPacket)]
        public unsafe static void Handler(Client.GameClient user, ServerSockets.Packet packet)
        {
            packet.GetFlower(out FlowerAction action, out uint UID, out uint ItemUID, out uint SendAmount, out FlowersType FlowerTyp);
            if (Role.Core.IsBoy(user.Player.Body) && action == FlowerAction.FlowerSend)
            {
                switch (ItemUID)
                {
                    case 0:
                        {
                            if (user.Player.Flowers.FreeFlowers > 0)
                            {
                                Role.IMapObj obj;
                                if (user.Player.View.TryGetValue(UID, out obj, Role.MapObjectType.Player))
                                {
                                    Role.Player Target = obj as Role.Player;
                                    if (Role.Core.IsGirl(Target.Body))
                                    {
                                        if (!Role.Instance.Flowers.ClientPoll.ContainsKey(Target.UID))
                                            Role.Instance.Flowers.ClientPoll.TryAdd(Target.UID, Target.Flowers);
                                        Target.Flowers.RedRoses += user.Player.Flowers.FreeFlowers;
                                        Program.GirlsFlowersRanking.UpdateRank(Target.Flowers.RedRoses, FlowerTyp);
                                        var stream = packet.FlowerCreate(action, user.Player.Name, Target.Name, user.Player.Flowers.FreeFlowers, FlowersType.RedRoses);
                                        Target.Send(stream);
                                        user.Send(stream);
                                        user.Player.Flowers.FreeFlowers = 0;
                                    }
                                }
                            }
                            break;
                        }
                    default:
                        {
                            if (user.Inventory.TryGetItem(ItemUID, out MsgGameItem GameItem))
                            {
                                Role.IMapObj obj;
                                if (user.Player.View.TryGetValue(UID, out obj, Role.MapObjectType.Player))
                                {
                                    Role.Player Target = obj as Role.Player;
                                    if (Role.Core.IsGirl(Target.Body))
                                    {
                                        if (!Role.Instance.Flowers.ClientPoll.ContainsKey(Target.UID))
                                            Role.Instance.Flowers.ClientPoll.TryAdd(Target.UID, Target.Flowers);
                                        if (SendAmount != Database.Server.ItemsBase[GameItem.ITEM_ID].Durability)
                                            break;
                                        var Flowers = Target.Flowers.SingleOrDefault(p => p.Type == FlowerTyp);
                                        if (Flowers != null)
                                        {
                                            Flowers += SendAmount;
                                            Program.GirlsFlowersRanking.UpdateRank(Flowers, FlowerTyp);
                                            uint FlowersToday = Target.Flowers.AllFlowersToday();
                                            Program.FlowersRankToday.UpdateRank(Target.UID, FlowersToday);
                                            var stream = packet.FlowerCreate(action, user.Player.Name, Target.Name, SendAmount, FlowerTyp);
                                            Target.Send(stream);
                                            user.Send(stream);
                                            user.Inventory.Update(GameItem, Role.Instance.AddMode.REMOVE, packet);
                                        }
                                    }
                                }
                            }
                            break;
                        }
                }
            }
        }
    }
}
