using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FBIBot.Modules.Mod.ModLog;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

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
            SocketRole role = await verificationDatabase.Roles.GetVerificationRoleAsync(Context.Guild);
            if (role == null)
            {
                await Context.Channel.SendMessageAsync("Our intelligence team has informed us that there is no role to give to verified citizens.");
                return;
            }

            EmbedBuilder embed1 = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription("Please give us a minute or two to give out citizenship documents...");

            ulong id = await modLogsDatabase.ModLogs.GetNextModLogID(Context.Guild);
            await Task.WhenAll
            (
                Context.Channel.SendMessageAsync(embed: embed1.Build()),
                VerifyAllModLog.SendToModLogAsync(Context.User as SocketGuildUser)
            );

            foreach (SocketGuildUser user in Context.Guild.Users)
            {
                if (!user.IsBot)
                {
                    await user.AddRoleAsync(role);
                    if (await verificationDatabase.Roles.GetVerificationRoleAsync(Context.Guild) != role)
                    {
                        EmbedBuilder emb = new EmbedBuilder()
                            .WithColor(new Color(206, 15, 65))
                            .WithTitle("Federal Bureau of Investigation")
                            .WithDescription("The citizenship process has been canceled due to a change in the verification role.");

                        await Task.WhenAll
                        (
                            user.RemoveRoleAsync(role),
                            Context.Channel.SendMessageAsync(embed: emb.Build()),
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
                Context.Channel.SendMessageAsync(embed: embed.Build()),
                VerifyAllModLog.SetStateAsync(Context.Guild, id, "Completed")
            );
        }
    }
}