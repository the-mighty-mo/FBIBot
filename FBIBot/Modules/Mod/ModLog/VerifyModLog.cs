using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod.ModLog
{
    public static class VerifyModLog
    {
        public static Task SendToModLogAsync(SocketGuildUser invoker, SocketGuildUser target) =>
            ModLogBase.SendToModLogAsync(
                new ModLogBase.ModLogInfo(
                    new ModLogBase.ModLogInfo.RequiredInfo(
                        invoker,
                        new Color(255, 255, 255),
                        "Verify User",
                        $"{target.Mention}"
                    )
                )
            );
    }
}