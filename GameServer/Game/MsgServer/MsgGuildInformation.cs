using System.Linq;

namespace COServer.Game.MsgServer
{
    public unsafe static partial class MsgBuilder
    {
        public static unsafe ServerSockets.Packet GuildMmemberInformationCreate(this ServerSockets.Packet stream,
            Client.GameClient info)
        {
            stream.InitWriter();
            stream.Write((uint)info.Player.MyGuildMember.MoneyDonate);//4
            stream.Write((uint)info.Player.MyGuildMember.Rank);//8 donation
            stream.Write(info.Player.MyGuildMember.Name, 16);//32, 16
            stream.Finalize(1112);
            return stream;

        }

        [PacketAttribute(1112)]
        private static void Process(Client.GameClient user, ServerSockets.Packet stream)
        {
            if (user.Player.MyGuild != null)
            {
                var target = user.Player.MyGuild.Members.Values.FirstOrDefault(x => x.Name == user.Player.Name);
                if (target == null)
                {
                    Console.WriteLine("NULLED");
                    return;
                }
            }
            stream.GuildMmemberInformationCreate(user);

        }
        public static unsafe ServerSockets.Packet GuildInformationCreate(this ServerSockets.Packet stream, MsgGuildInformation info)
        {
            stream.InitWriter();
            stream.Write(info.GuildID);//4
            stream.Write((uint)info.Donation);//8 donation
            stream.Write((uint)info.SilverFund);//12
                                                //  stream.Write(info.ConquerPointFund);//20
            stream.Write(info.MembersCount);//16
            stream.Write((byte)info.MyRank);//17
            stream.Write(info.LeaderName, 16);//32, 16
            /*   stream.Write(1);//1?? //48
               stream.Write(0);//52
               stream.Write(0);//56
               stream.Write(info.Level);//60
               stream.ZeroFill(3);//64
               stream.Write(info.CreateTime);//67
               stream.ZeroFill(21);//71
               */
            stream.Finalize(GamePackets.Guild);
            return stream;

        }
    }
    public class MsgGuildInformation
    {
        public uint GuildID;
        public long SilverFund;
        public uint Donation;
        public uint MembersCount;
        public uint MyRank;
        public uint Level;//60
        public uint CreateTime;//67
        public string LeaderName;

        public static MsgGuildInformation Create()
        {
            MsgGuildInformation packet = new MsgGuildInformation();
            return packet;
        }
    }
}
