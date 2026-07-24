using System;

namespace COServer.Database
{
    public class SystemBanned
    {
        public static uint LastToken = 10000;
        public static SafeDictionary<string, Client> BannedPoll = new SafeDictionary<string, Client>();
        public class Client
        {
            public uint Token = 0;
            public string IP = "";
            public uint Hours;
            public long StartBan;
            public string Name = "";

            public override string ToString()
            {
                return $"{Token}-{IP}";
            }
            public string ToString2()
            {
                Database.DBActions.WriteLine writer = new DBActions.WriteLine('/');
                writer.Add(IP).Add(Hours).Add(StartBan).Add(Name);
                return writer.Close();
            }
        }

        public static void AddBan(string IP, uint Hours, string name)
        {
            Client msg = new Client();
            msg.IP = IP;
            msg.Hours = Hours;
            msg.StartBan = DateTime.Now.Ticks;
            msg.Name = name;
            BannedPoll.Add(msg.IP, msg);
        }

        public static void RemoveBan(string IP)
        {
            Client msg = new Client();
            msg.IP = IP;

            BannedPoll.Remove(msg.IP);
        }
        public static bool IsBanned(string Ip, out string Messaj)
        {
            if (BannedPoll.ContainsKey(Ip))
            {
                var msg = BannedPoll[Ip];
                if (DateTime.FromBinary(msg.StartBan).AddHours(msg.Hours) < DateTime.Now)
                {
                    BannedPoll.Remove(msg.IP);
                }
                else
                {
                    if (msg.Token == 0)
                    {
                        msg.Token = LastToken;
                        LastToken++;
                    }
                    DateTime receiveban = DateTime.FromBinary(msg.StartBan);
                    DateTime TimerBan = receiveban.AddHours(msg.Hours);
                    TimeSpan time = TimeSpan.FromTicks(TimerBan.Ticks) - TimeSpan.FromTicks(DateTime.Now.Ticks);
                    Messaj = " " + time.Days + " Days " + time.Hours + " Hours " + time.Minutes + $" Minutes.\nS/N:{msg.Token}";
                    return true;
                }
            }
            Messaj = "";
            return false;
        }

        public static void Save()
        {
            using (Database.DBActions.Write writer = new DBActions.Write("BanHWID.txt"))
            {
                foreach (var ban in BannedPoll.Values)
                {
                    writer.Add(ban.ToString2());
                }
                writer.Execute(DBActions.Mode.Open);
            }
        }

        public static void Load()
        {
            using (Database.DBActions.Read Reader = new DBActions.Read("BanHWID.txt"))
            {

                if (Reader.Reader())
                {
                    uint count = (uint)Reader.Count;
                    for (uint x = 0; x < count; x++)
                    {
                        DBActions.ReadLine readline = new DBActions.ReadLine(Reader.ReadString(""), '/');
                        Client msg = new Client();
                        msg.IP = readline.Read((string)"");
                        msg.Hours = readline.Read((uint)0);
                        msg.StartBan = readline.Read((long)0);
                        msg.Name = readline.Read((string)"");
                        BannedPoll.Add(msg.IP, msg);
                    }
                }
            }
        }
    }
}
