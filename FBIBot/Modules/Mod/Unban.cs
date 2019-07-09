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
        public async Task UnbanAsync(SocketGuildUser user)
        {
            await Task.WhenAll
            (
                Context.Guild.RemoveBanAsync(user),
                Context.Channel.SendMessageAsync($"{user.Mention}, the now-ex-KGB spy, may reenter the nation.\n" +
                    $"They better not let their guard down."),
                SendToModLog.SendToModLogAsync(SendToModLog.LogType.Unban, Context.User as SocketGuildUser, user)
            );
        }

        [Command("unban")]
        [RequireMod]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task UnbanAsync(string user)
        {
            if (ulong.TryParse(user, out ulong userID))
            {
                SocketGuildUser u;
                if ((u = Context.Guild.GetUser(userID)) != null)
                {
                    await UnbanAsync(u);
                    return;
                }

                await Task.WhenAll
                (
                    Context.Guild.RemoveBanAsync(userID),
                    Context.Channel.SendMessageAsync($"<@{user}>, the now-ex-KGB spy, may enter the nation.\n" +
                        $"They better not let their guard down."),
                    SendToModLog.SendToModLogAsync(SendToModLog.LogType.Unban, Context.User as SocketGuildUser, userID)
                );
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given user does not exist.");
        }
    }
}
