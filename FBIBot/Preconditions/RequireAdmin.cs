using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace FBIBot
{
    public class RequireAdmin : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext Context, CommandInfo command, IServiceProvider services) =>
            Context.User is SocketGuildUser user
                ? await VerifyUser.IsAdmin(user)
                    ? PreconditionResult.FromSuccess()
                    : PreconditionResult.FromError("You are not a local director of the FBI and cannot use this command.")
                : PreconditionResult.FromError("You must be in a guild to run this command.");
    }
}
