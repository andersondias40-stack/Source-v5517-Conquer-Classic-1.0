using System;
using System.Security.Cryptography;
using MySql.Data.MySqlClient;

namespace COServer.Database
{
    public class MiningRepository
    {
        private static readonly string ConnectionString = "Server=localhost;Uid=root;Password=anderson;Database=zq;";

        public static void InsertMinedItem(string playerName, string itemName, DateTime minedAt)
        {
            string query = "INSERT INTO mined_items (player_name, item_name, mined_at) VALUES (@playerName, @itemName, @minedAt)";

            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@playerName", playerName);
                    cmd.Parameters.AddWithValue("@itemName", itemName);
                    cmd.Parameters.AddWithValue("@minedAt", minedAt);

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
