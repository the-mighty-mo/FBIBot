using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class Purge : ModuleBase<SocketCommandContext>
    {
        [Command("purge")]
        [RequireMod]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task PurgeAsync(string count = "100")
        {
            if (!int.TryParse(count, out int num))
            {
                await Context.Channel.SendMessageAsync("Our intelligence team has informed us that {count} is not a valid number of messages.");
                return;
            }

            await Context.Message.DeleteAsync();

            if (num > 1000)
            {
                num = 1000;
            }
            SocketTextChannel channel = Context.Guild.GetTextChannel(Context.Channel.Id);

            var msgs = await channel.GetMessagesAsync(num).FlattenAsync();
            await channel.DeleteMessagesAsync(msgs);

            try
            {
                await Task.WhenAll(msgs.Select(msg => msg.DeleteAsync()));
            }
            catch { }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"We have successfully shredded, burned, and disposed of {num} messages. Encrypt them better next time.");

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("purge")]
        [RequireMod]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task PurgeAsync(SocketGuildUser user, string count = "10")
        {
            if (!int.TryParse(count, out int num))
            {
                await Context.Channel.SendMessageAsync("Our intelligence team has informed us that {count} is not a valid number of messages.");
                return;
            }

            if (user == (Context.User as SocketGuildUser))
            {
                await Context.Message.DeleteAsync();
            }

            if (num > 100)
            {
                num = 100;
            }
            SocketTextChannel channel = Context.Guild.GetTextChannel(Context.Channel.Id);
            IEnumerable<IMessage> msgs = await channel.GetMessagesAsync(1000).FlattenAsync();

            try
            {
                await Task.WhenAll(msgs.Where(x => x.Author == (user as IUser)).Take(num).Select(msg => msg.DeleteAsync()));
            }
            catch { }

            await Context.Channel.SendMessageAsync($"We have successfully shredded, burned, and disposed of {num} messages sent by {user.Mention}. There goes all of the socialist propaganda.");
        }
    }
}
