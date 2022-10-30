using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FBIBot.Modules.Mod.ModLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Mod
{
    public class Arrest : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("arrest", "Sends the user to Guantanamo Bay for a bit. *This command creates its own role and channel*")]
        [RequireMod]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public Task ArrestAsync([RequireBotHierarchy("arrest")][RequireInvokerHierarchy("arrest")] SocketUser user, [Summary(description: "Timeout in minutes. Default: no timeout")] double? timeout = null, string? reason = null) =>
            ArrestAsync((user as SocketGuildUser)!, timeout, reason);

        private async Task ArrestAsync(SocketGuildUser user, double? timeout, string? reason)
        {
            IRole role = await modRolesDatabase.PrisonerRole.GetPrisonerRoleAsync(Context.Guild).ConfigureAwait(false)
                ?? await CreatePrisonerRoleAsync().ConfigureAwait(false);
            if (user.Roles.Contains(role))
            {
                await Context.Interaction.RespondAsync($"Our security team has informed us that {user.Nickname ?? user.Username} is already held captive.").ConfigureAwait(false);
                return;
            }

            ITextChannel channel = await modRolesDatabase.PrisonerChannel.GetPrisonerChannelAsync(Context.Guild).ConfigureAwait(false)
                ?? await CreateGuantanamoAsync(role).ConfigureAwait(false);

            List<SocketRole> roles = user.Roles.ToList();
            roles.Remove(Context.Guild.EveryoneRole);
            roles.RemoveAll(x => x.IsManaged);

            await Task.WhenAll
            (
                modRolesDatabase.UserRoles.SaveUserRolesAsync(roles, user),
                user.RemoveRolesAsync(roles),
                user.AddRoleAsync(role),
                modRolesDatabase.Prisoners.RecordPrisonerAsync(user)
            ).ConfigureAwait(false);

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(255, 61, 24))
                .WithDescription($"{user.Mention} has been sent to Guantanamo Bay{(timeout != null ? $" for {timeout} {(timeout == 1 ? "minute" : "minutes")}" : "")}.");

            EmbedFieldBuilder reasonField = new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName("Reason")
                    .WithValue($"{reason ?? "*No reason necessary*"}");
            embed.AddField(reasonField);

            await Task.WhenAll
            (
                Context.Interaction.RespondAsync(embed: embed.Build()),
                ArrestModLog.SendToModLogAsync((Context.User as SocketGuildUser)!, user, timeout, reason)
            ).ConfigureAwait(false);

            if (timeout != null)
            {
                await Task.Delay((int)(timeout * 60 * 1000)).ConfigureAwait(false);
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
                ).ConfigureAwait(false);

                List<Task> cmds = !await modRolesDatabase.Prisoners.HasPrisoners(Context.Guild).ConfigureAwait(false)
                    ? new List<Task>()
                    {
                        channel?.DeleteAsync() ?? Task.CompletedTask,
                        role?.DeleteAsync() ?? Task.CompletedTask,
                        modRolesDatabase.PrisonerChannel.RemovePrisonerChannelAsync(Context.Guild),
                        modRolesDatabase.PrisonerRole.RemovePrisonerRoleAsync(Context.Guild)
                    }
                    : new List<Task>();
                cmds.Add(FreeModLog.SendToModLogAsync(Context.Guild.CurrentUser, user));

                await Task.WhenAll(cmds).ConfigureAwait(false);
            }
        }

        private async Task<IRole> CreatePrisonerRoleAsync()
        {
            IRole role;
            GuildPermissions perms = new(viewChannel: false);
            Color color = new(127, 127, 127);
            role = await Context.Guild.CreateRoleAsync("Prisoner", perms, color, false, true).ConfigureAwait(false);

            await modRolesDatabase.PrisonerRole.SetPrisonerRoleAsync(role).ConfigureAwait(false);
            return role;
        }

        private async Task<ITextChannel> CreateGuantanamoAsync(IRole role)
        {
            ITextChannel channel = await Context.Guild.CreateTextChannelAsync("guantanamo").ConfigureAwait(false);
            OverwritePermissions perms = new(viewChannel: PermValue.Allow, sendMessages: PermValue.Allow, readMessageHistory: PermValue.Deny, addReactions: PermValue.Allow);

            await Task.WhenAll
            (
                channel.AddPermissionOverwriteAsync(role, perms),
                channel.AddPermissionOverwriteAsync(Context.Client.CurrentUser, OverwritePermissions.AllowAll(channel))
            ).ConfigureAwait(false);
            await channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, OverwritePermissions.DenyAll(channel)).ConfigureAwait(false);
            await modRolesDatabase.PrisonerChannel.SetPrisonerChannelAsync(channel).ConfigureAwait(false);

            return channel;
        }
    }
}