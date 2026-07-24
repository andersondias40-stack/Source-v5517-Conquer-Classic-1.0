
using System;
using System.IO;
using System.Linq;

namespace COServer.Database
{
    public static class GroupServerList
    {
        public static Server MyServerInfo = null;
        public static Server InterServer = null;
        public class Server
        {
            public uint ID;
            public uint MapID;
            public uint X;
            public uint Y;
            public uint Group;
            public string Name;
            public byte TransferType = 0;

            public string IPAddress = "";
            public ushort Port = 0;
        }
        public static SafeDictionary<uint, Server> GroupServers = new SafeDictionary<uint, Server>();

        public static Server[] CanTransferServers(bool canbr = false)
        {
            if (canbr == true)
            {
                return GroupServers.Values.Where(p => p.ID != MyServerInfo.ID && MyServerInfo.TransferType == p.TransferType && p.ID < 100).ToArray();
            }
            return GroupServers.Values.Where(p => p.ID != MyServerInfo.ID && MyServerInfo.TransferType == p.TransferType && p.ID < 100 && p.Name != Program.ServerConfig.ServerName).ToArray();
        }

        public static Server GetServer(uint ID)
        {
            return GroupServers.Values.Where(p => p.ID == ID).FirstOrDefault();
        }
        public static Server GetServer(string IpAddres, ushort Port)
        {
            Server server = null;
            foreach (var _server in GroupServers.Values)
            {
                if (_server.IPAddress == IpAddres && Port == _server.Port)
                {
                    server = _server;
                    break;
                }
            }
            return server;
        }
        public static void Load()
        {
            string[] baseText = File.ReadAllLines(Program.ServerConfig.DbLocation + "client_config.ini");
            foreach (var bas_line in baseText)
            {
                Database.DBActions.ReadLine line = new DBActions.ReadLine(bas_line, ' ');
                Server obj = new Server();
                obj.ID = line.Read((uint)0);
                obj.Name = line.Read("");
                obj.MapID = line.Read((uint)0);
                obj.X = line.Read((uint)0);
                obj.Y = line.Read((uint)0);
                obj.TransferType = line.Read((byte)0);
                //    line.Read((uint)0);
                obj.Group = line.Read((uint)0);
                line.Read((uint)0);
                line.Read((uint)0);
                obj.IPAddress = line.Read("");
                obj.Port = line.Read((ushort)0);
                GroupServers.Add(obj.ID, obj);

                if (obj.Name == Program.ServerConfig.ServerName)
                    MyServerInfo = obj;
                if (obj.Name == "Dark")
                    InterServer = obj;
            }
        }
    }
}
