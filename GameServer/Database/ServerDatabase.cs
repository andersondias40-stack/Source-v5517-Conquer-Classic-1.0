using System;
using System.IO;
using COServer.Game.MsgServer;
using System.Reflection;
using MYSQLCOMMAND = MySql.Data.MySqlClient.MySqlCommand;
using MYSQLREADER = MySql.Data.MySqlClient.MySqlDataReader;
using MYSQLCONNECTION = MySql.Data.MySqlClient.MySqlConnection;
using System.Windows.Forms;
using COServer.Database.DBActions;

namespace COServer.Database
{
    public class ServerDatabase
    {
        private static string ConnectionString = "Server=localhost;Port=3306;Database=zq;Uid=root;Pwd=anderson;";

        public static MYSQLCONNECTION MySqlConnection
        {
            get
            {
                MYSQLCONNECTION conn = new MYSQLCONNECTION();
                conn.ConnectionString = ConnectionString;
                return conn;
            }
        }
        public static void ResetingEveryDay(Client.GameClient client)
        {
            try
            {
                if (DateTime.Now.DayOfYear != client.Player.Day)
                {
                    client.Player.QuestLevel = 5;
                    client.TotalMobsLevel = 0;
                    client.TotalMobsKilled = 0;

                    client.Player.OpenHousePack = 0;
                    client.Player.DbTry = false;
                    client.Player.LotteryEntries = 0;
                    client.Player.BDExp = 0;
                    client.Player.ExpBallUsed = 0;
                    client.Player.TCCaptainTimes = 0;
                    client.DemonExterminator.FinishToday = 0;

                    if (client.Player.VipLevel == 6)
                    {
                        //client.Player.Flowers.FreeFlowers = 30;
                    }
                    else
                    {
                        //client.Player.Flowers.FreeFlowers = 1;
                    }
                    //foreach (var flower in client.Player.Flowers)
                        //flower.Amount2day = 0;
                    client.Player.Day = DateTime.Now.DayOfYear;
                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }

        public static void SaveClient(Client.GameClient client)
        {

            try
            {
                WindowsAPI.IniFile write = new WindowsAPI.IniFile("\\Users\\" + client.Player.UID + ".ini");

                if ((client.ClientFlag & Client.ServerFlag.LoginFull) != Client.ServerFlag.LoginFull)
                {

                    if (client.Map != null)
                        client.Map.Denquer(client);
                }

                if (HouseTable.InHouse(client.Player.Map) && client.Player.DynamicID != 0 || client.Player.DynamicID != 0)
                {
                    if (client.Socket != null && client.Socket.Alive == false)
                    {
                        client.Player.Map = 1002;
                        client.Player.X = 428;
                        client.Player.Y = 378;
                    }
                }
                if (client.Player.Map == 8509)
                {
                    if (client.Socket != null && client.Socket.Alive == false)
                    {
                        client.Player.Map = 1002;
                        client.Player.X = 428;
                        client.Player.Y = 378;
                    }
                }
                if ((client.ClientFlag & Client.ServerFlag.Disconnect) == Client.ServerFlag.Disconnect)
                {
                    #region ArenaRoom [Closer]
                    if (client.Player.Map >= 50 && client.Player.Map <= 53 && client.Player.HitPoints > 0) //10.000.000
                    {
                        client.Player.Money += 1000000;
                    }
                    #endregion
                    if (Program.OutMap.Contains(client.Player.Map) || Program.MapDC.Contains(client.Player.Map) || Game.MsgTournaments.MsgSchedules.DisCity.IsInDisCity(client.Player.Map))
                    {
                        client.Player.Map = 1002;
                        client.Player.X = 428;
                        client.Player.Y = 378;
                    }
                    if (client.Player.Map == 700 && Program.ArenaMaps.ContainsValue(client.Player.DynamicID))
                        client.Teleport(428, 380, 1002);
                    if (client.Player.Map == 1038 && Game.MsgTournaments.MsgSchedules.GuildWar.Proces == Game.MsgTournaments.ProcesType.Alive)
                    {
                        client.Player.Map = 1002;
                        client.Player.X = 428;
                        client.Player.Y = 378;
                    }

                    if (client.Player.Map == 1036 || client.Player.Map == 1039 || client.Player.Map == 1572) // Mapa 1572 incluso
                    {
                        client.Player.Map = 1002;
                        client.Player.X = 428;
                        client.Player.Y = 378;
                    }
                }

                if (!client.FullLoading)
                    return;
                write.Write<uint>("Character", "CountVote", client.CountVote);
                write.Write<uint>("Character", "UID", client.Player.UID);
                write.Write<ushort>("Character", "Body", client.Player.Body);
                write.Write<ushort>("Character", "Face", client.Player.Face);
                write.WriteString("Character", "Name", client.Player.Name);
                write.WriteString("Character", "Spouse", client.Player.Spouse);
                write.Write<byte>("Character", "Class", client.Player.Class);
                write.Write<byte>("Character", "FirstClass", client.Player.FirstClass);
                write.Write<byte>("Character", "SecoundeClass", client.Player.SecondClass);
                write.Write<ushort>("Character", "Avatar", client.Player.Avatar);
                write.Write<uint>("Character", "Map", client.Player.Map);
                write.Write<ushort>("Character", "X", client.Player.X);
                write.Write<ushort>("Character", "Y", client.Player.Y);
                write.Write<uint>("Character", "PMap", client.Player.PMap);
                write.Write<ushort>("Character", "PMapX", client.Player.PMapX);
                write.Write<ushort>("Character", "PMapY", client.Player.PMapY);
                write.Write<ushort>("Character", "Agility", client.Player.Agility);
                write.Write<ushort>("Character", "Strength", client.Player.Strength);
                write.Write<ushort>("Character", "Vitaliti", client.Player.Vitality);
                write.Write<ushort>("Character", "Spirit", client.Player.Spirit);
                write.Write<ushort>("Character", "Atributes", client.Player.Atributes);
                write.Write<byte>("Character", "Reborn", client.Player.Reborn);
                write.Write<ushort>("Character", "Level", client.Player.Level);
                write.Write<ushort>("Character", "Haire", client.Player.Hair);
                write.Write<ulong>("Character", "Experience", client.Player.Experience);
                write.Write<int>("Character", "MinHitPoints", client.Player.HitPoints);
                write.Write<ushort>("Character", "MinMana", client.Player.Mana);
                write.Write<uint>("Character", "ConquerPoints", client.Player.ConquerPoints);
                write.Write<int>("Character", "BoundConquerPoints", client.Player.BoundConquerPoints);
                write.Write<long>("Character", "Money", client.Player.Money);
                write.Write<uint>("Character", "VirtutePoints", client.Player.VirtutePoints);
                write.Write<ushort>("Character", "PkPoints", client.Player.PKPoints);
                write.Write<uint>("Character", "JailerUID", client.Player.JailerUID);
                write.Write<uint>("Character", "QuizPoints", client.Player.QuizPoints);
                write.Write<byte>("Character", "VipLevel", client.Player.VipLevel);
                write.Write<long>("Character", "VipTime", client.Player.ExpireVip.Ticks);
                write.Write<uint>("Character", "VotePoints", client.Player.VotePoints);
                write.Write<long>("Character", "LastDragonPill", client.Player.LastDragonPill.Ticks);
                write.Write<long>("Character", "WHMoney", client.Player.WHMoney);
                write.Write<uint>("Character", "BlessTime", client.Player.BlessTime);
                write.Write<uint>("Character", "SpouseUID", client.Player.SpouseUID);
                write.Write<int>("Character", "HeavenBlessing", client.Player.HeavenBlessing);
                write.Write<uint>("Character", "LostTimeBlessing", client.Player.HeavenBlessTime.Value);
                write.Write<uint>("Character", "HuntingBlessing", client.Player.HuntingBlessing);
                write.Write<uint>("Character", "OnlineTrainingPoints", client.Player.OnlineTrainingPoints);
                write.Write<long>("Character", "JoinOnflineTG", client.Player.JoinOnflineTG.Ticks);
                write.Write<int>("Character", "Day", client.Player.Day);
                write.Write<byte>("Character", "BDExp", client.Player.BDExp);
                write.Write<uint>("Character", "RateExp", client.Player.RateExp);
                write.Write<uint>("Character", "DExpTime", client.Player.DExpTime);
                write.Write<byte>("Character", "ExpBallUsed", client.Player.ExpBallUsed);
                write.Write<uint>("Character", "TempGarmentID", client.TempGarmentID);
                write.WriteString("Character", "Flowers", client.Player.Flowers.ToString());
                write.Write<ulong>("Character", "DonationNobility", client.Player.Nobility.Donation);
                write.Write<uint>("Character", "GuildID", client.Player.GuildID);
                write.Write<ushort>("Character", "GuildRank", (ushort)client.Player.GuildRank);
                if (client.Player.MyGuildMember != null)
                {
                    write.Write<long>("Character", "MoneyDonate", client.Player.MyGuildMember.MoneyDonate);
                }
                write.Write<byte>("Character", "FRL", client.Player.FirstRebornLevel);
                write.Write<byte>("Character", "SRL", client.Player.SecoundeRebornLevel);
                write.Write<byte>("Character", "LotteryEntries", client.Player.LotteryEntries);
                write.Write<bool>("Character", "DbTry", client.Player.DbTry);
                write.WriteString("Character", "DemonEx", client.DemonExterminator.ToString());
                write.Write<int>("Character", "Cursed", client.Player.CursedTimer);
                write.Write<uint>("Character", "AparenceType", client.Player.AparenceType);
                write.Write<uint>("Character", "TKills", client.Player.TournamentKills);
                write.WriteString("Character", "SecurityPass", GetSecurityPassword(client));
                write.Write<byte>("Character", "TCT", (byte)client.Player.TCCaptainTimes);
                write.Write<ushort>("Character", "NameEditCount", client.Player.NameEditCount);
                write.Write<uint>("Character", "Quest2rbS2Point", client.Player.Quest2rbS2Point);
                write.Write<byte>("Character", "Quest2rbBossesOrderby", client.Player.Quest2rbBossesOrderby);
                write.Write<uint>("Character", "ExpProtection", client.Player.ExpProtection);
                write.Write<ushort>("Character", "ExtraAtributes", client.Player.ExtraAtributes);
                write.Write<byte>("Character", "OpenHousePack", client.Player.OpenHousePack);
                write.Write<byte>("Character", "Quest2rbStage", client.Player.Quest2rbStage);
                write.Write<uint>("Character", "OnlinePoints", client.Player.OnlinePoints);
                write.Write<uint>("Character", "TotalMobsKilled", client.TotalMobsKilled);
                write.Write<byte>("Character", "IsClaimTheItem", client.Player.IsClaimTheItem);
                write.Write<int>("Character", "DragonPills", client.Player.DragonPills);
                write.Write<uint>("Character", "Merchant", client.Player.Merchant);
                write.Write<byte>("Character", "QuestLevel", client.Player.QuestLevel);
                write.Write<uint>("Character", "TotalMobsLevel", client.TotalMobsLevel);
                write.Write<long>("Character", "MerchantApplicationEnd", client.Player.MerchantApplicationEnd.ToBinary());
                write.Write<long>("Character", "TreasureBoxesPoint", client.Player.TreasureBoxesPoint);
                write.Write<bool>("Character", "GuildBeastClaimd", client.Player.GuildBeastClaimd);
                write.Write<bool>("Character", "SpawnGuildBeast", client.Player.SpawnGuildBeast);
                write.Write<bool>("Character", "PlayerHasItemTime", client.Player.PlayerHasItemTime);
                write.Write<bool>("Character", "claimsilvercup", client.Player.claimsilvercup);
                write.WriteString("Character", "PkName", client.Player.MyKillerName);
                write.Write<bool>("Character", "CanClaimFreeVip", client.Player.CanClaimFreeVip);
                write.Write<uint>("Character", "DbKilled", client.DbKilled);
                write.Write<uint>("Character", "Drop_Meteors", client.Drop_Meteors);
                write.Write<uint>("Character", "Drop_Stone", client.Drop_Stone);
                write.Write<int>("Character", "DepositMets", client.Player.DepositMets);
                write.Write<int>("Character", "DepositSMets", client.Player.DepositSMets);
                write.Write<int>("Character", "DepositDbs", client.Player.DepositDbs);
                write.Write<int>("Character", "DepositSDbs", client.Player.DepositSDbs);

                write.Write<int>("Character", "DepositStone1", client.Player.DepositStone1);
                write.Write<int>("Character", "DepositStone2", client.Player.DepositStone2);
                write.Write<int>("Character", "DepositStone3", client.Player.DepositStone3);
                write.Write<int>("Character", "DepositStone4", client.Player.DepositStone4);

                write.Write<int>("Character", "NormalPhoenixGem", client.Player.NormalPhoenixGem);
                write.Write<int>("Character", "NormalDragonGem", client.Player.NormalDragonGem);
                write.Write<int>("Character", "NormalFuryGem", client.Player.NormalFuryGem);
                write.Write<int>("Character", "NormalRainbowGem", client.Player.NormalRainbowGem);
                write.Write<int>("Character", "NormalKylinGem", client.Player.NormalKylinGem);
                write.Write<int>("Character", "NormalVioletGem", client.Player.NormalVioletGem);
                write.Write<int>("Character", "NormalMoonGem", client.Player.NormalMoonGem);
                write.Write<int>("Character", "NormalTortoiseGem", client.Player.NormalTortoiseGem);


                SaveClientItems(client);
                SaveClientSpells(client);
                SaveClientProfs(client);
                client.EffectStatus.Save();
                Role.Instance.House.Save(client);

                if ((client.ClientFlag & Client.ServerFlag.Disconnect) == Client.ServerFlag.Disconnect)
                {
                    Client.GameClient user;
                    Database.Server.GamePoll.TryRemove(client.Player.UID, out user);
                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }

        }
        public static string GetSecurityPassword(Client.GameClient user)
        {
            Database.DBActions.WriteLine writer = new DBActions.WriteLine(',');
            writer.Add(user.Player.SecurityPassword);
            writer.Add(user.Player.OnReset);
            writer.Add(user.Player.ResetSecurityPassowrd.Ticks);
            return writer.Close();
        }
        public static void LoadSecurityPassword(string line, Client.GameClient user)
        {
            Database.DBActions.ReadLine reader = new DBActions.ReadLine(line, ',');
            user.Player.SecurityPassword = reader.Read((uint)0);
            user.Player.OnReset = reader.Read((uint)0);
            if (user.Player.OnReset == 1)
            {
                user.Player.ResetSecurityPassowrd = DateTime.FromBinary(reader.Read((long)0));
                if (DateTime.Now > user.Player.ResetSecurityPassowrd)
                {
                    user.Player.OnReset = 0;
                    user.Player.SecurityPassword = 0;
                }
            }

        }


        public static void LoadCharacter(Client.GameClient client, uint UID)
        {
            client.Player.UID = UID;
            WindowsAPI.IniFile reader = new WindowsAPI.IniFile("\\Users\\" + UID + ".ini");
            client.Player.Body = reader.ReadUInt16("Character", "Body", 1002);
            client.Player.Face = reader.ReadUInt16("Character", "Face", 0);
            client.Player.Name = reader.ReadString("Character", "Name", "None");
            client.Player.Spouse = reader.ReadString("Character", "Spouse", "None");
            client.Player.Class = reader.ReadByte("Character", "Class", 0);
            client.Player.FirstClass = reader.ReadByte("Character", "FirstClass", 0);
            client.Player.SecondClass = reader.ReadByte("Character", "SecoundeClass", 0);
            client.Player.Avatar = reader.ReadUInt16("Character", "Avatar", 0);
            client.Player.Map = reader.ReadUInt32("Character", "Map", 1002);
            client.Player.X = reader.ReadUInt16("Character", "X", 248);
            client.Player.Y = reader.ReadUInt16("Character", "Y", 238);
            client.TempGarmentID = reader.ReadUInt32("Character", "TempGarmentID", 0);
            client.Player.PMap = reader.ReadUInt32("Character", "PMap", 1002);
            client.Player.PMapX = reader.ReadUInt16("Character", "PMapX", 300);
            client.Player.PMapY = reader.ReadUInt16("Character", "PMapY", 300);
            client.CountVote = reader.ReadUInt32("Character", "CountVote", 0);
            client.Player.Agility = reader.ReadUInt16("Character", "Agility", 0);
            client.Player.Strength = reader.ReadUInt16("Character", "Strength", 0);
            client.Player.Spirit = reader.ReadUInt16("Character", "Spirit", 0);
            client.Player.Vitality = reader.ReadUInt16("Character", "Vitaliti", 0);
            client.Player.Atributes = reader.ReadUInt16("Character", "Atributes", 0);
            client.Player.Reborn = reader.ReadByte("Character", "Reborn", 0);
            client.Player.Level = reader.ReadUInt16("Character", "Level", 0);
            client.Player.Hair = reader.ReadUInt16("Character", "Haire", 0);
            client.Player.Experience = (ulong)reader.ReadInt64("Character", "Experience", 0);
            client.Player.HitPoints = reader.ReadInt32("Character", "MinHitPoints", 0);
            client.Player.Mana = reader.ReadUInt16("Character", "MinMana", 0);
            client.Player.ConquerPoints = reader.ReadUInt32("Character", "ConquerPoints", 0);

            client.Player.BoundConquerPoints = reader.ReadInt32("Character", "BoundConquerPoints", 0);
            client.Player.Money = reader.ReadUInt32("Character", "Money", 0);
            client.Player.VirtutePoints = reader.ReadUInt32("Character", "VirtutePoints", 0);
            client.Player.PKPoints = reader.ReadUInt16("Character", "PkPoints", 0);
            client.Player.JailerUID = reader.ReadUInt32("Character", "JailerUID", 0);
            client.Player.MyKillerName = reader.ReadString("Character", "PkName", "None");

            client.Player.QuizPoints = reader.ReadUInt32("Character", "QuizPoints", 0);
            client.Player.VipLevel = reader.ReadByte("Character", "VipLevel", 0);
            client.Player.ExpireVip = DateTime.FromBinary(reader.ReadInt64("Character", "VipTime", 0));
            client.Player.VotePoints = reader.ReadUInt32("Character", "VotePoints", 0);
            client.Player.LastDragonPill = DateTime.FromBinary(reader.ReadInt64("Character", "LastDragonPill", 0));
            if (DateTime.Now > client.Player.ExpireVip)
            {
                if (client.Player.VipLevel > 1)
                    client.Player.VipLevel = 0;
            }
            client.Player.WHMoney = reader.ReadInt64("Character", "WHMoney", 0);
            client.Player.BlessTime = reader.ReadUInt32("Character", "BlessTime", 0);
            client.Player.SpouseUID = reader.ReadUInt32("Character", "SpouseUID", 0);
            client.Player.HeavenBlessing = reader.ReadInt32("Character", "HeavenBlessing", 0);
            client.Player.HeavenBlessTime = new Time32(reader.ReadUInt32("Character", "LostTimeBlessing", 0));
            client.Player.HuntingBlessing = reader.ReadUInt32("Character", "HuntingBlessing", 0);
            client.Player.OnlineTrainingPoints = reader.ReadUInt32("Character", "OnlineTrainingPoints", 0);
            client.Player.JoinOnflineTG = DateTime.FromBinary(reader.ReadInt64("Character", "JoinOnflineTG", 0));
            client.Player.RateExp = reader.ReadUInt32("Character", "RateExp", 0);
            client.Player.DExpTime = reader.ReadUInt32("Character", "DExpTime", 0);
            client.Player.Day = reader.ReadInt32("Character", "Day", 0);
            client.Player.BDExp = reader.ReadByte("Character", "BDExp", 0);
            client.Player.ExpBallUsed = reader.ReadByte("Character", "ExpBallUsed", 0);
            client.DbKilled = reader.ReadUInt32("Character", "DbKilled", 0);
            client.Drop_Meteors = reader.ReadUInt32("Character", "Drop_Meteors", 0);
            client.Drop_Stone = reader.ReadUInt32("Character", "Drop_Stone", 0);
            DataCore.LoadClient(client.Player);
            client.Player.GuildID = reader.ReadUInt32("Character", "GuildID", 0);
            client.Player.GuildRank = (Role.Flags.GuildMemberRank)reader.ReadByte("Character", "GuildRank", 50);
            if (client.Player.GuildID != 0)
            {
                Role.Instance.Guild myguild;
                if (Role.Instance.Guild.GuildPoll.TryGetValue(client.Player.GuildID, out myguild))
                {
                    client.Player.MyGuild = myguild;
                    Role.Instance.Guild.Member member;
                    if (myguild.Members.TryGetValue(client.Player.UID, out member))
                    {
                        member.IsOnline = true;
                        client.Player.GuildID = (ushort)myguild.Info.GuildID;
                        client.Player.MyGuildMember = member;
                        client.Player.GuildRank = member.Rank;


                    }
                    else
                    {
                        client.Player.MyGuild = null;
                        client.Player.GuildID = 0;
                        client.Player.GuildRank = (Role.Flags.GuildMemberRank)0;
                    }
                }
                else
                {
                    client.Player.MyGuild = null;
                    client.Player.GuildID = 0;
                    client.Player.GuildRank = (Role.Flags.GuildMemberRank)0;
                }
            }


            if (Role.Instance.Flowers.ClientPoll.ContainsKey(UID))
                client.Player.Flowers = Role.Instance.Flowers.ClientPoll[UID];
            else
                client.Player.Flowers = new Role.Instance.Flowers(UID, client.Player.Name, Role.Core.IsGirl(client.Player.Body));
            string flowerStr = reader.ReadString("Character", "Flowers", "");
            Database.DBActions.ReadLine Linereader = new DBActions.ReadLine(flowerStr, '/');
            client.Player.Flowers.FreeFlowers = Linereader.Read((uint)0);



            Role.Instance.Nobility nobility;
            if (Program.NobilityRanking.TryGetValue(UID, out nobility))
            {
                client.Player.Nobility = nobility;
                client.Player.NobilityRank = client.Player.Nobility.Rank;
            }
            else
            {
                client.Player.Nobility = new Role.Instance.Nobility(client);
                client.Player.Nobility.Donation = reader.ReadUInt64("Character", "DonationNobility", 0);
                client.Player.NobilityRank = client.Player.Nobility.Rank;
            }
            Role.Instance.Associate.MyAsociats Associate;
            if (Role.Instance.Associate.Associates.TryGetValue(client.Player.UID, out Associate))
            {
                client.Player.Associate = Associate;
                client.Player.Associate.MyClient = client;
                client.Player.Associate.Online = true;
                if (client.Player.Associate.Associat.ContainsKey(Role.Instance.Associate.Mentor))
                {
                    foreach (var member in client.Player.Associate.Associat[Role.Instance.Associate.Mentor].Values)
                    {
                        if (member.UID != 0)
                        {
                            Role.Instance.Associate.MyAsociats mentor;
                            if (Role.Instance.Associate.Associates.TryGetValue(member.UID, out mentor))
                            {
                                client.Player.MyMentor = mentor;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                client.Player.Associate = new Role.Instance.Associate.MyAsociats(client.Player.UID);
                client.Player.Associate.MyClient = client;
                client.Player.Associate.Online = true;
            }
            client.Player.FirstRebornLevel = reader.ReadByte("Character", "FRL", 0);
            client.Player.SecoundeRebornLevel = reader.ReadByte("Character", "SRL", 0);
            client.Player.LotteryEntries = reader.ReadByte("Character", "LotteryEntries", 0);
            client.Player.DbTry = reader.ReadBool("Character", "DbTry", false);
            client.DemonExterminator.ReadLine(reader.ReadString("Character", "DemonEx", "0/0/"));
            client.Player.CursedTimer = reader.ReadInt32("Character", "Cursed", 0);
            client.Player.Quest2rbS2Point = reader.ReadUInt32("Character", "Quest2rbS2Point", 0);
            client.Player.Quest2rbBossesOrderby = reader.ReadByte("Character", "Quest2rbBossesOrderby", 0);
            client.Player.AparenceType = reader.ReadUInt32("Character", "AparenceType", 0);
            client.Player.TournamentKills = reader.ReadUInt32("Character", "TKills", 0);
            LoadSecurityPassword(reader.ReadString("Character", "SecurityPass", "0,0,0"), client);
            client.Player.TCCaptainTimes = reader.ReadByte("Character", "TCT", 0);
            client.Player.NameEditCount = reader.ReadUInt16("Character", "NameEditCount", 0);
            client.Player.ExpProtection = reader.ReadUInt32("Character", "ExpProtection", 0);
            client.Player.ExtraAtributes = reader.ReadUInt16("Character", "ExtraAtributes", 0);
            client.Player.OpenHousePack = reader.ReadByte("Character", "OpenHousePack", 0);
            client.Player.Quest2rbStage = reader.ReadByte("Character", "Quest2rbStage", 0);
            client.Player.OnlinePoints = reader.ReadUInt32("Character", "OnlinePoints", 0);
            client.TotalMobsKilled = reader.ReadUInt32("Character", "TotalMobsKilled", 0);
            client.Player.DragonPills = reader.ReadInt32("Character", "DragonPills", 0);
            client.TotalMobsLevel = reader.ReadUInt32("Character", "TotalMobsLevel", 0);
            client.Player.QuestLevel = reader.ReadByte("Character", "QuestLevel", 0);
            client.Player.MerchantApplicationEnd = DateTime.FromBinary(reader.ReadInt64("Character", "MerchantApplicationEnd", 0));
            client.Player.Merchant = reader.ReadUInt32("Character", "Merchant", 0);
            client.Player.IsClaimTheItem = reader.ReadByte("Character", "IsClaimTheItem", 0); 
            client.Player.TreasureBoxesPoint = reader.ReadUInt32("Character", "TreasureBoxesPoint", 0);
            client.Player.GuildBeastClaimd = reader.ReadBool("Character", "GuildBeastClaimd", false);
            client.Player.SpawnGuildBeast = reader.ReadBool("Character", "SpawnGuildBeast", false);
            client.Player.PlayerHasItemTime = reader.ReadBool("Character", "PlayerHasItemTime", false);
            client.Player.claimsilvercup = reader.ReadBool("Character", "claimsilvercup", false);
            client.Player.CanClaimFreeVip = reader.ReadBool("Character", "CanClaimFreeVip", false);
            client.Player.DepositMets = reader.ReadInt32("Character", "DepositMets", 0);
            client.Player.DepositSMets = reader.ReadInt32("Character", "DepositSMets", 0);
            client.Player.DepositDbs = reader.ReadInt32("Character", "DepositDbs", 0);
            client.Player.DepositSDbs = reader.ReadInt32("Character", "DepositSDbs", 0);
            client.Player.DepositStone1 = reader.ReadInt32("Character", "DepositStone1", 0);
            client.Player.DepositStone2 = reader.ReadInt32("Character", "DepositStone2", 0);
            client.Player.DepositStone3 = reader.ReadInt32("Character", "DepositStone3", 0);
            client.Player.DepositStone4 = reader.ReadInt32("Character", "DepositStone4", 0);

            client.Player.NormalPhoenixGem = reader.ReadInt32("Character", "NormalPhoenixGem", 0);
            client.Player.NormalDragonGem = reader.ReadInt32("Character", "NormalDragonGem", 0);
            client.Player.NormalFuryGem = reader.ReadInt32("Character", "NormalFuryGem", 0);
            client.Player.NormalRainbowGem = reader.ReadInt32("Character", "NormalRainbowGem", 0);
            client.Player.NormalKylinGem = reader.ReadInt32("Character", "NormalKylinGem", 0);
            client.Player.NormalVioletGem = reader.ReadInt32("Character", "NormalVioletGem", 0);
            client.Player.NormalMoonGem = reader.ReadInt32("Character", "NormalMoonGem", 0);
            client.Player.NormalTortoiseGem = reader.ReadInt32("Character", "NormalTortoiseGem", 0);


            LoadClientItems(client);
            LoadClientSpells(client);
            LoadClientProfs(client);
            Role.Instance.House.Load(client);
            ResetingEveryDay(client);

            
            Role.Instance.Confiscator Container;
            if (Server.QueueContainer.PollContainers.TryGetValue(client.Player.UID, out Container))
                client.Confiscator = Container;
            try
            {
                client.Player.Associate.OnLoading(client);
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }

            client.FullLoading = true;
        }
        public unsafe static void LoadDBPackets()
        {
            Program.LoadPackets.Clear();
            var arraybuffer = File.ReadAllBytes(Program.ServerConfig.DbLocation + "\\array0.bin");
            int count = BitConverter.ToInt32(arraybuffer, 0);

            var dd = 0;
            int offset = 4;
            for (int x = 0; x < count; x++)
            {
                try
                {

                    ushort size = BitConverter.ToUInt16(arraybuffer, offset);
                    byte[] packet = new byte[size];
                    Buffer.BlockCopy(arraybuffer, offset, packet, 0, size);
                    offset += size;
                    dd++;
                    //   if (dd >= 2)
                    {
                        Program.LoadPackets.Add(packet);
                    }
                }
                catch
                {
                    //    break;
                }
            }

            /* WindowsAPI.BinaryFile binary = new WindowsAPI.BinaryFile();
             if (binary.Open(Program.ServerConfig.DbLocation + "\\array320.bin", System.IO.FileMode.Open))
             {
                 int count;
                 binary.Read(&count, 4);
                 for (int x = 0; x < count; x++)
                 {
                     int size;
                     binary.Read(&size, sizeof(ushort));

                     byte[] buff = new byte[size];
                     int size2 = size;
                     byte* ptr = (byte*)System.Runtime.InteropServices.Marshal.AllocHGlobal(size);
                     binary.Read(&ptr, size2);

                     fixed (byte* tt = buff)
                     {
                         WindowsAPI.Kernel32.memcpy(tt, ptr, size2);
                     }
                     Program.LoadPackets.Add(buff);
                 }
                 binary.Close();
             }*/

        }

        public unsafe static void LoadClientItems(Client.GameClient client)
        {
            try
            {

                WindowsAPI.BinaryFile binary = new WindowsAPI.BinaryFile();
                if (binary.Open(Program.ServerConfig.DbLocation + "\\PlayersItems\\" + client.Player.UID + ".bin", FileMode.Open))
                {
                    ClientItems.DBItem Item;
                    int ItemCount;
                    binary.Read(&ItemCount, sizeof(int));
                    for (int x = 0; x < ItemCount; x++)
                    {
                        binary.Read(&Item, sizeof(ClientItems.DBItem));
                        if (Item.ITEM_ID == 750000)//demonExterminator jar
                            client.DemonExterminator.ItemUID = Item.UID;

                        Game.MsgServer.MsgGameItem ClienItem = Item.GetDataItem();

                        //Console.WriteLine($"Load >>> ITEMCOUNT {x} >> user id {client.Player.UID} ID = {ClienItem.UID} / UID = {ClienItem.ITEM_ID} / Postion = {ClienItem.Position}" +
                        //    $"/ Plus = {ClienItem.Plus} / StackSize = {ClienItem.StackSize} / WHID = {ClienItem.WH_ID}");
                        if (Database.ItemType.ItemPosition(Item.ITEM_ID) == (ushort)Role.Flags.ConquerItem.Garment
                            || Database.ItemType.ItemPosition(Item.ITEM_ID) == (ushort)Role.Flags.ConquerItem.Bottle)
                        {
                            if (Item.Bless > 1)
                                Item.Bless = 1;
                        }
                     /*   if (Item.ITEM_ID == 720598 || (Item.ITEM_ID >= 2100065 && Item.ITEM_ID <= 2100095))
                            ClienItem.Bound = 1;
                        */
                        if (Item.WH_ID != 0)
                        {
                            if (Item.WH_ID == 100)
                            {
                                if (Item.Position > 0 /*&& Item.Position <= (ushort)Role.Flags.ConquerItem.AleternanteGarment*/)
                                {
                                    client.Equipment.ClientItems.TryAdd(Item.UID, ClienItem);
                                }
                            }
                            else
                            {
                                if (!client.Warehouse.ClientItems.ContainsKey(Item.WH_ID))
                                    client.Warehouse.ClientItems.TryAdd(Item.WH_ID, new System.Collections.Concurrent.ConcurrentDictionary<uint, Game.MsgServer.MsgGameItem>());
                                client.Warehouse.ClientItems[Item.WH_ID].TryAdd(Item.UID, ClienItem);
                            }
                        }
                        else
                        {
                            if (Item.Position > 0 && Item.Position <= (ushort)Role.Flags.ConquerItem.Tower)
                            {
                                client.Equipment.ClientItems.TryAdd(Item.UID, ClienItem);
                            }
                            else if (Item.Position == 0)
                            {
                                client.Inventory.AddDBItem(ClienItem);
                            }
                        }
                    }
                    binary.Read(&ItemCount, sizeof(int));
                    binary.Close();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine($"userid = {client.Player.UID} messaage {e.Message} tostr >>> {e.ToString()}");
            }
        }

        public unsafe static void SaveClientItems(Client.GameClient client)
        {
            try
            {
                //Console.WriteLine($"clientitemsaved {client.Player.UID}");
                WindowsAPI.BinaryFile binary = new WindowsAPI.BinaryFile();
                if (binary.Open(Program.ServerConfig.DbLocation + "\\PlayersItems\\" + client.Player.UID + ".bin", FileMode.Create))
                {
                    ClientItems.DBItem DBItem = new ClientItems.DBItem();
                    int ItemCount;
                    ItemCount = client.GetItemsCount();
                    binary.Write(&ItemCount, sizeof(int));
                    foreach (var ClienItem in client.AllMyItems())
                    {
                        DBItem.GetDBItem(ClienItem);
                        //Console.WriteLine($"save >>>>>>> >>user id {client.Player.UID} / ID = {ClienItem.UID} / UID = {ClienItem.ITEM_ID} / Postion = {ClienItem.Position}" +
                        //   $"/ Plus = {ClienItem.Plus} / StackSize = {ClienItem.StackSize} / WHID = {ClienItem.WH_ID}");

                        if (!binary.Write(&DBItem, sizeof(ClientItems.DBItem)))
                            Console.WriteLine("test");
                    }
                    binary.Write(&ItemCount, sizeof(int));
                    binary.Close();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine($"userid = {client.Player.UID} messaage {e.Message} tostr >>> {e.ToString()}");
            }
        }
        public unsafe static void LoadClientProfs(Client.GameClient client)
        {
            WindowsAPI.BinaryFile binary = new WindowsAPI.BinaryFile();
            if (binary.Open(Program.ServerConfig.DbLocation + "\\PlayersProfs\\" + client.Player.UID + ".bin", FileMode.Open))
            {
                ClientProficiency.DBProf DBProf;
                int CountProf;
                binary.Read(&CountProf, sizeof(int));
                for (int x = 0; x < CountProf; x++)
                {
                    binary.Read(&DBProf, sizeof(ClientProficiency.DBProf));
                    var ClientProf = DBProf.GetClientProf();
                    client.MyProfs.ClientProf.TryAdd(ClientProf.ID, ClientProf);
                }
                binary.Close();
            }
        }
        public unsafe static void SaveClientProfs(Client.GameClient client)
        {
            WindowsAPI.BinaryFile binary = new WindowsAPI.BinaryFile();
            if (binary.Open(Program.ServerConfig.DbLocation + "\\PlayersProfs\\" + client.Player.UID + ".bin", FileMode.Create))
            {
                ClientProficiency.DBProf DBProf = new ClientProficiency.DBProf();
                int CountProf;
                CountProf = client.MyProfs.ClientProf.Count;
                binary.Write(&CountProf, sizeof(int));
                foreach (var prof in client.MyProfs.ClientProf.Values)
                {
                    DBProf.GetDBSpell(prof);
                    binary.Write(&DBProf, sizeof(ClientProficiency.DBProf));
                }
                binary.Close();
            }
        }
        public unsafe static void LoadClientSpells(Client.GameClient client)
        {
            WindowsAPI.BinaryFile binary = new WindowsAPI.BinaryFile();
            if (binary.Open(Program.ServerConfig.DbLocation + "\\PlayersSpells\\" + client.Player.UID + ".bin", FileMode.Open))
            {
                ClientSpells.DBSpell DBSpell;
                int CountSpell;
                binary.Read(&CountSpell, sizeof(int));
                for (int x = 0; x < CountSpell; x++)
                {
                    binary.Read(&DBSpell, sizeof(ClientSpells.DBSpell));
                    var clientSpell = DBSpell.GetClientSpell();
                    client.MySpells.ClientSpells.TryAdd(clientSpell.ID, clientSpell);
                }
                binary.Close();
            }
        }
        public unsafe static void SaveClientSpells(Client.GameClient client)
        {
            WindowsAPI.BinaryFile binary = new WindowsAPI.BinaryFile();
            if (binary.Open(Program.ServerConfig.DbLocation + "\\PlayersSpells\\" + client.Player.UID + ".bin", FileMode.Create))
            {
                ClientSpells.DBSpell DBSpell = new ClientSpells.DBSpell();
                int SpellCount;
                SpellCount = client.MySpells.ClientSpells.Count;
                binary.Write(&SpellCount, sizeof(int));
                foreach (var spell in client.MySpells.ClientSpells.Values)
                {
                    DBSpell.GetDBSpell(spell);
                    binary.Write(&DBSpell, sizeof(ClientSpells.DBSpell));
                }
                binary.Close();
            }
        }
        public static void CreateCharacte(Client.GameClient client)
        {
            WindowsAPI.IniFile write = new WindowsAPI.IniFile("\\Users\\" + client.Player.UID + ".ini");
            write.Write<uint>("Character", "UID", client.Player.UID);
            write.Write<ushort>("Character", "Body", client.Player.Body);
            write.Write<ushort>("Character", "Face", client.Player.Face);
            write.WriteString("Character", "Name", client.Player.Name);
            write.Write<byte>("Character", "Class", client.Player.Class);
            write.Write<uint>("Character", "Map", client.Player.Map);
            write.Write<ushort>("Character", "X", client.Player.X);
            write.Write<ushort>("Character", "Y", client.Player.Y);

            client.Player.Nobility = new Role.Instance.Nobility(client);

            client.Player.Associate = new Role.Instance.Associate.MyAsociats(client.Player.UID);
            client.Player.Associate.MyClient = client;
            client.Player.Associate.Online = true;


            client.Player.Flowers = new Role.Instance.Flowers(client.Player.UID, client.Player.Name, Role.Core.IsGirl(client.Player.Body));
            client.FullLoading = true;
        }

        public static bool AllowCreate(uint UID)
        {
            return !File.Exists(Program.ServerConfig.DbLocation + "\\Users\\" + UID + ".ini");
        }
        public static void UpdateGuildMember(Role.Instance.Guild.Member Member)
        {
            WindowsAPI.IniFile write = new WindowsAPI.IniFile("\\Users\\" + Member.UID + ".ini");
            //  write.Write<uint>("Character", "GuildID", 0);
            write.Write<ushort>("Character", "GuildRank", 0);
        }
        public static void UpdateGuildMember(Role.Instance.Guild.UpdateDB member)
        {
            WindowsAPI.IniFile write = new WindowsAPI.IniFile("\\Users\\" + member.UID + ".ini");
            write.Write<ushort>("Character", "GuildRank", 0);
            write.Write<ushort>("Character", "GuildID", 0);
        }

        public static void UpdateMapRace(Role.GameMap map)
        {
            WindowsAPI.IniFile write = new WindowsAPI.IniFile("\\maps\\" + map.ID + ".ini");
            write.Write<uint>("info", "race_record", map.RecordSteedRace);
        }
        public static void DestroySpouse(Client.GameClient client)
        {
            if (client.Player.SpouseUID != 0)
            {
                WindowsAPI.IniFile write = new WindowsAPI.IniFile("\\Users\\" + client.Player.SpouseUID + ".ini");
                write.Write<uint>("Character", "SpouseUID", 0);
                write.WriteString("Character", "Spouse", "None");

                client.Player.SpouseUID = 0;
            }

            client.Player.Spouse = "None";
            using (var rec = new ServerSockets.RecycledPacket())
            {
                var stream = rec.GetStream();
                client.Player.SendString(stream, Game.MsgServer.MsgStringPacket.StringID.Spouse, false, new string[1] { "None" });
            }
        }
        public static string GenerateDate()
        {
            DateTime now = DateTime.Now;
            return now.Year.ToString() + "and" + now.Month.ToString() + "and" + now.Day.ToString() + "and" + now.Hour.ToString() + "and" + now.Minute.ToString() + "and" + now.Second.ToString();
        }
        public static void UpdateSpouse(Client.GameClient client)
        {
            if (client.Player.SpouseUID != 0)
            {
                WindowsAPI.IniFile write = new WindowsAPI.IniFile("\\Users\\" + client.Player.SpouseUID + ".ini");
                write.WriteString("Character", "Spouse", client.Player.Name);
            }
        }
        public static ExecuteLogin LoginQueue = new ExecuteLogin();

        public class ExecuteLogin : ConcurrentSmartThreadQueue<object>
        {
            public object SynRoot = new object();
            public ExecuteLogin()
                : base(5)
            {
                Start(10);
            }
            public void TryEnqueue(object obj)
            {
                //  lock (SynRoot)
                {

                    base.Enqueue(obj);
                }
            }
            protected unsafe override void OnDequeue(object obj, int time)
            {
                try
                {
                    var runDir = Application.StartupPath;

                    if (obj is string)
                    {
                        string text = obj as string;
                        if (text.StartsWith("[PHLogs]"))
                        {
                            string[] index = text.Split('@');
                            const string UnhandledExceptionsPath = "PHLoggs\\";
                            if (!Directory.Exists(runDir + UnhandledExceptionsPath))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath);
                            if (!Directory.Exists(runDir + "\\" + UnhandledExceptionsPath))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath);
                            if (!Directory.Exists(runDir + "\\" + UnhandledExceptionsPath))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath);
                            string fullPath = runDir + "\\" + UnhandledExceptionsPath + "\\";
                            if (!File.Exists(fullPath + index[1] + ".txt"))
                            {
                                File.WriteAllLines(fullPath + index[1] + ".txt", new string[0]);
                            }
                            using (var SW = File.AppendText(fullPath + index[1] + ".txt"))
                            {
                                SW.WriteLine(text);
                                SW.Close();
                            }
                        }
                        else if (text.StartsWith("[DemonBox]"))
                        {

                            const string UnhandledExceptionsPath = "Loggs\\";

                            var dt = DateTime.Now;
                            string date = "DemonBox" + dt.Month + "-" + dt.Day + "";

                            if (!Directory.Exists(runDir + UnhandledExceptionsPath))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath);
                            if (!Directory.Exists(runDir + "\\" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath + date);
                            if (!Directory.Exists(runDir + "\\" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath + date);

                            string fullPath = runDir + "\\" + UnhandledExceptionsPath + date + "\\";

                            if (!File.Exists(fullPath + date + ".txt"))
                            {
                                File.WriteAllLines(fullPath + date + ".txt", new string[0]);
                            }


                            using (var SW = File.AppendText(fullPath + date + ".txt"))
                            {
                                SW.WriteLine(text);
                                SW.Close();
                            }
                        }
                        else if (text.StartsWith("[Chat]"))
                        {

                            const string UnhandledExceptionsPath = "Loggs\\";

                            var dt = DateTime.Now;
                            string date = "Chat" + dt.Month + "-" + dt.Day + "";

                            if (!Directory.Exists(runDir + UnhandledExceptionsPath))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath);
                            if (!Directory.Exists(runDir + "\\" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath + date);
                            if (!Directory.Exists(runDir + "\\" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath + date);

                            string fullPath = runDir + "\\" + UnhandledExceptionsPath + date + "\\";

                            if (!File.Exists(fullPath + date + ".txt"))
                            {
                                File.WriteAllLines(fullPath + date + ".txt", new string[0]);
                            }


                            using (var SW = File.AppendText(fullPath + date + ".txt"))
                            {
                                SW.WriteLine(text);
                                SW.Close();
                            }
                        }
                        else if (text.StartsWith("[EVENT]"))
                        {

                            const string UnhandledExceptionsPath = "EVENTLoggs\\";

                            var dt = DateTime.Now;
                            string date = "EVENT" + dt.Month + "-" + dt.Day + "";

                            if (!Directory.Exists(runDir + UnhandledExceptionsPath))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath);
                            if (!Directory.Exists(runDir + "\\" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath + date);
                            if (!Directory.Exists(runDir + "\\" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath + date);

                            string fullPath = runDir + "\\" + UnhandledExceptionsPath + date + "\\";

                            if (!File.Exists(fullPath + date + ".txt"))
                            {
                                File.WriteAllLines(fullPath + date + ".txt", new string[0]);
                            }


                            using (var SW = File.AppendText(fullPath + date + ".txt"))
                            {
                                SW.WriteLine(text);
                                SW.Close();
                            }
                        }
                        else if (text.StartsWith("[CHEAT]"))
                        {

                            const string UnhandledExceptionsPath = "Loggs\\";

                            var dt = DateTime.Now;
                            string date = "CHEAT" + dt.Month + "-" + dt.Day + "";

                            if (!Directory.Exists(runDir + UnhandledExceptionsPath))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath);
                            if (!Directory.Exists(runDir + "\\" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath + date);
                            if (!Directory.Exists(runDir + "\\" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath + date);

                            string fullPath = runDir + "\\" + UnhandledExceptionsPath + date + "\\";

                            if (!File.Exists(fullPath + date + ".txt"))
                            {
                                File.WriteAllLines(fullPath + date + ".txt", new string[0]);
                            }


                            using (var SW = File.AppendText(fullPath + date + ".txt"))
                            {
                                SW.WriteLine(text);
                                SW.Close();
                            }
                        }
                        else if (text.StartsWith("[Item]"))
                        {

                            const string UnhandledExceptionsPath = "Loggs\\";

                            var dt = DateTime.Now;
                            string date = "Item" + dt.Month + "-" + dt.Day + "";

                            if (!Directory.Exists(runDir + UnhandledExceptionsPath))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath);
                            if (!Directory.Exists(runDir + "\\" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath + date);
                            if (!Directory.Exists(runDir + "\\" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath + date);

                            string fullPath = runDir + "\\" + UnhandledExceptionsPath + date + "\\";

                            if (!File.Exists(fullPath + date + ".txt"))
                            {
                                File.WriteAllLines(fullPath + date + ".txt", new string[0]);
                            }


                            using (var SW = File.AppendText(fullPath + date + ".txt"))
                            {
                                SW.WriteLine(text);
                                SW.Close();
                            }
                        }
                        else if (text.StartsWith("[CallStack]"))
                        {

                            const string UnhandledExceptionsPath = "Loggs\\";

                            var dt = DateTime.Now;
                            string date = "CallStack" + dt.Month + "-" + dt.Day + "";

                            if (!Directory.Exists(runDir + UnhandledExceptionsPath))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath);
                            if (!Directory.Exists(runDir + "\\" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath + date);
                            if (!Directory.Exists(runDir + "\\" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath + date);

                            string fullPath = runDir + "\\" + UnhandledExceptionsPath + date + "\\";

                            if (!File.Exists(fullPath + date + ".txt"))
                            {
                                File.WriteAllLines(fullPath + date + ".txt", new string[0]);
                            }


                            using (var SW = File.AppendText(fullPath + date + ".txt"))
                            {
                                SW.WriteLine(text);
                                SW.Close();
                            }
                        }
                        else if (text.StartsWith("[EVENT]"))
                        {

                            const string UnhandledExceptionsPath = "Loggs\\";

                            var dt = DateTime.Now;
                            string date = "PVEPoints" + dt.Month + "-" + dt.Day + "";

                            if (!Directory.Exists(runDir + UnhandledExceptionsPath))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath);
                            if (!Directory.Exists(runDir + "\\" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath + date);
                            if (!Directory.Exists(runDir + "\\" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath + date);

                            string fullPath = runDir + "\\" + UnhandledExceptionsPath + date + "\\";

                            if (!File.Exists(fullPath + date + ".txt"))
                            {
                                File.WriteAllLines(fullPath + date + ".txt", new string[0]);
                            }


                            using (var SW = File.AppendText(fullPath + date + ".txt"))
                            {
                                SW.WriteLine(text);
                                SW.Close();
                            }
                        }
                        if (text.StartsWith("[GMLogs]") || text.StartsWith("[HDLogs]"))
                        {
                            const string UnhandledExceptionsPath = "Loggs\\";

                            var dt = DateTime.Now;
                            string date = "GMLogs";

                            if (!Directory.Exists(runDir + UnhandledExceptionsPath))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath);
                            if (!Directory.Exists(runDir + "\\" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath + date);
                            if (!Directory.Exists(runDir + "\\" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath + date);

                            string fullPath = runDir + "\\" + UnhandledExceptionsPath + date + "\\";

                            if (!File.Exists(fullPath + date + ".txt"))
                            {
                                File.WriteAllLines(fullPath + date + ".txt", new string[0]);
                            }


                            using (var SW = File.AppendText(fullPath + date + ".txt"))
                            {
                                SW.WriteLine(text);
                                SW.Close();
                            }
                        }
                        if (text.StartsWith("[TradeConquerPoints]"))
                        {
                            const string UnhandledExceptionsPath = "Loggs\\";
                            var dt = DateTime.Now;
                            string date = "TradeConquerPoints";
                            if (!Directory.Exists(runDir + UnhandledExceptionsPath))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath);
                            if (!Directory.Exists(runDir + "\\" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath + date);
                            if (!Directory.Exists(runDir + "\\" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath + date);

                            string fullPath = runDir + "\\" + UnhandledExceptionsPath + date + "\\";

                            if (!File.Exists(fullPath + date + ".txt"))
                            {
                                File.WriteAllLines(fullPath + date + ".txt", new string[0]);
                            }


                            using (var SW = File.AppendText(fullPath + date + ".txt"))
                            {
                                SW.WriteLine(text);
                                SW.Close();
                            }
                        }
                        if (text.StartsWith("[TradeMoney]"))
                        {
                            const string UnhandledExceptionsPath = "Loggs\\";
                            var dt = DateTime.Now;
                            string date = "TradeMoney";
                            if (!Directory.Exists(runDir + UnhandledExceptionsPath))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath);
                            if (!Directory.Exists(runDir + "\\" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath + date);
                            if (!Directory.Exists(runDir + "\\" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath + date);

                            string fullPath = runDir + "\\" + UnhandledExceptionsPath + date + "\\";

                            if (!File.Exists(fullPath + date + ".txt"))
                            {
                                File.WriteAllLines(fullPath + date + ".txt", new string[0]);
                            }


                            using (var SW = File.AppendText(fullPath + date + ".txt"))
                            {
                                SW.WriteLine(text);
                                SW.Close();
                            }
                        }
                        if (text.StartsWith("[TradeItem]"))
                        {
                            const string UnhandledExceptionsPath = "Loggs\\";
                            var dt = DateTime.Now;
                            string date = "TradeItem";
                            if (!Directory.Exists(runDir + UnhandledExceptionsPath))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath);
                            if (!Directory.Exists(runDir + "\\" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath + date);
                            if (!Directory.Exists(runDir + "\\" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath + date);

                            string fullPath = runDir + "\\" + UnhandledExceptionsPath + date + "\\";

                            if (!File.Exists(fullPath + date + ".txt"))
                            {
                                File.WriteAllLines(fullPath + date + ".txt", new string[0]);
                            }


                            using (var SW = File.AppendText(fullPath + date + ".txt"))
                            {
                                SW.WriteLine(text);
                                SW.Close();
                            }
                        }
                        if (text.StartsWith("[FullTrade]"))
                        {
                            const string UnhandledExceptionsPath = "Loggs\\";
                            var dt = DateTime.Now;
                            string date = "FullTrade";
                            if (!Directory.Exists(runDir + UnhandledExceptionsPath))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath);
                            if (!Directory.Exists(runDir + "\\" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath + date);
                            if (!Directory.Exists(runDir + "\\" + UnhandledExceptionsPath + date))
                                Directory.CreateDirectory(runDir + "\\" + UnhandledExceptionsPath + date);

                            string fullPath = runDir + "\\" + UnhandledExceptionsPath + date + "\\";

                            if (!File.Exists(fullPath + date + ".txt"))
                            {
                                File.WriteAllLines(fullPath + date + ".txt", new string[0]);
                            }


                            using (var SW = File.AppendText(fullPath + date + ".txt"))
                            {
                                SW.WriteLine(text);
                                SW.Close();
                            }
                        }

                    }
                    //else if (obj is Role.GameMap)
                    //{
                    //    UpdateMapRace(obj as Role.GameMap);
                    //}
                    else if (obj is Role.Instance.Guild.Member)
                    {
                        UpdateGuildMember(obj as Role.Instance.Guild.Member);
                    }
                    else if (obj is Role.Instance.Guild.UpdateDB)
                    {
                        UpdateGuildMember(obj as Role.Instance.Guild.UpdateDB);
                    }
                    else
                    {
                        Client.GameClient client = obj as Client.GameClient;
                        if (client.Player != null && client.Player.Delete)
                        {
                            if (client.Map != null)
                                client.Map.View.LeaveMap<Role.Player>(client.Player);

                            DateTime Now64 = DateTime.Now;

                            Console.WriteLine("Client " + client.Player.Name + " delete he account.");
                            if (File.Exists(Program.ServerConfig.DbLocation + "\\Users\\" + client.Player.UID + ".ini"))
                                File.Copy(Program.ServerConfig.DbLocation + "\\Users\\" + client.Player.UID + ".ini", Program.ServerConfig.DbLocation + "\\BackUp\\Users\\" + client.Player.UID + "date" + GenerateDate() + ".ini", true);
                            if (File.Exists(Program.ServerConfig.DbLocation + "\\PlayersSpells\\" + client.Player.UID + ".bin"))
                                File.Copy(Program.ServerConfig.DbLocation + "\\PlayersSpells\\" + client.Player.UID + ".bin", Program.ServerConfig.DbLocation + "\\BackUp\\PlayersSpells\\" + client.Player.UID + "date" + GenerateDate() + ".bin", true);
                            if (File.Exists(Program.ServerConfig.DbLocation + "\\PlayersProfs\\" + client.Player.UID + ".bin"))
                                File.Copy(Program.ServerConfig.DbLocation + "\\PlayersProfs\\" + client.Player.UID + ".bin", Program.ServerConfig.DbLocation + "\\BackUp\\PlayersProfs\\" + client.Player.UID + "date" + GenerateDate() + ".bin", true);
                            if (File.Exists(Program.ServerConfig.DbLocation + "\\PlayersItems\\" + client.Player.UID + ".bin"))
                                File.Copy(Program.ServerConfig.DbLocation + "\\PlayersItems\\" + client.Player.UID + ".bin", Program.ServerConfig.DbLocation + "\\BackUp\\PlayersItems\\" + client.Player.UID + "date" + GenerateDate() + ".bin");


                            if (File.Exists(Program.ServerConfig.DbLocation + "\\Users\\" + client.Player.UID + ".ini"))
                                File.Delete(Program.ServerConfig.DbLocation + "\\Users\\" + client.Player.UID + ".ini");
                            if (File.Exists(Program.ServerConfig.DbLocation + "\\PlayersSpells\\" + client.Player.UID + ".bin"))
                                File.Delete(Program.ServerConfig.DbLocation + "\\PlayersSpells\\" + client.Player.UID + ".bin");
                            if (File.Exists(Program.ServerConfig.DbLocation + "\\PlayersProfs\\" + client.Player.UID + ".bin"))
                                File.Delete(Program.ServerConfig.DbLocation + "\\PlayersProfs\\" + client.Player.UID + ".bin");
                            if (File.Exists(Program.ServerConfig.DbLocation + "\\PlayersItems\\" + client.Player.UID + ".bin"))
                                File.Delete(Program.ServerConfig.DbLocation + "\\PlayersItems\\" + client.Player.UID + ".bin");
                           

                            Role.Instance.House house;
                            if (client.MyHouse != null && Role.Instance.House.HousePoll.ContainsKey(client.Player.UID))
                                Role.Instance.House.HousePoll.TryRemove(client.Player.UID, out house);

                            if (File.Exists(Program.ServerConfig.DbLocation + "\\Houses\\" + client.Player.UID + ".bin"))
                                File.Delete(Program.ServerConfig.DbLocation + "\\Houses\\" + client.Player.UID + ".bin");

                            //Role.Instance.Chi chi;
                            //if (Role.Instance.Chi.ChiPool.ContainsKey(client.Player.UID))
                            //{
                            //    Role.Instance.Chi.ChiPool.TryRemove(client.Player.UID, out chi);
                            //    WindowsAPI.IniFile write = new WindowsAPI.IniFile("\\BackUp\\ChiInfo.txt");
                            //    write.WriteString(client.Player.UID.ToString() + "date" + GenerateDate() + "", "Dragon", chi.Dragon.ToString());
                            //    write.WriteString(client.Player.UID.ToString() + "date" + GenerateDate() + "", "Phoenix", chi.Phoenix.ToString());
                            //    write.WriteString(client.Player.UID.ToString() + "date" + GenerateDate() + "", "Turtle", chi.Turtle.ToString());
                            //    write.WriteString(client.Player.UID.ToString() + "date" + GenerateDate() + "", "Tiger", chi.Tiger.ToString());

                            //}
                            Role.Instance.Flowers flow;
                            if (Role.Instance.Flowers.ClientPoll.ContainsKey(client.Player.UID))
                            {
                                Role.Instance.Flowers.ClientPoll.TryRemove(client.Player.UID, out flow);
                            }
                            Role.Instance.Associate.MyAsociats Associate;
                            if (Role.Instance.Associate.Associates.TryGetValue(client.Player.UID, out Associate))
                            {
                                Role.Instance.Associate.Associates.TryRemove(client.Player.UID, out Associate);
                            }
                            Client.GameClient user;
                            Database.Server.GamePoll.TryRemove(client.Player.UID, out user);

                            if (Server.NameUsed.Contains(user.Player.Name.GetHashCode()))
                            {
                                lock (Server.NameUsed)
                                    Server.NameUsed.Remove(user.Player.Name.GetHashCode());
                            }
                            return;

                        }
                        if ((client.ClientFlag & Client.ServerFlag.RemoveSpouse) == Client.ServerFlag.RemoveSpouse)
                        {
                            DestroySpouse(client);
                            client.ClientFlag &= ~Client.ServerFlag.RemoveSpouse;
                            return;
                        }
                        if ((client.ClientFlag & Client.ServerFlag.UpdateSpouse) == Client.ServerFlag.UpdateSpouse)
                        {
                            UpdateSpouse(client);
                            client.ClientFlag &= ~Client.ServerFlag.UpdateSpouse;
                            return;
                        }
                        if ((client.ClientFlag & Client.ServerFlag.SetLocation) != Client.ServerFlag.SetLocation && (client.ClientFlag & Client.ServerFlag.OnLoggion) == Client.ServerFlag.OnLoggion)
                        {
                            Game.MsgServer.MsgLoginClient.LoginHandler(client, client.OnLogin);
                        }
                        else if ((client.ClientFlag & Client.ServerFlag.QueuesSave) == Client.ServerFlag.QueuesSave)
                        {
                            if (client.Player.OnTransform)
                            {
                                client.Player.HitPoints = Math.Min(client.Player.HitPoints, (int)client.Status.MaxHitpoints);

                            }
                            SaveClient(client);
                        }
                    }
                }
                catch (Exception e) { Console.SaveException(e); }
            }
        }

    }
}
