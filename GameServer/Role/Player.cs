using COServer.EventsLib;
using COServer.Game.MsgServer;
using COServer.Game.MsgTournaments;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace COServer.Role
{
    public unsafe class Player : IMapObj
    {
        internal ushort ArenaDuel_Hits, Ss_Fb_Hits;
        public uint BuyItemS = 0;
        internal ushort Get5OutPoint;
        public bool GetPoint = false;
        public int DepositDbs = 0;
        public int DepositMets = 0;
        public int DepositSMets = 0;
        public int DepositSDbs = 0;
        public int DepositStone1 = 0;
        public int DepositStone2 = 0;
        public int DepositStone3 = 0;
        public int DepositStone4 = 0;
        public int NormalPhoenixGem = 0;
        public int NormalDragonGem = 0;
        public int NormalFuryGem = 0;
        public int NormalRainbowGem = 0;
        public int NormalKylinGem = 0;
        public int NormalVioletGem = 0;
        public int NormalMoonGem = 0;
        public int NormalTortoiseGem = 0;
        public uint TreasureBoxesPoint = 0;

        public bool OfflineMiner { get; set; } = false; // Novo campo para mineração offline

        #region RobotAuto
        public string SocketedItemsStatus => this.LootSocketedItems ? "[Enabled]" : "[Disabled]";
        public string QualityItemsStatus => this.LootQualityItems ? "[Enabled]" : "[Disabled]";
        public string BlessedItemsStatus => this.LootBlessedItems ? "[Enabled]" : "[Disabled]";
        public string DBallsStatus => this.LootDragonBalls ? "[Enabled]" : "[Disabled]";
        public string MeteorsStatus => this.LootMeteorItems ? "[Enabled]" : "[Disabled]";
        public string PlusItemsStatus => this.LootPlusItems ? "[Enabled]" : "[Disabled]";
        public string LootMoneyStatus => this.LootMoney ? "[Enabled]" : "[Disabled]";

        public bool Robot;

        public ushort DirectionChange;
        public ushort RobotX;
        public ushort RobotY;

        public bool LootDragonBalls = true, LootMeteorItems = true, LootSocketedItems = true, LootQualityItems = true,
                    LootBlessedItems = true, LootPlusItems = true, LootMoney = true;
        public DateTime RobotAttack = DateTime.Now;
        #endregion

        public Player Target;
        public bool claimsilvercup = false;
        public bool PlayerHasItemTime = false;
        public bool GuildBeastClaimd = false;
        public bool SpawnGuildBeast = false;
        public DateTime RemoveStamp;
        public bool RemoveAfter;
        public bool SkipBadOre = true;

        public static byte LotteryEntry(byte vipLevel)
        {
            byte chance = 0;
            switch (vipLevel)
            {
                default:
                    chance = 10;
                    break;

                case 6:
                    chance = 10;
                    break;
            }
            return chance;
        }
        public static void Reward(Client.GameClient user, ServerSockets.Packet stream, string Name)
        {
            string mymsg = "";
        jmp:
            byte rand = (byte)Program.GetRandom.Next(0, 1);
            switch (rand)
            {
                case 0://item.
                    {
                        uint[] Items = new uint[]
                        {
                            //Database.ItemType.DragonBall,
                            Database.ItemType.Meteor,
                            Database.ItemType.Meteor,
                            Database.ItemType.Meteor,
                            Database.ItemType.Meteor,
                            Database.ItemType.Meteor,
                            Database.ItemType.Meteor,
                            Database.ItemType.Meteor,
                            Database.ItemType.Meteor,
                            Database.ItemType.Meteor,
                            Database.ItemType.Meteor,
                            Database.ItemType.LotteryTick
                        };
                        uint ItemID = Items[Program.GetRandom.Next(0, Items.Length)];
                        Database.ItemType.DBItem DBItem;
                        if (Database.Server.ItemsBase.TryGetValue(ItemID, out DBItem))
                        {
                            if (user.Inventory.HaveSpace(1))
                                user.Inventory.Add(stream, DBItem.ID);
                            //else
                            //    user.Inventory.AddReturnedItem(stream, DBItem.ID);
                            user.SendSysMesage("You have received " + DBItem.Name + "  from " + Name + "!");

                            //mymsg = user.Player.Name + " got " + DBItem.Name + " from " + Name + "!";
                            //MsgSchedules.SendSysMesage(mymsg, Game.MsgServer.MsgMessage.ChatMode.System, Game.MsgServer.MsgMessage.MsgColor.white);
                        }
                        break;
                    }

                case 1:
                    {
                        uint[] Items = new uint[]
                        {
                            Database.ItemType.LotteryTick,
                            Database.ItemType.ExperiencePotion,
                            Database.ItemType.Meteor,
                            Database.ItemType.Meteor,
                            Database.ItemType.Meteor,
                            Database.ItemType.Meteor,
                            Database.ItemType.Meteor,
                            Database.ItemType.Meteor,
                            Database.ItemType.Meteor,
                            Database.ItemType.Meteor,
                            Database.ItemType.Meteor,
                        };
                        uint ItemID = Items[Program.GetRandom.Next(0, Items.Length)];
                        Database.ItemType.DBItem DBItem;
                        if (Database.Server.ItemsBase.TryGetValue(ItemID, out DBItem))
                        {
                            if (user.Inventory.HaveSpace(1))
                                user.Inventory.Add(stream, DBItem.ID);
                            //else
                            //    user.Inventory.AddReturnedItem(stream, DBItem.ID);
                            user.SendSysMesage("You have received " + DBItem.Name + "  from " + Name + "!");

                            //mymsg = user.Player.Name + " got " + DBItem.Name + " from " + Name + "!";
                            //MsgSchedules.SendSysMesage(mymsg, Game.MsgServer.MsgMessage.ChatMode.System, Game.MsgServer.MsgMessage.MsgColor.white);
                        }
                        break;
                    }
            }
            Database.ServerDatabase.LoginQueue.Enqueue(mymsg);

        }

        public bool TrashGold { get; set; } = true;
        public bool TrashItems { get; set; } = false;
        public byte Quest2rbStage = 0;
        public uint Quest2rbS2Point = 0;
        public byte Quest2rbBossesOrderby = 0;
        public bool OnRemoveLukyAmulet = false;
        public bool OnBluedBird = false;
        public bool OnFerentPill { get { return ContainFlag(MsgUpdate.Flags.Poisoned); } }
        public DateTime FerventPill = new DateTime();
        public DateTime LastDragonPill;
        public bool Mining = false;
        public Time32 NextMine;

        public byte OpenHousePack = 0;
        public ushort ExtraAtributes = 0;
        public DateTime MerchantApplicationEnd;
        private uint _merchant = 0;
        public uint Merchant
        {
            get
            {
                return _merchant;
            }
            set
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    if (value == byte.MaxValue)
                    {
                        var stream = rec.GetStream();
                        Game.MsgServer.MsgUpdate packet = new Game.MsgServer.MsgUpdate(stream, UID, 1);
                        stream = packet.Append(stream, MsgUpdate.DataType.Merchant, value);
                        stream = packet.GetArray(stream);
                        Owner.Send(stream);
                    }
                }
                _merchant = value;
            }
        }
        public bool OnMyOwnServer
        {
            get { return ServerID == Database.GroupServerList.MyServerInfo.ID; }
        }
        public ushort ServerID = 0;
        public ushort SetLocationType = 0;
        public DateTime StampJump = new DateTime();
        public int StampJumpMiliSeconds = 0;
        public bool Invisible = false;
        public DateTime StampSecorSpells = new DateTime();
        public DateTime StampBloodyScytle = new DateTime();
        public DateTime MedicineStamp = new DateTime();
        public bool Rate(int value)
        {
            return value > Program.GetRandom.Next() % 100;
        }
        public ushort NameEditCount = 0;
        public DateTime TaskQuestTimer = new DateTime();
        public uint QuestMultiple = 0;
        public uint VotePoints = 0;
        public DateTime AzurePillStamp = new DateTime();
        public bool StartVote = false;
        public bool CanClaimFreeVip = false;
        public Time32 StartVoteStamp = new Time32();
        public bool OnAttackPotion = false;
        public Time32 OnAttackPotionStamp = new Time32();
        public void ActiveAttackPotion(int Timer)
        {
            OnAttackPotion = true;
            OnAttackPotionStamp = Time32.Now.AddMinutes(Timer);
            AddFlag(MsgUpdate.Flags.Stigma, 1800, true);//60
            Owner.SendSysMesage("Your attack will increase for the next 30 minutes.", MsgMessage.ChatMode.System);
        }
        public bool OnDefensePotion = false;
        public Time32 OnDefensePotionStamp = new Time32();
        public void ActiveDefensePotion(int Timer)
        {
            OnDefensePotion = true;
            OnDefensePotionStamp = Time32.Now.AddMinutes(Timer);
            AddFlag(MsgUpdate.Flags.Shield, 1800, true);//60
            Owner.SendSysMesage("Your defense will increase for the next 30 minutes.", MsgMessage.ChatMode.System);
        }
        public bool AllowDynamic { get; set; }

        public Time32 PickStamp = Time32.Now;
        public bool ActivePick = false;
        public void AddPick(ServerSockets.Packet stream, string Name, ushort timer)
        {
            PickStamp = Time32.Now.AddSeconds(timer);
            Owner.Send(stream.ActionPick(UID, 1, timer, Name));
            ActivePick = true;
            ActionQuery action = new ActionQuery()
            {
                ObjId = UID,
                Type = (ActionType)1165,
                wParam1 = 277,
                wParam2 = 2050
            };
            Owner.Send(stream.ActionCreate(&action));
        }
        public void RemovePick(ServerSockets.Packet stream)
        {
            ActivePick = false;
            Owner.Send(stream.ActionPick(UID, 3, 0, Name));
        }
        public uint IndexInScreen { get; set; }
        public bool IsTrap() { return false; }
        public Time32 LastOnlineStamp = Time32.Now;

        public Time32 LastAttack;
        public Time32 KillCountCaptchaStamp;
        public bool CaptchaShown { get; set; }
        public bool WaitingKillCaptcha;
        public string KillCountCaptcha;
        public uint KillerPkPoints = 0;
        public uint XtremePkPoints = 0;
        public uint DragonWarHits = 0;
        public uint DragonWarScore = 0;
        public uint TeamDeathMacthKills = 0;
        public uint TournamentKills = 0;
        public uint KillersDisCity = 0;
        public uint AparenceType = 0;
        public uint TCCaptainTimes = 0;
        public uint SecurityPassword = 0;
        public uint OnReset = 0;
        public DateTime ResetSecurityPassowrd = new DateTime();
        public Time32 LoginTimer = Time32.Now;
        public Flags.PKMode PreviousPkMode = Flags.PKMode.Capture;

        public unsafe void SetPkMode(Flags.PKMode pkmode)
        {
            PreviousPkMode = PkMode;
            PkMode = pkmode;

            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                ActionQuery action = new ActionQuery()
                {
                    ObjId = UID,
                    dwParam = (uint)PkMode,
                    Type = ActionType.SetPkMode
                };
                Owner.Send(stream.ActionCreate(&action));
            }

        }
        public unsafe void RestorePkMode()
        {
            PkMode = PreviousPkMode;

            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                ActionQuery action = new ActionQuery()
                {
                    ObjId = UID,
                    dwParam = (uint)PkMode,
                    Type = ActionType.SetPkMode
                };
                Owner.Send(stream.ActionCreate(&action));
            }
        }
        public unsafe void UpdateVip(ServerSockets.Packet stream)
        {
            //SendUpdate(stream, VipLevel, MsgUpdate.DataType.VIPLevel, false);
            if (VipLevel > 0)
                Owner.Send(stream.VipStatusCreate(MsgVipStatus.VipFlags.FullVip));
            else
                Owner.Send(stream.VipStatusCreate(MsgVipStatus.VipFlags.None));
        }
        public int CursedTimer = 0;
        public void AddCursed(int time)
        {
            if (time != 0)
            {

                if (ContainFlag(Game.MsgServer.MsgUpdate.Flags.Cursed))
                    RemoveFlag(Game.MsgServer.MsgUpdate.Flags.Cursed);

                CursedTimer += time;
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    SendUpdate(stream, CursedTimer, Game.MsgServer.MsgUpdate.DataType.CursedTimer);
                }
                AddFlag(Game.MsgServer.MsgUpdate.Flags.Cursed, CursedTimer, false, 1);
            }
        }
        public bool Delete = false;
        public ConcurrentDictionary<ushort, FloorSpell.ClientFloorSpells> FloorSpells = new ConcurrentDictionary<ushort, FloorSpell.ClientFloorSpells>();
        public ushort RandomSpell = 0;
        public bool DbTry = false;
        public byte LotteryEntries;

        public Role.Instance.Guild.Member MyGuildMember;
        public Role.Instance.Guild MyGuild;
        public uint TargetGuild = 0;
        uint _extbattle;
        public unsafe uint ExtraBattlePower
        {
            get { return _extbattle; }
            set
            {
                _extbattle = value;
            }
        }
        public unsafe Flags.GuildMemberRank GuildRank = Flags.GuildMemberRank.None;
        public unsafe uint GuildID;
        public int MaxBP()
        {
            if (Nobility == null) return 0;
            switch (Nobility.Rank)
            {
                case Instance.Nobility.NobilityRank.King: return 385;
                case Instance.Nobility.NobilityRank.Prince: return 382;
                case Instance.Nobility.NobilityRank.Duke: return 380;
                default: return 379;
            }
        }
        uint _mentorBp;
        private uint MentorBp
        {
            get { return _mentorBp; }
            set
            {
                ExtraBattlePower -= _mentorBp;
                ExtraBattlePower += value;
                _mentorBp = value;
            }
        }
        public unsafe void SetMentorBattlePowers(uint val, uint mentorPotency)
        {

            MentorBp = val;
            //using (var rec = new ServerSockets.RecycledPacket())
            //{
            //    var stream = rec.GetStream();

            //    Game.MsgServer.MsgUpdate upd = new Game.MsgServer.MsgUpdate(stream, UID, 2);
            //    stream = upd.Append(stream, Game.MsgServer.MsgUpdate.DataType.ExtraBattlePower, val);
            //    stream = upd.Append(stream, Game.MsgServer.MsgUpdate.DataType.ExtraBattlePower, mentorPotency);
            //    stream = upd.GetArray(stream);
            //    Owner.Send(stream);
            //}
        }
        public uint targetTrade = 0;
        public uint FriendRequest = 0;
        public Role.Instance.Associate.MyAsociats MyMentor = null;
        public Role.Instance.Associate.MyAsociats Associate;
        public uint TradePartner = 0;
        public uint TargetFriend = 0;
        public Role.Instance.Nobility Nobility;
        Role.Instance.Nobility.NobilityRank _NobilityRank;
        public Role.Instance.Nobility.NobilityRank NobilityRank
        {
            get { return _NobilityRank; }
            set
            {
                _NobilityRank = value;
            }
        }
        public Instance.Flowers Flowers;
        public unsafe uint FlowerRank;
        public bool OnFairy = false;
        public ushort ActiveDance = 0;
        public Client.GameClient ObjInteraction;
        public unsafe Game.MsgServer.InteractQuery InteractionEffect = default(Game.MsgServer.InteractQuery);
        public bool OnInteractionEffect = false;
        public bool ContainReflect
        {
            get
            {
                if (MsgSchedules.CurrentTournament.Type == TournamentType.FiveNOut)
                    if (MsgSchedules.CurrentTournament.Process != ProcesType.Dead && MsgSchedules.CurrentTournament.InTournament(this.Owner))
                        return false;
                if (Program.ArenaMaps.ContainsValue(this.DynamicID))
                    return false;

                return Database.AtributesStatus.IsWarrior(SecondClass);
            }
        }
        public uint PkWarScore = 0;
        public bool BlackSpot = false;
        public Time32 Stamp_BlackSpot = new Time32();
        public byte UseStamina = 0;
        public Time32 Protect = new Time32();
        private Time32 ProtectedJumpAttack = new Time32();
        internal void ProtectAttack(int StampMiliSeconds)
        {
            Protect = Time32.Now.AddMilliseconds(StampMiliSeconds);
        }
        internal void ProtectJumpAttack(int Seconds)
        {
            ProtectedJumpAttack = Time32.Now.AddSeconds(Seconds);
        }
        internal bool AllowAttack()
        {
            return Time32.Now > Protect && Time32.Now > ProtectedJumpAttack;
        }
        public uint ShieldBlockDamage = 0;
        public uint SpouseUID = 0;
        public Time32 AttackStamp = new Time32();
        public Time32 SpellAttackStamp = new Time32();
        public Time32 ArcherStamp = new Time32();

        public bool OnTransform { get { return TransformationID != 0; } }
        public ClientTransform TransformInfo = null;
        public double PoisonLevel = 0;
        public byte PoisonLevehHu = 0;
        public bool ActivateCounterKill = false;
        public Action<Client.GameClient> MessageOK;
        public Action<Client.GameClient> MessageCancel;
        public Time32 StartMessageBox = new Time32();
        public unsafe void MessageBox(string text, Action<Client.GameClient> msg_ok, Action<Client.GameClient> msg_cancel, int Seconds = 0, Game.MsgServer.MsgStaticMessage.Messages messaj = Game.MsgServer.MsgStaticMessage.Messages.None)
        {
            if (!OnMyOwnServer)
                return;
            if (Program.BlockTeleportMap.Contains(Owner.Player.Map))
            {
                if (Owner != null && Owner.Map != null)
                    Owner.SendSysMesage("You can't use it in " + Owner.Map.Name + " ");
                return;
            }
            if (Owner.Player.Robot)
            {
                Owner.SendSysMesage("[AutoLevelUp] " + text);
                return;
            }
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();

                MessageOK = msg_ok;
                MessageCancel = msg_cancel;
                Game.MsgNpc.Dialog dialog = new Game.MsgNpc.Dialog(Owner, stream);
                dialog.CreateMessageBox(text).FinalizeDialog(true);
                StartMessageBox = Time32.Now.AddHours(24);
                if (Seconds != 0)
                {
                    StartMessageBox = Time32.Now.AddSeconds(Seconds);
                    if (messaj != Game.MsgServer.MsgStaticMessage.Messages.None)
                    {
                        Owner.Send(stream.StaticMessageCreate(messaj, MsgStaticMessage.Action.Append, (uint)Seconds));
                    }
                }
            }
        }
        public void RemoveBuffersMovements(ServerSockets.Packet stream)
        {
            // Intensify = false;

            RemoveFlag(Game.MsgServer.MsgUpdate.Flags.Praying);
            RemoveFlag(Game.MsgServer.MsgUpdate.Flags.CastPray);
        }
        public bool InRapidFire = false;
        public DateTime IntensifyActive;
        public bool InUseIntensify = false;
        public Time32 IntensifyStamp = new Time32();
        public bool Intensify = false;
        public int IntensifyDamage = 0;
        public int BattlePower
        {
            get
            {

                int val = (int)(Level + Reborn * 5 + Owner.Equipment.BattlePower + (byte)NobilityRank + ExtraBattlePower);
                if (val > MaxBP())
                    return MaxBP();

                return Math.Min(385, val);
            }
        }
        public int RealBattlePower
        {
            get
            {
                int val = (int)(Level + Reborn * 5 + Owner.Equipment.BattlePower + (byte)NobilityRank);
                if (val > MaxBP())
                    return MaxBP();

                return val;
            }
        }
        ushort stg;
        public ushort StagimaAttack
        {
            get { return stg; }
            set
            {
                stg = value;
            }
        }
        ushort azuredef;
        public byte AzureShieldLevel;
        public ushort AzureShieldDefence
        {
            get { return azuredef; }
            set
            {
                azuredef = value;
            }
        }
        public Time32 XPCountStamp = new Time32();

        public Time32 XPListStamp = new Time32();
        public ushort Stamina
        {
            get
            {
                if (Program.ArenaMaps.ContainsValue(DynamicID))
                    return 150;
                return _stamina;
            }
            set { _stamina = value; }
        }
        public ushort GetAddStamina()
        {
            if (OnTransform)
                return 2;
            switch (Action)
            {
                case Role.Flags.ConquerAction.Sit:
                    return 12;
                default:
                    return 2;
            }
            //return 100;
        }
        public StatusFlagsBigVector32 BitVector;
        public void AddSpellFlag(Game.MsgServer.MsgUpdate.Flags Flag, int Seconds, bool RemoveOnDead, int StampSeconds = 0)
        {
            if (BitVector.ContainFlag((int)Flag))
                UpdateFlag(Flag, Seconds, RemoveOnDead, StampSeconds);
            else AddFlag(Flag, Seconds, RemoveOnDead, StampSeconds);
        }
        public uint StatusFlag = 0;
        public bool AddFlag(Game.MsgServer.MsgUpdate.Flags Flag, int Seconds, bool RemoveOnDead, int StampSeconds = 0, uint showamount = 0, uint amount = 0)
        {
            if (!BitVector.ContainFlag((int)Flag))
            {
                StatusFlag |= (uint)Flag;
                BitVector.TryAdd((int)Flag, Seconds, RemoveOnDead, StampSeconds);

                UpdateFlagOffset();
                return true;
            }
            return false;
        }
        public bool RemoveFlag(Game.MsgServer.MsgUpdate.Flags Flag)
        {
            if (BitVector.ContainFlag((int)Flag))
            {
                StatusFlag &= (uint)~Flag;
                BitVector.TryRemove((int)Flag);
                UpdateFlagOffset();
                //if (Flag == MsgUpdate.Flags.Oblivion)
                //{
                //    using (var rec = new ServerSockets.RecycledPacket())
                //    {
                //        var stream = rec.GetStream();
                //        Owner.IncreaseExperience(stream, Owner.ExpOblivion);
                //    }
                //    Owner.ExpOblivion = 0;
                //}
                return true;
            }
            return false;
        }
        public bool UpdateFlag(Game.MsgServer.MsgUpdate.Flags Flag, int Seconds, bool SetNewTimer, int MaxTime)
        {
            return BitVector.UpdateFlag((int)Flag, Seconds, SetNewTimer, MaxTime);
        }
        public void ClearFlags()
        {
            BitVector.GetClear();
            UpdateFlagOffset();
        }
        public bool ContainFlag(Game.MsgServer.MsgUpdate.Flags Flag)
        {
            return BitVector.ContainFlag((int)Flag);
        }
        public bool CheckInvokeFlag(Game.MsgServer.MsgUpdate.Flags Flag, Time32 timer32)
        {
            return BitVector.CheckInvoke((int)Flag, timer32);
        }
        public unsafe void UpdateFlagOffset()
        {
            SendUpdate(BitVector.bits, Game.MsgServer.MsgUpdate.DataType.StatusFlag, true);
        }
        public unsafe void SendUpdateHP()
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var MyStream = rec.GetStream();
                Game.MsgServer.MsgUpdate Upd = new Game.MsgServer.MsgUpdate(MyStream, UID, 2);
                MyStream = Upd.Append(MyStream, Game.MsgServer.MsgUpdate.DataType.MaxHitpoints, Owner.Status.MaxHitpoints);
                MyStream = Upd.Append(MyStream, Game.MsgServer.MsgUpdate.DataType.Hitpoints, HitPoints);
                MyStream = Upd.GetArray(MyStream);
                View.SendView(MyStream, true);
                // Owner.Send(MyStream);
            }
        }

        public bool InActions()
        {
            return Action == Role.Flags.ConquerAction.Sit || Action == Role.Flags.ConquerAction.Sad
                || Action == Role.Flags.ConquerAction.Wave || Action == Role.Flags.ConquerAction.Angry
                || Action == Role.Flags.ConquerAction.Bow || Action == Role.Flags.ConquerAction.Cool
                || Action == Role.Flags.ConquerAction.Dance || Action == Role.Flags.ConquerAction.Lie
                || Action == Role.Flags.ConquerAction.Happy || Action == Role.Flags.ConquerAction.Kneel;
        }
        public ushort Dead_X;
        public ushort Dead_Y;
        public bool GetPkPkPoints = false;
        public bool CompleteLogin = false;
        public Time32 GhostStamp;
        public Role.Player KillerCursed;
        public unsafe void Dead(Role.Player killer, ushort DeadX, ushort DeadY, uint KillerUID)
        {
            if (Program.ArenaMaps.ContainsValue((uint)this.DynamicID))
                return;
            if (Program.ArenaMaps.ContainsValue((uint)this.Betting))
                return;

            if (InUseIntensify)
            {
                if (ContainFlag(MsgUpdate.Flags.Intensify))
                    RemoveFlag(MsgUpdate.Flags.Intensify);
                InUseIntensify = false;
            }
            if (Mining)
                Mining = false;
            if (OnTransform && TransformInfo != null)
            {
                TransformInfo.FinishTransform();
            }
            else if (OnTransform)
                TransformationID = 0;

            GhostStamp = Time32.Now;
            Owner.OnAutoAttack = false;
            Owner.SendSysMesage("You're dead.", MsgMessage.ChatMode.System);
            if (killer != null)
            {
                if (killer.Map == 1005 && killer.DynamicID == 999)
                {
                    killer.ArenaDuel_Hits += 1;
                }
                if (killer.Map == 1005 && killer.DynamicID == 9999)
                {
                    killer.Ss_Fb_Hits += 1;
                }
                #region ArenaRoom [Winner]
                if (killer.Map >= 50 && killer.Map <= 53)
                {
                    killer.Money += 2000000;
                    killer.Owner.Teleport(428, 378, 1002);
                    killer.SetPkMode(Role.Flags.PKMode.Capture);
                }
                #endregion
                EventManager.Die(this.Owner, killer.Owner);
            }
            GetPkPkPoints = true;
            if (this.ContainFlag(MsgUpdate.Flags.RedName)
                || this.ContainFlag(MsgUpdate.Flags.BlackName)
                || this.ContainFlag(MsgUpdate.Flags.FlashingName))
                GetPkPkPoints = false;

            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                if (Owner.Pet != null)
                    Owner.Pet.DeAtach(stream);
                if (!Program.FreePkMap.Contains(Map))
                {
                    if (Associate != null && killer != null)
                    {
                        //killer.Associate.AddPKExplorer(killer.Owner, this);
                        Associate.AddEnemy(Owner, killer);
                    }
                }

                Dead_X = DeadX;
                Dead_Y = DeadY;
                DeadStamp = Time32.Now;
                DeathStamp = DateTime.Now;
                HitPoints = 0;

                ClearFlags();
                AddFlag(Game.MsgServer.MsgUpdate.Flags.Dead, StatusFlagsBigVector32.PermanentFlag, true);

                Send(stream.MapStatusCreate(Map, Map, (uint)Owner.Map.TypeStatus));

                if (killer != null)
                {
                    killer.XPCount++;
                    if (killer.Owner.OnAutoAttack)
                        killer.Owner.OnAutoAttack = false;
                    //  KillerCursed = new Player(killer.Owner);
                    //    KillerCursed = killer;

                    if (killer.Map == 1508)
                    {
                        killer.PkWarScore += 10;
                    }
                    if ((Game.MsgTournaments.MsgSchedules.CurrentTournament.Type == Game.MsgTournaments.TournamentType.LastManStand
                       && Game.MsgTournaments.MsgSchedules.CurrentTournament.InTournament(Owner)))
                    {
                        killer.TournamentKills += 1;
                    }



                    if (Game.MsgTournaments.MsgSchedules.CurrentTournament.InTournament(Owner))
                    {
                        if (Game.MsgTournaments.MsgSchedules.CurrentTournament.Type == Game.MsgTournaments.TournamentType.LastManStand)
                        {
                            var tournament = Game.MsgTournaments.MsgSchedules.CurrentTournament as Game.MsgTournaments.MsgLastManStand;
                            tournament.KillSystem.Update(killer.Owner);
                            tournament.KillSystem.CheckDead(this.UID);
                        }
                    }

                    InteractQuery action = new InteractQuery()
                    {
                        UID = killer.UID,
                        X = DeadX,
                        Y = DeadY,
                        AtkType = MsgAttackPacket.AttackID.Death,
                        KillCounter = killer.KillCounter,
                        SpellID = (ushort)(Database.ItemType.IsBow(killer.Owner.Equipment.RightWeapon) ? 5 : 1),
                        OpponentUID = UID,
                    };
                    View.SendView(stream.InteractionCreate(&action), true);


                    if (!Program.NoDropItems.Contains(Map) && !Program.FreePkMap.Contains(Map))
                    {
                        if (Program.ArenaMaps.ContainsValue((uint)this.DynamicID))
                            return;
                        if (Program.ArenaMaps.ContainsValue((uint)this.Betting))
                            return;
                        // Abaixo são listados ID mapas para redname e black name serem mortos com drop equipe

                        if (killer.Map == 1020 && Game.MsgTournaments.MsgSchedules.PoleDomination.IsFinished() ||
                            killer.Map == 1015 && Game.MsgTournaments.MsgSchedules.PoleDominationBI.IsFinished() ||
                            killer.Map == 1011 && Game.MsgTournaments.MsgSchedules.PoleDominationPC.IsFinished() ||
                            killer.Map == 1000 && Game.MsgTournaments.MsgSchedules.PoleDominationDC.IsFinished() ||
                            killer.Map == 1212 ||
                            killer.Map == 1217 ||
                            killer.Map == 1001 ||
                            killer.Map == 1075 ||
                            killer.Map == 1002 ||
                            killer.Map == 1025 ||
                            killer.Map == 1026 ||
                            killer.Map == 1027 ||
                            killer.Map == 1028 ||
                            killer.Map == 1076 )
                        {
                            if (DynamicID == 0)
                            {
                                CheckDropItems(killer, stream);
                                CheckPkPoints(killer);
                            }
                        }
                    }
                    else
                    {
                        if (Program.ArenaMaps.ContainsValue((uint)this.DynamicID))
                            return;
                        if (Program.ArenaMaps.ContainsValue((uint)this.Betting))
                            return;
                        CheckDropHealingItems(killer, stream);
                    }

                }
                else
                {


                    InteractQuery action = new InteractQuery()
                    {
                        UID = KillerUID,
                        X = DeadX,
                        Y = DeadY,
                        AtkType = MsgAttackPacket.AttackID.Death,
                        OpponentUID = UID
                    };
                    View.SendView(stream.InteractionCreate(&action), true);


                    if (!Program.NoDropItems.Contains(Map) && !Program.FreePkMap.Contains(Map))
                    {
                        //if (!(Map == 1011 && DynamicID != 0))
                        if (Program.ArenaMaps.ContainsValue((uint)this.DynamicID))
                            return;
                        if (Program.ArenaMaps.ContainsValue((uint)this.Betting))
                            return;
                        if (DynamicID == 0)
                        {
                            CheckDropItems(killer, stream);
                            if (PKPoints >= 100)
                            {
                                Owner.Teleport(35, 72, 6000, 0, false);
                            }
                        }
                    }
                    else
                    {
                        if (!Program.NoDropItems.Contains(Map) && !Program.FreePkMap.Contains(Map))
                        {
                            if (Program.ArenaMaps.ContainsValue((uint)this.DynamicID))
                                return;
                            if (Program.ArenaMaps.ContainsValue((uint)this.Betting))
                                return;
                            //if (!(Map == 1011 && DynamicID != 0))
                            if (DynamicID == 0)
                            {
                                CheckDropHealingItems(killer, stream);
                            }
                        }
                    }
                }
            }
        }
        public void CheckDropHealingItems(Role.Player killer, ServerSockets.Packet stream)
        {
            try
            {
                ushort x = X;
                ushort y = Y;

                if (x > 5 && y > 5)
                {
                    var inventoryItems = Owner.Inventory.ClientItems.Values.ToArray();
                    // Verifica se há mais de 1 itens no inventário para permitir o drop
                    if (inventoryItems.Length > 2)
                    {
                        // Calcula 20% do total de itens no inventário
                        uint count = (uint)Math.Floor(inventoryItems.Length * 0.1);

                        for (int index = 0; index < count; index++)
                        {
                            try
                            {
                                if (inventoryItems.Length > index && inventoryItems[index] != null)
                                {
                                    var item = inventoryItems[index];

                                    if (item.Position == (ushort)Role.Flags.ConquerItem.Bottle)
                                        continue;

                                    if (item.Locked == 0 && item.Bound == 0 &&
                                        !Database.ItemType.unabletradeitem.Contains(item.ITEM_ID) &&
                                        !Database.ItemType.IsSash(item.ITEM_ID))
                                    {
                                        // Verifica se o item é do tipo "cura"
                                        if (item.ITEM_ID >= 720010 && item.ITEM_ID <= 720017 ||
                                            item.ITEM_ID >= 1000000 && item.ITEM_ID <= 1002050)
                                        {
                                            ushort New_X = (ushort)Program.GetRandom.Next((ushort)(x - 5), (ushort)(x + 5));
                                            ushort New_Y = (ushort)Program.GetRandom.Next((ushort)(y - 5), (ushort)(y + 5));

                                            if (Owner.Map.AddGroundItem(ref New_X, ref New_Y))
                                            {
                                                DropItem(item, New_X, New_Y, stream);
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }



        public void CheckDropItems(Role.Player killer, ServerSockets.Packet stream)
        {
            try
            {
                ushort x = X;
                ushort y = Y;
                uint DropMoney = (uint)(Money + 1 / 12);
                if (DropMoney > 1)
                {
                    DropMoney = (uint)Program.GetRandom.Next(1, (int)DropMoney) / 4;
                    if (Owner.Map.AddGroundItem(ref x, ref y))
                    {


                        Game.MsgServer.MsgGameItem DataItem = new Game.MsgServer.MsgGameItem();
                        DataItem.ITEM_ID = Database.ItemType.MoneyItemID((uint)DropMoney);
                        DataItem.Durability = (ushort)Program.GetRandom.Next(1000, 3000);
                        DataItem.MaximDurability = (ushort)Program.GetRandom.Next(DataItem.Durability, 6000);
                        DataItem.Color = Role.Flags.Color.Red;

                        Game.MsgFloorItem.MsgItem DropItem = new Game.MsgFloorItem.MsgItem(DataItem, x, y, Game.MsgFloorItem.MsgItem.ItemType.Money, DropMoney, DynamicID, Map, UID, false, Owner.Map);
                        if (Owner.Map.EnqueueItem(DropItem))
                        {
                            DropItem.SendAll(stream, Game.MsgFloorItem.MsgDropID.Visible);

                            Money -= DropMoney;
                            SendUpdate(stream, Money, Game.MsgServer.MsgUpdate.DataType.Money);
                        }
                    }
                }
                if (x > 5 && y > 5)
                {
                    var inventoryItems = Owner.Inventory.ClientItems.Values.ToArray();
                    if (inventoryItems.Length / 4 > 1)
                    {
                        uint count = (uint)Program.GetRandom.Next(1, (int)(inventoryItems.Length / 3));

                        for (int index = 0; index < count; index++)
                        {
                            try
                            {
                                if (inventoryItems.Length > index && inventoryItems[index] != null)
                                {
                                    var item = inventoryItems[index];
                                    if (item.Position == (ushort)Role.Flags.ConquerItem.Bottle)
                                        continue;
                                    if (item.Locked == 0 && item.Bound == 0
                                        && !Database.ItemType.unabletradeitem.Contains(item.ITEM_ID) && !Database.ItemType.IsSash(item.ITEM_ID))
                                    {

                                        ushort New_X = (ushort)Program.GetRandom.Next((ushort)(x - 5), (ushort)(x + 5));
                                        ushort New_Y = (ushort)Program.GetRandom.Next((ushort)(y - 5), (ushort)(y + 5));
                                        if (Owner.Map.AddGroundItem(ref New_X, ref New_Y))
                                        {
                                            DropItem(item, New_X, New_Y, stream);
                                        }
                                    }
                                }
                            }
                            catch (Exception e) { Console.WriteLine(e.ToString()); }
                        }

                    }
                }
                if (PKPoints >= 35 && killer != null && !Program.FreePkMap.Contains(Map))
                {
                    int Count_DropItem = (PKPoints >= 35 && PKPoints <= 99) ? 1 : 2;
                    var EquipmentArray = Owner.Equipment.CurentEquip.Where(p => p != null &&
                         p.Position != (ushort)Role.Flags.ConquerItem.Bottle
                         && p.Position != (ushort)Role.Flags.ConquerItem.Garment).ToArray();

                    if (EquipmentArray.Length > 0)
                    {
                        int trying = 0;
                        int Dropable = 0;
                        Dictionary<uint, Game.MsgServer.MsgGameItem> ItemsDrop = new Dictionary<uint, Game.MsgServer.MsgGameItem>();
                        do
                        {
                            if (trying == 14)
                                break;
                            byte ArrayPosition = (byte)Program.GetRandom.Next(0, EquipmentArray.Length);
                            var Element = EquipmentArray[ArrayPosition];
                            if (!ItemsDrop.ContainsKey(Element.UID))
                            {
                                ItemsDrop.Add(Element.UID, Element);
                                Dropable++;
                            }
                            trying++;
                        }
                        while (Dropable < Count_DropItem);

                        //remove equip item--------------


                        foreach (var item in ItemsDrop.Values)
                        {

                            Owner.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsuageID.RemoveEquipment, item.UID, item.Position, 0, 0, 0, 0));

                            Game.MsgServer.MsgGameItem Remover;
                            Owner.Equipment.ClientItems.TryRemove(item.UID, out Remover);
                        }
                        //compute status
                        Owner.Equipment.QueryEquipment();

                        //--------------------------------


                        //add container Item
                        foreach (var item in ItemsDrop.Values)
                            DropItem(item, (ushort)(x + 2), (ushort)(y + 2), stream);
                    }
                    Program.DiscordAPIRedDrop.Enqueue($"```diff\n- 🚩 {Name} Was Captured!\n" +
                                                    $"Killer: {killer.Name}\n" +
                                                    $"Item Dropped: Yes\n" +
                                                    $"Time: {DateTime.Now:yyyy-MM-dd HH:mm}```");
                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        public void DropItem(Game.MsgServer.MsgGameItem item, ushort x, ushort y, ServerSockets.Packet stream)
        {
            Game.MsgFloorItem.MsgItem DropItem = new Game.MsgFloorItem.MsgItem(item, x, y, Game.MsgFloorItem.MsgItem.ItemType.Item, 0, DynamicID, Map, UID, false, Owner.Map);

            if (Owner.Map.EnqueueItem(DropItem))
            {
                DropItem.SendAll(stream, Game.MsgFloorItem.MsgDropID.Visible);
                Owner.Inventory.Update(item, Role.Instance.AddMode.REMOVE, stream, true);
            }
        }
        public bool IsReviveHere = false;
        private void CheckPkPoints(Role.Player killer)
        {
            if (killer.OnMyOwnServer == true && OnMyOwnServer == false)
                return;
            if (Map == 3935)
                return;
            if (Map == 1011 && DynamicID != 0)
                return;
            if (!Program.FreePkMap.Contains(Map))
            {
                if (!this.ContainFlag(Game.MsgServer.MsgUpdate.Flags.RedName) && !this.ContainFlag(Game.MsgServer.MsgUpdate.Flags.BlackName))
                {
                    if (HeavenBlessing > 0)
                    {
                        if (killer.HeavenBlessing > 0)
                        {
                            Owner.LoseDeadExperience(killer.Owner);
                        }
                        else
                        {
                            Owner.SendSysMesage("You have Heavens Blessing, you'll lose no experience!", MsgMessage.ChatMode.System);
                            killer.AddCursed(5 * 60);
                        }
                    }
                    else
                        Owner.LoseDeadExperience(killer.Owner);

                    if (GetPkPkPoints)
                    {
                        // Verificar se o killer tem uma guilda e a guilda inimiga
                        if (killer.MyGuild != null && killer.MyGuildMember != null && MyGuild != null)
                        {
                            if (killer.MyGuild.Enemy.ContainsKey(GuildID))
                            {
                                // Adicionar PK Points ao killer
                                killer.PKPoints += 3;

                                // Checar se as guildas são diferentes
                                if (MyGuild.GuildName != killer.MyGuild.GuildName)
                                {
                                    killer.MyGuild.Info.SilverFund += 5000;
                                    killer.MyGuild.Info.Donation += 2500;
                                }

                                // Enviar mensagem para a guilda do killer
                                if (Database.Server.MapName.ContainsKey(Map))
                                {
                                    killer.MyGuild.SendMessajGuild(
                                        $"The ({killer.GuildRank}) {killer.Name} killed ({GuildRank}) {Name} from guild {MyGuild.GuildName} in {Database.Server.MapName[Map]}",
                                        Game.MsgServer.MsgMessage.ChatMode.Guild,
                                        Game.MsgServer.MsgMessage.MsgColor.yellow
                                    );
                                }
                                return;
                            }
                        }

                        // Verificar se o killer tem o morto como inimigo na lista de associados
                        if (killer.Associate.Contain(Role.Instance.Associate.Enemy, UID))
                        {
                            // Adicionar PK Points ao killer
                            killer.PKPoints += 5;

                            // Se o killer tiver guilda e o morto também, e se as guildas forem diferentes
                            if (killer.MyGuild != null && MyGuild != null && MyGuild.GuildName != killer.MyGuild.GuildName)
                            {
                                killer.MyGuild.Info.SilverFund += 10000;
                                killer.MyGuild.Info.Donation += 5000;
                            }
                            return;
                        }

                        // Caso geral: Adicionar 10 PK Points ao killer, mesmo se ele não tiver guilda
                        killer.PKPoints += 10;

                        // Se o killer tiver guilda e o morto também, e se as guildas forem diferentes
                        if (killer.MyGuild != null && MyGuild != null && MyGuild.GuildName != killer.MyGuild.GuildName)
                        {
                            killer.MyGuild.Info.SilverFund += 10000;
                            killer.MyGuild.Info.Donation += 5000;
                        }
                    }

                }
                else
                {
                    if (PKPoints > 99)//just for black name's
                    {
                        MyKillerName = killer.Name;
                        JailerUID = killer.UID;
                        Owner.Teleport(29, 72, 6000, 0, true);

                        using (var rec = new ServerSockets.RecycledPacket())
                        {
                            var stream = rec.GetStream();
                            Program.SendGlobalPackets.Enqueue(new Game.MsgServer.MsgMessage(Name + " has been captured by " + MyKillerName + " and sent to jail!", MsgMessage.MsgColor.white, MsgMessage.ChatMode.Center).GetArray(stream));

                            Program.DiscordAPIRedDrop.Enqueue($"``[{Name}] has been captured by [" + MyKillerName + "] and sent to jail and drop your itens!``");
                        }
                    }
                }
            }

        }
        public unsafe void Revive(ServerSockets.Packet stream)
        {
            ProtectAttack(10 * 1000);//10 Seconds
            HitPoints = (int)Owner.Status.MaxHitpoints;
            ClearFlags();
            TransformationID = 0;
            XPCount = 0;
            SendUpdate(stream, XPCount, MsgUpdate.DataType.XPCircle);
            Stamina = 100;
            SendUpdate(stream, Stamina, MsgUpdate.DataType.Stamina);
            Send(stream.MapStatusCreate(Map, Map, (uint)Owner.Map.TypeStatus));
            View.SendView(GetArray(stream, false), false);
        }
        public Time32 LastBlessAdd;

        public Time32 PkPointsStamp = new Time32();
        public uint BlessTime = 0;
        public Time32 CastPrayStamp = new Time32();
        public Time32 CastPrayActionsStamp = new Time32();
        public Game.MsgServer.MsgUpdate.Flags UseXPSpell;
        public void OpenXpSkill(Game.MsgServer.MsgUpdate.Flags flag, int Timer, int StampExec = 0)
        {
            XPCount = 0;
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                SendUpdate(stream, XPCount, Game.MsgServer.MsgUpdate.DataType.XPCircle);
            }
            Game.MsgServer.MsgUpdate.Flags UseSpell = OnXPSkill();
            if (UseSpell == Game.MsgServer.MsgUpdate.Flags.Normal)
            {
                KillCounter = 0;
                UseXPSpell = flag;
                AddFlag(flag, Timer, true, StampExec);
            }
            else
            {
                if (UseSpell != flag)
                {
                    RemoveXPSkill();
                    UseXPSpell = flag;
                    AddFlag(flag, Timer, true, StampExec);
                }
                else
                {
                    if (flag == MsgUpdate.Flags.Cyclone || flag == MsgUpdate.Flags.Superman)
                        UpdateFlag(flag, Timer, true, 20);
                    else
                        UpdateFlag(flag, Timer, true, 60);
                }
            }
        }
        public Game.MsgServer.MsgUpdate.Flags OnXPSkill()
        {
            if (ContainFlag(Game.MsgServer.MsgUpdate.Flags.Cyclone))
                return Game.MsgServer.MsgUpdate.Flags.Cyclone;
            else if (ContainFlag(Game.MsgServer.MsgUpdate.Flags.Superman))
                return Game.MsgServer.MsgUpdate.Flags.Superman;
            else if (ContainFlag(Game.MsgServer.MsgUpdate.Flags.Oblivion))
                return Game.MsgServer.MsgUpdate.Flags.Oblivion;
            else if (ContainFlag(Game.MsgServer.MsgUpdate.Flags.FatalStrike))
                return Game.MsgServer.MsgUpdate.Flags.FatalStrike;
            else if (ContainFlag(Game.MsgServer.MsgUpdate.Flags.ShurikenVortex))
                return Game.MsgServer.MsgUpdate.Flags.ShurikenVortex;
            else if (ContainFlag(Game.MsgServer.MsgUpdate.Flags.ChaintBolt))
                return Game.MsgServer.MsgUpdate.Flags.ChaintBolt;
            else if (ContainFlag(Game.MsgServer.MsgUpdate.Flags.BlackbeardsRage))
                return Game.MsgServer.MsgUpdate.Flags.BlackbeardsRage;
            else if (ContainFlag(Game.MsgServer.MsgUpdate.Flags.CannonBarrage))
                return Game.MsgServer.MsgUpdate.Flags.CannonBarrage;
            else
                return Game.MsgServer.MsgUpdate.Flags.Normal;
        }

        public void RemoveXPSkill()
        {
            if (ContainFlag(Game.MsgServer.MsgUpdate.Flags.Cyclone))
                RemoveFlag(Game.MsgServer.MsgUpdate.Flags.Cyclone);
            if (ContainFlag(Game.MsgServer.MsgUpdate.Flags.Superman))
                RemoveFlag(Game.MsgServer.MsgUpdate.Flags.Superman);
            if (ContainFlag(Game.MsgServer.MsgUpdate.Flags.Oblivion))
                RemoveFlag(Game.MsgServer.MsgUpdate.Flags.Oblivion);
            if (ContainFlag(Game.MsgServer.MsgUpdate.Flags.FatalStrike))
                RemoveFlag(Game.MsgServer.MsgUpdate.Flags.FatalStrike);
            if (ContainFlag(Game.MsgServer.MsgUpdate.Flags.ShurikenVortex))
                RemoveFlag(Game.MsgServer.MsgUpdate.Flags.ShurikenVortex);
            if (ContainFlag(Game.MsgServer.MsgUpdate.Flags.ChaintBolt))
                 RemoveFlag(Game.MsgServer.MsgUpdate.Flags.ChaintBolt);
            if (ContainFlag(Game.MsgServer.MsgUpdate.Flags.BlackbeardsRage))
                RemoveFlag(Game.MsgServer.MsgUpdate.Flags.BlackbeardsRage);
            if (ContainFlag(Game.MsgServer.MsgUpdate.Flags.CannonBarrage))
                RemoveFlag(Game.MsgServer.MsgUpdate.Flags.CannonBarrage);
        }
        public void UpdateXpSkill()
        {
            if (UseXPSpell == Game.MsgServer.MsgUpdate.Flags.Cyclone
                || UseXPSpell == Game.MsgServer.MsgUpdate.Flags.Superman)
            {
                if (ContainFlag(UseXPSpell))
                    UpdateFlag(UseXPSpell, 1, false, 30);
            }
        }
        public unsafe void SendScrennXPSkill(IMapObj obj)
        {
            if (OnXPSkill() != Game.MsgServer.MsgUpdate.Flags.Normal)
            {


                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();

                    InteractQuery action = new InteractQuery()
                    {
                        UID = UID,
                        KilledMonster = true,
                        X = X,
                        Y = Y,
                        AtkType = MsgAttackPacket.AttackID.Death,
                        KillCounter = KillCounter
                    };
                    obj.Send(stream.InteractionCreate(&action));

                }
            }
        }
        uint _KO;
        public uint KillCounter
        {
            get { return _KO; }
            set { _KO = value; }
        }
        ushort _xpc;
        public ushort XPCount
        {
            get { return _xpc; }
            set
            {
                _xpc = value;
            }
        }
        public DateTime DeathStamp, PkWarDeadStamp;
        public Time32 DeadStamp = new Time32();
        public ushort Avatar;
        public long WHMoney;
        public Time32 LastWorldMessaj = new Time32();
        public Flags.PKMode PkMode = Flags.PKMode.Capture;
        public Client.GameClient Owner;
        public MapObjectType ObjType { get; set; }
        public RoleView View;
        public unsafe void Send(ServerSockets.Packet msg)
        {
            Owner.Send(msg);
        }
        public Player(Client.GameClient _own)
        {
            AllowDynamic = false;
            this.Owner = _own;
            ObjType = MapObjectType.Player;
            View = new RoleView(Owner);
            BitVector = new StatusFlagsBigVector32(32 * 2);//6 -- 224 //128
        }
        public int Day = 0;
        public unsafe uint UID { get; set; }
        private string _name = "";
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
            }
        }
        public unsafe string ClanName = "";
        string _spouse = "None";
        public string Spouse
        {
            get
            {
                return _spouse;
            }
            set { _spouse = value; }
        }
        public ushort Agility;
        public ushort Vitality;
        public ushort Spirit;
        public ushort Strength;
        public ushort Atributes;
        byte _class;
        public unsafe byte Class
        {
            get { return _class; }
            set
            {
                _class = value;
                if (Owner.FullLoading)
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        SendUpdate(stream, value, Game.MsgServer.MsgUpdate.DataType.Class);
                    }
                }
            }
        }
        public byte IsClaimTheItem = 0;
        public byte FirstRebornLevel;
        public byte SecoundeRebornLevel;
        public unsafe byte FirstClass;
        public unsafe byte SecondClass;
        ushort _level;
        public unsafe ushort Level
        {
            get { return _level; }
            set
            {
                _level = value;
                if (_level >= 130)
                {
                    _level = 130;
                    Experience = 0;
                }
            }
        }
        public unsafe byte Reborn;
        uint _Money;
        public uint Money
        {
            get
            {
                return _Money;
            }
            set
            {
                if (value > 2000000000)
                {
                    uint dif = value - 2000000000;
                    WHMoney += dif;
                    value -= dif;
                    this.Owner.SendSysMesage(dif + " gold has transfered to your warehouse, you can't hold more than 2,000,000,000 in your inventory.", MsgMessage.ChatMode.Monster, MsgMessage.MsgColor.yellow);
                }
                _Money = value;
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    SendUpdate(stream, value, MsgUpdate.DataType.Money);
                }
            }
        }
        uint _cps;
        public uint ConquerPoints
        {
            get { return _cps; }
            set
            {
                if (Owner.FullLoading)
                {
                    if (value > _cps)
                    {
                        uint get_cps = value - _cps;
                        if (get_cps > 59)
                        {
                            string logs = "[CallStack]" + Name + " get " + get_cps + " he have " + _cps + "";
                            Database.ServerDatabase.LoginQueue.Enqueue(logs);
                        }
                    }
                    else
                    {
                        uint lost_cps = _cps - value;
                        if (lost_cps > 59)
                        {
                            string logs = "[CallStack]" + Name + " lost " + lost_cps + " he have " + _cps + "";
                            Database.ServerDatabase.LoginQueue.Enqueue(logs);
                        }
                    }
                }
                _cps = value;
                if (Owner.FullLoading)
                {

                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        Game.MsgServer.MsgUpdate packet = new Game.MsgServer.MsgUpdate(stream, UID, 1);
                        stream = packet.Append(stream, MsgUpdate.DataType.ConquerPoints, value);
                        stream = packet.GetArray(stream);
                        Owner.Send(stream);
                    }
                }
            }
        }
        int _bountCps;
        public int BoundConquerPoints
        {
            get { return _bountCps; }
            set
            {
                _bountCps = value;
                if (Owner.FullLoading)
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        Game.MsgServer.MsgUpdate packet = new Game.MsgServer.MsgUpdate(stream, UID, 1);
                        stream = packet.Append(stream, MsgUpdate.DataType.BoundConquerPoints, value);
                        stream = packet.GetArray(stream);
                        Owner.Send(stream);
                    }
                }
            }
        }
        public ulong Experience;
        public uint VirtutePoints;
        int _minhitpoints;
        public unsafe int HitPoints
        {
            get
            {
                if (Program.ArenaMaps.ContainsValue(DynamicID))
                    return (int)Owner.Status.MaxHitpoints;
                return _minhitpoints;
            }
            set
            {

                if (value > 0) DeadState = false;
                else DeadState = true;
                _minhitpoints = value;
                if (Owner.Team != null)
                {
                    var TeamMember = Owner.Team.GetMember(UID);
                    if (TeamMember != null)
                    {
                        TeamMember.Info.MaxHitpoints = (ushort)Owner.Status.MaxHitpoints;
                        TeamMember.Info.MinMHitpoints = (ushort)value;
                        Owner.Team.SendTeamInfo(TeamMember);
                    }
                }
                if (Owner.FullLoading)
                {
                    SendUpdateHP();
                }
            }
        }
        ushort _mana;
        public unsafe ushort Mana
        {
            get { return _mana; }
            set
            {
                _mana = value;
                if (Owner.FullLoading)
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        SendUpdate(stream, value, Game.MsgServer.MsgUpdate.DataType.Mana);
                    }
                }
            }
        }
        ushort _pkpoints;
        public ushort PKPoints
        {
            get { return _pkpoints; }
            set
            {
                _pkpoints = value;
                if (PKPoints > 99)
                {
                    RemoveFlag(Game.MsgServer.MsgUpdate.Flags.RedName);
                    AddFlag(Game.MsgServer.MsgUpdate.Flags.BlackName, StatusFlagsBigVector32.PermanentFlag, false, 6 * 60);
                }
                else if (PKPoints > 29)
                {
                    AddFlag(Game.MsgServer.MsgUpdate.Flags.RedName, StatusFlagsBigVector32.PermanentFlag, false, 6 * 60);
                    RemoveFlag(Game.MsgServer.MsgUpdate.Flags.BlackName);
                }
                else if (PKPoints < 30)
                {
                    RemoveFlag(Game.MsgServer.MsgUpdate.Flags.RedName);
                    RemoveFlag(Game.MsgServer.MsgUpdate.Flags.BlackName);
                }
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    SendUpdate(stream, PKPoints, Game.MsgServer.MsgUpdate.DataType.PKPoints);
                }
            }
        }
        public unsafe uint QuizPoints;
        DateTime _ExpireVip;
        public DateTime ExpireVip
        {
            get
            {
                var List = Database.ShareVIP.SharedPoll.GetValues().Where(p => p.ShareUID == UID).ToList();
                if (List.Count > 0)
                {
                    var client = List.FirstOrDefault();
                    return client.ShareEnds;
                }
                return _ExpireVip;
            }
            set
            {
                _ExpireVip = value;
            }
        }
        byte _viplevel;
        public byte VipLevel
        {
            get
            {
                var List = Database.ShareVIP.SharedPoll.GetValues().Where(p => p.ShareUID == UID).ToList();
                if (List.Count > 0)
                {
                    var client = List.FirstOrDefault();
                    return client.ShareLevel;
                }
                return _viplevel;
            }
            set
            {
                _viplevel = value;
            }
        }
        ushort face;
        public unsafe ushort Face
        {
            get { return face; }
            set
            {
                face = value;
                if (Owner.FullLoading)
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        SendUpdate(stream, Mesh, Game.MsgServer.MsgUpdate.DataType.Mesh);
                    }
                }
            }
        }
        public byte HairColor
        {
            get
            {
                return (byte)(Hair / 100);
            }
            set
            {
                Hair = (ushort)((value * 100) + (Hair % 100));
            }
        }
        public unsafe ushort Hair;
        public uint PDinamycID { get; set; }
        public uint DynamicID { get; set; }
        uint _mmmap;
        public uint Map
        {
            get { return _mmmap; }
            set { _mmmap = value; }
        }
        ushort xx, yy;
        public ushort dummyX = 0, dummyY = 0;
        public ushort dummyX2 = 0, dummyY2 = 0;
        public byte dummies = 0;
        public unsafe ushort X
        {
            get { return xx; }
            set { Px = X; xx = value; }
        }
        public unsafe ushort Y
        {
            get { return yy; }
            set { Py = Y; yy = value; }
        }
        public void ClearPreviouseCoord()
        {
            Px = 0;
            Py = 0;
        }
        public ushort Px;
        public ushort Py;
        public ushort PMapX;
        public ushort PMapY;
        public uint PMap;
        public int GetMyDistance(ushort X2, ushort Y2)
        {
            return Core.GetDistance(X, Y, X2, Y2);
        }
        public int OldGetDistance(ushort X2, ushort Y2)
        {
            return Core.GetDistance(Px, Py, X2, Y2);
        }
        public bool InView(ushort X2, ushort Y2, byte distance)
        {
            //      Console.WriteLine(Name + " " + OldGetDistance(X2, Y2) + " " + GetMyDistance(X2, Y2));
            return ((OldGetDistance(X2, Y2) > distance) && GetMyDistance(X2, Y2) <= distance);
        }
        public unsafe Flags.ConquerAngle Angle = Flags.ConquerAngle.East;
        public unsafe Flags.ConquerAction Action = Flags.ConquerAction.None;
        public byte ExpBallUsed = 0;
        public byte BDExp = 0;
        public DateTime JoinOnflineTG = new DateTime();
        public Time32 OnlineTrainingTime = new Time32();
        public Time32 ReceivePointsOnlineTraining = new Time32();
        public Time32 HeavenBlessTime = new Time32();
        public int HeavenBlessing = 0;
        public uint OnlineTrainingPoints = 0;
        public uint HuntingBlessing = 0;
        public uint DExpTime = 0;
        public uint RateExp = 2;
        public uint ExpProtection = 0;
        public unsafe void CreateExtraExpPacket(ServerSockets.Packet stream)
        {
            Game.MsgServer.MsgUpdate update = new Game.MsgServer.MsgUpdate(stream, UID, 1);
            stream = update.Append(stream, Game.MsgServer.MsgUpdate.DataType.DoubleExpTimer, new uint[4] { DExpTime, 0, (uint)(RateExp * 100), 0 });
            stream = update.GetArray(stream);
            Owner.Send(stream);
        }
        public void AddHeavenBlessing(ServerSockets.Packet stream, int Time)
        {
            if (!ContainFlag(Game.MsgServer.MsgUpdate.Flags.HeavenBlessing))
                HeavenBlessTime = Time32.Now;
            if (Time > 60 * 60 * 24)
                Owner.SendSysMesage("You`ve received " + Time / (60 * 60 * 24) + " days` blessing time.", Game.MsgServer.MsgMessage.ChatMode.System);
            else
            {
                Owner.SendSysMesage("You`ve received " + (Time / 60) / 60 + " hours` blessing time.", Game.MsgServer.MsgMessage.ChatMode.System);
            }

            bool None = HeavenBlessing == 0;
            HeavenBlessTime = HeavenBlessTime.AddSeconds(Time);

            HeavenBlessing += Time;
            CreateHeavenBlessPacket(stream, None);

            if (MyMentor != null)
            {
                MyMentor.Mentor_Blessing += (uint)(Time / 10000);
                Role.Instance.Associate.Member mee;
                if (MyMentor.Associat.ContainsKey(Role.Instance.Associate.Apprentice))
                {
                    if (MyMentor.Associat[Role.Instance.Associate.Apprentice].TryGetValue(UID, out mee))
                    {
                        mee.Blessing += (uint)(Time / 10000);
                    }
                }
            }

        }
        public void CreateHeavenBlessPacket(ServerSockets.Packet stream, bool ResetOnlineTraining)
        {
            if (HeavenBlessing > 0)
            {
                if (ResetOnlineTraining)
                {
                    ReceivePointsOnlineTraining = Time32.Now.AddMinutes(1);
                    OnlineTrainingTime = Time32.Now.AddMinutes(10);
                }
                AddFlag(Game.MsgServer.MsgUpdate.Flags.HeavenBlessing, Role.StatusFlagsBigVector32.PermanentFlag, false);
                SendUpdate(stream, HeavenBlessing, Game.MsgServer.MsgUpdate.DataType.HeavensBlessing, false);

                SendUpdate(stream, Game.MsgServer.MsgUpdate.OnlineTraining.Show, Game.MsgServer.MsgUpdate.DataType.OnlineTraining);
                if (Map == 601 || Map == 1039)
                    SendUpdate(stream, Game.MsgServer.MsgUpdate.OnlineTraining.InTraining, Game.MsgServer.MsgUpdate.DataType.OnlineTraining);
                SendString(stream, Game.MsgServer.MsgStringPacket.StringID.Effect, true, new string[1] { "bless" });
            }
        }
        public byte GetGender
        {
            get
            {
                if (Body % 10 >= 3)
                    return 0;
                else
                    return 1;
            }
        }
        ushort body;
        public unsafe ushort Body
        {
            get { return body; }
            set
            {
                body = value;
                if (Owner.FullLoading)
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        SendUpdate(stream, Mesh, Game.MsgServer.MsgUpdate.DataType.Mesh, true);
                    }
                }
            }
        }
        private ushort _transformationid;
        public unsafe ushort TransformationID
        {
            get
            {
                return _transformationid;
            }
            set
            {
                _transformationid = value;
                if (Owner.FullLoading)
                {
                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();
                        SendUpdate(stream, Mesh, Game.MsgServer.MsgUpdate.DataType.Mesh, true);
                    }
                }
            }
        }


        public bool Alive { get { return HitPoints > 0; } }
        public unsafe uint Mesh
        {
            get
            {
                //  if (_mesh != 0)
                //    return _mesh;
                //2471671003                     10000000
                return (uint)(TransformationID * 10000000 + Face * 10000 + Body);
            }
            //set { _mesh = value; }
        }

        public int DbCount { get; set; }

        public unsafe void SendUpdate(ServerSockets.Packet stream, long Value, Game.MsgServer.MsgUpdate.DataType datatype, bool scren = false)
        {
            Game.MsgServer.MsgUpdate packet = new Game.MsgServer.MsgUpdate(stream, UID, 1);
            stream = packet.Append(stream, datatype, Value);
            stream = packet.GetArray(stream);
            Owner.Send(stream);
            if (scren)
            {
                View.SendView(stream, false);
            }
        }
        public unsafe void SendUpdate(ServerSockets.Packet stream, Game.MsgServer.MsgUpdate.Flags Flag, uint Time, uint Dmg, uint Level, Game.MsgServer.MsgUpdate.DataType datatype, bool scren = false)
        {
            Game.MsgServer.MsgUpdate packet = new Game.MsgServer.MsgUpdate(stream, UID, 1);
            stream = packet.Append(stream, datatype, (byte)Flag, Time, Dmg, Level);
            stream = packet.GetArray(stream);
            Owner.Send(stream);
            if (scren)
                View.SendView(stream, false);
        }
        public unsafe void SendUpdate(uint[] Value, Game.MsgServer.MsgUpdate.DataType datatype, bool scren = false)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();

                Game.MsgServer.MsgUpdate packet = new Game.MsgServer.MsgUpdate(stream, UID, 1);
                stream = packet.Append(stream, datatype, Value);
                stream = packet.GetArray(stream);
                Owner.Send(stream);
                if (scren)
                    View.SendView(stream, false);
            }
        }
        public bool ShowGemEffects = true;
        public unsafe void SendString(ServerSockets.Packet stream, Game.MsgServer.MsgStringPacket.StringID id, bool SendScreen, params string[] args)
        {
            Game.MsgServer.MsgStringPacket packet = new Game.MsgServer.MsgStringPacket();
            packet.ID = id;
            packet.UID = UID;
            packet.Strings = args;

            if (SendScreen)
                View.SendView(stream.StringPacketCreate(packet), true);
            else
                Owner.Send(stream.StringPacketCreate(packet));
        }
        public unsafe void SendGemString(ServerSockets.Packet stream, Game.MsgServer.MsgStringPacket.StringID id, bool SendScreen, params string[] args)
        {
            Game.MsgServer.MsgStringPacket packet = new Game.MsgServer.MsgStringPacket();
            packet.ID = id;
            packet.UID = UID;
            packet.Strings = args;

            if (SendScreen)
                View.SendView(stream.StringPacketCreate(packet), true, true, ShowGemEffects);
            else
                if (ShowGemEffects)
                Owner.Send(stream.StringPacketCreate(packet));
        }
        public unsafe void SendString(ServerSockets.Packet stream, Game.MsgServer.MsgStringPacket.StringID id, uint _uid, bool SendScreen, params string[] args)
        {
            Game.MsgServer.MsgStringPacket packet = new Game.MsgServer.MsgStringPacket();
            packet.ID = id;
            packet.UID = _uid;
            packet.Strings = args;

            if (SendScreen)
                View.SendView(stream.StringPacketCreate(packet), true);
            else
                Owner.Send(stream.StringPacketCreate(packet));
        }
        public uint HeadId = 0;
        public uint GarmentId = 0;
        public uint ArmorId = 0;
        public uint LeftWeaponId = 0;
        public uint RightWeaponId = 0;
        public ushort ColorArmor = 0;
        public ushort ColorShield = 0;
        public ushort ColorHelment = 0;
        public uint HeadSoul = 0;
        public uint ArmorSoul = 0;
        public uint LeftWeapsonSoul = 0;
        public uint RightWeapsonSoul = 0;
        public uint RealUID = 0;
        public void AddMapEffect(ServerSockets.Packet stream, ushort x, ushort y, params string[] effect)
        {
            Game.MsgServer.MsgStringPacket packet = new Game.MsgServer.MsgStringPacket();
            packet.ID = MsgStringPacket.StringID.LocationEffect;
            packet.X = x;
            packet.Y = y;
            packet.Strings = effect;
            View.SendView(stream.StringPacketCreate(packet), true);
        }
        public void ClearItemsSpawn()
        {
            HeadId = GarmentId = ArmorId = LeftWeaponId = RightWeaponId = 0;
            ColorArmor = ColorShield = ColorHelment = 0;
            HeadSoul = ArmorSoul = LeftWeapsonSoul = RightWeapsonSoul = 0;
        }
        public unsafe ServerSockets.Packet GetArray(ServerSockets.Packet stream, bool WindowsView)
        {
            stream.InitWriter();
            stream.Write(Mesh); //4
            stream.Write(UID);//8

            if (MyGuild != null)
            {
                stream.Write((ushort)GuildID);//12 ushort
                stream.ZeroFill(1);// guild bransh maybe?
                stream.Write((byte)GuildRank);//15
            }
            else
                stream.ZeroFill(4);//15 total

            for (uint x = 0; x < BitVector.bits.Length; x++)
                stream.Write((uint)BitVector.bits[x]);//16


            stream.Write(HeadId);//24

            if (Map == EventsLib.EventManager.deathmatch.map && Owner.DMTeam != 0)
                stream.Write(EventsLib.EventManager.deathmatch.garments[Owner.DMTeam - 1]);
            else if (Map == EventsLib.EventManager.ctb.map)
            {
                if (Owner.TeamColor == EventsLib.CTBTeam.Blue)
                    stream.Write(181805);
                else stream.Write(181625);
            }
            else if (Map == EventsLib.EventManager.killthecaptain.map)
            {
                if (Owner.TeamKillTheCaptain == EventsLib.KillTheCaptainTeams.Blue)
                    stream.Write(181825);
                else stream.Write(181625);
            }
            else if (Map == EventsLib.EventManager.teamfreezewar.map)
            {
                if (Owner.TeamFreeze == EventsLib.FreezeWarTeams.Blue)
                    stream.Write(181825);
                else stream.Write(181625);
            }
            else stream.Write(GarmentId);//28
            stream.Write(ArmorId);//32
            stream.Write(LeftWeaponId);//36
            stream.Write(RightWeaponId);//40
            stream.ZeroFill(4);
            stream.Write(HitPoints);//48
            stream.Write(Hair);//52
            stream.Write(X);//54
            stream.Write(Y);//56
            stream.Write((byte)Angle);//58
            stream.Write((ushort)Action);//59
            stream.Write((byte)Reborn);//61
            stream.Write((ushort)Level);//62
            stream.Write((byte)(WindowsView ? 1 : 0));//64
            stream.Write(ExtraBattlePower);//65
            stream.ZeroFill(8);
            stream.Write((FlowerRank + 10000));//77
            stream.Write((uint)NobilityRank);//81
            stream.Write(ColorArmor);//85
            stream.Write(ColorShield);//87
            stream.Write(ColorHelment);//89
            stream.Write(QuizPoints);//quiz points 91

            //stream.Write(SteedPlus);//102 
            //stream.ZeroFill(2);//string cound 95
            //stream.Write(SteedColor);//108
            //stream.ZeroFill(24);//113 - 138 -- 22
            //if (OnMyOwnServer == false)
            //{
            //    string[] arraystring = new string[]
            //       {
            //           Name, string.Empty, ClanName, string.Empty, string.Empty, MyGuild != null ? MyGuild.GuildName : string.Empty, string.Empty
            //       };

            //    stream.Write(arraystring);
            //}
            //else
            //{

            string[] arraystring = new string[]
               {
                       Name, string.Empty, ClanName, string.Empty, string.Empty
               };

            stream.Write(arraystring);
            //}
            stream.Finalize(Game.GamePackets.SpawnPlayer);
            return stream;

        }
        public uint GetShareBattlePowers(uint target_battlepower)
        {
            return (uint)Database.TutorInfo.ShareBattle(this.Owner, (int)target_battlepower);
        }
        public uint CurrentTreasureBoxes = 0;
        public byte QuestLevel = 0;

        public string Mac = "";
        public string NewUser = "";
        //public Game.MsgTournaments.MsgFreezeWar.Team.TeamType FreezeTeamType;
        public DateTime LastMove;
        public DateTime LastSuccessCaptcha = DateTime.Now;
        public bool LootExpBall = true, LootDragonBall = true;
        private ushort _stamina;
        public int TotalHits, Hits, Chains, MaxChains, Betting;
        public byte MyHits;
        public int NextCaptcha = 5;
        public int KillTheCaptain;
        public uint OnlinePoints;
        public DateTime SummonGuild;

        public void RemoveSpecialGarment(ServerSockets.Packet stream)
        {
            GarmentId = 0;
               //Owner.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsuageID.Unequip, uint.MaxValue - 1, (ushort)Flags.ConquerItem.Garment, 0, 0, 0, 0));

            MsgGameItem item;
            if (Owner.Equipment.TryGetEquip(Flags.ConquerItem.Garment, out item))
            {
                Owner.Send(stream.ItemUsageCreate(MsgItemUsuagePacket.ItemUsuageID.Equip, item.UID, (ushort)Flags.ConquerItem.Garment, 0, 0, 0, 0));
                item.Mode = Flags.ItemMode.AddItem;
                item.Send(Owner, stream);
            }
            Owner.Equipment.QueryEquipment();
        }

        internal bool SendAllies = true;
        internal int OblivionMobs;
        internal bool DeadState = true;
        internal DateTime ShieldBlockEnd = DateTime.Now;
        internal DateTime JumpingStamp, PreviousJump;
        internal int FiveNOut = 0;
        internal int SkyFight = 0;
        internal bool BlockMovementCo = false;
        internal DateTime BlockMovement;
        internal int DragonPills = 0;
        public uint JailerUID = 0;
        public string MyKillerName;

        internal void SolveCaptcha()
        {
            WaitingKillCaptcha = false;
            KillCountCaptcha = "";
            LastSuccessCaptcha = DateTime.Now;
            NextCaptcha = Role.Core.Random.Next(15, 30);
        }

        internal bool IsBoy()
        {
            return Role.Core.IsBoy(Body);
        }
        public bool IsGirl()
        {
            return Role.Core.IsGirl(Body);
        }
    }
}
