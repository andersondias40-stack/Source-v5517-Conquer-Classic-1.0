#define ItemTime
using COServer.Game.MsgServer;
using COServer.Game.MsgTournaments;

using System;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using COServer.EventsLib;
using static COServer.Game.MsgServer.MsgItemUsuagePacket;
using System.Linq;
using static COServer.Game.MsgServer.MsgMessage;

namespace COServer.Client
{
    [Flags]
    public enum ServerFlag : ushort
    {
        None = 0,
        AcceptLogin = 1 << 0,
        CreateCharacter = 1 << 1,
        CreateCharacterSucces = 1 << 2,
        LoginFull = 1 << 3,
        SetLocation = 1 << 4,
        OnLoggion = 1 << 5,
        QueuesSave = 1 << 6,
        RemoveSpouse = 1 << 7,
        Disconnect = 1 << 8,
        UpdateSpouse = 1 << 9

    }
    public unsafe class GameClient
    {
        public TQHandle.ClientState TQState = null;
        public bool LoaderDisconnect = false;
        public DateTime ProtectPingTime = DateTime.Now;
        public bool PrintProcesses = false;
        public DateTime SendPingStamp = DateTime.Now;
        public DateTime LastCheckTime = DateTime.Now;
        public List<string> ProcessesList = new List<string>();
        public DateTime SendCheckStamp = DateTime.Now;
        public int Get5Out_points { get; set; }
        public EventsLib.BaseEvent EventBase;
        public uint CountVote = 0;
        public DateTime JumpAuto = DateTime.Now;
        public string AutoTargetName = "";
        public uint TempGarmentID = 0;
        internal int ElitePKPoints;
        public int Npoints { get; set; }
        public int FoundsToTransfer { get; set; } = 0;
        public int CTBScore;
        public CTBTeam TeamColor = CTBTeam.Blue;
        internal uint DMGarmentUid;
        internal ushort DMScore;
        internal byte DMTeam;
        public int DragonwarPts;
        public bool isDragonKing = false;
        public DateTime DragonWarStamp;
        internal FreezeWarTeams TeamFreeze;
        public bool HasBag = false;
        public bool RedTeam = false;
        public bool BlueTeam = false;
        public bool Invitations = false;
        public int TotalKillsGW = 0;
        public DateTime DeathHit = DateTime.Now;
        internal int FreezewarPoints;
        public DateTime FrozenStamp;
        ushort _kg = 0;
        public ushort KingOfTheHill
        {
            get { return _kg; }
            set
            {
                _kg = value;
                if (value != 0)
                    SendSysMesage("Your current points : " + value);
            }
        }
        public string DMTeamString()
        {
            switch (DMTeam)
            {
                case 1: return "White Team";
                case 2: return "Red Team";
                case 3: return "Blue Team";
                case 4: return "Black Team";
            }

            return "UNKNOWN";
        }
        public string AccountName(string name)
        {
            string username = "";
            using (var conn = new MySqlConnection(PayPalHandler.ConnectionString))
            using (var cmd = new MySqlCommand("select Username from accounts where EntityID='" + Player.UID + "'", conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        username = reader.GetString("Username");
                    }
                }
            }
            return username;
        }

        ////////LoaderINFO///////
        public uint EncryptTokenSpell;
        public DateTime LoaderTime = DateTime.Now;
        public uint StampThreadMemory = 0;
        public uint StampThreadTimer = 0;
        public bool TerminateLoader = false;
        public bool ActiveClient = false;
        public List<string> OpenedProcesses = new List<string>();

        public bool OnAutoHunt = false;
        public bool RequestSend;
        public int AutoHunting = 0;
        public Role.MonsterPet Pet;
        public Time32 AutoHunt = Time32.Now.AddMilliseconds(100);
        public Time32 AutoHuntAttack = Time32.Now.AddMilliseconds(100);
        public Time32 BuffersStamp = Time32.Now.AddMilliseconds(MapGroupThread.User_Buffers);
        public Time32 StaminStamp = Time32.Now.AddMilliseconds(MapGroupThread.User_Stamina);
        public Time32 AttackStamp = Time32.Now.AddMilliseconds(MapGroupThread.User_AutoAttack);
        public Time32 XPCountStamp = Time32.Now.AddMilliseconds(MapGroupThread.User_StampXPCount);
        public Time32 CheckSecoundsStamp = Time32.Now.AddMilliseconds(MapGroupThread.User_CheckSecounds);
        public Time32 CheckItemsView = Time32.Now.AddMilliseconds(MapGroupThread.User_CheckItems);
        public Time32 CheckItemTimeStamp = Time32.Now.AddMilliseconds(MapGroupThread.User_ItemTIme);
        public Time32 user_minig = Time32.Now.AddMilliseconds(MapGroupThread.user_minig);


        public void Disconnect(Client.GameClient client)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                try
                {

                    if (client.Team != null)
                        client.Team.Remove(client, true);

                    if (client.IsVendor)
                        client.MyVendor.StopVending(stream);

                    if (client.InTrade)
                        client.MyTrade.CloseTrade();

                    if (client.Player.MyGuildMember != null)
                        client.Player.MyGuildMember.IsOnline = false;

                    if (client.Pet != null)
                        client.Pet.DeAtach(stream);


                    if (client.Player.ObjInteraction != null)
                    {
                        client.Player.InteractionEffect.AtkType = COServer.Game.MsgServer.MsgAttackPacket.AttackID.InteractionStopEffect;

                        Game.MsgServer.InteractQuery action = COServer.Game.MsgServer.InteractQuery.ShallowCopy(client.Player.InteractionEffect);

                        client.Send(stream.InteractionCreate(&action));

                        client.Player.ObjInteraction.Player.OnInteractionEffect = false;
                        client.Player.ObjInteraction.Player.ObjInteraction = null;
                    }


                    client.Player.View.Clear(stream);


                }
                catch (Exception e)
                {
                    Console.WriteException(e);
                    client.Player.View.Clear(stream);
                }
                finally
                {
                    client.ClientFlag &= ~Client.ServerFlag.LoginFull;
                    client.ClientFlag |= Client.ServerFlag.Disconnect;
                    client.ClientFlag |= Client.ServerFlag.QueuesSave;
                    Database.ServerDatabase.LoginQueue.TryEnqueue(client);
                }

                try
                {
                    client.Player.Associate.OnDisconnect(stream, client);

                    client.Player.Associate.Online = false;
                    lock (client.Player.Associate.MyClient)
                        client.Player.Associate.MyClient = null;
                    client.Player.Associate.OnlineApprentice.Clear();
                    client.Map?.Denquer(client);
                    //done remove
                }
                catch (Exception e) { Console.WriteLine(e.ToString()); }
            }
        }

        internal static unsafe GameClient CharacterFromName(string p)
        {
            foreach (var x in Database.Server.GamePoll.Values)
            {
                if (p == x.Player.Name)
                    return x;
            }
            return null;
        }
        #region MsgTick
        public Time32 _ServersidePing;
        public int ServersidePing;
        public int AServersidePing;
        public bool WaitingPing;
        public long øServersidePing;
        public int ΩServersidePing;
        public long ƒServersidePing;
        public int LaggingCount;
        public Time32 LastLag;

        public int ServerLatency
        {
            get
            {
                if (WaitingPing) return (Time32.Now - _ServersidePing).AllMilliseconds;
                return (int)ƒServersidePing;
            }
        }
        public bool IsLagging()
        {
            return WaitingPing && ServerLatency > 3 * ƒServersidePing;
        }
        #endregion
        public int AdjustWeaponDamage(int damage)
        {
            //     damage = Game.MsgServer.AttackHandler.Calculate.Base.MulDiv(damage, GetDefense2(), DefaultDefense);

            int type1 = 0, type2 = 0;
            if (Player.LeftWeaponId > 0)
                type1 = (int)(Player.LeftWeaponId / 1000);
            if (Player.RightWeaponId > 0)
                type2 = (int)(Player.RightWeaponId / 1000);

            if (type1 > 0 && MyProfs.ClientProf.ContainsKey((ushort)type1) && MyProfs.ClientProf[(ushort)type1].Level > 12)
            {
                damage = (int)(damage * (1 + ((20 - MyProfs.ClientProf[(ushort)type1].Level) / 100f)));
            }
            else if (type2 > 0 && MyProfs.ClientProf.ContainsKey((ushort)type2) && MyProfs.ClientProf[(ushort)type2].Level > 12)
            {
                damage = (int)(damage * (1 + ((20 - MyProfs.ClientProf[(ushort)type2].Level) / 100f)));
            }

            return damage;
        }
        public object TimerSyncRoot = new object();
        public IDisposable[] TimerSubscriptions;
        public uint MobsKilled = 0, TotalMobsKilled = 0, TotalMobsLevel = 0;
        public DateTime StartQuizTimer = new DateTime();
        public int QuizRank = 0;
        public int GetQuizTimer()
        {
            TimeSpan now = new TimeSpan(DateTime.Now.Ticks);
            TimeSpan old = new TimeSpan(StartQuizTimer.Ticks);
            return (int)(now.TotalSeconds - old.TotalSeconds);
        }
        public ushort QuizShowPoints = 0;
        public byte RightAnswer = 1;
        public Game.MsgNpc.Npc OnRemoveNpc;
        public int TerainMask = 0;

        public ulong ExpOblivion = 0;
        public byte TRyDisconnect = 1;

        public bool IsInSpellRange(uint UID, byte range)
        {
            if (range == 0)
                range = 10;
            Role.IMapObj target;
            if (Player.View.TryGetValue(UID, out target, Role.MapObjectType.Monster))
            {
                return Role.Core.GetDistance(Player.X, Player.Y, target.X, target.Y) <= range;
            }
            else if (Player.View.TryGetValue(UID, out target, Role.MapObjectType.Player))
            {
                return Role.Core.GetDistance(Player.X, Player.Y, target.X, target.Y) <= range;
            }
            else if (Player.View.TryGetValue(UID, out target, Role.MapObjectType.SobNpc))
            {
                return Role.Core.GetDistance(Player.X, Player.Y, target.X, target.Y) <= range;
            }
            return false;
        }
        internal void LoseDeadExperience(Client.GameClient killer)
        {
            if (Fake)
                return;

            if (Player.Level >= 130)
                return;

            if (Player.ExpProtection > 0)
                return;

            var nextlevel = Database.Server.LevelInfo[Database.DBLevExp.Sort.User][(byte)(Player.Level)];
            if (nextlevel.Experience == 0)
            {
                return;//player level 140. Error divide by 0;
            }
            ulong loseexp = (ulong)((Player.Experience * (uint)(nextlevel.UpLevTime * nextlevel.MentorUpLevTime)) / nextlevel.Experience);
            double LoseExpPercent = (double)((double)loseexp / (double)nextlevel.Experience);

            if (Player.Experience > loseexp)
            {
                Player.Experience -= loseexp;//exp;
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    Player.SendUpdate(stream, (long)Player.Experience, Game.MsgServer.MsgUpdate.DataType.Experience);
                }
            }

            // to do : increase kill experince
            if (killer.Player.Level < Player.Level)
            {
                var killernextlevel = Database.Server.LevelInfo[Database.DBLevExp.Sort.User][(byte)(killer.Player.Level)];
                if (killernextlevel.Experience == 0)
                {
                    return;//player level 140. Error divide by 0;
                }
                double GetExp = (double)((double)100 / (double)killernextlevel.Experience) * (double)(loseexp * 10);
                killer.Player.Experience += (uint)GetExp;
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    killer.Player.SendUpdate(stream, (long)killer.Player.Experience, Game.MsgServer.MsgUpdate.DataType.Experience);
                }
            }
        }
        unsafe internal bool UpdateSpellSoul(ServerSockets.Packet stream, Role.Flags.SpellID SpellID, byte MaxLevel)
        {
            Game.MsgServer.MsgSpell spell;
            if (MySpells.ClientSpells.TryGetValue((ushort)SpellID, out spell))
            {
                if (spell.SoulLevel >= MaxLevel)
                {
                    CreateBoxDialog("Sorry, your spell " + SpellID.ToString() + " is max level.");
                    return false;
                }

                ActionQuery action = new ActionQuery()
                {
                    ObjId = Player.UID,
                    dwParam = (ushort)SpellID,
                    Type = ActionType.RemoveSpell
                };
                Send(stream.ActionCreate(&action));

                spell.SoulLevel++;
                spell.UseSpellSoul = spell.SoulLevel;

                Send(stream.SpellCreate(spell));

                return true;
            }
            else
            {
                CreateBoxDialog("Sorry, you don't have the spell " + SpellID.ToString() + ".");
                return false;
            }
        }
        public const int DefaultDefense = 10000;
        public Role.Instance.House MyHouse;
        //For anti proxy --------------
        public ushort MoveNpcMesh;
        public uint MoveNpcUID;
        public uint UseItem = 0;
        //-----------------------------
        public Role.Instance.DemonExterminator DemonExterminator;
        public uint RebornGem = 0;
        public Role.Instance.Vendor MyVendor;
        public bool IsVendor
        {
            get
            {
                if (MyVendor != null)
                    return MyVendor.InVending;
                return false;
            }
        }
        public Role.Instance.Trade MyTrade;
        public bool ProjectManager
        {
            get
            {
                return Player.Name.Contains("[PM]");
            }
        }
        public bool PlayerHelper
        {
            get
            {
                return Player.Name.Contains("[PH]");
            }
        }
        public bool InTrade
        {
            get
            {
                if (MyTrade != null)
                    return MyTrade.WindowOpen;
                return false;
            }
        }
        public bool FullLoading = false;
        public uint Vigor;
        public ulong GainExperience(double Experience, ushort targetlevel)
        {
            var deltaLevel = Player.Level - targetlevel;
            if (deltaLevel >= 3)//green
            {
                if (deltaLevel >= 3 && deltaLevel <= 5)
                    Experience *= .7;
                else if (deltaLevel > 5 && deltaLevel <= 10)
                    Experience *= .2;
                else if (deltaLevel > 10 && deltaLevel <= 20)
                    Experience *= .1;
                else if (deltaLevel > 20)
                    Experience *= .05;
            }
            else if (deltaLevel < -15)
                Experience *= 1.8;
            else if (deltaLevel < -8)
                Experience *= 1.5;
            else if (deltaLevel < -5)
                Experience *= 1.3;

            return (ulong)Experience;
        }
        #region XPLevel lvl130 Level130 uplevel
        public void IncreaseExperience(ServerSockets.Packet stream, uint Experience, Role.Flags.ExperienceEffect effect = Role.Flags.ExperienceEffect.None, bool PowerExp = false)
        {
            if (Player.CursedTimer > 2)
                return;
            if (Player.ContainFlag(MsgUpdate.Flags.Ghost)) return;
            if (Player.Level < 130)
            {
                if (effect != Role.Flags.ExperienceEffect.None)
                {
                    Player.SendString(stream, Game.MsgServer.MsgStringPacket.StringID.Effect, true, new string[1] { effect.ToString() });
                }
                if (Player.Map == 1300 && Player.Level >= 130 * 4)
                    Experience *= 50;
                else if (Player.Level < 130)
                    Experience *= Program.ServerConfig.UserExpRate ;

                if (Player.Owner.GemValues(Role.Flags.Gem.NormalRainbowGem) > 0)
                    Experience += Experience * Player.Owner.GemValues(Role.Flags.Gem.NormalRainbowGem) / 100;

                if (Player.Map == 1039)
                    Experience /= 40;

                if (Player.Level >= 130)
                    Experience /= 20;

                if (Player.DExpTime > 0)
                    Experience *= Player.RateExp;

                //if (Database.AtributesStatus.IsWater(Player.Class))
                //    Experience /= 2;
                //{
                //    if (Player.HeavenBlessing > 0)
                //        Experience += (uint)(Experience * 2 / 100);
                //}
                if (Player.HeavenBlessing > 0)
                    Experience += (uint)(Experience * 3 / 100);
                if (Player.BlessTime > 0 && Role.Core.Rate(1) && Player.Map != 1039)
                {
                    Experience += (uint)(Experience * 5);
                    Player.Owner.SendSysMesage("Lucky! You got x5 Experience.", MsgMessage.ChatMode.Action);
                }
                Player.Experience += (ulong)Experience;
                while (Player.Experience >= Database.Server.LevelInfo[Database.DBLevExp.Sort.User][(byte)Player.Level].Experience)
                {
                    Player.Experience -= Database.Server.LevelInfo[Database.DBLevExp.Sort.User][(byte)Player.Level].Experience;
                    ushort newlev = (ushort)(Player.Level + 1);
                    UpdateLevel(stream, newlev);
                    if (Player.Level >= 130)
                    {
                        Player.Experience = 0;
                        break;
                    }
                }
                UpdateRebornLastLevel(stream);

                Player.SendUpdate(stream, (long)Player.Experience, Game.MsgServer.MsgUpdate.DataType.Experience, false);

            }
        }
        public void UpdateRebornLastLevel(ServerSockets.Packet stream)
        {
            if (Player.Reborn > 0)
            {
                {
                    if (Player.Reborn == 1)
                    {
                        if (Player.Level >= 130 && Player.Level < Player.FirstRebornLevel)
                        {
                            UpdateLevel(stream, Player.FirstRebornLevel, true);
                        }
                    }
                    else if (Player.Reborn == 2)
                    {
                        if (Player.Level >= 130 && Player.Level < Player.SecoundeRebornLevel)
                            UpdateLevel(stream, Player.SecoundeRebornLevel, true);
                    }
                }
            }
        }
        #endregion
        public string InfoLevelUpdate(double amount = 600)
        {
            ulong ReceiveExperience = GainExpBall(amount, false, Role.Flags.ExperienceEffect.None, true);
            ulong MyExperince = Player.Experience;
            byte MyLevel = (byte)Player.Level;
            MyExperince += ReceiveExperience;
            while (MyExperince >= Database.Server.LevelInfo[Database.DBLevExp.Sort.User][(byte)MyLevel].Experience)
            {
                MyExperince -= Database.Server.LevelInfo[Database.DBLevExp.Sort.User][(byte)MyLevel].Experience;
                MyLevel++;
            }
            float Percentaj = (float)(Database.Server.LevelInfo[Database.DBLevExp.Sort.User][(byte)MyLevel].Experience / MyExperince);
            return "" + MyLevel + " (" + Percentaj + "%)";
        }
        public ulong GainExpBall(double amount = 600, bool sendMsg = false, Role.Flags.ExperienceEffect effect = Role.Flags.ExperienceEffect.None
            , bool JustCalculate = false, bool mentorexp = true)
        {
            if (Player.Level >= 130)
                return 0;
            if (sendMsg)
            {
                SendSysMesage("You've gained experience worth " + (amount * 1.0) / 600 + " exp ball(s).", Game.MsgServer.MsgMessage.ChatMode.System);
            }
            if (effect != Role.Flags.ExperienceEffect.None)
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    Player.SendString(stream, Game.MsgServer.MsgStringPacket.StringID.Effect, true, new string[1] { effect.ToString() });
                }
            }
            var LevelDBExp = Database.Server.LevelInfo[Database.DBLevExp.Sort.User][(byte)Player.Level];
            if (LevelDBExp == null)
                return 0;

            var ReceiveExp = (long)Player.Experience * LevelDBExp.UpLevTime / (double)LevelDBExp.Experience;
            if (ReceiveExp < 0 && Player.Level == 130)
            {
                ReceiveExp = 0;
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    UpdateLevel(stream, 130, true);
                }
                return 0;
            }
            else
                ReceiveExp += amount;

            byte IncreaseLevel = (byte)Player.Level;
            //LevelDBExp = Database.Server.LevelInfo[Database.DBLevExp.Sort.User][IncreaseLevel];
            var times = LevelDBExp.UpLevTime;

            while (IncreaseLevel < 130)
            {
                if (ReceiveExp < times)
                    break;
                ReceiveExp -= times;
                IncreaseLevel++;

                LevelDBExp = Database.Server.LevelInfo[Database.DBLevExp.Sort.User][IncreaseLevel];
                if (LevelDBExp == null)
                    break;

                times = LevelDBExp.UpLevTime;
            }
            if (times < 1) return 0;
            if (!JustCalculate)
            {
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    UpdateLevel(stream, IncreaseLevel, false, mentorexp);
                }
            }
            ReceiveExp /= times;

            LevelDBExp = Database.Server.LevelInfo[Database.DBLevExp.Sort.User][(byte)Player.Level];
            if (LevelDBExp == null)
                return 0;

            ulong CalculateEXp = (ulong)(ReceiveExp * LevelDBExp.Experience);
            if (!JustCalculate)
            {
                Player.Experience = CalculateEXp;
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    Player.SendUpdate(stream, (long)Player.Experience, Game.MsgServer.MsgUpdate.DataType.Experience, false);
                }
                using (var rec = new ServerSockets.RecycledPacket())
                {
                    var stream = rec.GetStream();
                    UpdateRebornLastLevel(stream);
                }
            }
            return CalculateEXp;
        }
        public ulong CalcExpBall(double amount, out ushort nextlevel)
        {
            if (Player.Level >= 130)
            { nextlevel = 0; return 0; }

            var LevelDBExp = Database.Server.LevelInfo[Database.DBLevExp.Sort.User][(byte)Player.Level];
            if (LevelDBExp == null)
            { nextlevel = 0; return 0; }

            var ReceiveExp = (long)Player.Experience * LevelDBExp.UpLevTime / (double)LevelDBExp.Experience;
            ReceiveExp += amount;

            byte IncreaseLevel = (byte)Player.Level;
            //LevelDBExp = Database.Server.LevelInfo[Database.DBLevExp.Sort.User][IncreaseLevel];
            var times = LevelDBExp.UpLevTime;

            while (IncreaseLevel < 130)
            {
                if (ReceiveExp < times)
                    break;
                ReceiveExp -= times;
                IncreaseLevel++;

                LevelDBExp = Database.Server.LevelInfo[Database.DBLevExp.Sort.User][IncreaseLevel];
                if (LevelDBExp == null)
                    break;

                times = LevelDBExp.UpLevTime;
            }

            if (times < 1) { nextlevel = IncreaseLevel; return 0; }
            ReceiveExp /= times;

            LevelDBExp = Database.Server.LevelInfo[Database.DBLevExp.Sort.User][(byte)Player.Level];
            if (LevelDBExp == null)
            { nextlevel = IncreaseLevel; return 0; }

            ulong CalculateEXp = (ulong)(ReceiveExp * LevelDBExp.Experience);

            nextlevel = IncreaseLevel;
            return CalculateEXp;
        }
        public object ItemSyncRoot = new object();

        public unsafe Game.MsgServer.InteractQuery AutoAttack = default(Game.MsgServer.InteractQuery);
        private bool _OnAutoAttack = false;
        public bool OnAutoAttack
        {
            get { return _OnAutoAttack; }
            set
            {
                _OnAutoAttack = value;
            }

        }
        public uint AcceptedGuildID = 0;
        public uint ConnectionUID = 0;
        public Game.MsgServer.MsgLoginClient OnLogin = default(Game.MsgServer.MsgLoginClient);
        public uint ActiveNpc = 0;
        public bool NewPlayer = false;
        public ServerFlag ClientFlag = ServerFlag.None;
        public Role.Player Player;
        public uint DbKilled = 0, Drop_Meteors = 0, Drop_Stone = 0;
        public Cryptography.GameCryptography Cryptography;
        public Cryptography.DHKeyExchange.ServerKeyExchange DHKeyExchance;

        private ServerSockets.SecuritySocket SocketNULL;
        public ServerSockets.SecuritySocket Socket
        {
            get
            {

                return SocketNULL;
            }
            set
            {

                SocketNULL = value;
            }
        }
        public Role.GameMap Map = null;
        public Role.Instance.Team Team = null;
        public ushort[] Gems = new ushort[13];
        public void AddGem(Role.Flags.Gem gem, ushort value)
        {
            if (value == 15 || value == 10 || value == 5)
            {
                if (gem == Role.Flags.Gem.NormalDragonGem || gem == Role.Flags.Gem.RefinedDragonGem || gem == Role.Flags.Gem.SuperDragonGem)
                    Status.PhysicalPercent += value;
                else
                    Status.MagicPercent += value;
            }
            Gems[(byte)((byte)gem / 10)] += value;
        }
        public uint GemValues(Role.Flags.Gem gem)
        {
            return Gems[(byte)((byte)gem / 10)];
        }
        public uint AjustDefense
        {
            get
            {
                uint defence = (uint)(Status.Defence);
                uint nDefence = 0;
                if (Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Shield) || Player.OnDefensePotion)
                {
                    nDefence += (uint)Game.MsgServer.AttackHandler.Calculate.Base.MulDiv((int)defence, 120, 100) - defence;////(uint)(defence * 1.3);// + 30% dmg
                }
                return defence + nDefence;
            }
        }

        public int KillTheCaptainPoints { get; internal set; }
        public DateTime bombStamp;
        bool _hasbomb;
        public bool HasTheBomb
        {
            get { return _hasbomb; }
            set
            {
                _hasbomb = value;
                if (value)
                {
                    // Add effect
                    Player.AddFlag(MsgUpdate.Flags.Poisoned, 60 * 60 * 24 * 30, false);
                    bombStamp = DateTime.Now.AddSeconds(10);
                }
                else
                {
                    Player.RemoveFlag(MsgUpdate.Flags.Poisoned);
                }
            }
        }
        private bool _hunted;
        public bool Hunted
        {
            get { return _hunted; }
            set
            {
                if (value)
                    Player.AddFlag(MsgUpdate.Flags.Flashy, 60 * 60 * 24 * 30, false);
                else
                    Player.RemoveFlag(MsgUpdate.Flags.Flashy);
                _hunted = value;
            }
        }

        public BaseEvent EventsLib { get; internal set; }

        internal DateTime LastCheatPacket = DateTime.Now;
        internal bool Reported;

        public uint AjustAttack(uint Damage)
        {
            uint nAttack = 0;

            if (Player.ContainFlag(Game.MsgServer.MsgUpdate.Flags.Stigma) || Player.OnAttackPotion)
            {
                nAttack += (uint)Game.MsgServer.AttackHandler.Calculate.Base.MulDiv((int)Damage, 130, 100) - Damage;
            }
            //if (Status.PhysicalPercent > 0)
            //{
            //    nAttack += (uint)Game.MsgServer.AttackHandler.Calculate.Base.MulDiv((int)Damage, (int)Status.PhysicalPercent, 100);// -Damage / 2;
            //    //(uint)Game.MsgServer.AttackHandler.Calculate.Base.MulDiv((int)Damage, (int)Status.PhysicalPercent, 100);
            //}
            //if (Player.Intensify || Player.InUseIntensify)
            //{
            //    Player.Intensify = false;//IntensifyDamage
            //    Player.InUseIntensify = false;
            //    nAttack += (uint)Game.MsgServer.AttackHandler.Calculate.Base.MulDiv((int)Damage, Player.IntensifyDamage, 100) - Damage;
            //}
            return Damage + nAttack;
        }
        public int GetDefense2()
        {
            return Player.Reborn > 0 && Player.Class % 10 > 3 ? 7000 : DefaultDefense;
        }

        public void Shift(ushort X, ushort Y, ServerSockets.Packet stream, bool SendData = true)
        {
            Player.Px = Player.X;
            Player.Py = Player.Y;

            if (SendData)
            {

                ActionQuery action = new ActionQuery()
                {
                    ObjId = Player.UID,
                    Type = ActionType.FlashStep,
                    wParam1 = X,
                    wParam2 = Y
                };
                Player.View.SendView(stream.ActionCreate(&action), true);

                Map.View.MoveTo<Role.IMapObj>(Player, X, Y);
                Player.X = X;
                Player.Y = Y;
                Player.View.Role(false, stream);
            }
            else
            {
                Map.View.MoveTo<Role.IMapObj>(Player, X, Y);
                Player.X = X;
                Player.Y = Y;
                Player.View.Role(false, null);
            }
        }
        public ushort UplevelProficiency;
        public DateTime WepSkill = DateTime.Now;
        public Role.Instance.Warehouse Warehouse;
        public Role.Instance.Equip Equipment;
        public Role.Instance.Inventory Inventory;
        public Role.Instance.Proficiency MyProfs;
        public Role.Instance.Spell MySpells;
        public Role.Instance.Confiscator Confiscator;
        public Role.Instance.EffectStatus EffectStatus;
        public GameClient(ServerSockets.SecuritySocket _socket, bool _OnInterServer = false)
        {
            DemonExterminator = new Role.Instance.DemonExterminator();
            Confiscator = new Role.Instance.Confiscator();
            EffectStatus = new Role.Instance.EffectStatus(this);
            ClientFlag |= ServerFlag.None;
            if (_socket != null)
            {
                Socket = _socket;
                Socket.Client = this;
                Socket.Game = this;
                //Cryptography = new Cryptography.GameCryptography(new byte[] { 0x49, 0x58, 0x41, 0x57, 0x4B, 0x32, 0x33, 0x4D, 0x31, 0x53, 0x44, 0x38, 0x35, 0x50, 0x44, 0x34, 0x00 });
                Cryptography = new Cryptography.GameCryptography(System.Text.ASCIIEncoding.ASCII.GetBytes(Program.LogginKey));
                Socket.SetCrypto(Cryptography);
            }
            Player = new Role.Player(this);

            DHKeyExchance = new Cryptography.DHKeyExchange.ServerKeyExchange();

            if (_socket != null)
            {
                //Send(DHKeyExchance.CreateServerKeyPacket(DHKey));
                Send(DHKeyExchance.CreateServerKeyPacket());
            }
        }
        public void Send(ServerSockets.Packet msg)
        {
            try
            {
                if (Fake || Socket.Alive == false)
                    return;

                Socket.Send(msg);

            }
            catch (Exception e)
            {
                Console.WriteException(e);
            }
        }
        public void Send(byte[] buffer)
        {
            try
            {
                if (Fake || Socket.Alive == false)
                    return;
                ushort length = BitConverter.ToUInt16(buffer, 0);
                if (length == 0)
                {
                    Network.Writer.WriteUInt16((ushort)(buffer.Length - 8), 0, buffer);
                }
                Network.Writer.WriteString("TQServer", buffer.Length - 8, buffer);

                ServerSockets.Packet stream = new ServerSockets.Packet(buffer);
                Socket.Send(stream);

            }
            catch (Exception e)
            {
                Console.WriteException(e);
            }
        }
        public bool Fake = false;
        public void SendSysMesage(string Messaj, Game.MsgServer.MsgMessage.ChatMode ChatType = Game.MsgServer.MsgMessage.ChatMode.Talk
            , Game.MsgServer.MsgMessage.MsgColor color = Game.MsgServer.MsgMessage.MsgColor.white, bool SendScren = false)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();

                if (SendScren)
                    Player.View.SendView(new Game.MsgServer.MsgMessage(Messaj, color, ChatType).GetArray(stream), true);
                else
                    Send(new Game.MsgServer.MsgMessage(Messaj, color, ChatType).GetArray(stream));
            }
        }
        public void SendScreen(byte[] msg, bool self = true)
        {
            Player.View.SendView(msg, self);
        }
        public void CreateDialog(ServerSockets.Packet stream, string Text, string OptionText)
        {
            Game.MsgNpc.Dialog dialog = new Game.MsgNpc.Dialog(this, stream);
            dialog.AddText(Text);
            if (OptionText != "")
                dialog.AddOption(OptionText, 255);
            dialog.FinalizeDialog();
        }
        public void CreateBoxDialog(string Text)
        {
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();

                Game.MsgNpc.Dialog dialog = new Game.MsgNpc.Dialog(this, stream);
                dialog.CreateMessageBox(Text).FinalizeDialog(true);
            }
        }
        public IEnumerable<Game.MsgServer.MsgGameItem> GetAllMainItems()
        {
            foreach (var item in Inventory.ClientItems.Values)
                yield return item;
            foreach (var item in Equipment.ClientItems.Values)
                yield return item;

        }
        public IEnumerable<Game.MsgServer.MsgGameItem> AllMyItems()
        {
            foreach (var item in Inventory.ClientItems.Values)
                yield return item;
            foreach (var item in Equipment.ClientItems.Values)
                yield return item;
            foreach (var Wh in Warehouse.ClientItems.Values)
            {
                foreach (var item in Wh.Values)
                    yield return item;
            }

        }
        public int GetItemsCount()
        {
            int count = 0;
            count += Inventory.ClientItems.Count;
            count += Equipment.ClientItems.Count;
            foreach (var Wh in Warehouse.ClientItems.Values)
            {
                count += Wh.Count;
            }


            return count;
        }
        public bool TryGetItem(uint UID, out Game.MsgServer.MsgGameItem item)
        {
            if (Equipment.TryGetValue(UID, out item))
                return true;
            if (Inventory.TryGetItem(UID, out item))
                return true;

            item = null;
            return false;
        }
        public ushort CalculateHitPoint()
        {
            ushort valor = 0;
            switch (Player.Class)
            {
                case 11:
                    valor += (ushort)(Player.Agility * 3.15 + Player.Spirit * 3.15 + Player.Strength * 3.15 + Player.Vitality * 25.2);
                    break;
                case 12:
                    valor += (ushort)(Player.Agility * 3.24 + Player.Spirit * 3.24 + Player.Strength * 3.24 + Player.Vitality * 25.9);
                    break;
                case 13:
                    valor += (ushort)(Player.Agility * 3.30 + Player.Spirit * 3.30 + Player.Strength * 3.30 + Player.Vitality * 26.4);
                    break;
                case 14:
                    valor += (ushort)(Player.Agility * 3.36 + Player.Spirit * 3.36 + Player.Strength * 3.36 + Player.Vitality * 26.8);
                    break;
                case 15:
                    valor += (ushort)(Player.Agility * 3.45 + Player.Spirit * 3.45 + Player.Strength * 3.45 + Player.Vitality * 27.6);
                    break;
                default:
                    valor += (ushort)(Player.Agility * 3 + Player.Spirit * 3 + Player.Strength * 3 + Player.Vitality * 24);
                    break;
            }
            return valor;


        }
        public ushort CalculateMana()
        {
            ushort valor = 0;
            switch (Player.Class)
            {
                case 142:
                case 132: valor += (ushort)(Player.Spirit * 15); break;
                case 143:
                case 133: valor += (ushort)(Player.Spirit * 20); break;
                case 144:
                case 134: valor += (ushort)(Player.Spirit * 25); break;
                case 145:
                case 135: valor += (ushort)(Player.Spirit * 30); break;
                default: valor += (ushort)(Player.Spirit * 5); break;
            }
            return valor;
        }
        public void Pullback()
        {
            Teleport(Player.X, Player.Y, Player.Map, Player.DynamicID);
        }
        public void TeleportCallBack()
        {
            Teleport(Player.PMapX, Player.PMapY, Player.PMap, Player.PDinamycID);
        }
        public void Teleport(ushort x, ushort y, uint MapID, uint DinamycID = 0, bool revive = true, bool CanTeleport = false)
        {
            if (Player.Map >= 50 && Player.Map <= 53 && Player.HitPoints > 0)
            {
                Player.Money += 1000000;
            }
            //if (EventManager.passthebombEv != null)
            {
                if (!Player.Alive)
                {

                    using (var rec = new ServerSockets.RecycledPacket())
                    {
                        var stream = rec.GetStream();

                        Player.Revive(stream);
                    }
                }

            }
            if (Player.InUseIntensify)
            {
                if (Player.ContainFlag(MsgUpdate.Flags.Intensify))
                    Player.RemoveFlag(MsgUpdate.Flags.Intensify);
                Player.InUseIntensify = false;
            }
            if (Player.Mining) Player.Mining = false;
            if (MapID == 1036 && Player.TransformInfo != null)
                Player.TransformInfo.FinishTransform();
            if (MapID == 1036 && Player.ContainFlag(MsgUpdate.Flags.Cyclone))
                Player.RemoveFlag(MsgUpdate.Flags.Cyclone);
            if (MapID == 1601 && Player.ContainFlag(MsgUpdate.Flags.Cyclone))
                Player.RemoveFlag(MsgUpdate.Flags.Cyclone);
            if (MapID == 1601 && Player.TransformInfo != null)
                Player.TransformInfo.FinishTransform();
            if (!ProjectManager)
            {
                if (Player.Map == 6001 && CanTeleport == false)
                    return;
            }
            if (Player.Map == 1038 && Player.Alive == false && CanTeleport == false)
                return;
            if (Program.ArenaMaps.ContainsKey(Player.Map) || Program.ArenaMaps.ContainsKey(Player.DynamicID) || Program.ArenaMaps.ContainsKey((uint)Player.Betting))
            {
                foreach (var bot in Bots.BotProcessring.Bots.Values)
                {
                    if (bot.Bot != null)
                    {
                        if (bot.Bot.Player.Map == Player.Map && bot.Bot.Player.DynamicID == Player.DynamicID)
                            bot.Dispose();
                    }
                }
            }


            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();

                if (this.Pet != null) this.Pet.DeAtach(stream);

                if (Player.SetLocationType != 1)
                {
                    if (!Player.OnMyOwnServer && MapID != 1002 && MapID != 3935)
                        return;
                }
                if (Program.MapCounterHits.Contains(Player.Map) || Player.Map == 1038 || Game.MsgTournaments.MsgSchedules.CurrentTournament.InTournament(this))
                {
                    if (MapID != Player.Map)
                    {
                        SendSysMesage("", MsgMessage.ChatMode.FirstRightCorner);

                        if (Game.MsgTournaments.MsgSchedules.CurrentTournament.Type == Game.MsgTournaments.TournamentType.KillTheCaptain)
                        {
                            if (Game.MsgTournaments.MsgSchedules.CurrentTournament.InTournament(this))
                            {
                                Player.RemoveSpecialGarment(stream);
                                if (Player.ContainFlag(MsgUpdate.Flags.Freeze))
                                    Player.RemoveFlag(MsgUpdate.Flags.Freeze);
                            }
                        }
                    }
                }

                if (Socket != null)//!= null for facke accounts.
                {
                    if (Socket.Alive == false)
                        return;
                }

#if TEST
                if (ProjectManager)
                    MyConsole.WriteLine("Name= " + Player.Name + " Tele to map = " + MapID + " X = " + x.ToString() + " Y = " + y.ToString() + "");
#endif
                if (IsVendor)
                    MyVendor.StopVending(stream);
                if (InTrade)
                    MyTrade.CloseTrade();


                if (MapID == 601 || MapID == 1039)
                {
                    if (Player.HeavenBlessing > 0)
                    {
                        Player.SendUpdate(stream, Game.MsgServer.MsgUpdate.OnlineTraining.InTraining, Game.MsgServer.MsgUpdate.DataType.OnlineTraining);
                    }
                }
                if (Player.Map == 601 || Player.Map == 1039)
                {
                    if (MapID != 601 && MapID != 1039)
                        Player.SendUpdate(stream, Game.MsgServer.MsgUpdate.OnlineTraining.Review, Game.MsgServer.MsgUpdate.DataType.OnlineTraining);
                }

                //Player.ClearPreviouseCoord();

                if (!Role.GameMap.CheckMap(MapID))
                {

                    MapID = 1002;
                    x = 429;
                    y = 378;
                }
                Role.GameMap GameMap;
                if (Database.Server.ServerMaps.TryGetValue(MapID, out GameMap))
                {
                    OnAutoAttack = false;
                    Player.RemoveBuffersMovements(stream);

                    Player.View.Clear(stream);


                    if (GameMap.BaseID != 0)
                    {
                        ActionQuery daction = new ActionQuery()
                        {
                            ObjId = Player.UID,
                            Type = ActionType.Teleport,
                            dwParam = GameMap.BaseID,
                            wParam1 = x,
                            wParam2 = y,
                            dwParam3 = GameMap.BaseID
                        };
                        Send(stream.ActionCreate(&daction));
                    }
                    else
                    {
                        ActionQuery aaction = new ActionQuery()
                        {
                            ObjId = Player.UID,
                            Type = ActionType.Teleport,
                            dwParam = MapID,
                            wParam1 = x,
                            wParam2 = y,
                            dwParam3 = MapID
                        };
                        Send(stream.ActionCreate(&aaction));
                    }
                    if (Player.Map != 700)
                    {
                        var aaaction = new ActionQuery()
                        {
                            ObjId = Player.UID,
                            Type = (ActionType)157,
                            dwParam = 2,
                            wParam1 = x,
                            wParam2 = y,
                            dwParam3 = MapID
                        };
                        Send(stream.ActionCreate(&aaaction));
                    }

                    var action = new ActionQuery()
                    {
                        ObjId = Player.UID,
                        Type = ActionType.StopVending,
                        dwParam = MapID,
                        wParam1 = x,
                        wParam2 = y,
                        dwParam3 = MapID
                    };
                    Send(stream.ActionCreate(&action));

                    if (MapID == 1780 && GameMap.BaseID == 0)
                    {
                        action = new ActionQuery()
                        {
                            ObjId = Player.UID,
                            Type = ActionType.SetMapColor,
                            dwParam = 0x323232,
                            wParam1 = x,
                            wParam2 = y
                        };
                        Send(stream.ActionCreate(&action));

                    }
                    else if (MapID == 3846)
                    {
                        action = new ActionQuery()
                        {
                            ObjId = Player.UID,
                            Type = ActionType.SetMapColor,
                            dwParam = 16755370,
                            wParam1 = x,
                            wParam2 = y
                        };
                        Send(stream.ActionCreate(&action));

                    }
                    else if (MapID == 10088 || MapID == 44455 || MapID == 44456)
                    {
                        action = new ActionQuery()
                        {
                            ObjId = Player.UID,
                            Type = ActionType.SetMapColor,
                            dwParam = 14535867,
                            wParam1 = x,
                            wParam2 = y
                        };
                        Send(stream.ActionCreate(&action));
                    }
                    else
                    {

                        if (GameMap.ID == 3830 || GameMap.ID == 3831 || GameMap.ID == 3832)
                        {
                            action = new ActionQuery()
                            {
                                ObjId = Player.UID,
                                Type = ActionType.SetMapColor,
                                dwParam = GameMap.MapColor,
                                wParam1 = x,
                                wParam2 = y

                            };
                            Send(stream.ActionCreate(&action));
                        }
                        else
                        {
                            action = new ActionQuery()
                            {
                                ObjId = Player.UID,
                                Type = ActionType.SetMapColor,
                                dwParam = 0,
                                wParam1 = x,
                                wParam2 = y

                            };
                            Send(stream.ActionCreate(&action));
                        }
                    }

                    if (MapID == Player.Map && Player.DynamicID == DinamycID)
                    {
                        Map.Denquer(this);
                        // Map.View.MoveTo<Role.IMapObj>(Player, x, y);
                        Player.X = x;
                        Player.Y = y;
                        Database.Server.ServerMaps[MapID].Enquer(this);
                    }
                    else
                    {
                        Player.PDinamycID = Player.DynamicID;
                        Player.PMapX = Player.X;
                        Player.PMapY = Player.Y;

                        Map.Denquer(this);

                        Player.DynamicID = DinamycID;
                        Player.X = x;
                        Player.Y = y;
                        Player.PMap = Player.Map;

                        Player.Map = MapID;


                        Database.Server.ServerMaps[MapID].Enquer(this);
                    }

                    if (Player.Map == 700)
                    {
                        Send(stream.MapStatusCreate(Map.ID, Map.ID, (uint)Map.TypeStatus));
                    }
                    else if (GameMap.BaseID != 0)
                        Send(stream.MapStatusCreate(Map.BaseID, Map.BaseID, (uint)Map.TypeStatus));
                    else
                    {
                        if (Player.Map == 3935)
                            Send(stream.MapStatusCreate(Map.ID, Map.ID, 846641133264903));
                        else
                            Send(stream.MapStatusCreate(Map.ID, Map.ID, (uint)Map.TypeStatus));
                    }
                    Player.View.Role(true);

                    if (!Player.Alive && revive && Player.Map != 1038)
                    {
                        Player.Revive(stream);
                    }
                    if (Player.ObjInteraction != null)
                    {
                        if (Role.Core.IsBoy(Player.Body))
                        {
                            Player.ObjInteraction.Teleport(x, y, MapID, DinamycID);
                        }
                    }
                    if (Player.Map == 1038 || Player.Map == 3868 || Game.MsgTournaments.MsgSchedules.CurrentTournament.InTournament(this))
                        if (Player.ContainFlag(MsgUpdate.Flags.Ride))
                            Player.RemoveFlag(MsgUpdate.Flags.Ride);

                    if (Player.Map == 1038 || Player.Map == 3868)
                    {
                        if (Player.ContainFlag(MsgUpdate.Flags.FatalStrike))
                            Player.RemoveFlag(MsgUpdate.Flags.FatalStrike);
                    }
                    // Send(stream.WeatherCreate(MsgWeather.WeatherType.Snow, 1000, 3, 0, 0));

                }
                Equipment.Show(stream, true);
            }
            // Player.View.Collect();
            if (Player.Map == 6000 && !Map.ValidLocation(x, y))
                Teleport(32, 73, 6000);

            Player.ProtectAttack(2000);
        }
        public Game.MsgServer.MsgStatus Status = new MsgStatus();
        public void UpdateLevel(ServerSockets.Packet stream, ushort Level, bool REsetExp = false, bool mentorexp = true)
        {

            if (Level == Player.Level)
                return;
            if (Player.MyGuildMember != null)
            {
                Player.MyGuildMember.Level = Level;
            }
            if (REsetExp)
                Player.Experience = 0;
            uint OldLevel = Player.Level;
            Player.Level = Level;


            Player.SendUpdate(stream, Player.Level, Game.MsgServer.MsgUpdate.DataType.Level);
            ActionQuery action = new ActionQuery()
            {
                Type = ActionType.Leveled,
                ObjId = Player.UID,
                wParam1 = Level
            };
            Player.View.SendView(stream.ActionCreate(&action), true);


            if (Player.Reborn == 0 && (
                Database.AtributesStatus.IsWater(Player.Class)
                ? (Level < 111 || OldLevel < 110 && Level > 110)
                : (Level < 121 || OldLevel < 120 && Level > 120)))
            {
                Database.DataCore.AtributeStatus.GetStatus(Player);
                Player.SendUpdate(stream, Player.Strength, Game.MsgServer.MsgUpdate.DataType.Strength);
                Player.SendUpdate(stream, Player.Agility, Game.MsgServer.MsgUpdate.DataType.Agility);
                Player.SendUpdate(stream, Player.Spirit, Game.MsgServer.MsgUpdate.DataType.Spirit);
                Player.SendUpdate(stream, Player.Vitality, Game.MsgServer.MsgUpdate.DataType.Vitality);
                Player.SendUpdate(stream, Player.Atributes, Game.MsgServer.MsgUpdate.DataType.Atributes);

            }
            else
            {
                if (OldLevel < Level)
                {
                    ushort artibute = (ushort)((Level - OldLevel) * 3);
                    Player.Atributes += artibute;
                    Player.SendUpdate(stream, Player.Atributes, Game.MsgServer.MsgUpdate.DataType.Atributes);
                }
            }
            Database.Server.RebornInfo.Reborn(this.Player, 0, stream);
            if (Player.MyMentor != null && mentorexp)
            {
                var LevelUp = Database.Server.LevelInfo[Database.DBLevExp.Sort.User][(byte)OldLevel];
                Player.MyMentor.Mentor_ExpBalls += (uint)LevelUp.MentorUpLevTime;
                Role.Instance.Associate.Member mee;
                if (Player.MyMentor.Associat.ContainsKey(Role.Instance.Associate.Apprentice))
                {
                    if (Player.MyMentor.Associat[Role.Instance.Associate.Apprentice].TryGetValue(Player.UID, out mee))
                    {

                        mee.ExpBalls += (uint)LevelUp.MentorUpLevTime;
                    }
                }
            }
            Equipment.QueryEquipment(false);
            Player.HitPoints = (int)Status.MaxHitpoints;

            if (Player.Level <= 70 && Team != null)
            {
                var teamleader = Team.Leader;
                if (teamleader.Player.UID != Player.UID)
                {
                    if (Role.Core.GetDistance(teamleader.Player.X, teamleader.Player.Y, Player.X, Player.Y) < Role.RoleView.ViewThreshold)
                    {
                        if (teamleader.Player.Map != Player.Map)
                            return;
                        if (!teamleader.Player.Alive || teamleader.Player.Level < 70)
                            return;

                        uint amount = (uint)Math.Max(1, Player.Level * 7 - 12);
                        teamleader.Player.VirtutePoints += amount /*(uint)(Player.Level * 7 - 12*/;
                        Team.SendTeam(new MsgMessage("Congratulations to leader, he has earned " + amount + " Virtue Points!", MsgMessage.MsgColor.white, MsgMessage.ChatMode.Team).GetArray(stream), 0);

                    }
                }
            }
            UpdateRebornLastLevel(stream);
        }
        public IEnumerable<Game.MsgServer.MsgGameItem> AllMyTimeItems()
        {
            foreach (var item in Inventory.ClientItems.Values)
            {
                if (item.Activate == 1)
                {
                    yield return item;
                }
            }
            foreach (var item in Equipment.ClientItems.Values)
            {
                if (item.Activate == 1)
                {
                    yield return item;
                }
            }
            foreach (var Wh in Warehouse.ClientItems.Values)
            {
                foreach (var item in Wh.Values)
                    if (item.Activate == 1)
                    {
                        yield return item;
                    }
            }

        }
        public KillTheCaptainTeams TeamKillTheCaptain;
        public DateTime LastOnlineStamp = DateTime.Now;
        public DateTime ItemStamp;
        internal string IP;
    }
}
