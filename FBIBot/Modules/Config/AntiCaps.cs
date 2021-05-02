using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Config
{
    public class AntiCaps : ModuleBase<SocketCommandContext>
    {
        [Command("anti-caps")]
        [Alias("anticaps")]
        [RequireAdmin]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task AntiCapsAsync(string enable)
        {
            bool isEnable = enable is "true" or "enable";
            bool isEnabled = await configDatabase.AntiCaps.GetAntiCapsAsync(Context.Guild);

            if (isEnable == isEnabled)
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that anti-caps is already {(isEnabled ? "enabled" : "disabled")}.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"We are now {(isEnable ? "permitted to remove" : "prohibited from removing")} REALLY LOUD PROTESTS.");

            List<Task> cmds = new()
            {
                Context.Channel.SendMessageAsync(embed: embed.Build())
            };
            if (isEnable)
            {
                cmds.Add(configDatabase.AntiCaps.SetAntiCapsAsync(Context.Guild));
            }
            else
            {
                cmds.Add(configDatabase.AntiCaps.RemoveAntiCapsAsync(Context.Guild));
            }

            await Task.WhenAll(cmds);
        }
    }
}