using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod.ModLog
{
    public static class RaidModeModLog
    {
        public static Task SendToModLogAsync(SocketGuildUser invoker, bool isEnabled) =>
            ModLogBase.SendToModLogAsync(
                new ModLogBase.ModLogInfo(
                    new ModLogBase.ModLogInfo.RequiredInfo(
                        invoker,
                        isEnabled ? new Color(206, 15, 65) : new Color(12, 156, 24),
                        "Raid Mode",
                        isEnabled ? "Enabled" : "Disabled"
                    )
                )
            );
    }
}