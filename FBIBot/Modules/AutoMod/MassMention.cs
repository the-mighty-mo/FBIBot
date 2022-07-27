using Discord.Commands;
using System.Threading.Tasks;

namespace FBIBot.Modules.AutoMod
{
    public class MassMention
    {
        private readonly SocketCommandContext Context;

        public MassMention(SocketCommandContext context) => Context = context;

        public Task WarnAsync() =>
            Task.WhenAll
            (
                Context.Message.DeleteAsync(),
                Context.Channel.SendMessageAsync($"\\tempwarn {Context.User.Mention} 0.5 Big mass mention\n" +
                    $"Message: {Context.Message.Content}")
            );

        public static Task<bool> IsMassMentionAsync(SocketCommandContext Context)
        {
            int count = Context.Message.MentionedRoles.Count + Context.Message.MentionedUsers.Count;
            return Task.Run(() => count > 5);
        }
    }
}