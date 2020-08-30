using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Modules.Config
{
    public class ModifyMutedRoles : ModuleBase<SocketCommandContext>
    {
        [Command("modify-muted-roles")]
        [RequireAdmin]
        public async Task ModifyMutedRolesAsync(string modify)
        {
            bool isModify = modify.ToLower() == "true" || modify.ToLower() == "enable";
            bool isModifying = await GetModifyMutedAsync(Context.Guild);
            string state = isModify ? "permitted to modify" : "prohibited from modifying";

            if (isModify == isModifying)
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that we are already {state} muted member's roles.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"We are now {state} muted member's roles.");

            List<Task> cmds = new List<Task>()
            {
                Context.Channel.SendMessageAsync("", false, embed.Build())
            };
            if (isModify)
            {
                cmds.Add(AddModifyMutedAsync(Context.Guild));
            }
            else
            {
                cmds.Add(RemoveModifyMutedAsync(Context.Guild));
            }

            await Task.WhenAll(cmds);
        }

        public static async Task AddModifyMutedAsync(SocketGuild g)
        {
            string add = "INSERT INTO ModifyMuted (guild_id) SELECT @guild_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM ModifyMuted WHERE guild_id = @guild_id);";
            using (SqliteCommand cmd = new SqliteCommand(add, Program.cnConfig))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task RemoveModifyMutedAsync(SocketGuild g)
        {
            string add = "DELETE FROM ModifyMuted WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(add, Program.cnConfig))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task<bool> GetModifyMutedAsync(SocketGuild g)
        {
            bool modify = false;

            string add = "SELECT * FROM ModifyMuted WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(add, Program.cnConfig))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                modify = await reader.ReadAsync();
                reader.Close();
            }

            return modify;
        }
    }
}
