using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FBIBot.Modules.Mod.ModLog;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class GetWarnings : ModuleBase<SocketCommandContext>
    {
        [Command("getwarnings")]
        [Alias("get-warnings")]
        [RequireMod]
        public async Task GetWarnsAsync(SocketGuildUser user)
        {
            List<ulong> ids = await GetWarningsAsync(user);
            if (ids.Count == 0)
            {
                await Context.Channel.SendMessageAsync("Our security team has informed us that the given user does not have any warnings.");
                return;
            }

            List<ulong> idsPastDay = new List<ulong>();
            List<ulong> idsPastHour = new List<ulong>();
            foreach (ulong id in ids)
            {
                IUserMessage msg = await ModLogManager.GetModLogAsync(Context.Guild, id);
                TimeSpan timeSinceLog = Context.Message.Timestamp - msg.Timestamp;
                if (timeSinceLog <= TimeSpan.FromDays(1))
                {
                    idsPastDay.Add(id);
                    if (timeSinceLog <= TimeSpan.FromHours(1))
                    {
                        idsPastHour.Add(id);
                    }
                }
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(255, 213, 31))
                .WithTitle($"Federal Bureau of Investigation - Warnings for {user.Nickname ?? user.Username}")
                .WithCurrentTimestamp();

            EmbedFieldBuilder warnings = new EmbedFieldBuilder()
                    .WithIsInline(true)
                    .WithName($"Total Warnings: {ids.Count}")
                    .WithValue($"- Past 24 hours: {idsPastDay.Count}\n" +
                               $"- Past hour: {idsPastHour.Count}");
            embed.AddField(warnings);

            EmbedFieldBuilder modLogs = new EmbedFieldBuilder()
                    .WithIsInline(true)
                    .WithName($"See mod logs: {string.Join(", ", ids)}")
                    .WithValue($"- Past 24 hours: {string.Join(", ", idsPastDay)}\n" +
                               $"- Past hour: {string.Join(", ", idsPastHour)}");
            embed.AddField(modLogs);

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("getwarnings")]
        [Alias("get-warnings")]
        [RequireMod]
        public async Task GetWarnsAsync(string user)
        {
            SocketGuildUser u;
            if (ulong.TryParse(user, out ulong userID) && (u = Context.Guild.GetUser(userID)) != null)
            {
                await GetWarnsAsync(u);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given user does not exist.");
        }

        public static async Task<List<ulong>> GetWarningsAsync(SocketGuildUser u)
        {
            List<ulong> ids = new List<ulong>();

            string getWarns = "SELECT id FROM Warnings WHERE guild_id = @guild_id AND user_id = @user_id;";
            using (SqliteCommand cmd = new SqliteCommand(getWarns, Program.cnModLogs))
            {
                cmd.Parameters.AddWithValue("@guild_id", u.Guild.Id.ToString());
                cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());

                SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    if (ulong.TryParse(reader["id"].ToString(), out ulong id))
                    {
                        ids.Add(id);
                    }
                }
                reader.Close();
            }

            return ids;
        }
    }
}
