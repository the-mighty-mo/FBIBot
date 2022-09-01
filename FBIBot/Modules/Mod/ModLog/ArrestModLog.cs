using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod.ModLog
{
    public static class ArrestModLog
    {
        public static async Task SendToModLogAsync(SocketGuildUser invoker, SocketGuildUser target, double? timeout, string reason)
        {
            await ModLogBase.SendToModLogAsync(
                new ModLogBase.ModLogInfo(
                    new ModLogBase.ModLogInfo.RequiredInfo(
                        invoker,
                        new Color(255, 61, 24),
                        $"Arrest User{(timeout is not null ? $" for {timeout} {(timeout == 1 ? "minute" : "minutes")}" : "")}",
                        $"{target.Mention}"
                    ),
                    new ModLogBase.ModLogInfo.ReasonInfo(
                        reason ?? "*No reason necessary*"
                    )
                )
            );
        }
    }
}