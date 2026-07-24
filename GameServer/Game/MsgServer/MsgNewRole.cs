using System;

namespace COServer.Game.MsgServer
{
    public static class MsgNewRole
    {

        public static object SynName = new object();


        public static void GetNewRoleInfo(this ServerSockets.Packet msg, out string name, out ushort Body, out byte Class)
        {
            msg.ReadBytes(16);
            name = msg.ReadCString(16);//20
            msg.ReadBytes(16);
            Body = msg.ReadUInt16();
            Class = msg.ReadUInt8();

        }

        [PacketAttribute(Game.GamePackets.NewClient)]
        public unsafe static void CreateCharacter(Client.GameClient client, ServerSockets.Packet stream)
        {
            if ((client.ClientFlag & Client.ServerFlag.CreateCharacter) == Client.ServerFlag.CreateCharacter)
            {
                client.ClientFlag &= ~Client.ServerFlag.AcceptLogin;


                string CharacterName; ushort Body; byte Class;

                stream.GetNewRoleInfo(out CharacterName, out Body, out Class);

                //last update
                //switch (Class)
                //{
                //    case 0:
                //    case 1: Class = 100; break;
                //    case 2:
                //    case 3: Class = 10; break;
                //    case 4:
                //    case 5: Class = 40; break;
                //    case 6:
                //    case 7: Class = 20; break;
                //    case 8:
                //    case 9: Class = 50; break;
                //    case 10:
                //    case 11: Class = 60; break;
                //}


                if (!ExitBody(Body))
                {
                    client.Send(new MsgServer.MsgMessage("Wrong body.", MsgMessage.MsgColor.red, MsgMessage.ChatMode.PopUP).GetArray(stream));
                    return;
                }
                if (!ExitClass(Class))
                {
                    client.Send(new MsgServer.MsgMessage("Wrong class.", MsgMessage.MsgColor.red, MsgMessage.ChatMode.PopUP).GetArray(stream));
                    return;
                }
                // Create char - Criação de account - create account char mapa VillageGateMan

                CharacterName = CharacterName.Replace("\0", "");
                if (Program.NameStrCheck(CharacterName))
                {
                    if (!Database.Server.NameUsed.Contains(CharacterName.GetHashCode()))
                    {
                        client.ClientFlag &= ~Client.ServerFlag.CreateCharacter;

                        lock (Database.Server.NameUsed)
                            Database.Server.NameUsed.Add(CharacterName.GetHashCode());

                        client.Player.Name = CharacterName;
                        client.Player.Class = Class;
                        client.Player.Body = Body;
                        client.Player.Level = 1;
                        client.Player.Map = 1010;
                        client.Player.X = 061;
                        client.Player.Y = 109;
                        client.Player.Money += 10000;

                        if (!client.Player.CanClaimFreeVip)
                        {
                            Database.VIPSystem.CheckUp(client);
                        }

                        Database.DataCore.LoadClient(client.Player);

                        client.Player.UID = client.ConnectionUID;

                        Database.DataCore.AtributeStatus.GetStatus(client.Player);

                        // Face e Hair padrão fixos
                        if (client.Player.Mesh < 1005) // masculino
                        {
                            client.Player.Face = 10;   // face masculina padrão (troque pelo ID desejado)
                            client.Player.Hair = 310;  // cabelo masculino padrão (troque pelo ID desejado)
                        }
                        else // feminino
                        {
                            client.Player.Face = 210;  // face feminina padrão (troque pelo ID desejado)
                            client.Player.Hair = 410;  // cabelo feminino padrão (troque pelo ID desejado)
                        }

                        if (client.Player.Class == 40)
                        {
                            // lógica específica para classe 40, se necessário
                        }
                        else if (client.Player.Class == 100)
                        {
                            if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Thunder))
                                client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Thunder);
                            if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Cure))
                                client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Cure);
                            if (!client.MySpells.ClientSpells.ContainsKey((ushort)Role.Flags.SpellID.Meditation))
                                client.MySpells.Add(stream, (ushort)Role.Flags.SpellID.Meditation);

                        }
                        else
                        client.NewPlayer = true;
                        client.Send(new MsgServer.MsgMessage("ANSWER_OK", MsgMessage.MsgColor.red, MsgMessage.ChatMode.PopUP).GetArray(stream));
                        Database.ServerStats.LastChar = client.Player.Name;
                        client.Status.MaxHitpoints = client.CalculateHitPoint();
                        client.Player.HitPoints = (int)client.Status.MaxHitpoints;
                        client.ClientFlag |= Client.ServerFlag.CreateCharacterSucces;
                        if ((client.ClientFlag & Client.ServerFlag.CreateCharacterSucces) == Client.ServerFlag.CreateCharacterSucces)
                        {
                            if (Database.ServerDatabase.AllowCreate(client.ConnectionUID))
                            {
                                client.ClientFlag &= ~Client.ServerFlag.CreateCharacterSucces;
                                Database.ServerDatabase.CreateCharacte(client);
                                Database.ServerDatabase.SaveClient(client);
                                MsgTournaments.MsgSchedules.SendSysMesage("Welcome new player, " + client.Player.Name + " to Conquer Classic 1.0", MsgServer.MsgMessage.ChatMode.TopLeftSystem, MsgServer.MsgMessage.MsgColor.white);
                                Console.WriteLine(client.Player.Name + " has created a new account IP [" + client.Socket.RemoteIp + "]");
                                Program.DiscordAPIfoundslog.Enqueue($"``NewPlayer ! {client.Player.Name}!``");
                                return;
                            }
                        }
                    }
                    else
                    {
                        client.Send(new MsgServer.MsgMessage("The name is in use! Try other name.", MsgMessage.MsgColor.red, MsgMessage.ChatMode.PopUP).GetArray(stream));
                    }
                }
                else
                {
                    client.Send(new MsgServer.MsgMessage("Invalid characters in name!", MsgMessage.MsgColor.red, MsgMessage.ChatMode.PopUP).GetArray(stream));
                }
            }
        }

        public static bool ExitBody(ushort _body)
        {
            return (_body == 1003 || _body == 1004 || _body == 2001 || _body == 2002);
        }

        public static bool ExitClass(byte cls)
        {
            return (cls == 10 || cls == 20 || cls == 40
                || cls == 50 || cls == 60 || cls == 70 || cls == 100 || cls == 80 || cls == 160);
        }
    }
}
