using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Config
{
    public class SetWelcome : ModuleBase<SocketCommandContext>
    {
        [Command("setwelcome")]
        [Alias("set-welcome")]
        [RequireAdmin]
        public async Task SetWelcomeAsync()
        {
            if (await modLogsDatabase.WelcomeChannel.GetWelcomeChannelAsync(Context.Guild) == null)
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that you are already lacking a welcome channel.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription("Welcome messages have been discontinued. They gave our agents away, anyways.");

            await Task.WhenAll
            (
                modLogsDatabase.WelcomeChannel.RemoveWelcomeChannelAsync(Context.Guild),
                Context.Channel.SendMessageAsync("", false, embed.Build())
            );
        }

        [Command("setwelcome")]
        [Alias("set-welcome")]
        [RequireAdmin]
        public async Task SetWelcomeAsync(SocketTextChannel channel)
        {
            if (await modLogsDatabase.WelcomeChannel.GetWelcomeChannelAsync(Context.Guild) == channel)
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that {channel.Mention} is already configured for welcome messages.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"Welcome messages will now be sent to {channel.Mention}.");

            await Task.WhenAll
            (
                modLogsDatabase.WelcomeChannel.SetWelcomeChannelAsync(channel),
                Context.Channel.SendMessageAsync("", false, embed.Build())
            );
        }

        [Command("setwelcome")]
        [Alias("set-welcome")]
        public async Task SetWelcomeAsync(string channel)
        {
            SocketTextChannel c;
            if (ulong.TryParse(channel, out ulong channelID) && (c = Context.Guild.GetTextChannel(channelID)) != null)
            {
                await SetWelcomeAsync(c);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given text channel does not exist.");
        }
    }
}
