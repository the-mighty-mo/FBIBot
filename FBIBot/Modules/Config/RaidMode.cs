using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FBIBot.Modules.Config
{
    public class RaidMode : ModuleBase<SocketCommandContext>
    {
        [Command("raidmode")]
        [RequireBotPermission(GuildPermission.ManageGuild)]
        public async Task RaidModeAsync(string enabled = "false")
        {
            SocketGuildUser u = Context.Guild.GetUser(Context.User.Id);
            if (!await VerifyUser.IsAdmin(u))
            {
                await Context.Channel.SendMessageAsync("You are not a local director of the FBI and cannot use this command.");
                return;
            }

            bool isEnabled = enabled.ToLower() == "true" || enabled.ToLower() == "enable";
            VerificationLevel? level = await GetVerificationLevelAsync(Context.Guild);

            if (isEnabled)
            {
                if (level != null)
                {
                    await Context.Channel.SendMessageAsync("THE FBI IS ALREADY IN RAID MODE");
                    return;
                }
                await SaveVerificationLevelAsync(Context.Guild);
                await Context.Guild.ModifyAsync(x => x.VerificationLevel = VerificationLevel.High);

                await Context.Channel.SendMessageAsync(":rotating_light: :rotating_light: WE HERE AT THE FBI ARE IN RAID MODE! :rotating_light: :rotating_light:");
                return;
            }

            if (level == null)
            {
                await Context.Channel.SendMessageAsync("The FBI is already out of raid mode.");
                return;
            }

            await Context.Guild.ModifyAsync(x => x.VerificationLevel = (VerificationLevel)level);
            await Context.Channel.SendMessageAsync("The FBI is now out of raid mode. Surveillance results will be posted in the Mod Logs.");
            await RemoveVerificationLevelAsync(Context.Guild);
        }

        public static async Task<VerificationLevel?> GetVerificationLevelAsync(SocketGuild g)
        {
            VerificationLevel? level = null;

            string getLevel = "SELECT level FROM RaidMode WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getLevel, Program.cnRaidMode))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    if (int.TryParse(reader["level"].ToString(), out int levelInt))
                    {
                        level = (VerificationLevel)levelInt;
                    }
                }
            }

            return await Task.Run(() => level);
        }

        public static async Task SaveVerificationLevelAsync(SocketGuild g)
        {
            string update = "UPDATE RaidMode SET level = @level WHERE guild_id = @guild_id;";
            string insert = "INSERT INTO RaidMode (guild_id, level) SELECT @guild_id, @level WHERE (Select Changes() = 0);";

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
    }
}
