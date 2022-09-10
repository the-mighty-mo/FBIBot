using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Databases.VerificationDatabaseTables
{
    public class CaptchaTable : ITable
    {
        private readonly SqliteConnection connection;

        public CaptchaTable(SqliteConnection connection) => this.connection = connection;

        public Task InitAsync()
        {
            using SqliteCommand cmd = new("CREATE TABLE IF NOT EXISTS Captcha (user_id TEXT PRIMARY KEY, captcha TEXT NOT NULL);", connection);
            return cmd.ExecuteNonQueryAsync();
        }

        public async Task<string?> GetCaptchaAsync(SocketUser u)
        {
            string? captcha = null;

            string read = "SELECT captcha FROM Captcha WHERE user_id = @user_id;";

            using SqliteCommand cmd = new(read, connection);
            cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());

            SqliteDataReader reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                captcha = (string)reader["captcha"];
            }
            reader.Close();

            return captcha;
        }

        public async Task SetCaptchaAsync(string captcha, SocketUser u)
        {
            string update = "UPDATE Captcha SET captcha = @captcha WHERE user_id = @user_id;";
            string insert = "INSERT INTO Captcha (user_id, captcha) SELECT @user_id, @captcha WHERE (SELECT Changes() = 0);";

            using SqliteCommand cmd = new(update + insert, connection);
            cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());
            cmd.Parameters.AddWithValue("@captcha", captcha);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task RemoveCaptchaAsync(SocketUser u)
        {
            string delete = "DELETE FROM Captcha WHERE user_id = @user_id;";

            using SqliteCommand cmd = new(delete, connection);
            cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
