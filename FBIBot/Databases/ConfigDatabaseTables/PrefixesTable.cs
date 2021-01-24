using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Databases.ConfigDatabaseTables
{
    public class PrefixesTable : ITable
    {
        private readonly SqliteConnection connection;

        public PrefixesTable(SqliteConnection connection) => this.connection = connection;

        public Task InitAsync()
        {
            using SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS Prefixes (guild_id TEXT PRIMARY KEY, prefix TEXT NOT NULL);", connection);
            return cmd.ExecuteNonQueryAsync();
        }

        public async Task<string> GetPrefixAsync(SocketGuild g)
        {
            string prefix = CommandHandler.prefix;

            string getPrefix = "SELECT prefix FROM Prefixes WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getPrefix, connection))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    prefix = (string)reader["prefix"];
                }
                reader.Close();
            }

            return prefix;
        }

        public async Task SetPrefixAsync(SocketGuild g, string prefix)
        {
            string update = "UPDATE Prefixes SET prefix = @prefix WHERE guild_id = @guild_id;";
            string insert = "INSERT INTO Prefixes (guild_id, prefix) SELECT @guild_id, @prefix WHERE (SELECT Changes() = 0);";

            using (SqliteCommand cmd = new SqliteCommand(update + insert, connection))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                cmd.Parameters.AddWithValue("@prefix", prefix);

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
