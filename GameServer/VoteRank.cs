using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COServer
{
    public class VoteRank
    {
        public class Users
        {
            public string Name;
            public uint UID;
            public uint VoteCount;
        }
        public static Dictionary<uint, Users> VoteRanksPoll = new Dictionary<uint, Users>();
        private ThreadItem _thread;
        public VoteRank()
        {
            Load();
            _thread = new ThreadItem(1000, WorkThread);
            _thread.Open();
        }
        public void Load()
        {
            WindowsAPI.IniFile ini = new WindowsAPI.IniFile("");
            foreach (string fname in System.IO.Directory.GetFiles(Program.ServerConfig.DbLocation + "\\Users\\"))
            {
                ini.FileName = fname;
                string name = ini.ReadString("Character", "Name", "");
                uint uid = ini.ReadUInt32("Character", "UID", 0);
                uint votepoint = ini.ReadUInt32("Character", "CountVote", 0);
                Users users = new Users() { Name = name, UID = uid, VoteCount = votepoint };
                if (users.VoteCount > 0)
                    VoteRanksPoll.Add(users.UID, users);
            }
        }
        public Users[] GetRanks
        {
            get
            {
                if (VoteRanksPoll.Count > 0)
                    return VoteRanksPoll.Values.OrderByDescending(e => e.VoteCount).Take(5).ToArray();
                else return null;
            }
        }
        public static uint Reward(uint rank)
        {
            uint valueReward = 0;
            switch (rank)
            {
                case 1: valueReward = 10000; break;
                case 2: valueReward = 5000; break;
                case 3: valueReward = 3000; break;
                case 4: valueReward = 2000; break;
                case 5: valueReward = 1000; break;
            }
            return valueReward;
        }
        public void WorkThread()
        {
            try
            {
                DateTime now32 = DateTime.Now;
                if (now32.DayOfWeek == DayOfWeek.Saturday)
                {
                    if (now32.Hour == 23 && now32.Minute == 59 && now32.Second <= 1)
                    {
                        if (GetRanks != null)
                        {
                            if (GetRanks.Length > 0)
                            {
                                for (int i = 0; i < GetRanks.Length; i++)
                                {
                                    uint Rank = (uint)(i + 1);
                                    uint cps = Reward(Rank);
                                    Client.GameClient gameClient;
                                    if (Database.Server.GamePoll.TryGetValue(GetRanks[i].UID, out gameClient))
                                    {
                                        gameClient.Player.ConquerPoints += cps;
                                        if (Rank == 1)
                                        {
                                            using (var rec = new ServerSockets.RecycledPacket())
                                            {
                                                var stream = rec.GetStream();
                                                Program.SendGlobalPackets.Enqueue(
                                             new Game.MsgServer.MsgMessage(gameClient.Player.Name + " won the Top Weekly Voter and won " + cps + " CPs.", "ALLUSERS", "Top Weekly Voter", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.TopLeftSystem).GetArray(stream));
                                            }
                                        }
                                        gameClient.SendSysMesage("You've won rank " + Rank + " in weekly voter and won " + cps + " CPs.");
                                    }
                                    else
                                    {
                                        if (Rank == 1)
                                        {
                                            using (var rec = new ServerSockets.RecycledPacket())
                                            {
                                                var stream = rec.GetStream();
                                                Program.SendGlobalPackets.Enqueue(
                                             new Game.MsgServer.MsgMessage(GetRanks[i].Name + " won the Top Weekly Voter and won " + cps + " CPs.", "ALLUSERS", "Top Weekly Voter", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.TopLeftSystem).GetArray(stream));
                                            }
                                        }
                                        WindowsAPI.IniFile data = new WindowsAPI.IniFile("\\Users\\" + GetRanks[i].UID + ".ini");
                                        uint Cps = data.ReadUInt32("Character", "ConquerPoints", 0);
                                        Cps += cps;
                                        data.Write<uint>("Character", "ConquerPoints", Cps);
                                    }
                                }
                                foreach (var c in Database.Server.GamePoll.Values)
                                {
                                    c.CountVote = 0;
                                }
                                WindowsAPI.IniFile ini = new WindowsAPI.IniFile("");
                                foreach (string fname in System.IO.Directory.GetFiles(Program.ServerConfig.DbLocation + "\\Users\\"))
                                {
                                    try
                                    {
                                        ini.FileName = fname;
                                        ini.Write<uint>("Character", "CountVote", 0);
                                    }
                                    catch
                                    {
                                        continue;
                                    }
                                }
                                VoteRanksPoll.Clear();
                                VoteRanksPoll = new Dictionary<uint, Users>();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.SaveException(e);
            }
        }
    }
}
