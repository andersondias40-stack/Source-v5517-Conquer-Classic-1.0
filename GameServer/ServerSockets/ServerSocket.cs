using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Concurrent;

namespace COServer.ServerSockets
{
    public class ServerSocket
    {
        private bool Alive = false;
        private Socket Connection;

        public Socket GetConnection
        {
            get { return Connection; }
        }
        public bool IsAlive
        {
            get { return Alive; }
        }

        private string ServerAddresIP = "";
        public uint SPort;
        public string ServerName = "";

        private DateTime StampConnect = new DateTime();
        private Action<SecuritySocket> ProcessConnection, ProcessDisconnect;
        private Action<SecuritySocket, Packet> ProcessReceive;
        private BruteforceProtection Bruteforce;
        public MyList<SecuritySocket> Clients = new MyList<SecuritySocket>();
        public ServerSocket(Action<SecuritySocket> _processConnection, Action<SecuritySocket, Packet> _procesreceive, Action<SecuritySocket> _processdisconnect)
        {
            ProcessConnection = _processConnection;
            ProcessReceive = _procesreceive;
            ProcessDisconnect = _processdisconnect;
            Connection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        public void Initilize(ushort MaxBufferSend, ushort MaxBufferReceive, uint MaxAcceptConnections, uint MaxClientsConnections)
        {
            Connection.ReceiveBufferSize = MaxBufferReceive;
            Connection.SendBufferSize = MaxBufferSend;
        }

        public void Connect(string IpAddres, ushort port, string aServerName)
        {
            SPort = port;
            ServerAddresIP = IpAddres;
            ServerName = aServerName;

            /*  Connection.Blocking = false;
              Connection.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
              Connection.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
              Connection.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
              Connection.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
              Connection.ReceiveBufferSize = 16384;
              Connection.SendBufferSize = 16384;
             */
            TryConnect(aServerName);
        }
        public void CallBack(Socket socket)
        {
            if (!Alive)
            {
                if (DateTime.Now > StampConnect)
                {
                    StampConnect = DateTime.Now.AddSeconds(5);
                    Console.WriteLine("Server Trying connect to InterServer...");
                    if (Connection.Poll(0, SelectMode.SelectRead))
                    {
                        Alive = Connection.Connected;
                        if (Alive)
                        {
                            var client = new SecuritySocket(this, ProcessDisconnect, ProcessReceive);
                            client.Create(Connection);
                            if (ProcessConnection != null)
                            {
                                ProcessConnection(client);

                            }
                        }
                    }
                }
            }

        }
        private void TryConnect(string servername)
        {
            /* Connection.Blocking = false;
             Connection.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
             Connection.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
             Connection.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
             Connection.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
             */

            //MyConsole.WriteLine("Server Trying connect to InterServer...");
            Connection.BeginConnect(ServerAddresIP, (int)SPort, new AsyncCallback(ConnectCallback), null);
        }
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Connection.EndConnect(ar);

                Console.WriteLine($"Socket connected to {Connection.RemoteEndPoint.ToString()} successful !");

                Connection.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                Connection.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                Connection.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
                Connection.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                Connection.Blocking = false;

                // Set option that allows socket to receive out-of-band information in the data stream.
                //  Connection.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.OutOfBandInline, true);

                Connection.ReceiveBufferSize = 16384;
                Connection.SendBufferSize = 16384;

                Alive = Connection.Connected;
                if (Alive)
                {
                    var client = new SecuritySocket(this, ProcessDisconnect, ProcessReceive);
                    client.Create(Connection);
                    client.Connection.Blocking = false;
                    if (ProcessConnection != null)
                    {
                        ProcessConnection(client);

                    }
                }
            }
            catch
            {

                TryConnect(ServerName);
            }
        }

        public void Open(string IpAddres, ushort port, int backlog)
        {
           // Bruteforce = new BruteforceProtection();
          //  Bruteforce.Init(10);

            SPort = port;
            ServerAddresIP = IpAddres;
            Connection.Bind(new IPEndPoint(IPAddress.Any, port));

            Connection.Listen((int)10);//100
            Connection.Blocking = false;
            Connection.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            Connection.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            Connection.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            Connection.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

            Console.WriteLine("Listening on port : " + port.ToString());
            Alive = true;
        }
        public void Accept()
        {

            try
            {
                if (Alive)
                {
                    if (Connection.Poll(0, SelectMode.SelectRead))
                    {

                        var socket = Connection.Accept();
                        string RemoteIP = (socket.RemoteEndPoint as IPEndPoint).Address.ToString();

                      // if (!Bruteforce.IsBanned(RemoteIP) || Bruteforce.AllowAddress(RemoteIP))
                        {

                            SecuritySocket user = new SecuritySocket(this, ProcessDisconnect, ProcessReceive);
                            user.Create(socket);
                            Clients.Add(user);


                            //Console.WriteLine("IP " + user.RemoteIp + " try to connect on port  " + SPort + " ");


                            if (ProcessConnection != null)
                            {

                                ProcessConnection.Invoke(user);
                            }


                            user.ConnectFull = true;
                            //Bruteforce.AddWatch(user.RemoteIp);
                        }
                        //else
                        //{
                        //    CloseNewSocket(socket);
                        //}

                    }
                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); Console.SaveException(e); }


        }
        public void CloseNewSocket(Socket socket)
        {
            WindowsAPI.ws2_32.shutdown(socket.Handle, WindowsAPI.ws2_32.ShutDownFlags.SD_BOTH);
            WindowsAPI.ws2_32.closesocket(socket.Handle);
            socket.Dispose();
            GC.SuppressFinalize(socket);
        }



        public void Close()
        {
            if (Alive)
            {

                Alive = false;
                Connection.Close(1);

            }
        }
    }
}
