using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace FBIBot.Modules.AutoMod
{
    public partial class Invite
    {
        private static readonly Regex regex = InviteRegex();

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

        [GeneratedRegex(@"discord\.(gg|com\/invite)\/\S+", RegexOptions.IgnoreCase)]
        private static partial Regex InviteRegex();
    }
}