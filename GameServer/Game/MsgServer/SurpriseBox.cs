using COServer.Client;
using System;
using System.Collections.Generic;

namespace COServer.Game.MsgServer
{
    public class SurpriseBox
    {
        static List<uint> VeryHigh2Socket = new List<uint>() // itens 2 socket.
        {
            113013, 114023, 117003, 118003, 120003, 121003, 130020, 131013, 133003, 134003, 141003, 142003, 150003, 152013, 160013, 410003, 420003, 421003

        };
        static List<uint> VeryHigh1Socket = new List<uint>() // itens 1 socket.
        {
            113013, 114023, 117003, 118003, 120003, 121003, 130020, 131013, 133003, 134003, 141003, 142003, 150003, 152013, 160013, 410003, 420003, 421003

        };        
        static List<uint> VeryHighPlus = new List<uint>() // Plusitens 
        {

 
            113013, 114023, 117003, 118003, 120003, 121003, 130020, 131013, 133003, 134003, 141003, 142003, 150003, 152013, 160013, 410003, 420003, 421003

        };
        static List<uint> High = new List<uint>() // itens variados raros//
        {
            700003, 700013, 700023, 700033, 700043, 700053, 700063, 700073, 723744, 723725, 730004, 721080, 720393, 723716, 
        };
        static List<uint> Mid = new List<uint>() // loss casa ganha
        {
            1080001, 1088000, 1088001, 1088002, 1060101, 752999, 752099, 752009, 752003, 725024, 725020, 723715, 723716,
            723712, 723711, 723700, 700001, 700011, 700021, 700031, 700041, 700051, 700061, 700071
        };



        public static void GetReward(GameClient client, ServerSockets.Packet stream)
        {
            uint reward;
            int chance = Role.Core.Random.Next(1, 101); // 1 a 100

            if (chance <= 3) // 3% de chance para item 2 socket
            {
                Role.Flags.Gem socktwo = Role.Flags.Gem.EmptySocket;
                Role.Flags.Gem sockone = Role.Flags.Gem.EmptySocket;

                reward = VeryHigh2Socket[Role.Core.Random.Next(0, VeryHigh2Socket.Count)];

                client.Inventory.Add(stream, reward, 1, 0, 0, 0, sockone, socktwo, false);
                client.SendSysMesage("You hit the jackpot! Check your inventory for an amazing reward!");
                Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage($"Congratulations [{client.Player.Name}] on winning a rare [ItemTwoSocket] in SurpriseBox!", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
                Program.DiscordAPISurpriseBox.Enqueue($"```diff\n+ 🎉 {client.Player.Name} Won a Rare Item!\n" +
                                                        $"Item: [ItemTwoSocket]\n" +
                                                        $"From: SurpriseBox🎁```");
            }
            else if (chance <= 8) // 5% para item 1 socket (3 a 8)
            {
                Role.Flags.Gem sockone = Role.Flags.Gem.EmptySocket;
                reward = VeryHigh1Socket[Role.Core.Random.Next(0, VeryHigh1Socket.Count)];

                client.Inventory.Add(stream, reward, 1, 0, 0, 0, sockone, 0, false);
                client.SendSysMesage("Incredible! You've received a rare reward! Check your inventory.");
                Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage($"Congratulations [{client.Player.Name}] on winning a rare [ItemOneSocket] in SurpriseBox!", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
                Program.DiscordAPISurpriseBox.Enqueue($"```diff\n+ 🎉 {client.Player.Name} Won a Rare Item!\n" +
                                            $"Item: [ItemOneSocket]\n" +
                                            $"From: SurpriseBox🎁```");
            }
            else if (chance <= 15) // 7% para item com + (9 a 15)
            {
                byte randomValue = Convert.ToByte(Role.Core.Random.Next(2, 5));
                reward = VeryHighPlus[Role.Core.Random.Next(0, VeryHighPlus.Count)];

                client.Inventory.Add(stream, reward, 1, randomValue, 0, 0, 0, 0, false);
                client.SendSysMesage("You've unlocked a powerful item! Check your inventory!");
                Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage($"Congratulations [{client.Player.Name}] on winning powerful [+{randomValue}PlusItem] in SurpriseBox!", Game.MsgServer.MsgMessage.MsgColor.white, Game.MsgServer.MsgMessage.ChatMode.System).GetArray(stream));
                Program.DiscordAPISurpriseBox.Enqueue($"```diff\n+ 💎 {client.Player.Name} Won a Powerful Item!\n" +
                                                        $"Item: [+{randomValue} PlusItem]\n" +
                                                        $"From: SurpriseBox🎁```");
            }
            else if (chance <= 30) // 15% para item raro (16 a 30)
            {
                reward = High[Role.Core.Random.Next(0, High.Count)];
                client.Inventory.Add(stream, reward, 1, 0, 0, 0, 0, 0, false);
                client.SendSysMesage("You got something special! Check your inventory!");
            }
            else // 70% restante
            {
                reward = Mid[Role.Core.Random.Next(0, Mid.Count)];
                client.Inventory.Add(stream, reward, 1, 0, 0, 0, 0, 0, false);
                client.SendSysMesage("🤖 You received a common item, but it’s still useful! Check your inventory.");
            }
        }
    }
}
