using System;
using System.Threading;

namespace COServer.ServerSockets
{
    public class BruteForceEntry
    {
        public string IPAddress;
        public int WatchCheck;
        public Time32 Unbantime;
        public Time32 AddedTimeRemove;
    }

    public class BruteforceProtection
    {
        private SafeDictionary<string, BruteForceEntry> collection = new SafeDictionary<string, BruteForceEntry>();
        private int BanOnWatch;


        private void _internalInit()
        { 

            while (true)
            {

                Time32 Now = Time32.Now;
                foreach (BruteForceEntry bfe in collection.Values)
                {
                    if (bfe.AddedTimeRemove <= Now)
                    {
                        collection.Remove(bfe.IPAddress);
                    }
                    else if (bfe.Unbantime.Value != 0)
                    {
                        if (bfe.Unbantime.Value <= Now.Value)
                        {
                            collection.Remove(bfe.IPAddress);
                        }
                    }
                }

                Thread.Sleep(1000);
            }
        }

        public void Init(int WatchBeforeBan)
        {
            BanOnWatch = WatchBeforeBan;
            new Thread(new ThreadStart(_internalInit)).Start();
        }

        public void AddWatch(string IPAddress)
        {
            lock (collection)
            {
                BruteForceEntry bfe;
                if (!collection.TryGetValue(IPAddress, out bfe))
                {
                    bfe = new BruteForceEntry();
                    bfe.IPAddress = IPAddress;
                    bfe.WatchCheck = 1;
                    bfe.AddedTimeRemove = Time32.Now.AddMinutes(3);
                    bfe.Unbantime = new Time32(0);
                    collection.Add(IPAddress, bfe);
                }
                else
                {
                    bfe.WatchCheck++;
                    if (bfe.WatchCheck >= BanOnWatch)
                    {
                        bfe.Unbantime = Time32.Now.AddMinutes(3);
                    }
                }
            }
        }
        public bool AllowAddress(string IPAddress)
        {
            foreach (var server in Database.GroupServerList.GroupServers.Values)
                if (server.IPAddress == IPAddress)
                    return true;
            return false;
        }
        public bool IsBanned(string IPAddress)
        {
            bool check = false;
            BruteForceEntry bfe;
            if (collection.TryGetValue(IPAddress, out bfe))
            {
                check = (bfe.Unbantime.Value != 0);
            }
            return check;
        }
    }
}
