using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FBIBot.Modules.AutoMod;
using FBIBot.Modules.Mod;
using System.Linq;
using System.Threading.Tasks;

namespace FBIBot.Modules.Config
{
    public class Unverify : ModuleBase<SocketCommandContext>
    {
        [Command("unverify")]
        [RequireAdmin]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task UnverifyAsync([RequireBotHierarchy("unverify")] [RequireInvokerHierarchy("unverify")] SocketGuildUser user, [Remainder] string reason = null)
        {
            SocketRole role = await SetVerify.GetVerificationRoleAsync(Context.Guild);
            if (role == null)
            {
                await Context.Channel.SendMessageAsync("Our intelligence team has informed us that there is no role to give to verified members.");
                return;
            }
            if (!user.Roles.Contains(role) && !await Verify.GetVerifiedAsync(user))
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that {user.Nickname ?? user.Username} is not verified.");
                return;
            }

            await Task.WhenAll
            (
                user.RemoveRoleAsync(role),
                Verify.RemoveVerifiedAsync(user),
                Verify.SendCaptchaAsync(user),
                Context.Channel.SendMessageAsync($"We have put the potential communist {user.Mention} under quarantine."),
                SendToModLog.SendToModLogAsync(SendToModLog.LogType.Unverify, Context.User as SocketGuildUser, user, null, reason)
            );
        }

        [Command("unverify")]
        [RequireAdmin]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task UnverifyAsync([RequireBotHierarchy("unverify")] [RequireInvokerHierarchy("unverify")] string user, [Remainder] string reason = null)
        {
            SocketGuildUser u;
            if (ulong.TryParse(user, out ulong userID) && (u = Context.Guild.GetUser(userID)) != null)
            {
                await UnverifyAsync(u, reason);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given user does not exist.");
        }
    }
}
