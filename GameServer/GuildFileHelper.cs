using System;
using System.IO;
using System.Text;

public static class GuildFileHelper
{
    private static readonly string GuildsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "guilds");

    public static void SaveGuildIni(int guildId, string guildName, uint leaderUid, string leaderName)
    {
        try
        {
            if (!Directory.Exists(GuildsFolder))
                Directory.CreateDirectory(GuildsFolder);

            string filePath = Path.Combine(GuildsFolder, guildId + ".ini");

            var sb = new StringBuilder();
            sb.AppendLine("[Guild]");
            sb.AppendLine("GuildID=" + guildId);
            sb.AppendLine("Name=" + guildName);
            sb.AppendLine("LeaderUID=" + leaderUid);
            sb.AppendLine("LeaderName=" + leaderName);
            sb.AppendLine("CreatedAt=" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[GuildFileHelper] Erro ao salvar guild ini: " + ex.Message);
        }
    }
}
