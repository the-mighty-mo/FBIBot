using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
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
                await Context.Channel.SendMessageAsync("Our intelligence team has informed us that there is no role to give to verified citizens.");
                return;
            }

            ulong id = await Mod.SendToModLog.GetNextModLogID(Context.Guild);
            await Task.WhenAll
            (
                Context.Channel.SendMessageAsync("Please give us a minute or two to give out citizenship documents..."),
                Mod.SendToModLog.SendToModLogAsync(Mod.SendToModLog.LogType.VerifyAll, Context.User as SocketGuildUser, null)
            );

            foreach (SocketGuildUser user in Context.Guild.Users)
            {
                if (!user.IsBot)
                {
                    await user.AddRoleAsync(role);
                    if (await SetVerify.GetVerificationRoleAsync(Context.Guild) != role)
                    {
                        await Task.WhenAll
                        (
                            user.RemoveRoleAsync(role),
                            Context.Channel.SendMessageAsync("The citizenship process has been canceled due to a change in the verification role."),
                            Mod.SendToModLog.SetStateAsync(Context.Guild, id, "Canceled (change in verification role)", Mod.SendToModLog.LogType.VerifyAll)
                        );
                        return;
                    }
                }
            }

            await Task.WhenAll
            (
                Context.Channel.SendMessageAsync("All current members have been granted citizenship."),
                Mod.SendToModLog.SetStateAsync(Context.Guild, id, "Completed", Mod.SendToModLog.LogType.VerifyAll)
            );
        }
    }
}
