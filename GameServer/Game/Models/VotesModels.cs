using System;

namespace COServer.Game.Models
{
    internal class VotesModels
    {
        public int Id { get; set; }
        public string Ip { get; set; }
        public DateTime Timestamp { get; set; }  // Timestamp do último voto
        public int VotePoints { get; set; }  // Contador de votos
    }
}
