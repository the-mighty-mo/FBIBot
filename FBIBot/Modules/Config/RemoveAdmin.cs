using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Config
{
    public class RemoveAdmin : ModuleBase<SocketCommandContext>
    {
        [Command("removeadmin")]
        [Alias("remove-admin", "remove-adminrole")]
        [RequireAdmin]
        public async Task RemoveAdminRoleAsync(SocketRole role)
        {
            if (!(await modRolesDatabase.Admins.GetAdminRolesAsync(Context.Guild)).Contains(role))
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
                modRolesDatabase.Admins.RemoveAdminAsync(role),
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
    }
}
