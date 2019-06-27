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
        [RequireOwner()]
        public async Task BanAsync(SocketGuildUser user)
        {
            await Context.Guild.RemoveBanAsync(user);
            await Context.Channel.SendMessageAsync($"{user.Mention}, the now-ex-KGB spy, may reenter the nation.\n" +
                $"They not let their guard down.");
        }

        [Command("unban")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireOwner()]
        public async Task BanAsync(string user)
        {
            SocketGuildUser u;
            if (ulong.TryParse(user, out ulong userID) && (u = Context.Guild.GetUser(userID)) != null)
            {
                await BanAsync(u);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given user does not exist.");
        }
    }
}
