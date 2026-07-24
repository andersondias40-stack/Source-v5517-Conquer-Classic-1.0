namespace COServer.Game.MsgServer
{
    public unsafe static class MsgTrade
    {
        public enum TradeID : uint
        {
            RequestNewTrade = 0x01,
            RequestCloseTrade = 0x02,
            RequestAddItemToTrade = 0x06,
            RequestAddMoneyToTrade = 0x07,
            RequestAddConquerPointsToTrade = 0x0D,
            RequestCompleteTrade = 0x0A,
            RemoveItem = 11,
            ShowTradeWindow = 0x03,
            CloseTradeWindow = 0x05,
            DisplayMoney = 0x08,
            DisplayConquerPoints = 0x0C
        }

        public static unsafe void GetTrade(this ServerSockets.Packet stream, out ulong dwParam, out TradeID ID)
        {
            dwParam = stream.ReadUInt32();
            ID = (TradeID)stream.ReadUInt16();
        }

        public static unsafe ServerSockets.Packet TradeCreate(this ServerSockets.Packet stream, ulong dwParam, TradeID ID)
        {
            stream.InitWriter();

            stream.Write((uint)dwParam);//4

            stream.Write((ushort)ID);//8


            stream.Finalize(GamePackets.Trade);
            return stream;
        }

        [PacketAttribute(GamePackets.Trade)]
        private static void HandlerTrade(Client.GameClient user, ServerSockets.Packet stream)
        {
            ulong dwParam;
            TradeID ID;
            stream.GetTrade(out dwParam, out ID);

            switch (ID)
            {

                case TradeID.RequestNewTrade:
                    {
                        if (user.MyTrade == null)
                            user.MyTrade = new Role.Instance.Trade(user);


                        Role.IMapObj obj;
                        if (user.Player.View.TryGetValue((uint)dwParam, out obj, Role.MapObjectType.Player))
                        {
                            Client.GameClient Partner = (obj as Role.Player).Owner;
                            if (Partner != null)
                            {
                                if (!Partner.InTrade && !user.InTrade)
                                {
                                    user.Player.targetTrade = (uint)Partner.Player.UID;
                                    user.MyTrade.Target = Partner;
                                    if (Partner.Player.targetTrade == user.Player.UID && user.Player.targetTrade == Partner.Player.UID)
                                    {
                                        if (user.MyTrade.Target.Player.UID == Partner.Player.UID && Partner.MyTrade.Target.Player.UID == user.Player.UID)
                                        {
                                            user.MyTrade.Target.MyTrade = new Role.Instance.Trade(user.MyTrade.Target);
                                            user.MyTrade.Target.MyTrade.Target = user;
                                            Partner.MyTrade.Target = user;
                                            Partner.MyTrade.WindowOpen = true;
                                            user.MyTrade.WindowOpen = true;
                                            Partner.MyTrade.Confirmed = false;
                                            user.MyTrade.Confirmed = false;
                                            user.Send(stream.TradeCreate(dwParam, TradeID.ShowTradeWindow));
                                            Partner.Send(stream.TradeCreate(user.Player.UID, TradeID.ShowTradeWindow));
                                        }
                                        else
                                        {
                                            user.SendSysMesage("Player is already in a trade.");

                                        }
                                    }
                                    else
                                    {

                                        Partner.Send(stream.TradeCreate(user.Player.UID, TradeID.RequestNewTrade));

                                    }
                                }
                                else
                                {
                                    user.SendSysMesage("Player is already in a trade.");

                                }
                            }
                        }
                        break;
                    }
                case TradeID.RequestCloseTrade:
                    {
                        if (user.InTrade)
                        {
                            user.MyTrade.CloseTrade();
                        }
                        break;
                    }
                case TradeID.RequestAddItemToTrade:
                    {
                        if (user.InTrade)
                        {
                            Game.MsgServer.MsgGameItem DataItem;
                            if (user.Inventory.TryGetItem((uint)dwParam, out DataItem))
                                user.MyTrade.AddItem(stream, (uint)dwParam, DataItem);
                        }
                        break;
                    }
                case TradeID.RequestAddConquerPointsToTrade:
                    {
                        if (user.InTrade)
                            user.MyTrade.AddConquerPoints((uint)dwParam, stream);
                        break;
                    }
                case TradeID.RequestAddMoneyToTrade:
                    {
                        if (user.InTrade)
                            user.MyTrade.AddMoney((uint)dwParam, stream);
                        break;
                    }
                case TradeID.RequestCompleteTrade:
                    {
                        try
                        {
                            if (user.MyTrade.Target.MyTrade.Target.Player.UID != user.Player.UID)
                            {
                                user.Send(stream.TradeCreate(dwParam, TradeID.CloseTradeWindow));
                                user.MyTrade = null;
                                break;
                            }
                        }
                        catch
                        {
                            user.Send(stream.TradeCreate(dwParam, TradeID.CloseTradeWindow));
                            user.MyTrade = null;
                            break;
                        }
                        if (user.InTrade)
                        {
                            if (user.MyTrade.Target.InTrade)
                            {
                                user.MyTrade.Confirmed = true;
                                if (!user.MyTrade.Target.MyTrade.Confirmed)
                                {

                                    user.MyTrade.Target.Send(stream.TradeCreate(dwParam, TradeID.RequestCompleteTrade));
                                }
                                else
                                {
                                    user.Player.targetTrade = 0;
                                    user.MyTrade.Target.Player.targetTrade = 0;


                                    user.Send(stream.TradeCreate(dwParam, TradeID.CloseTradeWindow));
                                    user.MyTrade.Target.Send(stream.TradeCreate(dwParam, TradeID.CloseTradeWindow));


                                    bool Acceped = false;
                                    if (user.Inventory.HaveSpace((byte)user.MyTrade.Target.MyTrade.Items.Count))
                                    {
                                        if (user.MyTrade.Target.MyTrade.ValidItems())
                                        {
                                            if (user.MyTrade.Target.Inventory.HaveSpace((byte)user.MyTrade.Items.Count))
                                            {
                                                if (user.MyTrade.ValidItems())
                                                {

                                                    user.Player.ConquerPoints += user.MyTrade.Target.MyTrade.ConquerPoints;
                                                    user.Player.Money += user.MyTrade.Target.MyTrade.Money;
                                                    user.Player.SendUpdate(stream, user.Player.Money, MsgUpdate.DataType.Money);

                                                    user.MyTrade.Target.Player.ConquerPoints += user.MyTrade.ConquerPoints;
                                                    user.MyTrade.Target.Player.Money += user.MyTrade.Money;
                                                    user.MyTrade.Target.Player.SendUpdate(stream, user.MyTrade.Target.Player.Money, MsgUpdate.DataType.Money);

                                                    foreach (var item in user.MyTrade.Target.MyTrade.Items.Values)
                                                    {
                                                        user.Inventory.Update(item, Role.Instance.AddMode.MOVE, stream);
                                                        user.MyTrade.Target.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream, true);
                                                    }
                                                    foreach (var item in user.MyTrade.Items.Values)
                                                    {
                                                        user.MyTrade.Target.Inventory.Update(item, Role.Instance.AddMode.MOVE, stream);
                                                        user.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream, true);
                                                    }
                                                }
                                                Acceped = true;
                                            }
                                        }
                                    }
                                    if (!Acceped)
                                    {
                                        user.SendSysMesage("There was an error with the trade", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
                                        user.MyTrade.Target.SendSysMesage("There was an error with the trade", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
                                    }
                                    user.MyTrade.Target.MyTrade = null;
                                    user.MyTrade = null;

                                }
                            }
                        }
                        break;
                    }

            }
        }
    }
}
