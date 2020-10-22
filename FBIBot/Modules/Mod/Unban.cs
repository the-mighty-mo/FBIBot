using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FBIBot.Modules.Mod.ModLog;
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
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(12, 156, 24))
                .WithDescription($"{user.Mention}, the now-ex-KGB spy, may reenter the nation.\n" +
                    $"They better not let their guard down.");

            await Task.WhenAll
            (
                Context.Guild.RemoveBanAsync(user),
                Context.Channel.SendMessageAsync(embed: embed.Build()),
                UnbanModLog.SendToModLogAsync(Context.User as SocketGuildUser, user)
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

                EmbedBuilder embed = new EmbedBuilder()
                    .WithColor(new Color(12, 156, 24))
                    .WithDescription($"<@{user}>, the now-ex-KGB spy, may enter the nation.\n" +
                        $"They better not let their guard down.");

                await Task.WhenAll
                (
                    Context.Guild.RemoveBanAsync(userID),
                    Context.Channel.SendMessageAsync(embed: embed.Build()),
                    UnbanModLog.SendToModLogAsync(Context.User as SocketGuildUser, userID)
                );
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given user does not exist.");
        }
    }
}
