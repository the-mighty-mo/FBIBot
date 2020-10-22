using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Config
{
    public class AntiInvite : ModuleBase<SocketCommandContext>
    {
        [Command("anti-invite")]
        [Alias("antiinvite")]
        [RequireAdmin]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task AntiInviteAsync(string enable)
        {
            bool isEnable = enable == "true" || enable == "enable";
            bool isEnabled = await configDatabase.AntiInvite.GetAntiInviteAsync(Context.Guild);

            if (isEnable == isEnabled)
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that anti-invite is already {(isEnabled ? "enabled" : "disabled")}.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"We are now {(isEnable ? "permitted to remove" : "prohibited from removing")} invitations to the socialist party.");

            List<Task> cmds = new List<Task>()
            {
                Context.Channel.SendMessageAsync(embed: embed.Build())
            };
            if (isEnable)
            {
                cmds.Add(configDatabase.AntiInvite.SetAntiInviteAsync(Context.Guild));
            }
            else
            {
                cmds.Add(configDatabase.AntiInvite.RemoveAntiInviteAsync(Context.Guild));
            }

            await Task.WhenAll(cmds);
        }
    }
}
