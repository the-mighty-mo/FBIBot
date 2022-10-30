using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod.ModLog
{
    public static class FreeModLog
    {
        public static Task SendToModLogAsync(SocketGuildUser invoker, SocketGuildUser target) =>
            ModLogBase.SendToModLogAsync(
                new ModLogBase.ModLogInfo(
                    new ModLogBase.ModLogInfo.RequiredInfo(
                        invoker,
                        new Color(12, 156, 24),
                        "Free User",
                        $"{target.Mention}"
                    )
                )
            );
    }
}