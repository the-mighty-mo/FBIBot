using Discord;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FBIBot
{
    public class Program
    {
        private DiscordSocketConfig _config;
        private DiscordSocketClient _client;
        private CommandHandler _handler;

        public static readonly Random rng = new Random();
        public static readonly SqliteConnection cnVerify = new SqliteConnection("Filename=Verification.db");
        public static readonly SqliteConnection cnModRoles = new SqliteConnection("Filename=ModRoles.db");
        public static readonly SqliteConnection cnModLogs = new SqliteConnection("Filename=ModLogs.db");
        public static readonly SqliteConnection cnConfig = new SqliteConnection("Filename=Config.db");
        public static readonly SqliteConnection cnRaidMode = new SqliteConnection("Filename=Raid-Mode.db");

        public static readonly bool isConsole = Console.OpenStandardInput(1) != Stream.Null;

        static void Main() => new Program().StartAsync().GetAwaiter().GetResult();

        public async Task StartAsync()
        {
            if (isConsole)
            {
                Console.Title = SecurityInfo.botName;
            }

            bool isRunning = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Count() > 1;
            if (isRunning)
            {
                await Task.Delay(1000);
                isRunning = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Count() > 1;

                if (isRunning)
                {
                    MessageBox.Show("Program is already running", SecurityInfo.botName);
                    return;
                }
            }

            _config = new DiscordSocketConfig
            {
                AlwaysDownloadUsers = false
            };

            _client = new DiscordSocketClient(_config);

            await _client.LoginAsync(TokenType.Bot, SecurityInfo.token);
            await _client.StartAsync();

            await _client.SetGameAsync("@The FBI help", null, ActivityType.Listening);

            IServiceProvider _services = new ServiceCollection().BuildServiceProvider();

            _handler = new CommandHandler(_client, _services);
            Task initCmd = _handler.InitCommandsAsync();

            List<Task> cmds = new List<Task>()
            {
                InitVerifySqlite(),
                InitModRolesSqlite(),
                InitModLogsSqlite(),
                InitConfigSqlite(),
                InitAntiRaidSqlite()
            };
            await Task.WhenAll(cmds);

            if (isConsole)
            {
                Console.WriteLine($"{SecurityInfo.botName} has finished loading");
            }

            await initCmd;
            await Task.Delay(-1);
        }

        static async Task InitVerifySqlite()
        {
            await cnVerify.OpenAsync();

            List<Task> cmds = new List<Task>();
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS Captcha (user_id TEXT PRIMARY KEY, captcha TEXT NOT NULL);", cnVerify))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS Verified (user_id TEXT PRIMARY KEY);", cnVerify))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS Attempts (user_id TEXT PRIMARY KEY, attempts INTEGER NOT NULL);", cnVerify))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS Roles (guild_id TEXT PRIMARY KEY, role_id TEXT NOT NULL);", cnVerify))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }

            await Task.WhenAll(cmds);
        }

        static async Task InitModRolesSqlite()
        {
            await cnModRoles.OpenAsync();

            List<Task> cmds = new List<Task>();
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS Muted (guild_id TEXT PRIMARY KEY, role_id TEXT NOT NULL);", cnModRoles))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS PrisonerRole (guild_id TEXT PRIMARY KEY, role_id TEXT NOT NULL);", cnModRoles))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS PrisonerChannel (guild_id TEXT PRIMARY KEY, channel_id TEXT NOT NULL);", cnModRoles))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS Prisoners (guild_id TEXT NOT NULL, user_id TEXT NOT NULL, UNIQUE (guild_id, user_id));", cnModRoles))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS UserRoles (guild_id TEXT NOT NULL, user_id TEXT NOT NULL, role_id TEXT NOT NULL, UNIQUE (guild_id, user_id, role_id));", cnModRoles))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS Mods (guild_id TEXT TEXT NOT NULL, role_id TEXT NOT NULL);", cnModRoles))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS Admins (guild_id TEXT NOT NULL, role_id TEXT NOT NULL);", cnModRoles))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }

            await Task.WhenAll(cmds);
        }

        static async Task InitModLogsSqlite()
        {
            await cnModLogs.OpenAsync();

            List<Task> cmds = new List<Task>();
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS ModLogChannel (guild_id TEXT PRIMARY KEY, channel_id TEXT NOT NULL);", cnModLogs))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS ModLogs (guild_id TEXT NOT NULL, id TEXT NOT NULL, channel_id TEXT NOT NULL, message_id TEXT NOT NULL, UNIQUE (guild_id, id));", cnModLogs))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS Warnings (guild_id TEXT NOT NULL, id TEXT NOT NULL, user_id TEXT NOT NULL, UNIQUE (guild_id, id));", cnModLogs))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }

            await Task.WhenAll(cmds);
        }

        static async Task InitConfigSqlite()
        {
            await cnConfig.OpenAsync();

            List<Task> cmds = new List<Task>();
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS Prefixes (guild_id TEXT PRIMARY KEY, prefix TEXT NOT NULL);", cnConfig))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS ModifyMuted (guild_id TEXT PRIMARY KEY);", cnConfig))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS AutoSurveillance (guild_id TEXT PRIMARY KEY);", cnConfig))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS AntiSpam (guild_id TEXT PRIMARY KEY);", cnConfig))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS AntiSingleSpam (guild_id TEXT PRIMARY KEY);", cnConfig))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS AntiMassMention (guild_id TEXT PRIMARY KEY);", cnConfig))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS AntiCaps (guild_id TEXT PRIMARY KEY);", cnConfig))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS AntiInvite (guild_id TEXT PRIMARY KEY);", cnConfig))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS AntiLink (guild_id TEXT PRIMARY KEY);", cnConfig))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }

            await Task.WhenAll(cmds);
        }

        static async Task InitAntiRaidSqlite()
        {
            await cnRaidMode.OpenAsync();

            List<Task> cmds = new List<Task>();
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS RaidMode (guild_id TEXT PRIMARY KEY, level TEXT NOT NULL);", cnRaidMode))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }
            using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS UsersBlocked (guild_id TEXT NOT NULL, user_id TEXT NOT NULL UNIQUE);", cnRaidMode))
            {
                cmds.Add(cmd.ExecuteNonQueryAsync());
            }

            await Task.WhenAll(cmds);
        }
    }
}