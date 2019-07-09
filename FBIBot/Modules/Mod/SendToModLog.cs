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
            RaidMode,
            Verify,
            VerifyAll,
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

        class ModLogInfo
        {
            public LogType t;
            public string length;
            public string reason;

            public Color color;
            public bool reasonAllowed = true;
            public bool isTime;
            public bool hasTarget;
            public double time;
            public string cmd;
            public string state = "Executed";

            public ModLogInfo(LogType t, SocketGuildUser target, string length, string reason)
            {
                this.t = t;
                this.length = length;
                this.reason = reason;

                isTime = double.TryParse(length, out time);
                cmd = t.ToString();
                hasTarget = target != null;
            }

            public ModLogInfo(LogType t, string length, string reason)
            {
                this.t = t;
                this.length = length;
                this.reason = reason;

                isTime = double.TryParse(length, out time);
                cmd = t.ToString();
                hasTarget = true;
            }
        }

        // Note: length for LogType.RemoveWarn is the Mod Log ID, and length for LogType.RemoveWarns is the number of removed warnings
        public static async Task<ulong> SendToModLogAsync(LogType t, SocketGuildUser invoker, SocketGuildUser target, string length = null, string reason = null)
        {
            ModLogInfo info = new ModLogInfo(t, target, length, reason);
            LogTypeSwitch(ref info);

            return await SendToModLogAsync(info, invoker, target?.Id);
        }

        public static async Task<ulong> SendToModLogAsync(LogType t, SocketGuildUser invoker, ulong targetID, string length = null, string reason = null)
        {
            ModLogInfo info = new ModLogInfo(t, length, reason);
            LogTypeSwitch(ref info);

            return await SendToModLogAsync(info, invoker, targetID);
        }

        private static async Task<ulong> SendToModLogAsync(ModLogInfo info, SocketGuildUser invoker, ulong? targetID)
        {
            ulong id = await GetNextModLogID(invoker.Guild);

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(info.color)
                .WithTitle($"Federal Bureau of Investigation - Log {id}")
                .WithCurrentTimestamp();

            if (info.hasTarget)
            {
                EmbedFieldBuilder command = new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName($"{info.cmd} User{(info.isTime ? $" for {info.length}" : "")}")
                    .WithValue($"<@{targetID}>");
                embed.AddField(command);
            }
            else
            {
                EmbedFieldBuilder command = new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName($"{info.cmd}{(info.isTime ? $" for {info.length}" : "")}")
                    .WithValue(info.state);
                embed.AddField(command);
            }

            EmbedFieldBuilder invoked = new EmbedFieldBuilder()
                .WithIsInline(false)
                .WithName("Invoked by")
                .WithValue(invoker.Mention);
            embed.AddField(invoked);

            if (info.reasonAllowed)
            {
                EmbedFieldBuilder field = new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName("Reason")
                    .WithValue(info.reason ?? "(none given)");
                embed.AddField(field);
            }

            var msg = await (await SetModLog.GetModLogChannelAsync(invoker.Guild))?.SendMessageAsync("", false, embed.Build());
            if (msg != null)
            {
                await SaveModLogAsync(msg, invoker.Guild, id);
            }

            return id;
        }

        private static void LogTypeSwitch(ref ModLogInfo info)
        {
            switch (info.t)
            {
            case LogType.RaidMode:
                info.color = new Color(206, 15, 65);
                info.cmd = "Raid Mode";
                info.reasonAllowed = false;
                info.isTime = false;
                info.hasTarget = false;
                info.state = info.length;
                break;
            case LogType.Verify:
                info.color = new Color(255, 255, 255);
                info.reasonAllowed = false;
                info.isTime = false;
                break;
            case LogType.VerifyAll:
                info.color = new Color(255, 255, 255);
                info.cmd = "Verify All Users";
                info.reasonAllowed = false;
                info.isTime = false;
                info.hasTarget = false;
                info.state = "Executed";
                break;
            case LogType.Warn:
                info.color = new Color(255, 213, 31);
                if (info.isTime)
                {
                    info.length += $" {(info.time == 1 ? "hour" : "hours")}";
                }
                break;
            case LogType.Mute:
                info.color = new Color(255, 110, 24);
                if (info.isTime)
                {
                    info.length += $" {(info.time == 1 ? "minute" : "minutes")}";
                }
                break;
            case LogType.Arrest:
                info.color = new Color(255, 61, 24);
                info.reason = "*No reason necessary*";
                if (info.isTime)
                {
                    info.length += $" {(info.time == 1 ? "minute" : "minutes")}";
                }
                break;
            case LogType.Kick:
                info.color = new Color(255, 12, 12);
                break;
            case LogType.Ban:
                info.color = new Color(130, 0, 0);
                if (info.isTime)
                {
                    info.length += $" {(info.time == 1 ? "day" : "days")}";
                }
                break;
            case LogType.Unverify:
                info.color = new Color(0, 0, 0);
                info.isTime = false;
                break;
            case LogType.RemoveWarn:
                info.cmd = $"Remove Warning {info.length} from";
                goto default;
            case LogType.RemoveWarns:
                info.cmd = $"Remove {info.length ?? "All"} Warnings from";
                goto default;
            case LogType.Unmute:
            case LogType.Free:
            case LogType.Unban:
            default:
                info.color = new Color(12, 156, 24);
                info.reasonAllowed = false;
                info.isTime = false;
                break;
            }
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

                SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
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

                SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
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
