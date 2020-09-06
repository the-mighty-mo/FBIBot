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
            bool isTime = double.TryParse(timeout, out double time);
            bool isMinutes = time < 1;
            if (time < 1)
            {
                time *= 60;
            }
            await ModLogBase.SendToModLogAsync(
                new ModLogBase.ModLogInfo(
                    new ModLogBase.ModLogInfo.RequiredInfo(
                        invoker,
                        new Color(255, 213, 31),
                        $"Warn User{(isTime ? $" for {time} {(time == 1 ? isMinutes ? "minute" : "hour" : isMinutes ? "minutes" : "hours")}" : "")}",
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
