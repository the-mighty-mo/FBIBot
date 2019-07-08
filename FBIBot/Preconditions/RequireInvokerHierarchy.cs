using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace FBIBot
{
    public class RequireInvokerHierarchy : ParameterPreconditionAttribute
    {
        private readonly string command;

        public RequireInvokerHierarchy(string cmd = "run this command on") => command = cmd;

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
        {
            SocketCommandContext Context = context as SocketCommandContext;

            return value is SocketGuildUser user
                ? await CheckPermissionsAsync(Context, user)
                : value is string userId && ulong.TryParse(userId, out ulong userID) && (user = Context.Guild.GetUser(userID)) != null
                    ? await CheckPermissionsAsync(Context, user)
                    : PreconditionResult.FromError("Our intelligence team has informed us that the given user does not exist.");
        }

        private async Task<PreconditionResult> CheckPermissionsAsync(SocketCommandContext Context, SocketGuildUser target) =>
            Context.User is SocketGuildUser invoker
                ? await VerifyUser.InvokerIsHigher(invoker, target)
                    ? PreconditionResult.FromSuccess()
                    : PreconditionResult.FromError($"You cannot {command} members with equal or higher authority than yourself.")
                : PreconditionResult.FromError("You must be in a server to run this command.");
    }
}
