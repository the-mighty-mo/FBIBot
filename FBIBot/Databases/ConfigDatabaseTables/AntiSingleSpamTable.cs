using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Databases.ConfigDatabaseTables
{
    public class AntiSingleSpamTable : ITable
    {
        private readonly SqliteConnection connection;

        public AntiSingleSpamTable(SqliteConnection connection) => this.connection = connection;

        public Task InitAsync()
        {
            using SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS AntiSingleSpam (guild_id TEXT PRIMARY KEY);", connection);
            return cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> GetAntiSingleSpamAsync(SocketGuild g)
        {
            bool isAntiSingleSpam = false;

            string getSingleSpam = "SELECT guild_id FROM AntiSingleSpam WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getSingleSpam, connection))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                isAntiSingleSpam = await reader.ReadAsync();
                reader.Close();
            }

            return isAntiSingleSpam;
        }

        public async Task SetAntiSingleSpamAsync(SocketGuild g)
        {
            string insert = "INSERT INTO AntiSingleSpam (guild_id) SELECT @guild_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM AntiSingleSpam WHERE guild_id = @guild_id);";

            using (SqliteCommand cmd = new SqliteCommand(insert, connection))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task RemoveAntiSingleSpamAsync(SocketGuild g)
        {
            string delete = "DELETE FROM AntiSingleSpam WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, connection))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
