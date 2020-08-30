using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FBIBot.Modules.Mod;
using System.Collections.Generic;
using System.Linq;
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
                    var msgs = (await channel.GetMessagesAsync(int.MaxValue).FlattenAsync()).Where(x => x.Author == Context.Guild.GetUser(Context.Client.CurrentUser.Id));
                    await channel.DeleteMessagesAsync(msgs);

                    try
                    {
                        await Task.WhenAll(msgs.Select(msg => msg.DeleteAsync()));
                    }
                    catch { }
                }
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription("We have shredded and burned all the moderation logs. The Russians shall never get hold of them!");

            await Task.WhenAll
            (
                cmds[0],
                cmds[1],
                Context.Channel.SendMessageAsync("", false, embed.Build())
            );
        }
    }
}
