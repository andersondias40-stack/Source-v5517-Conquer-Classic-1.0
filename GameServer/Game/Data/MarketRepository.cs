using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using COServer.Game.Models;  // Adiciona o namespace dos modelos
using MySql.Data.MySqlClient;  // Biblioteca MySQL

namespace COServer.Game.Data
{
    public static class MarketRepository
    {
        // Sua string de conexão com o banco de dados
        public const string ConnectionString = "Server=localhost;Uid=root;Password=anderson;Database=zq;";

        // Método para inserir um item no banco de dados
        public static void InsertMarketItem(MarketItem item)
        {
            string query = @"INSERT INTO MarketItems (PlayerName, ItemName, Price, Timestamp)
                             VALUES (@PlayerName, @ItemName, @Price, @Timestamp)";

            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    // Adiciona os parâmetros para evitar SQL Injection
                    cmd.Parameters.AddWithValue("@PlayerName", item.PlayerName);
                    cmd.Parameters.AddWithValue("@ItemName", item.ItemName);
                    cmd.Parameters.AddWithValue("@Price", item.Price);
                    cmd.Parameters.AddWithValue("@Timestamp", item.Timestamp);

                    // Executa o comando no banco de dados
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
