using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using FBIBot.Modules.Mod.ModLog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Mod
{
    public class Mute : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("mute", "Puts the user under house arrest so they can't type or speak in chats")]
        [RequireMod]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public Task MuteAsync([RequireBotHierarchy("mute")][RequireInvokerHierarchy("mute")] SocketUser user, [Summary(description: "Timeout in minutes. Default: no timeout")] double? timeout = null, string? reason = null) =>
            MuteAsync((user as SocketGuildUser)!, timeout, reason);

        private async Task MuteAsync(SocketGuildUser user, double? timeout, string? reason)
        {
            IRole role = await modRolesDatabase.Muted.GetMuteRole(Context.Guild) ?? await CreateMuteRoleAsync();
            if (user.Roles.Contains(role))
            {
                await Context.Interaction.RespondAsync($"Our security team has informed us that {user.Nickname ?? user.Username} is already under house arrest.");
                return;
            }

            List<SocketRole> roles = user.Roles.ToList();
            roles.Remove(Context.Guild.EveryoneRole);
            roles.RemoveAll(x => x.IsManaged);

            List<Task> cmds = new();
            bool modifyRoles = await configDatabase.ModifyMuted.GetModifyMutedAsync(Context.Guild);
            if (modifyRoles)
            {
                cmds.AddRange(new List<Task>()
                {
                    modRolesDatabase.UserRoles.SaveUserRolesAsync(roles, user),
                    user.RemoveRolesAsync(roles)
                });
            }
            cmds.Add(user.AddRoleAsync(role));

            await Task.WhenAll(cmds);

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(255, 110, 24))
                .WithDescription($"{user.Mention} has been placed under house arrest{(timeout != null ? $" for {timeout} {(timeout == 1 ? "minute" : "minutes")}" : "")}.");

            EmbedFieldBuilder reasonField = new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName("Reason")
                    .WithValue($"{reason ?? "[none given]"}");
            embed.AddField(reasonField);

            await Task.WhenAll
            (
                Context.Interaction.RespondAsync(embed: embed.Build()),
                MuteModLog.SendToModLogAsync((Context.User as SocketGuildUser)!, user, timeout, reason)
            );

            if (timeout != null)
            {
                await Task.Delay((int)(timeout * 60 * 1000));

                if (!user.Roles.Contains(role))
                {
                    return;
                }

                cmds = new List<Task>()
                {
                    user.RemoveRoleAsync(role),
                    modRolesDatabase.UserRoles.RemoveUserRolesAsync(user),
                    UnmuteModLog.SendToModLogAsync(Context.Guild.CurrentUser, user)
                };
                if (modifyRoles)
                {
                    cmds.Add(user.AddRolesAsync(roles));
                }

                await Task.WhenAll(cmds);
            }
        }

        private async Task<IRole> CreateMuteRoleAsync()
        {
            GuildPermissions perms = new(viewChannel: true, sendMessages: false, addReactions: false, connect: true, speak: false);
            Color color = new(54, 57, 63);
            RestRole role = await Context.Guild.CreateRoleAsync("Muted", perms, color, false, false);

            await modRolesDatabase.Muted.SetMuteRoleAsync(role, Context.Guild);
            return role;
        }
    }
}