using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Config
{
    public class AntiSpam : ModuleBase<SocketCommandContext>
    {
        [Command("anti-spam")]
        [Alias("antispam")]
        [RequireAdmin]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task AntiSpamAsync(string enable)
        {
            bool isEnable = enable is "true" or "enable";
            bool isEnabled = await configDatabase.AntiSpam.GetAntiSpamAsync(Context.Guild);

            if (isEnable == isEnabled)
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that anti-spam is already {(isEnabled ? "enabled" : "disabled")}.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"We are now {(isEnable ? "permitted to remove" : "prohibited from removing")} spammy, anti-American messages.");

            List<Task> cmds = new()
            {
                Context.Channel.SendMessageAsync(embed: embed.Build())
            };
            if (isEnable)
            {
                cmds.Add(configDatabase.AntiSpam.SetAntiSpamAsync(Context.Guild));
            }
            else
            {
                cmds.Add(configDatabase.AntiSpam.RemoveAntiSpamAsync(Context.Guild));
            }

            await Task.WhenAll(cmds);
        }
    }
}