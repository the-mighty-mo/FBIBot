using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FBIBot.Modules.Mod.ModLog;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class Kick : ModuleBase<SocketCommandContext>
    {
        [Command("kick")]
        [RequireMod]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task KickAsync([RequireBotHierarchy("kick")] [RequireInvokerHierarchy("kick")] SocketGuildUser user, [Remainder] string reason = null)
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(255, 12, 12))
                .WithDescription($"The criminal {user.Mention} has been sent to Brazil.");

            EmbedFieldBuilder reasonField = new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName("Reason")
                    .WithValue($"{reason ?? "[none given]"}");
            embed.AddField(reasonField);

            await Task.WhenAll
            (
                user.KickAsync(reason),
                Context.Channel.SendMessageAsync(embed: embed.Build()),
                KickModLog.SendToModLogAsync(Context.User as SocketGuildUser, user, reason)
            );
        }

        [Command("kick")]
        [RequireMod]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task KickAsync(string user, [Remainder] string reason = null)
        {
            SocketGuildUser u;
            if (ulong.TryParse(user, out ulong userID) && (u = Context.Guild.GetUser(userID)) != null)
            {
                await KickAsync(u, reason);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given user does not exist.");
        }
    }
}