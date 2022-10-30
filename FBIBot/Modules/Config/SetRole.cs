using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FBIBot.ParamEnums;
using System.Linq;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Config
{
    [Group("set-role", "Sets a role for bot actions")]
    [RequireAdmin]
    public class SetRole : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("mute", "Sets the role for members under house arrest (muted). *Unsets if no role is given*")]
        public Task SetMuteAsync(SocketRole? role = null) =>
            role == null ? SetMutePrivAsync() : SetMutePrivAsync(role);

        private async Task SetMutePrivAsync()
        {
            if (modRolesDatabase.Muted.GetMuteRole(Context.Guild) == null)
            {
                await Context.Interaction.RespondAsync("Our intelligence team has informed us that you already lack a muted role.").ConfigureAwait(false);
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription("You no longer have a muted role.\n" +
                    "~~Guess we'll take things into our own hands~~");

            await Task.WhenAll
            (
                modRolesDatabase.Muted.RemoveMuteRoleAsync(Context.Guild),
                Context.Interaction.RespondAsync(embed: embed.Build())
            ).ConfigureAwait(false);
        }

        private async Task SetMutePrivAsync(SocketRole role)
        {
            if (await modRolesDatabase.Muted.GetMuteRole(Context.Guild).ConfigureAwait(false) == role)
            {
                await Context.Interaction.RespondAsync($"All who commit treason already receive the {role.Mention} role.").ConfigureAwait(false);
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"All who commit treason will now receive the {role.Mention} role.");

            await Task.WhenAll
            (
                modRolesDatabase.Muted.SetMuteRoleAsync(role, Context.Guild),
                Context.Interaction.RespondAsync(embed: embed.Build())
            ).ConfigureAwait(false);
        }

        [SlashCommand("verify", "Sets the verification role. *Unsets if no role is given*")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public Task SetVerifyAsync(SocketRole? role = null, [Summary(description: "Whether to give out the new role and remove any old Verification role. Default: False")] BoolChoice changeRole = BoolChoice.False) =>
            role == null ? SetVerifyPrivAsync() : SetVerifyPrivAsync(role, changeRole);

        private async Task SetVerifyPrivAsync()
        {
            if (await verificationDatabase.Roles.GetVerificationRoleAsync(Context.Guild).ConfigureAwait(false) == null)
            {
                await Context.Interaction.RespondAsync("Our customs team has informed us that you already don't have a citizenship check.").ConfigureAwait(false);
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription("Citizenship will now go unchecked. This could go very poorly.");

            await Task.WhenAll
            (
                verificationDatabase.Roles.RemoveVerificationRoleAsync(Context.Guild),
                Context.Interaction.RespondAsync(embed: embed.Build())
            ).ConfigureAwait(false);
        }

        private async Task SetVerifyPrivAsync(SocketRole role, BoolChoice changeRole)
        {
            SocketRole? currentRole = await verificationDatabase.Roles.GetVerificationRoleAsync(Context.Guild).ConfigureAwait(false);
            if (currentRole == role)
            {
                await Context.Interaction.RespondAsync($"Our customs team has informed us that all patriotic citizens already receive the {role.Mention} role.").ConfigureAwait(false);
                return;
            }

            if (role.Position >= Context.Guild.CurrentUser.Hierarchy)
            {
                await Context.Interaction.RespondAsync("We cannot give members a role with equal or higher authority than our highest-ranking role.").ConfigureAwait(false);
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"All proud Americans will now receive the {role.Mention} role.");

            await Task.WhenAll
            (
                verificationDatabase.Roles.SetVerificationRoleAsync(role),
                Context.Interaction.RespondAsync(embed: embed.Build())
            ).ConfigureAwait(false);

            if (changeRole == BoolChoice.True)
            {
                await ManageRolesAsync(role, currentRole).ConfigureAwait(false);
            }
        }

        private async Task ManageRolesAsync(SocketRole role, SocketRole? oldRole)
        {
            foreach (SocketGuildUser user in Context.Guild.Users)
            {
                if (await verificationDatabase.Verified.GetVerifiedAsync(user).ConfigureAwait(false) || (oldRole != null && user.Roles.Contains(oldRole)))
                {
                    await user.AddRoleAsync(role).ConfigureAwait(false);
                    // If the verification role was changed while adding the role, return
                    if (await verificationDatabase.Roles.GetVerificationRoleAsync(Context.Guild).ConfigureAwait(false) != role)
                    {
                        await user.RemoveRoleAsync(role).ConfigureAwait(false);
                        return;
                    }

                    if (oldRole != null)
                    {
                        await user.RemoveRoleAsync(oldRole).ConfigureAwait(false);
                        SocketRole? newRole;
                        // If the verification role was changed while removing the old role, and the new role is the old role, return
                        if ((newRole = await verificationDatabase.Roles.GetVerificationRoleAsync(Context.Guild).ConfigureAwait(false)) != role
                            && newRole == oldRole)
                        {
                            await user.AddRoleAsync(oldRole).ConfigureAwait(false);
                            return;
                        }
                    }
                }
            }
        }
    }
}