using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FBIBot.ParamEnums;
using System.Linq;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Config
{
    public class ClearModLog : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("clear-mod-log", "Clears the Mod Log numbers. *Clears all warnings*")]
        [RequireAdmin]
        public async Task ClearModLogAsync([Summary(description: "Whether to clear all messages in the mod log. Default: False")] BoolChoice clear = BoolChoice.False)
        {
            bool isClear = clear == BoolChoice.True;
            if (await modLogsDatabase.ModLogs.GetNextModLogID(Context.Guild) == 1 && !isClear)
            {
                await Context.Interaction.RespondAsync("Our security team has informed us that there are no moderation logs.");
                return;
            }

            Task[] cmds =
            {
                modLogsDatabase.ModLogs.RemoveModLogsAsync(Context.Guild),
                modLogsDatabase.Warnings.RemoveAllWarningsAsync(Context.Guild)
            };

            if (isClear)
            {
                SocketTextChannel? channel = await modLogsDatabase.ModLogChannel.GetModLogChannelAsync(Context.Guild);
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
                Context.Interaction.RespondAsync(embed: embed.Build())
            );
        }
    }
}