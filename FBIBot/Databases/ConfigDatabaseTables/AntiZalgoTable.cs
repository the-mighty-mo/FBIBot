using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Databases.ConfigDatabaseTables
{
    public class AntiZalgoTable : ITable
    {
        private readonly SqliteConnection connection;

        public AntiZalgoTable(SqliteConnection connection) => this.connection = connection;

        public async Task InitAsync()
        {
            using SqliteCommand cmd = new("CREATE TABLE IF NOT EXISTS AntiZalgo (guild_id TEXT PRIMARY KEY);", connection);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task<bool> GetAntiZalgoAsync(SocketGuild g)
        {
            bool isAntiZalgo;

            string getZalgo = "SELECT guild_id FROM AntiZalgo WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(getZalgo, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            SqliteDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            isAntiZalgo = await reader.ReadAsync().ConfigureAwait(false);
            reader.Close();

            return isAntiZalgo;
        }

        public async Task SetAntiZalgoAsync(SocketGuild g)
        {
            string insert = "INSERT INTO AntiZalgo (guild_id) SELECT @guild_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM AntiZalgo WHERE guild_id = @guild_id);";

            using SqliteCommand cmd = new(insert, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task RemoveAntiZalgoAsync(SocketGuild g)
        {
            string delete = "DELETE FROM AntiZalgo WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(delete, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }
}
