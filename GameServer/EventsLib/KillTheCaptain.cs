using COServer.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COServer.EventsLib
{
    public enum KillTheCaptainTeams
    {
        Red = 0,
        Blue = 1
    }
    public class KillTheCaptain : BaseEvent
    {
        public bool isChosen = false;
        public KillTheCaptain()
            : base(8581, "Kill The Captain", 100, Game.MsgServer.MsgStaticMessage.Messages.KillTheCaptain) //prize in silvers , hour, minutes
        {
        }
        DateTime lastSent = DateTime.Now;
        List<string> score = new List<string>();
        public int RedScore = 0, BlueScore = 0;
        public override void worker()
        {
            base.worker();
            score.Clear();

            RedScore = BlueScore = 0;

            bool full_red = true, full_blue = true;
            if (Database.Server.GamePoll.Values.Where(e => e.Player.Map == map).Count() == 0)
                return;
            full_red = Database.Server.GamePoll.Values.Where(e => e.Player.Map == map && e.TeamKillTheCaptain == KillTheCaptainTeams.Red && e.Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Flashy)).Count() == 0;
            if (full_red)
                ChooseRandomLeader(KillTheCaptainTeams.Red);

            full_blue = Database.Server.GamePoll.Values.Where(e => e.Player.Map == map && e.TeamKillTheCaptain == KillTheCaptainTeams.Blue && e.Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Flashy)).Count() == 0;

            if (full_blue)
                ChooseRandomLeader(KillTheCaptainTeams.Blue);
            foreach (var player in Database.Server.GamePoll.Values)
                if (player.TeamKillTheCaptain == KillTheCaptainTeams.Red)
                    RedScore += player.KillTheCaptainPoints;
                else BlueScore += player.KillTheCaptainPoints;

            if (RedScore >= 200)
            {
                EndEvent(KillTheCaptainTeams.Red);
                return;
            }
            if (BlueScore >= 200)
            {
                EndEvent(KillTheCaptainTeams.Blue);
                return;
            }

            SendScore();
        }
        public void SendScore()
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                foreach (var C in Database.Server.GamePoll.Values.Where(e => e.Player.Map == map))
                {
                    C.Send(new Game.MsgServer.MsgMessage($"Your Team  ==> {C.TeamKillTheCaptain.ToString()}", "ALLUSERS", "SYSTEM", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.FirstRightCorner).GetArray(stream));
                    C.Send(new Game.MsgServer.MsgMessage("Red Score: " + RedScore, "ALLUSERS", "SYSTEM", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.ContinueRightCorner).GetArray(stream));
                    C.Send(new Game.MsgServer.MsgMessage("Blue Score: " + BlueScore, "ALLUSERS", "SYSTEM", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.ContinueRightCorner).GetArray(stream));
                }
            }
        }
        public void ChooseRandomLeader(KillTheCaptainTeams killTheCaptainTeams)
        {
            var players = Database.Server.GamePoll.Values.Where(e => e.Player.Map == map && e.Player.Alive && e.TeamKillTheCaptain == killTheCaptainTeams).ToArray();
            uint count = 0;
            uint rndplayer = (uint)Role.Core.Random.Next(0, players.Length);
            foreach (var player in players)
            {
                if (player.Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Flashy))
                    player.Player.RemoveFlag(Game.MsgServer.MsgUpdate.Flags.Flashy);
            }
            Client.GameClient gameClient = null;
            foreach (var player in players)
            {
                count++;
                if(count >= rndplayer)
                {
                    gameClient = player;
                    break;
                }
            }
            if(gameClient != null)
            {
                gameClient.Player.AddFlag(Game.MsgServer.MsgUpdate.Flags.Flashy, 60 * 60 * 24 * 30, false);
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("Team " + killTheCaptainTeams.ToString() + "'s new leader is " + gameClient.Player.Name, "ALLUSERS", "Vendetta", Game.MsgServer.MsgMessage.MsgColor.white, (Game.MsgServer.MsgMessage.ChatMode)2500).GetArray(stream));
                }
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
        private void EndEvent(KillTheCaptainTeams winnerTeam)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                foreach (var player in Database.Server.GamePoll.Values.Where(e => e.Player.Map == map))
                {
                    if (player.TeamKillTheCaptain == winnerTeam)
                    {
                        player.Inventory.Add(stream, 700071, 1); /// totoise
                        Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(winnerTeam + " team has won Vendetta. Very nice!", "ALLUSERS", "Vendetta", Game.MsgServer.MsgMessage.MsgColor.white, (Game.MsgServer.MsgMessage.ChatMode)2500).GetArray(stream));
                        Program.DiscordAPIwinners.Enqueue("``[" + winnerTeam + "] team has won Vendetta. Very nice, and [Totoise Normal]!``");
                    }
                    player.Teleport(439, 387, 1002);
                    player.Player.RemoveFlag(Game.MsgServer.MsgUpdate.Flags.Flashy);

                }


            }
        }
    }
}
