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
        public async Task ClearModLogAsync(string clear = "false")
        {
            SocketGuildUser u = Context.Guild.GetUser(Context.User.Id);
            if (!await VerifyUser.IsAdmin(u))
            {
                await Context.Channel.SendMessageAsync("You are not a local director of the FBI and cannot use this command.");
                return;
            }

            bool isClear = clear.ToLower() == "true";
            if (isClear)
            {
                SocketTextChannel channel = await SetModLog.GetModLogChannelAsync(Context.Guild);
                if (channel != null)
                {
                    await channel.DeleteMessagesAsync(await channel.GetMessagesAsync(int.MaxValue).FlattenAsync());

                    foreach (IMessage msg in await channel?.GetMessagesAsync(int.MaxValue).FlattenAsync())
                    {
                        await msg?.DeleteAsync();
                    }
                }
            }

            if (await SendToModLog.GetNextModLogID(Context.Guild) == 1 && !isClear)
            {
                await Context.Channel.SendMessageAsync("Our security team has informed us that there are no moderation logs.");
                return;
            }

            await SendToModLog.RemoveModLogsAsync(Context.Guild);
            await RemoveWarnings.RemoveAllWarningsAsync(Context.Guild);
            await Context.Channel.SendMessageAsync("We have shredded and burned all of the moderation logs. The Russians shall never get hold of them!");
        }
    }
}
