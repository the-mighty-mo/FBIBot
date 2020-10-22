using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FBIBot.Modules.Mod.ModLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Mod
{
    public class Arrest : ModuleBase<SocketCommandContext>
    {
        [Command("arrest")]
        [RequireMod]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task ArrestAsync([RequireBotHierarchy("arrest")] [RequireInvokerHierarchy("arrest")] SocketGuildUser user, string timeout = null)
        {
            IRole role = await modRolesDatabase.PrisonerRole.GetPrisonerRoleAsync(Context.Guild) ?? await CreatePrisonerRoleAsync();
            if (user.Roles.Contains(role))
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that {user.Nickname ?? user.Username} is already held captive.");
                return;
            }

            ITextChannel channel = await modRolesDatabase.PrisonerChannel.GetPrisonerChannelAsync(Context.Guild) ?? await CreateGuantanamoAsync(role);

            List<SocketRole> roles = user.Roles.ToList();
            roles.Remove(Context.Guild.EveryoneRole);

            await Task.WhenAll
            (
                modRolesDatabase.UserRoles.SaveUserRolesAsync(roles, user),
                user.RemoveRolesAsync(roles),
                user.AddRoleAsync(role),
                modRolesDatabase.Prisoners.RecordPrisonerAsync(user)
            );

            bool isTimeout = double.TryParse(timeout, out double minutes);
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(255, 61, 24))
                .WithDescription($"{user.Mention} has been sent to Guantanamo Bay{(timeout != null && isTimeout ? $" for {timeout} {(minutes == 1 ? "minute" : "minutes")}" : "")}.");

            await Task.WhenAll
            (
                Context.Channel.SendMessageAsync(embed: embed.Build()),
                ArrestModLog.SendToModLogAsync(Context.User as SocketGuildUser, user, timeout)
            );

            if (isTimeout)
            {
                await Task.Delay((int)(minutes * 60 * 1000));
                role = Context.Guild.GetRole(role.Id);

                if (!user.Roles.Contains(role))
                {
                    return;
                }

                await Task.WhenAll
                (
                    user.AddRolesAsync(roles),
                    user.RemoveRoleAsync(role),
                    modRolesDatabase.UserRoles.RemoveUserRolesAsync(user),
                    modRolesDatabase.Prisoners.RemovePrisonerAsync(user)
                );

                List<Task> cmds = !await modRolesDatabase.Prisoners.HasPrisoners(Context.Guild)
                    ? new List<Task>()
                    {
                        channel?.DeleteAsync(),
                        role?.DeleteAsync(),
                        modRolesDatabase.PrisonerChannel.RemovePrisonerChannelAsync(Context.Guild),
                        modRolesDatabase.PrisonerRole.RemovePrisonerRoleAsync(Context.Guild)
                    }
                    : new List<Task>();
                cmds.Add(FreeModLog.SendToModLogAsync(Context.Guild.CurrentUser, user));

                await Task.WhenAll(cmds);
            }
        }
        
        [Command("arrest")]
        [RequireMod]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task ArrestAsync(string user, string timeout = null)
        {
            SocketGuildUser u;
            if (ulong.TryParse(user, out ulong userID) && (u = Context.Guild.GetUser(userID)) != null)
            {
                await ArrestAsync(u, timeout);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given user does not exist.");
        }
        
        async Task<IRole> CreatePrisonerRoleAsync()
        {
            IRole role;
            GuildPermissions perms = new GuildPermissions(viewChannel: false);
            Color color = new Color(127, 127, 127);
            role = await Context.Guild.CreateRoleAsync("Prisoner", perms, color, false, true);

            await modRolesDatabase.PrisonerRole.SetPrisonerRoleAsync(role);
            return role;
        }

        async Task<ITextChannel> CreateGuantanamoAsync(IRole role)
        {
            ITextChannel channel = await Context.Guild.CreateTextChannelAsync("guantanamo");
            OverwritePermissions perms = new OverwritePermissions(viewChannel: PermValue.Allow, sendMessages: PermValue.Allow, readMessageHistory: PermValue.Deny, addReactions: PermValue.Allow);

            await Task.WhenAll
            (
                channel.AddPermissionOverwriteAsync(role, perms),
                channel.AddPermissionOverwriteAsync(Context.Client.CurrentUser, OverwritePermissions.AllowAll(channel))
            );
            await channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, OverwritePermissions.DenyAll(channel));
            await modRolesDatabase.PrisonerChannel.SetPrisonerChannelAsync(channel);

            return channel;
        }
    }
}
