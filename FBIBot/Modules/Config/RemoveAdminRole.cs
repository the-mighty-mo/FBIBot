using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Modules.Config
{
    public class RemoveAdminRole : ModuleBase<SocketCommandContext>
    {
        [Command("removeadmin")]
        [Alias("remove-admin", "remove-adminrole")]
        [RequireAdmin]
        public async Task RemoveAdminRoleAsync(SocketRole role)
        {
            if (!(await AddAdmin.GetAdminRolesAsync(Context.Guild)).Contains(role))
            {
                await Context.Channel.SendMessageAsync($"Our agents have informed us that members with the {role.Name} role aren't local directors.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"Members with the {role.Name} role are no longer local directors of the agency. The president was displeased with their performance.");

            await Task.WhenAll
            (
                RemoveAdminAsync(role),
                Context.Channel.SendMessageAsync("", false, embed.Build())
            );
        }

        [Command("removeadmin")]
        [Alias("remove-admin", "remove-adminrole")]
        [RequireAdmin]
        public async Task RemoveAdminRoleAsync(string role)
        {
            SocketRole r;
            if (ulong.TryParse(role, out ulong roleID) && (r = Context.Guild.GetRole(roleID)) != null)
            {
                await RemoveAdminRoleAsync(r);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given role does not exist.");
        }

        public static async Task RemoveAdminAsync(SocketRole role)
        {
            string delete = "DELETE FROM Admins WHERE guild_id = @guild_id AND role_id = @role_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, Program.cnModRoles))
            {
                cmd.Parameters.AddWithValue("@guild_id", role.Guild.Id.ToString());
                cmd.Parameters.AddWithValue("@role_id", role.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
