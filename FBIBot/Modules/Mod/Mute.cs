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
        public async Task MuteAsync(SocketGuildUser user)
        {
            SocketRole role = await GetMuteRole();
            if (role == null)
            {
                role = await CreateMuteRoleAsync();
                await Config.SetMute.SetMuteRoleAsync(role, Context.Guild);
            }

            List<SocketRole> roles = user.Roles.ToList();
            roles.Remove(Context.Guild.EveryoneRole);
            await SaveUserRolesAsync(roles);

            await user.RemoveRolesAsync(roles);
            await user.AddRoleAsync(role);

            await Context.Channel.SendMessageAsync($"{user.Nickname ?? user.Username} has been muted.");
        }

        [Command("mute")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task MuteAsync(string user)
        {
            SocketGuildUser u;
            if (ulong.TryParse(user, out ulong userID) && (u = Context.Guild.GetUser(userID)) != null)
            {
                await MuteAsync(u);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence tells us the given user does not exist.");
        }

        async Task<SocketRole> GetMuteRole()
        {
            SocketRole role = null;

            string getRole = "SELECT role_id FROM Muted WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getRole, Program.cnModRoles))
            {
                cmd.Parameters.AddWithValue("@guild_id", Context.Guild.Id);

                SqliteDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    ulong roleID = ulong.Parse(reader["role_id"].ToString());
                    role = Context.Guild.GetRole(roleID);
                }
            }

            return await Task.Run(() => role);
        }

        async Task<SocketRole> CreateMuteRoleAsync()
        {
            SocketRole role;
            GuildPermissions perms = new GuildPermissions(sendMessages: false);
            ulong roleID = (await Context.Guild.CreateRoleAsync("Muted", perms)).Id;
            role = Context.Guild.GetRole(roleID);

            return await Task.Run(() => role);
        }

        async Task SaveUserRolesAsync(List<SocketRole> roles)
        {
            List<Task> cmds = new List<Task>();
            string insert = "INSERT INTO UserRoles (guild_id, user_id, role_id) SELECT @guild_id, @user_id, @role_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM UserRoles WHERE guild_id = @guild_id AND user_id = @user_id AND role_id = @role_id);";

            foreach (SocketRole role in roles)
            {
                using (SqliteCommand cmd = new SqliteCommand(insert, Program.cnModRoles))
                {
                    cmd.Parameters.AddWithValue("@guild_id", Context.Guild.Id.ToString());
                    cmd.Parameters.AddWithValue("@user_id", Context.User.Id.ToString());
                    cmd.Parameters.AddWithValue("@role_id", role.Id.ToString());
                    cmds.Add(cmd.ExecuteNonQueryAsync());
                }
            }

            await Task.WhenAll(cmds);
        }
    }
}
