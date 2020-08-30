using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Modules.Config
{
    public class AutoSurveillance : ModuleBase<SocketCommandContext>
    {
        [Command("auto-surveillance")]
        [Alias("autosurveillance")]
        [RequireAdmin]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task AutoSurveillanceAsync(string enable)
        {
            bool isEnable = enable == "true" || enable == "enable";
            bool isEnabled = await GetAutoSurveillanceAsync(Context.Guild);

            if (isEnable == isEnabled)
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that Auto Surveillance is already {(isEnabled ? "enabled" : "disabled")}.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"We are now {(isEnable ? "permitted to perform" : "prohibited from performing")} surveillance on server members.");

            List<Task> cmds = new List<Task>()
            {
                Context.Channel.SendMessageAsync("", false, embed.Build())
            };
            if (isEnable)
            {
                cmds.Add(SetAutoSurveillanceAsync(Context.Guild));
            }
            else
            {
                cmds.Add(RemoveAutoSurveillanceAsync(Context.Guild));
            }

            await Task.WhenAll(cmds);
        }

        public static async Task<bool> GetAutoSurveillanceAsync(SocketGuild g)
        {
            bool isAutoSurveillance = false;

            string getSurveillance = "SELECT guild_id FROM AutoSurveillance WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getSurveillance, Program.cnConfig))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                isAutoSurveillance = await reader.ReadAsync();
                reader.Close();
            }

            return isAutoSurveillance;
        }

        public static async Task SetAutoSurveillanceAsync(SocketGuild g)
        {
            string insert = "INSERT INTO AutoSurveillance (guild_id) SELECT @guild_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM AutoSurveillance WHERE guild_id = @guild_id);";

            using (SqliteCommand cmd = new SqliteCommand(insert, Program.cnConfig))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task RemoveAutoSurveillanceAsync(SocketGuild g)
        {
            string delete = "DELETE FROM AutoSurveillance WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, Program.cnConfig))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
