using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot
{
    public class RequireModLog : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext Context, ICommandInfo command, IServiceProvider services) =>
            Context.User is SocketGuildUser && Context is SocketInteractionContext context
                ? await modLogsDatabase.ModLogChannel.GetModLogChannelAsync(context.Guild) != null
                    ? PreconditionResult.FromSuccess()
                    : PreconditionResult.FromError("You must set a mod log to use this command.")
                : PreconditionResult.FromError("You must be in a server to run this command.");
    }
}