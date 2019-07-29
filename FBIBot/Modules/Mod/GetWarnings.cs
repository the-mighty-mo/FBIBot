using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class GetWarnings : ModuleBase<SocketCommandContext>
    {
        [Command("getwarnings")]
        [Alias("get-warnings")]
        [RequireMod]
        public async Task GetWarnsAsync(SocketGuildUser user)
        {
            List<ulong> ids = await GetWarningsAsync(user);
            if (ids.Count == 0)
            {
                await Context.Channel.SendMessageAsync("Our security team has informed us that the given user does not have any warnings.");
                return;
            }

            await Context.Channel.SendMessageAsync($"{user.Nickname ?? user.Username} has {ids.Count} warnings.\n" +
                $"For more information, view mod logs: {string.Join(", ", ids)}");
        }

        [Command("getwarnings")]
        [Alias("get-warnings")]
        [RequireMod]
        public async Task GetWarnsAsync(string user)
        {
            SocketGuildUser u;
            if (ulong.TryParse(user, out ulong userID) && (u = Context.Guild.GetUser(userID)) != null)
            {
                await GetWarnsAsync(u);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given user does not exist.");
        }

        public static async Task<List<ulong>> GetWarningsAsync(SocketGuildUser u)
        {
            List<ulong> ids = new List<ulong>();

            string getWarns = "SELECT id FROM Warnings WHERE guild_id = @guild_id AND user_id = @user_id;";
            using (SqliteCommand cmd = new SqliteCommand(getWarns, Program.cnModLogs))
            {
                cmd.Parameters.AddWithValue("@guild_id", u.Guild.Id.ToString());
                cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());

                SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    if (ulong.TryParse(reader["id"].ToString(), out ulong id))
                    {
                        ids.Add(id);
                    }
                }
                reader.Close();
            }

            return ids;
        }
    }
}
