﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FBIBot.Modules.Mod.ModLog;
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

            EmbedBuilder embed1 = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription("Please give us a minute or two to give out citizenship documents...");

            ulong id = await ModLogBase.GetNextModLogID(Context.Guild);
            await Task.WhenAll
            (
                Context.Channel.SendMessageAsync("", false, embed1.Build()),
                VerifyAllModLog.SendToModLogAsync(Context.User as SocketGuildUser)
            );

            foreach (SocketGuildUser user in Context.Guild.Users)
            {
                if (!user.IsBot)
                {
                    await user.AddRoleAsync(role);
                    if (await SetVerify.GetVerificationRoleAsync(Context.Guild) != role)
                    {
                        EmbedBuilder embed2 = new EmbedBuilder()
                            .WithColor(new Color(206, 15, 65))
                            .WithTitle("Federal Bureau of Investigation")
                            .WithDescription("The citizenship process has been canceled due to a change in the verification role.");

                        await Task.WhenAll
                        (
                            user.RemoveRoleAsync(role),
                            Context.Channel.SendMessageAsync("", false, embed2.Build()),
                            VerifyAllModLog.SetStateAsync(Context.Guild, id, "Canceled (change in verification role)")
                        );
                        return;
                    }
                }
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription("All current members have been granted citizenship.");

            await Task.WhenAll
            (
                Context.Channel.SendMessageAsync("", false, embed.Build()),
                VerifyAllModLog.SetStateAsync(Context.Guild, id, "Completed")
            );
        }
    }
}
