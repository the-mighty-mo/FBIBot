using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class Slowmode : ModuleBase<SocketCommandContext>
    {
        [Command("slowmode")]
        [RequireMod]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task SlowModeAsync(string length = null)
        {
            if (length == null)
            {
                EmbedBuilder embed2 = new EmbedBuilder()
                    .WithColor(SecurityInfo.botColor)
                    .WithDescription("Slowmode has been disabled. Prepare for messages to fly by faster than an F-15 Strike Eagle.");

                await Task.WhenAll
                (
                    Context.Guild.GetTextChannel(Context.Channel.Id).ModifyAsync(x => x.SlowModeInterval = 0),
                    Context.Channel.SendMessageAsync("", false, embed2.Build())
                );
                return;
            }
            if (!int.TryParse(length, out int seconds))
            {
                await Context.Channel.SendMessageAsync($"Our intelligence team has informed us that {length} is not a valid number of seconds.");
                return;
            }

            if (seconds > 21600)
            {
                seconds = 21600;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithDescription($"Slowmode has been set to {seconds} seconds.");

            await Task.WhenAll
            (
                Context.Guild.GetTextChannel(Context.Channel.Id).ModifyAsync(x => x.SlowModeInterval = seconds),
                Context.Channel.SendMessageAsync("", false, embed.Build())
            );
        }
    }
}
