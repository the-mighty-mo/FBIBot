using Discord;
using Discord.WebSocket;
using FBIBot.Modules.Config;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod.ModLog
{
    public static class ModLogBase
    {
        public class ModLogInfo
        {
            public class RequiredInfo
            {
                public readonly SocketGuildUser invoker;
                public readonly Color color;
                public readonly string commandName;
                public readonly string commandValue;

                public RequiredInfo(SocketGuildUser invoker, Color color, string commandName, string commandValue)
                {
                    this.invoker = invoker;
                    this.color = color;
                    this.commandName = commandName;
                    this.commandValue = commandValue;
                }
            }

            public class ReasonInfo
            {
                public readonly string reason;

                public ReasonInfo(string reason = null)
                {
                    this.reason = reason;
                }
            }

            public SocketGuildUser Invoker { get; }
            public Color Color { get; }
            public string CommandName { get; }
            public string CommandValue { get; }

            public bool HasReasonField { get; }

            public string Reason { get; }

            public ModLogInfo(RequiredInfo info, ReasonInfo reasonInfo = null)
            {
                Invoker = info.invoker;
                Color = info.color;
                CommandName = info.commandName;
                CommandValue = info.commandValue;

                HasReasonField = reasonInfo != null;
                Reason = reasonInfo?.reason;
            }
        }

        public static async Task SendToModLogAsync(ModLogInfo info)
        {
            ulong id = await GetNextModLogID(info.Invoker.Guild);
            SocketTextChannel channel = await SetModLog.GetModLogChannelAsync(info.Invoker.Guild);

            if (channel == null)
            {
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(info.Color)
                .WithTitle($"Federal Bureau of Investigation - Log {id}")
                .WithCurrentTimestamp();

            EmbedFieldBuilder command = new EmbedFieldBuilder()
                .WithIsInline(false)
                .WithName(info.CommandName)
                .WithValue(info.CommandValue);
            embed.AddField(command);

            EmbedFieldBuilder invoked = new EmbedFieldBuilder()
                .WithIsInline(false)
                .WithName("Invoked by")
                .WithValue(info.Invoker.Mention);
            embed.AddField(invoked);

            if (info.HasReasonField)
            {
                EmbedFieldBuilder field = new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName("Reason")
                    .WithValue(info.Reason ?? "(none given)");
                embed.AddField(field);
            }

            var msg = await channel.SendMessageAsync("", false, embed.Build());
            if (msg != null)
            {
                await SaveModLogAsync(msg, info.Invoker.Guild, id);
            }
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
