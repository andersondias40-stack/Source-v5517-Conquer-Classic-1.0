using COServer.Game.MsgServer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace COServer.Role.Instance
{
    public class Flowers : IEnumerable<Flowers.Flower>
    {
        public class FlowersRankingToday
        {
            public const int File_Size = 30;
            public Dictionary<uint, uint> Flowers;
            public FlowersRankingToday()
            {
                Flowers = new Dictionary<uint, uint>();
            }
            public void UpdateRank(uint UID, uint Amount)
            {

                if (Flowers.Count < File_Size)
                {
                    Calculate(UID, Amount);
                }
                else
                {
                    var array = Flowers.Values.ToArray();
                    if (array[File_Size - 1] <= Amount)
                    {
                        Calculate(UID, Amount);
                    }
                    else
                    {
                        if (Flowers.ContainsKey(UID))
                            Calculate(UID, Amount);
                    }
                }
            }
            public void Calculate(uint UID, uint Amount)
            {
                lock (Flowers)
                {
                    if (!Flowers.ContainsKey(UID))
                        Flowers.Add(UID, Amount);
                    var clients = Flowers.Values.ToArray();
                    var array = Flowers.OrderByDescending(p => p.Value).ToArray();
                    int Rank = 1;
                    Flowers.Clear();
                    foreach (var user_power in array)
                    {
                        if (Rank <= File_Size && Flowers.Count < File_Size)
                        {
                            if (!Flowers.ContainsKey(user_power.Key))
                                Flowers.Add(user_power.Key, user_power.Value);
                        }
                        else
                            break;
                        Rank++;
                    }
                }
            }
        }
        public class FlowerRanking
        {
            public const int File_Size = 100;

            public bool Girls = true;

            public object SynRoot;
            public Dictionary<uint, Flower> RedRoses;
            public Dictionary<uint, Flower> Lilies;
            public Dictionary<uint, Flower> Orchids;
            public Dictionary<uint, Flower> Tulips;

            public FlowerRanking(bool _Girls = true)
            {
                Girls = _Girls;
                SynRoot = new object();
                RedRoses = new Dictionary<uint, Flower>(File_Size);
                Lilies = new Dictionary<uint, Flower>(File_Size);
                Orchids = new Dictionary<uint, Flower>(File_Size);
                Tulips = new Dictionary<uint, Flower>(File_Size);

            }

            public void UpdateRank(Flower AllFlower, MsgFlower.FlowersType Typ)
            {
                lock (SynRoot)
                {
                    switch (Typ)
                    {
                        case MsgFlower.FlowersType.RedRoses:
                            CreateRank(RedRoses, AllFlower);
                            break;
                        case MsgFlower.FlowersType.Lilies:
                            CreateRank(Lilies, AllFlower);
                            break;
                        case MsgFlower.FlowersType.Orchids:
                            CreateRank(Orchids, AllFlower);
                            break;
                        case MsgFlower.FlowersType.Tulips:
                            CreateRank(Tulips, AllFlower);
                            break;
                    }
                }
            }
            public void CreateRank(Dictionary<uint, Flower> AllRank, Flower AllFlower)
            {
                if (AllRank.Count < File_Size)
                {
                    Calculate(AllRank, AllFlower);
                }
                else
                {
                    var array = AllRank.Values.ToArray();
                    if (array[99].Amount <= AllFlower.Amount)
                    {
                        Calculate(AllRank, AllFlower);
                    }
                    else
                    {
                        if (AllRank.ContainsKey(AllFlower.UID))
                            Calculate(AllRank, AllFlower);
                    }
                }
            }
            public void Calculate(Dictionary<uint, Flower> FlowerRank, Flower Flower)
            {
                lock (FlowerRank)
                {
                    if (!FlowerRank.ContainsKey(Flower.UID))
                        FlowerRank.Add(Flower.UID, Flower);
                    var clients = FlowerRank.Values.ToArray();
                    var array = clients.OrderByDescending(p => p.Amount).ToArray();
                    int Rank = 1;
                    FlowerRank.Clear();
                    foreach (var user_power in array)
                    {
                        int OldRank = user_power.Rank;
                        user_power.Rank = Rank;
                        if (user_power.UID == Flower.UID)
                            Flower.Rank = Rank;
                        if (Rank <= File_Size && FlowerRank.Count < File_Size && user_power.Amount > 0)
                        {
                            if (!FlowerRank.ContainsKey(user_power.UID))
                                FlowerRank.Add(user_power.UID, user_power);
                        }
                        else
                        {
                            Rank = 0;
                            Flower.Rank = 0;
                            user_power.Rank = 0;
                        }
                        if (OldRank != user_power.Rank)
                        {
                            if (Girls)
                                UpdateRank(Flower.UID, Flower);
                        }
                        Rank++;
                    }
                }
            }
            public unsafe void UpdateRank(uint UID, Flower Flower)
            {
                Client.GameClient user;
                if (Database.Server.GamePoll.TryGetValue(UID, out user))
                {
                    user.Player.Flowers.UpdateMyRank(user, true);
                }
            }
        }

        public class Flower
        {
            public MsgFlower.FlowersType Type;
            public uint Amount;
            public uint Amount2day;

            public int Rank;
            public string Name;
            public uint UID;

            public override string ToString()
            {
                return $"{Amount} {Amount2day}";
            }

            public Flower(MsgFlower.FlowersType typ, uint _uid, string _name)
            {
                UID = _uid;
                Name = _name;
                Type = typ;
            }

            public byte GetGrade()
            {
                switch (Type)
                {
                    case MsgFlower.FlowersType.RedRoses: return 1;
                    case MsgFlower.FlowersType.Tulips: return 2;
                    case MsgFlower.FlowersType.Orchids: return 3;
                    case MsgFlower.FlowersType.Lilies: return 4;
                }
                return 0;
            }
            public static implicit operator uint(Flower Data)
            {
                return Data.Amount;
            }
            public static Flower operator +(Flower Data, uint amount)
            {
                Data.Amount += amount;
                Data.Amount2day += amount;
                return Data;
            }
        }

        public uint FreeFlowers;
        public string FreeFlowersFormat => $"{FreeFlowers} 0";
        public int Day;
        public bool isMale = false;
        public Flower RedRoses;
        public Flower Lilies;
        public Flower Orchids;
        public Flower Tulips;
        public string[] Format
        {
            get
            {
                if (isMale)
                    return new string[] { $"{FreeFlowers} 0 0 0 0 0 0 0" };
                else return new string[] { $"{RedRoses} {Lilies} {Orchids} {Tulips}" };
            }
        }
        public uint CreateFlowerIcon(Flower flow, bool Today = false, byte Rank = 0)
        {
            uint ID = (uint)(Today ? 30000001 : 30000002);
            if (flow == null)
                return (uint)(ID + GetRank(Rank));

            if (flow.Rank == 0 || flow.Rank > 100)
                return 0;
            return (uint)(ID + (uint)(100 * (byte)flow.Type) + GetRank(flow.Rank));
        }

        public ushort GetRank(int rank)
        {
            if (rank == 1)
                return 0;
            if (rank == 2)
                return 10000;
            if (rank == 3)
                return 20000;
            if (rank > 3)
                return 30000;

            return 0;
        }

        public static System.Collections.Concurrent.ConcurrentDictionary<uint, Flowers> ClientPoll = new System.Collections.Concurrent.ConcurrentDictionary<uint, Flowers>();

        public Flowers(uint UID, string Name, bool _ismale)
        {
            isMale = _ismale;
            if (isMale)
                FreeFlowers = 1;
            RedRoses = new Flower(MsgFlower.FlowersType.RedRoses, UID, Name);
            Lilies = new Flower(MsgFlower.FlowersType.Lilies, UID, Name);
            Orchids = new Flower(MsgFlower.FlowersType.Orchids, UID, Name);
            Tulips = new Flower(MsgFlower.FlowersType.Tulips, UID, Name);
        }

        public uint AllFlowersToday()
        {
            return (uint)this.Sum((x) => x.Amount2day);
        }

        public unsafe void UpdateMyRank(Client.GameClient user, bool ReloadScreen = false)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                var array = this.Where((f1) => f1.Rank >= 1 && f1.Rank <= 100).ToArray();
                Array.Sort(array, (f1, f2) =>
                {
                    int n_rank = f1.Rank.CompareTo(f2.Rank);
                    if (f2.Rank == f1.Rank)
                        return f2.GetGrade().CompareTo(f1.GetGrade());
                    return n_rank;
                });

                if (array.Length > 0)
                {
                    user.Player.FlowerRank = CreateFlowerIcon(array[0]);
                    foreach (var Flowers in array)
                    {
                        stream.GenericRankingCreate(MsgGenericRanking.Action.QueryCount, (MsgGenericRanking.RankType)user.Player.Flowers.CreateFlowerIcon(Flowers), 0, 1);
                        stream.AddItemGenericRankingCreate(Flowers.Rank, Flowers.Amount, Flowers.UID, Flowers.Name);
                        user.Send(stream.GenericRankingFinalize());
                    }
                }
                else
                {
                    if (Program.FlowersRankToday.Flowers.ContainsKey(user.Player.UID))
                    {
                        var RankToday = Program.FlowersRankToday.Flowers;
                        int Rank = 1;
                        foreach (var FToday in RankToday)
                        {
                            if (FToday.Key == user.Player.UID)
                            {
                                user.Player.FlowerRank = CreateFlowerIcon(null, true, (byte)Rank);
                                stream.GenericRankingCreate(MsgGenericRanking.Action.QueryCount, (MsgGenericRanking.RankType)CreateFlowerIcon(null, true, (byte)Rank), 0, 1);
                                stream.AddItemGenericRankingCreate2(Rank, FToday.Value, user.Player.UID, user.Player.Name);
                                user.Send(stream.GenericRankingFinalize());
                                break;
                            }
                            Rank++;
                        }
                    }
                }
                stream.GenericRankingCreate(MsgGenericRanking.Action.InformationRequest, MsgGenericRanking.RankType.None, 0, 0);
                stream.AddItemGenericRankingCreate2(0, 0, 0, "");
                user.Send(stream.GenericRankingFinalize());
                if (ReloadScreen)
                    user.Player.View.SendView(user.Player.GetArray(stream, false), false);
            }
        }

        public IEnumerator<Flower> GetEnumerator()
        {
            yield return RedRoses;
            yield return Lilies;
            yield return Orchids;
            yield return Tulips;
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        public override string ToString()
        {
            Database.DBActions.WriteLine writer = new Database.DBActions.WriteLine('/');
            writer.Add(FreeFlowers);
            foreach (var Flow in this)
            {
                writer.Add(Flow.Amount).Add(Flow.Amount2day);
            }
            return writer.Close();
        }
    }
}