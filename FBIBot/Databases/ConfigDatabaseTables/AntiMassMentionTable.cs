using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Databases.ConfigDatabaseTables
{
    public class AntiMassMentionTable : ITable
    {
        private readonly SqliteConnection connection;

        public AntiMassMentionTable(SqliteConnection connection) => this.connection = connection;

        public Task InitAsync()
        {
            using SqliteCommand cmd = new("CREATE TABLE IF NOT EXISTS AntiMassMention (guild_id TEXT PRIMARY KEY);", connection);
            return cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> GetAntiMassMentionAsync(SocketGuild g)
        {
            bool isAntiMassMention;

            string getMassMention = "SELECT guild_id FROM AntiMassMention WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(getMassMention, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            SqliteDataReader reader = await cmd.ExecuteReaderAsync();
            isAntiMassMention = await reader.ReadAsync();
            reader.Close();

            return isAntiMassMention;
        }

        public async Task SetAntiMassMentionAsync(SocketGuild g)
        {
            string insert = "INSERT INTO AntiMassMention (guild_id) SELECT @guild_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM AntiMassMention WHERE guild_id = @guild_id);";

            using SqliteCommand cmd = new(insert, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task RemoveAntiMassMentionAsync(SocketGuild g)
        {
            string delete = "DELETE FROM AntiMassMention WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(delete, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
