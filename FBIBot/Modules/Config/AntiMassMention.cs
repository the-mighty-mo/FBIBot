using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Config
{
    public class AntiMassMention : ModuleBase<SocketCommandContext>
    {
        [Command("anti-massmention")]
        [Alias("antimassmention")]
        [RequireAdmin]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task AntiMassMentionAsync(string enable)
        {
            bool isEnable = enable == "true" || enable == "enable";
            bool isEnabled = await configDatabase.AntiMassMention.GetAntiMassMentionAsync(Context.Guild);

            if (isEnable == isEnabled)
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that anti-mass-mention is already {(isEnabled ? "enabled" : "disabled")}.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"We are now {(isEnable ? "permitted to remove" : "prohibited from removing")} messages mentioning the rich they want to eat.");

            List<Task> cmds = new List<Task>()
            {
                Context.Channel.SendMessageAsync(embed: embed.Build())
            };
            if (isEnable)
            {
                cmds.Add(configDatabase.AntiMassMention.SetAntiMassMentionAsync(Context.Guild));
            }
            else
            {
                cmds.Add(configDatabase.AntiMassMention.RemoveAntiMassMentionAsync(Context.Guild));
            }

            await Task.WhenAll(cmds);
        }
    }
}
