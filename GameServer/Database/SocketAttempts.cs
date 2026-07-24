using System;
using MySql.Data.MySqlClient; // Certifique-se de que isso está incluído

namespace COServer.Database
{
    public class SocketAttempts
    {
        private static readonly string ConnectionString = "Server=localhost;Uid=root;Password=anderson;Database=zq;";

        public class Attempt
        {
            public uint PlayerID;
            public uint ItemUID;
            public int MeteorAttempts;
        }

        // Carrega as tentativas para um item específico
        public static int LoadMeteorAttempts(uint playerId, uint itemUid)
        {
            using (var conn = new MySql.Data.MySqlClient.MySqlConnection(ConnectionString))
            {
                conn.Open();
                string query = "SELECT meteor_attempts FROM socket_attempts WHERE player_id = @playerId AND item_uid = @itemUid";
                using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@playerId", playerId);
                    cmd.Parameters.AddWithValue("@itemUid", itemUid);
                    var result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }

        // Salva ou atualiza as tentativas para um item
        public static void SaveMeteorAttempts(uint playerId, uint itemUid, int attempts)
        {
            using (var conn = new MySql.Data.MySqlClient.MySqlConnection(ConnectionString))
            {
                conn.Open();
                string query = "INSERT INTO socket_attempts (player_id, item_uid, meteor_attempts) " +
                               "VALUES (@playerId, @itemUid, @attempts) " +
                               "ON DUPLICATE KEY UPDATE meteor_attempts = @attempts";
                using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@playerId", playerId);
                    cmd.Parameters.AddWithValue("@itemUid", itemUid);
                    cmd.Parameters.AddWithValue("@attempts", attempts);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}