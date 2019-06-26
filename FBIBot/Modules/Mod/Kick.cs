using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class Kick : ModuleBase<SocketCommandContext>
    {
        [Command("kick")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireOwner()]
        public async Task KickAsync(SocketGuildUser user, string reason = null)
        {
            await user.KickAsync(reason);
            await Context.Channel.SendMessageAsync($"The communist spy {user.Mention} has been given the ~~ban~~ freedom hammer.");
        }

        [Command("kick")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireOwner()]
        public async Task KickAsync(string user, string reason = null)
        {
            SocketGuildUser u;
            if (ulong.TryParse(user, out ulong userID) && (u = Context.Guild.GetUser(userID)) != null)
            {
                await KickAsync(u, reason);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given user does not exist.");
        }
    }
}
