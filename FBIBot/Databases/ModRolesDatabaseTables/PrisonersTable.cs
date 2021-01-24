using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Databases.ModRolesDatabaseTables
{
    public class PrisonersTable : ITable
    {
        private readonly SqliteConnection connection;

        public PrisonersTable(SqliteConnection connection) => this.connection = connection;

        public Task InitAsync()
        {
            using SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS Prisoners (guild_id TEXT NOT NULL, user_id TEXT NOT NULL, UNIQUE (guild_id, user_id));", connection);
            return cmd.ExecuteNonQueryAsync();
        }

        public async Task RecordPrisonerAsync(SocketGuildUser user)
        {
            string insert = "INSERT INTO Prisoners (guild_id, user_id) SELECT @guild_id, @user_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM Prisoners WHERE guild_id = @guild_id AND user_id = @user_id);";
            using (SqliteCommand cmd = new SqliteCommand(insert, connection))
            {
                cmd.Parameters.AddWithValue("@guild_id", user.Guild.Id.ToString());
                cmd.Parameters.AddWithValue("@user_id", user.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<bool> HasPrisoners(SocketGuild g)
        {
            bool hasPrisoners = false;

            string getUsers = "SELECT * FROM Prisoners WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getUsers, connection))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                hasPrisoners = await reader.ReadAsync();
                reader.Close();
            }

            return hasPrisoners;
        }

        public async Task RemovePrisonerAsync(SocketGuildUser user)
        {
            string delete = "DELETE FROM Prisoners WHERE guild_id = @guild_id AND user_id = @user_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, connection))
            {
                cmd.Parameters.AddWithValue("@guild_id", user.Guild.Id.ToString());
                cmd.Parameters.AddWithValue("@user_id", user.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
