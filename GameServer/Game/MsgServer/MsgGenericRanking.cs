using COServer.ServerSockets;
using System;
using System.Linq;

namespace COServer.Game.MsgServer
{
    public static unsafe partial class MsgBuilder
    {
        public static void GetGenericRanking(this Packet stream, out MsgGenericRanking.Action Mode, out MsgGenericRanking.RankType ranktyp, out byte Page)
        {
            Mode = (MsgGenericRanking.Action)stream.ReadUInt32();
            ranktyp = (MsgGenericRanking.RankType)stream.ReadUInt32();
            Page = stream.ReadUInt8();
        }

        public static Packet GenericRankingCreate(this Packet stream, MsgGenericRanking.Action Mode, MsgGenericRanking.RankType ranktyp, byte Page, byte count)
        {
            stream.InitWriter();
            stream.Write((uint)Mode);
            stream.Write((uint)ranktyp);
            stream.Write(Page);
            stream.Write(count);
            return stream;
        }

        public static unsafe ServerSockets.Packet AddItemGenericRankingCreate(this ServerSockets.Packet stream, int Rank, uint Amount, uint UID, string name)
        {
            stream.Write((long)Rank);
            // stream.Write(Rank);
            stream.Write((long)Amount);
            //stream.Write(Amount);
            stream.Write(UID);
            stream.Write(UID);
            stream.Write(name, 16);
            stream.Write(name, 16);
            //stream.Write(0);
            //stream.Write(0);
            //stream.Write(0);
            //stream.Write(0);
            //stream.Write((long)(0));
            return stream;
        }

        public static Packet AddItemGenericRankingCreate2(this Packet stream, int Rank, uint Amount, uint UID, string name)
        {
            string format = $"{Rank} {Amount} {UID} {UID} {name} {name}";
            stream.Write((byte)format.Length);
            stream.Write(format, format.Length);
            return stream;
        }

        public static unsafe ServerSockets.Packet AddItemGenericRankingCreate(this ServerSockets.Packet stream, int Rank, uint Amount, uint UID, string name
           , uint Level, uint Class, uint Mesh)
        {
            stream.Write((long)Rank);
            stream.Write((long)Amount);
            stream.Write(UID);
            stream.Write(UID);
            stream.Write(name, 16);
            stream.Write(name, 16);
            stream.Write(Level);
            stream.Write(Class);
            stream.Write(Mesh);
            stream.Write(0);
            stream.Write((long)(0));
            return stream;
        }
        public static unsafe ServerSockets.Packet AddItemGenericRankingCreate(this ServerSockets.Packet stream, int Rank, uint Amount, uint UID1, uint UID2, string name
          , uint Level, uint Class, uint Mesh)
        {
            stream.Write((long)Rank);
            stream.Write((long)Amount);
            stream.Write(UID1);
            stream.Write(UID2);
            stream.Write(name, 16);
            stream.Write(name, 16);
            stream.Write(Level);
            stream.Write(Class);
            stream.Write(Mesh);
            stream.Write(0);
            stream.Write((long)(0));
            return stream;
        }
        public static unsafe ServerSockets.Packet GenericRankingFinalize(this ServerSockets.Packet stream)
        {
            stream.Finalize(GamePackets.GenericRanking);
            return stream;
        }
    }

    public unsafe struct MsgGenericRanking
    {
        public enum Action : uint
        {
            Ranking = 1,
            QueryCount = 2,
            UpdateScreen = 4,
            InformationRequest = 5,
            PrestigeRanks = 6
        }
        public enum RankType : uint
        {
            None = 0,
            RoseFairy = 30000002,
            LilyFairy = 30000102,
            OrchidFairy = 30000202,
            TulipFairy = 30000302,
            Kiss = 30000402,
            Love = 30000502,
            Tins = 30000602,
            Jade = 30000702,

            Chi = 60000000,
            DragonChi = 60000001,
            PhoenixChi = 60000002,
            TigerChi = 60000003,
            TurtleChi = 60000004,
            InnerPower = 70000000,
            PrestigeRank = 80000000,

            TopTrojans = 80000001,
            TopWarriors = 80000002,
            TopArchers = 80000003,
            TopDraonWarriors = 80000007,
            TopWaters = 80000008,
            TopFires = 80000009,
            TopWindWalker = 80000010
        }

        public static object SynRoot = new object();
        [PacketAttribute(GamePackets.GenericRanking)]
        private static void Process(Client.GameClient user, Packet stream)
        {
            try
            {
                const int max = 10;
                stream.GetGenericRanking(out Action Mode, out RankType ranktyp, out byte Page);
                switch (Mode)
                {
                    case Action.QueryCount:
                        {
                            lock (SynRoot)
                            {
                                if (Role.Core.IsGirl(user.Player.Body))
                                    user.Player.Flowers.UpdateMyRank(user);
                            }
                            break;
                        }
                    case Action.Ranking:
                        {
                            var OldRank = ranktyp;
                            if (ranktyp >= RankType.RoseFairy && ranktyp <= RankType.TulipFairy)
                            {
                                Role.Instance.Flowers.Flower[] Powers = null;
                                if (ranktyp == RankType.RoseFairy)
                                    Powers = Program.GirlsFlowersRanking.RedRoses.Values.ToArray();
                                else if (ranktyp == RankType.OrchidFairy)
                                    Powers = Program.GirlsFlowersRanking.Orchids.Values.ToArray();
                                else if (ranktyp == RankType.LilyFairy)
                                    Powers = Program.GirlsFlowersRanking.Lilies.Values.ToArray();
                                else if (ranktyp == RankType.TulipFairy)
                                    Powers = Program.GirlsFlowersRanking.Tulips.Values.ToArray();
                                if (Powers == null) return;
                                int offset = Page * max;
                                int count = Math.Min(max, Powers.Length);
                                stream.GenericRankingCreate(Action.Ranking, ranktyp, Page, (byte)count);
                                for (byte x = 0; x < count; x++)
                                {
                                    if (x + offset >= Powers.Length)
                                        break;
                                    var entity = Powers[x + offset];
                                    stream.AddItemGenericRankingCreate2(entity.Rank, entity.Amount, entity.UID, entity.Name);
                                }
                                user.Send(stream.GenericRankingFinalize());
                            }
                            break;
                        }
                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
    }
}
