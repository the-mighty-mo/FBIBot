﻿using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Databases.VerificationDatabaseTables
{
    public class AttemptsTable : ITable
    {
        private readonly SqliteConnection connection;

        public AttemptsTable(SqliteConnection connection) => this.connection = connection;

        public Task InitAsync()
        {
            using SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS Attempts (user_id TEXT PRIMARY KEY, attempts INTEGER NOT NULL);", connection);
            return cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> GetAttemptsAsync(SocketUser u)
        {
            int attempts = 0;

            string read = "SELECT attempts FROM Attempts WHERE user_id = @user_id;";
            using (SqliteCommand cmd = new SqliteCommand(read, connection))
            {
                cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());

                SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    attempts = int.Parse(reader["attempts"].ToString()!);
                }
                reader.Close();
            }

            return attempts;
        }

        public async Task SetAttemptsAsync(SocketUser u, int attempts)
        {
            string update = "UPDATE Attempts SET attempts = @attempts WHERE user_id = @user_id;";
            string insert = "INSERT INTO Attempts (user_id, attempts) SELECT @user_id, @attempts WHERE (SELECT Changes() = 0);\n";

            using (SqliteCommand cmd = new SqliteCommand(update + insert, connection))
            {
                cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());
                cmd.Parameters.AddWithValue("@attempts", attempts);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task RemoveAttemptsAsync(SocketUser u)
        {
            string delete = "DELETE FROM Attempts WHERE user_id = @user_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, connection))
            {
                cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
