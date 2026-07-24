using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using COServer.Game.Data;
using COServer.Game.Models;
using System.Timers;

namespace COServer.Game.MsgServer
{
    public static class VoteSystem
    {
        private static Dictionary<string, List<string>> PlayerVotes = new Dictionary<string, List<string>>();
        private static Dictionary<string, Timer> PlayerTimers = new Dictionary<string, Timer>();
        public static void AddVote(string playerName, string ip)
        {

            lock (PlayerVotes)
            {
                var lastVote = VoteRepository.GetLastVote(playerName);

                if (lastVote != null && lastVote.Timestamp.AddHours(12) > DateTime.Now)
                {

                    return;
                }

                if (!PlayerVotes.ContainsKey(playerName))
                {
                    PlayerVotes[playerName] = new List<string>();
                }
                PlayerVotes[playerName].Add($"🗳️ {playerName} voted to support the server! Thank you for helping us grow! 🙏");

                if (lastVote != null)
                {
                    VoteRepository.UpdateVote(playerName);
                }
                else
                {

                    VoteRepository.InsertVoteDb(new VotesModels
                    {
                        Id = playerName.GetHashCode(),
                        Ip = ip,
                        Timestamp = DateTime.Now,
                        VotePoints = 1
                    });
                }

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
            lock (PlayerVotes)
            {
                if (PlayerVotes.ContainsKey(playerName) && PlayerVotes[playerName].Count > 0)
                {
                    string message = string.Join("\n", PlayerVotes[playerName]);
                    Program.DiscordAPIVote.Enqueue($"```{message}```");
                    PlayerVotes.Remove(playerName);
                    PlayerTimers.Remove(playerName);
                }
            }
        }
    }
}
    


