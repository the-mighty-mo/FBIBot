using Discord;
using Discord.Interactions;
using FBIBot.Modules.Mod.ModLog;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Mod
{
    public class ModifyReason : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("modify-reason", "Modifies the reason for the given mod log")]
        [RequireMod]
        [RequireModLog]
        public async Task ModifyReasonAsync(ulong id, string? reason = null)
        {
            if (id >= await modLogsDatabase.ModLogs.GetNextModLogID(Context.Guild))
            {
                await Context.Interaction.RespondAsync($"Our security team has informed us that {id} is not a valid Mod Log ID.");
                return;
            }

            if (!await ModLogManager.SetReasonAsync(new ModLogManager.ReasonInfo(Context.Guild, id, reason)))
            {
                await Context.Interaction.RespondAsync("Our security team has informed us that the given Mod Log is not permitted to have a valid reason. Don't ask.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithDescription("The mod log's reason has been updated. Probably.");

            await Context.Interaction.RespondAsync(embed: embed.Build());
        }
    }
}