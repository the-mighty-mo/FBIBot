using Discord;
using Discord.WebSocket;
using FBIBot.Modules.Config;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod.ModLog
{
    public static class RaidModeModLog
    {
        public static async Task SendToModLogAsync(SocketGuildUser invoker, bool isEnabled)
        {
            ulong id = await ModLogManager.GetNextModLogID(invoker.Guild);
            SocketTextChannel channel = await SetModLog.GetModLogChannelAsync(invoker.Guild);

            if (channel == null)
            {
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(isEnabled ? new Color(206, 15, 65) : new Color(12, 156, 24))
                .WithTitle($"Federal Bureau of Investigation - Log {id}")
                .WithCurrentTimestamp();

            EmbedFieldBuilder command = new EmbedFieldBuilder()
                .WithIsInline(false)
                .WithName("Raid Mode")
                .WithValue(isEnabled ? "Enabled" : "Disabled");
            embed.AddField(command);

            EmbedFieldBuilder invoked = new EmbedFieldBuilder()
                .WithIsInline(false)
                .WithName("Invoked by")
                .WithValue(invoker.Mention);
            embed.AddField(invoked);

            var msg = await channel.SendMessageAsync("", false, embed.Build());
            if (msg != null)
            {
                await ModLogManager.SaveModLogAsync(msg, invoker.Guild, id);
            }
        }
    }
}
