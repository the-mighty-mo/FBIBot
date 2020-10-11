using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace FBIBot
{
    public static class Program
    {
        private static DiscordSocketConfig config;
        private static DiscordSocketClient client;
        private static CommandHandler handler;

        public static readonly Random rng = new Random();

        public static async Task Main()
        {
            static bool isRunning() => Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Count() > 1;
            if (isRunning())
            {
                await Task.Delay(1000);

                if (isRunning())
                {
                    Console.WriteLine("Program is already running");
                    await Task.WhenAny(
                        Task.Run(() => Console.ReadLine()),
                        Task.Delay(5000)
                    );
                    return;
                }
            }

            Task initSqlite = DatabaseManager.InitAsync().ContinueWith(x => Console.WriteLine($"{SecurityInfo.botName} has finished loading"));
            Task initCommandHandler = SetupCommandHandlerAsync();

            await Task.WhenAll(
                initSqlite,
                initCommandHandler
            );
            await Task.Delay(-1);
        }

        private static async Task SetupCommandHandlerAsync()
        {
            config = new DiscordSocketConfig
            {
                AlwaysDownloadUsers = false
            };
            client = new DiscordSocketClient(config);

            await client.LoginAsync(TokenType.Bot, SecurityInfo.token);
            await client.StartAsync();
            await client.SetGameAsync($"@{SecurityInfo.botName} help", null, ActivityType.Listening);

            IServiceProvider _services = new ServiceCollection().BuildServiceProvider();
            handler = new CommandHandler(client, _services);
            await handler.InitCommandsAsync();
        }
    }
}