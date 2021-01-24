using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Databases.ConfigDatabaseTables
{
    public class AntiSpamTable : ITable
    {
        private readonly SqliteConnection connection;

        public AntiSpamTable(SqliteConnection connection) => this.connection = connection;

        public Task InitAsync()
        {
            using SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS AntiSpam (guild_id TEXT PRIMARY KEY);", connection);
            return cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> GetAntiSpamAsync(SocketGuild g)
        {
            bool isAntiSpam = false;

            string getSpam = "SELECT guild_id FROM AntiSpam WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getSpam, connection))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                isAntiSpam = await reader.ReadAsync();
                reader.Close();
            }

            return isAntiSpam;
        }

        public async Task SetAntiSpamAsync(SocketGuild g)
        {
            string insert = "INSERT INTO AntiSpam (guild_id) SELECT @guild_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM AntiSpam WHERE guild_id = @guild_id);";

            using (SqliteCommand cmd = new SqliteCommand(insert, connection))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task RemoveAntiSpamAsync(SocketGuild g)
        {
            string delete = "DELETE FROM AntiSpam WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, connection))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
