using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class Ban : ModuleBase<SocketCommandContext>
    {
        [Command("ban")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanAsync(SocketGuildUser user, string prune = null, [Remainder] string reason = null)
        {
            SocketGuildUser u = Context.Guild.GetUser(Context.User.Id);
            if (!await VerifyUser.IsMod(u))
            {
                await Context.Channel.SendMessageAsync("You are not an assistant of the FBI and cannot use this command.");
                return;
            }
            if (!await VerifyUser.BotIsHigher(Context.Guild.CurrentUser, user))
            {
                await Context.Channel.SendMessageAsync("We cannot ban members with equal or higher authority than ourselves.");
                return;
            }
            if (!await VerifyUser.InvokerIsHigher(u, user))
            {
                await Context.Channel.SendMessageAsync("You cannot ban members with equal or higher authority than yourself.");
                return;
            }

            if (int.TryParse(prune, out int pruneDays))
            {
                await user.BanAsync(pruneDays, reason);
            }
            else
            {
                await user.BanAsync(0, reason);
            }

            await Context.Channel.SendMessageAsync($"The communist spy {user.Mention} has been given the ~~ban~~ freedom hammer." +
                $"{(reason != null ? $"\nThe reason: {reason}" : "")}");
            await SendToModLog.SendToModLogAsync(SendToModLog.LogType.Ban, Context.User, user, null, reason);
        }

        [Command("ban")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanAsync(string user, string prune = null, [Remainder] string reason = null)
        {
            SocketGuildUser cu = Context.Guild.GetUser(Context.User.Id);
            if (!await VerifyUser.IsMod(cu))
            {
                await Context.Channel.SendMessageAsync("You are not an assistant of the FBI and cannot use this command.");
                return;
            }

            if (ulong.TryParse(user, out ulong userID))
            {
                SocketGuildUser u;
                if ((u = Context.Guild.GetUser(userID)) != null)
                {
                    await BanAsync(u, prune, reason);
                    return;
                }

                if (int.TryParse(prune, out int pruneDays))
                {
                    await Context.Guild.AddBanAsync(userID, pruneDays, reason);
                }
                else
                {
                    await Context.Guild.AddBanAsync(userID, 0, reason);
                }
                await Context.Channel.SendMessageAsync($"The communist spy <@{user}> shall not enter our borders." +
                    $"{(reason != null ? $"\nThe reason: {reason}" : "")}");
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given user does not exist.");
        }
    }
}
