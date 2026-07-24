using System;
using System.Collections.Generic;
using System.Linq;

namespace COServer.Game.MsgTournaments
{
    public class MsgClassPKWar
    {

        public static string CurrentClass { get; private set; } // Classe do dia
        public const ushort MapID = 1764;
        public const string FilleName = "\\ClassPkWar.ini";

        public enum TournamentType : byte
        {
            Trojan = 0,
            Warrior = 1,
            Archer = 2,
            Water = 3,
            Fire = 4,
            Count = 5
        }
        public enum TournamentLevel : byte
        {
            Level_130 = 0,
            Count = 4
        }

        public enum Weekday : byte
        {
            Sunday = 0,
            Monday = 1,
            Tuesday = 2,
            Wednesday = 3,
            Thursday = 4,
            Friday = 5,
            Saturday = 6
        }

        public static readonly Dictionary<TournamentType, Weekday> TournamentDays = new Dictionary<TournamentType, Weekday>
            {
                { TournamentType.Trojan, Weekday.Monday },
                { TournamentType.Warrior, Weekday.Tuesday },
                { TournamentType.Archer, Weekday.Wednesday },
                { TournamentType.Water, Weekday.Thursday },
                { TournamentType.Fire, Weekday.Friday }
            };

        public War[][] PkWars;
        public ProcesType Proces;

        public MsgClassPKWar(ProcesType _Proces)
        {
            Proces = _Proces;

            PkWars = new War[(byte)TournamentType.Count][];

            for (TournamentType i = TournamentType.Trojan; i < TournamentType.Count; i++)
            {
                PkWars[(byte)i] = new War[(byte)TournamentLevel.Count];

                for (TournamentLevel x = TournamentLevel.Level_130; x < TournamentLevel.Count; x++)
                {
                    PkWars[(byte)i][(byte)x] = new War(i, x, ProcesType.Dead);
                }
            }
        }
        internal void Start()
        {
            if (Proces == ProcesType.Dead)
            {
                var currentDay = (Weekday)(int)DateTime.Now.DayOfWeek;

                foreach (var tournament in TournamentDays)
                {
                    if (tournament.Value == currentDay)
                    {
                        var typ = tournament.Key;
                        PkWars[(byte)typ] = new War[(byte)TournamentLevel.Count];

                        for (TournamentLevel x = TournamentLevel.Level_130; x < TournamentLevel.Count; x++)
                        {
                            PkWars[(byte)typ][(byte)x] = new War(typ, x, ProcesType.Dead);
                        }


                        foreach (var war in PkWars[(byte)typ])
                        {

                            war.Start(Database.Server.ServerMaps[MapID]);
                        }
                    }
                }
                    
                Proces = ProcesType.Idle;
            }
        }
        internal void Stop()
        {
            if (Proces != ProcesType.Dead)
            {
                foreach (var tournament in PkWars)
                {
                    foreach (var war in tournament)
                    {
                        war.Stop();
                    }
                }
                Proces = ProcesType.Dead;
            }
        }

        internal MsgServer.MsgUpdate.Flags ObtainAura(TournamentType typ)
        {
            switch (typ)
            {
                case TournamentType.Trojan: return MsgServer.MsgUpdate.Flags.TopTrojan;
                case TournamentType.Warrior: return MsgServer.MsgUpdate.Flags.TopWarrior;
                case TournamentType.Archer: return MsgServer.MsgUpdate.Flags.TopArcher;
                case TournamentType.Fire: return MsgServer.MsgUpdate.Flags.TopFireTaoist;
                case TournamentType.Water: return MsgServer.MsgUpdate.Flags.TopWaterTaoist;
            }
            return MsgServer.MsgUpdate.Flags.Normal;
        }

        public War GetMyWar(Client.GameClient client)
        {
            foreach (var tournament in PkWars)
            {
                foreach (var war in tournament)
                {
                    if (war.DinamicID == client.Player.DynamicID)
                        return war;
                }
            }
            return null;
        }
        internal static TournamentType GetMyTournamentType(Client.GameClient client)
        {
            if (Database.AtributesStatus.IsTrojan(client.Player.Class))
                return TournamentType.Trojan;
            if (Database.AtributesStatus.IsWarrior(client.Player.Class))
                return TournamentType.Warrior;
            if (Database.AtributesStatus.IsArcher(client.Player.Class))
                return TournamentType.Archer;
            if (Database.AtributesStatus.IsWater(client.Player.Class))
                return TournamentType.Water;
            if (Database.AtributesStatus.IsFire(client.Player.Class))
                return TournamentType.Fire;
            return TournamentType.Count;
        }
        internal static TournamentLevel GetMyTournamentLevel(Client.GameClient client)
        {
            if (client.Player.Level >= 130)
                return TournamentLevel.Level_130;
            return TournamentLevel.Count;
        }
        internal ProcesType GetWar(Client.GameClient client, out War mywar)
        {
            var typ = GetMyTournamentType(client);
            var level = GetMyTournamentLevel(client);
            var tournament = PkWars[(byte)typ];
            foreach (var war in tournament)
            {
                if (war.Typ == typ && war.Level == level)
                {
                    mywar = war;
                    return war.Proces;
                }
            }
            mywar = null;
            return ProcesType.Dead;
        }
        internal void LoginClient(Client.GameClient client)
        {
            foreach (var tournament in PkWars)
            {
                foreach (var war in tournament)
                {
                    if (war.Winner == client.Player.UID)
                    {
                        client.Player.AddFlag(war.LastFlag, Role.StatusFlagsBigVector32.PermanentFlag, false);
                    }
                }
            }
        }
        internal void Save()
        {
            Database.DBActions.Write writer = new Database.DBActions.Write(FilleName);
            foreach (var tournament in PkWars)
            {
                foreach (var war in tournament)
                {
                    Database.DBActions.WriteLine line = new Database.DBActions.WriteLine('/');
                    line.Add((byte)war.Typ).Add((byte)war.Level).Add(war.Winner).Add((int)war.LastFlag);
                    writer.Add(line.Close());
                }
            }
            writer.Execute(Database.DBActions.Mode.Open);
        }
        internal void Load()
        {
            Database.DBActions.Read reader = new Database.DBActions.Read(FilleName);
            if (reader.Reader())
            {
                for (int x = 0; x < reader.Count; x++)
                {
                    Database.DBActions.ReadLine line = new Database.DBActions.ReadLine(reader.ReadString(""), '/');
                    byte typ = line.Read((byte)0);
                    byte level = line.Read((byte)0);
                    uint Winner = line.Read((uint)0);
                    MsgServer.MsgUpdate.Flags LastFlag = (MsgServer.MsgUpdate.Flags)line.Read((int)0);

                    PkWars[typ][level].Winner = Winner;
                    PkWars[typ][level].LastFlag = LastFlag;
                }
            }
        }
        public class War
        {
            public uint Winner = 0;
            public byte ReceiveReward = 0;
            public MsgServer.MsgUpdate.Flags LastFlag = 0;

            public TournamentLevel Level;
            public TournamentType Typ;
            public ProcesType Proces;
            public uint DinamicID;
            public DateTime FinishTimer = new DateTime();

            public War(TournamentType _typ, TournamentLevel _level, ProcesType proces)
            {
                Level = _level;
                Proces = proces;
                Typ = _typ;
            }
            public void Start(Role.GameMap map)
            {
                if (Proces == ProcesType.Dead)
                {
                    Proces = ProcesType.Alive;
                    FinishTimer = DateTime.Now.AddMinutes(10);
                    DinamicID = map.GenerateDynamicID();

                    MsgSchedules.SendSysMesage($"[ Class PK ] for {Typ.ToString()} has started in TwinCity ( 377,315 )",
                        MsgServer.MsgMessage.ChatMode.TopLeftSystem, MsgServer.MsgMessage.MsgColor.white);

                    Program.DiscordAPIevents.Enqueue($"``[ Class PK ] for {Typ.ToString()} has started in TwinCity ( 377,315 )``");

 
                    foreach (var client in Database.Server.GamePoll.Values)
                    {
                        var playerType = MsgClassPKWar.GetMyTournamentType(client);
                        if (playerType == Typ) // Only players of the matching class
                        {
                            client.Player.MessageBox($"[ Class PK ] for {Typ.ToString()} has started! Do you want to join?",
                                new Action<Client.GameClient>(p => p.Teleport(436, 244, MapID, DinamicID)), null, 60);
                        }
                    }
                    Console.WriteLine($"[ ClassPK ] tournament for {Typ.ToString()} started. DinamicID: {DinamicID}, Time: {DateTime.Now}");
                }
            }
            internal void Stop()
            {
                Proces = ProcesType.Dead;
            }
            public bool CheckJoin()
            {
                if (Proces == ProcesType.Dead)
                    return false;
                if (DateTime.Now < FinishTimer)
                    return true;
                else
                {
                    Proces = ProcesType.Alive;
                    return false;
                }
            }
            public bool IsFinish(Client.GameClient client)
            {
                if (Proces == ProcesType.Alive)
                {
                    var arrayMap = client.Map.Values.Where(p => p.Player.DynamicID == DinamicID && p.Player.Alive).ToArray();
                    if (arrayMap.Length == 1)
                    {
                        return true;
                    }
                }
                return false;
            }
            public void GetMyReward(Client.GameClient client, ServerSockets.Packet stream)
            {
                if (IsFinish(client))
                {
                    Proces = ProcesType.Dead;

                    //Remove old winner -----------------------
                    foreach (var user in Database.Server.GamePoll.Values)
                    {
                        if (user.Player != null)
                        {
                            if (user.Player.UID == Winner)
                            {
                                user.Player.RemoveFlag(LastFlag);
                            }
                        }
                    }
                    //------------------------------------------

                    var aura = MsgSchedules.ClassPkWar.ObtainAura(Typ);
                    if (aura != MsgServer.MsgUpdate.Flags.Normal)
                        client.Player.AddFlag(aura, Role.StatusFlagsBigVector32.PermanentFlag, false);

                    // Recompensa em silver
                    uint silverReward = 1000000; // 1 million silver
                    client.Player.Money += silverReward;

                    LastFlag = aura;
                    Winner = client.Player.UID;

                    MsgSchedules.SendSysMesage(
                        $"{client.Player.Name} winner {Typ.ToString()} PK War ({Level.ToString()}) and received {silverReward} silver.",
                        MsgServer.MsgMessage.ChatMode.TopLeftSystem,
                        MsgServer.MsgMessage.MsgColor.white
                    );

                    Program.DiscordAPIwinners.Enqueue(
                        $"``[{client.Player.Name}] winner {Typ.ToString()} PK War ({Level.ToString()}) and received {silverReward} silver.``"
                    );

                    client.Teleport(430, 269, 1002, 0);
                }
            }

        }
    }
}
