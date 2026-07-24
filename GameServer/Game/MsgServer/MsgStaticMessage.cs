using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COServer.Game.MsgServer
{
    public unsafe static partial class MsgBuilder
    {
        public static unsafe void GetStaticMessage(this ServerSockets.Packet stream, out uint Accept)
        {
            Accept = stream.ReadUInt32();
        }

        public static unsafe ServerSockets.Packet StaticMessageCreate(this ServerSockets.Packet stream, MsgStaticMessage.Messages Message
            , MsgStaticMessage.Action Mode, uint Secounds)
        {
            stream.InitWriter();

            stream.Write((uint)0);
            stream.Write((uint)Message);
            stream.Write((uint)Mode);
            stream.Write(Secounds);


            stream.Finalize(GamePackets.MsgStaticMessage);

            return stream;
        }


    }

    public unsafe struct MsgStaticMessage
    {
        public enum Messages : uint
        {
            None = 0,
            GuildWar = 10515,//Guild War is about to begin! Will you join it?
            
            //TDM = 10525,
            //DBShower = 10526,            
            Tthief = 10528,
            //KingHell = 10529,
            //KillTheCapt = 10532,
            ////CityWars = 10533,
            //EliteGW = 10534,
            discity = 10535,
            //Bosses = 10537,
            //SnowBanshee = 10541,

            TeratoDragon = 10542,
            //ThrillingSpook = 10543,
            //Capricorn = 10544,
            //Raikou = 10545,
            //SwordMaster = 10546,
            //NemesisTyrant = 10547,
            //LavaBeast = 10548,
            CaptureTheBag = 10561,
            DeathMatch = 10562,
            DragonKing = 10563,

            Fiveout = 647800,
            //FirstKiller = 223657,
            //GenderWar = 423657, 
            LastMan = 323657,
            //LuckyBox = 21565,
            //DonationWar = 19557,
            //Ss_Fb = 13557,
            //TopBlack = 20557,
            //TopConquer = 123657, 
            //EliteGuildWar = 119092,
            //FirePoleWar = 9471045,
            //UniquePK = 975460,
            //ExtremeFlagWar = 15561400,
            FreezeWar = 10565,
            GuildsDeathMatch = 10566,
            KillTheCaptain = 10567,
            KillTheFugitive = 10568,
            KingOfTheHill = 10569,
            //LastmanStanding = 10570,
            //PassTheBomb = 10571,
            TeamFreezewar = 10572,
            PKDeathMatch = 12310231,
            ClassPk = 12310232,
            GuildSurvival = 12310233

        }
        public enum Action : uint
        {
            Append = 6
        }
        [PacketAttribute(GamePackets.MsgStaticMessage)]
        public static void Handler(Client.GameClient user, ServerSockets.Packet stream)
        {
            uint Accept;
            stream.GetStaticMessage(out Accept);

            if (Accept == 1)
            {
                if (Program.BlockTeleportMap.Contains(user.Player.Map))
                    return;
                if (user.Player.StartMessageBox > Time32.Now)
                {
                    if (user.Player.MessageOK != null)
                        user.Player.MessageOK.Invoke(user);
                    else if (user.Player.MessageCancel != null)
                        user.Player.MessageCancel.Invoke(user);
                }
                user.Player.MessageOK = null;
                user.Player.MessageCancel = null;
            }
        }
    }
}
