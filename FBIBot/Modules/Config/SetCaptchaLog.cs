using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Config
{
    public class SetCaptchaLog : ModuleBase<SocketCommandContext>
    {
        [Command("setcaptchalog")]
        [Alias("set-captchalog", "set-captcha-log")]
        [RequireAdmin]
        public async Task SetCaptchaLogAsync()
        {
            if (await modLogsDatabase.CaptchaLogChannel.GetCaptchaLogChannelAsync(Context.Guild) == null)
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that you are already lacking a CAPTCHA log channel.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription("CAPTCHA logs will now go undisclosed. That information was confidential, anyways.");

            await Task.WhenAll
            (
                modLogsDatabase.CaptchaLogChannel.RemoveCaptchaLogChannelAsync(Context.Guild),
                Context.Channel.SendMessageAsync("", false, embed.Build())
            );
        }

        [Command("setcaptchalog")]
        [Alias("set-captchalog", "set-captcha-log")]
        [RequireAdmin]
        public async Task SetCaptchaLogAsync(SocketTextChannel channel)
        {
            if (await modLogsDatabase.CaptchaLogChannel.GetCaptchaLogChannelAsync(Context.Guild) == channel)
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that {channel.Mention} is already configured for CAPTCHA logs.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"Once-confidential CAPTCHA logs will now be disclosed to {channel.Mention}.");

            await Task.WhenAll
            (
                modLogsDatabase.CaptchaLogChannel.SetCaptchaLogChannelAsync(channel),
                Context.Channel.SendMessageAsync("", false, embed.Build())
            );
        }

        [Command("setcaptchalog")]
        [Alias("set-captchalog", "set-captcha-log")]
        public async Task SetCaptchaLogAsync(string channel)
        {
            SocketTextChannel c;
            if (ulong.TryParse(channel, out ulong channelID) && (c = Context.Guild.GetTextChannel(channelID)) != null)
            {
                await SetCaptchaLogAsync(c);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given text channel does not exist.");
        }
    }
}
