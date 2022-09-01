using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FBIBot.Modules.Mod.ModLog;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Mod
{
    public class RemoveWarning : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("remove-warning", "Removes the given warning from the user")]
        [RequireMod]
        public async Task RemoveWarnAsync([RequireInvokerHierarchy("remove warnings from")] SocketGuildUser user, ulong id)
        {
            if (!await modLogsDatabase.Warnings.GetWarningAsync(user, id))
            {
                await Context.Interaction.RespondAsync($"Our security team has informed us that the given warning does not exist.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(12, 156, 24))
                .WithDescription($"The given warning has been pardoned.");

            await Task.WhenAll
            (
                Context.Interaction.RespondAsync(embed: embed.Build()),
                RemoveWarningModLog.SendToModLogAsync(Context.User as SocketGuildUser, user, id.ToString()),
                modLogsDatabase.Warnings.RemoveWarningAsync(user, id)
            );
        }
    }
}