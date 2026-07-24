using System;
using System.Collections.Generic;

namespace COServer.Database
{
    public class GuildTable
    {
        //save ----------------------
        internal static void Save()
        {
            foreach (var obj in Role.Instance.Guild.GuildPoll)
            {
                if (obj.Value.CanSave == false)
                    continue;
                var guild = obj.Value;
                using (DBActions.Write writer = new DBActions.Write("Guilds\\" + obj.Key + ".txt"))
                {
                    writer.Add(guild.ToString())
                        .Add(ToStringAlly(guild)).Add(ToStringEnemy(guild));
                    writer.Execute(DBActions.Mode.Open);
                }
            }
        }
        public static string ToStringAlly(Role.Instance.Guild guild)
        {
            DBActions.WriteLine writer = new DBActions.WriteLine('/');
            writer.Add(guild.Ally.Count);
            foreach (Role.Instance.Guild ally in guild.Ally.Values)
                writer.Add(ally.Info.GuildID);
            return writer.Close();
        }
        public static string ToStringEnemy(Role.Instance.Guild guild)
        {
            DBActions.WriteLine writer = new DBActions.WriteLine('/');
            writer.Add(guild.Enemy.Count);
            foreach (Role.Instance.Guild enemy in guild.Enemy.Values)
                writer.Add(enemy.Info.GuildID);
            return writer.Close();
        }
        //------------------------


        internal static void Load()
        {
            foreach (string fname in System.IO.Directory.GetFiles(Program.ServerConfig.DbLocation + "\\Guilds\\"))
            {
                using (DBActions.Read reader = new DBActions.Read(fname, true))
                {
                    if (reader.Reader())
                    {
                        //--------- guild info ------------------
                        DBActions.ReadLine GuildReader = new DBActions.ReadLine(reader.ReadString("0/"), '/');
                        uint ID = GuildReader.Read((uint)0);
                        if (ID > 100000)
                            continue;
                        if (ID > Role.Instance.Guild.Counter.Count)
                            Role.Instance.Guild.Counter.Set(ID);

                        Role.Instance.Guild guild = new Role.Instance.Guild(null, GuildReader.Read("None"), null);
                        guild.Info.GuildID = ID;
                        guild.Info.LeaderName = GuildReader.Read("None");
                        guild.Info.SilverFund = GuildReader.Read((long)0);
                        guild.Info.Donation = GuildReader.Read((uint)0);
                        guild.Info.CreateTime = GuildReader.Read((uint)0);
                        guild.Bulletin = GuildReader.Read("None");

                        //---------load ally ---------------------
                        LoadGuildAlly(ID, reader.ReadString("0/"));
                        //-----------------------------------

                        //---------load enemy --------------------
                        LoadGuildEnemy(ID, reader.ReadString("0/"));
                        //----------------------------------------

                        //---------load arsenals ------------------

                        if (!Role.Instance.Guild.GuildPoll.ContainsKey(guild.Info.GuildID))
                            Role.Instance.Guild.GuildPoll.TryAdd(guild.Info.GuildID, guild);
                    }
                }
            }
            ExecuteAllyAndEnemy();
            LoadMembers();
            foreach (var guilds in Role.Instance.Guild.GuildPoll.Values)
            {
                guilds.CreateMembersRank();
                guilds.UpdateGuildInfo();
            }
            KernelThread.LastGuildPulse = DateTime.Now;
            enemy.Clear();
            ally.Clear();



            Console.WriteLine("Loading " + Role.Instance.Guild.GuildPoll.Count + " Guilds ! ");
            GC.Collect();
        }
        private static void LoadMembers()
        {
            WindowsAPI.IniFile ini = new WindowsAPI.IniFile("");
            foreach (string fname in System.IO.Directory.GetFiles(Program.ServerConfig.DbLocation + "\\Users\\"))
            {
                ini.FileName = fname;

                uint UID = ini.ReadUInt32("Character", "UID", 0);
                string Name = ini.ReadString("Character", "Name", "None");
                uint GuildID = ini.ReadUInt32("Character", "GuildID", 0);
                if (GuildID != 0)
                {
                    Role.Instance.Guild Guild;
                    if (Role.Instance.Guild.GuildPoll.TryGetValue(GuildID, out Guild))
                    {
                        ushort Body = ini.ReadUInt16("Character", "Body", 1002);
                        ushort Face = ini.ReadUInt16("Character", "Face", 0);

                        Role.Instance.Guild.Member member = new Role.Instance.Guild.Member();
                        member.UID = UID;
                        member.Name = Name;
                        member.Rank = (Role.Flags.GuildMemberRank)ini.ReadByte("Character", "GuildRank", 50);
                        member.MoneyDonate = ini.ReadInt64("Character", "MoneyDonate", 0);
                        member.Level = ini.ReadUInt16("Character", "Level", 0);

                        if (!Guild.Members.ContainsKey(member.UID))
                            Guild.Members.TryAdd(member.UID, member);


                    }
                }

            }
        }
        public static void ExecuteAllyAndEnemy()
        {
            foreach (var obj in ally)
            {
                foreach (var guild in obj.Value)
                {
                    Role.Instance.Guild alyguild;
                    if (Role.Instance.Guild.GuildPoll.TryGetValue(guild, out alyguild))
                    {
                        if (Role.Instance.Guild.GuildPoll.ContainsKey(obj.Key))
                        {
                            Role.Instance.Guild.GuildPoll[obj.Key].Ally.TryAdd(alyguild.Info.GuildID, alyguild);
                        }
                    }
                }
            }
            foreach (var obj in enemy)
            {
                foreach (var guild in obj.Value)
                {
                    Role.Instance.Guild alyenemy;
                    if (Role.Instance.Guild.GuildPoll.TryGetValue(guild, out alyenemy))
                    {
                        if (Role.Instance.Guild.GuildPoll.ContainsKey(obj.Key))
                        {
                            Role.Instance.Guild.GuildPoll[obj.Key].Enemy.TryAdd(alyenemy.Info.GuildID, alyenemy);
                        }
                    }
                }
            }
        }
        public static Dictionary<uint, List<uint>> ally = new Dictionary<uint, List<uint>>();
        public static Dictionary<uint, List<uint>> enemy = new Dictionary<uint, List<uint>>();
        public static void LoadGuildAlly(uint id, string line)
        {
            Database.DBActions.ReadLine reader = new DBActions.ReadLine(line, '/');
            int count = reader.Read(0);
            for (int x = 0; x < count; x++)
            {
                if (ally.ContainsKey(id))
                    ally[id].Add(reader.Read((uint)0));
                else
                    ally.Add(id, new List<uint>() { reader.Read((uint)0) });
            }
        }
        public static void LoadGuildEnemy(uint id, string line)
        {
            Database.DBActions.ReadLine reader = new DBActions.ReadLine(line, '/');
            int count = reader.Read(0);
            for (int x = 0; x < count; x++)
            {
                if (enemy.ContainsKey(id))
                    enemy[id].Add(reader.Read((uint)0));
                else
                    enemy.Add(id, new List<uint>() { reader.Read((uint)0) });
            }
        }
    }
}
