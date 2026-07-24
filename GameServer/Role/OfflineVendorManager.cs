using COServer.Game.MsgServer;
using COServer.Role.Instance;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace COServer.Role.Instance
{
    public class OfflineVendorManager
    {
        private class OfflineVendorState
        {
            public Client.GameClient Client { get; set; }
            public Vendor Vendor { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public bool Active { get; set; }
        }

        private static readonly ConcurrentDictionary<uint, OfflineVendorState> OfflineVendors = new ConcurrentDictionary<uint, OfflineVendorState>();
        private static readonly TimeSpan VendingDuration = TimeSpan.FromHours(24);
        private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(10); // Intervalo para verificar vendas

        public static void StartOfflineVending(Client.GameClient client)
        {
            if (client == null || client.MyVendor == null || !client.MyVendor.InVending) return;
            var state = new OfflineVendorState
            {
                Client = client,
                Vendor = client.MyVendor,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.Add(VendingDuration),
                Active = true
            };

            OfflineVendors.TryAdd(client.Player.UID, state);

            // Mantém o NPC visível no mapa
            Program.SendGlobalPackets.Enqueue(new MsgMessage(
                $"{client.Player.Name} está vendendo offline!",
                MsgMessage.MsgColor.white,
                MsgMessage.ChatMode.System).GetArray(new ServerSockets.Packet(ServerSockets.Packet.MAX_SIZE)));

            Task.Run(() => ProcessOfflineVending(state));
        }

        private static async Task ProcessOfflineVending(OfflineVendorState state)
        {
            while (state.Active && DateTime.Now < state.EndTime)
            {
                try
                {
                    // Aqui você pode verificar vendas reais ou simular compras para teste
                    await Task.Delay(CheckInterval);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now}] Erro na venda offline para {state.Client.Player.Name}: {ex.Message}");
                }
            }

            // Quando a venda offline termina, remove o NPC do mapa
            if (OfflineVendors.TryRemove(state.Client.Player.UID, out var removedState))
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    removedState.Vendor.StopVending(stream); // Remove o NPC do mapa
                }
            }
        }

        public static void StopOfflineVending(uint playerId)
        {
            if (OfflineVendors.TryRemove(playerId, out var state))
            {
                state.Active = false;
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    state.Vendor.StopVending(stream); // Remove o NPC do mapa
                }
            }
        }

        public static bool IsVendingOffline(uint playerId)
        {
            return OfflineVendors.ContainsKey(playerId);
        }
    }
}