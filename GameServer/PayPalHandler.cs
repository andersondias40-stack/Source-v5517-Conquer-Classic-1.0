using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COServer
{
    public class PayPalHandler
    {
        public const string ConnectionString = "Server=localhost;username=root;password=anderson;database=zq;";
        public static int getFounds(string username)
        {
            int founds = 0;

            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    Console.WriteLine($" Username :{username}");
                    using (var cmd = new MySqlCommand("select founds from payments where username=@u"
                        , conn))
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@u", username);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                founds = int.Parse(reader.GetString("founds"));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return founds;
        }

        public static void logDonation(string user, string name, string log)
        {
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    using (var cmd = new MySqlCommand("insert into log_payments (username,name,log) values (@user,@name,@log)"
                        , conn))
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@user", user);
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@log", log);
                        cmd.ExecuteNonQuery();

                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
