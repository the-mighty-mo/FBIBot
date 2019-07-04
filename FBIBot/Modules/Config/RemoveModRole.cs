using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Modules.Config
{
    public class RemoveModRole : ModuleBase<SocketCommandContext>
    {
        [Command("remove-modrole")]
        [RequireAdmin]
        public async Task RemoveModRoleAsync(SocketRole role)
        {
            if (!(await AddModRole.GetModRolesAsync(Context.Guild)).Contains(role))
            {
                await Context.Channel.SendMessageAsync($"Our agents have informed us that members with the {role.Name} role aren't assistants.");
                return;
            }

            await RemoveModAsync(role);
            await Context.Channel.SendMessageAsync($"Members with the {role.Name} role are no longer assistants of the agency. They were getting kind of suspicious, anyways.");
        }

        [Command("remove-modrole")]
        [RequireAdmin]
        public async Task RemoveModRoleAsync(string role)
        {
            SocketRole r;
            if (ulong.TryParse(role, out ulong roleID) && (r = Context.Guild.GetRole(roleID)) != null)
            {
                await RemoveModRoleAsync(r);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given role does not exist.");
        }

        public static async Task RemoveModAsync(SocketRole role)
        {
            string delete = "DELETE FROM Mods WHERE guild_id = @guild_id AND role_id = @role_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, Program.cnModRoles))
            {
                cmd.Parameters.AddWithValue("@guild_id", role.Guild.Id.ToString());
                cmd.Parameters.AddWithValue("@role_id", role.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
