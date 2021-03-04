using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Config
{
    public class SetVerify : ModuleBase<SocketCommandContext>
    {
        [Command("setverify")]
        [Alias("set-verify")]
        [RequireAdmin]
        public async Task SetVerifyAsync()
        {
            if (await verificationDatabase.Roles.GetVerificationRoleAsync(Context.Guild) == null)
            {
                await Context.Channel.SendMessageAsync("Our customs team has informed us that you already don't have a citizenship check.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription("Citizenship will now go unchecked. This could go very poorly.");

            await Task.WhenAll
            (
                verificationDatabase.Roles.RemoveVerificationRoleAsync(Context.Guild),
                Context.Channel.SendMessageAsync(embed: embed.Build())
            );
        }

        [Command("setverify")]
        [Alias("set-verify")]
        [RequireAdmin]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task SetVerifyAsync(SocketRole role, string changeRole = "false")
        {
            SocketRole currentRole = await verificationDatabase.Roles.GetVerificationRoleAsync(Context.Guild);
            if (currentRole == role)
            {
                await Context.Channel.SendMessageAsync($"Our customs team has informed us that all patriotic citizens already receive the {role.Mention} role.");
                return;
            }

            if (role.Position >= Context.Guild.CurrentUser.Hierarchy)
            {
                await Context.Channel.SendMessageAsync("We cannot give members a role with equal or higher authority than our highest-ranking role.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"All proud Americans will now receive the {role.Mention} role.");

            await Task.WhenAll
            (
                verificationDatabase.Roles.SetVerificationRoleAsync(role),
                Context.Channel.SendMessageAsync(embed: embed.Build())
            );

            if (changeRole.Equals("true"))
            {
                await ManageRolesAsync(role, currentRole);
            }
        }

        [Command("setverify")]
        [Alias("set-verify")]
        [RequireAdmin]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task SetVerifyAsync(string role, string changeRole = "false")
        {
            SocketRole r;
            if (ulong.TryParse(role, out ulong roleID) && (r = Context.Guild.GetRole(roleID)) != null)
            {
                await SetVerifyAsync(r, changeRole);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence tells us the given role does not exist.");
        }

        private async Task ManageRolesAsync(SocketRole role, SocketRole oldRole)
        {
            SocketRole newRole;
            foreach (SocketGuildUser user in Context.Guild.Users)
            {
                if (await verificationDatabase.Verified.GetVerifiedAsync(user) || (oldRole != null && user.Roles.Contains(oldRole)))
                {
                    await user.AddRoleAsync(role);
                    // If the verification role was changed while adding the role, return
                    if (await verificationDatabase.Roles.GetVerificationRoleAsync(Context.Guild) != role)
                    {
                        await user.RemoveRoleAsync(role);
                        return;
                    }

                    if (oldRole != null)
                    {
                        await user.RemoveRoleAsync(oldRole);
                        // If the verification role was changed while removing the old role, and the new role is the old role, return
                        if ((newRole = await verificationDatabase.Roles.GetVerificationRoleAsync(Context.Guild)) != role && newRole == oldRole)
                        {
                            await user.AddRoleAsync(oldRole);
                            return;
                        }
                    }
                }
            }
        }
    }
}