using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Databases.ConfigDatabaseTables
{
    public class AntiInviteTable : ITable
    {
        private readonly SqliteConnection connection;

        public AntiInviteTable(SqliteConnection connection) => this.connection = connection;

        public async Task InitAsync()
        {
            using SqliteCommand cmd = new("CREATE TABLE IF NOT EXISTS AntiInvite (guild_id TEXT PRIMARY KEY);", connection);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task<bool> GetAntiInviteAsync(SocketGuild g)
        {
            bool isAntiInvite;

            string getInvite = "SELECT guild_id FROM AntiInvite WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(getInvite, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            SqliteDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            isAntiInvite = await reader.ReadAsync().ConfigureAwait(false);
            reader.Close();

            return isAntiInvite;
        }

        public async Task SetAntiInviteAsync(SocketGuild g)
        {
            string insert = "INSERT INTO AntiInvite (guild_id) SELECT @guild_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM AntiInvite WHERE guild_id = @guild_id);";

            using SqliteCommand cmd = new(insert, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task RemoveAntiInviteAsync(SocketGuild g)
        {
            string delete = "DELETE FROM AntiInvite WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(delete, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }
}
