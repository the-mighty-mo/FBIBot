using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FBIBot.Modules.AutoMod;
using FBIBot.Modules.Mod;

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

            await user.RemoveRoleAsync(role);
            await Context.Channel.SendMessageAsync($"We have put the potential communist {user.Mention} under quarantine.");
            await SendToModLog.SendToModLogAsync(SendToModLog.LogType.Unverify, Context.User as SocketGuildUser, user, null, reason);
            await Verify.RemoveVerifiedAsync(user);
            await Verify.SendCaptchaAsync(user);
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
