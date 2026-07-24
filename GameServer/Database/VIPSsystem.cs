using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient; // Diretiva correta

namespace COServer.Database
{
    public class VIPSystem
    {
        private static readonly string ConnectionString = "Server=localhost;Uid=root;Password=anderson;Database=zq;";

        public class User
        {
            public uint UID;
            public string IP;
            public uint CanClaimFreeVip;
            public override string ToString()
            {
                var writer = new DBActions.WriteLine('/');
                writer.Add(UID).Add(IP).Add(CanClaimFreeVip);
                return writer.Close();
            }
        }

        private static List<User> UsersPoll = new List<User>();

        // Versão atualizada para verificar IP e UID
        public static bool HasClaimedFreeVip(string ip)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string query = "SELECT 1 FROM vip_claims WHERE ip = @ip LIMIT 1";

                using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ip", ip);

                    using (var reader = cmd.ExecuteReader())
                    {
                        return reader.HasRows; // Retorna true se o IP já foi usado
                    }
                }
            }
        }

        // Método de salvamento atualizado
        public static void SaveVipClaim(uint playerId, string ip)
        {
            using (var conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                string query = "INSERT INTO vip_claims (player_id, ip) VALUES (@playerId, @ip)";

                using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@playerId", playerId);
                    cmd.Parameters.AddWithValue("@ip", ip);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Mantido o restante do código original
        public static bool TryGetObject(uint UID, string IP, out User obj)
        {
            foreach (var _obj in UsersPoll)
            {
                if (_obj.UID == UID || _obj.IP == IP)
                {
                    obj = _obj;
                    return true;
                }
            }
            obj = null;
            return false;
        }

        public static bool CanClaimVIP(Client.GameClient client)
        {
            User _user;
            if (TryGetObject(client.Player.UID, client.Socket.RemoteIp, out _user))
            {
                if (_user.CanClaimFreeVip == 1)
                    return true;
            }
            return false;
        }

        public static void CheckUp(Client.GameClient client)
        {
            string clientIP = client.Socket.RemoteIp;

            // Verifica apenas por IP
            if (!HasClaimedFreeVip(clientIP))
            {
                client.Player.CanClaimFreeVip = true; // Apenas marca que o jogador pode resgatar no NPC
            }
        }

        public static void Save()
        {
            using (Database.DBActions.Write _wr = new Database.DBActions.Write("VIP.txt"))
            {
                foreach (var _obj in UsersPoll)
                    _wr.Add(_obj.ToString());
                _wr.Execute(DBActions.Mode.Open);
            }
        }   

        public static void Load()
        {
            using (Database.DBActions.Read r = new Database.DBActions.Read("VIP.txt"))
            {
                if (r.Reader())
                {
                    int count = r.Count;
                    for (uint x = 0; x < count; x++)
                    {
                        Database.DBActions.ReadLine reader = new DBActions.ReadLine(r.ReadString(""), '/');
                        User user = new User();
                        user.UID = reader.Read((uint)0);
                        user.IP = reader.Read("");
                        user.CanClaimFreeVip = reader.Read((uint)0);
                        UsersPoll.Add(user);
                    }
                }
            }
        }
    }
}