using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FBIBot.Modules.Mod.ModLog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class Ban : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("ban", "Gives the communist the ~~ban~~ freedom hammer")]
        [RequireMod]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public Task BanAsync([RequireBotHierarchy("ban")][RequireInvokerHierarchy("ban")] SocketUser user, [Summary(description: "Length of the ban in days. Default: permanent")] double? length = null, [Summary(description: "Number of days of the communist's messages to prune")] string prune = null, string reason = null) =>
            BanAsync(user as SocketGuildUser, length, prune, reason);

        private async Task BanAsync(SocketGuildUser user, double? length, string prune, string reason)
        {
            if (length is null)
            {
                await BanPrivAsync(user, prune, reason);
            }
            else
            {
                await TempBanPrivAsync(user, length.Value, prune, reason);
            }
        }

        private async Task BanPrivAsync(SocketGuildUser user, string prune, string reason)
        {
            List<Task> cmds = int.TryParse(prune, out int pruneDays)
                ? new List<Task>() {
                    user.BanAsync(pruneDays, reason)
                }
                : new List<Task>() {
                    user.BanAsync(0, reason)
                };

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(130, 0, 0))
                .WithDescription($"The communist spy {user.Mention} has been given the ~~ban~~ freedom hammer.");

            EmbedFieldBuilder reasonField = new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName("Reason")
                    .WithValue($"{reason ?? "[none given]"}");
            embed.AddField(reasonField);

            cmds.AddRange(new List<Task>()
            {
                Context.Interaction.RespondAsync(embed: embed.Build()),
                BanModLog.SendToModLogAsync(Context.User as SocketGuildUser, user, null, reason)
            });
            await Task.WhenAll(cmds);
        }

        private async Task TempBanPrivAsync(SocketGuildUser user, double length, string prune, string reason)
        {
            List<Task> cmds = int.TryParse(prune, out int pruneDays)
                ? new List<Task>() {
                    user.BanAsync(pruneDays, reason)
                }
                : new List<Task>() {
                    user.BanAsync(0, reason)
                };

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(130, 0, 0))
                .WithDescription($"The communist spy {user.Mention} has been sent to Brazil for {length} {(length == 1 ? "day" : "days")}.");

            EmbedFieldBuilder reasonField = new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName("Reason")
                    .WithValue($"{reason ?? "[none given]"}");
            embed.AddField(reasonField);

            cmds.AddRange(new List<Task>()
            {
                Context.Interaction.RespondAsync(embed: embed.Build()),
                BanModLog.SendToModLogAsync(Context.User as SocketGuildUser, user, length, reason)
            });
            await Task.WhenAll(cmds);

            await Task.Delay((int)(length * 24 * 60 * 60 * 1000));
            await Task.WhenAll
            (
                Context.Guild.RemoveBanAsync(user),
                UnbanModLog.SendToModLogAsync(Context.Guild.CurrentUser, user)
            );
        }
    }
}