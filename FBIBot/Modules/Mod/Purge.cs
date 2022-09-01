using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    [Group("purge", "Shreds, burns, and disposes of a number of messages from the channel")]
    public class Purge : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("all", "Shreds, burns, and disposes of a number of messages from the channel")]
        [RequireMod]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task PurgeAsync(int count = 100)
        {
            if (count > 1000)
            {
                count = 1000;
            }
            SocketTextChannel channel = Context.Guild.GetTextChannel(Context.Channel.Id);

            var msgs = await channel.GetMessagesAsync(count).FlattenAsync();
            await channel.DeleteMessagesAsync(msgs);

            try
            {
                await Task.WhenAll(msgs.Select(msg => msg.DeleteAsync()));
            }
            catch { }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"We have successfully shredded, burned, and disposed of {count} messages. Encrypt them better next time.");

            await Context.Interaction.RespondAsync(embed: embed.Build());
        }

        [SlashCommand("user", "Shreds, burns, and disposes of a number of messages from a user in the channel")]
        [RequireMod]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task PurgeAsync(SocketGuildUser user, int count = 10)
        {
            if (count > 100)
            {
                count = 100;
            }
            SocketTextChannel channel = Context.Guild.GetTextChannel(Context.Channel.Id);
            IEnumerable<IMessage> msgs = await channel.GetMessagesAsync(1000).FlattenAsync();

            try
            {
                await Task.WhenAll(msgs.Where(x => x.Author == user).Take(count).Select(msg => msg.DeleteAsync()));
            }
            catch { }

            await Context.Interaction.RespondAsync($"We have successfully shredded, burned, and disposed of {count} messages sent by {user.Mention}. There goes all of the socialist propaganda.");
        }
    }
}