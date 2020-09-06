using Discord;
using Discord.WebSocket;
using FBIBot.Modules.Config;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod.ModLog
{
    public static class WarnModLog
    {
        public static async Task SendToModLogAsync(SocketGuildUser invoker, SocketGuildUser target, string timeout, string reason)
        {
            ulong id = await ModLogBase.GetNextModLogID(invoker.Guild);
            SocketTextChannel channel = await SetModLog.GetModLogChannelAsync(invoker.Guild);

            if (channel == null)
            {
                return;
            }
            bool isTime = double.TryParse(timeout, out double time);
            bool isMinutes = time < 1;
            if (time < 1)
            {
                time *= 60;
            } 

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(255, 213, 31))
                .WithTitle($"Federal Bureau of Investigation - Log {id}")
                .WithCurrentTimestamp();

            EmbedFieldBuilder command = new EmbedFieldBuilder()
                .WithIsInline(false)
                .WithName($"Warn User{(isTime ? $" for {time} {(time == 1 ? isMinutes ? "minute" : "hour" : isMinutes ? "minutes" : "hours")}" : "")}")
                .WithValue($"{target.Mention}");
            embed.AddField(command);

            EmbedFieldBuilder invoked = new EmbedFieldBuilder()
                .WithIsInline(false)
                .WithName("Invoked by")
                .WithValue(invoker.Mention);
            embed.AddField(invoked);

            EmbedFieldBuilder field = new EmbedFieldBuilder()
                .WithIsInline(false)
                .WithName("Reason")
                .WithValue(reason ?? "(none given)");
            embed.AddField(field);

            var msg = await channel.SendMessageAsync("", false, embed.Build());
            if (msg != null)
            {
                await ModLogBase.SaveModLogAsync(msg, invoker.Guild, id);
            }
        }
    }
}
