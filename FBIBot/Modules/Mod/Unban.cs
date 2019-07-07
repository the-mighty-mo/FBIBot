using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class Unban : ModuleBase<SocketCommandContext>
    {
        [Command("unban")]
        [RequireMod]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanAsync(SocketGuildUser user)
        {
            await Context.Guild.RemoveBanAsync(user);
            await Context.Channel.SendMessageAsync($"{user.Mention}, the now-ex-KGB spy, may reenter the nation.\n" +
                $"They better not let their guard down.");
            await SendToModLog.SendToModLogAsync(SendToModLog.LogType.Unban, Context.User as SocketGuildUser, user);
        }

        [Command("unban")]
        [RequireMod]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanAsync(string user)
        {
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
