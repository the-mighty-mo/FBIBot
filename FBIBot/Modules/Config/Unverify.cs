using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FBIBot.Modules.AutoMod;
using FBIBot.Modules.Mod.ModLog;
using System.Linq;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Config
{
    public class Unverify : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("unverify", "Removes the verification role from the user and removes the user from the list of verified users")]
        [RequireAdmin]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task UnverifyAsync([RequireInvokerHierarchy("unverify")] SocketGuildUser user, string reason = null)
        {
            SocketRole role = await verificationDatabase.Roles.GetVerificationRoleAsync(Context.Guild);
            if (role == null)
            {
                await Context.Interaction.RespondAsync("Our intelligence team has informed us that there is no role to give to verified members.");
                return;
            }
            if (!user.Roles.Contains(role) && !await verificationDatabase.Verified.GetVerifiedAsync(user))
            {
                await Context.Interaction.RespondAsync($"Our security team has informed us that {user.Nickname ?? user.Username} is not verified.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"We have put the potential communist {user.Mention} under quarantine.");

            await Task.WhenAll
            (
                user.RemoveRoleAsync(role),
                verificationDatabase.Verified.RemoveVerifiedAsync(user),
                Verify.SendCaptchaAsync(user),
                Context.Interaction.RespondAsync(embed: embed.Build()),
                UnverifyModLog.SendToModLogAsync(Context.User as SocketGuildUser, user, reason)
            );
        }
    }
}