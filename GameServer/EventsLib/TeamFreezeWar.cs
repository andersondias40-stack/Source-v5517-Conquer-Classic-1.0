using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using COServer.Game;
namespace COServer.EventsLib
{
    public enum FreezeWarTeams
    {
        Red = 0,
        Blue = 1
    }
    public class TeamFreezeWar : BaseEvent
    {
        public bool isChosen = false;
        public TeamFreezeWar()
            : base(8736, "Team Freeze War", 100, Game.MsgServer.MsgStaticMessage.Messages.TeamFreezewar) //prize in silvers 1M, hour, minutes
        {
        }
        DateTime lastSent = DateTime.Now;
        List<string> score = new List<string>();

        public override void worker()
        {
            base.worker();
            score.Clear();

            bool full_red = true, full_blue = true;
            if (Database.Server.GamePoll.Values.Where(e => e.Player.Map == map).Count() == 0)
                return;
            full_red = Database.Server.GamePoll.Values.Where(e => e.Player.Map == map && e.TeamFreeze == FreezeWarTeams.Red && e.Player.TransformationID == 0).Count() == 0;
            if (full_red)
            {
                EndEvent(FreezeWarTeams.Blue);
                return;
            }
            full_blue = Database.Server.GamePoll.Values.Where(e => e.Player.Map == map && e.TeamFreeze == FreezeWarTeams.Blue && e.Player.TransformationID == 0).Count() == 0;
            if (full_blue)
            {
                EndEvent(FreezeWarTeams.Red);
                return;
            }
        }

        public byte NextTeam()
        {
            int nextid = ++TeamNow;
            if (nextid % 2 == 0)
                return 0;
            else return 1;
        }
        int TeamNow = 0;
        private void EndEvent(FreezeWarTeams winnerTeam)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                foreach (var player in Database.Server.GamePoll.Values.Where(e => e.Player.Map == map))
                {
                    if (player.TeamFreeze == winnerTeam)
                    {
                        player.Inventory.Add(stream, 723713, 1);  //Class1MoneyBag  300k silvers
                    }
                    player.Teleport(1006, 036, 32);
                    player.Player.RemoveFlag( Game.MsgServer.MsgUpdate.Flags.Freeze);
                }
                Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(winnerTeam + " team has won the event.", "ALLUSERS", "Team Freeze War", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.TopLeftSystem).GetArray(stream));
            }
        }
    }
}
