using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using COServer.Database;
using COServer.Game.MsgTournaments;

namespace COServer.Database
{
    internal class ServerStats
    {
        public static string LastChar = "None.";
        internal static void Update()
        {
            try
            {
                using (var conn = new MySql.Data.MySqlClient.MySqlConnection(PayPalHandler.ConnectionString))
                using (var cmd = new MySql.Data.MySqlClient.MySqlCommand("Update configuration SET Online=@o, LastChar=@last, GWWinner=@gw", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@o", Database.Server.GamePoll.Count);
                    cmd.Parameters.AddWithValue("@last", LastChar);
                    if (MsgSchedules.GuildWar.Winner != null && MsgSchedules.GuildWar.Proces == ProcesType.Dead)
                        cmd.Parameters.AddWithValue("@gw", MsgSchedules.GuildWar.Winner.Name);
                    else
                        cmd.Parameters.AddWithValue("@gw", "None");
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
