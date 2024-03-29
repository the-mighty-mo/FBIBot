﻿using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    [Group("purge", "Shreds, burns, and disposes of a number of messages from the channel")]
    [RequireMod]
    public class Purge : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("all", "Shreds, burns, and disposes of a number of messages from the channel")]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task PurgeAsync([Summary(description: "Default: 100")] int count = 100)
        {
            if (count > 1000)
            {
                count = 1000;
            }
            SocketTextChannel channel = Context.Guild.GetTextChannel(Context.Channel.Id);

            var msgs = await channel.GetMessagesAsync(count).FlattenAsync().ConfigureAwait(false);
            await channel.DeleteMessagesAsync(msgs).ConfigureAwait(false);

            try
            {
                await Task.WhenAll(msgs.Select(msg => msg.DeleteAsync())).ConfigureAwait(false);
            }
            catch { }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"We have successfully shredded, burned, and disposed of {count} messages. Encrypt them better next time.");

            await Context.Interaction.RespondAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        [SlashCommand("user", "Shreds, burns, and disposes of a number of messages from a user in the channel")]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task PurgeAsync(SocketUser user, [Summary(description: "Default: 10")] int count = 10)
        {
            if (count > 100)
            {
                count = 100;
            }
            SocketTextChannel channel = Context.Guild.GetTextChannel(Context.Channel.Id);
            IEnumerable<IMessage> msgs = await channel.GetMessagesAsync(1000).FlattenAsync().ConfigureAwait(false);

            try
            {
                await Task.WhenAll(msgs.Where(x => x.Author == user).Take(count).Select(msg => msg.DeleteAsync())).ConfigureAwait(false);
            }
            catch { }

            await Context.Interaction.RespondAsync($"We have successfully shredded, burned, and disposed of {count} messages sent by {user.Mention}. There goes all of the socialist propaganda.").ConfigureAwait(false);
        }
    }
}