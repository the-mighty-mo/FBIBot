﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Modules.Config
{
    public class RaidMode : ModuleBase<SocketCommandContext>
    {
        [Command("raidmode")]
        [Alias("raid-mode")]
        [RequireAdmin]
        [RequireBotPermission(GuildPermission.ManageGuild)]
        public async Task RaidModeAsync()
        {
            VerificationLevel? level = await GetVerificationLevelAsync(Context.Guild);
            bool isDisabled = level == null;

            if (isDisabled)
            {
                await SaveVerificationLevelAsync(Context.Guild);

                EmbedBuilder embed2 = new EmbedBuilder()
                    .WithColor(new Color(206, 15, 65))
                    .WithDescription(":rotating_light: :rotating_light: WE HERE AT THE FBI ARE IN RAID MODE! :rotating_light: :rotating_light:");

                await Task.WhenAll
                (
                    Context.Guild.ModifyAsync(x => x.VerificationLevel = VerificationLevel.High),
                    Context.Channel.SendMessageAsync("", false, embed2.Build()),
                    Mod.SendToModLog.SendToModLogAsync(Mod.SendToModLog.LogType.RaidMode, Context.User as SocketGuildUser, null, "Enabled")
                );
                return;
            }

            Task[] cmds =
            {
                Context.Guild.ModifyAsync(x => x.VerificationLevel = (VerificationLevel)level!),
                RemoveVerificationLevelAsync(Context.Guild)
            };

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(12, 156, 24))
                .WithDescription("The FBI is now out of raid mode. Surveillance results will be posted in the Mod Logs.");

            await Task.WhenAll
            (
                Context.Channel.SendMessageAsync("", false, embed.Build()),
                Mod.SendToModLog.SendToModLogAsync(Mod.SendToModLog.LogType.RaidMode, Context.User as SocketGuildUser, null, "Disabled")
            );
            await Task.WhenAll
            (
                cmds[0],
                cmds[1],
                SendUsersAsync()
            );
        }

        async Task SendUsersAsync()
        {
            List<string> blockedUsers = await GetBlockedUsersAsync(Context.Guild);
            SocketTextChannel channel = await SetModLog.GetModLogChannelAsync(Context.Guild);
            List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();
            int messages = 1;

            if (blockedUsers.Count == 0)
            {
                EmbedBuilder embed2 = new EmbedBuilder()
                    .WithColor(new Color(12, 156, 24))
                    .WithTitle($"FBI Raid Mode Surveillance Results{(messages > 1 ? $" ({messages})" : "")}")
                    .WithCurrentTimestamp();

                EmbedFieldBuilder field = new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName("No users blocked")
                    .WithValue("No users attempted to join the server during Raid Mode.");
                embed2.AddField(field);

                await channel.SendMessageAsync("", false, embed2.Build());
            }
            else
            {
                int i = 1;
                int j = 1;
                string blocked = "";
                foreach (string userID in blockedUsers)
                {
                    if (j > 3)
                    {
                        EmbedBuilder _embed = new EmbedBuilder()
                            .WithColor(new Color(206, 15, 65))
                            .WithTitle($"FBI Raid Mode Surveillance Results ({messages})")
                            .WithCurrentTimestamp()
                            .WithFields(fields);
                        await channel.SendMessageAsync("", false, _embed.Build());
                        j = 1;
                        messages++;
                        fields.Clear();
                    }

                    blocked += $"{(i > 1 ? "\n" : "")}<@{userID}> (ID: {userID})";

                    if (++i > 20)
                    {
                        EmbedFieldBuilder field = new EmbedFieldBuilder()
                            .WithIsInline(false)
                            .WithName($"Blocked Users ({3 * (messages - 1) + j})")
                            .WithValue(blocked);
                        fields.Add(field);
                        i = 1;
                        j++;
                        blocked = "";
                    }
                }
                if (i <= 20 && i > 1)
                {
                    EmbedFieldBuilder field = new EmbedFieldBuilder()
                        .WithIsInline(false)
                        .WithName($"Blocked Users{(j > 1 || messages > 1 ? $" ({3 * (messages - 1) + j})" : "")}")
                        .WithValue(blocked);
                    fields.Add(field);
                }

                EmbedBuilder embed = new EmbedBuilder()
                    .WithColor(new Color(206, 15, 65))
                    .WithTitle($"FBI Raid Mode Surveillance Results{(messages > 1 ? $" ({messages})" : "")}")
                    .WithCurrentTimestamp()
                    .WithFields(fields);

                await channel.SendMessageAsync("", false, embed.Build());
            }

            await RemoveBlockedUsersAsync(Context.Guild);
        }

        public static async Task<VerificationLevel?> GetVerificationLevelAsync(SocketGuild g)
        {
            VerificationLevel? level = null;

            string getLevel = "SELECT level FROM RaidMode WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getLevel, Program.cnRaidMode))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    if (int.TryParse(reader["level"].ToString(), out int levelInt))
                    {
                        level = (VerificationLevel)levelInt;
                    }
                }
                reader.Close();
            }

            return level;
        }

        public static async Task SaveVerificationLevelAsync(SocketGuild g)
        {
            string update = "UPDATE RaidMode SET level = @level WHERE guild_id = @guild_id;";
            string insert = "INSERT INTO RaidMode (guild_id, level) SELECT @guild_id, @level WHERE (SELECT Changes() = 0);";

            using (SqliteCommand cmd = new SqliteCommand(update + insert, Program.cnRaidMode))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                cmd.Parameters.AddWithValue("@level", ((int)g.VerificationLevel).ToString());

                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task RemoveVerificationLevelAsync(SocketGuild g)
        {
            string delete = "DELETE FROM RaidMode WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, Program.cnRaidMode))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task AddBlockedUserAsync(SocketGuildUser u)
        {
            string insert = "INSERT INTO UsersBlocked (guild_id, user_id) SELECT @guild_id, @user_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM UsersBlocked WHERE guild_id = @guild_id AND user_id = @user_id);";

            using (SqliteCommand cmd = new SqliteCommand(insert, Program.cnRaidMode))
            {
                cmd.Parameters.AddWithValue("@guild_id", u.Guild.Id.ToString());
                cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());

                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task<List<string>> GetBlockedUsersAsync(SocketGuild g)
        {
            List<string> blockedUsers = new List<string>();

            string getBlocked = "SELECT user_id FROM UsersBlocked WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getBlocked, Program.cnRaidMode))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    blockedUsers.Add((string)reader["user_id"]);
                }
                reader.Close();
            }

            return blockedUsers;
        }

        public static async Task RemoveBlockedUsersAsync(SocketGuild g)
        {
            string delete = "DELETE FROM UsersBlocked WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, Program.cnRaidMode))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
