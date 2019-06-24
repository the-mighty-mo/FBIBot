using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace FBIBot
{
    class CommandHandler
    {
        public const char prefix = '\\';

        private readonly DiscordSocketClient _client;
        private readonly CommandService _service;
        private readonly IServiceProvider _services;

        public CommandHandler(DiscordSocketClient client, IServiceProvider services)
        {
            _client = client;
            _services = services;

            _service = new CommandService();
            _service.AddModulesAsync(Assembly.GetEntryAssembly(), services);

            _client.Connected += SendConnectMessage;
            _client.Disconnected += SendDisconnectError;
            _client.MessageReceived += HandleCommandAsync;
        }

        private async Task SendConnectMessage()
        {
            if (Program.isConsole)
            {
                await Console.Out.WriteLineAsync($"{SecurityInfo.botName} Online");
            }
        }

        private async Task SendDisconnectError(Exception e)
        {
            if (Program.isConsole)
            {
                await Console.Out.WriteLineAsync(e.Message);
            }
        }

        private async Task HandleCommandAsync(SocketMessage m)
        {
            if (!(m is SocketUserMessage msg))
            {
                return;
            }

            SocketCommandContext context = new SocketCommandContext(_client, msg);

            int argPos = 0;
            if (!context.User.IsBot)
            {
                // Commands
                if (msg.HasCharPrefix(prefix, ref argPos))
                {
                    var result = await _service.ExecuteAsync(context, argPos, _services);

                    if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                    {
                        await context.Channel.SendMessageAsync($"Error: {result.ErrorReason}");
                    }
                }
            }
        }
    }
}
