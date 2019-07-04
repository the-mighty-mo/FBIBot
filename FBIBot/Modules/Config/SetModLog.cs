using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Modules.Config
{
    public class SetModLog : ModuleBase<SocketCommandContext>
    {
        [Command("setmodlog")]
        [RequireAdmin]
        public async Task SetModLogAsync()
        {
            if (await GetModLogChannelAsync(Context.Guild) == null)
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that you are already lacking a mod log channel.");
                return;
            }

            await RemoveModLogChannelAsync(Context.Guild);
            await Context.Channel.SendMessageAsync("Moderation logs will now go undisclosed. That information was confidential, anyways.");
        }

        [Command("setmodlog")]
        [RequireAdmin]
        public async Task SetModLogAsync(SocketTextChannel channel)
        {
            SocketGuildUser u = Context.Guild.GetUser(Context.User.Id);
            if (!await VerifyUser.IsAdmin(u))
            {
                await Context.Channel.SendMessageAsync("You are not a local director of the FBI and cannot use this command.");
                return;
            }

            if (await GetModLogChannelAsync(Context.Guild) == channel)
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that {channel.Mention} is already configured for mod logs.");
                return;
            }

            await SetModLogChannelAsync(channel);
            await Context.Channel.SendMessageAsync($"Once-confidential moderation logs will now be disclosed to {channel.Mention}.");
        }

        [Command("setmodlog")]
        public async Task SetModLogAsync(string channel)
        {
            SocketTextChannel c;
            if (ulong.TryParse(channel, out ulong channelID) && (c = Context.Guild.GetTextChannel(channelID)) != null)
            {
                await SetModLogAsync(c);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given text channel does not exist.");
        }

        public static async Task<SocketTextChannel> GetModLogChannelAsync(SocketGuild g)
        {
            SocketTextChannel channel = null;

            string getChannel = "SELECT channel_id FROM ModLogChannel WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getChannel, Program.cnModLogs))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    ulong.TryParse(reader["channel_id"].ToString(), out ulong channelID);
                    channel = g.GetTextChannel(channelID);
                }
                reader.Close();
            }

            return await Task.Run(() => channel);
        }

        public static async Task SetModLogChannelAsync(SocketTextChannel channel)
        {
            string update = "UPDATE ModLogChannel SET channel_id = @channel_id WHERE guild_id = @guild_id;";
            string insert = "INSERT INTO ModLogChannel (guild_id, channel_id) SELECT @guild_id, @channel_id WHERE (SELECT Changes() = 0);";

            using (SqliteCommand cmd = new SqliteCommand(update + insert, Program.cnModLogs))
            {
                cmd.Parameters.AddWithValue("@guild_id", channel.Guild.Id.ToString());
                cmd.Parameters.AddWithValue("@channel_id", channel.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task RemoveModLogChannelAsync(SocketGuild g)
        {
            string delete = "DELETE FROM ModLogChannel WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, Program.cnModLogs))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
