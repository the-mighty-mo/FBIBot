using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class TempBan : ModuleBase<SocketCommandContext>
    {
        [Command("tempban")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task TempBanAsync(SocketGuildUser user, string length, string prune = null, [Remainder] string reason = null)
        {
            SocketGuildUser u = Context.Guild.GetUser(Context.User.Id);
            if (!await VerifyUser.IsMod(u))
            {
                await Context.Channel.SendMessageAsync("You are not an assistant of the FBI and cannot use this command.");
                return;
            }

            if (!double.TryParse(length, out double days))
            {
                await Context.Channel.SendMessageAsync($"Unfortunately, {length} is not a valid prison sentence length.");
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

            await Context.Channel.SendMessageAsync($"The communist spy {user.Mention} has been exiled to Mexico for {length} {(days == 1 ? "day" : "days")}." +
                $"{(reason != null ? $"\nThe reason: {reason}" : "")}");
            await SendToModLog.SendToModLogAsync(SendToModLog.LogType.Ban, Context.User, user, length, reason);

            await Task.Delay((int)(days * 24 * 60 * 60 * 1000));
            await Context.Guild.RemoveBanAsync(user);
            await SendToModLog.SendToModLogAsync(SendToModLog.LogType.Unban, Context.Client.CurrentUser, user);
        }

        [Command("tempban")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task TempBanAsync(string user, string length, string prune = null, [Remainder] string reason = null)
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
                if ((u = Context.Guild.GetUser(userID)) != null || !double.TryParse(length, out double days))
                {
                    await TempBanAsync(u, length, prune, reason);
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
                await Context.Channel.SendMessageAsync($"The communist spy <@{user}> shall not enter our borders for {length} {(days == 1 ? "day" : "days")}." +
                    $"{(reason != null ? $"\nThe reason: {reason}" : "")}");

                await Task.Delay((int)(days * 24 * 60 * 60 * 1000));
                await Context.Guild.RemoveBanAsync(userID);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given user does not exist.");
        }
    }
}
