using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod.ModLog
{
    public static class UnmuteModLog
    {
        public static Task SendToModLogAsync(SocketGuildUser invoker, SocketGuildUser target) =>
            ModLogBase.SendToModLogAsync(
                new ModLogBase.ModLogInfo(
                    new ModLogBase.ModLogInfo.RequiredInfo(
                        invoker,
                        new Color(12, 156, 24),
                        "Unmute User",
                        $"{target.Mention}"
                    )
                )
            );
    }
}