using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace FBIBot.Modules.AutoMod.AutoSurveillance
{
    public class AutoSurveillanceArrest
    {
        private static readonly string[] messages = new string[]
        {
            "You want to explain yourself?",
            "Why don't you take a seat over there?",
            "FBI OPEN UP!",
            "Where do you think you're going?"
        };

        private readonly SocketCommandContext Context;

        public AutoSurveillanceArrest(SocketCommandContext context) => Context = context;

        public Task ArrestAsync() =>
            Task.WhenAll
            (
                Context.Message.DeleteAsync(),
                Context.Channel.SendMessageAsync($"\\temp-arrest {Context.User.Mention} 5"),
                Context.User.SendMessageAsync(messages[Program.Rng.Next(messages.Length)])
            );
    }
}
