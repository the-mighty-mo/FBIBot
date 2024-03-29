﻿using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Databases.ConfigDatabaseTables
{
    public class ModifyMutedTable : ITable
    {
        private readonly SqliteConnection connection;

        public ModifyMutedTable(SqliteConnection connection) => this.connection = connection;

        public async Task InitAsync()
        {
            using SqliteCommand cmd = new("CREATE TABLE IF NOT EXISTS ModifyMuted (guild_id TEXT PRIMARY KEY);", connection);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task AddModifyMutedAsync(SocketGuild g)
        {
            string add = "INSERT INTO ModifyMuted (guild_id) SELECT @guild_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM ModifyMuted WHERE guild_id = @guild_id);";

            using SqliteCommand cmd = new(add, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task RemoveModifyMutedAsync(SocketGuild g)
        {
            string add = "DELETE FROM ModifyMuted WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(add, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task<bool> GetModifyMutedAsync(SocketGuild g)
        {
            bool modify;

            string add = "SELECT * FROM ModifyMuted WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(add, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            SqliteDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            modify = await reader.ReadAsync().ConfigureAwait(false);
            reader.Close();

            return modify;
        }
    }
}
