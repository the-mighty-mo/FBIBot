using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace FBIBot.Modules.AutoMod
{
    public class Link
    {
        private readonly SocketCommandContext Context;

        public Link(SocketCommandContext context) => Context = context;

        public Task RemoveAsync() =>
            Task.WhenAll
            (
                Context.Message.DeleteAsync(),
                Context.User.SendMessageAsync($"You cannot send links in the server {Context.Guild.Name}.\n" +
                    $"Message removed: {Context.Message.Content}")
            );

        public static Task<bool> IsLinkAsync(SocketCommandContext Context)
        {
            var message = Context.Message.Content;
            return Task.Run(() => message.Contains("http://") || message.Contains("https://"));
        }
    }
}