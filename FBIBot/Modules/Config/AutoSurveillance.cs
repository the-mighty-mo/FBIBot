using Discord;
using Discord.Interactions;
using FBIBot.ParamEnums;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Config
{
    public class AutoSurveillance : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("auto-surveillance", "Permits the FBI to perform surveillance operations on server members")]
        [RequireAdmin]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task AutoSurveillanceAsync(EnableChoice state)
        {
            bool isEnable = state == EnableChoice.Enable;
            bool isEnabled = await configDatabase.AutoSurveillance.GetAutoSurveillanceAsync(Context.Guild).ConfigureAwait(false);

            if (isEnable == isEnabled)
            {
                await Context.Interaction.RespondAsync($"Our security team has informed us that Auto Surveillance is already {(isEnabled ? "enabled" : "disabled")}.").ConfigureAwait(false);
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"We are now {(isEnable ? "permitted to perform" : "prohibited from performing")} surveillance on server members.");

            List<Task> cmds = new()
            {
                Context.Interaction.RespondAsync(embed: embed.Build())
            };
            if (isEnable)
            {
                cmds.Add(configDatabase.AutoSurveillance.SetAutoSurveillanceAsync(Context.Guild));
            }
            else
            {
                cmds.Add(configDatabase.AutoSurveillance.RemoveAutoSurveillanceAsync(Context.Guild));
            }

            await Task.WhenAll(cmds).ConfigureAwait(false);
        }
    }
}