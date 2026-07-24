using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COServer.EventsLib
{
    public class GuildsDeathMatch : BaseEvent
    {
        public static Dictionary<string, int> PrizesClaimed
             = new Dictionary<string, int>();
        public GuildsDeathMatch()
            : base(8509, "Guilds Death Match", 100, Game.MsgServer.MsgStaticMessage.Messages.GuildsDeathMatch) //prize doesn't count here
        {
        }
        DateTime lastSent = DateTime.Now;
        public Dictionary<string, int> GuildScores
             = new Dictionary<string, int>();
        List<string> score = new List<string>();

        /*
         * Logica do evento:
         * Pegar todos os players de cada guild e mostrar quantos membros cada guild tem e que estao vivos
         * Ex: GuildA: 2, GuildB: 3
         * 
         * Quando a contagem de guildas presentes no mapa estiver 1, determinar que a guild eh a vencedora
        */
        public override void worker()
        {
            base.worker();
            score.Clear();
            GuildScores.Clear();

            // Enumerar todos os players que estao no mapa do evento e estao vivos.
            foreach (var c in Database.Server.GamePoll.Values.Where(e => e.Player.Map == map && e.Player.Alive))
            {
                // Se o player nao estiver numa guilda, teleportar para fora do evento.
                if (c.Player.MyGuild == null)
                {
                    c.Teleport(430, 329,1002);
                    continue;
                }

                // Se a guilda do jogador nao estiver na lista de guildas do evento, salvar ela.
                if (!GuildScores.ContainsKey(c.Player.MyGuild.GuildName))
                    GuildScores.Add(c.Player.MyGuild.GuildName, 0);

                // Incrementar o numero de jogadores vivos da guilda no mapa do evento.
                GuildScores[c.Player.MyGuild.GuildName]++;
            }

            //Console.WriteLine(GuildScores.Count);

            // Construir o scoreboard do evento. Pegue as 10 primeiras guildas.
            foreach (var p in GuildScores.OrderByDescending(e => e.Value).Take(10))
                score.Add(p.Key + " : " + p.Value + ".");

            // Ha apenas uma guild restante.
            if (GuildScores.Count <= 1)
            {   
                // Console.WriteLine("Teleportando todos os players");

                // Pegar o nome da guild vencedora (se for valida).
                string winnerG = GuildScores.Take(1).SingleOrDefault().Key;

                // Se a guilda for valida, presentear todos os vencedores.
                if (winnerG != null)
                {
                    // Enviar mensagem para todos do server.
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        Program.SendGlobalPackets.Enqueue(
                            new Game.MsgServer.MsgMessage(winnerG + " has won the Guilds Death Match, and all players won PowerExpBall!", "ALLUSERS", "[EVENT]", Game.MsgServer.MsgMessage.MsgColor.white, (Game.MsgServer.MsgMessage.ChatMode)2000).GetArray(stream));
                        Program.DiscordAPIwinners.Enqueue("``[" + winnerG + "] has won the Guilds Death Match, and all players won PowerExpBall!!``");
                    }

                    // Presentear todos os jogadores que estão no mapa do evento e são da guilda vencedora
                    foreach (var c in Database.Server.GamePoll.Values.Where(e => e.Player.Map == map && e.Player.MyGuild != null && e.Player.MyGuild.GuildName == winnerG))
                    {
                        // Adiciona o item ao inventário do jogador
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            c.Inventory.Add(stream, 722057, 1); // Adiciona 1 item powerexpball! ao inventário
                        }

                        // Envia mensagem ao jogador informando que recebeu o item
                        c.SendSysMesage("You received a powerexpball!", (Game.MsgServer.MsgMessage.ChatMode)2005);
                    }
                }


                // Teleportar todo mundo pro mapa principal.
                foreach (var c in Database.Server.GamePoll.Values.Where(e => e.Player.Map == map && e.Player.Alive))
                    c.Teleport(428, 378, 1002);
            }

            // Atualizar scoreboard.
            if (DateTime.Now > lastSent.AddSeconds(5))
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
                    C.Send(new Game.MsgServer.MsgMessage("Guilds - Information", "ALLUSERS", "SYSTEM", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.FirstRightCorner).GetArray(stream));
                    C.Send(new Game.MsgServer.MsgMessage("Total players of guilds in map : " + GuildScores.Count, "ALLUSERS", "SYSTEM", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.ContinueRightCorner).GetArray(stream));
                    foreach (string t in text)
                        C.Send(new Game.MsgServer.MsgMessage(t, "ALLUSERS", "SYSTEM", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.ContinueRightCorner).GetArray(stream));
                }
            }
        }
    }
}
