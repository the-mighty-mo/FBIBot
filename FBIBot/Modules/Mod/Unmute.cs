using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FBIBot.Modules.Mod.ModLog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Mod
{
    public class Unmute : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("unmute", "Frees the house-arrested user")]
        [RequireMod]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public Task UnmuteAsync([RequireInvokerHierarchy("unmute")] SocketUser user) =>
            UnmuteAsync(user as SocketGuildUser);

        private async Task UnmuteAsync(SocketGuildUser user)
        {
            SocketRole role = await modRolesDatabase.Muted.GetMuteRole(Context.Guild);
            List<SocketRole> roles = await modRolesDatabase.UserRoles.GetUserRolesAsync(user);
            if ((role == null || !user.Roles.Contains(role)) && roles.Count == 0)
            {
                await Context.Interaction.RespondAsync($"Our security team has informed us that {user.Nickname ?? user.Username} is not muted.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(12, 156, 24))
                .WithDescription($"{user.Mention} has been freed from house arrest after a good amount of ~~brainwashing~~ self-reflection.");

            List<Task> cmds = new()
            {
                Context.Interaction.RespondAsync(embed: embed.Build()),
                UnmuteModLog.SendToModLogAsync(Context.User as SocketGuildUser, user)
            };
            if (roles.Count > 0)
            {
                cmds.AddRange(new List<Task>()
                {
                    user.AddRolesAsync(roles),
                    modRolesDatabase.UserRoles.RemoveUserRolesAsync(user)
                });
            }
            if (role != null)
            {
                cmds.Add(user.RemoveRoleAsync(role));
            }

            await Task.WhenAll(cmds);
        }
    }
}