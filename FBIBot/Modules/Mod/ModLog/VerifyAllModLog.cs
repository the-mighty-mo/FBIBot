using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod.ModLog
{
    public static class VerifyAllModLog
    {
        public static Task SendToModLogAsync(SocketGuildUser invoker) =>
            ModLogBase.SendToModLogAsync(
                new ModLogBase.ModLogInfo(
                    new ModLogBase.ModLogInfo.RequiredInfo(
                        invoker,
                        new Color(255, 255, 255),
                        "Verify All Users",
                        "Running"
                    )
                )
            );

        public static Task<bool> SetStateAsync(SocketGuild g, ulong id, string state) =>
            ModLogManager.SetStateAsync(
                new ModLogManager.StateInfo(g, id, "Verify All Users", state)
            );
    }
}