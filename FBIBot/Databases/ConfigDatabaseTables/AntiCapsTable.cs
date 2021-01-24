using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Databases.ConfigDatabaseTables
{
    public class AntiCapsTable : ITable
    {
        private readonly SqliteConnection connection;

        public AntiCapsTable(SqliteConnection connection) => this.connection = connection;

        public Task InitAsync()
        {
            using SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS AntiCaps (guild_id TEXT PRIMARY KEY);", connection);
            return cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> GetAntiCapsAsync(SocketGuild g)
        {
            bool isAntiCaps = false;

            string getCaps = "SELECT guild_id FROM AntiCaps WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getCaps, connection))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                isAntiCaps = await reader.ReadAsync();
                reader.Close();
            }

            return isAntiCaps;
        }

        public async Task SetAntiCapsAsync(SocketGuild g)
        {
            string insert = "INSERT INTO AntiCaps (guild_id) SELECT @guild_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM AntiCaps WHERE guild_id = @guild_id);";

            using (SqliteCommand cmd = new SqliteCommand(insert, connection))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task RemoveAntiCapsAsync(SocketGuild g)
        {
            string delete = "DELETE FROM AntiCaps WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, connection))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
