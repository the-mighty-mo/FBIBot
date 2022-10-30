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
                await Task.Delay(1000).ConfigureAwait(false);

                if (isRunning())
                {
                    Console.WriteLine("Program is already running");
                    await Task.WhenAny(
                        Task.Run(() => Console.ReadLine()),
                        Task.Delay(5000)
                    ).ConfigureAwait(false);
                    return;
                }
            }

            Task initSqlite = DatabaseManager.InitAsync().ContinueWith(
                _ => Console.WriteLine($"{SecurityInfo.botName} has finished loading"),
                TaskContinuationOptions.ExecuteSynchronously
            );
            Task initCommandHandler = SetupCommandHandlerAsync();

            await Task.WhenAll(
                initSqlite,
                initCommandHandler
            ).ConfigureAwait(false);
            await Task.Delay(-1).ConfigureAwait(false);
        }

        private static async Task SetupCommandHandlerAsync()
        {
            DiscordSocketConfig config = new()
            {
                AlwaysDownloadUsers = false,
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers | GatewayIntents.GuildPresences | GatewayIntents.GuildMessages
            };
            DiscordSocketClient client = new(config);

            await client.LoginAsync(TokenType.Bot, SecurityInfo.token).ConfigureAwait(false);
            await client.StartAsync().ConfigureAwait(false);
            await client.SetGameAsync($"/help", null, ActivityType.Listening).ConfigureAwait(false);

            IServiceProvider _services = new ServiceCollection().BuildServiceProvider();
            CommandHandler handler = new(client, _services);
            await handler.InitCommandsAsync().ConfigureAwait(false);
        }
    }
}