using System.Linq;

namespace COServer.EventsLib
{
    public class KillTheHunted : BaseEvent
    {
        public bool isChosen = false;
        public KillTheHunted()
            : base(8522, "Kill The Fugitive", 100, Game.MsgServer.MsgStaticMessage.Messages.KillTheFugitive)
        {
            Program.FreePkMap.Add((uint)map);
        }
        public override void worker()
        {
            base.worker();
            var list = Database.Server.GamePoll.Values.Where(e => e.Player.Map == map && e.Player.Alive);
            if (list.Count() > 1)
            {
                if (Database.Server.GamePoll.Values.Where(e => e.Hunted == true && e.Player.Alive && e.Player.Map == map).Count() == 0)
                    Pass();
            }
            else if (list.Count() == 1)
            {
                var winner = list.SingleOrDefault();
                winner.Teleport(430, 378, 1002);
                winner.Hunted = false;
                winner.Player.Money += 100000; // 100k

                // Adiciona o item Tortoise ao vencedor
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    winner.Inventory.Add(stream, 700071, 1); // Adiciona 1 item Tortoise

                    // Envia a mensagem de vitória
                    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage($"{winner.Player.Name} has won the Kill The Fugitive.", "ALLUSERS", "PrisonOfficer", Game.MsgServer.MsgMessage.MsgColor.white, (Game.MsgServer.MsgMessage.ChatMode)2005).GetArray(stream));
                    Program.DiscordAPIwinners.Enqueue("``[" + winner.Player.Name + "] has won Vendetta. Very nice, and won [Tortoise Normal]!``");
                }
            }
        }
        public void Pass()
        {
            var list = Database.Server.GamePoll.Values.Where(e => e.Player.Map == map && e.Player.Alive).ToList();
            if (list.Count() > 1)
            {
                foreach (var p in list)
                    if (p.Hunted)
                        p.Hunted = false;
                var rndPlayer = list[Role.Core.Random.Next(0, list.Count())];
                rndPlayer.Hunted = true;
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage("The new target is " + rndPlayer.Player.Name, "ALLUSERS", "PrisonOfficer", Game.MsgServer.MsgMessage.MsgColor.white, (Game.MsgServer.MsgMessage.ChatMode)2005).GetArray(stream));
                }
            }
        }
    }

}