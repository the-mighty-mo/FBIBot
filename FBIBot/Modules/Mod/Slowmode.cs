using Discord;
using Discord.Interactions;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class Slowmode : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("slowmode", "Enables slowmode in the chat. *Disables slowmode if no time is given*")]
        [RequireMod]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task SlowModeAsync([Summary(description: "Time in seconds. Max: 21600")] int? seconds = null)
        {
            if (seconds == null)
            {
                EmbedBuilder emb = new EmbedBuilder()
                    .WithColor(SecurityInfo.botColor)
                    .WithDescription("Slowmode has been disabled. Prepare for messages to fly by faster than an F-15 Strike Eagle.");

                await Task.WhenAll
                (
                    Context.Guild.GetTextChannel(Context.Channel.Id).ModifyAsync(x => x.SlowModeInterval = 0),
                    Context.Interaction.RespondAsync(embed: emb.Build())
                );
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
                Context.Guild.GetTextChannel(Context.Channel.Id).ModifyAsync(x => x.SlowModeInterval = seconds.Value),
                Context.Interaction.RespondAsync(embed: embed.Build())
            );
        }
    }
}