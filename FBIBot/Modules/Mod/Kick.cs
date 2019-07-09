using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class Kick : ModuleBase<SocketCommandContext>
    {
        [Command("kick")]
        [RequireMod]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task KickAsync([RequireBotHierarchy("kick")] [RequireInvokerHierarchy("kick")] SocketGuildUser user, [Remainder] string reason = null)
        {
            await Task.WhenAll
            (
                user.KickAsync(reason),
                Context.Channel.SendMessageAsync($"The criminal {user.Mention} has been deported to probably Europe."),
                SendToModLog.SendToModLogAsync(SendToModLog.LogType.Kick, Context.User as SocketGuildUser, user, null, reason)
            );
        }

        [Command("kick")]
        [RequireMod]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task KickAsync(string user, [Remainder] string reason = null)
        {
            SocketGuildUser u;
            if (ulong.TryParse(user, out ulong userID) && (u = Context.Guild.GetUser(userID)) != null)
            {
                await KickAsync(u, reason);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given user does not exist.");
        }
    }
}
