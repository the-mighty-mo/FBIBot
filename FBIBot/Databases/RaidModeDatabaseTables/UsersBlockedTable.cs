﻿using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Databases.RaidModeDatabaseTables
{
    public class UsersBlockedTable : ITable
    {
        private readonly SqliteConnection connection;

        public UsersBlockedTable(SqliteConnection connection) => this.connection = connection;

        public Task InitAsync()
        {
            using SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS UsersBlocked (guild_id TEXT NOT NULL, user_id TEXT NOT NULL UNIQUE);", connection);
            return cmd.ExecuteNonQueryAsync();
        }

        public async Task AddBlockedUserAsync(SocketGuildUser u)
        {
            string insert = "INSERT INTO UsersBlocked (guild_id, user_id) SELECT @guild_id, @user_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM UsersBlocked WHERE guild_id = @guild_id AND user_id = @user_id);";

            using (SqliteCommand cmd = new SqliteCommand(insert, connection))
            {
                cmd.Parameters.AddWithValue("@guild_id", u.Guild.Id.ToString());
                cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());

                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<string>> GetBlockedUsersAsync(SocketGuild g)
        {
            List<string> blockedUsers = new List<string>();

            string getBlocked = "SELECT user_id FROM UsersBlocked WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getBlocked, connection))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    blockedUsers.Add((string)reader["user_id"]);
                }
                reader.Close();
            }

            return blockedUsers;
        }

        public async Task RemoveBlockedUsersAsync(SocketGuild g)
        {
            string delete = "DELETE FROM UsersBlocked WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, connection))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
