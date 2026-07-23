using System;
using AccServer.Network;
using AccServer.Database;
using System.Windows.Forms;
using AccServer.Network.Sockets;
using AccServer.Network.AuthPackets;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AccServer
{
    public unsafe class Program
    {
        public static Counter EntityUID;
        public static FastRandom Random = new FastRandom();
        public static ServerSocket AuthServer;
        public static World World;
        public static ushort Port = 9958;
        public static Time32 Login;
        private static object SyncLogin;
        private static System.Collections.Concurrent.ConcurrentDictionary<uint, int> LoginProtection;
        private const int TimeLimit = 10000;
         private static void WorkConsole()
        {
            while (true)
            {
                try
                {
                        CommandsAI(Console.ReadLine());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Database.DataHolder.CreateConnection();
            World = new World();
            World.Init();
            EntityUID = new Counter(0);
            LoginProtection = new System.Collections.Concurrent.ConcurrentDictionary<uint, int>();
            SyncLogin = new object();
            Console.WriteLine("\n  Connecting to the server...");
            Network.Cryptography.AuthCryptography.PrepareAuthCryptography();
            Server.Load();
            AuthServer = new ServerSocket();
            AuthServer.OnClientConnect += AuthServer_OnClientConnect;
            AuthServer.OnClientReceive += AuthServer_OnClientReceive;
            AuthServer.OnClientDisconnect += AuthServer_OnClientDisconnect;
            AuthServer.Enable(Port, "0.0.0.0");
            Console.WriteLine("  Port: " + Port);
            Console.WriteLine("  Connected successfully\n");
            Console.WriteLine("");
            WorkConsole();
            CommandsAI(Console.ReadLine());
        }
        public static void CommandsAI(string command)
        {
            if (command == null) return;
            string[] data = command.Split(' ');
            switch (data[0])
            {
                case "@memo":
                    {
                        var proc = System.Diagnostics.Process.GetCurrentProcess();
                        Console.WriteLine("Thread: " + proc.Threads.Count);
                        Console.WriteLine("Memory (MB): " + ((double)((double)proc.WorkingSet64 / 1024)) / 1024);
                        proc.Close();
                        break;
                    }
                case "@a":
                    {
                        Console.Clear();
                        break;
                    }
                case "@restart":
                    {
                        AuthServer.Disable();
                        Application.Restart();
                        Environment.Exit(0);
                        break;
                    }
            }
        }
        public static List<string> HwidBanned = new List<string>();
        private static void AuthServer_OnClientReceive(byte[] buffer, int length, ClientWrapper arg3)
        {
            var player = arg3.Connector as Client.AuthClient;
            player.Cryptographer.Decrypt(buffer, length);
            player.Queue.Enqueue(buffer, length);
            while (player.Queue.CanDequeue())
            {
                byte[] packet = player.Queue.Dequeue();
                player.Info = new Authentication();
                player.Info.Deserialize(packet);
                player.Account = new AccountTable(player.Info.Username);
                Database.ServerInfo Server = null;
                Forward Fw = new Forward();
                if (player.Info.Server == null)
                {
                    arg3.Disconnect();
                    return;
                }
                if (Database.Server.Servers.TryGetValue(player.Info.Server, out Server))
                {
                    if (!player.Account.exists)
                    {
                        Fw.Type = Forward.ForwardType.WrongAccount;
                    }
                    if (player.Account.Password == player.Info.Password && player.Account.exists)
                    {
                        Fw.Type = Forward.ForwardType.Ready;
                        if (player.Account.EntityID == 0)
                        {
                            using (MySqlCommand cmd = new MySqlCommand(MySqlCommandType.SELECT))
                            {
                                cmd.Select("configuration");
                                using (MySqlReader r = new MySqlReader(cmd))
                                {
                                    if (r.Read())
                                    {
                                        EntityUID = new Counter(r.ReadUInt32("EntityID"));
                                        player.Account.EntityID = EntityUID.Next;
                                        using (MySqlCommand cmd2 = new MySqlCommand(MySqlCommandType.UPDATE).Update("configuration")
                                        .Set("EntityID", player.Account.EntityID)) cmd2.Execute();
                                        player.Account.Save();
                                    }
                                }
                            }
                        }
                    }
                    if (Fw.Type != Forward.ForwardType.Ready)
                    {
                        Fw.Type = Forward.ForwardType.InvalidInfo;
                    }
                    if (player.Account.Banned)
                    {
                        Fw.Type = Forward.ForwardType.Banned;
                    }
                    lock (SyncLogin)
                    {
                        if (Fw.Type == Forward.ForwardType.Ready)
                        {
                            try
                            {
                                using (MySqlCommand cmd = new MySqlCommand(MySqlCommandType.SELECT))
                                {
                                    cmd.Select("macs").Where("id", player.Account.EntityID);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }
                            Fw.Identifier = player.Account.EntityID;
                            Fw.State = (uint)player.Account.State;
                            Fw.IP = Server.IP;
                            Fw.Port = Server.Port;
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Player: {0} has Connected in server: {1}! IP:[{2}].", player.Info.Username, player.Info.Server, player.IP);
                        }
                    }
                }
                else
                    Fw.Type = Forward.ForwardType.ServersNotConfigured;
                player.Send(Fw);
            }
        }
        private static void AuthServer_OnClientDisconnect(ClientWrapper obj)
        {
            obj.Disconnect();
        }
        private static void AuthServer_OnClientConnect(ClientWrapper obj)
        {
            Client.AuthClient authState;
            obj.Connector = (authState = new Client.AuthClient(obj));
            authState.Cryptographer = new Network.Cryptography.AuthCryptography();
        }
    }
}