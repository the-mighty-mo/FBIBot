using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod.ModLog
{
    public static class ArrestModLog
    {
        public static async Task SendToModLogAsync(SocketGuildUser invoker, SocketGuildUser target, string timeout)
        {
            bool isTime = double.TryParse(timeout, out double time);
            await ModLogBase.SendToModLogAsync(
                new ModLogBase.ModLogInfo(
                    new ModLogBase.ModLogInfo.RequiredInfo(
                        invoker,
                        new Color(255, 61, 24),
                        $"Arrest User{(isTime ? $" for {time} {(time == 1 ? "minute" : "minutes")}" : "")}",
                        $"{target.Mention}"
                    ),
                    new ModLogBase.ModLogInfo.ReasonInfo(
                        "*No reason necessary*"
                    )
                )
            );
        }
    }
}
