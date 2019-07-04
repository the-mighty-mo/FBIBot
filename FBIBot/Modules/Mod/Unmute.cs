using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class Unmute : ModuleBase<SocketCommandContext>
    {
        [Command("unmute")]
        [RequireMod]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task UnmuteAsync([RequireInvokerHierarchy("unmute")] SocketGuildUser user)
        {
            SocketRole role = await Config.SetMute.GetMuteRole(Context.Guild);
            List<SocketRole> roles = await GetUserRolesAsync(user);
            if ((role == null || !user.Roles.Contains(role)) && roles.Count == 0)
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that {user.Nickname ?? user.Username} is not muted.");
                return;
            }

            if (roles.Count > 0)
            {
                await user.AddRolesAsync(roles);
                await RemoveUserRolesAsync(user);
            }
            if (role != null)
            {
                await user.RemoveRoleAsync(role);
            }

            await Context.Channel.SendMessageAsync($"{user.Mention} has been freed from house arrest after a good amount of ~~brainwashing~~ self-reflection.");
            await SendToModLog.SendToModLogAsync(SendToModLog.LogType.Unmute, Context.User, user);
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

        public static async Task<List<SocketRole>> GetUserRolesAsync(SocketGuildUser user)
        {
            List<SocketRole> roles = new List<SocketRole>();

            string getRoles = "SELECT role_id FROM UserRoles WHERE guild_id = @guild_id AND user_id = @user_id;";
            using (SqliteCommand cmd = new SqliteCommand(getRoles, Program.cnModRoles))
            {
                cmd.Parameters.AddWithValue("@guild_id", user.Guild.Id.ToString());
                cmd.Parameters.AddWithValue("@user_id", user.Id.ToString());

                SqliteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    ulong.TryParse(reader["role_id"].ToString(), out ulong roleID);
                    SocketRole role = user.Guild.GetRole(roleID);
                    if (role != null)
                    {
                        roles.Add(role);
                    }
                }
                reader.Close();
            }

            return await Task.Run(() => roles);
        }

        public static async Task RemoveUserRolesAsync(SocketGuildUser user)
        {
            string delete = "DELETE FROM UserRoles WHERE guild_id = @guild_id AND user_id = @user_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, Program.cnModRoles))
            {
                cmd.Parameters.AddWithValue("@guild_id", user.Guild.Id.ToString());
                cmd.Parameters.AddWithValue("@user_id", user.Id.ToString());

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
