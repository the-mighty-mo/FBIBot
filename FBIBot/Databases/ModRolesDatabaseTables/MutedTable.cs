using Discord;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Databases.ModRolesDatabaseTables
{
    public class MutedTable : ITable
    {
        private readonly SqliteConnection connection;

        public MutedTable(SqliteConnection connection) => this.connection = connection;

        public async Task InitAsync()
        {
            using SqliteCommand cmd = new("CREATE TABLE IF NOT EXISTS Muted (guild_id TEXT PRIMARY KEY, role_id TEXT NOT NULL);", connection);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task<SocketRole?> GetMuteRole(SocketGuild g)
        {
            SocketRole? role = null;

            string getRole = "SELECT role_id FROM Muted WHERE guild_id = @guild_id;";

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

        public async Task SetMuteRoleAsync(IRole role, SocketGuild g)
        {
            string update = "UPDATE Muted SET role_id = @role_id WHERE guild_id = @guild_id;";
            string insert = "INSERT INTO Muted (guild_id, role_id) SELECT @guild_id, @role_id WHERE (SELECT Changes() = 0);";

            using SqliteCommand cmd = new(update + insert, connection);

            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
            cmd.Parameters.AddWithValue("@role_id", role.Id.ToString());

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task RemoveMuteRoleAsync(SocketGuild g)
        {
            string delete = "DELETE FROM Muted WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(delete, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }
}
