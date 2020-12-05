using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Config
{
    public class ModifyMutedRoles : ModuleBase<SocketCommandContext>
    {
        [Command("modify-muted-roles")]
        [RequireAdmin]
        public async Task ModifyMutedRolesAsync(string modify)
        {
            bool isModify = modify.ToLower() == "true" || modify.ToLower() == "enable";
            bool isModifying = await configDatabase.ModifyMuted.GetModifyMutedAsync(Context.Guild);
            string state = isModify ? "permitted to modify" : "prohibited from modifying";

            if (isModify == isModifying)
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that we are already {state} muted member's roles.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"We are now {state} muted member's roles.");

            List<Task> cmds = new List<Task>()
            {
                Context.Channel.SendMessageAsync(embed: embed.Build())
            };
            if (isModify)
            {
                cmds.Add(configDatabase.ModifyMuted.AddModifyMutedAsync(Context.Guild));
            }
            else
            {
                cmds.Add(configDatabase.ModifyMuted.RemoveModifyMutedAsync(Context.Guild));
            }

            await Task.WhenAll(cmds);
        }
    }
}