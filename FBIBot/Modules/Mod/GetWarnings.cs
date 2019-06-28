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
        public async Task GetWarnsAsync(SocketGuildUser user)
        {
            SocketGuildUser u = Context.Guild.GetUser(Context.User.Id);
            if (!await VerifyUser.IsMod(u))
            {
                await Context.Channel.SendMessageAsync("You are not a local director of the FBI and cannot use this command.");
                return;
            }

            List<ulong> ids = await GetWarningsAsync(Context.Guild);
            if (ids.Count == 0)
            {
                await Context.Channel.SendMessageAsync("Our security team has informed us that the given user does not have any warnings.");
                return;
            }

            await Context.Channel.SendMessageAsync($"{user.Nickname ?? user.Username} has {ids.Count} warnings.\n" +
                $"For more information, view mod logs: {string.Join(", ", ids)}");
        }

        [Command("getwarnings")]
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

        public static async Task<List<ulong>> GetWarningsAsync(SocketGuild g)
        {
            List<ulong> ids = new List<ulong>();

            string getWarns = "SELECT id FROM Warnings WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getWarns, Program.cnModLogs))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (ulong.TryParse(reader["id"].ToString(), out ulong id))
                    {
                        ids.Add(id);
                    }
                }
            }

            return await Task.Run(() => ids);
        }
    }
}
