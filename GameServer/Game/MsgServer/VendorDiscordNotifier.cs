using System;
using System.Collections.Generic;
using System.Timers;
using COServer.Game.Data;  
using COServer.Game.Models; 

namespace COServer.Game.MsgServer
{
    public static class VendorDiscordNotifier
    {
        private static Dictionary<string, List<string>> PlayerItems = new Dictionary<string, List<string>>();
        private static Dictionary<string, Timer> PlayerTimers = new Dictionary<string, Timer>();

        public static void AddItem(string playerName, string itemName, uint amount, byte plus)
        {
            lock (PlayerItems)
            {
                if (!PlayerItems.ContainsKey(playerName))
                {
                    PlayerItems[playerName] = new List<string>();
                }
                string plusText = plus > 0 ? $" +{plus}" : "";
                PlayerItems[playerName].Add($"🛒 {playerName} listed for sale: {itemName}{plusText} for CPS💎{amount.ToString("#,0").Replace(",", "k")}");


                MarketRepository.InsertMarketItem(new MarketItem
                {
                    PlayerName = playerName,
                    ItemName = itemName,
                    Price = amount,
                    Timestamp = DateTime.Now  
                });


                if (!PlayerTimers.ContainsKey(playerName))
                {
                    Timer timer = new Timer(60000); 
                    timer.Elapsed += (sender, e) => SendToDiscord(playerName);
                    timer.AutoReset = false;
                    timer.Start();
                    PlayerTimers[playerName] = timer;
                }
            }
        }

     
        private static void SendToDiscord(string playerName)
        {
            lock (PlayerItems)
            {
                if (PlayerItems.ContainsKey(playerName) && PlayerItems[playerName].Count > 0)
                {
           
                    string message = string.Join("\n", PlayerItems[playerName]);
                    Program.DiscordAPITapete.Enqueue($"```{message}```");

                    // Limpa a lista de itens e remove o timer para esse jogador
                    PlayerItems.Remove(playerName);
                    PlayerTimers.Remove(playerName);
                }
            }
        }
    }
}
