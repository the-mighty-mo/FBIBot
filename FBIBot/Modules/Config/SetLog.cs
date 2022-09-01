using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Config
{
    [Group("set-log", "Sets a log channel for bot actions")]
    [RequireAdmin]
    public class SetLog : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("captcha", "Sets the channel for the CAPTCHA Log. *Unsets if no channel is given*")]
        public async Task SetCaptchaLogAsync(SocketTextChannel channel = null)
        {
            if (channel is null)
            {
                await SetCaptchaLogPrivAsync();
            }
            else
            {
                await SetCaptchaLogPrivAsync(channel);
            }
        }

        private async Task SetCaptchaLogPrivAsync()
        {
            if (await modLogsDatabase.CaptchaLogChannel.GetCaptchaLogChannelAsync(Context.Guild) == null)
            {
                await Context.Interaction.RespondAsync($"Our security team has informed us that you are already lacking a CAPTCHA log channel.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription("CAPTCHA logs will now go undisclosed. That information was confidential, anyways.");

            await Task.WhenAll
            (
                modLogsDatabase.CaptchaLogChannel.RemoveCaptchaLogChannelAsync(Context.Guild),
                Context.Interaction.RespondAsync(embed: embed.Build())
            );
        }

        private async Task SetCaptchaLogPrivAsync(SocketTextChannel channel)
        {
            if (await modLogsDatabase.CaptchaLogChannel.GetCaptchaLogChannelAsync(Context.Guild) == channel)
            {
                await Context.Interaction.RespondAsync($"Our security team has informed us that {channel.Mention} is already configured for CAPTCHA logs.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"Once-confidential CAPTCHA logs will now be disclosed to {channel.Mention}.");

            await Task.WhenAll
            (
                modLogsDatabase.CaptchaLogChannel.SetCaptchaLogChannelAsync(channel),
                Context.Interaction.RespondAsync(embed: embed.Build())
            );
        }

        [SlashCommand("mod", "Sets the channel for the Mod Log. *Unsets if no channel is given*")]
        public async Task SetModLogAsync(SocketTextChannel channel = null)
        {
            if (channel is null)
            {
                await SetModLogPrivAsync();
            }
            else
            {
                await SetModLogPrivAsync(channel);
            }
        }

        private async Task SetModLogPrivAsync()
        {
            if (await modLogsDatabase.ModLogChannel.GetModLogChannelAsync(Context.Guild) == null)
            {
                await Context.Interaction.RespondAsync($"Our security team has informed us that you are already lacking a mod log channel.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription("Moderation logs will now go undisclosed. That information was confidential, anyways.");

            await Task.WhenAll
            (
                modLogsDatabase.ModLogChannel.RemoveModLogChannelAsync(Context.Guild),
                Context.Interaction.RespondAsync(embed: embed.Build())
            );
        }

        private async Task SetModLogPrivAsync(SocketTextChannel channel)
        {
            if (await modLogsDatabase.ModLogChannel.GetModLogChannelAsync(Context.Guild) == channel)
            {
                await Context.Interaction.RespondAsync($"Our security team has informed us that {channel.Mention} is already configured for mod logs.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"Once-confidential moderation logs will now be disclosed to {channel.Mention}.");

            await Task.WhenAll
            (
                modLogsDatabase.ModLogChannel.SetModLogChannelAsync(channel),
                Context.Interaction.RespondAsync(embed: embed.Build())
            );
        }

        [SlashCommand("welcome", "Sets the channel for welcome messages. *Unsets if no channel is given*")]
        public async Task SetWelcomeAsync(SocketTextChannel channel = null)
        {
            if (channel is null)
            {
                await SetWelcomePrivAsync();
            }
            else
            {
                await SetWelcomePrivAsync(channel);
            }
        }

        private async Task SetWelcomePrivAsync()
        {
            if (await modLogsDatabase.WelcomeChannel.GetWelcomeChannelAsync(Context.Guild) == null)
            {
                await Context.Interaction.RespondAsync($"Our security team has informed us that you are already lacking a welcome channel.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription("Welcome messages have been discontinued. They gave our agents away, anyways.");

            await Task.WhenAll
            (
                modLogsDatabase.WelcomeChannel.RemoveWelcomeChannelAsync(Context.Guild),
                Context.Interaction.RespondAsync(embed: embed.Build())
            );
        }

        private async Task SetWelcomePrivAsync(SocketTextChannel channel)
        {
            if (await modLogsDatabase.WelcomeChannel.GetWelcomeChannelAsync(Context.Guild) == channel)
            {
                await Context.Interaction.RespondAsync($"Our security team has informed us that {channel.Mention} is already configured for welcome messages.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"Welcome messages will now be sent to {channel.Mention}.");

            await Task.WhenAll
            (
                modLogsDatabase.WelcomeChannel.SetWelcomeChannelAsync(channel),
                Context.Interaction.RespondAsync(embed: embed.Build())
            );
        }
    }
}