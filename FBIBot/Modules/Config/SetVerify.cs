using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Modules.Config
{
    public class SetVerify : ModuleBase<SocketCommandContext>
    {
        [Command("setverify")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireOwner()]
        public async Task VerifyRoleAsync(SocketRole role)
        {
            await SetVerificationRoleAsync(role.Id);
            await Context.Channel.SendMessageAsync($"All proud Americans will now receive the {role.Name} role.");
        }

        [Command("setverify")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireOwner()]
        public async Task VerifyRoleAsync(string role)
        {
            SocketRole r;
            if (ulong.TryParse(role, out ulong roleID) && (r = Context.Guild.GetRole(roleID)) != null)
            {
                await VerifyRoleAsync(r);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence tells us the given role does not exist.");
        }

        public static async Task<SocketRole> GetVerificationRoleAsync(SocketGuild g)
        {
            SocketRole role = null;

            string getRole = "SELECT role_id FROM Roles WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getRole, Program.cnVerify))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id);

                SqliteDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    ulong roleID = ulong.Parse(reader["role_id"].ToString());
                    role = g.GetRole(roleID);
                }
                reader.Close();
            }

            return await Task.Run(() => role);
        }

        async Task SetVerificationRoleAsync(ulong role)
        {
            string update = "UPDATE Roles SET role_id = @role_id WHERE guild_id = @guild_id;";
            string insert = "INSERT INTO Roles (guild_id, role_id) SELECT @guild_id, @role_id WHERE (Select Changes() = 0);";

            using (SqliteCommand cmd = new SqliteCommand(update + insert, Program.cnVerify))
            {
                cmd.Parameters.AddWithValue("@guild_id", Context.Guild.Id.ToString());
                cmd.Parameters.AddWithValue("@role_id", role.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
