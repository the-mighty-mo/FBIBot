using Discord.Interactions;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace FBIBot
{
    public sealed class RequireBotHierarchy : RequireHierarchy
    {
        public RequireBotHierarchy(string cmd = DEFAULT_CMD) : base(cmd) { }

        protected sealed override async Task<PreconditionResult> CheckRequirementsAsync(SocketInteractionContext Context, SocketGuildUser target) =>
            Context.User is SocketGuildUser
                ? await VerifyUser.BotIsHigher(Context.Guild.CurrentUser, target).ConfigureAwait(false)
                    ? PreconditionResult.FromSuccess()
                    : PreconditionResult.FromError($"We cannot {command} members with equal or higher authority than ourselves.")
                : PreconditionResult.FromError("You must be in a server to run this command.");
    }
}