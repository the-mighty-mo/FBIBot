using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FBIBot.Modules.Mod.ModLog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Mod
{
    public class Mute : ModuleBase<SocketCommandContext>
    {
        [Command("mute")]
        [RequireMod]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task MuteAsync([RequireBotHierarchy("mute")][RequireInvokerHierarchy("mute")] SocketGuildUser user, [Remainder] string reason = null) => await TempMuteAsync(user, null, reason);

        [Command("mute")]
        [RequireMod]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task MuteAsync(string user, [Remainder] string reason = null) => await TempMuteAsync(user, null, reason);

        [Command("tempmute")]
        [Alias("temp-mute")]
        [RequireMod]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task TempMuteAsync([RequireBotHierarchy("mute")] [RequireInvokerHierarchy("mute")] SocketGuildUser user, string timeout = null, [Remainder] string reason = null)
        {
            IRole role = await modRolesDatabase.Muted.GetMuteRole(Context.Guild) ?? await CreateMuteRoleAsync();
            if (user.Roles.Contains(role))
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that {user.Nickname ?? user.Username} is already under house arrest.");
                return;
            }

            List<SocketRole> roles = user.Roles.ToList();
            roles.Remove(Context.Guild.EveryoneRole);
            roles.RemoveAll(x => x.IsManaged);

            List<Task> cmds = new List<Task>();
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

            bool isTimeout = double.TryParse(timeout, out double minutes);
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(255, 110, 24))
                .WithDescription($"{user.Mention} has been placed under house arrest{(timeout != null && isTimeout ? $" for {timeout} {(minutes == 1 ? "minute" : "minutes")}" : "")}.");

            EmbedFieldBuilder reasonField = new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName("Reason")
                    .WithValue($"{reason ?? "[none given]"}");
            embed.AddField(reasonField);

            await Task.WhenAll
            (
                Context.Channel.SendMessageAsync(embed: embed.Build()),
                MuteModLog.SendToModLogAsync(Context.User as SocketGuildUser, user, timeout, reason)
            );

            if (isTimeout)
            {
                await Task.Delay((int)(minutes * 60 * 1000));

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

        [Command("tempmute")]
        [Alias("temp-mute")]
        [RequireMod]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task TempMuteAsync(string user, string timeout = null, [Remainder] string reason = null)
        {
            SocketGuildUser u;
            if (ulong.TryParse(user, out ulong userID) && (u = Context.Guild.GetUser(userID)) != null)
            {
                await TempMuteAsync(u, timeout, reason);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given user does not exist.");
        }

        private async Task<IRole> CreateMuteRoleAsync()
        {
            GuildPermissions perms = new GuildPermissions(viewChannel: true, sendMessages: false, addReactions: false, connect: true, speak: false);
            Color color = new Color(54, 57, 63);
            var role = await Context.Guild.CreateRoleAsync("Muted", perms, color, false, false);

            await modRolesDatabase.Muted.SetMuteRoleAsync(role, Context.Guild);
            return role;
        }
    }
}