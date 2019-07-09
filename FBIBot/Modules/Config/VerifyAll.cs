using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace FBIBot.Modules.Config
{
    public class VerifyAll : ModuleBase<SocketCommandContext>
    {
        [Command("verifyall")]
        [Alias("verify-all")]
        [RequireAdmin]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task VerifyAllAsync()
        {
            SocketRole role = await SetVerify.GetVerificationRoleAsync(Context.Guild);
            if (role == null)
            {
                await Context.Channel.SendMessageAsync("Our intelligence team has informed us that there is no role to give to verified members.");
                return;
            }

            foreach (SocketGuildUser user in Context.Guild.Users)
            {
                if (!user.IsBot)
                {
                    await user.AddRoleAsync(role);
                }
            }

            await Task.WhenAll
            (
                Context.Channel.SendMessageAsync("All current members have been granted citizenship."),
                Mod.SendToModLog.SendToModLogAsync(Mod.SendToModLog.LogType.VerifyAll, Context.User as SocketGuildUser, null)
            );
        }
    }
}
