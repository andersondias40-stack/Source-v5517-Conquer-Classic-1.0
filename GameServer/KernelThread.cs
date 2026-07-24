using COServer.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COServer
{
    public class KernelThread
    {
        static int _last = 0;

        public static int GetOnline()
        {
            int current = Database.Server.GamePoll.Count;
            if (current > _last)
                _last = current;
            return current;
        }

        public static int GetMaxOnline()
        { return _last; }
        private ThreadItem _thread, eventsthread;
        public const int
            TournamentsStamp = 1000,
          BroadCastStamp = 1000,
          ResetDayStamp = 6000,
          SaveDatabaseStamp = 180000;//3600000 -- 30 MIN 1800000
        public KernelThread(int interval)
        {
            _thread = new ThreadItem(interval, ServerFunctions);
            _thread.Open();
            eventsthread = new ThreadItem(interval, EventProcess);
            eventsthread.Open();
        }
        private void EventProcess()
        {
            EventsLib.EventManager.Worker();
        }
        private int lastonline = 0;
        public int Online
        {
            get
            {
                int current = Database.Server.GamePoll.Count;
                if (current > lastonline)
                    lastonline = current;
                return current;
            }
        }
        public int MaxOnline { get { return lastonline; } }
        public static Time32 UpdateServerStatus = Time32.Now;
        public static DateTime LastServerPulse, LastPokerPulse, LastSavePulse, LastGuildPulse, LastDBUpdate;
        private void ServerFunctions()
        {
            var clock = Time32.Now;
            if (clock > UpdateServerStatus)
            {
                Console.Title = Program.ServerConfig.ServerName + " - Online: " + GetOnline() + " - Max " + GetMaxOnline() + " QueuePackets: " + ServerSockets.PacketRecycle.Count;

                UpdateServerStatus = Time32.Now.AddSeconds(5);
                LastServerPulse = DateTime.Now;
                using (var cmd = new Database.MySqlCommand(MySqlCommandType.UPDATE).Update("onlineplayers"))
                {
                    cmd.Set("Online", GetOnline())// that too
                        .Execute();
                }
            }
            if (DateTime.Now > LastDBUpdate.AddMinutes(1))
            {
                LastDBUpdate = DateTime.Now;
                Database.ServerStats.Update();
            }
            if (clock > Program.ResetRandom)
            {
                Program.GetRandom = new FastRandom(Environment.TickCount);
                Program.ResetRandom = Time32.Now.AddMinutes(30);
            }
            if (DateTime.Now > LastGuildPulse.AddHours(24))
            {
                foreach (var guilds in Role.Instance.Guild.GuildPoll.Values)
                {
                    guilds.CreateMembersRank();
                    guilds.UpdateGuildInfo();
                }
                LastGuildPulse = DateTime.Now;
            }

            //if (DateTime.Now.DayOfWeek >= (DayOfWeek)1 && DateTime.Now.DayOfWeek <= (DayOfWeek)4 || DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
            //{
            //    if (DateTime.Now.Hour == 1 || DateTime.Now.Hour == 5 || DateTime.Now.Hour == 9 ||
            //        DateTime.Now.Hour == 13 || DateTime.Now.Hour == 17 || DateTime.Now.Hour == 21)
            //    {
            //        Game.MsgMonster.BossesBase.BossesTimer(DateTime.Now.DayOfWeek);
            //    }
            //}

            Game.MsgTournaments.MsgSchedules.CheckUp(clock);

            Game.MsgTournaments.MsgBroadcast.Work(clock);

            Database.Server.Reset(clock);
            Program.SaveDBPayers(clock);
        }
    }
}
