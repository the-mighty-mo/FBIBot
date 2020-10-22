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
    public class Unmute : ModuleBase<SocketCommandContext>
    {
        [Command("unmute")]
        [RequireMod]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task UnmuteAsync([RequireInvokerHierarchy("unmute")] SocketGuildUser user)
        {
            SocketRole role = await modRolesDatabase.Muted.GetMuteRole(Context.Guild);
            List<SocketRole> roles = await modRolesDatabase.UserRoles.GetUserRolesAsync(user);
            if ((role == null || !user.Roles.Contains(role)) && roles.Count == 0)
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that {user.Nickname ?? user.Username} is not muted.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(12, 156, 24))
                .WithDescription($"{user.Mention} has been freed from house arrest after a good amount of ~~brainwashing~~ self-reflection.");

            List<Task> cmds = new List<Task>()
            {
                Context.Channel.SendMessageAsync(embed: embed.Build()),
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

        [Command("unmute")]
        [RequireMod]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task UnmuteAsync(string user)
        {
            SocketGuildUser u;
            if (ulong.TryParse(user, out ulong userID) && (u = Context.Guild.GetUser(userID)) != null)
            {
                await UnmuteAsync(u);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given user does not exist.");
        }
    }
}
