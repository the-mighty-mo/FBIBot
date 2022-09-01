using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Config
{
    [Group("remove-role", "Removes a role from the bot")]
    [RequireAdmin]
    public class RemoveRole : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("admin", "Removes the role from the list of local directors of the bureau due to presidential disapproval")]
        public async Task RemoveAdminRoleAsync(SocketRole role)
        {
            if (!(await modRolesDatabase.Admins.GetAdminRolesAsync(Context.Guild)).Contains(role))
            {
                await Context.Interaction.RespondAsync($"Our agents have informed us that members with the {role.Mention} role aren't local directors.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"Members with the {role.Mention} role are no longer local directors of the agency. The president was displeased with their performance.");

            await Task.WhenAll
            (
                modRolesDatabase.Admins.RemoveAdminAsync(role),
                Context.Interaction.RespondAsync(embed: embed.Build())
            );
        }

        [SlashCommand("mod", "Removes the role from the list of assistants of the bureau out of suspicion")]
        public async Task RemoveModRoleAsync(SocketRole role)
        {
            if (!(await modRolesDatabase.Mods.GetModRolesAsync(Context.Guild)).Contains(role))
            {
                await Context.Interaction.RespondAsync($"Our agents have informed us that members with the {role.Mention} role aren't assistants.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"Members with the {role.Mention} role are no longer assistants of the agency. They were getting kind of suspicious, anyways.");

            await Task.WhenAll
            (
                modRolesDatabase.Mods.RemoveModAsync(role),
                Context.Interaction.RespondAsync(embed: embed.Build())
            );
        }
    }
}