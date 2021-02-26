using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Config
{
    public class RemoveMod : ModuleBase<SocketCommandContext>
    {
        [Command("removemod")]
        [Alias("remove-mod", "remove-modrole")]
        [RequireAdmin]
        public async Task RemoveModRoleAsync(SocketRole role)
        {
            if (!(await modRolesDatabase.Mods.GetModRolesAsync(Context.Guild)).Contains(role))
            {
                await Context.Channel.SendMessageAsync($"Our agents have informed us that members with the {role.Mention} role aren't assistants.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"Members with the {role.Mention} role are no longer assistants of the agency. They were getting kind of suspicious, anyways.");

            await Task.WhenAll
            (
                modRolesDatabase.Mods.RemoveModAsync(role),
                Context.Channel.SendMessageAsync(embed: embed.Build())
            );
        }

        [Command("removemod")]
        [Alias("remove-mod", "remove-modrole")]
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
    }
}