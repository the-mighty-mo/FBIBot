using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod.ModLog
{
    public static class UnmuteModLog
    {
        public static async Task SendToModLogAsync(SocketGuildUser invoker, SocketGuildUser target) =>
            await ModLogBase.SendToModLogAsync(
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