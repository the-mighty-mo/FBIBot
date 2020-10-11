using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot
{
    public class RequireModLog : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext Context, CommandInfo command, IServiceProvider services) =>
            Context.User is SocketGuildUser
                ? await modLogsDatabase.ModLogChannel.GetModLogChannelAsync((Context as SocketCommandContext)?.Guild) != null
                    ? PreconditionResult.FromSuccess()
                    : PreconditionResult.FromError("You must set a mod log to use this command.")
                : PreconditionResult.FromError("You must be in a server to run this command.");
    }
}
