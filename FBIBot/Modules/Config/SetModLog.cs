using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Config
{
    public class SetModLog : ModuleBase<SocketCommandContext>
    {
        [Command("setmodlog")]
        [Alias("set-modlog", "set-mod-log")]
        [RequireAdmin]
        public async Task SetModLogAsync()
        {
            if (await modLogsDatabase.ModLogChannel.GetModLogChannelAsync(Context.Guild) == null)
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that you are already lacking a mod log channel.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription("Moderation logs will now go undisclosed. That information was confidential, anyways.");

            await Task.WhenAll
            (
                modLogsDatabase.ModLogChannel.RemoveModLogChannelAsync(Context.Guild),
                Context.Channel.SendMessageAsync(embed: embed.Build())
            );
        }

        [Command("setmodlog")]
        [Alias("set-modlog", "set-mod-log")]
        [RequireAdmin]
        public async Task SetModLogAsync(SocketTextChannel channel)
        {
            if (await modLogsDatabase.ModLogChannel.GetModLogChannelAsync(Context.Guild) == channel)
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that {channel.Mention} is already configured for mod logs.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"Once-confidential moderation logs will now be disclosed to {channel.Mention}.");

            await Task.WhenAll
            (
                modLogsDatabase.ModLogChannel.SetModLogChannelAsync(channel),
                Context.Channel.SendMessageAsync(embed: embed.Build())
            );
        }

        [Command("setmodlog")]
        [Alias("set-modlog", "set-mod-log")]
        public async Task SetModLogAsync(string channel)
        {
            SocketTextChannel c;
            if (ulong.TryParse(channel, out ulong channelID) && (c = Context.Guild.GetTextChannel(channelID)) != null)
            {
                await SetModLogAsync(c);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given text channel does not exist.");
        }
    }
}