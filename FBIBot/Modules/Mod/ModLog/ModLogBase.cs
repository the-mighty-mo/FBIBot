using Discord;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod.ModLog
{
    public static class ModLogBase
    {
        public static async Task<bool> SetReasonAsync(SocketGuild g, ulong id, string reason = null)
        {
            IUserMessage msg = await GetModLogAsync(g, id);
            if (msg == null || msg.Embeds.Count == 0)
            {
                return false;
            }
            EmbedBuilder e = msg.Embeds.ToList()[0].ToEmbedBuilder();
            List<EmbedFieldBuilder> fields = e.Fields.ToList();

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(e.Color ?? SecurityInfo.botColor)
                .WithTitle($"Federal Bureau of Investigation - Log {id}")
                .WithCurrentTimestamp();

            EmbedFieldBuilder field = e.Fields.FirstOrDefault(x => x.Name == "Reason");
            EmbedFieldBuilder f = e.Fields.FirstOrDefault(x => x.Name.Contains("Arrest User"));
            if (field == null || f != null)
            {
                return false;
            }
            fields.Remove(field);

            field.WithValue(reason ?? "(none given)");
            fields.Add(field);
            embed.WithFields(fields);

            await msg.ModifyAsync(x => x.Embed = embed.Build());

            return true;
        }

        public static async Task<ulong> GetNextModLogID(SocketGuild g)
        {
            ulong id = 0;

            string getID = "SELECT MAX(CAST(id AS INTEGER)) AS id FROM ModLogs WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getID, Program.cnModLogs))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    ulong.TryParse(reader["id"].ToString(), out id);
                }
                reader.Close();
            }

            return ++id;
        }

        public static async Task<IUserMessage> GetModLogAsync(SocketGuild g, ulong id)
        {
            IUserMessage msg = null;

            string getMessage = "SELECT channel_id, message_id FROM ModLogs WHERE guild_id = @guild_id AND id = @id;";
            using (SqliteCommand cmd = new SqliteCommand(getMessage, Program.cnModLogs))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                cmd.Parameters.AddWithValue("@id", id.ToString());

                SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    ulong.TryParse(reader["channel_id"].ToString(), out ulong channelID);
                    ulong.TryParse(reader["message_id"].ToString(), out ulong messageID);

                    msg = await g.GetTextChannel(channelID)?.GetMessageAsync(messageID) as IUserMessage;
                }
                reader.Close();
            }

            return msg;
        }

        public static async Task SaveModLogAsync(IUserMessage msg, SocketGuild g, ulong id)
        {
            string insert = "INSERT INTO ModLogs (guild_id, id, channel_id, message_id) SELECT @guild_id, @id, @channel_id, @message_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM ModLogs WHERE guild_id = @guild_id AND id = @id);";
            using (SqliteCommand cmd = new SqliteCommand(insert, Program.cnModLogs))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                cmd.Parameters.AddWithValue("@id", id.ToString());
                cmd.Parameters.AddWithValue("@channel_id", msg.Channel.Id.ToString());
                cmd.Parameters.AddWithValue("@message_id", msg.Id.ToString());

                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task RemoveModLogsAsync(SocketGuild g)
        {
            string delete = "DELETE FROM ModLogs WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, Program.cnModLogs))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
