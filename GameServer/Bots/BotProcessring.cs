using System;
using System.Collections.Concurrent;
using System.Timers;

namespace COServer.Bots
{
    public class BotProcessring
    {
        public static ConcurrentDictionary<uint, AI> Bots = new ConcurrentDictionary<uint, AI>();

        private readonly Timer[] Timer = new Timer[2];
        public BotProcessring()
        {
            Timer[0] = new Timer
            {
                Interval = 100,
                AutoReset = false
            };
            Timer[0].Elapsed += OnTimer1;
            Timer[0].Start();
            Timer[1] = new Timer
            {
                Interval = 100,
                AutoReset = false
            };
            Timer[1].Elapsed += OnTimer2;
            Timer[1].Start();
        }

        public void OnTimer1(object sender, ElapsedEventArgs e)
        {
            foreach (var bot in Bots.Values)
            {
                if (!bot.Bot.Player.Alive) continue;
                if (bot.ToStart > System.DateTime.Now) continue;
                bot.HandleJump();
            }
            Timer[0].Start();
        }

        public void OnTimer2(object sender, ElapsedEventArgs e)
        {
            foreach (var bot in Bots.Values)
            {
                if (!bot.Bot.Player.Alive) continue;
                if (bot.ToStart > System.DateTime.Now) continue;
                bot.Attack();
            }
            Timer[1].Start();
        }
    }
}
