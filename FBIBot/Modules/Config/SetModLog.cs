using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FBIBot.Modules.Config
{
    public class SetModLog : ModuleBase<SocketCommandContext>
    {
        [Command("setmodlog")]
        public async Task SetModLogAsync(SocketTextChannel channel)
        {
            SocketGuildUser u = Context.Guild.GetUser(Context.User.Id);
            if (!await VerifyUser.IsAdmin(u))
            {
                await Context.Channel.SendMessageAsync("You are not a local director of the FBI and cannot use this command.");
                return;
            }


        }

        [Command("setmodlog")]
        public async Task SetModLogAsync(string channel)
        {
            SocketTextChannel c;
            if (ulong.TryParse(channel, out ulong channelID) && (c = Context.Guild.GetTextChannel(channelID)) != null)
            {
                await SetModLogAsync(c);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given text channel does not exist.");
        }
    }
}
