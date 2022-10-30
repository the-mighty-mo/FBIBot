using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod.ModLog
{
    public static class UnverifyModLog
    {
        public static Task SendToModLogAsync(SocketGuildUser invoker, SocketGuildUser target, string? reason) =>
            ModLogBase.SendToModLogAsync(
                new ModLogBase.ModLogInfo(
                    new ModLogBase.ModLogInfo.RequiredInfo(
                        invoker,
                        new Color(0, 0, 0),
                        "Unverify User",
                        $"{target.Mention}"
                    ),
                    new ModLogBase.ModLogInfo.ReasonInfo(
                        reason
                    )
                )
            );
    }
}