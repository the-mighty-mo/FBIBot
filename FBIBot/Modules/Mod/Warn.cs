﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FBIBot.Modules.Mod.ModLog;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class Warn : ModuleBase<SocketCommandContext>
    {
        [Command("warn")]
        [RequireMod]
        [RequireModLog]
        public async Task WarnAsync([RequireInvokerHierarchy("warn")] SocketGuildUser user, [Remainder] string reason = null) => await TempWarnAsync(user, null, reason);

        [Command("warn")]
        [RequireMod]
        [RequireModLog]
        public async Task WarnAsync(string user, [Remainder] string reason = null) => await TempWarnAsync(user, null, reason);

        [Command("tempwarn")]
        [Alias("temp-warn")]
        [RequireMod]
        [RequireModLog]
        public async Task TempWarnAsync([RequireInvokerHierarchy("warn")] SocketGuildUser user, string length, [Remainder] string reason = null)
        {
            ulong id = await ModLogBase.GetNextModLogID(Context.Guild);

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(255, 213, 31))
                .WithDescription($"{user.Mention} stop protesting capitalism.");

            if (double.TryParse(length, out double h))
            {
                EmbedFieldBuilder lengthField = new EmbedFieldBuilder()
                        .WithIsInline(false)
                        .WithName("Length")
                        .WithValue(h < 1 ? $"{h * 60} minutes" : $"{h} hours");
                embed.AddField(lengthField);
            }

            EmbedFieldBuilder reasonField = new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName("Reason")
                    .WithValue($"{reason ?? "[none given]"}");
            embed.AddField(reasonField);

            await Task.WhenAll
            (
                Context.Channel.SendMessageAsync("", false, embed.Build()),
                WarnModLog.SendToModLogAsync(Context.User as SocketGuildUser, user, length, reason),
                AddWarningAsync(user, id)
            );

            if (double.TryParse(length, out double hours))
            {
                await Task.Delay((int)(hours * 60 * 60 * 1000));

                if (!await RemoveWarning.GetWarningAsync(user, id))
                {
                    return;
                }

                await Task.WhenAll
                (
                    RemoveWarningModLog.SendToModLogAsync(Context.Guild.CurrentUser, user, id.ToString()),
                    RemoveWarning.RemoveWarningAsync(user, id)
                );
            }
        }

        [Command("tempwarn")]
        [Alias("temp-warn")]
        [RequireMod]
        [RequireModLog]
        public async Task TempWarnAsync(string user, string length, [Remainder] string reason = null)
        {
            SocketGuildUser u;
            if (ulong.TryParse(user, out ulong userID) && (u = Context.Guild.GetUser(userID)) != null)
            {
                await TempWarnAsync(u, length, reason);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given user does not exist.");
        }

        public static async Task AddWarningAsync(SocketGuildUser u, ulong id)
        {
            string addWarning = "INSERT INTO Warnings (guild_id, id, user_id) SELECT @guild_id, @id, @user_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM Warnings WHERE guild_id = @guild_id AND id = @id AND user_id = @user_id);";
            using (SqliteCommand cmd = new SqliteCommand(addWarning, Program.cnModLogs))
            {
                cmd.Parameters.AddWithValue("@guild_id", u.Guild.Id.ToString());
                cmd.Parameters.AddWithValue("@id", id.ToString());
                cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
