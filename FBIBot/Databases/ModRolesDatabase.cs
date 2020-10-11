using Discord;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Databases
{
    public class ModRolesDatabase
    {
        private readonly SqliteConnection connection = new SqliteConnection("Filename=ModRoles.db");

        public readonly MutedTable Muted;
        public readonly PrisonerRoleTable PrisonerRole;
        public readonly PrisonerChannelTable PrisonerChannel;
        public readonly PrisonersTable Prisoners;
        public readonly UserRolesTable UserRoles;
        public readonly ModsTable Mods;
        public readonly AdminsTable Admins;

        public ModRolesDatabase()
        {
            Muted = new MutedTable(connection);
            PrisonerRole = new PrisonerRoleTable(connection);
            PrisonerChannel = new PrisonerChannelTable(connection);
            Prisoners = new PrisonersTable(connection);
            UserRoles = new UserRolesTable(connection);
            Mods = new ModsTable(connection);
            Admins = new AdminsTable(connection);
        }

        public async Task InitAsync()
        {
            await connection.OpenAsync();

            List<Task> cmds = new List<Task>();
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS Muted (guild_id TEXT PRIMARY KEY, role_id TEXT NOT NULL);", connection))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS PrisonerRole (guild_id TEXT PRIMARY KEY, role_id TEXT NOT NULL);", connection))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS PrisonerChannel (guild_id TEXT PRIMARY KEY, channel_id TEXT NOT NULL);", connection))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS Prisoners (guild_id TEXT NOT NULL, user_id TEXT NOT NULL, UNIQUE (guild_id, user_id));", connection))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS UserRoles (guild_id TEXT NOT NULL, user_id TEXT NOT NULL, role_id TEXT NOT NULL, UNIQUE (guild_id, user_id, role_id));", connection))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS Mods (guild_id TEXT TEXT NOT NULL, role_id TEXT NOT NULL);", connection))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS Admins (guild_id TEXT NOT NULL, role_id TEXT NOT NULL);", connection))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }

            await Task.WhenAll(cmds);
        }

        public async Task CloseAsync() => await connection.CloseAsync();

        public class MutedTable
        {
            private readonly SqliteConnection connection;

            public MutedTable(SqliteConnection connection) => this.connection = connection;

            public async Task<SocketRole> GetMuteRole(SocketGuild g)
            {
                SocketRole role = null;

                string getRole = "SELECT role_id FROM Muted WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(getRole, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id);

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        ulong roleID = ulong.Parse(reader["role_id"].ToString()!);
                        role = g.GetRole(roleID);
                    }
                    reader.Close();
                }

                return role;
            }

            public async Task SetMuteRoleAsync(IRole role, SocketGuild g)
            {
                string update = "UPDATE Muted SET role_id = @role_id WHERE guild_id = @guild_id;";
                string insert = "INSERT INTO Muted (guild_id, role_id) SELECT @guild_id, @role_id WHERE (SELECT Changes() = 0);";

                using (SqliteCommand cmd = new SqliteCommand(update + insert, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                    cmd.Parameters.AddWithValue("@role_id", role.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public  async Task RemoveMuteRoleAsync(SocketGuild g)
            {
                string delete = "DELETE FROM Muted WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(delete, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public class PrisonerRoleTable
        {
            private readonly SqliteConnection connection;

            public PrisonerRoleTable(SqliteConnection connection) => this.connection = connection;

            public async Task<SocketRole> GetPrisonerRoleAsync(SocketGuild g)
            {
                SocketRole role = null;

                string getRole = "SELECT role_id FROM PrisonerRole WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(getRole, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        ulong.TryParse(reader["role_id"].ToString(), out ulong roleID);
                        role = g.GetRole(roleID);
                    }
                    reader.Close();
                }

                return role;
            }

            public async Task SetPrisonerRoleAsync(IRole role)
            {
                string update = "UPDATE PrisonerRole SET role_id = @role_id WHERE guild_id = @guild_id;";
                string insert = "INSERT INTO PrisonerRole (guild_id, role_id) SELECT @guild_id, @role_id WHERE (SELECT Changes() = 0);";

                using (SqliteCommand cmd = new SqliteCommand(update + insert, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", role.Guild.Id.ToString());
                    cmd.Parameters.AddWithValue("@role_id", role.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public async Task RemovePrisonerRoleAsync(SocketGuild g)
            {
                string delete = "DELETE FROM PrisonerRole WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(delete, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public class PrisonerChannelTable
        {
            private readonly SqliteConnection connection;

            public PrisonerChannelTable(SqliteConnection connection) => this.connection = connection;

            public async Task<SocketTextChannel> GetPrisonerChannelAsync(SocketGuild g)
            {
                SocketTextChannel channel = null;

                string getChannel = "SELECT channel_id FROM PrisonerChannel WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(getChannel, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        ulong channelID = ulong.Parse(reader["channel_id"].ToString()!);
                        channel = g.GetTextChannel(channelID);
                    }
                    reader.Close();
                }

                return channel;
            }

            public async Task SetPrisonerChannelAsync(ITextChannel channel)
            {
                string update = "UPDATE PrisonerChannel SET channel_id = @channel_id WHERE guild_id = @guild_id;";
                string insert = "INSERT INTO PrisonerChannel (guild_id, channel_id) SELECT @guild_id, @channel_id WHERE (SELECT Changes() = 0);";
                using (SqliteCommand cmd = new SqliteCommand(update + insert, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", channel.Guild.Id.ToString());
                    cmd.Parameters.AddWithValue("@channel_id", channel.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public async Task RemovePrisonerChannelAsync(SocketGuild g)
            {
                string delete = "DELETE FROM PrisonerChannel WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(delete, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public class PrisonersTable
        {
            private readonly SqliteConnection connection;

            public PrisonersTable(SqliteConnection connection) => this.connection = connection;

            public async Task RecordPrisonerAsync(SocketGuildUser user)
            {
                string insert = "INSERT INTO Prisoners (guild_id, user_id) SELECT @guild_id, @user_id\n" +
                    "WHERE NOT EXISTS (SELECT 1 FROM Prisoners WHERE guild_id = @guild_id AND user_id = @user_id);";
                using (SqliteCommand cmd = new SqliteCommand(insert, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", user.Guild.Id.ToString());
                    cmd.Parameters.AddWithValue("@user_id", user.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public async Task<bool> HasPrisoners(SocketGuild g)
            {
                bool hasPrisoners = false;

                string getUsers = "SELECT * FROM Prisoners WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(getUsers, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    hasPrisoners = await reader.ReadAsync();
                    reader.Close();
                }

                return hasPrisoners;
            }

            public async Task RemovePrisonerAsync(SocketGuildUser user)
            {
                string delete = "DELETE FROM Prisoners WHERE guild_id = @guild_id AND user_id = @user_id;";
                using (SqliteCommand cmd = new SqliteCommand(delete, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", user.Guild.Id.ToString());
                    cmd.Parameters.AddWithValue("@user_id", user.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public class UserRolesTable
        {
            private readonly SqliteConnection connection;

            public UserRolesTable(SqliteConnection connection) => this.connection = connection;

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

        public class ModsTable
        {
            private readonly SqliteConnection connection;

            public ModsTable(SqliteConnection connection) => this.connection = connection;

            public async Task<List<SocketRole>> GetModRolesAsync(SocketGuild g)
            {
                List<SocketRole> roles = new List<SocketRole>();

                string getRoles = "SELECT role_id FROM Mods WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(getRoles, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        ulong roleID = ulong.Parse(reader["role_id"].ToString()!);
                        SocketRole role = g.GetRole(roleID);
                        if (role != null)
                        {
                            roles.Add(role);
                        }
                    }
                    reader.Close();
                }

                return roles;
            }

            public async Task AddModRoleAsync(SocketRole role)
            {
                string insert = "INSERT INTO Mods (guild_id, role_id) SELECT @guild_id, @role_id\n" +
                    "WHERE NOT EXISTS (SELECT 1 FROM Mods WHERE guild_id = @guild_id AND role_id = @role_id);";

                using (SqliteCommand cmd = new SqliteCommand(insert, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", role.Guild.Id.ToString());
                    cmd.Parameters.AddWithValue("@role_id", role.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public async Task RemoveModAsync(SocketRole role)
            {
                string delete = "DELETE FROM Mods WHERE guild_id = @guild_id AND role_id = @role_id;";
                using (SqliteCommand cmd = new SqliteCommand(delete, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", role.Guild.Id.ToString());
                    cmd.Parameters.AddWithValue("@role_id", role.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public class AdminsTable
        {
            private readonly SqliteConnection connection;

            public AdminsTable(SqliteConnection connection) => this.connection = connection;

            public async Task<List<SocketRole>> GetAdminRolesAsync(SocketGuild g)
            {
                List<SocketRole> roles = new List<SocketRole>();

                string getRoles = "SELECT role_id FROM Admins WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(getRoles, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        ulong roleID = ulong.Parse(reader["role_id"].ToString()!);
                        SocketRole role = g.GetRole(roleID);
                        if (role != null)
                        {
                            roles.Add(role);
                        }
                    }
                    reader.Close();
                }

                return roles;
            }

            public async Task AddAdminRoleAsync(SocketRole role)
            {
                string insert = "INSERT INTO Admins (guild_id, role_id) SELECT @guild_id, @role_id\n" +
                    "WHERE NOT EXISTS (SELECT 1 FROM Admins WHERE guild_id = @guild_id AND role_id = @role_id);";

                using (SqliteCommand cmd = new SqliteCommand(insert, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", role.Guild.Id.ToString());
                    cmd.Parameters.AddWithValue("@role_id", role.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public async Task RemoveAdminAsync(SocketRole role)
            {
                string delete = "DELETE FROM Admins WHERE guild_id = @guild_id AND role_id = @role_id;";
                using (SqliteCommand cmd = new SqliteCommand(delete, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", role.Guild.Id.ToString());
                    cmd.Parameters.AddWithValue("@role_id", role.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
