using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FBIBot.Modules.Mod.ModLog;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Mod
{
    public class RemoveWarnings : ModuleBase<SocketCommandContext>
    {
        [Command("removewarnings")]
        [Alias("remove-warnings", "clearwarnings", "clear-warnings")]
        [RequireMod]
        public async Task RemoveWarnsAsync([RequireInvokerHierarchy("remove warnings from")] SocketGuildUser user, string count = null)
        {
            if (count != null && !int.TryParse(count, out int _))
            {
                await Context.Channel.SendMessageAsync($"Our intelligence team has informed us that {count} is not a valid number of warnings.");
                return;
            }
            if (count == "0")
            {
                await Context.Channel.SendMessageAsync("Why are you trying to remove ***0*** warnings? I have more important things to do.");
                return;
            }
            if ((await modLogsDatabase.Warnings.GetWarningsAsync(user)).Count == 0)
            {
                await Context.Channel.SendMessageAsync("Our security team has informed us that the given user does not have any warnings.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(12, 156, 24))
                .WithDescription($"We have pardoned {count ?? "all"} warnings for {user.Mention}.");

            await Task.WhenAll
            (
                Context.Channel.SendMessageAsync("", false, embed.Build()),
                RemoveWarningsModLog.SendToModLogAsync(Context.User as SocketGuildUser, user, count),
                modLogsDatabase.Warnings.RemoveWarningsAsync(user, count)
            );
        }

        [Command("removewarnings")]
        [Alias("remove-warnings")]
        [RequireMod]
        public async Task RemoveWarnsAsync(string user, string count = null)
        {
            SocketGuildUser u;
            if (ulong.TryParse(user, out ulong userID) && (u = Context.Guild.GetUser(userID)) != null)
            {
                await RemoveWarnsAsync(u, count);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given user does not exist.");
        }
    }
}
