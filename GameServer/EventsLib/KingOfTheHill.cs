using COServer.Role;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COServer.EventsLib
{
    public class KingOfTheHill : BaseEvent
    {
        public bool isChosen = false;
        public KingOfTheHill()
            : base(8504, "King Of The Hill", 100, Game.MsgServer.MsgStaticMessage.Messages.KingOfTheHill) //prize in silvers 100k , hour, minutes
        {
            Database.Server.Dmg1Maps.Add(map);
        }
        DateTime lastSent = DateTime.Now;
        List<string> score = new List<string>();
        public override void worker()
        {
            base.worker();
            score.Clear();
            foreach (var player in Database.Server.GamePoll.Values.Where(e => e.Player.Map == map && e.Player.Alive))
                if (IsInCenter(player.Player.X, player.Player.Y))
                    player.KingOfTheHill += 2;

            foreach (var player in Database.Server.GamePoll.Values
                .Where(e => e.Player.Map == map)
                .OrderByDescending(e => e.KingOfTheHill)
                .Take(5))
                score.Add(player.Player.Name + " : " + player.KingOfTheHill + " pts.");
            if (DateTime.Now > lastSent.AddSeconds(2))
            {
                SendScore(score);
                lastSent = DateTime.Now;
            }
        }
        public void SendScore(List<string> text)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                foreach (var C in Database.Server.GamePoll.Values.Where(e => e.Player.Map == map))
                {
                    C.Send(new Game.MsgServer.MsgMessage("King Of The Hill - Scores", "ALLUSERS", "SYSTEM", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.FirstRightCorner).GetArray(stream));
                    foreach (string t in text)
                        C.Send(new Game.MsgServer.MsgMessage(t, "ALLUSERS", "SYSTEM", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.ContinueRightCorner).GetArray(stream));
                }
            }
        }
        public static bool IsInCenter(int x, int y)
        {
            return MyMath.PointDistance(x, y, 50, 50) <= 12;
        }

    }
}
