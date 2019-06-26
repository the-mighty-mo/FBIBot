using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class Mute : ModuleBase<SocketCommandContext>
    {
        [Command("mute")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task MuteAsync(SocketGuildUser user, string timeout = "")
        {
            SocketRole role = await Config.SetMute.GetMuteRole(Context.Guild);
            if (role == null)
            {
                role = await CreateMuteRoleAsync();
                await Config.SetMute.SetMuteRoleAsync(role, Context.Guild);
            }
            else if (user.Roles.Contains(role))
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that {user.Nickname ?? user.Username} is already muted.");
            }

            List<SocketRole> roles = user.Roles.ToList();
            roles.Remove(Context.Guild.EveryoneRole);
            await SaveUserRolesAsync(roles, user);

            bool modifyRoles = await Config.ModifyMutedRoles.GetModifyMutedAsync(Context.Guild);

            if (modifyRoles)
            {
                await user.RemoveRolesAsync(roles);
            }
            await user.AddRoleAsync(role);

            await Context.Channel.SendMessageAsync($"{user.Mention} has been placed under house arrest{(timeout.Length > 0 ? $" for {timeout} minutes" : "")}.");

            if (timeout.Length > 0 && double.TryParse(timeout, out double minutes))
            {
                await Task.Delay((int)(minutes * 60 * 1000));

                if (modifyRoles)
                {
                    await user.AddRolesAsync(roles);
                }
                await user.RemoveRoleAsync(role);
                await Unmute.RemoveUserRolesAsync(user);

                await Context.Channel.SendMessageAsync($"{user.Mention} has been freed from house arrest after a good amount of ~~brainwashing~~ self-reflection.");
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
            GuildPermissions perms = new GuildPermissions(sendMessages: false, addReactions: false, speak: false);
            Color color = new Color(54, 57, 63);
            ulong roleID = (await Context.Guild.CreateRoleAsync("Muted", perms, color)).Id;
            role = Context.Guild.GetRole(roleID);

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
