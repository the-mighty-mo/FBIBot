using Discord;
using Discord.Commands;
using Discord.Interactions;
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
        private readonly InteractionService interactions;
        private readonly IServiceProvider services;

        public CommandHandler(DiscordSocketClient client, IServiceProvider services)
        {
            this.client = client;
            this.services = services;

            InteractionServiceConfig interactionCfg = new()
            {
                DefaultRunMode = Discord.Interactions.RunMode.Async
            };
            interactions = new(client.Rest, interactionCfg);

            CommandServiceConfig commandCfg = new()
            {
                DefaultRunMode = Discord.Commands.RunMode.Async
            };
            commands = new(commandCfg);
        }

        public async Task InitCommandsAsync()
        {
            client.Ready += ReadyAsync;
            client.Connected += SendConnectMessage;
            client.Disconnected += SendDisconnectError;
            client.UserJoined += SendWelcomeMessage;
            client.GuildMemberUpdated += CheckUsernameAsync;
            client.MessageReceived += HandleCommandAsync;
            client.SlashCommandExecuted += HandleSlashCommandAsync;

            await Task.WhenAll(
                interactions.AddModulesAsync(Assembly.GetEntryAssembly(), services),
                commands.AddModulesAsync(Assembly.GetEntryAssembly(), services)
            ).ConfigureAwait(false);
            interactions.SlashCommandExecuted += SendInteractionErrorAsync;
            commands.CommandExecuted += SendCommandErrorAsync;
        }

        private Task ReadyAsync() =>
            interactions.RegisterCommandsGloballyAsync(true);

        private async Task SendInteractionErrorAsync(SlashCommandInfo info, IInteractionContext context, Discord.Interactions.IResult result)
        {
            if (!result.IsSuccess && info.RunMode == Discord.Interactions.RunMode.Async && result.Error is not InteractionCommandError.UnknownCommand)
            {
                if (result.Error is InteractionCommandError.UnmetPrecondition)
                {
                    await context.Interaction.RespondAsync($"Error: {result.ErrorReason}").ConfigureAwait(false);
                }
                else
                {
                    await context.Channel.SendMessageAsync($"Error: {result.ErrorReason}").ConfigureAwait(false);
                }
            }
        }

        private async Task SendCommandErrorAsync(Optional<CommandInfo> info, ICommandContext context, Discord.Commands.IResult result)
        {
            if (!result.IsSuccess && info.GetValueOrDefault()?.RunMode == Discord.Commands.RunMode.Async && result.Error is not (CommandError.UnknownCommand or CommandError.UnmetPrecondition))
            {
                await context.Channel.SendMessageAsync($"Error: {result.ErrorReason}").ConfigureAwait(false);
            }
        }

        private Task SendConnectMessage() =>
            Console.Out.WriteLineAsync($"{SecurityInfo.botName} is online");

        private Task SendDisconnectError(Exception e) =>
            Console.Out.WriteLineAsync(e.Message);

        private async Task SendWelcomeMessage(SocketGuildUser u)
        {
            if (await raidModeDatabase.RaidMode.GetVerificationLevelAsync(u.Guild).ConfigureAwait(false) != null && !u.IsBot)
            {
                await Task.WhenAll
                (
                    u.SendMessageAsync($":rotating_light: :rotating_light: The FBI Office of {u.Guild.Name} is currently in Raid Mode. As a result, you may not join the server at this time. :rotating_light: :rotating_light:"),
                    raidModeDatabase.UsersBlocked.AddBlockedUserAsync(u)
                ).ConfigureAwait(false);
                await u.KickAsync("FBI RAID MODE").ConfigureAwait(false);
                return;
            }

            SocketTextChannel? channel = await modLogsDatabase.WelcomeChannel.GetWelcomeChannelAsync(u.Guild).ConfigureAwait(false);
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
                int index = Program.Rng.Next(messages.Count);
                await channel.SendMessageAsync($"{u.Mention} {messages[index]}").ConfigureAwait(false);
            }

            if (await verificationDatabase.Verified.GetVerifiedAsync(u).ConfigureAwait(false))
            {
                SocketRole? role = await verificationDatabase.Roles.GetVerificationRoleAsync(u.Guild).ConfigureAwait(false);
                if (role != null && u.Guild.CurrentUser.GetPermissions(u.Guild.DefaultChannel).ManageRoles)
                {
                    await u.AddRoleAsync(role).ConfigureAwait(false);
                }
            }
            else if (!u.IsBot && await verificationDatabase.Roles.GetVerificationRoleAsync(u.Guild).ConfigureAwait(false) != null)
            {
                await Verify.SendCaptchaAsync(u).ConfigureAwait(false);
            }

            string name = u.Nickname ?? u.Username;
            Task<bool>[] isZalgo =
            {
                Zalgo.IsZalgoAsync(name),
                configDatabase.AntiZalgo.GetAntiZalgoAsync(u.Guild)
            };

            if ((await Task.WhenAll(isZalgo).ConfigureAwait(false)).All(x => x))
            {
                try
                {
                    await u.ModifyAsync(async x => x.Nickname = await Zalgo.RemoveZalgoAsync(name).ConfigureAwait(false)).ConfigureAwait(false);
                }
                catch { }
            }
        }

        private async Task CheckUsernameAsync(Cacheable<SocketGuildUser, ulong> orig, SocketGuildUser updated)
        {
            if (!orig.HasValue)
            {
                /* we never cached the original */
                return;
            }

            string oldName = orig.Value.Nickname ?? orig.Value.Username;
            string newName = updated.Nickname ?? updated.Username;

            if (oldName == newName)
            {
                return;
            }

            Task<bool>[] isZalgo =
            {
                Zalgo.IsZalgoAsync(newName),
                configDatabase.AntiZalgo.GetAntiZalgoAsync(updated.Guild)
            };

            if ((await Task.WhenAll(isZalgo).ConfigureAwait(false)).All(x => x))
            {
                try
                {
                    await updated.ModifyAsync(x => x.Nickname = oldName).ConfigureAwait(false);
                }
                catch { }
            }
        }

        private Task<bool> CanBotRunCommandsAsync(SocketUser usr) =>
            Task.FromResult(usr.Id == client.CurrentUser.Id);

        private static Task<bool> ShouldDeleteBotCommands() =>
            Task.FromResult(true);

        private async Task HandleSlashCommandAsync(SocketSlashCommand m)
        {
            if (m.User.IsBot && !await CanBotRunCommandsAsync(m.User).ConfigureAwait(false))
            {
                return;
            }

            SocketInteractionContext Context = new(client, m);

            await interactions.ExecuteCommandAsync(Context, services).ConfigureAwait(false);

            List<Task> cmds = new();
            if (m.User.IsBot && await ShouldDeleteBotCommands().ConfigureAwait(false))
            {
                cmds.Add(m.DeleteOriginalResponseAsync());
            }

            await Task.WhenAll(cmds).ConfigureAwait(false);
        }

        private async Task HandleCommandAsync(SocketMessage m)
        {
            if (m is not SocketUserMessage msg || (msg.Author.IsBot && !await CanBotRunCommandsAsync(msg.Author).ConfigureAwait(false)))
            {
                return;
            }

            SocketCommandContext Context = new(client, msg);
            bool isCommand = msg.HasMentionPrefix(client.CurrentUser, ref argPos) || msg.HasStringPrefix(prefix, ref argPos);

            if (isCommand)
            {
                var result = await commands.ExecuteAsync(Context, argPos, services).ConfigureAwait(false);

                List<Task> cmds = new();
                if (msg.Author.IsBot && await ShouldDeleteBotCommands().ConfigureAwait(false))
                {
                    cmds.Add(msg.DeleteAsync());
                }
                else if (!result.IsSuccess && result.Error == CommandError.UnmetPrecondition)
                {
                    cmds.Add(Context.Channel.SendMessageAsync(result.ErrorReason));
                }

                await Task.WhenAll(cmds).ConfigureAwait(false);
            }

            if (!msg.Author.IsBot)
            {
                await AutoModAsync(Context, isCommand).ConfigureAwait(false);
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

            if (await configDatabase.AutoSurveillance.GetAutoSurveillanceAsync(Context.Guild).ConfigureAwait(false))
            {
                if (await AutoSurveillanceAsync(Context).ConfigureAwait(false))
                {
                    return;
                }
            }

            if ((await Task.WhenAll(isZalgo).ConfigureAwait(false)).All(x => x))
            {
                await new Zalgo(Context).WarnAsync().ConfigureAwait(false);
            }
            else if (((await Task.WhenAll(isSpam).ConfigureAwait(false)).All(x => x)
                || (await Task.WhenAll(isSingleSpam).ConfigureAwait(false)).All(x => x))
                && !isCommand)
            {
                await new Spam(Context).WarnAsync().ConfigureAwait(false);
            }
            else if ((await Task.WhenAll(isMassMention).ConfigureAwait(false)).All(x => x))
            {
                await new MassMention(Context).WarnAsync().ConfigureAwait(false);
            }
            else if ((await Task.WhenAll(isCaps).ConfigureAwait(false)).All(x => x))
            {
                await new CAPS(Context).WARNASYNC().ConfigureAwait(false);
            }
            else if ((await Task.WhenAll(isInvite).ConfigureAwait(false)).All(x => x))
            {
                await new Invite(Context).RemoveAsync().ConfigureAwait(false);
            }
            else if ((await Task.WhenAll(isLink).ConfigureAwait(false)).All(x => x))
            {
                await new Link(Context).RemoveAsync().ConfigureAwait(false);
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

            if ((await Task.WhenAll(shouldArrest).ConfigureAwait(false)).Contains(true))
            {
                await new AutoSurveillanceArrest(Context).ArrestAsync().ConfigureAwait(false);
                return true;
            }
            else if (await noRedSam.ConfigureAwait(false))
            {
                await new NoRedSam(Context).CleanseSamAsync().ConfigureAwait(false);
                return true;
            }
            return false;
        }
    }
}