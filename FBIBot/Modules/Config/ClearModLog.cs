using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

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
            if (await modLogsDatabase.ModLogs.GetNextModLogID(Context.Guild) == 1 && !isClear)
            {
                await Context.Channel.SendMessageAsync("Our security team has informed us that there are no moderation logs.");
                return;
            }

            Task[] cmds =
            {
                modLogsDatabase.ModLogs.RemoveModLogsAsync(Context.Guild),
                modLogsDatabase.Warnings.RemoveAllWarningsAsync(Context.Guild)
            };

            if (isClear)
            {
                SocketTextChannel channel = await modLogsDatabase.ModLogChannel.GetModLogChannelAsync(Context.Guild);
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
                Task.WhenAll(cmds),
                Context.Channel.SendMessageAsync(embed: embed.Build())
            );
        }
    }
}
