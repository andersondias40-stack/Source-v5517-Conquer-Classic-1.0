    using System;
    using System.Collections.Generic;
using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using COServer.Client;

    namespace COServer.EventsLib
    {
        public enum CTBTeam
        {
            Red,
            Blue
        }
        public enum BroadCastLoc
        {
            World,
            Map,
            Score,
            Title
        }
        public class BaseEvent
        {
            public uint map;
            public string name;
            public uint prize;
            public DateTime senton;
            public DateTime LastSpawn;
            public uint[] prizes;
            public string EventTitle = "Base Event";
            public EventStage Stage = EventStage.None;
            public Game.MsgServer.MsgStaticMessage.Messages IDMsg;
            public virtual void SendInvitation()
            {

                Stage = EventStage.None;
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    string msg = "";
                    //Program.DiscordAPI.Enqueue($"``{msg}``");
                    var stream = rec.GetStream();
                    Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage($"{name} has started, type /pvp to join.", "ALLUSERS", Game.MsgServer.MsgMessage.MsgColor.yellow, Game.MsgServer.MsgMessage.ChatMode.TopLeft).GetArray(stream));
               
                }
            }
            public enum EventStage
            {
                None,
                Inviting,
                Countdown,
                Starting,
                Fighting,
                Over
            }
            public void AddRandomPrize(Role.Player winner)
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    if (winner.Owner.Inventory.HaveSpace(1))
                    {
                        switch (EventManager.rnd.Next(0, 5))
                        {
                            case 0:
                                {
                                    winner.Owner.Inventory.Add(stream, 1088000, 1);
                                    winner.Owner.SendSysMesage($"You got a DB from {name}.");
                                    break;
                                }
                            case 1:
                                {
                                    winner.Owner.Inventory.Add(stream, 720027, 1);
                                    winner.Owner.SendSysMesage($"You got a MeteorScroll from {name}.");
                                    break;
                                }
                            case 2:
                                {
                                    winner.Owner.Inventory.Add(stream, 1080001, 1);
                                    winner.Owner.SendSysMesage($"You got a Emerald from {name}.");
                                    break;
                                }
                            default:
                            case 3:
                                {
                                    winner.Owner.Inventory.Add(stream, 723711, 1);
                                    winner.Owner.SendSysMesage($"You got a MeteorTear Pack from {name}.");
                                    break;
                                }
                            case 4:
                                {
                                    winner.Owner.Inventory.Add(stream, 721258, 1);
                                    winner.Owner.SendSysMesage($"You got a Cleanwater from {name}.");
                                    break;
                                }
                        }
                    }
                    else
                    {
                        winner.Money += 1000000;
                        winner.Owner.SendSysMesage($"You've received 1kk gold as a prize from {name}.");
                    }
                }
            }

            internal void AddPlayer(GameClient client)
            {
                throw new NotImplementedException();
            }

            public BaseEvent(uint map, string name, uint prize, Game.MsgServer.MsgStaticMessage.Messages Msg, uint[] prizes = null)
            {
                this.map = map;
                this.name = name;
                this.prize = prize;
                senton = DateTime.Now;
                this.IDMsg = Msg;
                if (prizes != null)
                    this.prizes = prizes;
            }
            public virtual void worker()
            {
                if (map == EventManager.teamfreezewar.map || map == EventManager.killhunted.map)
                    return;
            if (map == EventManager.freezewar.map)
            {
                if (DateTime.Now > senton.AddMinutes(10) || Database.Server.GamePoll
                        .Values
                        .Where(e => e.Player.Map == map && e.Player.Alive).Count() == 1)
                {
                    var winners = Database.Server.GamePoll
                        .Values
                        .Where(e => e.Player.Map == map && e.Player.Alive)
                        .OrderByDescending(e => e.FreezewarPoints)
                        .Take(1); // Apenas o primeiro colocado

                    foreach (var pl in winners)
                    {
                        pl.Player.RemoveFlag(Game.MsgServer.MsgUpdate.Flags.Freeze);
                        pl.Player.RemoveFlag(Game.MsgServer.MsgUpdate.Flags.Fly);
                        pl.Teleport(430, 346, 1002);

                        // Adiciona o prêmio ao primeiro colocado
                        pl.Player.ConquerPoints += prize; // Prêmio em Conquer Points

                        // Inicializa o stream para adicionar o item Tortoise
                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            pl.Inventory.Add(stream, 700071, 1); // Adiciona 1 item Tortoise ao inventário do primeiro colocado
                        }

                        pl.SendSysMesage("You got " + prize + " CPs!", (Game.MsgServer.MsgMessage.ChatMode)2005);
                        pl.SendSysMesage("You got an item [Tortoise]", (Game.MsgServer.MsgMessage.ChatMode)2005); // Mensagem de item

                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            Program.SendGlobalPackets.Enqueue(
                                new Game.MsgServer.MsgMessage(pl.Player.Name + " has won the " + name + "!", "ALLUSERS", "PVPEvents", Game.MsgServer.MsgMessage.MsgColor.white, (Game.MsgServer.MsgMessage.ChatMode)2000).GetArray(stream));
                            Program.DiscordAPIwinners.Enqueue("``[" + pl.Player.Name + "] has won the " + name + "You got an item [Tortoise] Normal!``");
                        }
                    }

                    // Teleporta todos os outros jogadores que estavam no evento
                    foreach (var pls in Database.Server.GamePoll.Values.Where(e => e.Player.Map == map))
                    {
                        pls.Teleport(430, 346, 1002);
                        pls.Player.RemoveFlag(Game.MsgServer.MsgUpdate.Flags.Freeze);
                        pls.Player.RemoveFlag(Game.MsgServer.MsgUpdate.Flags.Fly);
                    }
                }
                return;
            }



            if (map != EventManager.kingofthehill.map && map != EventManager.ctb.map && map != EventManager.guildsdm.map
    && map != EventManager.killthecaptain.map
    && map != EventManager.deathmatch.map && map != EventManager.dragonwar.map)
            {
                if (DateTime.Now > senton.AddMinutes(1) &&
                    Database.Server.GamePoll.Values.Where(e => e.Player.Map == map && e.Player.Alive).Count() == 1)
                {
                    var pl = Database.Server.GamePoll.Values.Where(e => e.Player.Map == map && e.Player.Alive).SingleOrDefault();

                    // Adiciona o prêmio em gold
                    pl.Player.Money += 20000;

                    // Inicializa o stream para adicionar o item Tortoise
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        pl.Inventory.Add(stream, 723792, 1); // Adiciona 1 item Tortoise ao inventário do primeiro colocado
                    }

                    // Envia mensagens de prêmio ao jogador
                    pl.SendSysMesage("You got 20,000 silver!", (Game.MsgServer.MsgMessage.ChatMode)2005);
                    pl.SendSysMesage("You got an item [Meteor Bag]", (Game.MsgServer.MsgMessage.ChatMode)2005); // Mensagem de item


                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        Program.DiscordAPIwinners.Enqueue("``" + pl.Player.Name + "`` has win the " + name + "! win 20,000 silver + [Meteor Bag]``");
                        Program.SendGlobalPackets.Enqueue(
                        new Game.MsgServer.MsgMessage(pl.Player.Name + " has win the " + name + "!", "ALLUSERS", "PVPEvents", Game.MsgServer.MsgMessage.MsgColor.white, (Game.MsgServer.MsgMessage.ChatMode)2000).GetArray(stream));
                        


                    }   
                   
                }
            }
            else
                {
                    if (Database.Server.GamePoll.Values.Where(e => e.Player.Map == map && e.KingOfTheHill >= 500).Count() >= 1
                        && map == EventManager.kingofthehill.map)
                    {
                        // Event ended
                        int c = 0;
                        foreach (var player in Database.Server.GamePoll.Values.Where(e => e.Player.Map == map))
                        {
                            if (c >= 5) break;
                            player.Teleport(430, 346, 1002);
                            player.Player.ConquerPoints += (uint)(prize / (c + 1));
                            string prize_string = " ";
                            if (prizes != null)
                                for (int i = 0; i < prizes.Length; i++)
                                    if (player.Inventory.HaveSpace(1))
                                    {
                                        using (var rec = new ServerSockets.RecycledPacket())
                                        {
                                            var stream = rec.GetStream();
                                            player.Inventory.Add(stream, prizes[i], 1);
                                        }
                                        Database.ItemType.DBItem dBItem;
                                        if(Database.Server.ItemsBase.TryGetValue(prizes[i],out dBItem))
                                        {
                                            player.SendSysMesage("You got an item [" + dBItem.Name + "]", (Game.MsgServer.MsgMessage.ChatMode)2005);
                                            prize_string += dBItem.Name;
                                        }                                   
                                    }
                            player.SendSysMesage("You got " + (uint)(prize / (c + 1)) + " CPs!", (Game.MsgServer.MsgMessage.ChatMode)2005);
                        }

                        foreach (var pl in Database.Server.GamePoll.Values.Where(e => e.Player.Map == map))
                            pl.Teleport(430, 346, 1002);
                    }
                    else if (DateTime.Now > senton.AddMinutes(3) && map == EventManager.guildsdm.map)
                    {
                        foreach (var player in Database.Server.GamePoll.Values.Where(e => e.Player.Map == map && (!e.Player.Alive || e.Player.MyGuild == null)))
                        {
                            player.Teleport(430, 346, 1002);
                            using (var rec = new ServerSockets.RecycledPacket())
                            {
                                var stream = rec.GetStream();
                                player.Player.Revive(stream);
                            }
                        }
                    }
                    else if (DateTime.Now > senton.AddMinutes(6) && map == EventManager.deathmatch.map)
                    {
                        int[] scores =
                        {
                            EventManager.deathmatch.WhiteTeam, EventManager.deathmatch.RedTeam,
                            EventManager.deathmatch.BlueTeam, EventManager.deathmatch.BlackTeam
                        };
                        int winner = scores.Max();
                        byte winning_team = 0;
                        if (winner == EventManager.deathmatch.WhiteTeam)
                            winning_team = 1;
                        else if (winner == EventManager.deathmatch.RedTeam)
                            winning_team = 2;
                        else if (winner == EventManager.deathmatch.BlueTeam)
                            winning_team = 3;
                        else if (winner == EventManager.deathmatch.BlackTeam)
                            winning_team = 4;
                        foreach (var player in Database.Server.GamePoll.Values.Where(e => e.Player.Map == map))
                        {
                            player.Teleport(430, 346, 1002);

                            if (player.DMTeam == winning_team)
                            {
                                uint final_prize = (uint)(prize * (player.DMScore / 2));
                                player.Player.ConquerPoints += final_prize;
                            }
                            player.DMScore = 0;
                            player.DMTeam = 0;
                        }

                        EventManager.deathmatch.WhiteTeam
                            = EventManager.deathmatch.BlueTeam
                                = EventManager.deathmatch.BlackTeam
                                    = EventManager.deathmatch.WhiteTeam = 0;
                   
                    }
                    else if (map != EventManager.ctb.map && map != EventManager.dragonwar.map)
                    {
                        foreach (var player in Database.Server.GamePoll.Values.Where(e => e.Player.Map == map))
                        {
                            if (!player.Player.Alive && DateTime.Now > player.DeathHit.AddSeconds(2))
                            {
                                using (var rec = new ServerSockets.RecycledPacket())    
                                {
                                    var stream = rec.GetStream();
                                    player.Player.Revive(stream);
                                }
                                if (player.Player.Map == EventManager.kingofthehill.map)
                                    player.Player.HitPoints = 2;
                            }
                        }
                    }
                }
            }
            public void Broadcast(string msg, BroadCastLoc loc, uint index = 0, uint Map = 0)
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    if (loc == BroadCastLoc.World)
                    {
                        Program.SendGlobalPackets.Enqueue(
                            new Game.MsgServer.MsgMessage(msg, "ALLUSERS", "[GM]", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                    }
                    else if (loc == BroadCastLoc.Map)
                    {
                        foreach (var C in Database.Server.GamePoll.Values.Where(e => e.Player.Map == map))
                            C.Send(new Game.MsgServer.MsgMessage(msg, "ALLUSERS", "[GM]", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.Center).GetArray(stream));
                    }
                }
            }
        }
    }
