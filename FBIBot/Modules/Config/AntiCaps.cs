using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Modules.Config
{
    public class AntiCaps : ModuleBase<SocketCommandContext>
    {
        [Command("anti-caps")]
        [Alias("anticaps")]
        [RequireAdmin]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task AntiCapsAsync(string enable)
        {
            bool isEnable = enable == "true" || enable == "enable";
            bool isEnabled = await GetAntiCapsAsync(Context.Guild);

            if (isEnable == isEnabled)
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that anti-caps is already {(isEnabled ? "enabled" : "disabled")}.");
                return;
            }

            List<Task> cmds = new List<Task>()
            {
                Context.Channel.SendMessageAsync($"We are now {(isEnable ? "permitted to remove" : "prohibited from removing")} REALLY LOUD PROTESTS.")
            };
            if (isEnable)
            {
                cmds.Add(SetAntiCapsAsync(Context.Guild));
            }
            else
            {
                cmds.Add(RemoveAntiCapsAsync(Context.Guild));
            }

            await Task.WhenAll(cmds);
        }

        public static async Task<bool> GetAntiCapsAsync(SocketGuild g)
        {
            bool isAntiCaps = false;

            string getSpam = "SELECT guild_id FROM AntiCaps WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getSpam, Program.cnConfig))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                isAntiCaps = await reader.ReadAsync();
                reader.Close();
            }

            return isAntiCaps;
        }

        public static async Task SetAntiCapsAsync(SocketGuild g)
        {
            string insert = "INSERT INTO AntiCaps (guild_id) SELECT @guild_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM AntiCaps WHERE guild_id = @guild_id);";

            using (SqliteCommand cmd = new SqliteCommand(insert, Program.cnConfig))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task RemoveAntiCapsAsync(SocketGuild g)
        {
            string delete = "DELETE FROM AntiCaps WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, Program.cnConfig))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
