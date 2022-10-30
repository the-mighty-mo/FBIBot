using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Databases.ConfigDatabaseTables
{
    public class AntiSingleSpamTable : ITable
    {
        private readonly SqliteConnection connection;

        public AntiSingleSpamTable(SqliteConnection connection) => this.connection = connection;

        public async Task InitAsync()
        {
            using SqliteCommand cmd = new("CREATE TABLE IF NOT EXISTS AntiSingleSpam (guild_id TEXT PRIMARY KEY);", connection);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task<bool> GetAntiSingleSpamAsync(SocketGuild g)
        {
            bool isAntiSingleSpam;

            string getSingleSpam = "SELECT guild_id FROM AntiSingleSpam WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(getSingleSpam, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            SqliteDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            isAntiSingleSpam = await reader.ReadAsync().ConfigureAwait(false);
            reader.Close();

            return isAntiSingleSpam;
        }

        public async Task SetAntiSingleSpamAsync(SocketGuild g)
        {
            string insert = "INSERT INTO AntiSingleSpam (guild_id) SELECT @guild_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM AntiSingleSpam WHERE guild_id = @guild_id);";

            using SqliteCommand cmd = new(insert, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task RemoveAntiSingleSpamAsync(SocketGuild g)
        {
            string delete = "DELETE FROM AntiSingleSpam WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(delete, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }
}
