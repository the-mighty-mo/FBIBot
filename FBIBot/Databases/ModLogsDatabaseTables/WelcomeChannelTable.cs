using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Databases.ModLogsDatabaseTables
{
    public class WelcomeChannelTable : ITable
    {
        private readonly SqliteConnection connection;

        public WelcomeChannelTable(SqliteConnection connection) => this.connection = connection;

        public async Task InitAsync()
        {
            using SqliteCommand cmd = new("CREATE TABLE IF NOT EXISTS WelcomeChannel (guild_id TEXT PRIMARY KEY, channel_id TEXT NOT NULL);", connection);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task<SocketTextChannel?> GetWelcomeChannelAsync(SocketGuild g)
        {
            SocketTextChannel? channel = null;

            string getChannel = "SELECT channel_id FROM WelcomeChannel WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(getChannel, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            SqliteDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            if (await reader.ReadAsync().ConfigureAwait(false))
            {
                _ = ulong.TryParse(reader["channel_id"].ToString(), out ulong channelID);
                channel = g.GetTextChannel(channelID);
            }
            reader.Close();

            return channel;
        }

        public async Task SetWelcomeChannelAsync(SocketTextChannel channel)
        {
            string update = "UPDATE WelcomeChannel SET channel_id = @channel_id WHERE guild_id = @guild_id;";
            string insert = "INSERT INTO WelcomeChannel (guild_id, channel_id) SELECT @guild_id, @channel_id WHERE (SELECT Changes() = 0);";

            using SqliteCommand cmd = new(update + insert, connection);

            cmd.Parameters.AddWithValue("@guild_id", channel.Guild.Id.ToString());
            cmd.Parameters.AddWithValue("@channel_id", channel.Id.ToString());

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task RemoveWelcomeChannelAsync(SocketGuild g)
        {
            string delete = "DELETE FROM WelcomeChannel WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(delete, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }
}
