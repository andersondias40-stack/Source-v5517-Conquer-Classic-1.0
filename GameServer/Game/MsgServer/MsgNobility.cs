using System;
using System.Collections.Generic;
using System.Linq;
namespace COServer.Game.MsgServer
{
    public unsafe static partial class MsgBuilder
    {
        public static unsafe void GetNobility(this ServerSockets.Packet stream, out MsgNobility.NobilityAction mode, out ulong UID, out MsgNobility.DonationTyp donationtyp)
        {
            mode = (MsgNobility.NobilityAction)stream.ReadInt32();//4
            UID = stream.ReadUInt64();//8
           stream.SeekForward(4);
            donationtyp = (MsgNobility.DonationTyp)stream.ReadUInt32();

        }
        public static unsafe ServerSockets.Packet NobilityIconCreate(this ServerSockets.Packet stream, Role.Instance.Nobility nobility)
        {
            stream.InitWriter();
            string StrList = "" + nobility.UID + " " + nobility.Donation / 500000000 + " " + (byte)nobility.Rank + " " + nobility.Position + "";
            stream.Write((uint)MsgNobility.NobilityAction.Icon);//4
            stream.Write(nobility.UID);//8
            stream.ZeroFill(16);
            stream.Write(StrList);

            stream.Finalize(GamePackets.Nobility);

            return stream;
        }
    }

    public unsafe struct MsgNobility
    {
        public enum NobilityAction : uint
        {
            Donate = 1,
            RankListen = 2,
            Icon = 3,
            NobilityInformarion = 4,
        }
        public enum DonationTyp : byte
        {
            Money = 0,
            ConquerPoints = 1
        }


        [PacketAttribute(GamePackets.Nobility)]
        public unsafe static void HandlerNobility(Client.GameClient user, ServerSockets.Packet stream)
        {
            NobilityAction Action;
            ulong UID;
            DonationTyp donationtyp;
            stream.GetNobility(out Action, out UID, out donationtyp);

            switch (Action)
            {
                case NobilityAction.Donate:
                    {
                        if (!user.Player.OnMyOwnServer)
                            return;
                        if (user.InTrade)
                            return;

                        // Aceita somente doações em GOLD (Money)
                        if (donationtyp != DonationTyp.Money)
                            return;

                        // Bloqueia doações abaixo de 500 milhões
                        if (UID < 500000000)
                            return;

                        if (user.Player.Money >= (long)UID)
                        {
                            user.Player.Money -= (uint)UID;
                            user.Player.SendUpdate(stream, user.Player.Money, MsgUpdate.DataType.Money);
                            user.Player.Nobility.Donation += UID;
                            user.Send(stream.NobilityIconCreate(user.Player.Nobility));
                            Program.NobilityRanking.UpdateRank(user.Player.Nobility);
                        }

                        break;
                    }


                case NobilityAction.RankListen:
                    {
                        int displyPage = (int)UID;
                        var info = Program.NobilityRanking.GetArray();
                        try
                        {
                            const int max = 10;
                            int offset = displyPage * max;
                            int count = Math.Min(max, Math.Max(0, info.Length - offset));
                            var list = new List<string>();
                            stream.InitWriter();

                            stream.Write((uint)NobilityAction.RankListen);
                            stream.Write((ushort)displyPage);
                            int max_show = (int)Math.Ceiling((info.Length * 1.0) / max);
                            stream.Write((ushort)(max_show));
                            int count_show = 50;
                            if (info.Length < 50)
                            {
                                int current = info.Length / 10;
                                if (current == displyPage)
                                    count_show = current;
                            }
                            if (info.Length < 10)
                                count_show = info.Length;
                            else
                                count_show = info.Length - offset;
                            stream.Write((ushort)count_show);
                            stream.ZeroFill(13);

                            for (int x = 0; x < count; x++)
                            {
                                if (info.Length > offset + x)
                                {
                                    var element = info[offset + x];
                                    if (element.Position < 50)
                                    {
                                        var TheString = $"{element.UID} {(uint)element.Gender} 0 {element.Name} {element.Donation} {(uint)element.Rank} {element.Position}";
                                        TheString = (char)TheString.Length + TheString;
                                        list.Add(TheString);
                                    }
                                }
                            }

                            string theString = "";
                            for (int x = 0; x < list.Count; x++)
                            {
                                theString += list[x];
                            }
                            stream.Write(theString);
                            stream.Finalize(Game.GamePackets.Nobility);

                            user.Send(stream);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }

                        break;
                    }

                case NobilityAction.NobilityInformarion:
                    {
                        stream.InitWriter();

                        stream.Write((uint)NobilityAction.NobilityInformarion);

                        stream.Write(Program.NobilityRanking.KnightDonation);
                        stream.Write(uint.MaxValue);
                        stream.Write((uint)Role.Instance.Nobility.NobilityRank.Knight);

                        stream.Write(Program.NobilityRanking.KnightDonation);
                        stream.Write(uint.MaxValue);
                        stream.Write((uint)Role.Instance.Nobility.NobilityRank.Baron);

                        stream.Write(Program.NobilityRanking.EarlDonation);
                        stream.Write(uint.MaxValue);
                        stream.Write((uint)Role.Instance.Nobility.NobilityRank.Earl);

                        stream.Write(Program.NobilityRanking.DukeDonation);
                        stream.Write(uint.MaxValue);
                        stream.Write((uint)Role.Instance.Nobility.NobilityRank.Duke);

                        stream.Write(Program.NobilityRanking.PrinceDonation);
                        stream.Write(uint.MaxValue);
                        stream.Write((uint)Role.Instance.Nobility.NobilityRank.Prince);

                        stream.Write(Program.NobilityRanking.KingDonation);
                        stream.Write(uint.MaxValue);
                        stream.Write((uint)Role.Instance.Nobility.NobilityRank.King);

                        stream.Finalize(GamePackets.Nobility);

                        user.Send(stream);

                        break;
                    }
            }
        }

    }
}
