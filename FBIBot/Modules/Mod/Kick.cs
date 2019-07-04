using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class Kick : ModuleBase<SocketCommandContext>
    {
        [Command("kick")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task KickAsync(SocketGuildUser user, [Remainder] string reason = null)
        {
            SocketGuildUser u = Context.Guild.GetUser(Context.User.Id);
            if (!await VerifyUser.IsMod(u))
            {
                await Context.Channel.SendMessageAsync("You are not an assistant of the FBI and cannot use this command.");
                return;
            }
            if (!await VerifyUser.BotIsHigher(Context.Guild.CurrentUser, user))
            {
                await Context.Channel.SendMessageAsync("We cannot kick members with equal or higher authority than ourselves.");
                return;
            }
            if (!await VerifyUser.InvokerIsHigher(u, user))
            {
                await Context.Channel.SendMessageAsync("You cannot kick members with equal or higher authority than yourself.");
                return;
            }

            await user.KickAsync(reason);
            await Context.Channel.SendMessageAsync($"The criminal {user.Mention} has been deported to probably Europe." +
                $"{(reason != null ? $"\nThe reason: {reason}" : "")}");
            await SendToModLog.SendToModLogAsync(SendToModLog.LogType.Kick, Context.User, user, null, reason);
        }

        [Command("kick")]
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
