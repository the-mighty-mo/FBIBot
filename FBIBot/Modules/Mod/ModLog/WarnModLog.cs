using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod.ModLog
{
    public static class WarnModLog
    {
        public static Task SendToModLogAsync(SocketGuildUser invoker, SocketGuildUser target, double? timeout, string? reason)
        {
            bool isMinutes = timeout < 1;
            if (timeout < 1)
            {
                timeout *= 60;
            }
            return ModLogBase.SendToModLogAsync(
                new ModLogBase.ModLogInfo(
                    new ModLogBase.ModLogInfo.RequiredInfo(
                        invoker,
                        new Color(255, 213, 31),
                        $"Warn User{(timeout != null ? $" for {timeout} {(timeout == 1 ? isMinutes ? "minute" : "hour" : isMinutes ? "minutes" : "hours")}" : "")}",
                        $"{target.Mention}"
                    ),
                    new ModLogBase.ModLogInfo.ReasonInfo(
                        reason
                    )
                )
            );
        }
    }
}