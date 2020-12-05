using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Config
{
    public class AutoSurveillance : ModuleBase<SocketCommandContext>
    {
        [Command("auto-surveillance")]
        [Alias("autosurveillance")]
        [RequireAdmin]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task AutoSurveillanceAsync(string enable)
        {
            bool isEnable = enable == "true" || enable == "enable";
            bool isEnabled = await configDatabase.AutoSurveillance.GetAutoSurveillanceAsync(Context.Guild);

            if (isEnable == isEnabled)
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that Auto Surveillance is already {(isEnabled ? "enabled" : "disabled")}.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"We are now {(isEnable ? "permitted to perform" : "prohibited from performing")} surveillance on server members.");

            List<Task> cmds = new List<Task>()
            {
                Context.Channel.SendMessageAsync(embed: embed.Build())
            };
            if (isEnable)
            {
                cmds.Add(configDatabase.AutoSurveillance.SetAutoSurveillanceAsync(Context.Guild));
            }
            else
            {
                cmds.Add(configDatabase.AutoSurveillance.RemoveAutoSurveillanceAsync(Context.Guild));
            }

            await Task.WhenAll(cmds);
        }
    }
}