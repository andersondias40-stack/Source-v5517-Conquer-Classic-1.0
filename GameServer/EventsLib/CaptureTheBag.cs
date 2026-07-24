using COServer.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COServer.EventsLib
{
    public class CaptureTheBag : BaseEvent
    {
        public static bool Red = false, Blue = false;
        public static bool RedOnFloor = false, BlueOnFloor = false;
        public int ScoreBlue = 0, ScoreRed = 0;
        public CaptureTheBag()
            : base(8516, "Capture The Bag", 100, Game.MsgServer.MsgStaticMessage.Messages.CaptureTheBag)
        {
        }
        DateTime lastSent = DateTime.Now;
        List<string> score = new List<string>();
        public void TeleportPlayersToMap()
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                Dictionary<uint, Role.Player> TeamOne = new Dictionary<uint, Role.Player>();
                Dictionary<uint, Role.Player> TeamTwo = new Dictionary<uint, Role.Player>();
                ushort X = 0, Y = 0;
                foreach (Client.GameClient c in Database.Server.GamePoll
                    .Values
                    .Where(e => e.Player.Map == 1767))
                {
                    if (TeamOne.Count <= TeamTwo.Count)
                    {
                        TeamOne.Add(c.Player.UID, c.Player);
                        c.SendSysMesage("Congratulations! You've joined the Blue Team!");
                        X = (ushort)(95 + Role.Core.Random.Next(0, 3) - Role.Core.Random.Next(0, 3));
                        Y = (ushort)(35 + Role.Core.Random.Next(0, 3) - Role.Core.Random.Next(0, 3));
                        c.TeamColor = CTBTeam.Blue;
                    }
                    else
                    {
                        TeamTwo.Add(c.Player.UID, c.Player);
                        c.SendSysMesage("Congratulations! You've joined the Red Team!");
                        X = (ushort)(175 + Role.Core.Random.Next(0, 3) - Role.Core.Random.Next(0, 3));
                        Y = (ushort)(205 + Role.Core.Random.Next(0, 3) - Role.Core.Random.Next(0, 3));
                        c.TeamColor = CTBTeam.Red;
                    }
                    c.HasBag = false;
                    c.Player.RemoveFlag(Game.MsgServer.MsgUpdate.Flags.Flashy);
                    c.Player.RemoveFlag(Game.MsgServer.MsgUpdate.Flags.Fly);
                    c.Player.RemoveFlag(Game.MsgServer.MsgUpdate.Flags.Cyclone);
                    c.Player.RemoveFlag(Game.MsgServer.MsgUpdate.Flags.Superman);
                    c.Teleport(X, Y, map);
                    c.Player.Revive(stream);
                    c.Equipment.Show(stream, false);
                }
                DropBlue();
                DropRed();
            }
        }
        void TeleAfterRevive(Client.GameClient C)
        {
            ushort X1 = 0;
            ushort Y1 = 0;
            if (C.TeamColor == CTBTeam.Blue)
            {
                X1 = (ushort)(95 + Role.Core.Random.Next(0, 3) - Role.Core.Random.Next(0, 3));
                Y1 = (ushort)(35 + Role.Core.Random.Next(0, 3) - Role.Core.Random.Next(0, 3));
            }
            else
            {
                X1 = (ushort)(175 + Role.Core.Random.Next(0, 3) - Role.Core.Random.Next(0, 3));
                Y1 = (ushort)(205 + Role.Core.Random.Next(0, 3) - Role.Core.Random.Next(0, 3));
            }
            C.Teleport(X1, Y1, map);
        }
        public override void worker()
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                base.worker();
                score.Clear();

                #region Worker

                bool pass_red = false, pass_blue = false;
                int red_counter = 0, blue_counter = 0;


                foreach (var player in Database.Server.GamePoll
                       .Values
                       .Where(e => e.Player.Map == map))
                {
                    if (player.TeamColor == CTBTeam.Red)
                        red_counter++;
                    else
                        blue_counter++;
                    if (player.HasBag)
                        if (player.TeamColor == CTBTeam.Blue)
                            pass_red = true;
                        else
                            pass_blue = true;
                    if (!player.Player.Alive)
                    {
                        if (DateTime.Now > player.DeathHit.AddSeconds(5))
                        {
                            player.Player.Revive(stream);
                            TeleAfterRevive(player);
                        }
                    }
                    else if (player.Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Flashy))
                    {
                        if (InBase(player))
                        {
                            player.HasBag = false;
                            player.Player.RemoveFlag(Game.MsgServer.MsgUpdate.Flags.Flashy);
                            player.CTBScore += 40;
                            if (player.TeamColor == CTBTeam.Blue)
                            {
                                Red = false;
                                ScoreBlue += 40;
                                DropRed();
                                Broadcast(player.Player.Name + " from the BlueTeam has sucessfully retrieved the RedBag!", BroadCastLoc.Map);
                            }
                            else
                            {
                                Blue = false;
                                ScoreRed += 40;
                                DropBlue();
                                Broadcast(player.Player.Name + " from the RedTeam has sucessfully retrieved the BlueBag!", BroadCastLoc.Map);
                            }
                            TeleAfterRevive(player);
                        }


                    }
                }

                if (!pass_red && !RedOnFloor)
                {
                    Red = false;
                    DropRed();
                }

                if (!pass_blue && !BlueOnFloor)
                {
                    Blue = false;
                    DropBlue();
                }
                #endregion

                #region Winners
                if (DateTime.Now > senton.AddMinutes(10) || red_counter == 0 || blue_counter == 0)
                {
                    if (Database.Server.GamePoll.Values.Where(e => e.Player.Map == map).Count() > 0)
                    {
                        if (ScoreRed > ScoreBlue)
                            Broadcast("Red Team has won the Capture the Bag Event! Congratulations to the winning team!", BroadCastLoc.World);
                        else if (ScoreBlue > ScoreRed)
                            Broadcast("Blue Team has won the Capture the Bag Event! Congratulations to the winning team!", BroadCastLoc.World);
                        else
                            Broadcast("Capture the Bag Event has come to an end and both teams scored the same amount of points!", BroadCastLoc.World);
                    }
                    foreach (var C in Database.Server.GamePoll
                        .Values
                        .Where(e => e.Player.Map == map)
                        .OrderByDescending(e => e.CTBScore))
                    {

                        int _reward = C.CTBScore;
                        if (ScoreRed > ScoreBlue && C.TeamColor == CTBTeam.Red)
                            _reward = Convert.ToUInt16(_reward * 10);
                        else if (ScoreBlue > ScoreRed && C.TeamColor == CTBTeam.Blue)
                            _reward = Convert.ToUInt16(_reward * 10);

                        if (_reward > 250)
                            _reward = 250;
                        uint final_reward = (uint)(_reward * 1000);
                        C.Teleport(439, 388, 1002);
                        C.Equipment.Show(stream, true);
                        C.Player.Money += final_reward;
                        C.SendSysMesage("Congratulations! You've received " + final_reward + " gold for your participation in the Capture the Bag Event!");
                        string reward = $"[EVENT] {C.Player.Name} has claimed {final_reward} gold from CTB.";
                        Database.ServerDatabase.LoginQueue.Enqueue(reward);
                    }
                    return;
                }
                #endregion

                #region Scores
                if (ScoreRed > ScoreBlue)
                {
                    score.Add("RedTeam Score: " + ScoreRed);
                    score.Add("BlueTeam Score: " + ScoreBlue);
                }
                else
                {
                    score.Add("BlueTeam Score: " + ScoreBlue);
                    score.Add("RedTeam Score: " + ScoreRed);
                }
                if (DateTime.Now > lastSent.AddSeconds(2))
                {
                    SendScore(score);
                    lastSent = DateTime.Now;
                }
                #endregion
            }
        }
        public void DropRed(int x = 172, int y = 224)
        {
            var Gmap = Database.Server.ServerMaps[map];
            Game.MsgServer.MsgGameItem DataItem = new Game.MsgServer.MsgGameItem();
            DataItem.ITEM_ID = 710100U;
            Database.ItemType.DBItem DBitem;
            if (Database.Server.ItemsBase.TryGetValue(DataItem.ITEM_ID, out DBitem))
            {
                DataItem.Durability = DataItem.MaximDurability = DBitem.MaxDurability;
            }
            Game.MsgFloorItem.MsgItem DropItem = new Game.MsgFloorItem.MsgItem(DataItem, (ushort)x, (ushort)y, Game.MsgFloorItem.MsgItem.ItemType.Item, 1, 0, map, 0, true, Gmap);
            if (Gmap.EnqueueItem(DropItem))
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    DropItem.SendAll(stream, Game.MsgFloorItem.MsgDropID.Visible);
                }
            }
            Red = true;
            RedOnFloor = true;
        }
        public void DropBlue(int x = 180, int y = 215)
        {
            var Gmap = Database.Server.ServerMaps[map];
            Game.MsgServer.MsgGameItem DataItem = new Game.MsgServer.MsgGameItem();
            DataItem.ITEM_ID = 722741;
            Database.ItemType.DBItem DBitem;
            if (Database.Server.ItemsBase.TryGetValue(DataItem.ITEM_ID, out DBitem))
            {
                DataItem.Durability = DataItem.MaximDurability = DBitem.MaxDurability;
            }
            Game.MsgFloorItem.MsgItem DropItem = new Game.MsgFloorItem.MsgItem(DataItem, (ushort)x, (ushort)y, Game.MsgFloorItem.MsgItem.ItemType.Item, 1, 0, map, 0, true, Gmap);
            if (Gmap.EnqueueItem(DropItem))
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    DropItem.SendAll(stream, Game.MsgFloorItem.MsgDropID.Visible);
                }
            }
            Blue = true;
            BlueOnFloor = true;
        }
        public static bool InBase(Client.GameClient C)
        {
            if (C.TeamColor == CTBTeam.Blue)
            {
                if (C.Player.X >= 91 && C.Player.X <= 96 && C.Player.Y >= 17 && C.Player.Y <= 22)
                    return true;
            }
            else if (C.TeamColor == CTBTeam.Red)
                if (C.Player.X >= 178 && C.Player.X <= 183 && C.Player.Y >= 213 && C.Player.Y <= 218)
                    return true;

            return false;
        }

        public void SendScore(List<string> text)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                foreach (var C in Database.Server.GamePoll.Values.Where(e => e.Player.Map == map))
                {
                    C.Send(new Game.MsgServer.MsgMessage("Capture The Bag - Scores", "ALLUSERS", "SYSTEM", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.FirstRightCorner).GetArray(stream));
                    C.Send(new Game.MsgServer.MsgMessage("My Team : " + C.TeamColor.ToString(), "ALLUSERS", "SYSTEM", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.ContinueRightCorner).GetArray(stream));
                    foreach (string t in text)
                        C.Send(new Game.MsgServer.MsgMessage(t, "ALLUSERS", "SYSTEM", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.ContinueRightCorner).GetArray(stream));
                }
            }
        }
    }
}
