using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FBIBot.Modules.Config
{
    public class VerifyRole : ModuleBase<SocketCommandContext>
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

        async Task SetVerificationRoleAsync(ulong role)
        {
            string createView = "CREATE VIEW IF NOT EXISTS roleupdate AS SELECT guild_id, role_id FROM Roles;";
            string createTrigger = "CREATE TRIGGER IF NOT EXISTS updaterole INSTEAD OF INSERT ON roleupdate\n" +
                "BEGIN\n" +
                "UPDATE Roles SET role_id = NEW.role_id WHERE guild_id = NEW.guild_id;\n" +
                "INSERT INTO Roles (guild_id, role_id) SELECT NEW.guild_id, NEW.role_id WHERE (Select Changes() = 0);\n" +
                "END;";
            string insert = "INSERT INTO roleupdate (guild_id, role_id) VALUES (@guild_id, @role_id);";
            string drop = "DROP TRIGGER updaterole; DROP VIEW roleupdate;";

            using (SqliteCommand cmd = new SqliteCommand(createView + createTrigger + insert + drop, Program.cnVerify))
            {
                cmd.Parameters.AddWithValue("@guild_id", Context.Guild.Id);
                cmd.Parameters.AddWithValue("@role_id", role);
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
