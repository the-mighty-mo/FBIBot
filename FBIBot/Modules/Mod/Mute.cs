using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class Mute : ModuleBase<SocketCommandContext>
    {
        [Command("mute")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task MuteAsync(SocketGuildUser user, string timeout = null, [Remainder] string reason = null)
        {
            SocketGuildUser u = Context.Guild.GetUser(Context.User.Id);
            if (!await VerifyUser.IsMod(u))
            {
                await Context.Channel.SendMessageAsync("You are not a local director of the FBI and cannot use this command.");
                return;
            }

            SocketRole role = await Config.SetMute.GetMuteRole(Context.Guild) ?? await CreateMuteRoleAsync();

            if (user.Roles.Contains(role))
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that {user.Nickname ?? user.Username} is already under house arrest.");
            }

            List<SocketRole> roles = user.Roles.ToList();
            roles.Remove(Context.Guild.EveryoneRole);

            bool modifyRoles = await Config.ModifyMutedRoles.GetModifyMutedAsync(Context.Guild);
            if (modifyRoles)
            {
                await SaveUserRolesAsync(roles, user);
                try
                {
                    await user.RemoveRolesAsync(roles);
                }
                catch
                {
                    await Context.Channel.SendMessageAsync("We cannot mute members with higher authority than ourselves.");
                    await user.AddRolesAsync(roles);
                    await Unmute.RemoveUserRolesAsync(user);
                    return;
                }
            }
            await user.AddRoleAsync(role);

            bool isTimeout = double.TryParse(timeout, out double minutes);

            await Context.Channel.SendMessageAsync($"{user.Mention} has been placed under house arrest{(timeout != null && isTimeout ? $" for {timeout} {(minutes == 1 ? "minute" : "minutes")}" : "")}." +
                $"{(reason != null ? $"\nThe reason: {reason}" : "")}");
            await SendToModLog.SendToModLogAsync(SendToModLog.LogType.Mute, Context.User, user, isTimeout ? timeout : null, reason);

            if (timeout != null && isTimeout)
            {
                await Task.Delay((int)(minutes * 60 * 1000));

                if (!user.Roles.Contains(role))
                {
                    return;
                }

                if (modifyRoles)
                {
                    await user.AddRolesAsync(roles);
                }
                await user.RemoveRoleAsync(role);
                await Unmute.RemoveUserRolesAsync(user);

                await SendToModLog.SendToModLogAsync(SendToModLog.LogType.Unmute, Context.Client.CurrentUser, user);
            }
        }

        [Command("mute")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task MuteAsync(string user, string timeout = "")
        {
            SocketGuildUser u;
            if (ulong.TryParse(user, out ulong userID) && (u = Context.Guild.GetUser(userID)) != null)
            {
                await MuteAsync(u, timeout);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given user does not exist.");
        }

        async Task<SocketRole> CreateMuteRoleAsync()
        {
            SocketRole role;
            GuildPermissions perms = new GuildPermissions(viewChannel: true, sendMessages: false, addReactions: false, connect: true, speak: false);
            Color color = new Color(54, 57, 63);
            ulong roleID = (await Context.Guild.CreateRoleAsync("Muted", perms, color)).Id;
            role = Context.Guild.GetRole(roleID);

            await Config.SetMute.SetMuteRoleAsync(role, Context.Guild);
            return await Task.Run(() => role);
        }

        public static async Task SaveUserRolesAsync(List<SocketRole> roles, SocketGuildUser user)
        {
            List<Task> cmds = new List<Task>();
            string insert = "INSERT INTO UserRoles (guild_id, user_id, role_id) SELECT @guild_id, @user_id, @role_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM UserRoles WHERE guild_id = @guild_id AND user_id = @user_id AND role_id = @role_id);";

            foreach (SocketRole role in roles)
            {
                using (SqliteCommand cmd = new SqliteCommand(insert, Program.cnModRoles))
                {
                    cmd.Parameters.AddWithValue("@guild_id", user.Guild.Id.ToString());
                    cmd.Parameters.AddWithValue("@user_id", user.Id.ToString());
                    cmd.Parameters.AddWithValue("@role_id", role.Id.ToString());
                    cmds.Add(cmd.ExecuteNonQueryAsync());
                }
            }

            await Task.WhenAll(cmds);
        }
    }
}
