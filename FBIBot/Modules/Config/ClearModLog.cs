using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FBIBot.Modules.Mod;
using System.Collections.Generic;
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
            if (await SendToModLog.GetNextModLogID(Context.Guild) == 1 && !isClear)
            {
                await Context.Channel.SendMessageAsync("Our security team has informed us that there are no moderation logs.");
                return;
            }

            Task[] cmds =
            {
                SendToModLog.RemoveModLogsAsync(Context.Guild),
                RemoveWarnings.RemoveAllWarningsAsync(Context.Guild)
            };

            if (isClear)
            {
                SocketTextChannel channel = await SetModLog.GetModLogChannelAsync(Context.Guild);
                if (channel != null)
                {
                    var msgs = await channel.GetMessagesAsync(int.MaxValue).FlattenAsync();
                    await channel.DeleteMessagesAsync(msgs);

                    try
                    {
                        List<Task> dels = new List<Task>();
                        foreach (var msg in msgs)
                        {
                            dels.Add(msg.DeleteAsync());
                        }

                        await Task.WhenAll(dels);
                    }
                    catch { }
                }
            }

            await Task.WhenAll
            (
                cmds[0],
                cmds[1],
                Context.Channel.SendMessageAsync("We have shredded and burned all of the moderation logs. The Russians shall never get hold of them!")
            );
        }
    }
}
