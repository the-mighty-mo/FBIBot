using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Databases
{
    public class VerificationDatabase
    {
        private readonly SqliteConnection connection = new SqliteConnection("Filename=Verification.db");

        public readonly CaptchaTable Captcha;
        public readonly VerifiedTable Verified;
        public readonly AttemptsTable Attempts;
        public readonly RolesTable Roles;

        public VerificationDatabase()
        {
            Captcha = new CaptchaTable(connection);
            Verified = new VerifiedTable(connection);
            Attempts = new AttemptsTable(connection);
            Roles = new RolesTable(connection);
        }

        public async Task InitAsync()
        {
            await connection.OpenAsync();

            List<Task> cmds = new List<Task>();
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS Captcha (user_id TEXT PRIMARY KEY, captcha TEXT NOT NULL);", connection))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS Verified (user_id TEXT PRIMARY KEY);", connection))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS Attempts (user_id TEXT PRIMARY KEY, attempts INTEGER NOT NULL);", connection))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS Roles (guild_id TEXT PRIMARY KEY, role_id TEXT NOT NULL);", connection))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }

            await Task.WhenAll(cmds);
        }

        public async Task CloseAsync() => await connection.CloseAsync();

        public class CaptchaTable
        {
            private readonly SqliteConnection connection;

            public CaptchaTable(SqliteConnection connection) => this.connection = connection;

            public async Task<string> GetCaptchaAsync(SocketUser u)
            {
                string captcha = null;

                string read = "SELECT captcha FROM Captcha WHERE user_id = @user_id;";
                using (SqliteCommand cmd = new SqliteCommand(read, connection))
                {
                    cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        captcha = (string)reader["captcha"];
                    }
                    reader.Close();
                }

                return captcha;
            }

            public async Task SetCaptchaAsync(string captcha, SocketUser u)
            {
                string update = "UPDATE Captcha SET captcha = @captcha WHERE user_id = @user_id;";
                string insert = "INSERT INTO Captcha (user_id, captcha) SELECT @user_id, @captcha WHERE (SELECT Changes() = 0);";

                using (SqliteCommand cmd = new SqliteCommand(update + insert, connection))
                {
                    cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());
                    cmd.Parameters.AddWithValue("@captcha", captcha);
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public async Task RemoveCaptchaAsync(SocketUser u)
            {
                string delete = "DELETE FROM Captcha WHERE user_id = @user_id;";
                using (SqliteCommand cmd = new SqliteCommand(delete, connection))
                {
                    cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public class VerifiedTable
        {
            private readonly SqliteConnection connection;

            public VerifiedTable(SqliteConnection connection) => this.connection = connection;

            public async Task<bool> GetVerifiedAsync(SocketUser u)
            {
                bool isVerified = false;

                string verify = "SELECT user_id FROM Verified WHERE user_id = @user_id;";
                using (SqliteCommand cmd = new SqliteCommand(verify, connection))
                {
                    cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    isVerified = await reader.ReadAsync();
                    reader.Close();
                }

                return isVerified;
            }

            public async Task SetVerifiedAsync(SocketUser u)
            {
                await new CaptchaTable(connection).RemoveCaptchaAsync(u);

                string verify = "INSERT INTO Verified (user_id) SELECT @user_id WHERE NOT EXISTS (SELECT 1 FROM Verified WHERE user_id = @user_id);";
                using (SqliteCommand cmd = new SqliteCommand(verify, connection))
                {
                    cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public async Task RemoveVerifiedAsync(SocketUser u)
            {
                string delete = "DELETE FROM Verified WHERE user_id = @user_id;";
                using (SqliteCommand cmd = new SqliteCommand(delete, connection))
                {
                    cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public class AttemptsTable
        {
            private readonly SqliteConnection connection;

            public AttemptsTable(SqliteConnection connection) => this.connection = connection;

            public async Task<int> GetAttemptsAsync(SocketUser u)
            {
                int attempts = 0;

                string read = "SELECT attempts FROM Attempts WHERE user_id = @user_id;";
                using (SqliteCommand cmd = new SqliteCommand(read, connection))
                {
                    cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        attempts = int.Parse(reader["attempts"].ToString()!);
                    }
                    reader.Close();
                }

                return attempts;
            }

            public async Task SetAttemptsAsync(SocketUser u, int attempts)
            {
                string update = "UPDATE Attempts SET attempts = @attempts WHERE user_id = @user_id;";
                string insert = "INSERT INTO Attempts (user_id, attempts) SELECT @user_id, @attempts WHERE (SELECT Changes() = 0);\n";

                using (SqliteCommand cmd = new SqliteCommand(update + insert, connection))
                {
                    cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());
                    cmd.Parameters.AddWithValue("@attempts", attempts);
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public async Task RemoveAttemptsAsync(SocketUser u)
            {
                string delete = "DELETE FROM Attempts WHERE user_id = @user_id;";
                using (SqliteCommand cmd = new SqliteCommand(delete, connection))
                {
                    cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public class RolesTable
        {
            private readonly SqliteConnection connection;

            public RolesTable(SqliteConnection connection) => this.connection = connection;

            public async Task<SocketRole> GetVerificationRoleAsync(SocketGuild g)
            {
                SocketRole role = null;

                string getRole = "SELECT role_id FROM Roles WHERE guild_id = @guild_id;";
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

            public async Task SetVerificationRoleAsync(SocketRole role)
            {
                string update = "UPDATE Roles SET role_id = @role_id WHERE guild_id = @guild_id;";
                string insert = "INSERT INTO Roles (guild_id, role_id) SELECT @guild_id, @role_id WHERE (SELECT Changes() = 0);";

                using (SqliteCommand cmd = new SqliteCommand(update + insert, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", role.Guild.Id.ToString());
                    cmd.Parameters.AddWithValue("@role_id", role.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public async Task RemoveVerificationRoleAsync(SocketGuild g)
            {
                string delete = "DELETE FROM Roles WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(delete, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}