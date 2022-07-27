using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.AutoMod
{
    public static class SendToCaptchaLog
    {
        public enum CaptchaType
        {
            Requested,
            Failed,
            Completed,
            OutOfAttempts
        }

        private class CaptchaLogInfo
        {
            public CaptchaType t;
            public SocketGuildUser user;
            public string code;
            public string given;
            public int attempts;

            public string result;
            public Color color;

            public CaptchaLogInfo(CaptchaType t, SocketGuildUser user, string code, string given = null, int attempts = 0)
            {
                this.t = t;
                this.user = user;
                this.code = code;
                this.given = given;
                this.attempts = attempts;
            }
        }

        public static Task SendToCaptchaLogAsync(CaptchaType t, SocketGuildUser user, string code, string given = null, int attempts = 0)
        {
            CaptchaLogInfo info = new(t, user, code, given, attempts);
            CAPTCHATypeSwitch(ref info);

            return SendToCaptchaLogAsync(info);
        }

        private static async Task SendToCaptchaLogAsync(CaptchaLogInfo info)
        {
            SocketTextChannel channel = await modLogsDatabase.CaptchaLogChannel.GetCaptchaLogChannelAsync(info.user.Guild);
            if (channel == null)
            {
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(info.color)
                .WithTitle($"Federal Bureau of Investigation - {info.result}")
                .WithCurrentTimestamp();

            EmbedFieldBuilder captchaResult = new EmbedFieldBuilder()
                .WithIsInline(true)
                .WithName("User")
                .WithValue(info.user.Mention);
            embed.AddField(captchaResult);

            if (info.attempts != 0)
            {
                EmbedFieldBuilder attempts = new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName("Attempts:")
                    .WithValue($"{info.attempts}/5");
                embed.AddField(attempts);
            }

            EmbedFieldBuilder captchaCode = new EmbedFieldBuilder()
                .WithIsInline(true)
                .WithName("Looking for")
                .WithValue(info.code);
            embed.AddField(captchaCode);

            if (info.given != null)
            {
                EmbedFieldBuilder resultGiven = new EmbedFieldBuilder()
                    .WithIsInline(true)
                    .WithName("Given")
                    .WithValue(info.given);
                embed.AddField(resultGiven);
            }

            await channel.SendMessageAsync(embed: embed.Build());
        }

        private static void CAPTCHATypeSwitch(ref CaptchaLogInfo info)
        {
            switch (info.t)
            {
            case CaptchaType.Completed:
                info.result = "CAPTCHA Completed";
                info.color = new Color(0, 150, 0);
                break;
            case CaptchaType.Failed:
                info.result = "CAPTCHA Attempt Failed";
                info.color = new Color(255, 12, 12);
                break;
            case CaptchaType.OutOfAttempts:
                info.result = "CAPTCHA Failed";
                info.color = new Color(130, 0, 0);
                break;
            case CaptchaType.Requested:
            default:
                info.result = "CAPTCHA Sent";
                info.color = SecurityInfo.botColor;
                break;
            }
        }
    }
}