using Discord;
using Discord.Interactions;
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

        public sealed override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, IParameterInfo parameter, object value, IServiceProvider services)
        {
            if (context is SocketInteractionContext Context)
            {
                if (value is SocketGuildUser user)
                {
                    return await CheckRequirementsAsync(Context, user).ConfigureAwait(false);
                }
                else if (value is string userId && ulong.TryParse(userId, out ulong userID) && (user = Context.Guild.GetUser(userID)) != null)
                {
                    return await CheckRequirementsAsync(Context, user).ConfigureAwait(false);
                }
            }
            return PreconditionResult.FromError("Our intelligence team has informed us that the given user does not exist.");
        }

        protected abstract Task<PreconditionResult> CheckRequirementsAsync(SocketInteractionContext Context, SocketGuildUser target);
    }
}