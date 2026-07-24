using COServer.Role;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COServer.EventsLib
{
    public class DragonWar : BaseEvent
    {
        public bool ChooseKing = true;
        public DragonWar()
           : base(8530, "Dragon King", 100, Game.MsgServer.MsgStaticMessage.Messages.DragonKing)
        {

        }
        DateTime lastSent = DateTime.Now;
        List<string> score = new List<string>();
        public override void worker()
        {
            base.worker();
            score.Clear();

            if (Database.Server.GamePoll.Values.Where(e => e.Player.Map == map && e.isDragonKing && e.Player.Alive).Count() == 0)
                ChooseKing = true;

            int c = 0;
            foreach (var player in Database.Server.GamePoll.Values
                .Where(e => e.Player.Map == map)
                .OrderByDescending(e => e.DragonwarPts))
            {
                if (c < 5)
                    score.Add(player.Player.Name + " : " + player.DragonwarPts + " pts.");
                c++;
                if (player.DragonwarPts >= 300)
                {
                    EndEvent();
                    return;
                }
                if (player.isDragonKing)
                    if (DateTime.Now > player.DragonWarStamp.AddSeconds(1))
                    {
                        player.DragonWarStamp = DateTime.Now;
                        player.DragonwarPts += 1;
                    }
                if (!player.Player.Alive && DateTime.Now > player.DeathHit.AddSeconds(2))
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        player.Player.Revive(stream);
                        EventsLib.EventManager.RndCoordinates(map, player);
                    }
                }

            }

            if (DateTime.Now > lastSent.AddSeconds(2))
            {
                SendScore(score);
                lastSent = DateTime.Now;
            }
        }

        private void EndEvent()
        {
            int c = 0;
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                foreach (var player in Database.Server.GamePoll.Values
                .Where(e => e.Player.Map == map)
                .OrderByDescending(e => e.DragonwarPts))
                {
                    if (c < 1)
                    {
                        switch (EventManager.rnd.Next(0, 5))
                        {

                            case 0:
                                player.Player.Money += 500000;
                                player.SendSysMesage("You've received 500k gold.");
                                break;

                            case 1:
                                player.Inventory.Add(stream, 730001, 1);
                                player.SendSysMesage("You've received a +1Stone.");
                                break;

                            case 2:
                                player.Inventory.Add(stream, 720027, 1);
                                player.SendSysMesage("You've received a Metscroll.");
                                break;

                            case 3:
                                for (int i = 0; i < 2; i++)
                                    player.Inventory.Add(stream, 1088000, 1);
                                player.SendSysMesage("You've received a DragonBall.");
                                break;

                            case 4:
                                player.Inventory.Add(stream, 720393, 1);
                                player.SendSysMesage("You've received a ExpPill.");
                                break;
                        }

                        player.Player.Money += (uint)((4 - c) * 1000000);
                        Program.SendGlobalPackets.Enqueue(
                      new Game.MsgServer.MsgMessage(player.Player.Name + " has won the Dragon War", "ALLUSERS", "Dragon War", Game.MsgServer.MsgMessage.MsgColor.white, (Game.MsgServer.MsgMessage.ChatMode)2000).GetArray(stream));
                    }
                    if (c == 0)
                        player.Player.AddFlag(Game.MsgServer.MsgUpdate.Flags.DragonWar, StatusFlagsBigVector32.PermanentFlag, false);
                    else if (player.Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.DragonWar))
                        player.Player.RemoveFlag(Game.MsgServer.MsgUpdate.Flags.DragonWar);
                    c++;
                    player.Teleport(438, 358, 1002);
                }
                foreach (var pla in Database.Server.GamePoll.Values)
                    pla.isDragonKing = false;
            }

        }
        public void SendScore(List<string> text)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                foreach (var C in Database.Server.GamePoll.Values.Where(e => e.Player.Map == map))
                {
                    C.Send(new Game.MsgServer.MsgMessage("Dragon War - Scores", "ALLUSERS", "SYSTEM", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.FirstRightCorner).GetArray(stream));
                    C.Send(new Game.MsgServer.MsgMessage("My Points : " + C.DragonwarPts, "ALLUSERS", "SYSTEM", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.ContinueRightCorner).GetArray(stream));
                    foreach (string t in text)
                        C.Send(new Game.MsgServer.MsgMessage(t, "ALLUSERS", "SYSTEM", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.ContinueRightCorner).GetArray(stream));
                }
            }
        }
    }
}
