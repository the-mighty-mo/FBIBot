using Discord;
using Discord.Interactions;
using FBIBot.ParamEnums;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Config
{
    public class ModifyMutedRoles : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("modify-muted-roles", "Allows the bot to remove and save the roles of muted members")]
        [RequireAdmin]
        public async Task ModifyMutedRolesAsync(EnableChoice modify)
        {
            bool isModify = modify == EnableChoice.Enable;
            bool isModifying = await configDatabase.ModifyMuted.GetModifyMutedAsync(Context.Guild);
            string state = isModify ? "permitted to modify" : "prohibited from modifying";

            if (isModify == isModifying)
            {
                await Context.Interaction.RespondAsync($"Our security team has informed us that we are already {state} muted member's roles.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"We are now {state} muted member's roles.");

            List<Task> cmds = new()
            {
                Context.Interaction.RespondAsync(embed: embed.Build())
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