using Discord;
using Discord.WebSocket;
using FBIBot.Modules.Config;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class SendToModLog
    {
        public enum LogType
        {
            Verify,
            Warn,
            Mute,
            Arrest,
            Kick,
            Ban,
            Unverify,
            RemoveWarn,
            RemoveWarns,
            Unmute,
            Free,
            Unban
        }

        // Note: length for LogType.RemoveWarn is the Mod Log ID, and length for LogType.RemoveWarns is the number of removed warnings
        public static async Task<ulong> SendToModLogAsync(LogType t, SocketUser invoker, SocketGuildUser u, string length = null, string reason = null)
        {
            Color color;
            bool reasonAllowed = true;
            bool isTime = double.TryParse(length, out double time);
            string cmd = t.ToString();
            ulong id = await GetNextModLogID(u.Guild);

            switch (t)
            {
            case LogType.Verify:
                color = new Color(255, 255, 255);
                reasonAllowed = false;
                isTime = false;
                break;
            case LogType.Warn:
                color = new Color(228, 226, 24);
                if (isTime)
                {
                    length += $" {(time == 1 ? "hour" : "hours")}";
                }
                break;
            case LogType.Mute:
                color = new Color(255, 110, 24);
                if (isTime)
                {
                    length += $" {(time == 1 ? "minute" : "minutes")}";
                }
                break;
            case LogType.Arrest:
                color = new Color(255, 61, 24);
                reason = "*No reason necessary*";
                if (isTime)
                {
                    length += $" {(time == 1 ? "minute" : "minutes")}";
                }
                break;
            case LogType.Kick:
                color = new Color(255, 12, 12);
                break;
            case LogType.Ban:
                color = new Color(130, 0, 0);
                if (isTime)
                {
                    length += $" {(time == 1 ? "day" : "days")}";
                }
                break;
            case LogType.Unverify:
                color = new Color(0, 0, 0);
                isTime = false;
                break;
            case LogType.RemoveWarn:
                cmd = $"Remove Warning {length} from";
                goto default;
            case LogType.RemoveWarns:
                cmd = $"Remove {length ?? "All"} Warnings from";
                goto default;
            case LogType.Unmute:
            case LogType.Free:
            case LogType.Unban:
            default:
                color = new Color(12, 156, 24);
                reasonAllowed = false;
                isTime = false;
                break;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(color)
                .WithTitle($"Federal Bureau of Investigation - Log {id}")
                .WithCurrentTimestamp();

            EmbedFieldBuilder affected = new EmbedFieldBuilder()
                .WithIsInline(false)
                .WithName($"{cmd} User{(isTime ? $" for {length}" : "")}")
                .WithValue(u.Mention);
            embed.AddField(affected);

            EmbedFieldBuilder invoked = new EmbedFieldBuilder()
                .WithIsInline(false)
                .WithName("Invoked by")
                .WithValue(invoker.Mention);
            embed.AddField(invoked);

            if (reasonAllowed)
            {
                EmbedFieldBuilder field = new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName("Reason")
                    .WithValue(reason ?? "(none given)");
                embed.AddField(field);
            }

            var msg = await (await SetModLog.GetModLogChannelAsync(u.Guild))?.SendMessageAsync("", false, embed.Build());
            if (msg != null)
            {
                await SaveModLogAsync(msg, u.Guild, id);
            }

            return id;
        }

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

            return await Task.Run(() => true);
        }

        public static async Task<ulong> GetNextModLogID(SocketGuild g)
        {
            ulong id = 0;

            string getID = "SELECT MAX(CAST(id AS INTEGER)) AS id FROM ModLogs WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getID, Program.cnModLogs))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    ulong.TryParse(reader["id"].ToString(), out id);
                }
                reader.Close();
            }

            return await Task.Run(() => ++id);
        }

        public static async Task<IUserMessage> GetModLogAsync(SocketGuild g, ulong id)
        {
            IUserMessage msg = null;

            string getMessage = "SELECT channel_id, message_id FROM ModLogs WHERE guild_id = @guild_id AND id = @id;";
            using (SqliteCommand cmd = new SqliteCommand(getMessage, Program.cnModLogs))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                cmd.Parameters.AddWithValue("@id", id.ToString());

                SqliteDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    ulong.TryParse(reader["channel_id"].ToString(), out ulong channelID);
                    ulong.TryParse(reader["message_id"].ToString(), out ulong messageID);

                    msg = (await g.GetTextChannel(channelID)?.GetMessageAsync(messageID)) as IUserMessage;
                }
                reader.Close();
            }

            return await Task.Run(() => msg);
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
