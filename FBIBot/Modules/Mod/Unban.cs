using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class Unban : ModuleBase<SocketCommandContext>
    {
        [Command("unban")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanAsync(SocketGuildUser user)
        {
            SocketGuildUser u = Context.Guild.GetUser(Context.User.Id);
            if (!await VerifyUser.IsMod(u))
            {
                await Context.Channel.SendMessageAsync("You are not an assistant of the FBI and cannot use this command.");
                return;
            }

            await Context.Guild.RemoveBanAsync(user);
            await Context.Channel.SendMessageAsync($"{user.Mention}, the now-ex-KGB spy, may reenter the nation.\n" +
                $"They better not let their guard down.");
            await SendToModLog.SendToModLogAsync(SendToModLog.LogType.Unban, Context.User, user);
        }

        [Command("unban")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanAsync(string user)
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
                    await BanAsync(u);
                    return;
                }
                await Context.Guild.RemoveBanAsync(userID);
                await Context.Channel.SendMessageAsync($"<@{user}>, the now-ex-KGB spy, may enter the nation.\n" +
                    $"They better not let their guard down.");
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given user does not exist.");
        }
    }
}
