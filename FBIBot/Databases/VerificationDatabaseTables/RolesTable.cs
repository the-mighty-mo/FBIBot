﻿using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Databases.VerificationDatabaseTables
{
    public class RolesTable : ITable
    {
        private readonly SqliteConnection connection;

        public RolesTable(SqliteConnection connection) => this.connection = connection;

        public async Task InitAsync()
        {
            using SqliteCommand cmd = new("CREATE TABLE IF NOT EXISTS Roles (guild_id TEXT PRIMARY KEY, role_id TEXT NOT NULL);", connection);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task<SocketRole?> GetVerificationRoleAsync(SocketGuild g)
        {
            SocketRole? role = null;

            string getRole = "SELECT role_id FROM Roles WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(getRole, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id);

            SqliteDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            if (await reader.ReadAsync().ConfigureAwait(false))
            {
                ulong roleID = ulong.Parse(reader["role_id"].ToString()!);
                role = g.GetRole(roleID);
            }
            reader.Close();

            return role;
        }

        public async Task SetVerificationRoleAsync(SocketRole role)
        {
            string update = "UPDATE Roles SET role_id = @role_id WHERE guild_id = @guild_id;";
            string insert = "INSERT INTO Roles (guild_id, role_id) SELECT @guild_id, @role_id WHERE (SELECT Changes() = 0);";

            using SqliteCommand cmd = new(update + insert, connection);
            cmd.Parameters.AddWithValue("@guild_id", role.Guild.Id.ToString());
            cmd.Parameters.AddWithValue("@role_id", role.Id.ToString());

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task RemoveVerificationRoleAsync(SocketGuild g)
        {
            string delete = "DELETE FROM Roles WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(delete, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }
}
