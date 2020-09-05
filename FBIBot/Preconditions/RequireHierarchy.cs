using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace FBIBot
{
    public abstract class RequireHierarchy : ParameterPreconditionAttribute
    {
        protected readonly string command;
        protected const string DEFAULT_CMD = "run this command on";

        protected RequireHierarchy(string cmd = DEFAULT_CMD) => command = cmd;

        public sealed override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
        {
            SocketCommandContext Context = context as SocketCommandContext;

            return value is SocketGuildUser user
                ? await CheckPermissionsAsync(Context, user)
                : value is string userId && ulong.TryParse(userId, out ulong userID) && (user = Context.Guild.GetUser(userID)) != null
                    ? await CheckPermissionsAsync(Context, user)
                    : PreconditionResult.FromError("Our intelligence team has informed us that the given user does not exist.");
        }

        protected abstract Task<PreconditionResult> CheckPermissionsAsync(SocketCommandContext Context, SocketGuildUser target);
    }
}
