using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod.ModLog
{
    public static class RemoveWarningModLog
    {
        public static Task SendToModLogAsync(SocketGuildUser invoker, SocketGuildUser target, ulong warning) =>
            ModLogBase.SendToModLogAsync(
                new ModLogBase.ModLogInfo(
                    new ModLogBase.ModLogInfo.RequiredInfo(
                        invoker,
                        new Color(12, 156, 24),
                        $"Remove Warning {warning} from User",
                        $"{target.Mention}"
                    )
                )
            );
    }
}