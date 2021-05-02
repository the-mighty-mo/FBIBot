using Discord;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Databases.ModLogsDatabaseTables
{
    public class ModLogsTable : ITable
    {
        private readonly SqliteConnection connection;

        public ModLogsTable(SqliteConnection connection) => this.connection = connection;

        public Task InitAsync()
        {
            using SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS ModLogs (guild_id TEXT NOT NULL, id TEXT NOT NULL, channel_id TEXT NOT NULL, message_id TEXT NOT NULL, UNIQUE (guild_id, id));", connection);
            return cmd.ExecuteNonQueryAsync();
        }

        public async Task<ulong> GetNextModLogID(SocketGuild g)
        {
            ulong id = 0;

            string getID = "SELECT MAX(CAST(id AS INTEGER)) AS id FROM ModLogs WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getID, connection))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    _ = ulong.TryParse(reader["id"].ToString(), out id);
                }
                reader.Close();
            }

            return ++id;
        }

        public async Task<IUserMessage> GetModLogAsync(SocketGuild g, ulong id)
        {
            IUserMessage msg = null;

            string getMessage = "SELECT channel_id, message_id FROM ModLogs WHERE guild_id = @guild_id AND id = @id;";
            using (SqliteCommand cmd = new SqliteCommand(getMessage, connection))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                cmd.Parameters.AddWithValue("@id", id.ToString());

                SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    _ = ulong.TryParse(reader["channel_id"].ToString(), out ulong channelID);
                    _ = ulong.TryParse(reader["message_id"].ToString(), out ulong messageID);

                    if (g.GetTextChannel(channelID) is var channel)
                    {
                        msg = await channel.GetMessageAsync(messageID) as IUserMessage;
                    }
                }
                reader.Close();
            }

            return msg;
        }

        public async Task SaveModLogAsync(IUserMessage msg, SocketGuild g, ulong id)
        {
            string insert = "INSERT INTO ModLogs (guild_id, id, channel_id, message_id) SELECT @guild_id, @id, @channel_id, @message_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM ModLogs WHERE guild_id = @guild_id AND id = @id);";

            using SqliteCommand cmd = new SqliteCommand(insert, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
            cmd.Parameters.AddWithValue("@id", id.ToString());
            cmd.Parameters.AddWithValue("@channel_id", msg.Channel.Id.ToString());
            cmd.Parameters.AddWithValue("@message_id", msg.Id.ToString());

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task RemoveModLogsAsync(SocketGuild g)
        {
            string delete = "DELETE FROM ModLogs WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new SqliteCommand(delete, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
