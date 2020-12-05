using Discord;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Databases
{
    public class ModLogsDatabase
    {
        private readonly SqliteConnection connection = new SqliteConnection("Filename=ModLogs.db");

        public readonly ModLogChannelTable ModLogChannel;
        public readonly CaptchaLogChannelTable CaptchaLogChannel;
        public readonly WelcomeChannelTable WelcomeChannel;
        public readonly ModLogsTable ModLogs;
        public readonly WarningsTable Warnings;

        public ModLogsDatabase()
        {
            ModLogChannel = new ModLogChannelTable(connection);
            CaptchaLogChannel = new CaptchaLogChannelTable(connection);
            WelcomeChannel = new WelcomeChannelTable(connection);
            ModLogs = new ModLogsTable(connection);
            Warnings = new WarningsTable(connection);
        }

        public async Task InitAsync()
        {
            await connection.OpenAsync();

            List<Task> cmds = new List<Task>();
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS ModLogChannel (guild_id TEXT PRIMARY KEY, channel_id TEXT NOT NULL);", connection))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS CaptchaLogChannel (guild_id TEXT PRIMARY KEY, channel_id TEXT NOT NULL);", connection))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS WelcomeChannel (guild_id TEXT PRIMARY KEY, channel_id TEXT NOT NULL);", connection))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS ModLogs (guild_id TEXT NOT NULL, id TEXT NOT NULL, channel_id TEXT NOT NULL, message_id TEXT NOT NULL, UNIQUE (guild_id, id));", connection))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS Warnings (guild_id TEXT NOT NULL, id TEXT NOT NULL, user_id TEXT NOT NULL, UNIQUE (guild_id, id));", connection))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }

            await Task.WhenAll(cmds);
        }

        public async Task CloseAsync() => await connection.CloseAsync();

        public class ModLogChannelTable
        {
            private readonly SqliteConnection connection;

            public ModLogChannelTable(SqliteConnection connection) => this.connection = connection;

            public async Task<SocketTextChannel> GetModLogChannelAsync(SocketGuild g)
            {
                SocketTextChannel channel = null;

                string getChannel = "SELECT channel_id FROM ModLogChannel WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(getChannel, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        _ = ulong.TryParse(reader["channel_id"].ToString(), out ulong channelID);
                        channel = g.GetTextChannel(channelID);
                    }
                    reader.Close();
                }

                return channel;
            }

            public async Task SetModLogChannelAsync(SocketTextChannel channel)
            {
                string update = "UPDATE ModLogChannel SET channel_id = @channel_id WHERE guild_id = @guild_id;";
                string insert = "INSERT INTO ModLogChannel (guild_id, channel_id) SELECT @guild_id, @channel_id WHERE (SELECT Changes() = 0);";

                using (SqliteCommand cmd = new SqliteCommand(update + insert, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", channel.Guild.Id.ToString());
                    cmd.Parameters.AddWithValue("@channel_id", channel.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public async Task RemoveModLogChannelAsync(SocketGuild g)
            {
                string delete = "DELETE FROM ModLogChannel WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(delete, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public class CaptchaLogChannelTable
        {
            private readonly SqliteConnection connection;

            public CaptchaLogChannelTable(SqliteConnection connection) => this.connection = connection;

            public async Task<SocketTextChannel> GetCaptchaLogChannelAsync(SocketGuild g)
            {
                SocketTextChannel channel = null;

                string getChannel = "SELECT channel_id FROM CaptchaLogChannel WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(getChannel, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        _ = ulong.TryParse(reader["channel_id"].ToString(), out ulong channelID);
                        channel = g.GetTextChannel(channelID);
                    }
                    reader.Close();
                }

                return channel;
            }

            public async Task SetCaptchaLogChannelAsync(SocketTextChannel channel)
            {
                string update = "UPDATE CaptchaLogChannel SET channel_id = @channel_id WHERE guild_id = @guild_id;";
                string insert = "INSERT INTO CaptchaLogChannel (guild_id, channel_id) SELECT @guild_id, @channel_id WHERE (SELECT Changes() = 0);";

                using (SqliteCommand cmd = new SqliteCommand(update + insert, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", channel.Guild.Id.ToString());
                    cmd.Parameters.AddWithValue("@channel_id", channel.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public async Task RemoveCaptchaLogChannelAsync(SocketGuild g)
            {
                string delete = "DELETE FROM CaptchaLogChannel WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(delete, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public class WelcomeChannelTable
        {
            private readonly SqliteConnection connection;

            public WelcomeChannelTable(SqliteConnection connection) => this.connection = connection;

            public async Task<SocketTextChannel> GetWelcomeChannelAsync(SocketGuild g)
            {
                SocketTextChannel channel = null;

                string getChannel = "SELECT channel_id FROM WelcomeChannel WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(getChannel, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        _ = ulong.TryParse(reader["channel_id"].ToString(), out ulong channelID);
                        channel = g.GetTextChannel(channelID);
                    }
                    reader.Close();
                }

                return channel;
            }

            public async Task SetWelcomeChannelAsync(SocketTextChannel channel)
            {
                string update = "UPDATE WelcomeChannel SET channel_id = @channel_id WHERE guild_id = @guild_id;";
                string insert = "INSERT INTO WelcomeChannel (guild_id, channel_id) SELECT @guild_id, @channel_id WHERE (SELECT Changes() = 0);";

                using (SqliteCommand cmd = new SqliteCommand(update + insert, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", channel.Guild.Id.ToString());
                    cmd.Parameters.AddWithValue("@channel_id", channel.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public async Task RemoveWelcomeChannelAsync(SocketGuild g)
            {
                string delete = "DELETE FROM WelcomeChannel WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(delete, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public class ModLogsTable
        {
            private readonly SqliteConnection connection;

            public ModLogsTable(SqliteConnection connection) => this.connection = connection;

            public async Task<ulong> GetNextModLogID(SocketGuild g)
            {
                ulong id = 0;

                string getID = "SELECT MAX(CAST(id AS INTEGER)) AS id FROM ModLogs WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(getID, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        _ = ulong.TryParse(reader["id"].ToString(), out id);
                    }
                    reader.Close();
                }

                return ++id;
            }

            public async Task<IUserMessage> GetModLogAsync(SocketGuild g, ulong id)
            {
                IUserMessage msg = null;

                string getMessage = "SELECT channel_id, message_id FROM ModLogs WHERE guild_id = @guild_id AND id = @id;";
                using (SqliteCommand cmd = new SqliteCommand(getMessage, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                    cmd.Parameters.AddWithValue("@id", id.ToString());

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        _ = ulong.TryParse(reader["channel_id"].ToString(), out ulong channelID);
                        _ = ulong.TryParse(reader["message_id"].ToString(), out ulong messageID);

                        msg = await g.GetTextChannel(channelID)?.GetMessageAsync(messageID) as IUserMessage;
                    }
                    reader.Close();
                }

                return msg;
            }

            public async Task SaveModLogAsync(IUserMessage msg, SocketGuild g, ulong id)
            {
                string insert = "INSERT INTO ModLogs (guild_id, id, channel_id, message_id) SELECT @guild_id, @id, @channel_id, @message_id\n" +
                    "WHERE NOT EXISTS (SELECT 1 FROM ModLogs WHERE guild_id = @guild_id AND id = @id);";
                using (SqliteCommand cmd = new SqliteCommand(insert, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                    cmd.Parameters.AddWithValue("@id", id.ToString());
                    cmd.Parameters.AddWithValue("@channel_id", msg.Channel.Id.ToString());
                    cmd.Parameters.AddWithValue("@message_id", msg.Id.ToString());

                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public async Task RemoveModLogsAsync(SocketGuild g)
            {
                string delete = "DELETE FROM ModLogs WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(delete, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public class WarningsTable
        {
            private readonly SqliteConnection connection;

            public WarningsTable(SqliteConnection connection) => this.connection = connection;

            public async Task AddWarningAsync(SocketGuildUser u, ulong id)
            {
                string addWarning = "INSERT INTO Warnings (guild_id, id, user_id) SELECT @guild_id, @id, @user_id\n" +
                    "WHERE NOT EXISTS (SELECT 1 FROM Warnings WHERE guild_id = @guild_id AND id = @id AND user_id = @user_id);";
                using (SqliteCommand cmd = new SqliteCommand(addWarning, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", u.Guild.Id.ToString());
                    cmd.Parameters.AddWithValue("@id", id.ToString());
                    cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());

                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public async Task<bool> GetWarningAsync(SocketGuildUser u, ulong id)
            {
                bool hasWarning = false;

                string getWarning = "SELECT * FROM Warnings WHERE guild_id = @guild_id AND id = @id AND user_id = @user_id;";
                using (SqliteCommand cmd = new SqliteCommand(getWarning, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", u.Guild.Id.ToString());
                    cmd.Parameters.AddWithValue("@id", id.ToString());
                    cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    hasWarning = await reader.ReadAsync();
                    reader.Close();
                }

                return hasWarning;
            }

            public async Task<List<ulong>> GetWarningsAsync(SocketGuildUser u)
            {
                List<ulong> ids = new List<ulong>();

                string getWarns = "SELECT id FROM Warnings WHERE guild_id = @guild_id AND user_id = @user_id;";
                using (SqliteCommand cmd = new SqliteCommand(getWarns, connection))
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

            public async Task RemoveWarningAsync(SocketGuildUser u, ulong id)
            {
                string delete = "DELETE FROM Warnings WHERE guild_id = @guild_id AND id = @id AND user_id = @user_id;";
                using (SqliteCommand cmd = new SqliteCommand(delete, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", u.Guild.Id.ToString());
                    cmd.Parameters.AddWithValue("@id", id.ToString());
                    cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());

                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public async Task RemoveWarningsAsync(SocketGuildUser u, string count = null)
            {
                string delete = "DELETE FROM Warnings WHERE guild_id = @guild_id AND user_id = @user_id";
                if (count != null)
                {
                    delete += " AND id IN (SELECT id FROM Warnings WHERE guild_id = @guild_id AND user_id = @user_id ORDER BY CAST(id AS INTEGER) ASC LIMIT @count)";
                }
                delete += ";";

                using (SqliteCommand cmd = new SqliteCommand(delete, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", u.Guild.Id.ToString());
                    cmd.Parameters.AddWithValue("@user_id", u.Id.ToString());
                    cmd.Parameters.AddWithValue("@count", count);

                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public async Task RemoveAllWarningsAsync(SocketGuild g)
            {
                string delete = "DELETE FROM Warnings WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(delete, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}