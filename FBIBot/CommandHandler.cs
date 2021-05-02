using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FBIBot.Modules.AutoMod;
using FBIBot.Modules.AutoMod.AutoSurveillance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot
{
    public class CommandHandler
    {
        public const string prefix = "\\";
        private static int argPos = 0;

        private readonly DiscordSocketClient client;
        private readonly CommandService commands;
        private readonly IServiceProvider services;

        public CommandHandler(DiscordSocketClient client, IServiceProvider services)
        {
            this.client = client;
            this.services = services;

            CommandServiceConfig config = new()
            {
                DefaultRunMode = RunMode.Async
            };
            commands = new CommandService(config);
        }

        public async Task InitCommandsAsync()
        {
            client.Connected += SendConnectMessage;
            client.Disconnected += SendDisconnectError;
            client.JoinedGuild += SendJoinMessage;
            client.UserJoined += SendWelcomeMessage;
            client.GuildMemberUpdated += CheckUsernameAsync;
            client.MessageReceived += HandleCommandAsync;

            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
            commands.CommandExecuted += SendErrorAsync;
        }

        private async Task SendErrorAsync(Optional<CommandInfo> info, ICommandContext context, IResult result)
        {
            if (!result.IsSuccess && info.Value.RunMode == RunMode.Async && result.Error != CommandError.UnknownCommand && result.Error != CommandError.UnmetPrecondition)
            {
                await context.Channel.SendMessageAsync($"Error: {result.ErrorReason}");
            }
        }

        private async Task SendConnectMessage() =>
            await Console.Out.WriteLineAsync($"{SecurityInfo.botName} is online");

        private async Task SendDisconnectError(Exception e) =>
            await Console.Out.WriteLineAsync(e.Message);

        private async Task SendJoinMessage(SocketGuild g) =>
            await g.DefaultChannel.SendMessageAsync("Someone called for some democracy and justice?");

        private async Task SendWelcomeMessage(SocketGuildUser u)
        {
            if (await raidModeDatabase.RaidMode.GetVerificationLevelAsync(u.Guild) != null && !u.IsBot)
            {
                await Task.WhenAll
                (
                    u.SendMessageAsync($":rotating_light: :rotating_light: The FBI of {u.Guild.Name} is currently in Raid Mode. As a result, you may not join the server at this time. :rotating_light: :rotating_light:"),
                    raidModeDatabase.UsersBlocked.AddBlockedUserAsync(u)
                );
                await u.KickAsync("FBI RAID MODE");
                return;
            }

            SocketTextChannel channel = await modLogsDatabase.WelcomeChannel.GetWelcomeChannelAsync(u.Guild);
            if (channel != null)
            {
                List<string> messages = new()
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
            }

            if (await verificationDatabase.Verified.GetVerifiedAsync(u))
            {
                SocketRole role = await verificationDatabase.Roles.GetVerificationRoleAsync(u.Guild);
                if (role != null && u.Guild.CurrentUser.GetPermissions(u.Guild.DefaultChannel).ManageRoles)
                {
                    await u.AddRoleAsync(role);
                }
            }
            else if (!u.IsBot && await verificationDatabase.Roles.GetVerificationRoleAsync(u.Guild) != null)
            {
                await Verify.SendCaptchaAsync(u);
            }

            string name = u.Nickname ?? u.Username;
            Task<bool>[] isZalgo =
            {
                Zalgo.IsZalgoAsync(name),
                configDatabase.AntiZalgo.GetAntiZalgoAsync(u.Guild)
            };

            if ((await Task.WhenAll(isZalgo)).All(x => x))
            {
                try
                {
                    await u.ModifyAsync(async x => x.Nickname = await Zalgo.RemoveZalgoAsync(name));
                }
                catch { }
            }
        }

        private async Task CheckUsernameAsync(SocketGuildUser orig, SocketGuildUser updated)
        {
            string oldName = orig.Nickname ?? orig.Username;
            string newName = updated.Nickname ?? updated.Username;

            Task<bool>[] isZalgo =
            {
                Zalgo.IsZalgoAsync(newName),
                configDatabase.AntiZalgo.GetAntiZalgoAsync(updated.Guild)
            };

            if ((await Task.WhenAll(isZalgo)).All(x => x))
            {
                try
                {
                    await updated.ModifyAsync(x => x.Nickname = oldName);
                }
                catch { }
            }
        }

        private async Task<bool> CanBotRunCommandsAsync(SocketUserMessage msg) =>
            await Task.Run(() => msg.Author.Id == client.CurrentUser.Id);

        private async Task<bool> ShouldDeleteBotCommands() =>
            await Task.Run(() => true);

        private async Task HandleCommandAsync(SocketMessage m)
        {
            if (m is not SocketUserMessage msg || (msg.Author.IsBot && !await CanBotRunCommandsAsync(msg)))
            {
                return;
            }

            SocketCommandContext Context = new(client, msg);
            string _prefix = Context.Guild != null ? await configDatabase.Prefixes.GetPrefixAsync(Context.Guild) : prefix;
            bool isCommand = msg.HasMentionPrefix(client.CurrentUser, ref argPos) || msg.HasStringPrefix(_prefix, ref argPos);

            if (isCommand)
            {
                IResult result = await commands.ExecuteAsync(Context, argPos, services);

                List<Task> cmds = new();
                if (msg.Author.IsBot && await ShouldDeleteBotCommands())
                {
                    cmds.Add(msg.DeleteAsync());
                }
                else if (!result.IsSuccess && result.Error == CommandError.UnmetPrecondition)
                {
                    cmds.Add(Context.Channel.SendMessageAsync(result.ErrorReason));
                }

                await Task.WhenAll(cmds);
            }

            if (!msg.Author.IsBot)
            {
                await AutoModAsync(Context, isCommand);
            }
        }

        private static async Task AutoModAsync(SocketCommandContext Context, bool isCommand)
        {
            Task<bool>[] isZalgo =
            {
                Zalgo.IsZalgoAsync(Context),
                configDatabase.AntiZalgo.GetAntiZalgoAsync(Context.Guild)
            };
            Task<bool>[] isSpam =
            {
                Spam.IsSpamAsync(Context),
                configDatabase.AntiSpam.GetAntiSpamAsync(Context.Guild)
            };
            Task<bool>[] isSingleSpam =
            {
                Spam.IsSingleSpamAsync(Context),
                configDatabase.AntiSingleSpam.GetAntiSingleSpamAsync(Context.Guild)
            };
            Task<bool>[] isMassMention =
            {
                MassMention.IsMassMentionAsync(Context),
                configDatabase.AntiMassMention.GetAntiMassMentionAsync(Context.Guild)
            };
            Task<bool>[] isCaps =
            {
                CAPS.ISCAPSASYNC(Context),
                configDatabase.AntiCaps.GetAntiCapsAsync(Context.Guild)
            };
            Task<bool>[] isInvite =
            {
                Invite.HasInviteAsync(Context),
                configDatabase.AntiInvite.GetAntiInviteAsync(Context.Guild)
            };
            Task<bool>[] isLink =
            {
                Link.IsLinkAsync(Context),
                configDatabase.AntiLink.GetAntiLinkAsync(Context.Guild)
            };

            if (await configDatabase.AutoSurveillance.GetAutoSurveillanceAsync(Context.Guild))
            {
                if (await AutoSurveillanceAsync(Context))
                {
                    return;
                }
            }

            if ((await Task.WhenAll(isZalgo)).All(x => x))
            {
                await new Zalgo(Context).WarnAsync();
            }
            else if (((await Task.WhenAll(isSpam)).All(x => x) || (await Task.WhenAll(isSingleSpam)).All(x => x)) && !isCommand)
            {
                await new Spam(Context).WarnAsync();
            }
            else if ((await Task.WhenAll(isMassMention)).All(x => x))
            {
                await new MassMention(Context).WarnAsync();
            }
            else if ((await Task.WhenAll(isCaps)).All(x => x))
            {
                await new CAPS(Context).WARNASYNC();
            }
            else if ((await Task.WhenAll(isInvite)).All(x => x))
            {
                await new Invite(Context).RemoveAsync();
            }
            else if ((await Task.WhenAll(isLink)).All(x => x))
            {
                await new Link(Context).RemoveAsync();
            }
        }

        private static async Task<bool> AutoSurveillanceAsync(SocketCommandContext Context)
        {
            Task<bool>[] shouldArrest =
            {
                Pedophile.IsPedophileAsync(Context),
                KillThePresident.IsGoingToKillThePresidentAsync(Context),
                LostTheGame.HasLostTheGameAsync(Context)
            };
            Task<bool> noRedSam = NoRedSam.IsRedSamAsync(Context);

            if ((await Task.WhenAll(shouldArrest)).Contains(true))
            {
                await new AutoSurveillanceArrest(Context).ArrestAsync();
                return true;
            }
            else if (await noRedSam)
            {
                await new NoRedSam(Context).CleanseSamAsync();
                return true;
            }
            return false;
        }
    }
}