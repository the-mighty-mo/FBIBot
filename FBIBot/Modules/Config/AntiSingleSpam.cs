using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Modules.Config
{
    public class AntiSingleSpam : ModuleBase<SocketCommandContext>
    {
        [Command("anti-singlespam")]
        [Alias("antisinglespam")]
        [RequireAdmin]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task AntiSingleSpamAsync(string enable)
        {
            bool isEnable = enable == "true" || enable == "enable";
            bool isEnabled = await GetAntiSingleSpamAsync(Context.Guild);

            if (isEnable == isEnabled)
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that anti-single-spam is already {(isEnabled ? "enabled" : "disabled")}.");
                return;
            }

            List<Task> cmds = new List<Task>()
            {
                Context.Channel.SendMessageAsync($"We are now {(isEnable ? "permitted to remove" : "prohibited from removing")} *big*, spammy, anti-American messages.")
            };
            if (isEnable)
            {
                cmds.Add(SetAntiSingleSpamAsync(Context.Guild));
            }
            else
            {
                cmds.Add(RemoveAntiSingleSpamAsync(Context.Guild));
            }

            await Task.WhenAll(cmds);
        }

        public static async Task<bool> GetAntiSingleSpamAsync(SocketGuild g)
        {
            bool isAntiSingleSpam = false;

            string getSpam = "SELECT guild_id FROM AntiSingleSpam WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getSpam, Program.cnConfig))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                isAntiSingleSpam = await reader.ReadAsync();
                reader.Close();
            }

            return isAntiSingleSpam;
        }

        public static async Task SetAntiSingleSpamAsync(SocketGuild g)
        {
            string insert = "INSERT INTO AntiSingleSpam (guild_id) SELECT @guild_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM AntiSingleSpam WHERE guild_id = @guild_id);";

            using (SqliteCommand cmd = new SqliteCommand(insert, Program.cnConfig))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task RemoveAntiSingleSpamAsync(SocketGuild g)
        {
            string delete = "DELETE FROM AntiSingleSpam WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, Program.cnConfig))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
