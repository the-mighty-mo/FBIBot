using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace FBIBot
{
    public sealed class RequireInvokerHierarchy : RequireHierarchy
    {
        public RequireInvokerHierarchy(string cmd = DEFAULT_CMD) : base(cmd) { }

        protected sealed override async Task<PreconditionResult> CheckPermissionsAsync(SocketCommandContext Context, SocketGuildUser target) =>
            Context.User is SocketGuildUser invoker
                ? await VerifyUser.InvokerIsHigher(invoker, target)
                    ? PreconditionResult.FromSuccess()
                    : PreconditionResult.FromError($"You cannot {command} members with equal or higher authority than yourself.")
                : PreconditionResult.FromError("You must be in a server to run this command.");
    }
}
