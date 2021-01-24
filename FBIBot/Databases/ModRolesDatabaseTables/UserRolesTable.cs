using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Databases.ModRolesDatabaseTables
{
    public class UserRolesTable : ITable
    {
        private readonly SqliteConnection connection;

        public UserRolesTable(SqliteConnection connection) => this.connection = connection;

        public Task InitAsync()
        {
            using SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS UserRoles (guild_id TEXT NOT NULL, user_id TEXT NOT NULL, role_id TEXT NOT NULL, UNIQUE (guild_id, user_id, role_id));", connection);
            return cmd.ExecuteNonQueryAsync();
        }

        public async Task SaveUserRolesAsync(List<SocketRole> roles, SocketGuildUser user)
        {
            List<Task> cmds = new List<Task>();
            string insert = "INSERT INTO UserRoles (guild_id, user_id, role_id) SELECT @guild_id, @user_id, @role_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM UserRoles WHERE guild_id = @guild_id AND user_id = @user_id AND role_id = @role_id);";

            foreach (SocketRole role in roles)
            {
                using (SqliteCommand cmd = new SqliteCommand(insert, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", user.Guild.Id.ToString());
                    cmd.Parameters.AddWithValue("@user_id", user.Id.ToString());
                    cmd.Parameters.AddWithValue("@role_id", role.Id.ToString());
                    cmds.Add(cmd.ExecuteNonQueryAsync());
                }
            }

            await Task.WhenAll(cmds);
        }

        public async Task<List<SocketRole>> GetUserRolesAsync(SocketGuildUser user)
        {
            List<SocketRole> roles = new List<SocketRole>();

            string getRoles = "SELECT role_id FROM UserRoles WHERE guild_id = @guild_id AND user_id = @user_id;";
            using (SqliteCommand cmd = new SqliteCommand(getRoles, connection))
            {
                cmd.Parameters.AddWithValue("@guild_id", user.Guild.Id.ToString());
                cmd.Parameters.AddWithValue("@user_id", user.Id.ToString());

                SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
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

            return roles;
        }

        public async Task RemoveUserRolesAsync(SocketGuildUser user)
        {
            string delete = "DELETE FROM UserRoles WHERE guild_id = @guild_id AND user_id = @user_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, connection))
            {
                cmd.Parameters.AddWithValue("@guild_id", user.Guild.Id.ToString());
                cmd.Parameters.AddWithValue("@user_id", user.Id.ToString());

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
