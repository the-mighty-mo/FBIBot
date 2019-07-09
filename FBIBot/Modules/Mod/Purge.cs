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

            if (num > 1000)
            {
                num = 1000;
            }
            SocketTextChannel channel = Context.Guild.GetTextChannel(Context.Channel.Id);

            var msgs = await channel.GetMessagesAsync(num).FlattenAsync();
            await channel.DeleteMessagesAsync(msgs);

            try
            {
                List<Task> cmds = new List<Task>();
                foreach (var msg in msgs)
                {
                    cmds.Add(msg.DeleteAsync());
                }

                await Task.WhenAll(cmds);
            }
            catch { }

            await Context.Channel.SendMessageAsync($"We have successfully shredded, burned, and disposed of {num} messages. Encrypt them better next time.");
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

            if (num > 100)
            {
                num = 100;
            }
            SocketTextChannel channel = Context.Guild.GetTextChannel(Context.Channel.Id);
            IEnumerable<IMessage> msgs = await channel.GetMessagesAsync(1000).FlattenAsync();
            int i = 0;

            try
            {
                List<Task> cmds = new List<Task>();
                foreach (IMessage msg in msgs.Where(x => x.Author == (user as IUser)))
                {
                    cmds.Add(msg.DeleteAsync());
                    i++;

                    if (i >= num)
                    {
                        i = num;
                        break;
                    }
                }

                await Task.WhenAll(cmds);
            }
            catch { }

            await Context.Channel.SendMessageAsync($"We have successfully shredded, burned, and disposed of {num} messages sent by {user.Mention}. There goes all of the socialist propaganda.");
        }
    }
}
