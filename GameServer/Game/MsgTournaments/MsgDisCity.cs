using System;
using System.Collections.Generic;

namespace COServer.Game.MsgTournaments
{
    public class MsgDisCity
    {

        private ProcesType Mode;
        private DateTime FinishTime = new DateTime();
        private DateTime TeleportToMap4 = new DateTime();

        private int PlayersMap2 = 0;
        private int PlayersMap3 = 0;

        public List<uint> RewardPlayers = new List<uint>();

        private Role.GameMap Map1;
        private Role.GameMap Map2;
        private Role.GameMap Map3;
        private Role.GameMap Map4;

        public MsgDisCity()
        {
            Mode = ProcesType.Dead;
        }

        public bool IsInDisCity(uint Map)
        {

            return 2021 == Map || 2022 == Map || 2023 == Map || 2024 == Map;
        }
        public bool AllowJoin() { return Mode == ProcesType.Idle; }

        public void CreateMaps()
        {
            if (Map1 == null)
            {
                if (Database.Server.ServerMaps.ContainsKey(2021))
                {
                    Map1 = Database.Server.ServerMaps[2021];
                    Console.WriteLine("Map1 (ID 2021) inicializado com sucesso.");
                }
                else
                    Console.WriteLine("Map1 (ID 2021) não encontrado em ServerMaps.");
            }

            if (Map2 == null)
            {
                if (Database.Server.ServerMaps.ContainsKey(2022))
                {
                    Map2 = Database.Server.ServerMaps[2022];
                    Console.WriteLine("Map2 (ID 2022) inicializado com sucesso.");
                }
                else
                    Console.WriteLine("Map2 (ID 2022) não encontrado em ServerMaps.");
            }

            if (Map3 == null)
            {
                if (Database.Server.ServerMaps.ContainsKey(2023))
                {
                    Map3 = Database.Server.ServerMaps[2023];
                    Console.WriteLine("Map3 (ID 2023) inicializado com sucesso.");
                }
                else
                    Console.WriteLine("Map3 (ID 2023) não encontrado em ServerMaps.");
            }

            if (Map4 == null)
            {
                if (Database.Server.ServerMaps.ContainsKey(2024))
                {
                    Map4 = Database.Server.ServerMaps[2024];
                    Console.WriteLine("Map4 (ID 2024) inicializado com sucesso.");
                }
                else
                    Console.WriteLine("Map4 (ID 2024) não encontrado em ServerMaps.");
            }
        }

        public void Open()
        {
            if (Mode == ProcesType.Dead)
            {
                Console.WriteLine("DisCity event is opening...");
                RewardPlayers.Clear();
                CreateMaps();
                Mode = ProcesType.Idle; // Certifique-se de que o modo está sendo definido como Idle
                FinishTime = DateTime.Now.AddMinutes(5);
                PlayersMap2 = PlayersMap3 = 0;
                TeleportToMap4 = DateTime.Now.AddMinutes(25);
            }
            else
            {
                Console.WriteLine($"DisCity event is already open. Current mode: {Mode}");
            }
        }

        public void CheckUp()
        {
            if (Mode == ProcesType.Idle)
            {
                if (DateTime.Now > FinishTime)
                {
                    Console.WriteLine("DisCity finalizando...");
                    FinishTime = DateTime.Now.AddMinutes(60);

                    MsgSchedules.SendSysMesage("Dis City has started, all players receive experience worth x5 ExpBalls! Signups are now closed.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.white);

                    Mode = ProcesType.Alive;

                    foreach (var user in Map1.Values)
                        user.GainExpBall(600 * 5, true, Role.Flags.ExperienceEffect.angelwing);
                    foreach (var user in Map2.Values)
                        user.GainExpBall(600 * 5, true, Role.Flags.ExperienceEffect.angelwing);
                }
            }
            else if (Mode == ProcesType.Alive)
            {
                if (DateTime.Now > TeleportToMap4)
                {
                    if (!Map4.ContainMobID(66432))
                    {
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            Database.Server.AddMapMonster(stream, Map4, 66432, 143, 146, 18, 18, 1, 0, true, MsgFloorItem.MsgItemPacket.EffectMonsters.EarthquakeAndNight);
                        }
                    }
                    TeleportToMap4 = DateTime.Now.AddMinutes(9999);
                    foreach (var user in Database.Server.GamePoll.Values)
                    {
                        if (user.Player.Map == Map3.ID)
                        {
                            user.Teleport(151, 278, Map4.ID);

                            MsgSchedules.SendSysMesage("All players of Dis City Stage 3 have teleported to Stage 4!", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.white);
                        }
                    }
                }
                if (DateTime.Now > FinishTime)
                {
                    Console.WriteLine("DisCity finalizado. Teleportando jogadores de volta...");
                    foreach (var user in Database.Server.GamePoll.Values)
                    {
                        if (user.Player.Map == Map3.ID || user.Player.Map == Map4.ID || user.Player.Map == Map2.ID || user.Player.Map == Map1.ID)
                        {
                            user.Teleport(532, 485, 1020);
                            MsgSchedules.SendSysMesage("Dis City has ended. All players of Dis City have been teleported back to Ape City.", MsgServer.MsgMessage.ChatMode.Center, MsgServer.MsgMessage.MsgColor.white);
                        }
                    }
                    Mode = ProcesType.Dead; // Reinicia o modo para Dead
                }
            }
        }
        public void KillTheUltimatePluto(Client.GameClient client)
        {
            MsgSchedules.SendSysMesage("" + client.Player.Name + " has defeated Ultimate Pluto!", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.white);
        }
        public void RewardDarkHorn(Client.GameClient client, ServerSockets.Packet stream)
        {
            client.Inventory.Remove(790001, 1, stream);

            client.Inventory.Add(stream, Database.ItemType.DragonBallScroll, 1);

            client.Inventory.Add(stream, Database.ItemType.PowerExpBall, 1);

            client.Inventory.Add(stream, Database.ItemType.MoonBox, 1);

            MsgSchedules.SendSysMesage("" + client.Player.Name + " has claimed the reward of 1 DragonBall Scroll, 1 PowerEXPBall, and 1 MoonBox!", MsgServer.MsgMessage.ChatMode.System, MsgServer.MsgMessage.MsgColor.white);


        }
        public void TeleportMap1(ServerSockets.Packet stream, Client.GameClient client)
        {
            ushort x = 0;
            ushort y = 0;
            Map1.GetRandCoord(ref x, ref y);
            client.Teleport(x, y, Map1.ID);

            if (!RewardPlayers.Contains(client.Player.UID))
            {
                client.SendSysMesage("You've entered Dis City and have received 1 hour of Heavens Blessing, and an Exp Potion(B).");
                client.Inventory.Add(stream, 723017, 1, 0, 0, 0, Role.Flags.Gem.NoSocket, Role.Flags.Gem.NoSocket);
                client.Player.AddHeavenBlessing(stream,3);
                RewardPlayers.Add(client.Player.UID);
            }

        }
        public void TeleportToMap2(Client.GameClient client)
        {
            if (PlayersMap2 == 60)
            {
                client.SendSysMesage("I`ll send those who can`t enter Hell Gate back to Ape City.");
            }
            else
            {
                PlayersMap2 += 1;
                MsgSchedules.SendSysMesage("No." + PlayersMap2.ToString() + " Knight " + client.Player.Name + " " + ((client.Player.MyGuild != null) ? "of (" + client.Player.MyGuild.GuildName + ")".ToString() : "") + " has fought through Hell Gate and entered Hell Hall.", MsgServer.MsgMessage.ChatMode.TopLeftSystem, MsgServer.MsgMessage.MsgColor.white);
                client.Teleport(214, 336, Map2.ID);
                client.GainExpBall(3 * 600, true, Role.Flags.ExperienceEffect.angelwing);
                client.Player.KillersDisCity = 0;
            }
        }
        public void TeleportToMap3(Client.GameClient client)
        {
            PlayersMap3 += 1;

            MsgSchedules.SendSysMesage("No." + PlayersMap2.ToString() + " Knight " + client.Player.Name + " " + ((client.Player.MyGuild != null) ? "of (" + client.Player.MyGuild.GuildName + ")".ToString() : "") + "has entered the left flank of Hell Cloister!", MsgServer.MsgMessage.ChatMode.TopLeftSystem, MsgServer.MsgMessage.MsgColor.white);


            client.Teleport(300, 650, Map3.ID);
            client.GainExpBall(4 * 600, true, Role.Flags.ExperienceEffect.angelwing);
            client.Player.KillersDisCity = 0;
        }

        public int KillsMap2Records(byte _class)
        {
            if (_class >= 10 && _class <= 15 || _class >= 50 && _class <= 55)
                return 800;
            else if (_class >= 20 && _class <= 25)
                return 900;
            else if (_class >= 40 && _class <= 45)
                return 1300;
            else if (_class <= 135)
                return 600;
            else
                return 1000;
        }
    }
}
