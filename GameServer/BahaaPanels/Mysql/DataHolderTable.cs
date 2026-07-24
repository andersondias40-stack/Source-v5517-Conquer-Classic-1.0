using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using MYSQLCONNECTION = MySql.Data.MySqlClient.MySqlConnection;

namespace COServer.Database
{
    public class BackupService
    {

    }
    public unsafe static class DataHolder
    {
        public static string ConnectionString;
        private static string MySqlUsername, MySqlPassword, MySqlDatabase, MySqlHost;
        public static void CreateConnection()
        {
            MySqlUsername = "root";
            MySqlHost = "localhost";
            MySqlPassword = "anderson";
            MySqlDatabase = "zq";
            ConnectionString = "Server=" + MySqlHost + ";Database='" + MySqlDatabase + "';Username='" + MySqlUsername + "';Password='" + MySqlPassword + "';Pooling=true; Max Pool Size = 300; Min Pool Size = 5";
        }

        public static MYSQLCONNECTION MySqlConnection
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