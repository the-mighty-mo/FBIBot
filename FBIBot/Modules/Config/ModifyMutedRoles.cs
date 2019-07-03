using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Modules.Config
{
    public class ModifyMutedRoles : ModuleBase<SocketCommandContext>
    {
        [Command("modify-muted-roles")]
        public async Task ModifyMutedRolesAsync(string modify)
        {
            SocketGuildUser u = Context.Guild.GetUser(Context.User.Id);
            if (!await VerifyUser.IsAdmin(u))
            {
                await Context.Channel.SendMessageAsync("You are not a local director of the FBI and cannot use this command.");
                return;
            }

            bool isModify = modify.ToLower() == "true" || modify.ToLower() == "enable";
            string state = isModify ? "permitted to modify" : "prohibited from modifying";

            if (isModify && !await GetModifyMutedAsync(Context.Guild))
            {
                await AddModifyMutedAsync(Context.Guild);
            }
            else if (!isModify && await GetModifyMutedAsync(Context.Guild))
            {
                await RemoveModifyMutedAsync(Context.Guild);
            }
            else
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that we are already {state} muted member's roles.");
                return;
            }

            await Context.Channel.SendMessageAsync($"We are now {state} muted member's roles.");
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

                SqliteDataReader reader = cmd.ExecuteReader();
                modify = reader.Read();
                reader.Close();
            }

            return await Task.Run(() => modify);
        }
    }
}
