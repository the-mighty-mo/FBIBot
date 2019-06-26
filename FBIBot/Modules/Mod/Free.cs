using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class Free : ModuleBase<SocketCommandContext>
    {
        [Command("arrest")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireOwner()]
        public async Task FreeAsync(SocketGuildUser user)
        {
            SocketRole role = await Arrest.GetPrisonerRoleAsync(Context.Guild);
            List<SocketRole> roles = await Unmute.GetUserRolesAsync(user);
            if ((role == null || !user.Roles.Contains(role)) && roles.Count == 0)
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that {user.Nickname ?? user.Username} is not held captive.");
                return;
            }

            if (roles.Count > 0)
            {
                await user.AddRolesAsync(roles);
                await Unmute.RemoveUserRolesAsync(user);
            }
            if (role != null)
            {
                await user.RemoveRoleAsync(role);
            }

            await Context.Channel.SendMessageAsync($"{user.Mention} has been freed from Guantanamo Bay after a good amount of ~~torture~~ re-education.");
        }

        [Command("arrest")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireOwner()]
        public async Task FreeAsync(string user)
        {
            SocketGuildUser u;
            if (ulong.TryParse(user, out ulong userID) && (u = Context.Guild.GetUser(userID)) != null)
            {
                await FreeAsync(u);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given user does not exist.");
        }
    }
}
