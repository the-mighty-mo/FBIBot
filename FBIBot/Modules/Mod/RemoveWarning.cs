using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FBIBot.Modules.Mod.ModLog;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Mod
{
    public class RemoveWarning : ModuleBase<SocketCommandContext>
    {
        [Command("removewarning")]
        [Alias("remove-warning")]
        [RequireMod]
        public async Task RemoveWarnAsync([RequireInvokerHierarchy("remove warnings from")] SocketGuildUser user, string id)
        {
            if (!ulong.TryParse(id, out ulong ID))
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that {id} is not a valid Mod Log ID.");
                return;
            }

            if (!await modLogsDatabase.Warnings.GetWarningAsync(user, ID))
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that the given warning does not exist.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(12, 156, 24))
                .WithDescription($"The given warning has been pardoned.");

            await Task.WhenAll
            (
                Context.Channel.SendMessageAsync("", false, embed.Build()),
                RemoveWarningModLog.SendToModLogAsync(Context.User as SocketGuildUser, user, id),
                modLogsDatabase.Warnings.RemoveWarningAsync(user, ID)
            );
        }

        [Command("removewarning")]
        [Alias("remove-warning")]
        [RequireMod]
        public async Task RemoveWarnAsync(string user, string id)
        {
            SocketGuildUser u;
            if (ulong.TryParse(user, out ulong userID) && (u = Context.Guild.GetUser(userID)) != null)
            {
                await RemoveWarnAsync(u, id);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given user does not exist.");
        }
    }
}
