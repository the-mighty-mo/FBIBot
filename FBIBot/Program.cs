using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FBIBot
{
    public static class Program
    {
        public static readonly Random Rng = new();

        public static async Task Main()
        {
            static bool isRunning() => Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1;
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
            DiscordSocketConfig config = new()
            {
                AlwaysDownloadUsers = false,
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers | GatewayIntents.GuildPresences | GatewayIntents.GuildMessages
            };
            DiscordSocketClient client = new(config);

            await client.LoginAsync(TokenType.Bot, SecurityInfo.token);
            await client.StartAsync();
            await client.SetGameAsync($"/help", null, ActivityType.Listening);

            IServiceProvider _services = new ServiceCollection().BuildServiceProvider();
            CommandHandler handler = new(client, _services);
            await handler.InitCommandsAsync();
        }
    }
}