using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COServer.Game.Models
{
    public class MarketItem
    {
        public string PlayerName { get; set; }
        public string ItemName { get; set; }
        public uint Price { get; set; }
        public DateTime Timestamp { get; set; }  // Timestamp como DateTime
    }
}
