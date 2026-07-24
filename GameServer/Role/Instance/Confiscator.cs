using COServer.Game.MsgFloorItem;
using System;
using System.Collections.Concurrent;

namespace COServer.Role.Instance
{
    public class Confiscator
    {
        public static Counter CounterUID = new Counter(1);

        public ConcurrentDictionary<uint, Game.MsgServer.MsgDetainedItem> RedeemContainer;
        public ConcurrentDictionary<uint, Game.MsgServer.MsgDetainedItem> ClaimContainer;

        public Confiscator()
        {

            RedeemContainer = new ConcurrentDictionary<uint, Game.MsgServer.MsgDetainedItem>();
            ClaimContainer = new ConcurrentDictionary<uint, Game.MsgServer.MsgDetainedItem>();
        }

        public bool IsEmptyReadem() { return RedeemContainer.IsEmpty; }
        public bool IsEmptyClaim() { return ClaimContainer.IsEmpty; }


        internal unsafe void AddItem(Client.GameClient Owner, Client.GameClient Gainer, Game.MsgServer.MsgGameItem GameItem, ServerSockets.Packet stream)
        {
            Game.MsgServer.MsgDetainedItem DetainedItem = Game.MsgServer.MsgDetainedItem.Create(GameItem);
            DetainedItem.UID = CounterUID.Next;
            DetainedItem.OwnerUID = Owner.Player.UID;
            DetainedItem.OwnerName = Owner.Player.Name;
            DetainedItem.GainerUID = Gainer.Player.UID;
            DetainedItem.GainerName = Gainer.Player.Name;
            DetainedItem.ConquerPointsCost = CalculateCpsCost(DetainedItem);

            DateTime Timer = DateTime.Now;
            DetainedItem.Date = CreateTimer(Timer.Year, Timer.Month, Timer.Day);

            DetainedItem.Action = Game.MsgServer.MsgDetainedItem.ContainerType.DetainPage;
            DetainedItem.Send(Owner, stream);
            DetainedItem.Action = Game.MsgServer.MsgDetainedItem.ContainerType.ClaimPage;
            DetainedItem.Send(Gainer, stream);

            Owner.Confiscator.RedeemContainer.TryAdd(DetainedItem.UID, DetainedItem);
            Gainer.Confiscator.ClaimContainer.TryAdd(DetainedItem.UID, DetainedItem);


            //add Drop Effect
            ushort x = Owner.Player.X;
            ushort y = Owner.Player.Y;
            if (Owner.Map.AddGroundItem(ref x, ref y, 2))
            {
                Game.MsgFloorItem.MsgItemPacket DropEffect = Game.MsgFloorItem.MsgItemPacket.Create();
                DropEffect.DropType = Game.MsgFloorItem.MsgDropID.DropDetain;
                DropEffect.m_Color = (byte)GameItem.Color;
                DropEffect.m_ID = GameItem.ITEM_ID;
                DropEffect.m_UID = GameItem.UID;
                DropEffect.m_X = x;
                DropEffect.m_Y = y;

                Owner.Player.View.SendView(stream.ItemPacketCreate(DropEffect), true);
            }
            //-------------------

            Database.Server.QueueContainer.QueueObj(Owner.Player.UID, Owner.Confiscator);
            Database.Server.QueueContainer.QueueObj(Gainer.Player.UID, Gainer.Confiscator);
        }
        internal static int CreateTimer(int year, int month, int day)
        {
            int Timer = year * 10000 + month * 100 + day;
            return Timer;
        }
        internal static DateTime GetTimer(int Timer)
        {
            int Year = Timer / 10000;
            int Month = (Timer / 100) - Year * 100;
            int Day = Timer - (Year * 10000) - (Month * 100);
            return new DateTime(Year, Month, Day);
        }
        internal static int CalculateCpsCost(Game.MsgServer.MsgDetainedItem Item)
        {
            const byte Multiplier = 1;

            //Oficial conquer calculation --------------------------
            int amount = 10;
            if (Item.ItemID % 10 == 9)
                amount += 40;
            switch (Item.Plus)
            {
                case 0: break;
                case 1: amount += 1; break;
                case 2: amount += 2; break;
                case 3: amount += 5; break;
                case 4: amount += 15; break;
                case 5: amount += 30; break;
                case 6: amount += 90; break;
                case 7: amount += 270; break;
                case 8: amount += 600; break;
                default:
                    amount += 1200; break;

            }
            switch (Database.ItemType.ItemPosition(Item.ItemID))
            {
                case (ushort)Role.Flags.ConquerItem.LeftWeapon:
                case (ushort)Role.Flags.ConquerItem.RightWeapon:
                    {
                        if (Item.SocketOne != Flags.Gem.NoSocket && Item.SocketTwo != Flags.Gem.NoSocket)
                            amount += 100;
                        else if (Item.SocketOne != Flags.Gem.NoSocket)
                            amount += 10;
                        break;
                    }
                default:
                    {
                        if (Item.SocketOne != Flags.Gem.NoSocket && Item.SocketTwo != Flags.Gem.NoSocket)
                            amount += 500;
                        else if (Item.SocketOne != Flags.Gem.NoSocket)
                            amount += 150;
                        break;
                    }
            }

            //--------------------------------

            return amount * Multiplier;
        }
    }
}
