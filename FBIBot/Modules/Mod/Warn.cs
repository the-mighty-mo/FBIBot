using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FBIBot.Modules.Mod.ModLog;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Mod
{
    public class Warn : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("warn", "Gives the user a warning to stop protesting capitalism")]
        [RequireMod]
        [RequireModLog]
        public Task WarnAsync([RequireInvokerHierarchy("warn")] SocketUser user, [Summary(description: "Length of the warning in hours. Default: permanent")] double? length = null, string reason = null) =>
            WarnAsync(user as SocketGuildUser, length, reason);

        private async Task WarnAsync(SocketGuildUser user, double? length, string reason)
        {
            ulong id = await modLogsDatabase.ModLogs.GetNextModLogID(Context.Guild);

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(255, 213, 31))
                .WithDescription($"{user.Mention} stop protesting capitalism.");

            if (length is not null)
            {
                EmbedFieldBuilder lengthField = new EmbedFieldBuilder()
                        .WithIsInline(false)
                        .WithName("Length")
                        .WithValue(length < 1 ? $"{length * 60} minutes" : $"{length} hours");
                embed.AddField(lengthField);
            }

            EmbedFieldBuilder reasonField = new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName("Reason")
                    .WithValue($"{reason ?? "[none given]"}");
            embed.AddField(reasonField);

            await Task.WhenAll
            (
                Context.Interaction.RespondAsync(embed: embed.Build()),
                WarnModLog.SendToModLogAsync(Context.User as SocketGuildUser, user, length, reason),
                modLogsDatabase.Warnings.AddWarningAsync(user, id)
            );

            if (length is not null)
            {
                await Task.Delay((int)(length * 60 * 60 * 1000));

                if (!await modLogsDatabase.Warnings.GetWarningAsync(user, id))
                {
                    return;
                }

                await Task.WhenAll
                (
                    RemoveWarningModLog.SendToModLogAsync(Context.Guild.CurrentUser, user, id),
                    modLogsDatabase.Warnings.RemoveWarningAsync(user, id)
                );
            }
        }
    }
}