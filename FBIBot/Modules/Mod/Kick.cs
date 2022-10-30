using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FBIBot.Modules.Mod.ModLog;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class Kick : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("kick", "Deports the criminal to probably Europe")]
        [RequireMod]
        [RequireBotPermission(GuildPermission.KickMembers)]
        public Task KickAsync([RequireBotHierarchy("kick")][RequireInvokerHierarchy("kick")] SocketUser user, string? reason = null) =>
            KickAsync((user as SocketGuildUser)!, reason);

        private Task KickAsync(SocketGuildUser user, string? reason)
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(255, 12, 12))
                .WithDescription($"The criminal {user.Mention} has been sent to Brazil.");

            EmbedFieldBuilder reasonField = new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName("Reason")
                    .WithValue($"{reason ?? "[none given]"}");
            embed.AddField(reasonField);

            return Task.WhenAll
            (
                user.KickAsync(reason),
                Context.Interaction.RespondAsync(embed: embed.Build()),
                KickModLog.SendToModLogAsync((Context.User as SocketGuildUser)!, user, reason)
            );
        }
    }
}