using COServer.Game.MsgFloorItem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COServer.Game.MsgTournaments
{
    public class MsgSquama
    {
        public ProcesType Process { get; set; }
        public DateTime StartTimer = new DateTime();
        public uint SquamaCount = 50;

        public Dictionary<uint, MsgFloorItem.MsgItem> Squama = new Dictionary<uint, MsgFloorItem.MsgItem>();

        private Counter SquamaUID = new Counter(1000);

        public List<uint> Prizes = new List<uint>() { Database.ItemType.Meteor, Database.ItemType.DragonBall };
        public MsgFloorItem.MsgItem Item;

        private Role.GameMap Map;

        // start time 8 >> 5 min remove squama count 

        public MsgSquama()
        {
            Process = ProcesType.Dead;
        }

        public void Open()
        {

            if (Process == ProcesType.Dead)
            {
                Process = ProcesType.Alive;
                StartTimer = DateTime.Now;

                for (byte z = 0; z < SquamaCount; z++)
                {
                    Generate();
                }

                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Squamas have appeared in all maps!", "ALLUSERS", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.TopLeftSystem).GetArray(stream));

                }
            }
        }

        public void Generate()
        {
            if (Process == ProcesType.Alive)
            {
                foreach (var map in Database.Server.SMapName)
                {
                    Map = Database.Server.ServerMaps[map.Key];
                    GenerateSquama(Map);
                }
            }

        }

        public void GenerateSquamaTime(Role.GameMap Maps)
        {
            if (Process == ProcesType.Idle)
            {
                if (DateTime.Now > StartTimer.AddMinutes(5))
                {
                    if (Squama.Count < 5 * 6)
                    {
                        GenerateSquama(Maps);
                    }
                }
            }
            else if (Process == ProcesType.Alive)
            {
                if (DateTime.Now > StartTimer.AddMinutes(15))
                {
                    if (Squama.Count < 2 * 6)
                    {
                        GenerateSquama(Maps);
                    }
                }
                else if (DateTime.Now > StartTimer.AddMinutes(30))
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();

                        foreach (var squa in Squama)
                        {
                            squa.Value.SendAll(stream, MsgDropID.RemoveEffect);
                        }
                    }
                    Squama.Clear();

                    Process = ProcesType.Dead;
                }
                else GenerateSquama(Maps);
            }
        }

        public void GenerateSquama(Role.GameMap Map)
        {
            ushort x = 0, y = 0;
            Map.GetRandCoord(ref x, ref y);

            Console.WriteLine($"Squama appeared in {Map.Name} x = {x}, y {y}");
            Item = new Game.MsgFloorItem.MsgItem(null, x, y, MsgFloorItem.MsgItem.ItemType.Effect, 0, 0, Map.ID, 0, false, Map, 5 * 60);
            Item.MsgFloor.m_ID = 11;
            Item.MsgFloor.m_Color = 2;
            Item.MsgFloor.DropType = Game.MsgFloorItem.MsgDropID.Effect;
            Item.SquamaTrap = true;
            Item.GMap.View.EnterMap<Role.IMapObj>(Item);

            Squama.Add(SquamaUID.Next, Item);
        }

        public void ClaimedReward(Client.GameClient client, uint FloorID)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();

                string Reward = "";
                if (MsgServer.AttackHandler.Calculate.Base.Success(0.05))
                {
                    if (client.Inventory.HaveSpace(1))//award DB
                    {
                        var RND = Program.GetRandom.Next(0, Prizes.Count);
                        var item = Prizes[RND];

                        client.Inventory.Add(stream, item);

                        Reward = Database.Server.ItemsBase[item].Name;
                    }
                }
                else
                {
                    ushort[] money = new ushort[6] { 1000, 5000, 10000, 15000, 20000, 25000 };
                    var prize = money[Program.GetRandom.Next(0, money.Length)];
                    client.Player.Money += prize;
                    Reward += $"{prize} Silver";
                }
                client.SendSysMesage($"Congratulations! {client.Player.Name} has found the squama and received {Reward}.", MsgServer.MsgMessage.ChatMode.System);

                foreach (var squa in Squama.Where(x => x.Key == FloorID))
                {
                    squa.Value.SendAll(stream, MsgDropID.RemoveEffect);
                    Item.GMap.View.LeaveMap<Role.IMapObj>(squa.Value);
                }


                Squama.Remove(FloorID);

                Item.SquamaTrap = false;
                GenerateSquamaTime(client.Map);
            }
        }
    }

}
