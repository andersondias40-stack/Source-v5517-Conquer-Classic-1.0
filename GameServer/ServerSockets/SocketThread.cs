using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace COServer.ServerSockets
{
    public class SocketThread
    {

        const int SOCKET_PROCESS_INTERVAL = 1, FD_SETSIZE = 2048;

        public static MyList<SecuritySocket> ConnectionPoll = new MyList<SecuritySocket>();

        private static ServerSocket[] Sockets;
        public SocketThread(params ServerSocket[] _Sockets)
        {
            Sockets = _Sockets;
            var ThreadItem = new ThreadItem(SOCKET_PROCESS_INTERVAL, CheckUp);
            ThreadItem.Open();

        }
        public static void CheckUp()
        {
            try
            {
                foreach (var socket in ConnectionPoll.GetValues())
                {
                    {
                        try
                        {
                            socket.ReceiveBuffer();
                            socket.HandlerBuffer();

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                            continue;
                        }
                        try
                        {
                            while (SecuritySocket.TrySend(socket)) ;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                    }

                }


                foreach (var socket in Sockets)
                {
                    if (socket == null)
                        continue;

                    socket.Accept();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    }
}