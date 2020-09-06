using Discord;
using Discord.WebSocket;
using FBIBot.Modules.Config;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod.ModLog
{
    public static class RemoveWarningModLog
    {
        public static async Task SendToModLogAsync(SocketGuildUser invoker, SocketGuildUser target, string warning)
        {
            ulong id = await ModLogBase.GetNextModLogID(invoker.Guild);
            SocketTextChannel channel = await SetModLog.GetModLogChannelAsync(invoker.Guild);

            if (channel == null)
            {
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(12, 156, 24))
                .WithTitle($"Federal Bureau of Investigation - Log {id}")
                .WithCurrentTimestamp();

            EmbedFieldBuilder command = new EmbedFieldBuilder()
                .WithIsInline(false)
                .WithName($"Remove Warning {warning} from User")
                .WithValue($"{target.Mention}");
            embed.AddField(command);

            EmbedFieldBuilder invoked = new EmbedFieldBuilder()
                .WithIsInline(false)
                .WithName("Invoked by")
                .WithValue(invoker.Mention);
            embed.AddField(invoked);

            var msg = await channel.SendMessageAsync("", false, embed.Build());
            if (msg != null)
            {
                await ModLogBase.SaveModLogAsync(msg, invoker.Guild, id);
            }
        }
    }
}
