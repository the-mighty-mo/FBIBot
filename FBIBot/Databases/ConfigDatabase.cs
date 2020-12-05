using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Databases
{
    public class ConfigDatabase
    {
        private readonly SqliteConnection connection = new SqliteConnection("Filename=Config.db");

        public readonly PrefixesTable Prefixes;
        public readonly ModifyMutedTable ModifyMuted;
        public readonly AutoSurveillanceTable AutoSurveillance;
        public readonly AntiZalgoTable AntiZalgo;
        public readonly AntiSpamTable AntiSpam;
        public readonly AntiSingleSpamTable AntiSingleSpam;
        public readonly AntiMassMentionTable AntiMassMention;
        public readonly AntiCapsTable AntiCaps;
        public readonly AntiInviteTable AntiInvite;
        public readonly AntiLinkTable AntiLink;

        public ConfigDatabase()
        {
            Prefixes = new PrefixesTable(connection);
            ModifyMuted = new ModifyMutedTable(connection);
            AutoSurveillance = new AutoSurveillanceTable(connection);
            AntiZalgo = new AntiZalgoTable(connection);
            AntiSpam = new AntiSpamTable(connection);
            AntiSingleSpam = new AntiSingleSpamTable(connection);
            AntiMassMention = new AntiMassMentionTable(connection);
            AntiCaps = new AntiCapsTable(connection);
            AntiInvite = new AntiInviteTable(connection);
            AntiLink = new AntiLinkTable(connection);
        }

        public async Task InitAsync()
        {
            await connection.OpenAsync();

            List<Task> cmds = new List<Task>();
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS Prefixes (guild_id TEXT PRIMARY KEY, prefix TEXT NOT NULL);", connection))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS ModifyMuted (guild_id TEXT PRIMARY KEY);", connection))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS AutoSurveillance (guild_id TEXT PRIMARY KEY);", connection))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS AntiZalgo (guild_id TEXT PRIMARY KEY);", connection))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS AntiSpam (guild_id TEXT PRIMARY KEY);", connection))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS AntiSingleSpam (guild_id TEXT PRIMARY KEY);", connection))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS AntiMassMention (guild_id TEXT PRIMARY KEY);", connection))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS AntiCaps (guild_id TEXT PRIMARY KEY);", connection))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS AntiInvite (guild_id TEXT PRIMARY KEY);", connection))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS AntiLink (guild_id TEXT PRIMARY KEY);", connection))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }

            await Task.WhenAll(cmds);
        }

        public async Task CloseAsync() => await connection.CloseAsync();

        public class PrefixesTable
        {
            private readonly SqliteConnection connection;

            public PrefixesTable(SqliteConnection connection) => this.connection = connection;

            public async Task<string> GetPrefixAsync(SocketGuild g)
            {
                string prefix = CommandHandler.prefix;

                string getPrefix = "SELECT prefix FROM Prefixes WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(getPrefix, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        prefix = (string)reader["prefix"];
                    }
                    reader.Close();
                }

                return prefix;
            }

            public async Task SetPrefixAsync(SocketGuild g, string prefix)
            {
                string update = "UPDATE Prefixes SET prefix = @prefix WHERE guild_id = @guild_id;";
                string insert = "INSERT INTO Prefixes (guild_id, prefix) SELECT @guild_id, @prefix WHERE (SELECT Changes() = 0);";

                using (SqliteCommand cmd = new SqliteCommand(update + insert, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                    cmd.Parameters.AddWithValue("@prefix", prefix);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public class ModifyMutedTable
        {
            private readonly SqliteConnection connection;

            public ModifyMutedTable(SqliteConnection connection) => this.connection = connection;

            public async Task AddModifyMutedAsync(SocketGuild g)
            {
                string add = "INSERT INTO ModifyMuted (guild_id) SELECT @guild_id\n" +
                    "WHERE NOT EXISTS (SELECT 1 FROM ModifyMuted WHERE guild_id = @guild_id);";
                using (SqliteCommand cmd = new SqliteCommand(add, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public async Task RemoveModifyMutedAsync(SocketGuild g)
            {
                string add = "DELETE FROM ModifyMuted WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(add, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public async Task<bool> GetModifyMutedAsync(SocketGuild g)
            {
                bool modify = false;

                string add = "SELECT * FROM ModifyMuted WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(add, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    modify = await reader.ReadAsync();
                    reader.Close();
                }

                return modify;
            }
        }

        public class AutoSurveillanceTable
        {
            private readonly SqliteConnection connection;

            public AutoSurveillanceTable(SqliteConnection connection) => this.connection = connection;

            public async Task<bool> GetAutoSurveillanceAsync(SocketGuild g)
            {
                bool isAutoSurveillance = false;

                string getSurveillance = "SELECT guild_id FROM AutoSurveillance WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(getSurveillance, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    isAutoSurveillance = await reader.ReadAsync();
                    reader.Close();
                }

                return isAutoSurveillance;
            }

            public async Task SetAutoSurveillanceAsync(SocketGuild g)
            {
                string insert = "INSERT INTO AutoSurveillance (guild_id) SELECT @guild_id\n" +
                    "WHERE NOT EXISTS (SELECT 1 FROM AutoSurveillance WHERE guild_id = @guild_id);";

                using (SqliteCommand cmd = new SqliteCommand(insert, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public async Task RemoveAutoSurveillanceAsync(SocketGuild g)
            {
                string delete = "DELETE FROM AutoSurveillance WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(delete, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public class AntiZalgoTable
        {
            private readonly SqliteConnection connection;

            public AntiZalgoTable(SqliteConnection connection) => this.connection = connection;

            public async Task<bool> GetAntiZalgoAsync(SocketGuild g)
            {
                bool isAntiZalgo = false;

                string getZalgo = "SELECT guild_id FROM AntiZalgo WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(getZalgo, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    isAntiZalgo = await reader.ReadAsync();
                    reader.Close();
                }

                return isAntiZalgo;
            }

            public async Task SetAntiZalgoAsync(SocketGuild g)
            {
                string insert = "INSERT INTO AntiZalgo (guild_id) SELECT @guild_id\n" +
                    "WHERE NOT EXISTS (SELECT 1 FROM AntiZalgo WHERE guild_id = @guild_id);";

                using (SqliteCommand cmd = new SqliteCommand(insert, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public async Task RemoveAntiZalgoAsync(SocketGuild g)
            {
                string delete = "DELETE FROM AntiZalgo WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(delete, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public class AntiSpamTable
        {
            private readonly SqliteConnection connection;

            public AntiSpamTable(SqliteConnection connection) => this.connection = connection;

            public async Task<bool> GetAntiSpamAsync(SocketGuild g)
            {
                bool isAntiSpam = false;

                string getSpam = "SELECT guild_id FROM AntiSpam WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(getSpam, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    isAntiSpam = await reader.ReadAsync();
                    reader.Close();
                }

                return isAntiSpam;
            }

            public async Task SetAntiSpamAsync(SocketGuild g)
            {
                string insert = "INSERT INTO AntiSpam (guild_id) SELECT @guild_id\n" +
                    "WHERE NOT EXISTS (SELECT 1 FROM AntiSpam WHERE guild_id = @guild_id);";

                using (SqliteCommand cmd = new SqliteCommand(insert, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public async Task RemoveAntiSpamAsync(SocketGuild g)
            {
                string delete = "DELETE FROM AntiSpam WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(delete, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public class AntiSingleSpamTable
        {
            private readonly SqliteConnection connection;

            public AntiSingleSpamTable(SqliteConnection connection) => this.connection = connection;

            public async Task<bool> GetAntiSingleSpamAsync(SocketGuild g)
            {
                bool isAntiSingleSpam = false;

                string getSingleSpam = "SELECT guild_id FROM AntiSingleSpam WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(getSingleSpam, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    isAntiSingleSpam = await reader.ReadAsync();
                    reader.Close();
                }

                return isAntiSingleSpam;
            }

            public async Task SetAntiSingleSpamAsync(SocketGuild g)
            {
                string insert = "INSERT INTO AntiSingleSpam (guild_id) SELECT @guild_id\n" +
                    "WHERE NOT EXISTS (SELECT 1 FROM AntiSingleSpam WHERE guild_id = @guild_id);";

                using (SqliteCommand cmd = new SqliteCommand(insert, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public async Task RemoveAntiSingleSpamAsync(SocketGuild g)
            {
                string delete = "DELETE FROM AntiSingleSpam WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(delete, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public class AntiMassMentionTable
        {
            private readonly SqliteConnection connection;

            public AntiMassMentionTable(SqliteConnection connection) => this.connection = connection;

            public async Task<bool> GetAntiMassMentionAsync(SocketGuild g)
            {
                bool isAntiMassMention = false;

                string getMassMention = "SELECT guild_id FROM AntiMassMention WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(getMassMention, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    isAntiMassMention = await reader.ReadAsync();
                    reader.Close();
                }

                return isAntiMassMention;
            }

            public async Task SetAntiMassMentionAsync(SocketGuild g)
            {
                string insert = "INSERT INTO AntiMassMention (guild_id) SELECT @guild_id\n" +
                    "WHERE NOT EXISTS (SELECT 1 FROM AntiMassMention WHERE guild_id = @guild_id);";

                using (SqliteCommand cmd = new SqliteCommand(insert, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public async Task RemoveAntiMassMentionAsync(SocketGuild g)
            {
                string delete = "DELETE FROM AntiMassMention WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(delete, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public class AntiCapsTable
        {
            private readonly SqliteConnection connection;

            public AntiCapsTable(SqliteConnection connection) => this.connection = connection;

            public async Task<bool> GetAntiCapsAsync(SocketGuild g)
            {
                bool isAntiCaps = false;

                string getCaps = "SELECT guild_id FROM AntiCaps WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(getCaps, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    isAntiCaps = await reader.ReadAsync();
                    reader.Close();
                }

                return isAntiCaps;
            }

            public async Task SetAntiCapsAsync(SocketGuild g)
            {
                string insert = "INSERT INTO AntiCaps (guild_id) SELECT @guild_id\n" +
                    "WHERE NOT EXISTS (SELECT 1 FROM AntiCaps WHERE guild_id = @guild_id);";

                using (SqliteCommand cmd = new SqliteCommand(insert, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public async Task RemoveAntiCapsAsync(SocketGuild g)
            {
                string delete = "DELETE FROM AntiCaps WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(delete, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public class AntiInviteTable
        {
            private readonly SqliteConnection connection;

            public AntiInviteTable(SqliteConnection connection) => this.connection = connection;

            public async Task<bool> GetAntiInviteAsync(SocketGuild g)
            {
                bool isAntiInvite = false;

                string getInvite = "SELECT guild_id FROM AntiInvite WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(getInvite, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    isAntiInvite = await reader.ReadAsync();
                    reader.Close();
                }

                return isAntiInvite;
            }

            public async Task SetAntiInviteAsync(SocketGuild g)
            {
                string insert = "INSERT INTO AntiInvite (guild_id) SELECT @guild_id\n" +
                    "WHERE NOT EXISTS (SELECT 1 FROM AntiInvite WHERE guild_id = @guild_id);";

                using (SqliteCommand cmd = new SqliteCommand(insert, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public async Task RemoveAntiInviteAsync(SocketGuild g)
            {
                string delete = "DELETE FROM AntiInvite WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(delete, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public class AntiLinkTable
        {
            private readonly SqliteConnection connection;

            public AntiLinkTable(SqliteConnection connection) => this.connection = connection;

            public async Task<bool> GetAntiLinkAsync(SocketGuild g)
            {
                bool isAntiLink = false;

                string getLink = "SELECT guild_id FROM AntiLink WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(getLink, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                    SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                    isAntiLink = await reader.ReadAsync();
                    reader.Close();
                }

                return isAntiLink;
            }

            public async Task SetAntiLinkAsync(SocketGuild g)
            {
                string insert = "INSERT INTO AntiLink (guild_id) SELECT @guild_id\n" +
                    "WHERE NOT EXISTS (SELECT 1 FROM AntiLink WHERE guild_id = @guild_id);";

                using (SqliteCommand cmd = new SqliteCommand(insert, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            public async Task RemoveAntiLinkAsync(SocketGuild g)
            {
                string delete = "DELETE FROM AntiLink WHERE guild_id = @guild_id;";
                using (SqliteCommand cmd = new SqliteCommand(delete, connection))
                {
                    cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}