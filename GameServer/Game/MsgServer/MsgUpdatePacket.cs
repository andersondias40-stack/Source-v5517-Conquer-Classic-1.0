using System;

namespace COServer.Game.MsgServer
{
    public unsafe static partial class MsgBuilder
    {

        public static void GetUpdatePacket(this ServerSockets.Packet stream, out MsgUpdate.DataType ID, out ulong Value)
        {
            stream.SeekForward(sizeof(uint));
            uint uid = stream.ReadUInt32();
            uint count = stream.ReadUInt32();
            ID = (MsgUpdate.DataType)stream.ReadUInt32();
            Value = stream.ReadUInt64();

        }
    }


    public unsafe class MsgUpdate
    {
        public class OnlineTraining
        {
            public const byte
            Show = 0,
            InTraining = 1,
            Review = 2,
            IncreasePoints = 3,
            ReceiveExperience = 4,
            Remove = 5;
        }
        public class CreditGifts
        {
            public const byte
                Show = 0,
                CanClaim = 1,
                Claim = 5,
                ShowSpecialItems = 6;
        }
        [Flags]
        public enum Flags : int
        {
            Normal = 3,//0x0,
            FlashingName = 0,
            Poisoned = 1,
            Invisible = 2,
            XPList = 4,
            Dead = 5,
            TeamLeader = 6,
            StarOfAccuracy = 7,
            MagicShield = 8,
            Shield = 8,
            Stigma = 9,
            Ghost = 10,
            FadeAway = 11,
            RedName = 14,
            BlackName = 15,
            ReflectMelee = 17,
            Superman = 18,
            Ball = 19,
            Ball2 = 20,
            Invisibility = 22,
            Cyclone = 23,
            Dodge = 26,
            Fly = 27,
            Intensify = 28,
            CastPray = 30,
            Praying = 31,
            Cursed = 32,
            HeavenBlessing = 33,
            TopGuildLeader = 34,
            TopDeputyLeader = 35,
            MonthlyPKChampion = 36,
            WeeklyPKChampion = 37,
            TopWarrior = 38,
            TopTrojan = 39,
            TopArcher = 40,
            TopWaterTaoist = 41,
            TopFireTaoist = 42,
            TopNinja = 43,
            DragonWar = 45,
            ShurikenVortex = 46,
            FatalStrike = 47,
            Flashy = 48,
            Ride = 50,
            TopSpouse = 51,
            Accelerated = 52,
            Deceleration = 53,
            Frightened = 54,
            HeavenSparkle = 55,
            IncMoveSpeed = 56,
            GodlyShield = 57,
            Dizzy = 58,
            Freeze = 59,
            Confused = 60,
            Top8Weekly = 63,
            Top4Weekly = 64,
            Top2Weekly = 65,
            ChaintBolt = 92,
            AzureShield = 93,
            ScurvyBomb = 96,//that is use for abuse.
            TyrantAura = 98,
            FeandAura = 100,
            MetalAura = 102,
            WoodAura = 104,
            WaterAura = 106,
            FireAura = 108,
            EartAura = 110,
            SoulShackle = 111,
            Oblivion = 112,
            ShieldBlock = 113,
            TopMonk = 114,
            TopPirate = 122,
            CTF_Flag = 118,
            PoisonStar = 119,
            CannonBarrage = 120,
            BlackbeardsRage = 121,
            DefensiveStance = 126,
            MagicDefender = 128,
            viptitle = 150,

        }
        [Flags]
        public enum DataType : uint
        {
            Hitpoints = 0,
            MaxHitpoints = 1,
            Mana = 2,
            MaxMana = 3,
            Money = 4,
            Experience = 5,
            PKPoints = 6,
            Class = 7,
            Stamina = 8,
            WHMoney = 9,
            Atributes = 10,
            Mesh = 11,
            Level = 12,
            Spirit = 13,
            Vitality = 14,
            Strength = 15,
            Agility = 16,
            HeavensBlessing = 17,
            DoubleExpTimer = 18,
            CursedTimer = 20,
            Reborn = 22,
            VirtutePoints = 23,
            StatusFlag = 25,
            HairStyle = 26,
            XPCircle = 27,
            LuckyTimeTimer = 28,
            ConquerPoints = 29,
            OnlineTraining = 31,
            ExtraBattlePower = 36,
            Merchant = 38,
            VIPLevel = 39,
            QuizPoints = 40,
            EnlightPoints = 41,

            ClanShareBp = 42,
            GuildBattlePower = 44,
            BoundConquerPoints = 45,
            RaceShopPoints = 47,
            Contestant = 48,
            AzureShield = 49,
            FirsRebornClass = 51,
            SecondRebornClass = 50,
            Team = 52,
            SoulShackle = 54,
            Fatigue = 55,
            DefensiveStance = 56,
        }
        public unsafe MsgUpdate(ServerSockets.Packet stream, uint UID, uint count = 1)
        {
            stream.InitWriter();
            // stream.Write(Time32.Now.Value);
            stream.Write(UID);//4
            stream.Write(count);//8
                                // Console.WriteLine("Update Count + " + stream.Position);
        }
        public ServerSockets.Packet Append(ServerSockets.Packet stream, DataType ID, long Value)
        {
            stream.Write((uint)ID);
            stream.Write(Value);//16
                                // stream.Write(0ul);//0x01
                                //   Console.WriteLine("Update Value + " + stream.Position);

            return stream;
        }
        public ServerSockets.Packet Append(ServerSockets.Packet stream, DataType ID, uint Flag, uint Time, uint Dmg, uint Level)
        {
            stream.Write((uint)ID);
            stream.Write(Flag);
            stream.Write(Time);
            stream.Write(Dmg);
            stream.Write(Level);
            stream.Write(0ul);
            stream.Write(0u);
            stream.Write(0);
            //  Console.WriteLine("Update Dmg + " + stream.Position);

            return stream;
        }
        public ServerSockets.Packet Append(ServerSockets.Packet stream, DataType ID, uint[] Value)
        {
            stream.Write((uint)ID);

            if (Value != null)
            {
                for (int x = 0; x < 4; x++)//4
                {
                    if (Value.Length > x)
                    {
                        stream.Write(Value[x]);//16
                    }
                    else
                        stream.Write(0u);
                }
            }
            return stream;
        }
        public ServerSockets.Packet GetArray(ServerSockets.Packet stream)
        {
            stream.Finalize(GamePackets.Update);
            return stream;
        }
    }
}
