using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace FBIBot.Modules.AutoMod
{
    public class Link
    {
        readonly SocketCommandContext Context;

        public Link(SocketCommandContext context) => Context = context;

        public async Task RemoveAsync()
        {
            await Context.Message.DeleteAsync();
            await Context.User.SendMessageAsync($"You cannot send links in the server {Context.Guild.Name}.\n" +
                $"Message removed: {Context.Message.Content}");
        }

        public static async Task<bool> IsLinkAsync(SocketCommandContext Context)
        {
            string message = Context.Message.Content;
            bool hasLink = message.Contains("http://") || message.Contains("https://");

            return await Task.Run(() => hasLink);
        }
    }
}
