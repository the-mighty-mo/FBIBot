using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Modules.Config
{
    public class AddMod : ModuleBase<SocketCommandContext>
    {
        [Command("add-mod")]
        [Alias("addmod")]
        [RequireAdmin]
        public async Task AddModAsync(SocketRole role)
        {
            if ((await GetModRolesAsync(Context.Guild)).Contains(role))
            {
                await Context.Channel.SendMessageAsync($"Members with the {role.Name} role are already assisting the FBI.");
                return;
            }

            await AddModRoleAsync(role);
            if ((await AddAdmin.GetAdminRolesAsync(Context.Guild)).Contains(role))
            {
                await Task.WhenAll
                (
                    RemoveAdminRole.RemoveAdminAsync(role),
                    Context.Channel.SendMessageAsync($"Members with the {role.Name} role have been demoted to assistants of the agency.")
                );
            }
            else
            {
                await Context.Channel.SendMessageAsync($"Members with the {role.Name} role may now assist our agents in ensuring freedom, democracy, and justice for all.");
            }
        }

        [Command("add-mod")]
        [Alias("addmod")]
        [RequireAdmin]
        public async Task AddModAsync(string role)
        {
            SocketRole r;
            if (ulong.TryParse(role, out ulong roleID) && (r = Context.Guild.GetRole(roleID)) != null)
            {
                await AddModAsync(r);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given role does not exist.");
        }

        public static async Task<List<SocketRole>> GetModRolesAsync(SocketGuild g)
        {
            List<SocketRole> roles = new List<SocketRole>();

            string getRoles = "SELECT role_id FROM Mods WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getRoles, Program.cnModRoles))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    ulong roleID = ulong.Parse(reader["role_id"].ToString());
                    SocketRole role = g.GetRole(roleID);
                    if (role != null)
                    {
                        roles.Add(role);
                    }
                }
                reader.Close();
            }

            return roles;
        }

        public static async Task AddModRoleAsync(SocketRole role)
        {
            string insert = "INSERT INTO Mods (guild_id, role_id) SELECT @guild_id, @role_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM Mods WHERE guild_id = @guild_id AND role_id = @role_id);";

            using (SqliteCommand cmd = new SqliteCommand(insert, Program.cnModRoles))
            {
                cmd.Parameters.AddWithValue("@guild_id", role.Guild.Id.ToString());
                cmd.Parameters.AddWithValue("@role_id", role.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
