using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace AccServer.Database
{
    using MYSQLCOMMAND = MySql.Data.MySqlClient.MySqlCommand;
    using MYSQLREADER = MySql.Data.MySqlClient.MySqlDataReader;
    using MYSQLCONNECTION = MySql.Data.MySqlClient.MySqlConnection;

    public unsafe static class DataHolder
    {
        private static string ConnectionString;
        public static void CreateConnection()
        {
            var list = System.Configuration.ConfigurationManager.ConnectionStrings;
            ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings[list.Count - 1].ConnectionString;
        }
        public static MYSQLCONNECTION   MySqlConnection
        {
            get
            {
                MYSQLCONNECTION conn = new MYSQLCONNECTION();
                conn.ConnectionString = ConnectionString;
                return conn;
            }
        }
    }
}