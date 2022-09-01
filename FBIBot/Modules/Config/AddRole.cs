using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Config
{
    [Group("add-role", "Adds a role to the bot")]
    public class AddRole : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("admin", "Adds the role to a list of local directors of the bureau")]
        [RequireAdmin]
        public async Task AddAdminAsync(SocketRole role)
        {
            if ((await modRolesDatabase.Admins.GetAdminRolesAsync(Context.Guild)).Contains(role))
            {
                await Context.Interaction.RespondAsync($"Members with the {role.Mention} role are already local directors of the FBI.");
                return;
            }

            List<Task> cmds = new()
            {
                modRolesDatabase.Admins.AddAdminRoleAsync(role)
            };
            string description;
            if ((await modRolesDatabase.Mods.GetModRolesAsync(Context.Guild)).Contains(role))
            {
                description = $"Members with the {role.Mention} role have been promoted to local directors of the FBI.";
                cmds.Add(modRolesDatabase.Mods.RemoveModAsync(role));
            }
            else
            {
                description = $"Members with the {role.Mention} role are now local directors of the FBI.";
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription(description);
            cmds.Add(Context.Interaction.RespondAsync(embed: embed.Build()));

            await Task.WhenAll(cmds);
        }

        [SlashCommand("mod", "Adds the role to a list of assistants of the bureau")]
        [RequireAdmin]
        public async Task AddModAsync(SocketRole role)
        {
            if ((await modRolesDatabase.Mods.GetModRolesAsync(Context.Guild)).Contains(role))
            {
                await Context.Interaction.RespondAsync($"Members with the {role.Mention} role are already assisting the FBI.");
                return;
            }

            List<Task> cmds = new()
            {
                modRolesDatabase.Mods.AddModRoleAsync(role)
            };
            string description;
            if ((await modRolesDatabase.Admins.GetAdminRolesAsync(Context.Guild)).Contains(role))
            {
                description = $"Members with the {role.Mention} role have been demoted to assistants of the agency.";
                cmds.Add(modRolesDatabase.Admins.RemoveAdminAsync(role));
            }
            else
            {
                description = $"Members with the {role.Mention} role may now assist our agents in ensuring freedom, democracy, and justice for all.";
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription(description);
            cmds.Add(Context.Interaction.RespondAsync(embed: embed.Build()));

            await Task.WhenAll(cmds);
        }
    }
}