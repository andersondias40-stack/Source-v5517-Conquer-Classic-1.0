using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using COServer.Game.MsgServer;
using COServer.Database;
using COServer.Game.MsgTournaments;

namespace COServer
{
    public class ProjectControl
    {
        #region Variable
        #region LotteryControl # CoMMando-Abdallah #
        public static double ElitePlus8Items, Super2SocItems, Super1SocItems, SuperNoSocItems, RateH5, RateH4, RateH3, RateH2;
        #endregion
        #region Server-Control # CoMMando-Abdallah #
        public static uint NormalDb_Drop, VipDb_Drop, Vip_Drop_Meteors, Normal_Drop_Meteors, Vip_Drop_Stone, Normal_Drop_Stone;
        public static byte Max_DragonBall, Max_Meteors, Max_Stone, Max_DragonBall_Vip, Max_Meteors_Vip, Max_Stone_Vip;
        #endregion
        #region Event Time  # CoMMando-Abdallah #
        public static byte NobilityMinute_On = 0;
        public static byte NobilitySec_On = 0;
        public static byte NobilityMinute_OFF = 0;
        #endregion
        #region Event Prize # CoMMando-Abdallah #
        public static uint SS_FBCps = 0;
        public static uint SpeedWarCps = 0;
        public static uint Top_SpecialCps = 0;
        public static uint KingCps = 0;
        public static uint PrinceCps = 0;
        public static uint DukeCps = 0;
        public static uint EarlCps = 0;
        #endregion
        #region Event Flags # CoMMando-Abdallah #
        public uint SS_FBTop = 0;
        public uint SpeedWarTop = 0;
        public uint Top_SpecialTop = 0;
        public uint KingTop = 0;
        public uint PrinceTop = 0;
        public uint DukeTop = 0;
        public uint EarlTop = 0;
        #endregion
        #endregion
        #region Event-Time # CoMMando-Abdallah #
        public static void EventTime()
        {
            MySqlCommand cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("NobilityPk").Where("Owner", "AbdallahKhalel");
            MySqlReader r = new MySqlReader(cmd);
            if (r.Read())
            {
                //Time On & Off
                NobilityMinute_On = r.ReadByte("NobilityMinute_On");
                NobilitySec_On = r.ReadByte("NobilitySec_On");
                NobilityMinute_OFF = r.ReadByte("NobilityMinute_OFF");

            }
            Console.WriteLine("Event-Time Loaded");
            r.Close();
            r.Dispose();
        }
        #endregion
        #region Event-Prize # CoMMando-Abdallah #
        public static void EventPrize()
        {
            MySqlCommand cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("EventPrize").Where("Owner", "AbdallahKhalel");
            MySqlReader r = new MySqlReader(cmd);
            if (r.Read())
            {
                SS_FBCps = r.ReadUInt32("SS_FBCps");
                SpeedWarCps = r.ReadUInt32("SpeedWarCps");
                Top_SpecialCps = r.ReadUInt32("Top_SpecialCps");
                KingCps = r.ReadUInt32("KingCps");
                PrinceCps = r.ReadUInt32("PrinceCps");
                DukeCps = r.ReadUInt32("DukeCps");
                EarlCps = r.ReadUInt32("EarlCps");
            }
            Console.WriteLine("Event-Prize Loaded");
            r.Close();
            r.Dispose();
        }
        #endregion
        #region Server-Control # CoMMando-Abdallah #
        public static void ServerControl()
        {
            MySqlCommand cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("servercontrol");
            MySqlReader r = new MySqlReader(cmd);
            if (r.Read())
            {
                NormalDb_Drop = r.ReadUInt32("NormalDb_Drop");
                VipDb_Drop = r.ReadUInt32("VipDb_Drop");
                Vip_Drop_Meteors = r.ReadUInt32("Vip_Drop_Meteors");
                Normal_Drop_Meteors = r.ReadUInt32("Normal_Drop_Meteors");
                Vip_Drop_Stone = r.ReadUInt32("Vip_Drop_Stone");
                Normal_Drop_Stone = r.ReadUInt32("Normal_Drop_Stone");
                Max_DragonBall = r.ReadByte("Max_DragonBall_Normal");
                Max_Meteors = r.ReadByte("Max_Meteors_Normal");
                Max_Stone = r.ReadByte("Max_Stone_Normal");
                Max_DragonBall_Vip = r.ReadByte("Max_DragonBall_Vip");
                Max_Meteors_Vip = r.ReadByte("Max_Meteors_Vip");
                Max_Stone_Vip = r.ReadByte("Max_Stone_Vip");
            }
            Console.WriteLine("Server-Control Loaded");
            r.Close();
            r.Dispose();
        }
        #endregion
        #region Lottery-Control # CoMMando-Abdallah #
        public static void LotteryControl()
        {
            MySqlCommand cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("lotterycontrol").Where("Owner", "AbdallahKhalel");
            MySqlReader r = new MySqlReader(cmd);
            if (r.Read())
            {
                ElitePlus8Items = r.ReadDouble("ElitePlus8Items");
                Super2SocItems = r.ReadDouble("Super2SocItems");
                Super1SocItems = r.ReadDouble("Super1SocItems");
                SuperNoSocItems = r.ReadDouble("SuperNoSocItems");
                RateH5 = r.ReadDouble("RateH5");
                RateH4 = r.ReadDouble("RateH4");
                RateH3 = r.ReadDouble("RateH3");
                RateH2 = r.ReadDouble("RateH2");
            }
            Console.WriteLine("Lottery-Control Loaded");
            r.Close();
            r.Dispose();
        }
        #endregion
        #region Load / Update [Flags] # CoMmando-Abdallah #
        public static void LoadFlags()
        {
            MySqlCommand cmd = new MySqlCommand(MySqlCommandType.SELECT).Select("EventFlags").Where("Owner", "AbdallahKhalel");
            MySqlReader r = new MySqlReader(cmd);
            if (r.Read())
            {
                MsgSchedules.PlayerTop.SS_FBTop = r.ReadUInt32("SS_FBTop");
                MsgSchedules.PlayerTop.SpeedWarTop = r.ReadUInt32("SpeedWarTop");
                MsgSchedules.PlayerTop.Top_SpecialTop = r.ReadUInt32("SpecialTop");
                MsgSchedules.PlayerTop.KingTop = r.ReadUInt32("KingTop");
                MsgSchedules.PlayerTop.PrinceTop = r.ReadUInt32("PrinceTop");
                MsgSchedules.PlayerTop.DukeTop = r.ReadUInt32("DukeTop");
                MsgSchedules.PlayerTop.EarlTop = r.ReadUInt32("EarlTop");
            }
            Console.WriteLine("Event-Flags Loaded");
            r.Close();
            r.Dispose();
        }
        public static void UpdateFlags()
        {
            MySqlCommand cmd = new MySqlCommand(MySqlCommandType.UPDATE);
            cmd.Update("EventFlags")
                .Set("SS_FBTop", MsgSchedules.PlayerTop.SS_FBTop)
                .Set("SpeedWarTop", MsgSchedules.PlayerTop.SpeedWarTop)
                .Set("SpecialTop", MsgSchedules.PlayerTop.Top_SpecialTop)
                .Set("KingTop", MsgSchedules.PlayerTop.KingTop)
                .Set("PrinceTop", MsgSchedules.PlayerTop.PrinceTop)
                .Set("DukeTop", MsgSchedules.PlayerTop.DukeTop)
                .Set("EarlTop", MsgSchedules.PlayerTop.EarlTop)
                .Where("Owner", "AbdallahKhalel")
                .Execute();
        }
        #endregion

    }
}
