using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FBIBot.Modules.AutoMod
{
    public class KillThePresident
    {
        readonly SocketCommandContext Context;

        public KillThePresident(SocketCommandContext context) => Context = context;

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

        public static async Task<bool> IsGoingToKillThePresidentAsync(SocketCommandContext Context)
        {
            await Task.Yield();

            string message = Context.Message.Content;
            Regex regex = new Regex(@"\bI(('m|\s+am)\s+going\s+to|\s+want\s+to)(\s+\w*)*(kill|kil(?!\w+)|assassinate|murder)(\w*\s)+the\s+president\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            return regex.IsMatch(message);
        }
    }
}
