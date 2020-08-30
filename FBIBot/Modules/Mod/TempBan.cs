using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class TempBan : ModuleBase<SocketCommandContext>
    {
        [Command("tempban")]
        [Alias("temp-ban")]
        [RequireMod]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task TempBanAsync([RequireBotHierarchy("tempban")] [RequireInvokerHierarchy("tempban")] SocketGuildUser user, string length, string prune = null, [Remainder] string reason = null)
        {
            if (!double.TryParse(length, out double days))
            {
                await Context.Channel.SendMessageAsync($"Unfortunately, {length} is not a valid prison sentence length.");
                return;
            }

            List<Task> cmds = int.TryParse(prune, out int pruneDays)
                ? new List<Task>() {
                    user.BanAsync(pruneDays, reason)
                }
                : new List<Task>() {
                    user.BanAsync(0, reason)
                };

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(130, 0, 0))
                .WithDescription($"The communist spy {user.Mention} has been sent to Brazil for {length} {(days == 1 ? "day" : "days")}.");

            EmbedFieldBuilder reasonField = new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName("Reason")
                    .WithValue($"{reason ?? "[none given]"}");
            embed.AddField(reasonField);

            cmds.AddRange(new List<Task>()
            {
                Context.Channel.SendMessageAsync("", false, embed.Build()),
                SendToModLog.SendToModLogAsync(SendToModLog.LogType.Ban, Context.User as SocketGuildUser, user, length, reason)
            });
            await Task.WhenAll(cmds);

            await Task.Delay((int)(days * 24 * 60 * 60 * 1000));
            await Task.WhenAll
            (
                Context.Guild.RemoveBanAsync(user),
                SendToModLog.SendToModLogAsync(SendToModLog.LogType.Unban, Context.Guild.CurrentUser, user)
            );
        }

        [Command("tempban")]
        [Alias("temp-ban")]
        [RequireMod]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task TempBanAsync(string user, string length, string prune = null, [Remainder] string reason = null)
        {
            if (ulong.TryParse(user, out ulong userID))
            {
                SocketGuildUser u;
                if ((u = Context.Guild.GetUser(userID)) != null || !double.TryParse(length, out double days))
                {
                    await TempBanAsync(u, length, prune, reason);
                    return;
                }

                List<Task> cmds = int.TryParse(prune, out int pruneDays)
                    ? new List<Task>()
                    {
                        Context.Guild.AddBanAsync(userID, pruneDays, reason)
                    }
                    : new List<Task>()
                    {
                        Context.Guild.AddBanAsync(userID, 0, reason)
                    };

                EmbedBuilder embed = new EmbedBuilder()
                    .WithColor(new Color(130, 0, 0))
                    .WithDescription($"The communist spy <@{user}> shall not enter our borders for {length} {(days == 1 ? "day" : "days")}!");

                EmbedFieldBuilder reasonField = new EmbedFieldBuilder()
                        .WithIsInline(false)
                        .WithName("Reason")
                        .WithValue($"{reason ?? "[none given]"}");
                embed.AddField(reasonField);

                cmds.AddRange(new List<Task>()
                {
                    Context.Channel.SendMessageAsync("", false, embed.Build()),
                    SendToModLog.SendToModLogAsync(SendToModLog.LogType.Ban, Context.User as SocketGuildUser, userID, length, reason)
                });
                await Task.WhenAll(cmds);

                await Task.Delay((int)(days * 24 * 60 * 60 * 1000));
                await Task.WhenAll
                (
                    Context.Guild.RemoveBanAsync(userID),
                    SendToModLog.SendToModLogAsync(SendToModLog.LogType.Unban, Context.Guild.CurrentUser, userID)
                );
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given user does not exist.");
        }
    }
}
