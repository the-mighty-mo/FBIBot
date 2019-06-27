using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FBIBot.Modules.Config;

namespace FBIBot.Modules.Mod
{
    public class SendToModLog
    {
        public enum LogType
        {
            Warn,
            Mute,
            Arrest,
            Kick,
            Ban,
            Unmute,
            Free,
            Unban
        }

        public static async Task SendToModLogAsync(LogType t, SocketUser invoker, SocketGuildUser u, string length = null, string reason = null)
        {
            Color color;
            bool reasonAllowed = true;
            bool isTime = double.TryParse(length, out double time);

            switch (t)
            {
            case LogType.Warn:
                color = new Color(228, 226, 24);
                break;
            case LogType.Mute:
                color = new Color(255, 110, 24);
                if (isTime)
                {
                    length += $" {(time == 1 ? "minute" : "minutes")}";
                }
                break;
            case LogType.Arrest:
                reason = "*No reason necessary*";
                if (isTime)
                {
                    length += $" {(time == 1 ? "minute" : "minutes")}";
                }
                color = new Color(255, 61, 24);
                break;
            case LogType.Kick:
                color = new Color(255, 12, 12);
                break;
            case LogType.Ban:
                if (isTime)
                {
                    length += $" {(time == 1 ? "day" : "days")}";
                }
                color = new Color(130, 0, 0);
                break;
            case LogType.Unmute:
            case LogType.Free:
            case LogType.Unban:
            default:
                reasonAllowed = false;
                color = new Color(12, 156, 24);
                break;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(color)
                .WithTitle("Federal Bureau of Investigation - Log")
                .WithCurrentTimestamp();

            EmbedFieldBuilder affected = new EmbedFieldBuilder()
                .WithIsInline(false)
                .WithName($"{t.ToString()} User{(isTime ? $" for {length}" : "")}")
                .WithValue(u.Mention);
            embed.AddField(affected);

            EmbedFieldBuilder invoked = new EmbedFieldBuilder()
                .WithIsInline(false)
                .WithName("Invoked by")
                .WithValue(invoker.Mention);
            embed.AddField(invoked);

            if (reasonAllowed)
            {
                EmbedFieldBuilder field = new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName("Reason")
                    .WithValue(reason ?? "(none given)");
                embed.AddField(field);
            }

            await (await SetModLog.GetModLogChannelAsync(u.Guild))?.SendMessageAsync("", false, embed.Build());
        }
    }
}
