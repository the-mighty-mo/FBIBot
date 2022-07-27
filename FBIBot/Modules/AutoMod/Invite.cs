using Discord;
using Discord.Commands;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FBIBot.Modules.AutoMod
{
    public class Invite
    {
        private static readonly Regex regex = new(@"discord\.(gg|com\/invite)\/\S+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly SocketCommandContext Context;

        public Invite(SocketCommandContext context) => Context = context;

        public Task RemoveAsync() =>
            Task.WhenAll
            (
                Context.Message.DeleteAsync(),
                Context.User.SendMessageAsync($"You cannot send invite links in the server {Context.Guild.Name}.\n" +
                    $"Message removed: {Context.Message.Content}")
            );

        public static Task<bool> HasInviteAsync(SocketCommandContext Context)
        {
            var message = Context.Message.Content;
            return Task.Run(() => regex.IsMatch(message));
        }
    }
}