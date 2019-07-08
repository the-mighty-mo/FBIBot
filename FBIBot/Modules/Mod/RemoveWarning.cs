using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class RemoveWarning : ModuleBase<SocketCommandContext>
    {
        [Command("removewarning")]
        [Alias("remove-warning")]
        [RequireMod]
        public async Task RemoveWarnAsync([RequireInvokerHierarchy("remove warnings from")] SocketGuildUser user, string id)
        {
            if (!ulong.TryParse(id, out ulong ID))
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that {id} is not a valid Mod Log ID.");
                return;
            }

            if (!await GetWarningAsync(user, ID))
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that the given warning does not exist.");
                return;
            }

            await RemoveWarningAsync(user, ID);
            await Context.Channel.SendMessageAsync("Warning pardoned.");
            await SendToModLog.SendToModLogAsync(SendToModLog.LogType.RemoveWarn, Context.User as SocketGuildUser, user, id.ToString());
        }

        [Command("removewarning")]
        [Alias("remove-warning")]
        [RequireMod]
        public async Task RemoveWarnAsync(string user, string id)
        {
            SocketGuildUser u;
            if (ulong.TryParse(user, out ulong userID) && (u = Context.Guild.GetUser(userID)) != null)
            {
                await RemoveWarnAsync(u, id);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given user does not exist.");
        }

        public static async Task<bool> GetWarningAsync(SocketGuildUser u, ulong id)
        {
            bool hasWarning = false;

            string getWarning = "SELECT * FROM Warnings WHERE guild_id = @guild_id AND id = @id AND user_id = @user_id;";
            using (SqliteCommand cmd = new SqliteCommand(getWarning, Program.cnModLogs))
            {
                cmd.Parameters.AddWithValue("@guild_id", u.Guild.Id.ToString());
                cmd.Parameters.AddWithValue("@id", id.ToString());
                cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());

                SqliteDataReader reader = cmd.ExecuteReader();
                hasWarning = reader.Read();
                reader.Close();
            }

            return await Task.Run(() => hasWarning);
        }

        public static async Task RemoveWarningAsync(SocketGuildUser u, ulong id)
        {
            string delete = "DELETE FROM Warnings WHERE guild_id = @guild_id AND id = @id AND user_id = @user_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, Program.cnModLogs))
            {
                cmd.Parameters.AddWithValue("@guild_id", u.Guild.Id.ToString());
                cmd.Parameters.AddWithValue("@id", id.ToString());
                cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
