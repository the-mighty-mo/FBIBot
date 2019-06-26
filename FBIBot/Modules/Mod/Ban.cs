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
    public class Ban : ModuleBase<SocketCommandContext>
    {
        [Command("ban")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireOwner()]
        public async Task BanAsync(SocketGuildUser user, string reason = "", string prune = "0")
        {
            if (int.TryParse(prune, out int pruneDays))
            {
                await user.BanAsync(pruneDays, reason);
            }
            else
            {
                await user.BanAsync(0, reason);
            }

            await Context.Channel.SendMessageAsync($"The communist spy {user.Mention} has been given the ~~ban~~ freedom hammer.");
        }

        [Command("ban")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireOwner()]
        public async Task BanAsync(string user, string reason = "", string prune = "0")
        {
            SocketGuildUser u;
            if (ulong.TryParse(user, out ulong userID) && (u = Context.Guild.GetUser(userID)) != null)
            {
                await BanAsync(u, reason, prune);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given user does not exist.");
        }
    }
}
