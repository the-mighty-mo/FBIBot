using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Modules.AutoMod.AutoSurveillance
{
    public class AutoSurveillanceArrest
    {
        readonly SocketCommandContext Context;

        public AutoSurveillanceArrest(SocketCommandContext context) => Context = context;

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
    }
}
