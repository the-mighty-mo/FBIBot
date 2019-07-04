using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class RemoveWarnings : ModuleBase<SocketCommandContext>
    {
        [Command("removewarnings")]
        [RequireMod]
        public async Task RemoveWarnsAsync([RequireInvokerHierarchy("remove warnings from")] SocketGuildUser user, string count = null)
        {
            if (count != null && !int.TryParse(count, out int _))
            {
                await Context.Channel.SendMessageAsync($"Our intelligence team has informed us that {count} is not a valid number of warnings.");
                return;
            }
            if (count == "0")
            {
                await Context.Channel.SendMessageAsync("Why are you trying to remove ***0*** warnings? I have more important things to do.");
                return;
            }
            if ((await GetWarnings.GetWarningsAsync(user)).Count == 0)
            {
                await Context.Channel.SendMessageAsync("Our security team has informed us that the given user does not have any warnings.");
                return;
            }

            await RemoveWarningsAsync(user, count);
            await Context.Channel.SendMessageAsync($"We have removed {count ?? "all"} warnings for {user.Mention}.");
            await SendToModLog.SendToModLogAsync(SendToModLog.LogType.RemoveWarns, Context.User, user, count);
        }

        [Command("removewarnings")]
        [RequireMod]
        public async Task RemoveWarnsAsync(string user, string count = null)
        {
            SocketGuildUser u;
            if (ulong.TryParse(user, out ulong userID) && (u = Context.Guild.GetUser(userID)) != null)
            {
                await RemoveWarnsAsync(u, count);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given user does not exist.");
        }

        public static async Task RemoveWarningsAsync(SocketGuildUser u, string count = null)
        {
            string delete = "DELETE FROM Warnings WHERE guild_id = @guild_id AND user_id = @user_id";
            if (count != null)
            {
                delete += " AND id IN (SELECT id FROM Warnings WHERE guild_id = @guild_id AND user_id = @user_id ORDER BY CAST(id AS INTEGER) ASC LIMIT @count)";
            }
            delete += ";";

            using (SqliteCommand cmd = new SqliteCommand(delete, Program.cnModLogs))
            {
                cmd.Parameters.AddWithValue("@guild_id", u.Guild.Id.ToString());
                cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());
                cmd.Parameters.AddWithValue("@count", count);

                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task RemoveAllWarningsAsync(SocketGuild g)
        {
            string delete = "DELETE FROM Warnings WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, Program.cnModLogs))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
