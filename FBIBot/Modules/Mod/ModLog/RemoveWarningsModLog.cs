using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod.ModLog
{
    public static class RemoveWarningsModLog
    {
        public static Task SendToModLogAsync(SocketGuildUser invoker, SocketGuildUser target, int? count) =>
            ModLogBase.SendToModLogAsync(
                new ModLogBase.ModLogInfo(
                    new ModLogBase.ModLogInfo.RequiredInfo(
                        invoker,
                        new Color(12, 156, 24),
                        $"Remove {count?.ToString() ?? "All"} Warnings from User",
                        $"{target.Mention}"
                    )
                )
            );
    }
}