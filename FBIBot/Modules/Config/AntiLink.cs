using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Config
{
    public class AntiLink : ModuleBase<SocketCommandContext>
    {
        [Command("anti-link")]
        [Alias("antilink")]
        [RequireAdmin]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task AntiLinkAsync(string enable)
        {
            bool isEnable = enable == "true" || enable == "enable";
            bool isEnabled = await configDatabase.AntiLink.GetAntiLinkAsync(Context.Guild);

            if (isEnable == isEnabled)
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that anti-link is already {(isEnabled ? "enabled" : "disabled")}.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"We are now {(isEnable ? "permitted to remove" : "prohibited from removing")} messages linking external, communist propaganda.");

            List<Task> cmds = new List<Task>()
            {
                Context.Channel.SendMessageAsync("", false, embed.Build())
            };
            if (isEnable)
            {
                cmds.Add(configDatabase.AntiLink.SetAntiLinkAsync(Context.Guild));
            }
            else
            {
                cmds.Add(configDatabase.AntiLink.RemoveAntiLinkAsync(Context.Guild));
            }

            await Task.WhenAll(cmds);
        }
    }
}
