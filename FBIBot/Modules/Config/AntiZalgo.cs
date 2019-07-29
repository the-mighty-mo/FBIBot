using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Modules.Config
{
    public class AntiZalgo : ModuleBase<SocketCommandContext>
    {
        [Command("anti-zalgo")]
        [Alias("antizalgo")]
        [RequireAdmin]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task AntiZalgoAsync(string enable)
        {
            bool isEnable = enable == "true" || enable == "enable";
            bool isEnabled = await GetAntiZalgoAsync(Context.Guild);

            if (isEnable == isEnabled)
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that anti-zalgo is already {(isEnabled ? "enabled" : "disabled")}.");
                return;
            }

            List<Task> cmds = new List<Task>()
            {
                Context.Channel.SendMessageAsync($"We are now {(isEnable ? "permitted to remove" : "prohibited from removing")} messages leaked from Area 51.")
            };
            if (isEnable)
            {
                cmds.Add(SetAntiZalgoAsync(Context.Guild));
            }
            else
            {
                cmds.Add(RemoveAntiZalgoAsync(Context.Guild));
            }

            await Task.WhenAll(cmds);
        }

        public static async Task<bool> GetAntiZalgoAsync(SocketGuild g)
        {
            bool isAntiZalgo = false;

            string getZalgo = "SELECT guild_id FROM AntiZalgo WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getZalgo, Program.cnConfig))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                isAntiZalgo = await reader.ReadAsync();
                reader.Close();
            }

            return isAntiZalgo;
        }

        public static async Task SetAntiZalgoAsync(SocketGuild g)
        {
            string insert = "INSERT INTO AntiZalgo (guild_id) SELECT @guild_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM AntiZalgo WHERE guild_id = @guild_id);";

            using (SqliteCommand cmd = new SqliteCommand(insert, Program.cnConfig))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task RemoveAntiZalgoAsync(SocketGuild g)
        {
            string delete = "DELETE FROM AntiZalgo WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, Program.cnConfig))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
