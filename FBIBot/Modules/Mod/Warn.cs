using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class Warn : ModuleBase<SocketCommandContext>
    {
        [Command("warn")]
        public async Task WarnAsync(SocketGuildUser user, string length = null, [Remainder] string reason = null)
        {
            SocketGuildUser u = Context.Guild.GetUser(Context.User.Id);
            if (!await VerifyUser.IsMod(u))
            {
                await Context.Channel.SendMessageAsync("You are not a local director of the FBI and cannot use this command.");
                return;
            }

            await Context.Channel.SendMessageAsync($"{user.Mention} stop protesting capitalism." +
                $"{(reason != null ? $"\nThe reason: {reason}" : "")}");
            ulong id = await SendToModLog.SendToModLogAsync(SendToModLog.LogType.Warn, Context.User, user, null, reason);
            await AddWarningAsync(user, id);

            if (double.TryParse(length, out double hours))
            {
                await Task.Delay((int)(hours * 60 * 60 * 1000));

                if (!await RemoveWarning.GetWarningAsync(user, id))
                {
                    return;
                }

                await SendToModLog.SendToModLogAsync(SendToModLog.LogType.RemoveWarn, Context.Client.CurrentUser, user, id.ToString());
                await RemoveWarning.RemoveWarningAsync(user, id);
            }
        }

        [Command("warn")]
        public async Task WarnAsync(string user, string length = null, [Remainder] string reason = null)
        {
            SocketGuildUser u;
            if (ulong.TryParse(user, out ulong userID) && (u = Context.Guild.GetUser(userID)) != null)
            {
                await WarnAsync(u, length, reason);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given user does not exist.");
        }

        public static async Task AddWarningAsync(SocketGuildUser u, ulong id)
        {
            string addWarning = "INSERT INTO Warnings (guild_id, id, user_id) SELECT @guild_id, @id, @user_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM Warnings WHERE guild_id = @guild_id AND id = @id AND user_id = @user_id);";
            using (SqliteCommand cmd = new SqliteCommand(addWarning, Program.cnModLogs))
            {
                cmd.Parameters.AddWithValue("@guild_id", u.Guild.Id.ToString());
                cmd.Parameters.AddWithValue("@id", id.ToString());
                cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
