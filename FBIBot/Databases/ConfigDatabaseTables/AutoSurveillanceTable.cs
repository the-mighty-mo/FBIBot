using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Databases.ConfigDatabaseTables
{
    public class AutoSurveillanceTable : ITable
    {
        private readonly SqliteConnection connection;

        public AutoSurveillanceTable(SqliteConnection connection) => this.connection = connection;

        public Task InitAsync()
        {
            using SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS AutoSurveillance (guild_id TEXT PRIMARY KEY);", connection);
            return cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> GetAutoSurveillanceAsync(SocketGuild g)
        {
            bool isAutoSurveillance = false;

            string getSurveillance = "SELECT guild_id FROM AutoSurveillance WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getSurveillance, connection))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                isAutoSurveillance = await reader.ReadAsync();
                reader.Close();
            }

            return isAutoSurveillance;
        }

        public async Task SetAutoSurveillanceAsync(SocketGuild g)
        {
            string insert = "INSERT INTO AutoSurveillance (guild_id) SELECT @guild_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM AutoSurveillance WHERE guild_id = @guild_id);";

            using (SqliteCommand cmd = new SqliteCommand(insert, connection))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task RemoveAutoSurveillanceAsync(SocketGuild g)
        {
            string delete = "DELETE FROM AutoSurveillance WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, connection))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
