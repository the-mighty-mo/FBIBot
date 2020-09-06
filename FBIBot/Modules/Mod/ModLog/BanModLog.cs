using Discord;
using Discord.WebSocket;
using FBIBot.Modules.Config;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod.ModLog
{
    public static class BanModLog
    {
        public static async Task SendToModLogAsync(SocketGuildUser invoker, SocketGuildUser target, string timeout, string reason)
            => await SendToModLogAsync(invoker, target.Id, timeout, reason);

        public static async Task SendToModLogAsync(SocketGuildUser invoker, ulong? target, string timeout, string reason)
        {
            ulong id = await ModLogBase.GetNextModLogID(invoker.Guild);
            SocketTextChannel channel = await SetModLog.GetModLogChannelAsync(invoker.Guild);

            if (channel == null)
            {
                return;
            }
            bool isTime = double.TryParse(timeout, out double time);

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(130, 0, 0))
                .WithTitle($"Federal Bureau of Investigation - Log {id}")
                .WithCurrentTimestamp();

            EmbedFieldBuilder command = new EmbedFieldBuilder()
                .WithIsInline(false)
                .WithName($"Ban User{(isTime ? $" for {time} {(time == 1 ? "day" : "days")}" : "")}")
                .WithValue($"<@{target}>");
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
