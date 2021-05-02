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
            if (context is SocketCommandContext Context)
            {
                if (value is SocketGuildUser user)
                {
                    return await CheckPermissionsAsync(Context, user);
                }
                else if (value is string userId && ulong.TryParse(userId, out ulong userID) && (user = Context.Guild.GetUser(userID)) != null)
                {
                    return await CheckPermissionsAsync(Context, user);
                }
            }
            return PreconditionResult.FromError("Our intelligence team has informed us that the given user does not exist.");
        }

        protected abstract Task<PreconditionResult> CheckPermissionsAsync(SocketCommandContext Context, SocketGuildUser target);
    }
}