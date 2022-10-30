using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod.ModLog
{
    public static class KickModLog
    {
        public static Task SendToModLogAsync(SocketGuildUser invoker, SocketGuildUser target, string? reason) =>
            ModLogBase.SendToModLogAsync(
                new ModLogBase.ModLogInfo(
                    new ModLogBase.ModLogInfo.RequiredInfo(
                        invoker,
                        new Color(255, 12, 12),
                        "Kick User",
                        $"{target.Mention}"
                    ),
                    new ModLogBase.ModLogInfo.ReasonInfo(
                        reason
                    )
                )
            );
    }
}