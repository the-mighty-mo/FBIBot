using Discord.Commands;
using System.Threading.Tasks;

namespace FBIBot.Modules.AutoMod
{
    public class MassMention
    {
        readonly SocketCommandContext Context;

        public MassMention(SocketCommandContext context) => Context = context;

        public async Task WarnAsync()
        {
            await Task.WhenAll
            (
                Context.Message.DeleteAsync(),
                Context.Channel.SendMessageAsync($"\\warn {Context.User.Mention} 0.5 Big mass mention\n" +
                    $"Message: {Context.Message.Content}")
            );
        }

        public static async Task<bool> IsMassMentionAsync(SocketCommandContext Context)
        {
            await Task.Yield();

            int count = Context.Message.MentionedRoles.Count + Context.Message.MentionedUsers.Count;
            bool hasMassMention = count >= 5;

            return hasMassMention;
        }
    }
}
