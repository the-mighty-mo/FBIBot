using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod.ModLog
{
    public static class BanModLog
    {
        public static async Task SendToModLogAsync(SocketGuildUser invoker, SocketGuildUser target, string timeout, string reason)
            => await SendToModLogAsync(invoker, target.Id, timeout, reason);

        public static async Task SendToModLogAsync(SocketGuildUser invoker, ulong? target, string timeout, string reason)
        {
            bool isTime = double.TryParse(timeout, out double time);
            await ModLogBase.SendToModLogAsync(
                new ModLogBase.ModLogInfo(
                    new ModLogBase.ModLogInfo.RequiredInfo(
                        invoker,
                        new Color(130, 0, 0),
                        $"Ban User{(isTime ? $" for {time} {(time == 1 ? "day" : "days")}" : "")}",
                        $"<@{target}>"
                    ),
                    new ModLogBase.ModLogInfo.ReasonInfo(
                        reason
                    )
                )
            );
        }
    }
}
