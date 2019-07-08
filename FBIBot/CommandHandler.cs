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
    public class CommandHandler
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
            _commands.CommandExecuted += SendErrorAsync;
        }

        private async Task SendErrorAsync(Optional<CommandInfo> info, ICommandContext context, IResult result)
        {
            if (!result.IsSuccess && info.Value.RunMode == RunMode.Async && result.Error != CommandError.UnknownCommand && result.Error != CommandError.UnmetPrecondition)
            {
                await context.Channel.SendMessageAsync($"Error: {result.ErrorReason}");
            }
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
                List<Task> cmds = new List<Task>()
                {
                    u.SendMessageAsync($":rotating_light: :rotating_light: The FBI of {u.Guild.Name} is currently in Raid Mode. As a result, you may not join the server at this time.:rotating_light: :rotating_light:"),
                    RaidMode.AddBlockedUserAsync(u)
                };
                await Task.WhenAll(cmds);
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

            if (await Verify.GetVerifiedAsync(u))
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

            SocketCommandContext Context = new SocketCommandContext(_client, msg);
            string _prefix = Context.Guild != null ? await SetPrefix.GetPrefixAsync(Context.Guild) : prefix;
            bool isCommand = msg.HasMentionPrefix(_client.CurrentUser, ref argPos) || msg.HasStringPrefix(_prefix, ref argPos);

            if (isCommand)
            {
                var result = await _commands.ExecuteAsync(Context, argPos, _services);

                List<Task> cmds = new List<Task>();
                if (!result.IsSuccess && result.Error == CommandError.UnmetPrecondition)
                {
                    cmds.Add(Context.Channel.SendMessageAsync(result.ErrorReason));
                }

                if (msg.Author.IsBot)
                {
                    cmds.Add(msg.DeleteAsync());
                }

                await Task.WhenAll(cmds);
            }

            if (msg.Author.IsBot)
            {
                return;
            }

            await AutoModAsync(Context, isCommand);
        }

        private static async Task AutoModAsync(SocketCommandContext Context, bool isCommand)
        {
            if (await AutoSurveillance.GetAutoSurveillanceAsync(Context.Guild))
            {
                if (await Pedophile.IsPedophileAsync(Context))
                {
                    await new Pedophile(Context).ArrestAsync();
                    return;
                }
            }
            if (((await Spam.IsSpamAsync(Context) && await AntiSpam.GetAntiSpamAsync(Context.Guild))
                || (await Spam.IsSingleSpamAsync(Context) && await AntiSingleSpam.GetAntiSingleSpamAsync(Context.Guild))) && !isCommand)
            {
                await new Spam(Context).WarnAsync();
            }
            else if (await MassMention.IsMassMentionAsync(Context) && await AntiMassMention.GetAntiMassMentionAsync(Context.Guild))
            {
                await new MassMention(Context).WarnAsync();
            }
            else if (await CAPS.ISCAPSASYNC(Context) && await AntiCaps.GetAntiCapsAsync(Context.Guild))
            {
                await new CAPS(Context).WARNASYNC();
            }
            else if (await Invite.HasInviteAsync(Context) && await AntiInvite.GetAntiInviteAsync(Context.Guild))
            {
                await new Invite(Context).RemoveAsync();
            }
            else if (await Link.HasLinkAsync(Context) && await AntiLink.GetAntiLinkAsync(Context.Guild))
            {
                await new Link(Context).RemoveAsync();
            }
        }
    }
}
