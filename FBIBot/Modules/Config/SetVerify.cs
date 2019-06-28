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
        public async Task SetVerifyAsync()
        {
            SocketGuildUser u = Context.Guild.GetUser(Context.User.Id);
            if (!await VerifyUser.IsAdmin(u))
            {
                await Context.Channel.SendMessageAsync("You are not a local director of the FBI and cannot use this command.");
                return;
            }

            if (await GetVerificationRoleAsync(Context.Guild) == null)
            {
                await Context.Channel.SendMessageAsync("Our customs team has informed us that you already don't have a citizenship check.");
                return;
            }

            await RemoveVerificationRoleAsync(Context.Guild);
            await Context.Channel.SendMessageAsync("Citizenship will now go unchecked. This could go very poorly.");
        }

        [Command("setverify")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task SetVerifyAsync(SocketRole role)
        {
            SocketGuildUser u = Context.Guild.GetUser(Context.User.Id);
            if (!await VerifyUser.IsAdmin(u))
            {
                await Context.Channel.SendMessageAsync("You are not a local director of the FBI and cannot use this command.");
                return;
            }

            if (await GetVerificationRoleAsync(Context.Guild) == role)
            {
                await Context.Channel.SendMessageAsync($"Our customs team has informed us that all patriotic citizens already receive the {role.Name} role.");
                return;
            }

            await SetVerificationRoleAsync(role);
            await Context.Channel.SendMessageAsync($"All proud Americans will now receive the {role.Name} role.");
        }

        [Command("setverify")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task SetVerifyAsync(string role)
        {
            SocketRole r;
            if (ulong.TryParse(role, out ulong roleID) && (r = Context.Guild.GetRole(roleID)) != null)
            {
                await SetVerifyAsync(r);
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

        public static async Task SetVerificationRoleAsync(SocketRole role)
        {
            string update = "UPDATE Roles SET role_id = @role_id WHERE guild_id = @guild_id;";
            string insert = "INSERT INTO Roles (guild_id, role_id) SELECT @guild_id, @role_id WHERE (SELECT Changes() = 0);";

            using (SqliteCommand cmd = new SqliteCommand(update + insert, Program.cnVerify))
            {
                cmd.Parameters.AddWithValue("@guild_id", role.Guild.Id.ToString());
                cmd.Parameters.AddWithValue("@role_id", role.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task RemoveVerificationRoleAsync(SocketGuild g)
        {
            string delete = "DELETE FROM Roles WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, Program.cnVerify))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
