using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COServer.EventsLib
{
    public class FreezeWar : BaseEvent
    {
        public bool isChosen = false;
        public FreezeWar()
            : base(8515, "Freeze War", 100, Game.MsgServer.MsgStaticMessage.Messages.FreezeWar) //prize in silvers , hour, minutes
        {
        }
        DateTime lastSent = DateTime.Now;
        List<string> score = new List<string>();
        public override void worker()
        {
            base.worker();
            score.Clear();
            int c = 0;
            foreach (var player in Database.Server.GamePoll.Values
                .Where(e => e.Player.Map == map)
                .OrderByDescending(e => e.FreezewarPoints))
            {
                if (c < 5)
                    score.Add(player.Player.Name + " : " + player.FreezewarPoints + " pts.");
                c++;
                if (DateTime.Now > player.FrozenStamp.AddSeconds(5) && player.Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Freeze))
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        player.Player.Revive(stream);
                    }
                    player.Player.RemoveFlag(Game.MsgServer.MsgUpdate.Flags.Freeze);
                    player.Player.RemoveFlag(Game.MsgServer.MsgUpdate.Flags.Fly);
                    player.SendSysMesage("You're not frozen anymore.");
                }
            }
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
                    C.Send(new Game.MsgServer.MsgMessage("Freezewar - Scores", "ALLUSERS", "SYSTEM", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.FirstRightCorner).GetArray(stream));
                    C.Send(new Game.MsgServer.MsgMessage("My Points : " + C.FreezewarPoints, "ALLUSERS", "SYSTEM", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.ContinueRightCorner).GetArray(stream));
                    foreach (string t in text)
                        C.Send(new Game.MsgServer.MsgMessage(t, "ALLUSERS", "SYSTEM", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.ContinueRightCorner).GetArray(stream));
                }
            }
        }
    }
}
