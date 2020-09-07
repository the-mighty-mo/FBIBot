using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod.ModLog
{
    public static class VerifyAllModLog
    {
        public static async Task SendToModLogAsync(SocketGuildUser invoker)
        {
            await ModLogBase.SendToModLogAsync(
                new ModLogBase.ModLogInfo(
                    new ModLogBase.ModLogInfo.RequiredInfo(
                        invoker,
                        new Color(255, 255, 255),
                        "Verify All Users",
                        "Running"
                    )
                )
            );
        }

        public static async Task<bool> SetStateAsync(SocketGuild g, ulong id, string state)
            => await ModLogManager.SetStateAsync(
                new ModLogManager.StateInfo(g, id, "Verify All Users", state)
            );
    }
}
