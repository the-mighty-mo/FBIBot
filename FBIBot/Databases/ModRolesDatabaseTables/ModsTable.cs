using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Databases.ModRolesDatabaseTables
{
    public class ModsTable : ITable
    {
        private readonly SqliteConnection connection;

        public ModsTable(SqliteConnection connection) => this.connection = connection;

        public async Task InitAsync()
        {
            using SqliteCommand cmd = new("CREATE TABLE IF NOT EXISTS Mods (guild_id TEXT TEXT NOT NULL, role_id TEXT NOT NULL);", connection);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task<List<SocketRole>> GetModRolesAsync(SocketGuild g)
        {
            List<SocketRole> roles = new();

            string getRoles = "SELECT role_id FROM Mods WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(getRoles, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            SqliteDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                ulong roleID = ulong.Parse(reader["role_id"].ToString()!);
                SocketRole role = g.GetRole(roleID);
                if (role != null)
                {
                    roles.Add(role);
                }
            }
            reader.Close();

            return roles;
        }

        public async Task AddModRoleAsync(SocketRole role)
        {
            string insert = "INSERT INTO Mods (guild_id, role_id) SELECT @guild_id, @role_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM Mods WHERE guild_id = @guild_id AND role_id = @role_id);";

            using SqliteCommand cmd = new(insert, connection);
            cmd.Parameters.AddWithValue("@guild_id", role.Guild.Id.ToString());
            cmd.Parameters.AddWithValue("@role_id", role.Id.ToString());

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task RemoveModAsync(SocketRole role)
        {
            string delete = "DELETE FROM Mods WHERE guild_id = @guild_id AND role_id = @role_id;";

            using SqliteCommand cmd = new(delete, connection);
            cmd.Parameters.AddWithValue("@guild_id", role.Guild.Id.ToString());
            cmd.Parameters.AddWithValue("@role_id", role.Id.ToString());

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }
}
