using COServer.Game.MsgServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COServer.Database
{
    public class VoteSystem
    {

        public class User
        {
            public uint UID;
            public string IP;
            public string HWID;
            public string Mac;
            public DateTime Timer = new DateTime();
            public override string ToString()
            {
                var writer = new DBActions.WriteLine('/');
                writer.Add(UID).Add(IP).Add(Timer.Ticks)/*.Add(HWID).Add(Mac)*/;
                return writer.Close();
            }
        }

        private static List<User> UsersPoll = new List<User>();


        public static bool TryGetObject(uint UID, string IP, /*string HWID,string Mac,*/ out User obj)
        {
            foreach (var _obj in UsersPoll)
            {
                Client.GameClient player;
                if (Database.Server.GamePoll.TryGetValue(_obj.UID, out player))
                {
                    if (_obj.UID == UID || _obj.IP == IP/* || _obj.HWID == HWID || _obj.Mac == Mac*/)
                    {
                        obj = _obj;
                        return true;
                    }
                }
            }
            obj = null;
            return false;
        }
        public static bool CanVote(Client.GameClient client)
        {
            User _user;
            if (TryGetObject(client.Player.UID, client.Socket.RemoteIp, /*client.OnLogin.HWID,*/ /*client.OnLogin.MacAddress,*/ out _user))
            {
                if (_user.Timer.AddHours(12) < DateTime.Now)
                    return true;
                else
                    return false;
            }
            return true;
        }
        public static void CheckUp(Client.GameClient client)
        {
            if (client.Player.StartVote)
            {
                if (Time32.Now > client.Player.StartVoteStamp)
                {
                    if (CanVote(client))
                    {
                        User _user;
                        if (TryGetObject(client.Player.UID, client.Socket.RemoteIp, /*client.OnLogin.HWID,*/ /*client.OnLogin.MacAddress,*/ out _user))
                        {
                            _user.Timer = DateTime.Now;
                        }
                        else
                        {
                            _user = new User();
                            _user.UID = client.Player.UID;
                            _user.IP = client.Socket.RemoteIp;
                            _user.Timer = DateTime.Now;
                            //_user.HWID = client.OnLogin.HWID;
                            //_user.Mac = client.OnLogin.MacAddress;
                            UsersPoll.Add(_user);
                        }
                        client.Player.VotePoints += 1;
                        client.CountVote += 1;
                        VoteRank.Users hero;
                        if(VoteRank.VoteRanksPoll.TryGetValue(client.Player.UID, out hero))
                        {
                            hero.VoteCount = client.CountVote;
                        }
                        else
                        {
                            VoteRank.Users newhero = new VoteRank.Users() { Name = client.Player.Name, UID = client.Player.UID, VoteCount = client.CountVote };
                            VoteRank.VoteRanksPoll.Add(newhero.UID, newhero);
                        }
                        client.SendSysMesage("Thank you for your support. You`ve received 1 Vote Point and you got " + client.Player.VotePoints + ".");
                    }
                    else client.SendSysMesage("You've already claimed your reward for voting. Each player may only vote once every 12 hours.");
                    client.Player.StartVote = false;
                }
            }
        }
        public static void Save()
        {
            using (Database.DBActions.Write _wr = new Database.DBActions.Write("Votes.txt"))
            {
                foreach (var _obj in UsersPoll)
                    _wr.Add(_obj.ToString());
                _wr.Execute(DBActions.Mode.Open);
            }
        }
        public static void Load()
        {
            using (Database.DBActions.Read r = new Database.DBActions.Read("Votes.txt"))
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
                        user.Timer = DateTime.FromBinary(reader.Read((long)0));
                        user.HWID = reader.Read("");
                        user.Mac = reader.Read("");
                        UsersPoll.Add(user);
                    }
                }
            }

        }

    }
}
