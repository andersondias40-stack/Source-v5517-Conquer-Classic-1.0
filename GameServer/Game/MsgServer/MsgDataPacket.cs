using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;


namespace COServer.Game.MsgServer
{
    using ActionInvoker = CachedAttributeInvocation<ProcessAction, MsgDataPacket.DataAttribute, ActionType>;
    public unsafe delegate void ProcessAction(Client.GameClient user, ServerSockets.Packet msg, ActionQuery* data);

    public class CustomCommands
    {
        public const ushort
            ExitQuestion = 1,
            Minimize = 2,
            ShowReviveButton = 1053,
            FlowerPointer = 1067,
            Enchant = 1091,
            LoginScreen = 1153,
            SelectRecipiet = 30,
            JoinGuild = 34,
            MakeFriend = 38,
            ChatWhisper = 40,
            CloseClient = 43,
            HotKey = 53,
            Furniture = 54,
            TQForum = 79,
            PathFind = 97,
            LockItem = 102,
            ShowRevive = 1053,
            HideRevive = 1054,
            StatueMaker = 1066,
            GambleOpen = 1077,
            GambleClose = 1078,
            Compose = 1086,
            Craft1 = 1088,
            Craft2 = 1089,
            Warehouse = 1090,
            ShoppingMallShow = 1100,
            ShoppingMallHide = 1101,
            NoOfflineTraining = 1117,
            Interact = 1122,
            CenterClient = 1155,
            ClaimCP = 1197,
            ClaimAmount = 1198,
            MerchantApply = 1201,
            MerchantDone = 1202,
            RedeemEquipment = 1233,
            ClaimPrize = 1234,
            RepairAll = 1239,
            FlowerIcon = 1244,
            SendFlower = 1246,//47+48
            ReciveFlower = 1248,
            WarehouseVIP = 1272,
            UseExpBall = 1288,
            HackProtection = 1298,
            HideGUI = 1307,
            Inscribe = 3059,
            BuyPrayStone = 3069,
            HonorStore = 3104,
            Opponent = 3107,
            CountDownQualifier = 3109,
            QualifierStart = 3111,
            ItemsReturnedShow = 3117,
            ItemsReturnedWindow = 3118,
            ItemsReturnedHide = 3119,
            QuestFinished = 3147,
            QuestPoint = 3148,
            QuestPointSparkle = 3164,
            StudyPointsUp = 3192,
            Updates = 3218,
            IncreaseLineage = 3227,
            HorseRacingStore = 3245,
            GuildPKTourny = 3249,
            QuitPK = 3251,
            Spectators = 3252,
            CardPlayOpen = 3270,
            CardPlayClost = 3271,
            ArtifactPurification = 3344,
            SafeguardConvoyShow = 3389,
            SafeguardConvoyHide = 3390,
            RefineryStabilization = 3392,
            ArtifactStabilization = 3398,
            SmallChat = 3406,
            NormalChat = 3407,
            Reincarnation = 3439;
    }
    public class DialogCommands
    {
        public const ushort
            Compose = 1,
            Craft = 2,
            Warehouse = 4,
            DetainRedeem = 336,
            DetainClaim = 337,
            VIPWarehouse = 341,
            Breeding = 368,
            PurificationWindow = 455,
            StabilizationRifinery = 448,
            StabilizationWindow = 459,
            TalismanUpgrade = 347,
            GemComposing = 422,
            OpenSockets = 425,
            Blessing = 426,
            TortoiseGemComposing = 438,
            HorseRacingStore = 464,
            Reincarnation = 485,
            ChangeName = 489,
            GarmentShop = 502,
            DegradeEquip = 506,
            Browse = 572,
            JiangHuSetName = 617,
            SendTwinCityTime = 538,
            BrowseAuction = 572,
            HowGetStudy = 595,
            TheFactionWar = 599, //-> 1072 packet
            ResetSecundaryPassword = 639,
            NewLottery = 656, // Packet -> 2804
            CreateUnion = 693,
            SetKingdomTitle = 700,
            ChangeNameUnion = 723;

    }
    public enum ActionType : ushort
    {
        RemoveTrap = 434,
        UpdateSpell = 252,
        UpdateProf = 253,
        DragonBall = 165,
        OpenGuiNpc = 160,
        AutoPatcher = 162,
        CountDown = 159,
        ChangeLookface = 151,
        SetLocation = 74,
        Hotkeys = 75,
        ConfirmAssociates = 76,
        ConfirmProficiencies = 77,
        ConfirmSpells = 78,
        ChangeDirection = 79,
        ChangeStance = 81,
        ChangeMap = 85,
        Mining = 99,
        Teleport = 86,
        Leveled = 92,
        Revive = 94,
        DeleteCharacter = 95,
        SetPkMode = 96,
        ConfirmGuild = 97,
        LocationTeamLieder = 101,
        RequestEntity = 102,
        SetMapColor = 104,
        TeamSearchForMember = 106,
        RemoveSpell = 109,
        StartVendor = 111,
        StopVending = 114,
        OpenCustom = 116,
        ViewEquipment = 117,
        EndTransformation = 118,
        EndFly = 120,
        ViewEnemyInfo = 123,
        OpenDialog = 126,
        CompleteLogin = 132,//132 130
        ReviveMonster = 134,
        RemoveEntity = 135,//135
        Jump = 137,
        Ghost = 145,
        ViewFriendInfo = 148,
        ChangeFace = 151,
        TradePartnerInfo = 152,
        AbortMagic = 163,
        Bulletin = 166,

        PokerTeleporter = 167,
        FlashStep = 156,
        Away = 161,
        Pick = 164,
        ClikerON = 171,
        ClikerEntry = 172,
        SetAppearanceType = 178,
        AllowAnimation = 130,
        CreditGifts = 255,
        UpdateInventorySash = 256,
        Dialog = 186,
        QuerySpawn = 310,
        QueryEquipment = 408,
        BeginSteedRace = 401,
        FinishSteedRace = 402,
        SubmitGoldBrick = 436,
        AddBlackList = 440,
        RemoveBlackList = 441,
        DrawStory = 443,
        PetAttack = 447
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct ActionQuery
    {
        public uint ObjId;//4
        public uint dwParam;//8
        public int Timestamp;//12
        public ActionType Type;//16
        public ushort Fascing;// 18
        public ushort wParam1; // 20
        public ushort wParam2; // 22
        public uint dwParam3; // 24
        public uint dwParam4; // 26
        public ushort wParam5; // 28 total length

        public ushort Data1Low
        {
            get { return (ushort)dwParam; }
            set { dwParam = (uint)((Data1High << 16) | value); }
        }

        /// <summary>
        /// Offset [14]
        /// </summary>
        public ushort Data1High
        {
            get { return (ushort)(dwParam >> 16); }
            set { dwParam = (uint)((value << 16) | Data1Low); }
        }
    }
    public unsafe static class MsgDataPacket
    {
        public static unsafe ServerSockets.Packet ActionPick(this ServerSockets.Packet stream, uint UID, ushort dwparam2, ushort timer, string text)
        {
            stream.InitWriter();
            // stream.Write(Time32.Now.GetHashCode());
            stream.Write(UID); // 4
            stream.Write(220); // 8
            stream.Write(Time32.Now.GetHashCode()); // 16
            stream.Write((ushort)ActionType.Pick); // 20
            stream.Write(dwparam2); // 22
            //stream.Write(dwParam3);//24
            stream.ZeroFill(8); // 24
            stream.Write(timer); //28
            stream.ZeroFill(3); // 30
            stream.Write(text); // 34
            stream.Finalize(GamePackets.DataMap);
            return stream;
        }
        public static unsafe ServerSockets.Packet JumpCustom(this ServerSockets.Packet stream, uint UID, ushort x, ushort y)
        {
            stream.InitWriter();
            // stream.Write(Time32.Now.GetHashCode());
            stream.Write(UID); // 4
            stream.Write((ushort)x); // 8
            stream.Write((ushort)y); // 8
            stream.Write((ushort)ushort.MaxValue); // 8
            stream.Write((ushort)ushort.MaxValue); // 8
            stream.Write((ushort)137); // 8
            stream.Write(UID); // 4
            stream.Write(Time32.Now.GetHashCode()); // 16
            stream.Finalize(GamePackets.DataMap);
            return stream;
        }
        public static unsafe ServerSockets.Packet ItemGarm(this ServerSockets.Packet stream, uint ID)
        {
            stream.InitWriter();
            stream.Write(1);//4
            stream.Write(ID);//8
            stream.Write((ushort)10);//12
            stream.Write((ushort)10);//14
            stream.Write((ushort)1);//16
            stream.Write((byte)9);//18
            stream.Finalize(GamePackets.Item);
            //return stream;
            //stream.InitWriter();

            //stream.Write(1);//4
            //stream.Write(ID);//8
            //stream.Write((ushort)10);//12
            //stream.Write((ushort)10);//14
            //stream.Write((ushort)Role.Flags.ItemMode.AddItem);//16
            //stream.Write((ushort)9);//18
            //stream.Write(0);//20

            //stream.Write((byte)0);//24

            //stream.Write((byte)0);//25
            //stream.Write((ushort)0);//26
            //stream.Write((byte)0);//28
            //stream.Write((byte)0);//29
            //stream.Write((byte)0);//30
            //stream.Write((byte)0);//31
            //                           //stream.Write(item.Suspicious);//31 ???
            //                           //stream.Write((uint)99999);
            //                           //stream.Write((byte)123);
            //stream.ZeroFill(6);//38
            //stream.Write((byte)0);//38
            //stream.ZeroFill(1);//38
            //stream.Write((uint)1);//40
            //stream.Write((uint)item.PlusProgress);//44
            //stream.Finalize(Game.GamePackets.Item);

            return stream;
        }
        
        public static unsafe void Action(this ServerSockets.Packet stream, ActionQuery* pQuery)
        {
            stream.ReadUnsafe(pQuery, sizeof(ActionQuery));
        }
        public static unsafe ServerSockets.Packet ActionCreate(this ServerSockets.Packet stream, ActionQuery* pQuery)
        {
            stream.InitWriter();
            stream.WriteUnsafe(pQuery, sizeof(ActionQuery));
            stream.Finalize(GamePackets.DataMap);
            return stream;
        }
        public static unsafe ServerSockets.Packet ActionCreateWithString(this ServerSockets.Packet stream, ActionQuery* pQuery, params string[] str)
        {
            stream.InitWriter();
            stream.WriteUnsafe(pQuery, sizeof(ActionQuery));
            stream.SeekBackwards(1);
            stream.Write(str);
            stream.Finalize(GamePackets.DataMap);
            return stream;
        }
        public class DataAttribute : Attribute
        {
            public static readonly Func<DataAttribute, ActionType> Translator = (a) => a.Type;
            public ActionType Type { get; private set; }
            public DataAttribute(ActionType type)
            {
                this.Type = type;
            }
        }

        public static ActionInvoker invoker = new ActionInvoker(DataAttribute.Translator);

        [PacketAttribute(GamePackets.DataMap)]
        private unsafe static void Process(Client.GameClient user, ServerSockets.Packet stream)
        {
            try
            {

                ActionQuery data;
                stream.Action(&data);

                Tuple<DataAttribute, ProcessAction> processFolded;
                if (invoker.TryGetInvoker(data.Type, out processFolded))
                    processFolded.Item2(user, stream, &data);
                else
                {
                    //if (user.ProjectManager)
                       // Console.WriteLine("DataMap not find  " + data.Type + " ");
                }
            }
            catch (Exception e) { Console.WriteException(e); }
        }

        [DataAttribute(ActionType.ViewFriendInfo)]
        public unsafe static void ViewFriendInfo(Client.GameClient client, ServerSockets.Packet msg, ActionQuery* data)
        {
            Client.GameClient Target;
            if (Database.Server.GamePoll.TryGetValue(data->dwParam, out Target))
            {
                client.Send(msg.KnownPersonInfoCreate(Target, false));
            }
        }
        [DataAttribute(ActionType.Mining)]
        public unsafe static void Mining(Client.GameClient client, ServerSockets.Packet msg, ActionQuery* data)
        {
            if (!client.Map.TypeStatus.HasFlag(Role.MapTypeFlags.MineEnable))
            {
                client.SendSysMesage("Mining is unavalable on this map.");
                return;
            }
            if (!client.Player.Mining)
            {
                client.Player.Mining = true;
                client.Send(msg.ActionCreate(data));
            }
        }
        [DataAttribute(ActionType.Bulletin)]
        public unsafe static void Bulletin(Client.GameClient client, ServerSockets.Packet msg, ActionQuery* data)
        {
            if (client.Player.Map == 1017 || client.Player.Map == 1081 || client.Player.Map == 2060 || client.Player.Map == 9972
                  || client.Player.Map == 1080 || client.Player.Map == 3820 || client.Player.Map == 3954
              || client.Player.Map == 1806
                  || Game.MsgTournaments.MsgSchedules.DisCity.IsInDisCity(client.Player.Map) || client.Player.Map == 1508
          || client.Player.Map == 1768
          || client.Player.Map == 1505 || client.Player.Map == 1506 || client.Player.Map == 1509 || client.Player.Map == 1508 || client.Player.Map == 1507)
            {
                return;
            }

            if (client.Player.Map == 1038 || client.Player.Map == MsgTournaments.MsgClassPKWar.MapID || client.Player.DynamicID != 0
                || client.Player.Map == 6001)
            {

                return;
            }
            if (Program.BlockTeleportMap.Contains(client.Player.Map))
                return;
            switch (data->dwParam)
            {
                case 105://house
                    {
                        client.Teleport(200, 95, 1036);
                        client.Player.SendString(msg, MsgStringPacket.StringID.Effect, true, "movego");
                        break;
                    }
            }
        }
        [DataAttribute(ActionType.RemoveBlackList)]
        public unsafe static void RemoveBlackList(Client.GameClient client, ServerSockets.Packet msg, ActionQuery* data)
        {
            int size = msg.ReadInt8();
            string TargetName = msg.ReadCString(size);
            foreach (var user in Database.Server.GamePoll.Values)
            {
                if (user.Player.Name == TargetName)
                {
                    data->ObjId = user.Player.UID;
                    break;
                }
            }
            data->dwParam = 1;
            client.Send(msg.ActionCreateWithString(data, TargetName));
        }
        [DataAttribute(ActionType.AddBlackList)]
        public unsafe static void AddBlackList(Client.GameClient client, ServerSockets.Packet msg, ActionQuery* data)
        {
            int size = msg.ReadInt8();
            string TargetName = msg.ReadCString(size);
            foreach (var user in Database.Server.GamePoll.Values)
            {
                if (user.Player.Name == TargetName)
                {
                    data->ObjId = user.Player.UID;
                    break;
                }
            }
            data->dwParam = 1;
            client.Send(msg.ActionCreateWithString(data, TargetName));

        }
       [DataAttribute(ActionType.SetAppearanceType)]
        public unsafe static void SetAppearanceType(Client.GameClient client, ServerSockets.Packet msg, ActionQuery* data)
        {
            client.Player.AparenceType = data->dwParam;
            data->ObjId = client.Player.UID;
            client.Send(msg.ActionCreate(data));
        }

        [DataAttribute(ActionType.Ghost)]
        public unsafe static void Ghost(Client.GameClient client, ServerSockets.Packet msg, ActionQuery* data)
        {
            if (client.Player.Alive == false)
            {
                //client.Player.SendString(msg, MsgStringPacket.StringID.Effect, true, "ghost");
                data->ObjId = client.Player.UID;
                data->wParam1 = client.Player.X;
                data->wParam2 = client.Player.Y;
                client.Player.View.SendView(msg.ActionCreate(data), true);
            }
        }
        [DataAttribute(ActionType.StartVendor)]
        public unsafe static void StartVendor(Client.GameClient client, ServerSockets.Packet msg, ActionQuery* data)
        {
            if (!client.IsVendor)
            {
                client.MyVendor = new Role.Instance.Vendor(client);
                client.MyVendor.CreateVendor(msg);
                data->dwParam = client.MyVendor.VendorUID;
                data->wParam1 = client.MyVendor.VendorNpc.X;
                data->wParam2 = client.MyVendor.VendorNpc.Y;
                client.Send(msg.ActionCreate(data));
            }
        }
        [DataAttribute(ActionType.Revive)]
        public unsafe static void Revive(Client.GameClient client, ServerSockets.Packet msg, ActionQuery* data)
        {
            if (Time32.Now > client.Player.DeadStamp.AddSeconds(20))
            {
                if (!client.Player.Alive)
                {
                    bool ReviveHere = data->dwParam == 1;

                    if (ReviveHere)
                    {
                        if (client.Player.ContainFlag(MsgUpdate.Flags.BlackName))
                        {
                            client.Teleport(200, 200, 6000);
                        }
                        if (client.Player.Map == 1038
                        || client.Player.Map == MsgTournaments.MsgClassPKWar.MapID
                        || Program.FreePkMap.Contains(client.Player.Map))
                        {
                            client.Teleport(428, 378, 1002);
                        }
                        else
                            client.Player.Revive(msg);
                        return;
                    }
                    else
                    {
                        if (Program.RevivePoint.Contains(client.Player.Map))
                        {
                            client.Teleport(428, 378, 1002);
                        }

                        //if (Game.MsgTournaments.MsgSchedules.CityWar.Process == MsgTournaments.ProcesType.Alive)
                        //{
                        //    if (Game.MsgTournaments.MsgSchedules.CityWar.CurentWar != null)
                        //    {
                        //        if (Game.MsgTournaments.MsgSchedules.CityWar.CurentWar.InWar(client))
                        //        {
                        //            Game.MsgTournaments.MsgSchedules.CityWar.CurentWar.Teleport(client);
                        //            return;
                        //        }
                        //    }
                        //}
                        if (client.Player.Map == 1011 && client.Player.DynamicID != 0)
                        {
                            client.Teleport(client.Map.Reborn_X, client.Map.Reborn_Y, client.Map.Reborn_Map, client.Player.DynamicID);
                            return;
                        }
                        if (client.Player.Map == 700 && client.Player.DynamicID != 0)
                        {
                            client.Teleport(428, 378, 1002);
                            return;

                        }
                        if (client.Player.Map == 4000 || client.Player.Map == 4003 || client.Player.Map == 4006 || client.Player.Map == 4008 || client.Player.Map == 4009)
                        {
                            client.Teleport(85, 75, 4020);
                            return;
                        }
                        if (client.Player.Map == 3998)
                        {
                            client.Teleport(106, 383, 3998);
                            return;
                        }
                        if (client.Player.Map == 2071)
                        {
                            client.Teleport(48, 135, 2071);
                            return;
                        }
                        if (client.Player.OnMyOwnServer == false)
                        {
                            client.Teleport((ushort)432, 390, 1002);
                            return;
                        }
                        if (client.Player.Map == 1762 || client.Player.Map == 1927 || client.Player.Map == 1999 || client.Player.Map == 2054 || client.Player.Map == 2055)
                        {
                            client.Teleport(477, 640, 1000);
                            return;
                        }
                        if (client.Player.Map == 1082)
                        {
                            client.Teleport(724, 573, 1015);
                            return;
                        }
                        if (client.Player.Map == 1212)
                        {
                            client.Teleport(439, 387, 1002);
                            return;
                        }
                        if (client.Player.Map == 1038)//  map
                        {
                            client.Teleport(50, 50, 6000);
                            return;
                        }
                        else
                        {
                            if (client.Map.Reborn_Map != client.Player.Map)
                            {
                                if (client.Map.Reborn_Map == 0)
                                {
                                    client.Teleport(428, 378, 1002);
                                    return;
                                }
                                if (client.Player.Map == 1210)
                                    client.Teleport(428, 386, 1002);
                                else
                                {
                                    var map = Database.Server.ServerMaps[client.Map.Reborn_Map];
                                    client.Teleport(map.Reborn_X, map.Reborn_Y, map.Reborn_Map);
                                }
                            }
                            else
                            {
                                if (client.Map.Reborn_X != 0)
                                    client.Teleport(client.Map.Reborn_X, client.Map.Reborn_Y, client.Map.Reborn_Map);
                                else
                                {
                                    var map = Database.Server.ServerMaps[client.Map.Reborn_Map];
                                    if (map.Reborn_X != 0)
                                        client.Teleport(map.Reborn_X, map.Reborn_Y, map.Reborn_Map);
                                    else
                                    {
                                        map = Database.Server.ServerMaps[map.Reborn_Map];
                                        if (map.Reborn_X != 0)
                                            client.Teleport(map.Reborn_X, map.Reborn_Y, map.Reborn_Map);
                                        else
                                            client.Teleport(428, 378, 1002);
                                    }
                                }
                            }
                        }
                        if (client.Player.X == 0 || client.Player.Y == 0)//invalid map reborn
                            client.Teleport(428, 378, 1002);
                    }
                }
            }
        }
        
        [DataAttribute(ActionType.EndTransformation)]
        public unsafe static void EndTransformation(Client.GameClient client, ServerSockets.Packet msg, ActionQuery* data)
        {
            if (client.Player.OnTransform)
            {
                if (client.Player.TransformInfo != null)
                {
                    client.Player.TransformInfo.Stamp = Time32.Now;
                }
            }
        }
        [DataAttribute(ActionType.UpdateSpell)]
        public unsafe static void UpdateSpell(Client.GameClient client, ServerSockets.Packet msg, ActionQuery* data)
        {
            MsgSpell Spell;
            if (client.MySpells.ClientSpells.TryGetValue((ushort)data->dwParam, out Spell))
            {
                Dictionary<ushort, Database.MagicType.Magic> DbSpells;
                if (Database.Server.Magic.TryGetValue(Spell.ID, out DbSpells))
                {
                    if (Spell.Level < DbSpells.Count - 1)
                    {
                        float Costfloat = (((float)DbSpells[Spell.Level].Experience - (float)Spell.Experience) / (float)DbSpells[Spell.Level].Experience * (float)DbSpells[Spell.Level].CPUpgradeRatio) * 27.0f / 600.0f;
                        uint Cost = (uint)Math.Ceiling(Costfloat);
                        if (client.Player.ConquerPoints >= Cost)
                        {
                            client.Player.ConquerPoints -= (uint)Cost;

                            client.MySpells.Add(msg, Spell.ID, (ushort)(Spell.Level + 1), Spell.SoulLevel, Spell.PreviousLevel, Spell.Experience);
                        }
                    }
                }
            }
        }
        [DataAttribute(ActionType.UpdateProf)]
        public unsafe static void UpdateProf(Client.GameClient client, ServerSockets.Packet msg, ActionQuery* data)
        {
            if (client != null)
            {
                Console.WriteLine("unuseed prof");
                MsgProficiency prof;
                if (client.MyProfs.ClientProf.TryGetValue(data->dwParam, out prof))
                {
                    if (prof.Level <= 19)
                    {
                        ushort PriceUpdate = Database.Server.PriceUpdatePorf[prof.Level];
                        //float Costfloat = (((float)Role.Instance.Proficiency.ProficiencyLevelExperience((byte)prof.Level)) - (float)prof.Experience) / (float)Role.Instance.Proficiency.MaxExperience * (float)PriceUpdate) * 27.0f / 600.0f;
                        //uint Cost = (uint)Math.Ceiling(Costfloat);
                        //if (client.Player.ConquerPoints >= Cost)
                        //{
                        //    client.Player.ConquerPoints -= Cost;

                        //    client.MyProfs.Add(msg, prof.ID, prof.Level + 1, prof.Experience, prof.PreviouseLevel);
                        //}
                    }
                }
            }
        }
        [DataAttribute(ActionType.StopVending)]
        public unsafe static void StopVending(Client.GameClient client, ServerSockets.Packet msg, ActionQuery* data)
        {
            if (client.IsVendor)
                client.MyVendor.StopVending(msg);

            // update the field of view from a completely new location (so clear old fov)

          //  client.Player.View.CleanScreen();
            client.Player.SendUpdate(msg, client.Player.VipLevel, MsgUpdate.DataType.VIPLevel);

        }
        [DataAttribute(ActionType.EndFly)]
        public unsafe static void EndFly(Client.GameClient client, ServerSockets.Packet msg, ActionQuery* data)
        {
            client.Player.RemoveFlag(MsgUpdate.Flags.Fly);
        }
        [DataAttribute(ActionType.TeamSearchForMember)]
        public unsafe static void TeamSearchForMember(Client.GameClient client, ServerSockets.Packet msg, ActionQuery* data)
        {
            if (client.Team != null)
            {
                Client.GameClient Member;
                if (client.Team.TryGetMember(data->dwParam, out Member))
                {
                    if (Member.Player.Map == client.Player.Map)
                    {
                        data->wParam1 = Member.Player.X;
                        data->wParam2 = Member.Player.Y;
                        client.Send(msg.ActionCreate(data));
                    }
                }
            }
        }
        [DataAttribute(ActionType.SetLocation)]
        public unsafe static void SetLocation(Client.GameClient client, ServerSockets.Packet msg, ActionQuery* data)
        {
            if ((client.ClientFlag & Client.ServerFlag.SetLocation) != Client.ServerFlag.SetLocation)
            {


#if !Roullete
                if (client.Player.Map == 3852)
                {
                    client.Player.Map = 1002;
                    client.Player.X = 428;
                    client.Player.Y = 378;
                }
#endif

#if !Poker
                if (client.Player.Map == 1860 || client.Player.Map == 1858)
                {
                    client.Player.Map = 1002;
                    client.Player.X = 428;
                    client.Player.Y = 378;
                }
#endif
                if (!Role.GameMap.CheckMap(client.Player.Map))
                {
                    client.Player.Map = 1002;
                    client.Player.X = 428;
                    client.Player.Y = 378;
                }
            back:
                if (Database.Server.ServerMaps.TryGetValue(client.Player.Map, out client.Map))
                {

                    client.ClientFlag |= Client.ServerFlag.SetLocation;
                    client.Map.Enquer(client);

                    if (client.Player.HitPoints == 0)
                    {
                        client.Player.HitPoints = 1;

                        if (client.Player.Map == 1038)// gw map
                        {
                            client.Teleport(428, 378, 1002);
                        }
                        else
                        {
                            if (client.Player.Map == 601)
                                client.Teleport(428, 378, 1002);
                            else
                            {
                                if (client.Map.Reborn_Map == 1002)
                                {
                                    client.Map.Reborn_X = 428;
                                    client.Map.Reborn_Y = 378;
                                }
                                if (client.Map.Reborn_X != 0)
                                    client.Teleport(client.Map.Reborn_X, client.Map.Reborn_Y, client.Map.Reborn_Map);
                                else
                                {
                                    Role.GameMap map;// ;= Database.Server.ServerMaps[client.Map.Reborn_Map];
                                    if (Database.Server.ServerMaps.TryGetValue(client.Map.Reborn_Map, out map))
                                    {
                                        if (map.Reborn_X != 0)
                                            client.Teleport(map.Reborn_X, map.Reborn_Y, map.Reborn_Map);
                                        else
                                        {
                                            map = Database.Server.ServerMaps[map.Reborn_Map];
                                            if (map.Reborn_X != 0)
                                                client.Teleport(map.Reborn_X, map.Reborn_Y, map.Reborn_Map);
                                            else
                                                client.Teleport(428, 378, 1002);
                                        }
                                    }
                                    else
                                    {

                                        client.Teleport(428, 378, 1002);
                                    }
                                }
                                if (client.Player.X == 0 || client.Player.Y == 0)//invalid map reborn
                                    client.Teleport(428, 378, 1002);
                            }
                        }
                    }

                    if (client.Map.BaseID != 0)
                        data->dwParam = client.Map.BaseID;
                    else
                        data->dwParam = client.Player.Map;
                    data->wParam1 = client.Player.X;
                    data->wParam2 = client.Player.Y;
                    client.Send(msg.ActionCreate(data));




                    client.Player.ClearPreviouseCoord();
                    client.Player.View.Role();



                }
                else
                {
                    client.Player.Map = 1002;
                    client.Player.X = 428;
                    client.Player.Y = 378;
                    Database.Server.ServerMaps[1002].Enquer(client);
                    goto back;
                }

                client.Send(msg.MapStatusCreate(client.Map.ID, client.Map.ID, (uint)client.Map.TypeStatus));

                //client.Send(msg.WeatherCreate(MsgWeather.WeatherType.Snow, 1000, 3, 0, 0));
                if (client.Player.Map == 3846)
                {

                    ActionQuery datam = new ActionQuery()
                    {
                        ObjId = client.Player.UID,
                        Type = ActionType.SetMapColor,
                        dwParam = 16755370,
                        wParam1 = client.Player.X,
                        wParam2 = client.Player.Y
                    };

                    client.Send(msg.ActionCreate(&datam));
                }
            }
            /*
            msg.InitWriter();
            msg.Write(Time32.Now.Value);
            msg.Write(client.Player.UID);
            msg.Write((uint)0);
            msg.ZeroFill(8);
            msg.Write((uint)0x9d);
            msg.Write((ushort)91);
            msg.Write((byte)0x5f);
            msg.Write((byte)0x01);
            msg.ZeroFill(10
                );
            msg.Finalize(10010);
            client.Send(msg);*/


        }
        [DataAttribute(ActionType.ChangeMap)]
        public unsafe static void ChangeMap(Client.GameClient client, ServerSockets.Packet msg, ActionQuery* data)
        {
            if (client.Player.Mining) client.Player.Mining = false;
                ushort Portal_X = (ushort)(data->dwParam & 0xFFFF);
            ushort Portal_Y = (ushort)(data->dwParam >> 16);
            if (Database.HouseTable.InHouse(client.Player.Map) && client.Player.DynamicID != 0)
            {
                client.Teleport(199, 093, 1036);
                return;
            }
            if (client.Player.Map == 1210 && Portal_X == 4 && Portal_Y == 329)
            {
                client.Teleport(951, 558, 1211);
                return;
            }
            if (client.Player.Map == 1082 && Portal_X == 308 && Portal_Y == 283
                || Portal_X == 170 && Portal_Y == 99
                || Portal_X == 99 && Portal_Y == 171
                || Portal_X == 182 && Portal_Y == 281
                || Portal_X == 272 && Portal_Y == 176)
            {
                client.Teleport(208, 216, 1082);
                return;
            }
            foreach (var portal in client.Map.Portals)
            {
                if (Role.Core.GetDistance(client.Player.X, client.Player.Y, portal.X, portal.Y) < 7)
                {
                    //if (portal.Destiantion_MapID == 1210)
                    //{
                    //    client.Teleport(431, 387, 1002);
                    //    return;
                    //}
                  

                    client.Teleport(portal.Destiantion_X, portal.Destiantion_Y, portal.Destiantion_MapID);
                    client.Send(msg.ActionCreate(data));
                    return;
                }
            }
            if (client.ProjectManager)
            {
                client.SendSysMesage("Invalid Portal : X = " + Portal_X + ", Y= " + Portal_Y + " Map = " + client.Player.Map + "", MsgMessage.ChatMode.System, MsgMessage.MsgColor.yellow);
            }
            client.Teleport(428, 378, 1002);
        }
        [DataAttribute(ActionType.ChangeLookface)]
        public unsafe static void ChangeLookface(Client.GameClient client, ServerSockets.Packet msg, ActionQuery* data)
        {
            if (client.Player.Money >= 500)
            {
                uint newface = data->dwParam;
                if (client.Player.Body > 2000)
                {
                    newface = newface < 200 ? newface + 200 : newface;
                    client.Player.Face = (ushort)newface;
                }
                else
                {
                    newface = newface > 200 ? newface - 200 : newface;
                    client.Player.Face = (ushort)newface;
                }
                client.Player.Money -= 500;
                client.Send(msg.ActionCreate(data));
                client.Player.SendUpdate(msg, client.Player.Money, MsgUpdate.DataType.Money);
            }
            else
            {
                client.SendSysMesage("You don't have 500 silvers with you.", MsgMessage.ChatMode.TopLeft);
            }
        }
        [DataAttribute(ActionType.RequestEntity)]
        public unsafe static void RequestEntity(Client.GameClient client, ServerSockets.Packet msg, ActionQuery* data)
        {
            Role.IMapObj obj;
            if (client.Player.View.TryGetValue(data->dwParam, out obj, Role.MapObjectType.Player))
            {
                var pClient = (obj as Role.Player).Owner;
                if (pClient.Player.Invisible)
                    return;
                client.Send(obj.GetArray(msg, false));
                if (pClient.Pet != null) client.Send(pClient.Pet.monster.GetArray(msg, false));
            }
            else if (client.Player.View.TryGetValue(data->dwParam, out obj, Role.MapObjectType.Monster))
            {
                if (!client.Player.Robot)
                client.Send(obj.GetArray(msg, false));
            }
        }

        [DataAttribute(ActionType.QuerySpawn)]
        public unsafe static void QuerySpawn(Client.GameClient client, ServerSockets.Packet msg, ActionQuery* data)
        {
            Client.GameClient Target;
            if (Database.Server.GamePoll.TryGetValue(data->dwParam, out Target))
            {

                client.Send(Target.Player.GetArray(msg, true));
                if (client.Pet != null && Target.Pet != null) client.Send(Target.Pet?.monster.GetArray(msg, true));
            }

        }
        [DataAttribute(ActionType.ViewEquipment)]
        public unsafe static void ViewEquipment(Client.GameClient client, ServerSockets.Packet msg, ActionQuery* data)
        {
            Client.GameClient Target;
            if (Database.Server.GamePoll.TryGetValue(data->dwParam, out Target))
            {
                if (Target.Equipment == null || Target.Equipment.CurentEquip == null) return;

                foreach (var item in Target.Equipment.CurentEquip)
                {
                    if (item != null)
                    {
                        item.Mode = Role.Flags.ItemMode.View;
                        client.Send(item.ItemCreateViwer(msg, item, Target.Player.UID));

                    }
                }

                if (!client.ProjectManager)
                {
                    Target.SendSysMesage("" + client.Player.Name + " is checking you out!", MsgMessage.ChatMode.System);
                }
            }
        }
        [DataAttribute(ActionType.Hotkeys)]
        public unsafe static void Hotkeys(Client.GameClient client, ServerSockets.Packet msg, ActionQuery* data)
        {
            client.Send(msg.ActionCreate(data));

        }
        public static int CalculateJumpStamp(int distance)
        {
            return (int)400 + distance * 40;
        }
        [DataAttribute(ActionType.Jump)]
        public unsafe static void PlayerJump(Client.GameClient client, ServerSockets.Packet msg, ActionQuery* data)
        {
            //client.SendSysMesage($"{data->dwParam_Lo},{data->dwParam_Hi}");
            if (!client.Player.Alive) return;
            //if (client.Player.Robot)
            //{
            //    client.Pullback();
            //    client.Player.MessageBox("Can`t move while auto hunting, do you want to stop auto hunting?", new Action<Client.GameClient>(p =>
            //    {
            //        p.Player.Robot = false;
            //        p.AutoHunting = 0;
            //    }), null, 0);
            //}
            if (client.Player.InUseIntensify)
            {
                if (client.Player.ContainFlag(MsgUpdate.Flags.Intensify))
                    client.Player.RemoveFlag(MsgUpdate.Flags.Intensify);
                client.Player.InUseIntensify = false;
            }
            if (client.Player.Mining) client.Player.Mining = false;

            if (client.Player.BlockMovementCo)
            {
                if (DateTime.Now < client.Player.BlockMovement)
                {
                    client.SendSysMesage($"You can`t move for {(client.Player.BlockMovement - DateTime.Now).TotalSeconds} seconds.");
                    client.Pullback();
                    return;
                }
                else
                    client.Player.BlockMovementCo = false;
            }

            //if (client.Player.dummies == 0)
            //{
            //    client.Player.dummyX = client.Player.X;
            //    client.Player.dummyY = client.Player.Y;
            //    client.Player.dummies = 1;
            //}
            //else
            //{
            //    client.Player.dummyX2 = client.Player.X;
            //    client.Player.dummyY2 = client.Player.Y;
            //    client.Player.dummies = 0;
            //}

            client.Player.LastMove = DateTime.Now;

            client.OnAutoAttack = false;
            client.Player.RemoveBuffersMovements(msg);

            if (!client.Player.Alive)
            {
                client.Map.View.MoveTo<Role.IMapObj>(client.Player, client.Player.Dead_X, client.Player.Dead_Y);
                if (client.Player.Dead_X == 0 || client.Player.Dead_Y == 0)
                {
                    client.Player.X = 428;
                    client.Player.Y = 378;
                }
                else
                {
                    client.Player.X = client.Player.Dead_X;
                    client.Player.Y = client.Player.Dead_Y;
                }

                InteractQuery action = new InteractQuery()
                {
                    X = client.Player.Dead_X,
                    AtkType = MsgAttackPacket.AttackID.Death,
                    Y = client.Player.Dead_Y,
                    OpponentUID = client.Player.UID
                };
                client.Player.View.SendView(msg.InteractionCreate(&action), true);

                return;
            }

            ushort JumpX = (ushort)(data->Data1Low);
            ushort JumpY = (ushort)(data->Data1High);

            client.Player.Protect = Time32.Now;

            //if (client.IsLagging())
            //{
            //    if (Time32.Now > client.LastLag.AddMinutes(5))
            //        client.LaggingCount = 0;
            //    client.LaggingCount++;
            //    client.LastLag = Time32.Now;
            //    if (client.LaggingCount == 15)
            //        client.Socket.Disconnect();
            //}
            if (client.Map == null)
            {
                client.Teleport(428, 378, 1002);
                return;
            }
            if (client.Player.Map == 1038)
            {
                if (!Game.MsgTournaments.MsgSchedules.GuildWar.ValidJump(client.TerainMask, out client.TerainMask, JumpX, JumpY))
                {
                    client.SendSysMesage("Illegal jumping over the gates detected!");
                    //client.Teleport(165, 212, 1038);
                    client.Pullback();
                    return;
                }
            }
            //if (!client.Map.ValidLocation(JumpX, JumpY))
            //{
            //    client.Pullback();
            //    return;
            //}
            //if ((client.Player.PreviousJump - client.Player.JumpingStamp).TotalMilliseconds <= 500)
            //{
            //    if (!client.Player.ContainFlag(MsgUpdate.Flags.Ride) && !client.Player.ContainFlag(MsgUpdate.Flags.Cyclone) && !client.Player.ContainFlag(MsgUpdate.Flags.Oblivion) && !client.Player.OnTransform)
            //    {
            //        client.Player.CountSpeedHack++;
            //        if (client.Player.CountSpeedHack >= 4)
            //        {
            //            MsgCheatPacket.Report(client.Player.Name, "SpeedHACK");
            //            using (var rec = new ServerSockets.RecycledPacket())
            //            {
            //                var stream = rec.GetStream();
            //                Role.Core.SendGlobalMessage(stream, client.Player.Name + "<<< This ape was cheating by speedhack and was disconnected.", MsgMessage.ChatMode.TopLeftSystem);
            //            }
            //            client.Socket.Disconnect();
            //        }
            //    }
            //}
            //else
            //{
            //    client.Player.CountSpeedHack = Math.Max(0, client.Player.CountSpeedHack - 1);
            //}
            client.Player.PreviousJump = client.Player.JumpingStamp;
            client.Player.JumpingStamp = DateTime.Now;


            if (client.Player.ObjInteraction != null)
            {

                client.Player.Angle = Role.Core.GetAngle(client.Player.X, client.Player.Y, JumpX, JumpY);
                client.Player.Action = Role.Flags.ConquerAction.Jump;
                client.Map.View.MoveTo<Role.IMapObj>(client.Player, JumpX, JumpY);
                client.Player.X = JumpX;
                client.Player.Y = JumpY;
                client.Player.View.Role(false, null);//msg.ActionCreate(data));

                client.Map.View.MoveTo<Role.IMapObj>(client.Player.ObjInteraction.Player, JumpX, JumpY);
                client.Player.ObjInteraction.Player.X = JumpX;
                client.Player.ObjInteraction.Player.Y = JumpY;
                client.Player.ObjInteraction.Player.Action = Role.Flags.ConquerAction.Jump;
                client.Player.ObjInteraction.Player.Angle = Role.Core.GetAngle(client.Player.X, client.Player.Y, JumpX, JumpY);
                client.Player.ObjInteraction.Player.View.Role(false, null);

                return;
            }

            int Distance = Role.Core.GetDistance(client.Player.X, client.Player.Y, JumpX, JumpY);

            client.Player.StampJump = DateTime.Now;
            client.Player.StampJumpMiliSeconds = CalculateJumpStamp(Distance);
            //if (client.Player.ContainFlag(MsgUpdate.Flags.Ride))
            //{
            //    uint vigor = (ushort)(1.5F * (Distance / 2));
            //    if (client.Vigor >= vigor)
            //        client.Vigor -= vigor;
            //    else
            //        client.Vigor = 0;


            //    client.Send(msg.ServerInfoCreate(MsgServerInfo.Action.Vigor, client.Vigor));

            //}
            data->dwParam3 = client.Player.Map;
            if (data->ObjId < 1000000)
            {
                client.Player.View.SendView(msg.ActionCreate(data), true);
                client.Pet.monster.Facing = Role.Core.GetAngle(client.Pet.monster.X, client.Pet.monster.Y, JumpX, JumpY);
                client.Pet.monster.Action = Role.Flags.ConquerAction.Jump;
                client.Map.View.MoveTo<Role.IMapObj>(client.Pet.monster, JumpX, JumpY);
                client.Pet.monster.X = JumpX;
                client.Pet.monster.Y = JumpY;
                client.Pet.monster.UpdateMonsterView(null, msg);
            }
            else
            {
                client.Player.View.SendView(msg.ActionCreate(data), true);
                client.Player.Angle = Role.Core.GetAngle(client.Player.X, client.Player.Y, JumpX, JumpY);
                client.Player.Action = Role.Flags.ConquerAction.Jump;
                client.Map.View.MoveTo<Role.IMapObj>(client.Player, JumpX, JumpY);
                client.Player.X = JumpX;
                client.Player.Y = JumpY;
            }
            client.Player.View.Role(false, null);//msg.ActionCreate(data));

            if (client.Player.ActivePick)
                client.Player.RemovePick(msg);

            var Squama = MsgTournaments.MsgSchedules.Squama.Squama.Where(x => x.Value.X == client.Player.X && x.Value.Y == client.Player.Y).SingleOrDefault();
            if (Squama.Value != null)
            {
                MsgTournaments.MsgSchedules.Squama.ClaimedReward(client, Squama.Key);
                Squama.Value.SquamaTrap = false;
            }
        }

        [DataAttribute(ActionType.DeleteCharacter)]
        public unsafe static void DeleteCharacter(Client.GameClient client, ServerSockets.Packet msg, ActionQuery* data)
        {

            if (client.Player.OnMyOwnServer == false)
                return;
            if (data->dwParam == 0)
            {
                if (client.Player.MyGuild != null)
                {
                    client.SendSysMesage("Please remove your guild.");
                    return;
                }
                client.Player.Delete = true;
                client.Socket.Disconnect();
            }
            else
            {
                client.SendSysMesage("Your password is incorrect.");
            }

        }
        [DataAttribute(ActionType.ChangeDirection)]
        public unsafe static void ChangeDirection(Client.GameClient client, ServerSockets.Packet msg, ActionQuery* data)
        {
            client.OnAutoAttack = false;
            client.Player.Angle = (Role.Flags.ConquerAngle)data->Fascing;
            client.Player.View.SendView(msg.ActionCreate(data), true);
            if (client.Player.ActivePick)
                client.Player.RemovePick(msg);
        }
        [DataAttribute(ActionType.ChangeStance)]
        public unsafe static void ChangeStance(Client.GameClient client, ServerSockets.Packet msg, ActionQuery* data)
        {
            if (client.Player.Mining) client.Player.Mining = false;

            client.OnAutoAttack = false;//230 cool -210 knel - 270 lie -- 200 bow -- 250 sit 
            client.Player.Action = (Role.Flags.ConquerAction)data->dwParam;

            if (client.Player.Action == Role.Flags.ConquerAction.Cool)
            {
                if (client.Equipment.FullSuper)
                    data->dwParam = (uint)(data->dwParam | (uint)(client.Player.Class * 0x10000 + 0x1000000));
                else if (client.Equipment.SuperArmor)
                    data->dwParam = (uint)(data->dwParam | (uint)(client.Player.Class * 0x10000));
            }
            client.Player.View.SendView(msg.ActionCreate(data), true);

            if (client.Player.ContainFlag(MsgUpdate.Flags.CastPray))
            {
                foreach (var user in client.Player.View.Roles(Role.MapObjectType.Player))
                {
                    if (Role.Core.GetDistance(client.Player.X, client.Player.Y, user.X, user.Y) <= 4)
                    {
                        data->Timestamp = (int)user.UID;
                        client.Player.View.SendView(msg.ActionCreate(data), true);
                    }
                }
            }
            if (client.Player.ActivePick)
                client.Player.RemovePick(msg);
        }
        [DataAttribute(ActionType.SetPkMode)]
        public unsafe static void SetPkMode(Client.GameClient client, ServerSockets.Packet msg, ActionQuery* data)
        {
            client.OnAutoAttack = false;
            if (client.Player.PkMode == Role.Flags.PKMode.Jiang)
                client.SendSysMesage("Jianghu will turn of in 10 minutes.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
            client.Player.PkMode = (Role.Flags.PKMode)data->dwParam;
            client.Send(msg.ActionCreate(data));
            if (client.Player.PkMode == Role.Flags.PKMode.PK)
                client.SendSysMesage("Free PK mode. You can attack monsters and all players.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
            else if (client.Player.PkMode == Role.Flags.PKMode.Capture)
                client.SendSysMesage("Capture PK mode. You can only attack monsters, black-name, and blue-name players.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
            else if (client.Player.PkMode == Role.Flags.PKMode.Peace)
                client.SendSysMesage("Peace mode. You can only attack monsters.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
            else if (client.Player.PkMode == Role.Flags.PKMode.Team)
                client.SendSysMesage("Team PK mode. You can attack monsters, and all players except your teammates or guild.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
            else if (client.Player.PkMode == Role.Flags.PKMode.Revange)
                client.SendSysMesage("Revenge PK mode. You can attack monsters and all your Enemy list players.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
            else if (client.Player.PkMode == Role.Flags.PKMode.Guild)
                client.SendSysMesage("Guild PK mode. You can attack monsters and all Your Guild's Enemies.", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
        }
        [DataAttribute(ActionType.ConfirmAssociates)]
        public unsafe static void ConfirmAssociates(Client.GameClient client, ServerSockets.Packet msg, ActionQuery* data)
        {
            client.Send(msg.ActionCreate(data));
        }
        [DataAttribute(ActionType.ConfirmSpells)]
        public unsafe static void ConfirmSpells(Client.GameClient client, ServerSockets.Packet msg, ActionQuery* data)
        {
            client.MySpells.SendAll(msg);
            client.Send(msg.ActionCreate(data));
        }
        [DataAttribute(ActionType.ConfirmProficiencies)]
        public unsafe static void ConfirmProficiencies(Client.GameClient client, ServerSockets.Packet msg, ActionQuery* data)
        {
            client.MyProfs.SendAll(msg);
            client.Send(msg.ActionCreate(data));
        }
        [DataAttribute(ActionType.ConfirmGuild)]
        public unsafe static void ConfirmGuild(Client.GameClient client, ServerSockets.Packet msg, ActionQuery* data)
        {
            if (client.Player.MyGuild != null && client.Player.MyGuildMember != null)
            {
                client.Player.SendString(msg, Game.MsgServer.MsgStringPacket.StringID.GuildName, client.Player.MyGuild.Info.GuildID, true
                    , new string[1] { client.Player.MyGuild.GuildName });

                client.Player.MyGuild.SendThat(client.Player);
                client.Player.SendString(msg, Game.MsgServer.MsgStringPacket.StringID.GuildName, client.Player.MyGuild.Info.GuildID, true
                    , new string[1] { client.Player.MyGuild.GuildName });

                client.Player.MyGuild.SendGuildAlly(msg, true, client);
                client.Player.MyGuild.SendGuilEnnemy(msg, true, client);
            }
            client.Send(msg.ActionCreate(data));
        }
    
        [DataAttribute(ActionType.AllowAnimation)]
        public unsafe static void AllowAnimation(Client.GameClient client, ServerSockets.Packet msg, ActionQuery* data)
        {
            if (client.Pet != null)
            {
                //short distance = Role.Core.GetDistance(client.Pet.monster.X, client.Pet.monster.Y, client.Player.X, client.Player.Y);
                //if (distance > 9)
                {
                    //ushort X = (ushort)(client.Player.X + Program.GetRandom.Next(2));
                    //ushort Y = (ushort)(client.Player.Y + Program.GetRandom.Next(2));

                    //if (!client.Map.ValidLocation(X, Y))
                    //{
                    //    X = client.Player.X;
                    //    Y = client.Player.Y;
                    //}
                    //client.Pet.monster.X = X;
                    //client.Pet.monster.Y = Y;
                    data->Type = ActionType.Jump;
                    data->ObjId = client.Pet.monster.UID;
                    //data->dwParam = (uint)client.Pet.monster.X << 16 | client.Pet.monster.Y;
                    //data->dwParam3 = client.Pet.monster.X;
                    //data->dwParam4 = client.Pet.monster.Y;
                    //client.Pet.monster.View.SendScreen(msg.ActionCreate(data), client.Pet.monster.GMap);
                }
                //if (!client.Map.ValidLocation(data->Data1Low, data->Data1High))
                //{
                //    /* Make sure the guard is only jumping close to it's owner */
                //    if (Role.Core.GetDistance(client.Player.X, client.Player.Y, data->Data1Low, data->Data1High) >= 12)
                //    {
                //        data->Data1Low = client.Player.X;
                //        data->Data1High = client.Player.Y;
                //    }
                //}
                //else
                //{
                //    data->Data1Low = client.Player.X;
                //    data->Data1High = client.Player.Y;
                //}
                //client.Pet.Owner.Player.Angle = Role.Core.GetAngle(client.Player.X, client.Player.Y, data->Data1Low, data->Data1High);
               

                //InterActionWalk inter = new InterActionWalk()
                //{
                //    Mode = MsgInterAction.Action.Jump,
                //    X = client.Pet.monster.X,
                //    Y = client.Pet.monster.Y,
                //    UID = client.Pet.monster.UID,
                //    //OponentUID = client.Player.UID
                //};
                //client.Player.View.SendView(msg.InterActionWalk(&inter), true);

                
                
            }
            else
            {
                client.Equipment.Show(msg);
                data->ObjId = client.Player.UID;
                client.Player.CompleteLogin = true;
                client.Player.Protect = Time32.Now.AddSeconds(5);


                if (client.Player.GuildID == 0 && client.Player.Level > 20)
                {
                    client.SendSysMesage("Join a guild, and find some companions in this turbulent world!", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
                }
            }
            client.Send(msg.ActionCreate(data));
        }
        [DataAttribute(ActionType.CompleteLogin)]
        public unsafe static void CompleteLogin(Client.GameClient client, ServerSockets.Packet msg, ActionQuery* data)
        {
            client.Player.CompleteLogin = true;
            //MsgProtect.ProGuardHandler.SendCheats(client);
            client.Player.Protect = Time32.Now.AddSeconds(5);
            client.SendSysMesage(Program.UpTimeMsg, MsgMessage.ChatMode.Whisper, MsgMessage.MsgColor.white);
            client.SendSysMesage(Program.OnlineMsg, MsgMessage.ChatMode.Whisper, MsgMessage.MsgColor.white);
            client.Player.UpdateVip(msg);
     
            if (client.Player.Merchant == 1 && client.Player.MerchantApplicationEnd > DateTime.Now)
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    InteractQuery action = new InteractQuery()
                    {
                        AtkType = MsgAttackPacket.AttackID.MerchantProgress,
                    };
                    client.Send(stream);
                }
            }
            if (client.Player.Merchant == 1 && client.Player.MerchantApplicationEnd <= DateTime.Now)
            {
                client.Player.Merchant = 255;
            }
            if (client.Player.GuildID == 0 && client.Player.Level > 20)
            {
                client.SendSysMesage("Join a guild, and find some companions in this turbulent world!", MsgMessage.ChatMode.System, MsgMessage.MsgColor.red);
            }
            //client.Send(GuardShield.MsgGuardShield.RequestJumpInfo(GuardShield.MsgGuardShield.JumpType.SetJumpFar, 18));

        }
    }
}