using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using COServer.Role;

namespace COServer.Game.MsgServer
{
    public struct MsgLoginClient
    {
        public ushort Length;
        public ushort PacketID;
        public uint AccountHash;
        public uint Key, HDSerial;
        public string MachineName, MacAddress, HWID, Username;
        public static ConcurrentDictionary<string, List<string>> PlayersIP = new ConcurrentDictionary<string, List<string>>();

        [PacketAttribute(GamePackets.LoginGame)]
        public unsafe static void LoginGame(Client.GameClient client, ServerSockets.Packet packet)
        {
            client.OnLogin = new MsgLoginClient()
            {
                Key = packet.ReadUInt32(),
                AccountHash = packet.ReadUInt32(),
            };
            client.ClientFlag |= Client.ServerFlag.OnLoggion;
            Database.ServerDatabase.LoginQueue.TryEnqueue(client);
        }

        public unsafe static void LoginHandler(Client.GameClient client, MsgLoginClient packet)
        {
            client.ClientFlag &= ~Client.ServerFlag.OnLoggion;

            if (client.Socket != null)
            {
                if (client.Socket.RemoteIp == "NONE")
                {
                    client.Socket.Disconnect();
                    Console.WriteLine("Break login client.");
                    return;
                }
            }
            try
            {
                if (client.OnLogin.Key > 100000000 || client.OnLogin.Key < 1000000)
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        string Messaj = "You can't do this";
                        client.Send(new MsgServer.MsgMessage(Messaj, "ALLUSERS", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Dialog).GetArray(stream));

                    }
                    return;
                }
                string BanMessaje;
                if (Database.SystemBanned.IsBanned(client.Socket.RemoteIp, out BanMessaje))
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        client.Send(new MsgServer.MsgMessage(BanMessaje, "ALLUSERS", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Dialog).GetArray(stream));
                    }
                    return;
                }
                //if (Database.SystemBanned.IsBanned(client.OnLogin.HWID, out BanMessaje))
                //{
                //    using (var rec = new ServerSockets.RecycledPacket())
                //    {
                //        var stream = rec.GetStream();
                //        client.Send(new MsgServer.MsgMessage(BanMessaje, "ALLUSERS", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Dialog).GetArray(stream));
                //    }
                //    return;
                //}
                if ((client.ClientFlag & Client.ServerFlag.AcceptLogin) != Client.ServerFlag.AcceptLogin)
                {

                    var login = client.OnLogin;

                    client.ConnectionUID = login.Key;

                    if (client != null && client.Player != null)
                    {
                        if (Role.OfflineMiningManager.IsMiningOffline(client.Player.UID))
                        {
                            Console.WriteLine($"[{DateTime.Now}] {client.Player.Name} reconectado. Finalizando mineração offline.");
                            client.SendSysMesage("Você minerou itens offline! Aqui estão os resultados...");
                            Role.OfflineMiningManager.StopOfflineMining(client);
                        }
                    }

                    if (Role.Instance.OfflineVendorManager.IsVendingOffline(client.Player.UID))
                    {
                        client.SendSysMesage("Você vendeu itens offline! Verificando resultados...");
                        Role.Instance.OfflineVendorManager.StopOfflineVending(client.Player.UID);
                    }


                    if (Database.SystemBannedAccount.IsBanned(client.ConnectionUID, out BanMessaje))
                    {

                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            string aMessaj = "Your account has been banned for: " + BanMessaje + " ";
                            client.Send(new MsgServer.MsgMessage(aMessaj, "ALLUSERS", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Dialog).GetArray(stream));
                        }
                        return;
                    }

                    //if (Database.SystemBannedPC.IsBanned(client, out BanMessaje))
                    //{

                    //    using (var rec = new ServerSockets.RecycledPacket())
                    //    {
                    //        var stream = rec.GetStream();
                    //        client.Send(new MsgServer.MsgMessage(BanMessaje, "ALLUSERS", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Dialog).GetArray(stream));
                    //    }
                    //    return;
                    //}
                    string Messaj = "NEW_ROLE";
                    if (Database.ServerDatabase.AllowCreate(login.Key) == false)
                    {

                        Client.GameClient InGame = null;
                        if (Database.Server.GamePoll.TryGetValue((uint)login.Key, out InGame))
                        {
                            if (InGame.Player != null)
                            {
                                Console.WriteLine("Account is trying to join but is in use. " + InGame.Player.Name);

                                if (InGame.Player.UID == 0)
                                {
                                    Database.Server.GamePoll.TryRemove((uint)login.Key, out InGame);
                                    if (InGame != null && InGame.Player != null)
                                    {
                                        if (InGame.Map != null)
                                            InGame.Map.Denquer(InGame);
                                    }
                                }
                            }
                            InGame.Socket.Disconnect();
                            Messaj = "Sorry, your account is already online, try again.";
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                client.Send(new MsgServer.MsgMessage(Messaj, "ALLUSERS", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Dialog).GetArray(stream));
                            }
                            if (InGame.TRyDisconnect-- == 0)
                            {
                                if (InGame.Player != null && InGame.FullLoading)
                                {
                                    InGame.ClientFlag |= Client.ServerFlag.Disconnect;
                                    //if ((InGame.ClientFlag & Client.ServerFlag.LoginFull) == Client.ServerFlag.LoginFull)
                                    Database.ServerDatabase.SaveClient(InGame);
                                }
                                InGame.Socket.Disconnect();
                                client.Disconnect(InGame);
                                Database.Server.GamePoll.TryRemove((uint)login.Key, out InGame);
                                if (InGame != null && InGame.Player != null)
                                {
                                    if (InGame.Map != null)
                                        InGame.Map.Denquer(InGame);
                                }
                            }
                        }
                        else
                        {

                            Database.Server.GamePoll.TryAdd((uint)login.Key, client);
                            Messaj = "ANSWER_OK";
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                if ((client.ClientFlag & Client.ServerFlag.CreateCharacterSucces) != Client.ServerFlag.CreateCharacterSucces)
                                {
                                    //  lock (client.Player)
                                    //      client.Player = new Role.Player(client);
                                    Database.ServerDatabase.LoadCharacter(client, (uint)login.Key);
                                }
                                if (client.Socket.RemoteIp == "NONE")
                                {
                                    Database.Server.GamePoll.Remove((uint)login.Key);
                                    client.Socket.Disconnect();
                                    return;
                                }
                                client.ClientFlag |= Client.ServerFlag.AcceptLogin;
                                Console.WriteLine(client.Player.Name + " has logged in on IP [" + client.Socket.RemoteIp + "]", ConsoleColor.Green);
                                client.IP = client.Socket.RemoteIp;
                                try
                                {
                                    List<string> ListP;
                                    if (PlayersIP.TryGetValue(client.IP, out ListP))
                                    {
                                        if (!ListP.Contains(client.Player.Name))
                                            PlayersIP[client.IP].Add(client.Player.Name);
                                    }
                                    else
                                        PlayersIP.TryAdd(client.IP, new List<string>() { client.Player.Name });
                                }
                                catch
                                {

                                }
                                client.Send(new MsgServer.MsgMessage(Messaj, "ALLUSERS", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Dialog).GetArray(stream));
                                client.Send(stream.LoginHandlerCreate(1, client.Player.Map));
                                MsgLoginHandler.LoadMap(client, stream);
                            }
                        }
                    }
                    else//new client
                    {
                        client.ClientFlag |= Client.ServerFlag.CreateCharacter;
                        //client.Socket.OverrideTiming = true;
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            client.Send(new MsgServer.MsgMessage(Messaj, "ALLUSERS", MsgMessage.MsgColor.red, MsgMessage.ChatMode.Dialog).GetArray(stream));
                        }
                    }
                }
            }
            catch (Exception e) { Console.WriteException(e); }
        }
    }
}
