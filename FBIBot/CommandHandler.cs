using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FBIBot.Modules.AutoMod;
using FBIBot.Modules.Config;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace FBIBot
{
    class CommandHandler
    {
        public const string prefix = "\\";
        public static int argPos = 0;

        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        public CommandHandler(DiscordSocketClient client, IServiceProvider services)
        {
            _client = client;
            _services = services;

            CommandServiceConfig config = new CommandServiceConfig()
            {
                DefaultRunMode = RunMode.Async
            };
            _commands = new CommandService(config);
        }

        public async Task InitCommandsAsync()
        {
            _client.Connected += SendConnectMessage;
            _client.Disconnected += SendDisconnectError;
            _client.JoinedGuild += SendJoinMessage;
            _client.UserJoined += SendWelcomeMessage;
            _client.MessageReceived += HandleCommandAsync;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task SendConnectMessage()
        {
            if (Program.isConsole)
            {
                await Console.Out.WriteLineAsync($"{SecurityInfo.botName} is online");
            }
        }

        private async Task SendDisconnectError(Exception e)
        {
            if (Program.isConsole)
            {
                await Console.Out.WriteLineAsync(e.Message);
            }
        }

        private async Task SendJoinMessage(SocketGuild g) => await g.DefaultChannel.SendMessageAsync("Someone called for some democracy and justice?");

        private async Task SendWelcomeMessage(SocketGuildUser u)
        {
            if (await RaidMode.GetVerificationLevelAsync(u.Guild) != null && !u.IsBot)
            {
                await u.SendMessageAsync($":rotating_light: :rotating_light: The FBI of {u.Guild.Name} is currently in Raid Mode. As a result, you may not join the server at this time.:rotating_light: :rotating_light:");
                await RaidMode.AddBlockedUserAsync(u);
                await u.KickAsync("FBI RAID MODE");
                return;
            }

            SocketTextChannel channel = u.Guild.DefaultChannel;
            List<string> messages = new List<string>()
            {
                "Don't even think about it.",
                "**FBI OPEN U**...wait...we'll be back shortly with a warrant.",
                "Where do you think you're going?",
                "Ladies and gentlemen, we got 'em.",
                "Give us a moment while we send a representative into your camera...",
                "You thought it was a normal server, but it was me! THE FBI!",
                "Walk out slowly with your arms i...wait, wrong person.",
                "You want to know how we figured it out?"
            };
            int index = Program.rng.Next(messages.Count);
            await channel.SendMessageAsync($"{u.Mention} {messages[index]}");

            if (await Verify.IsVerifiedAsync(u))
            {
                SocketRole role = await SetVerify.GetVerificationRoleAsync(u.Guild);
                if (role != null && u.Guild.CurrentUser.GetPermissions(u.Guild.DefaultChannel).ManageRoles)
                {
                    await u.AddRoleAsync(role);
                }
            }
            else if (!u.IsBot && await SetVerify.GetVerificationRoleAsync(u.Guild) != null)
            {
                await Verify.SendCaptchaAsync(u);
            }
        }

        private async Task HandleCommandAsync(SocketMessage m)
        {
            if (!(m is SocketUserMessage msg) || (msg.Author.IsBot && msg.Author.Id != _client.CurrentUser.Id))
            {
                return;
            }

            SocketCommandContext context = new SocketCommandContext(_client, msg);
            string _prefix = context.Guild != null ? await Prefix.GetPrefixAsync(context.Guild) : prefix;

            if (msg.HasMentionPrefix(_client.CurrentUser, ref argPos) || msg.HasStringPrefix(_prefix, ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);

                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                {
                    await context.Channel.SendMessageAsync($"Error: {result.ErrorReason}");
                }

                if (msg.Author.IsBot)
                {
                    await msg.DeleteAsync();
                }
            }

            if (await IsPedophile(msg))
            {
                await new Pedophile().AntiPedophileAsync(msg.Author);
                await msg.DeleteAsync();
                await context.Channel.SendMessageAsync($"\\arrest {msg.Author.Mention}");
            }
        }

        private async Task<bool> IsPedophile(SocketUserMessage msg)
        {
            bool isPedophile = false;

            List<string> bad = new List<string>()
            {
                "i like",
                "i love"
            };
            List<string> stillBad = new List<string>()
            {
                "kids",
                "children",
                "little kids",
                "little children"
            };
            foreach (string b in bad)
            {
                foreach (string s in stillBad)
                {
                    if (msg.Content.ToLower().Contains($"{b} {s}"))
                    {
                        isPedophile = true;
                        break;
                    }
                }
                if (isPedophile)
                {
                    break;
                }
            }

            return await Task.Run(() => isPedophile);
        }
    }
}
