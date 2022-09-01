using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FBIBot.Modules.Mod.ModLog;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Mod
{
    public class RemoveWarnings : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("remove-warnings", "Removes a number of warnings from the user. Removes the oldest first")]
        [RequireMod]
        public async Task RemoveWarnsAsync([RequireInvokerHierarchy("remove warnings from")] SocketGuildUser user, [Summary(description: "Default: all")] int? count = null)
        {
            if (count == 0)
            {
                await Context.Interaction.RespondAsync("Why are you trying to remove ***0*** warnings? I have more important things to do.");
                return;
            }
            if ((await modLogsDatabase.Warnings.GetWarningsAsync(user)).Count == 0)
            {
                await Context.Interaction.RespondAsync("Our security team has informed us that the given user does not have any warnings.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(12, 156, 24))
                .WithDescription($"We have pardoned {count?.ToString() ?? "all"} warnings for {user.Mention}.");

            await Task.WhenAll
            (
                Context.Interaction.RespondAsync(embed: embed.Build()),
                RemoveWarningsModLog.SendToModLogAsync(Context.User as SocketGuildUser, user, count),
                modLogsDatabase.Warnings.RemoveWarningsAsync(user, count)
            );
        }
    }
}