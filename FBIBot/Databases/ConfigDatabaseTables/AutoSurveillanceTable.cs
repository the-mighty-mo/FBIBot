﻿using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Databases.ConfigDatabaseTables
{
    public class AutoSurveillanceTable : ITable
    {
        private readonly SqliteConnection connection;

        public AutoSurveillanceTable(SqliteConnection connection) => this.connection = connection;

        public async Task InitAsync()
        {
            using SqliteCommand cmd = new("CREATE TABLE IF NOT EXISTS AutoSurveillance (guild_id TEXT PRIMARY KEY);", connection);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task<bool> GetAutoSurveillanceAsync(SocketGuild g)
        {
            bool isAutoSurveillance;

            string getSurveillance = "SELECT guild_id FROM AutoSurveillance WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(getSurveillance, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            SqliteDataReader reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            isAutoSurveillance = await reader.ReadAsync().ConfigureAwait(false);
            reader.Close();

            return isAutoSurveillance;
        }

        public async Task SetAutoSurveillanceAsync(SocketGuild g)
        {
            string insert = "INSERT INTO AutoSurveillance (guild_id) SELECT @guild_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM AutoSurveillance WHERE guild_id = @guild_id);";

            using SqliteCommand cmd = new(insert, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task RemoveAutoSurveillanceAsync(SocketGuild g)
        {
            string delete = "DELETE FROM AutoSurveillance WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(delete, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }
}
