using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FBIBot.Modules.Mod.ModLog;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Config
{
    public class VerifyAll : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("verify-all", "Grants citizenship all current freedom-loving Americans")]
        [RequireAdmin]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task VerifyAllAsync()
        {
            SocketRole? role = await verificationDatabase.Roles.GetVerificationRoleAsync(Context.Guild).ConfigureAwait(false);
            if (role == null)
            {
                await Context.Interaction.RespondAsync("Our intelligence team has informed us that there is no role to give to verified citizens.").ConfigureAwait(false);
                return;
            }

            EmbedBuilder embed1 = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription("Please give us a minute or two to give out citizenship documents...");

            ulong id = await modLogsDatabase.ModLogs.GetNextModLogID(Context.Guild).ConfigureAwait(false);
            await Task.WhenAll
            (
                Context.Interaction.RespondAsync(embed: embed1.Build()),
                VerifyAllModLog.SendToModLogAsync((Context.User as SocketGuildUser)!)
            ).ConfigureAwait(false);

            foreach (SocketGuildUser user in Context.Guild.Users)
            {
                if (!user.IsBot)
                {
                    await user.AddRoleAsync(role).ConfigureAwait(false);
                    if (await verificationDatabase.Roles.GetVerificationRoleAsync(Context.Guild).ConfigureAwait(false) != role)
                    {
                        EmbedBuilder emb = new EmbedBuilder()
                            .WithColor(new Color(206, 15, 65))
                            .WithTitle("Federal Bureau of Investigation")
                            .WithDescription("The citizenship process has been canceled due to a change in the verification role.");

                        await Task.WhenAll
                        (
                            user.RemoveRoleAsync(role),
                            Context.Interaction.RespondAsync(embed: emb.Build()),
                            VerifyAllModLog.SetStateAsync(Context.Guild, id, "Canceled (change in verification role)")
                        ).ConfigureAwait(false);
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
                Context.Interaction.RespondAsync(embed: embed.Build()),
                VerifyAllModLog.SetStateAsync(Context.Guild, id, "Completed")
            ).ConfigureAwait(false);
        }
    }
}