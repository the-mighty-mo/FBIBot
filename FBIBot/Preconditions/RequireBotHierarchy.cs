using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace FBIBot
{
    public class RequireBotHierarchy : ParameterPreconditionAttribute
    {
        private readonly string command;

        public RequireBotHierarchy(string cmd = "run this command on") => command = cmd;

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext Context, ParameterInfo parameter, object value, IServiceProvider services) =>
            value is SocketGuildUser user
                ? await CheckPermissionsAsync(Context as SocketCommandContext, user)
                : value is string userId && ulong.TryParse(userId, out ulong userID)
                    ? await CheckPermissionsAsync(Context as SocketCommandContext, userID)
                    : PreconditionResult.FromError("Our intelligence team has informed us that the given user does not exist.");

        private async Task<PreconditionResult> CheckPermissionsAsync(SocketCommandContext Context, ulong userID) =>
            Context.Guild.GetUser(userID) != null
                ? await CheckPermissionsAsync(Context, Context.Guild.GetUser(userID))
                : PreconditionResult.FromError("Our intelligence team has informed us that the given user does not exist.");

        private async Task<PreconditionResult> CheckPermissionsAsync(SocketCommandContext Context, SocketGuildUser target) =>
            Context.User is SocketGuildUser
                ? await VerifyUser.BotIsHigher(Context.Guild.CurrentUser, target)
                    ? PreconditionResult.FromSuccess()
                    : PreconditionResult.FromError($"We cannot {command} members with equal or higher authority than ourselves.")
                : PreconditionResult.FromError("You must be in a server to run this command.");
    }
}
