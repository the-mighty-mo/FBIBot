using Discord;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Databases.RaidModeDatabaseTables
{
    public class RaidModeTable : ITable
    {
        private readonly SqliteConnection connection;

        public RaidModeTable(SqliteConnection connection) => this.connection = connection;

        public Task InitAsync()
        {
            using SqliteCommand cmd = new("CREATE TABLE IF NOT EXISTS RaidMode (guild_id TEXT PRIMARY KEY, level TEXT NOT NULL);", connection);
            return cmd.ExecuteNonQueryAsync();
        }

        public async Task<VerificationLevel?> GetVerificationLevelAsync(SocketGuild g)
        {
            VerificationLevel? level = null;

            string getLevel = "SELECT level FROM RaidMode WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(getLevel, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            SqliteDataReader reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                if (int.TryParse(reader["level"].ToString(), out int levelInt))
                {
                    level = (VerificationLevel)levelInt;
                }
            }
            reader.Close();

            return level;
        }

        public async Task SaveVerificationLevelAsync(SocketGuild g)
        {
            string update = "UPDATE RaidMode SET level = @level WHERE guild_id = @guild_id;";
            string insert = "INSERT INTO RaidMode (guild_id, level) SELECT @guild_id, @level WHERE (SELECT Changes() = 0);";

            using SqliteCommand cmd = new(update + insert, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
            cmd.Parameters.AddWithValue("@level", ((int)g.VerificationLevel).ToString());

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task RemoveVerificationLevelAsync(SocketGuild g)
        {
            string delete = "DELETE FROM RaidMode WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(delete, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
