using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Config
{
    public class AntiZalgo : ModuleBase<SocketCommandContext>
    {
        [Command("anti-zalgo")]
        [Alias("antizalgo")]
        [RequireAdmin]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task AntiZalgoAsync(string enable)
        {
            bool isEnable = enable == "true" || enable == "enable";
            bool isEnabled = await configDatabase.AntiZalgo.GetAntiZalgoAsync(Context.Guild);

            if (isEnable == isEnabled)
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that anti-zalgo is already {(isEnabled ? "enabled" : "disabled")}.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"We are now {(isEnable ? "permitted to remove" : "prohibited from removing")} messages leaked from Area 51.");

            List<Task> cmds = new List<Task>()
            {
                Context.Channel.SendMessageAsync(embed: embed.Build())
            };
            if (isEnable)
            {
                cmds.Add(configDatabase.AntiZalgo.SetAntiZalgoAsync(Context.Guild));
            }
            else
            {
                cmds.Add(configDatabase.AntiZalgo.RemoveAntiZalgoAsync(Context.Guild));
            }

            await Task.WhenAll(cmds);
        }
    }
}
