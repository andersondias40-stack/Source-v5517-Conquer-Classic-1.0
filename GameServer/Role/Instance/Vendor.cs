using COServer.Game.MsgServer;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace COServer.Role.Instance
{
    public class Vendor
    {
        public static Counter VendorCounter = new Counter(100000);
        public const byte MaxItems = 20;
        private static readonly TimeSpan VendingDuration = TimeSpan.FromHours(48); // Duração máxima de 24 horas
        private static readonly TimeSpan VisionUpdateInterval = TimeSpan.FromSeconds(2); // Atualiza visão a cada 2 segundos

        public class VendorItem
        {
            public Game.MsgServer.MsgItemView.ActionMode CostType;
            public Game.MsgServer.MsgGameItem DataItem;
            public uint AmountCost;
        }

        public Client.GameClient Owner;
        public ConcurrentDictionary<uint, VendorItem> Items;
        public Game.MsgServer.MsgMessage HalkMeesaje = null;
        public SobNpc VendorNpc;
        public uint VendorUID;
        public bool InVending;
        public bool OfflineVending; // Flag para indicar vending offline

        public Vendor(Client.GameClient client)
        {
            Items = new ConcurrentDictionary<uint, VendorItem>();
            Owner = client;
        }

        public unsafe void CreateVendor(ServerSockets.Packet stream)
        {
            if (InVending) return;

            VendorUID = VendorCounter.Next;

            VendorNpc = new SobNpc();
            VendorNpc.ObjType = MapObjectType.SobNpc;
            VendorNpc.OwnerVendor = Owner;
            VendorNpc.Name = Owner.Player.Name;
            VendorNpc.UID = VendorUID;
            VendorNpc.Mesh = SobNpc.StaticMesh.Vendor;
            VendorNpc.Type = Flags.NpcType.Booth;
            VendorNpc.Map = Owner.Player.Map;
            VendorNpc.X = (ushort)(Owner.Player.X + 1);
            VendorNpc.Y = Owner.Player.Y;

            Owner.Map.View.EnterMap<Role.IMapObj>(VendorNpc);

            foreach (var IObj in Owner.Player.View.Roles(MapObjectType.Player))
            {
                Role.Player screenObj = IObj as Role.Player;
                screenObj.View.CanAdd(VendorNpc, true, stream);
            }
            Owner.Player.Send(VendorNpc.GetArray(stream, false));
            InVending = true;

            // Inicia o processo offline automaticamente ao criar o vendor
            StartOfflineVending();
        }

        public unsafe void StopVending(ServerSockets.Packet stream)
        {
            if (!InVending) return;

            ActionQuery actione = new ActionQuery()
            {
                ObjId = VendorUID,
                Type = ActionType.RemoveEntity
            };
            Owner.Player.View.SendView(stream.ActionCreate(&actione), true);

            Items.Clear();
            Owner.Map.View.LeaveMap<Role.IMapObj>(VendorNpc);
            InVending = false;
            OfflineVending = false; // Para o vending offline
            Owner.MyVendor = null;

            // Remove o personagem do mapa ao parar o vending
            Owner.Map.View.LeaveMap<Role.IMapObj>(Owner.Player);
        }

        private void StartOfflineVending()
        {
            if (OfflineVending) return;

            OfflineVending = true;
            Thread vendingThread = new Thread(() => ProcessOfflineVending())
            {
                IsBackground = true
            };
            vendingThread.Start();
        }

        private void ProcessOfflineVending()
        {
            DateTime endTime = DateTime.Now.Add(VendingDuration);
            DateTime lastVisionUpdate = DateTime.Now;

            while (InVending && OfflineVending && DateTime.Now < endTime)
            {
                try
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();

                        // Atualiza a visão do personagem e do tapete no mapa
                        if (DateTime.Now - lastVisionUpdate >= VisionUpdateInterval)
                        {
                            Owner.Map.SendToRange(Owner.Player.GetArray(stream, false), Owner.Player.X, Owner.Player.Y);
                            Owner.Map.SendToRange(VendorNpc.GetArray(stream, false), VendorNpc.X, VendorNpc.Y);
                            lastVisionUpdate = DateTime.Now;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now}] Erro no vending offline para {Owner.Player.Name}: {ex.Message}");
                }

                Thread.Sleep(VisionUpdateInterval);
            }

            // Finaliza o vending quando o tempo acabar
            if (InVending && OfflineVending)
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    StopVending(rec.GetStream());
                }
            }
        }

        public bool AddItem(Game.MsgServer.MsgGameItem DataItem, Game.MsgServer.MsgItemView.ActionMode CostType, uint Amout)
        {
            if (DataItem.Bound == 1 || DataItem.Locked != 0 || DataItem.ITEM_ID == 750000)
                return false;

            if (Items.Count == MaxItems)
                return false;
            if (!Items.ContainsKey(DataItem.UID))
            {
                VendorItem VItem = new VendorItem();
                VItem.DataItem = DataItem;
                VItem.CostType = CostType;
                VItem.AmountCost = Amout;
                Items.TryAdd(DataItem.UID, VItem);

                string itemName = Database.Server.ItemsBase.GetItemName(DataItem.ITEM_ID);
                VendorDiscordNotifier.AddItem(Owner.Player.Name, itemName, Amout, DataItem.Plus);

                return true;
            }
            return false;
        }
    }
}