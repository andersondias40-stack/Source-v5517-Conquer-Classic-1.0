using COServer;
using System;
using System.IO;
using System.Text;

namespace AccServer.Database
{
    public unsafe class AccountTable
    {
        public enum AccountState : byte
        {
            NotActivated = 100,
            ProjectManager = 255,
            GameHelper = 5,
            GameMaster = 3,
            Player = 2,
            Banned = 1,
            Cheat = 80,
            DoesntExist = 0
        }
        public string Username;
        public string Password;
        public string IP;
        public int RandomKey;

        public string Hwid;

        public AccountState State;
        public uint EntityID;
        public bool exists = false;
        public bool Banned;
        public static SafeRandom Random = new SafeRandom();

        public AccountTable(string username)
        {
            if (username == null) return;
            Username = username;
            Password = "";
            IP = "";
            State = AccountState.DoesntExist;
            EntityID = 0;
            using (var cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("accounts").Where("Username", username))
            using (var reader = new MySqlReader(cmd))
            {
                if (reader.Read())
                {
                    exists = true;
                    Password = reader.ReadString("Password");
                    IP = reader.ReadString("Ip");
                    EntityID = reader.ReadUInt32("EntityID");
                    State = (AccountState)reader.ReadInt32("State");
                    if (State == (AccountState)1)
                    {
                        Banned = true;
                    }
                }
            }
        }
        public uint GenerateKey(int randomKey = 0)
        {
            if (randomKey == 0)
                RandomKey = Random.Next(11, 253) % 100 + 1;
            return (uint)
                        (Username.GetHashCode() *
                        Password.GetHashCode() *
                        RandomKey);
        }
        public void SaveIP()
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE))
                cmd.Update("accounts").Set("Ip", IP)//.Set("HWID", Hwid)
                    .Where("Username", Username).Execute();
        }
        public void Save()
        {
            using (var cmd = new MySqlCommand(MySqlCommandType.UPDATE))
                cmd.Update("accounts").Set("EntityID", EntityID).Set("Ip", IP)//.Set("HWID", Hwid)
                    .Where("Username", Username).Execute();
        }
    }
}