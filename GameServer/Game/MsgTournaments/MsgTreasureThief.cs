using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COServer.Game.MsgTournaments
{
    public class MsgTreasureChests : ITournament
    {
        public const ushort
            MapID = 1780;
        public ProcesType Process { get; set; }
        public int CurrentBoxes = 0;
        public DateTime StartTimer = new DateTime();
        public DateTime BoxesStamp = new DateTime();
        Role.GameMap _map;
        public Role.GameMap Map
        {
            get
            {
                if (_map == null)
                    _map = Database.Server.ServerMaps[MapID];
                return _map;
            }
        }
        public TournamentType Type { get; set; }
        public MsgTreasureChests(TournamentType _type)
        {
            Type = _type;
            Process = ProcesType.Dead;
        }
        public bool InTournament(Client.GameClient user)
        {
            return user.Player.Map == MapID;
        }
        public void Open()
        {
            if (Process != ProcesType.Alive)
            {
                Create();
                foreach (var user in Database.Server.GamePoll.Values)
                    user.Player.CurrentTreasureBoxes = 0;
                Process = ProcesType.Alive;
                StartTimer = DateTime.Now.AddMinutes(5);
                BoxesStamp = DateTime.Now.AddSeconds(15);

            }
        }
        public bool Join(Client.GameClient user, ServerSockets.Packet stream)
        {
            if (user.Player.Level < 15)
            {
                user.SendSysMesage("Need to be level 15 at least.");
                return false;
            }
            if (Process == ProcesType.Alive)
            {
                if (user.Player.OnTransform)
                {
                    user.Player.TransformInfo.FinishTransform();
                }
                user.Player.RemoveFlag(MsgServer.MsgUpdate.Flags.Fly);

                ushort x = 0;
                ushort y = 0;
                Map.GetRandCoord(ref x, ref y);
                user.Teleport(x, y, MapID);
                return true;
            }
            return false;
        }
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
                np.Map = MapID;
                np.X = x;
                np.Y = y;
                Map.AddNpc(np);
            }
            CurrentBoxes = 6;
        }
        public void CheckUp()
        {
            if (Process == ProcesType.Alive)
            {
                if (DateTime.Now > StartTimer)
                {
                    foreach (var user in Map.Values)
                    {
                        user.Teleport(429, 379, 1002);
                    }
                    Process = ProcesType.Dead;
                }
                else if (DateTime.Now > BoxesStamp)
                {
                    GenerateBoxes();
                    BoxesStamp = DateTime.Now.AddSeconds(15);
                }
            }
        }
        public void Reward(Client.GameClient user, Game.MsgNpc.Npc npc, ServerSockets.Packet stream)
        {
            CurrentBoxes -= 1;
            byte rand = (byte)Program.GetRandom.Next(0, 5);
            switch (rand)
            {
                case 0://money
                    {
                        if (Role.Core.Rate(0.002))
                        {
                            user.Player.TreasureBoxesPoint += 5;
                            //MsgSchedules.SendSysMesage(user.Player.Name + " got 5 Point while opening the Treasure Chest!", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.white);
                            user.SendSysMesage("You got 5 Points from opening the Treasure Chest!");

                        }
                        else
                        {
                            user.Player.TreasureBoxesPoint += 1;
                            user.SendSysMesage("You got 1 Point from opening the Treasure Chest!");

                            //MsgSchedules.SendSysMesage(user.Player.Name + " got 1 Point while opening the Treasure Chest!", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.white);
                        }
                        break;
                    }
                case 1://experience
                    {
                        if (Role.Core.Rate(0.002))
                        {
                            user.Player.TreasureBoxesPoint += 5;
                            user.SendSysMesage("You got 5 Points from opening the Treasure Chest!");

                            //MsgSchedules.SendSysMesage(user.Player.Name + " got 5 Point while opening the Treasure Chest!", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.white);
                        }
                        else
                        {
                            user.Player.TreasureBoxesPoint += 1;
                            user.SendSysMesage("You got 1 Point from opening the Treasure Chest!");

                            //MsgSchedules.SendSysMesage(user.Player.Name + " got 1 Point while opening the Treasure Chest!", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.white);
                        }
                        break;
                    }
                case 2://cps
                    {
                        if (Role.Core.Rate(0.001))
                        {
                            user.Player.TreasureBoxesPoint += 5;
                            user.SendSysMesage("You got 5 Points from opening the Treasure Chest!");

                            //MsgSchedules.SendSysMesage(user.Player.Name + " got 5 Point while opening the Treasure Chest!", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.white);
                        }
                        else
                        {
                            user.Player.TreasureBoxesPoint += 1;
                            user.SendSysMesage("You got 1 Point from opening the Treasure Chest!");

                            //MsgSchedules.SendSysMesage(user.Player.Name + " got 1 Point while opening the Treasure Chest!", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.white);
                        }
                        break;
                    }
                case 3://dead.
                    {
                        if (Role.Core.Rate(0.002))
                        {
                            user.Player.TreasureBoxesPoint += 5;
                            user.SendSysMesage("You got 5 Points from opening the Treasure Chest!");

                            //MsgSchedules.SendSysMesage(user.Player.Name + " got 5 Point while opening the Treasure Chest!", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.white);
                        }
                        else
                        {
                            user.Player.TreasureBoxesPoint += 1;
                            user.SendSysMesage("You got 1 Point from opening the Treasure Chest!");

                            //MsgSchedules.SendSysMesage(user.Player.Name + " got 1 Point while opening the Treasure Chest!", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.white);
                        }
                        break;
                    }
                case 4://dead.
                    {
                        user.Player.TreasureBoxesPoint += 1;
                        user.SendSysMesage("You got 1 Point from opening the Treasure Chest!");

                        //MsgSchedules.SendSysMesage(user.Player.Name + " got 1 Point while opening the Treasure Chest!", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.white);
                        break;
                    }
            }
            user.Player.CurrentTreasureBoxes += 1;
            user.SendSysMesage("You got 1 Point from opening the Treasure Chest!");

            user.Player.SendString(stream, MsgServer.MsgStringPacket.StringID.Effect, true, "accession1");
            Map.RemoveNpc(npc, stream);

            ShuffleGuildScores(stream);

        }
        public void ShuffleGuildScores(ServerSockets.Packet stream)
        {
            foreach (var user in Map.Values)
            {
                Game.MsgServer.MsgMessage msg = new MsgServer.MsgMessage("---Your Score: " + user.Player.CurrentTreasureBoxes + "---", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.FirstRightCorner);
                user.Send(msg.GetArray(stream));
            }
            var array = Map.Values.OrderByDescending(p => p.Player.CurrentTreasureBoxes).ToArray();
            for (int x = 0; x < Math.Min(5, Map.Values.Length); x++)
            {
                var element = array[x];
                Game.MsgServer.MsgMessage msg = new MsgServer.MsgMessage("No " + (x + 1).ToString() + "- " + element.Player.Name + " Opened " + element.Player.CurrentTreasureBoxes.ToString() + " Boxes!", MsgServer.MsgMessage.MsgColor.yellow, MsgServer.MsgMessage.ChatMode.ContinueRightCorner);
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
