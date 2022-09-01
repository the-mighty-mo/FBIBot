using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Databases.ModLogsDatabaseTables
{
    public class WarningsTable : ITable
    {
        private readonly SqliteConnection connection;

        public WarningsTable(SqliteConnection connection) => this.connection = connection;

        public Task InitAsync()
        {
            using SqliteCommand cmd = new("CREATE TABLE IF NOT EXISTS Warnings (guild_id TEXT NOT NULL, id TEXT NOT NULL, user_id TEXT NOT NULL, UNIQUE (guild_id, id));", connection);
            return cmd.ExecuteNonQueryAsync();
        }

        public async Task AddWarningAsync(SocketGuildUser u, ulong id)
        {
            string addWarning = "INSERT INTO Warnings (guild_id, id, user_id) SELECT @guild_id, @id, @user_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM Warnings WHERE guild_id = @guild_id AND id = @id AND user_id = @user_id);";

            using SqliteCommand cmd = new(addWarning, connection);
            cmd.Parameters.AddWithValue("@guild_id", u.Guild.Id.ToString());
            cmd.Parameters.AddWithValue("@id", id.ToString());
            cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> GetWarningAsync(SocketGuildUser u, ulong id)
        {
            bool hasWarning;

            string getWarning = "SELECT * FROM Warnings WHERE guild_id = @guild_id AND id = @id AND user_id = @user_id;";

            using SqliteCommand cmd = new(getWarning, connection);
            cmd.Parameters.AddWithValue("@guild_id", u.Guild.Id.ToString());
            cmd.Parameters.AddWithValue("@id", id.ToString());
            cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());

            SqliteDataReader reader = await cmd.ExecuteReaderAsync();
            hasWarning = await reader.ReadAsync();
            reader.Close();

            return hasWarning;
        }

        public async Task<List<ulong>> GetWarningsAsync(SocketGuildUser u)
        {
            List<ulong> ids = new();

            string getWarns = "SELECT id FROM Warnings WHERE guild_id = @guild_id AND user_id = @user_id;";

            using SqliteCommand cmd = new(getWarns, connection);
            cmd.Parameters.AddWithValue("@guild_id", u.Guild.Id.ToString());
            cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());

            SqliteDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                if (ulong.TryParse(reader["id"].ToString(), out ulong id))
                {
                    ids.Add(id);
                }
            }
            reader.Close();

            return ids;
        }

        public async Task RemoveWarningAsync(SocketGuildUser u, ulong id)
        {
            string delete = "DELETE FROM Warnings WHERE guild_id = @guild_id AND id = @id AND user_id = @user_id;";

            using SqliteCommand cmd = new(delete, connection);
            cmd.Parameters.AddWithValue("@guild_id", u.Guild.Id.ToString());
            cmd.Parameters.AddWithValue("@id", id.ToString());
            cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task RemoveWarningsAsync(SocketGuildUser u, int? count = null)
        {
            string delete = "DELETE FROM Warnings WHERE guild_id = @guild_id AND user_id = @user_id";
            if (count != null)
            {
                delete += " AND id IN (SELECT id FROM Warnings WHERE guild_id = @guild_id AND user_id = @user_id ORDER BY CAST(id AS INTEGER) ASC LIMIT @count)";
            }
            delete += ";";

            using SqliteCommand cmd = new(delete, connection);
            cmd.Parameters.AddWithValue("@guild_id", u.Guild.Id.ToString());
            cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());
            cmd.Parameters.AddWithValue("@count", count?.ToString());

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task RemoveAllWarningsAsync(SocketGuild g)
        {
            string delete = "DELETE FROM Warnings WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(delete, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
