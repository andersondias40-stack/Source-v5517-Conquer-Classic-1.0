using COServer.Game.MsgServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COServer.Game.MsgTournaments
{
    public class LuckyBox
    {
        public int CurrentBoxes = 0;
        public int AllEventBoxes = 15;
        public DateTime BoxesStamp = new DateTime();
        public const int FinishMinutes = 3;
        public const uint map = 130;
        private ProcesType Mode;
        private DateTime FinishTimer = new DateTime();
        private string Title = "Top_LuckyBox";
        public uint WinnerUID = 0;
        Role.GameMap _map;
        public Role.GameMap Map
        {
            get
            {
                if (_map == null)
                    _map = Database.Server.ServerMaps[map];
                return _map;
            }
        }
        public LuckyBox()
        {
            Mode = ProcesType.Dead;
            if (!Program.OutMap.Contains(map))
                Program.OutMap.Add(map);
        }
        public void Open()
        {
            if (Mode == ProcesType.Dead)
            {
                Mode = ProcesType.Alive;
                FinishTimer = DateTime.Now.AddMinutes(FinishMinutes);
                BoxesStamp = DateTime.Now.AddSeconds(40);
            }
        }
        public bool AllowJoin(Client.GameClient user, ServerSockets.Packet stream)
        {
            if (Mode == ProcesType.Alive)
            {
                ushort x = 0;
                ushort y = 0;
                Database.Server.ServerMaps[map].GetRandCoord(ref x, ref y);
                user.Teleport(x, y, map);
                return true;
            }
            return false;
        }
        public void CheckUp()
        {
            if (Mode == ProcesType.Alive)
            {
                if (DateTime.Now > BoxesStamp)
                {
                    GenerateBoxes();
                    BoxesStamp = DateTime.Now.AddSeconds(30);
                }
                foreach (var player in Database.Server.GamePoll.Values
                    .Where(e => e.Player.Map == map))
                {
                    if (!player.Player.Alive && DateTime.Now > player.DeathHit.AddSeconds(2))
                    {
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            player.Player.Revive(stream);
                            ushort x = 0;
                            ushort y = 0;
                            Map.GetRandCoord(ref x, ref y);
                            player.Teleport(x, y, map);
                        }
                    }
                }
                if (DateTime.Now > FinishTimer)
                {
                    Mode = ProcesType.Dead;
                }
            }
        }
        public bool IsFinished() { return Mode == ProcesType.Dead; }
        private void Create()
        {
            GenerateBoxes();
        }
        private void GenerateBoxes()
        {
            for (int i = CurrentBoxes; i < 6; i++)
            {
                byte rand = (byte)Program.GetRandom.Next(0, 5);
                ushort x = 0;
                ushort y = 0;
                Map.GetRandCoord(ref x, ref y);

                Game.MsgNpc.Npc np = Game.MsgNpc.Npc.Create();
                while (true)
                {
                    np.UID = (uint)Program.GetRandom.Next(10000, 100000);
                    if (Map.View.Contain(np.UID, x, y) == false)
                        break;
                }
                np.NpcType = Role.Flags.NpcType.Talker;
                switch (rand)
                {
                    case 0: np.Mesh = 9267; break;
                    case 1: np.Mesh = 9267; break;
                    case 2: np.Mesh = 9267; break;
                    case 3: np.Mesh = 9267; break;
                    case 4: np.Mesh = 9267; break;
                    default: np.Mesh = 9267; break;
                }
                np.Map = map;
                np.X = x;
                np.Y = y;
                Map.AddNpc(np);
            }
            CurrentBoxes = 6;
        }
        public void Reward(Client.GameClient user, Game.MsgNpc.Npc npc, ServerSockets.Packet stream)
        {
            CurrentBoxes -= 1;
            byte rand = (byte)Program.GetRandom.Next(0, 4);
            switch (rand)
            {
                case 1://cps
                    {
                        uint value = (uint)Program.GetRandom.Next(100, 400);
                        user.Player.ConquerPoints += value;
                        MsgSchedules.SendSysMesage(user.Player.Name + " got " + value.ToString() + " CPs while opening the TreasureBox!", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
                        user.CreateBoxDialog("You've received " + value + " ConquerPoints.");
                        break;
                    }
                case 2://dead.
                    {
                        user.Player.Dead(null, user.Player.X, user.Player.Y, 0);
                        MsgSchedules.SendSysMesage(user.Player.Name + " found DEATH! while opening the TreasureBox!", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
                        break;
                    }
                case 3://item.
                    {
                        uint[] Items = new uint[]
                        {
                            Database.ItemType.DragonBall,
                            //Database.ItemType.YellowExpBall,
                            //Database.ItemType.Class1MoneyBag,
                            //Database.ItemType.ProfToken,
                            //Database.ItemType.PrayingStoneM,
                            //Database.ItemType.BlackTulip,
                            (uint)(700000 + (uint)Role.Flags.Gem.RefinedDragonGem),
                            (uint)(700000 + (uint)Role.Flags.Gem.RefinedFuryGem),
                            (uint)(700000 + (uint)Role.Flags.Gem.RefinedKylinGem),
                            (uint)(700000 + (uint)Role.Flags.Gem.RefinedMoonGem),
                            (uint)(700000 + (uint)Role.Flags.Gem.RefinedPhoenixGem),
                            (uint)(700000 + (uint)Role.Flags.Gem.RefinedRainbowGem),
                            (uint)(700000 + (uint)Role.Flags.Gem.RefinedVioletGem),
                            (uint)(700000 + (uint)Role.Flags.Gem.SuperDragonGem),
                            (uint)(700000 + (uint)Role.Flags.Gem.SuperFuryGem),
                            (uint)(700000 + (uint)Role.Flags.Gem.SuperKylinGem),
                            (uint)(700000 + (uint)Role.Flags.Gem.SuperMoonGem),
                            (uint)(700000 + (uint)Role.Flags.Gem.SuperPhoenixGem),
                            (uint)(700000 + (uint)Role.Flags.Gem.SuperRainbowGem),
                            (uint)(700000 + (uint)Role.Flags.Gem.SuperVioletGem),
                            Database.ItemType.MeteorScroll,
                            Database.ItemType.Meteor,
                           // Database.ItemType.LifeFruitBasket,
                            Database.ItemType.DragonBall
                        };
                        uint ItemID = Items[Program.GetRandom.Next(0, Items.Length)];
                        Database.ItemType.DBItem DBItem;
                        if (Database.Server.ItemsBase.TryGetValue(ItemID, out DBItem))
                        {
                            if (user.Inventory.HaveSpace(1))
                                user.Inventory.Add(stream, DBItem.ID);
                            else
                                user.Inventory.AddReturnedItem(stream, DBItem.ID);
                            MsgSchedules.SendSysMesage(user.Player.Name + " got " + DBItem.Name + " while opening the TreasureBox!", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            user.Player.CurrentTreasureBoxes += 1;
            Game.MsgServer.MsgStringPacket packet = new Game.MsgServer.MsgStringPacket();
            packet.ID = MsgStringPacket.StringID.Effect;
            packet.UID = npc.UID;
            packet.Strings = new string[1] { "lottery" };
            user.Player.View.SendView(stream.StringPacketCreate(packet), true);
            //user.Player.SendString(stream, MsgStringPacket.StringID.Effect, true, "lottery");
            Map.RemoveNpc(npc, stream);
            ShuffleGuildScores(stream);
        }
        public void ShuffleGuildScores(ServerSockets.Packet stream)
        {
            var array = Map.Values.OrderByDescending(p => p.Player.CurrentTreasureBoxes).ToArray();
            for (int x = 0; x < Math.Min(10, Map.Values.Length); x++)
            {
                var element = array[x];
                Game.MsgServer.MsgMessage msg = new MsgMessage("No " + (x + 1).ToString() + "- " + element.Player.Name + " Opened " + element.Player.CurrentTreasureBoxes.ToString() + " Boxes!", MsgMessage.MsgColor.yellow, MsgMessage.ChatMode.FirstRightCorner);
                Send(msg.GetArray(stream));
            }
        }
        public void Send(ServerSockets.Packet stream)
        {
            foreach (var user in Map.Values)
                user.Send(stream);
        }
    }
}
