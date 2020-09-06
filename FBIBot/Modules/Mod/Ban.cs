using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FBIBot.Modules.Mod.ModLog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class Ban : ModuleBase<SocketCommandContext>
    {
        [Command("ban")]
        [RequireMod]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanAsync([RequireBotHierarchy("ban")] [RequireInvokerHierarchy("ban")] SocketGuildUser user, string prune = null, [Remainder] string reason = null)
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
                Context.Channel.SendMessageAsync("", false, embed.Build()),
                BanModLog.SendToModLogAsync(Context.User as SocketGuildUser, user, null, reason)
            });
            await Task.WhenAll(cmds);
        }

        [Command("ban")]
        [RequireMod]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanAsync(string user, string prune = null, [Remainder] string reason = null)
        {
            if (ulong.TryParse(user, out ulong userID))
            {
                SocketGuildUser u;
                if ((u = Context.Guild.GetUser(userID)) != null)
                {
                    await BanAsync(u, prune, reason);
                    return;
                }

                List<Task> cmds = int.TryParse(prune, out int pruneDays)
                    ? new List<Task>()
                    {
                        Context.Guild.AddBanAsync(userID, pruneDays, reason)
                    }
                    : new List<Task>() {
                        Context.Guild.AddBanAsync(userID, 0, reason)
                    };

                EmbedBuilder embed = new EmbedBuilder()
                    .WithColor(new Color(130, 0, 0))
                    .WithDescription($"The communist spy <@{user}> shall never enter our borders!");

                EmbedFieldBuilder reasonField = new EmbedFieldBuilder()
                        .WithIsInline(false)
                        .WithName("Reason")
                        .WithValue($"{reason ?? "[none given]"}");
                embed.AddField(reasonField);

                cmds.AddRange(new List<Task>()
                {
                    Context.Channel.SendMessageAsync("", false, embed.Build()),
                    BanModLog.SendToModLogAsync(Context.User as SocketGuildUser, userID, null, reason)
                });
                await Task.WhenAll(cmds);

                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given user does not exist.");
        }
    }
}
