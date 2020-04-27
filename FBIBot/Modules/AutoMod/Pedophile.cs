using Discord;
using Discord.Commands;
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
            List<string> messages = new List<string>()
            {
                "You want to explain yourself?",
                "Why don't you take a seat over there?",
                "FBI OPEN UP!",
                "Where do you think you're going?"
            };

            await Task.WhenAll
            (
                Context.Message.DeleteAsync(),
                Context.Channel.SendMessageAsync($"\\arrest {Context.User.Mention} 5"),
                Context.User.SendMessageAsync(messages[Program.rng.Next(messages.Count)])
            );
        }

        public static async Task<bool> IsPedophileAsync(SocketCommandContext Context)
        {
            await Task.Yield();

            string message = Context.Message.Content;
            Regex regex = new Regex(@"\bI\s+(like|love)\s+(?!(no(?!\w+)|none\s*of))(\w*\s)+(children|kids)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            return regex.IsMatch(message);
        }
    }
}
