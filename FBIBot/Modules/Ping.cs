using Discord;
using Discord.Commands;
using Discord.Rest;
using System.Threading.Tasks;

namespace FBIBot.Modules
{
    public class Ping : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        public async Task PingAsync()
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("The FBI")
                .WithDescription(":ping_pong:**Pong!**")
                .WithCurrentTimestamp();
            RestUserMessage msg = await Context.Channel.SendMessageAsync(embed: embed.Build());

            embed.WithDescription(":ping_pong:**Pong!**\n" +
                $"**Server:** {(int)(msg.Timestamp - Context.Message.Timestamp).TotalMilliseconds}ms\n" +
                $"**API:** {Context.Client.Latency}ms");

            await msg.ModifyAsync(x => x.Embed = embed.Build());
        }
    }
}
