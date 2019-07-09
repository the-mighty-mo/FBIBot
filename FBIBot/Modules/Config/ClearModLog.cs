using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FBIBot.Modules.Mod;
using System.Threading.Tasks;

namespace FBIBot.Modules.Config
{
    public class ClearModLog : ModuleBase<SocketCommandContext>
    {
        [Command("clearmodlog")]
        [Alias("clearmodlogs")]
        [RequireAdmin]
        public async Task ClearModLogAsync(string clear = "false")
        {
            bool isClear = clear.ToLower() == "true";
            if (isClear)
            {
                SocketTextChannel channel = await SetModLog.GetModLogChannelAsync(Context.Guild);
                if (channel != null)
                {
                    var msgs = await channel.GetMessagesAsync(int.MaxValue).FlattenAsync();
                    await channel.DeleteMessagesAsync(msgs);

                    foreach (var msg in msgs)
                    {
                        try
                        {
                            await msg.DeleteAsync();
                        }
                        catch { }
                    }
                }
            }

            if (await SendToModLog.GetNextModLogID(Context.Guild) == 1 && !isClear)
            {
                await Context.Channel.SendMessageAsync("Our security team has informed us that there are no moderation logs.");
                return;
            }

            await Task.WhenAll
            (
                SendToModLog.RemoveModLogsAsync(Context.Guild),
                RemoveWarnings.RemoveAllWarningsAsync(Context.Guild),
                Context.Channel.SendMessageAsync("We have shredded and burned all of the moderation logs. The Russians shall never get hold of them!")
            );
        }
    }
}
