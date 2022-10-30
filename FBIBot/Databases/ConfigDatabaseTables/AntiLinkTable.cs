using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Databases.ConfigDatabaseTables
{
    public class AntiLinkTable : ITable
    {
        private readonly SqliteConnection connection;

        public AntiLinkTable(SqliteConnection connection) => this.connection = connection;

        public async Task InitAsync()
        {
            using SqliteCommand cmd = new("CREATE TABLE IF NOT EXISTS AntiLink (guild_id TEXT PRIMARY KEY);", connection);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task<bool> GetAntiLinkAsync(SocketGuild g)
        {
            bool isAntiLink;

            string getLink = "SELECT guild_id FROM AntiLink WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(getLink, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            SqliteDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            isAntiLink = await reader.ReadAsync().ConfigureAwait(false);
            reader.Close();

            return isAntiLink;
        }

        public async Task SetAntiLinkAsync(SocketGuild g)
        {
            string insert = "INSERT INTO AntiLink (guild_id) SELECT @guild_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM AntiLink WHERE guild_id = @guild_id);";

            using SqliteCommand cmd = new(insert, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task RemoveAntiLinkAsync(SocketGuild g)
        {
            string delete = "DELETE FROM AntiLink WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(delete, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }
}
