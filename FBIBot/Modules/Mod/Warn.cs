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
    public class Warn : ModuleBase<SocketCommandContext>
    {
        [Command("warn")]
        public async Task WarnAsync(SocketGuildUser user, [Remainder] string reason = null)
        {
            await Context.Channel.SendMessageAsync($"{user.Mention} stop protesting capitalism.");
            await SendToModLog.SendToModLogAsync(SendToModLog.LogType.Warn, Context.User, user, null, reason);
        }

        [Command("warn")]
        public async Task WarnAsync(string user, [Remainder] string reason = null)
        {
            SocketGuildUser u;
            if (ulong.TryParse(user, out ulong userID) && (u = Context.Guild.GetUser(userID)) != null)
            {
                await WarnAsync(u, reason);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given user does not exist.");
        }
    }
}
