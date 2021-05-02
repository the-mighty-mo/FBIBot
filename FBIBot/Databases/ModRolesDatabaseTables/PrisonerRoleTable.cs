using Discord;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Databases.ModRolesDatabaseTables
{
    public class PrisonerRoleTable : ITable
    {
        private readonly SqliteConnection connection;

        public PrisonerRoleTable(SqliteConnection connection) => this.connection = connection;

        public Task InitAsync()
        {
            using SqliteCommand cmd = new("CREATE TABLE IF NOT EXISTS PrisonerRole (guild_id TEXT PRIMARY KEY, role_id TEXT NOT NULL);", connection);
            return cmd.ExecuteNonQueryAsync();
        }

        public async Task<SocketRole> GetPrisonerRoleAsync(SocketGuild g)
        {
            SocketRole role = null;

            string getRole = "SELECT role_id FROM PrisonerRole WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(getRole, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            SqliteDataReader reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                _ = ulong.TryParse(reader["role_id"].ToString(), out ulong roleID);
                role = g.GetRole(roleID);
            }
            reader.Close();

            return role;
        }

        public async Task SetPrisonerRoleAsync(IRole role)
        {
            string update = "UPDATE PrisonerRole SET role_id = @role_id WHERE guild_id = @guild_id;";
            string insert = "INSERT INTO PrisonerRole (guild_id, role_id) SELECT @guild_id, @role_id WHERE (SELECT Changes() = 0);";

            using SqliteCommand cmd = new(update + insert, connection);
            cmd.Parameters.AddWithValue("@guild_id", role.Guild.Id.ToString());
            cmd.Parameters.AddWithValue("@role_id", role.Id.ToString());

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task RemovePrisonerRoleAsync(SocketGuild g)
        {
            string delete = "DELETE FROM PrisonerRole WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(delete, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
