using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COServer.EventsLib
{
    public class DeathMatch : BaseEvent
    {
        public bool isChosen = false;

        // Variável para verificar se o evento já foi finalizado
        private bool isEventFinished = false;

        public DeathMatch()
            : base(8505, "DeathMatch", 100, Game.MsgServer.MsgStaticMessage.Messages.DeathMatch)
        {
        }

        DateTime lastSent = DateTime.Now;
        List<string> score = new List<string>();
        public int WhiteTeam = 0, RedTeam = 0, BlueTeam = 0, BlackTeam = 0;

        // Método worker executado repetidamente para controlar o evento
        public override void worker()
        {
            // Chama o worker da classe base
            base.worker();

            // Se o evento já foi finalizado, não continua executando
            if (isEventFinished)
                return;

            // Limpa o placar anterior
            score.Clear();
            score.Add("BlackTeam : " + BlackTeam);
            score.Add("BlueTeam : " + BlueTeam);
            score.Add("RedTeam : " + RedTeam);
            score.Add("WhiteTeam : " + WhiteTeam);

            // Verifica se algum time chegou a 100 pontos
            if (BlackTeam >= 30 || BlueTeam >= 30 || RedTeam >= 30 || WhiteTeam >= 30)
            {
                EndEvent();
                return;
            }

            // Atualiza e envia o placar a cada 5 segundos
            if (DateTime.Now > lastSent.AddSeconds(5))
            {
                SendScore(score);
                lastSent = DateTime.Now;
            }
        }

        // Método para enviar o placar dos times aos jogadores
        public void SendScore(List<string> text)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                foreach (var C in Database.Server.GamePoll.Values.Where(e => e.Player.Map == map))
                {
                    C.Send(new Game.MsgServer.MsgMessage("DeathMatch - Scores", "ALLUSERS", "SYSTEM", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.FirstRightCorner).GetArray(stream));
                    C.Send(new Game.MsgServer.MsgMessage("TEAM : " + C.DMTeamString(), "ALLUSERS", "SYSTEM", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.ContinueRightCorner).GetArray(stream));
                    foreach (string t in text)
                        C.Send(new Game.MsgServer.MsgMessage(t, "ALLUSERS", "SYSTEM", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.ContinueRightCorner).GetArray(stream));
                }
            }
        }

        // Método que define o próximo time de forma cíclica
        public byte NextTeam()
        {
            int nextid = ++TeamNow;
            if (nextid % 4 == 0)
                return 4;
            else if (nextid % 3 == 0)
                return 3;
            else if (nextid % 2 == 0)
                return 2;
            else return 1;
        }

        // IDs de vestimentas para os times
        public uint[] garments = { 181325, 181625, 181825, 181525 };

        int TeamNow = 0;

        // Método para finalizar o evento
        public void EndEvent()
        {
            // Define o time vencedor com base nos pontos
            string winningTeam = "";
            if (BlackTeam >= 30)
                winningTeam = "Black Team";
            else if (BlueTeam >= 30)
                winningTeam = "Blue Team";
            else if (RedTeam >= 30)
                winningTeam = "Red Team";
            else if (WhiteTeam >= 30)
                winningTeam = "White Team";

            // Envia a mensagem de vitória e dá o prêmio apenas para o time vencedor
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                // Mensagem de vitória enviada apenas uma vez
                foreach (var C in Database.Server.GamePoll.Values.Where(e => e.Player.Map == map))
                {
                    // Teleportar todos os jogadores para um local específico
                    C.Teleport(439, 388, 1002); // Teleportação comum para todos os jogadores

                    // Verifica se o jogador faz parte do time vencedor
                    if (C.DMTeamString() == winningTeam)
                    {
                        C.Inventory.Add(stream, 700071, 1); // Tortoise para vencedores
                    }
                }

                // Envia a mensagem de vitória para todos os jogadores no mapa uma vez
                string victoryMessage = $"{winningTeam} wins the DeathMatch!";
                foreach (var C in Database.Server.GamePoll.Values.Where(e => e.Player.Map == map))
                {
                    C.Send(new Game.MsgServer.MsgMessage(victoryMessage, "ALLUSERS", "SYSTEM", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.TopLeft).GetArray(stream));
                }

                // Envio para o Discord
                Program.DiscordAPIwinners.Enqueue($"``[{winningTeam}] has won the DeathMatch and won item [Tortoise] Normal!``");
            }

            // Finaliza o evento, evitando que continue executando
            isEventFinished = true;
        }
    }
}
