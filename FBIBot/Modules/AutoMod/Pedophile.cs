using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FBIBot.Modules.AutoMod
{
    public class Pedophile
    {
        readonly SocketCommandContext Context;

        public Pedophile(SocketCommandContext context) => Context = context;

        public async Task ArrestAsync()
        {
            await Context.Message.DeleteAsync();
            await Context.User.SendMessageAsync("You want to explain yourself?");
            await Context.Channel.SendMessageAsync($"\\arrest {Context.User.Mention} 5");
        }

        public static async Task<bool> IsPedophileAsync(SocketUserMessage msg)
        {
            bool isPedophile = false;

            Regex regex = new Regex(@"\bI (like|love) (?!no )(?!none of the)(\w+\s|)+(children|kids)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Match match = regex.Match(msg.Content);

            isPedophile = match.Success;

            return await Task.Run(() => isPedophile);
        }
    }
}
