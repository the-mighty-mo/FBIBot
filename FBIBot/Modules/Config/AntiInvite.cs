using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Modules.Config
{
    public class AntiInvite : ModuleBase<SocketCommandContext>
    {
        [Command("anti-invite")]
        [Alias("antiinvite")]
        [RequireAdmin]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task AntiInviteAsync(string enable)
        {
            bool isEnable = enable == "true" || enable == "enable";
            bool isEnabled = await GetAntiInviteAsync(Context.Guild);

            if (isEnable == isEnabled)
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that anti-invite is already {(isEnabled ? "enabled" : "disabled")}.");
                return;
            }

            List<Task> cmds = new List<Task>()
            {
                Context.Channel.SendMessageAsync($"We are now {(isEnable ? "permitted to remove" : "prohibited from removing")} invitations to the socialist party.")
            };
            if (isEnable)
            {
                cmds.Add(SetAntiInviteAsync(Context.Guild));
            }
            else
            {
                cmds.Add(RemoveAntiInviteAsync(Context.Guild));
            }

            await Task.WhenAll(cmds);
        }

        public static async Task<bool> GetAntiInviteAsync(SocketGuild g)
        {
            bool isAntiInvite = false;

            string getInvite = "SELECT guild_id FROM AntiInvite WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getInvite, Program.cnConfig))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                isAntiInvite = await reader.ReadAsync();
                reader.Close();
            }

            return isAntiInvite;
        }

        public static async Task SetAntiInviteAsync(SocketGuild g)
        {
            string insert = "INSERT INTO AntiInvite (guild_id) SELECT @guild_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM AntiInvite WHERE guild_id = @guild_id);";

            using (SqliteCommand cmd = new SqliteCommand(insert, Program.cnConfig))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task RemoveAntiInviteAsync(SocketGuild g)
        {
            string delete = "DELETE FROM AntiInvite WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, Program.cnConfig))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
