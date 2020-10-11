using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Config
{
    public class AddMod : ModuleBase<SocketCommandContext>
    {
        [Command("addmod")]
        [Alias("add-mod", "add-modrole")]
        [RequireAdmin]
        public async Task AddModAsync(SocketRole role)
        {
            if ((await modRolesDatabase.Mods.GetModRolesAsync(Context.Guild)).Contains(role))
            {
                await Context.Channel.SendMessageAsync($"Members with the {role.Name} role are already assisting the FBI.");
                return;
            }

            List<Task> cmds = new List<Task>()
            {
                modRolesDatabase.Mods.AddModRoleAsync(role)
            };
            string description;
            if ((await modRolesDatabase.Admins.GetAdminRolesAsync(Context.Guild)).Contains(role))
            {
                description = $"Members with the {role.Name} role have been demoted to assistants of the agency.";
                cmds.Add(modRolesDatabase.Admins.RemoveAdminAsync(role));
            }
            else
            {
                description = $"Members with the {role.Name} role may now assist our agents in ensuring freedom, democracy, and justice for all.";
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription(description);
            cmds.Add(Context.Channel.SendMessageAsync("", false, embed.Build()));

            await Task.WhenAll(cmds);
        }

        [Command("addmod")]
        [Alias("add-mod", "add-modrole")]
        [RequireAdmin]
        public async Task AddModAsync(string role)
        {
            SocketRole r;
            if (ulong.TryParse(role, out ulong roleID) && (r = Context.Guild.GetRole(roleID)) != null)
            {
                await AddModAsync(r);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given role does not exist.");
        }
    }
}
