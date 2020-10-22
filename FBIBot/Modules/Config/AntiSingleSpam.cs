using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Config
{
    public class AntiSingleSpam : ModuleBase<SocketCommandContext>
    {
        [Command("anti-singlespam")]
        [Alias("antisinglespam")]
        [RequireAdmin]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task AntiSingleSpamAsync(string enable)
        {
            bool isEnable = enable == "true" || enable == "enable";
            bool isEnabled = await configDatabase.AntiSingleSpam.GetAntiSingleSpamAsync(Context.Guild);

            if (isEnable == isEnabled)
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that anti-single-spam is already {(isEnabled ? "enabled" : "disabled")}.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"We are now {(isEnable ? "permitted to remove" : "prohibited from removing")} *big*, spammy, anti-American messages.");

            List<Task> cmds = new List<Task>()
            {
                Context.Channel.SendMessageAsync(embed: embed.Build())
            };
            if (isEnable)
            {
                cmds.Add(configDatabase.AntiSingleSpam.SetAntiSingleSpamAsync(Context.Guild));
            }
            else
            {
                cmds.Add(configDatabase.AntiSingleSpam.RemoveAntiSingleSpamAsync(Context.Guild));
            }

            await Task.WhenAll(cmds);
        }
    }
}
