using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Config
{
    public class AddAdmin : ModuleBase<SocketCommandContext>
    {
        [Command("addadmin")]
        [Alias("add-admin", "add-adminrole")]
        [RequireAdmin]
        public async Task AddAdminAsync(SocketRole role)
        {
            if ((await modRolesDatabase.Admins.GetAdminRolesAsync(Context.Guild)).Contains(role))
            {
                await Context.Channel.SendMessageAsync($"Members with the {role.Mention} role are already local directors of the FBI.");
                return;
            }

            List<Task> cmds = new List<Task>()
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
            cmds.Add(Context.Channel.SendMessageAsync(embed: embed.Build()));

            await Task.WhenAll(cmds);
        }

        [Command("addadmin")]
        [Alias("add-admin", "add-adminrole")]
        [RequireAdmin]
        public async Task AddAdminAsync(string role)
        {
            SocketRole r;
            if (ulong.TryParse(role, out ulong roleID) && (r = Context.Guild.GetRole(roleID)) != null)
            {
                await AddAdminAsync(r);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given role does not exist.");
        }
    }
}