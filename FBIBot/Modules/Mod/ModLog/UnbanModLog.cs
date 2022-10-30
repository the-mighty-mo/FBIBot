using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod.ModLog
{
    public static class UnbanModLog
    {
        public static Task SendToModLogAsync(SocketGuildUser invoker, SocketGuildUser target)
            => SendToModLogAsync(invoker, target.Id);

        public static Task SendToModLogAsync(SocketGuildUser invoker, ulong? target) =>
            ModLogBase.SendToModLogAsync(
                new ModLogBase.ModLogInfo(
                    new ModLogBase.ModLogInfo.RequiredInfo(
                        invoker,
                        new Color(12, 156, 24),
                        "Unban User",
                        $"<@{target}>"
                    )
                )
            );
    }
}