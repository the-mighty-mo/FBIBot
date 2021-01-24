using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Databases.VerificationDatabaseTables
{
    public class VerifiedTable : ITable
    {
        private readonly SqliteConnection connection;

        public VerifiedTable(SqliteConnection connection) => this.connection = connection;

        public Task InitAsync()
        {
            using SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS Verified (user_id TEXT PRIMARY KEY);", connection);
            return cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> GetVerifiedAsync(SocketUser u)
        {
            bool isVerified = false;

            string verify = "SELECT user_id FROM Verified WHERE user_id = @user_id;";
            using (SqliteCommand cmd = new SqliteCommand(verify, connection))
            {
                cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());

                SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                isVerified = await reader.ReadAsync();
                reader.Close();
            }

            return isVerified;
        }

        public async Task SetVerifiedAsync(SocketUser u)
        {
            await new CaptchaTable(connection).RemoveCaptchaAsync(u);

            string verify = "INSERT INTO Verified (user_id) SELECT @user_id WHERE NOT EXISTS (SELECT 1 FROM Verified WHERE user_id = @user_id);";
            using (SqliteCommand cmd = new SqliteCommand(verify, connection))
            {
                cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task RemoveVerifiedAsync(SocketUser u)
        {
            string delete = "DELETE FROM Verified WHERE user_id = @user_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, connection))
            {
                cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
