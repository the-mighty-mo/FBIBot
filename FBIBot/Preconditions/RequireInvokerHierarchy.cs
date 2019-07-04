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

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext Context, ParameterInfo parameter, object value, IServiceProvider services) =>
            value is SocketGuildUser user
                ? await CheckPermissionsAsync(Context as SocketCommandContext, parameter, user, services)
                : value is string userId && ulong.TryParse(userId, out ulong userID)
                    ? await CheckPermissionsAsync(Context as SocketCommandContext, parameter, userID, services)
                    : PreconditionResult.FromError("Our intelligence team has informed us that the given user does not exist.");

        private async Task<PreconditionResult> CheckPermissionsAsync(SocketCommandContext Context, ParameterInfo parameter, ulong userID, IServiceProvider services) =>
            Context.Guild.GetUser(userID) != null
                ? await CheckPermissionsAsync(Context, parameter, Context.Guild.GetUser(userID), services)
                : PreconditionResult.FromError("Our intelligence team has informed us that the given user does not exist.");

        private async Task<PreconditionResult> CheckPermissionsAsync(SocketCommandContext Context, ParameterInfo parameter, SocketGuildUser target, IServiceProvider services) =>
            Context.User is SocketGuildUser invoker
                ? await VerifyUser.InvokerIsHigher(invoker, target)
                    ? PreconditionResult.FromSuccess()
                    : PreconditionResult.FromError($"You cannot {command} members with equal or higher authority than yourself.")
                : PreconditionResult.FromError("You must be in a guild to run this command.");
    }
}
