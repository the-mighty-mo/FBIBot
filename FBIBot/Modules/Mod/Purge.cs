using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class Purge : ModuleBase<SocketCommandContext>
    {
        [Command("purge")]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task PurgeAsync(string count = "100")
        {
            SocketGuildUser u = Context.Guild.GetUser(Context.User.Id);
            if (!await VerifyUser.IsMod(u))
            {
                await Context.Channel.SendMessageAsync("You are not an assistant of the FBI and cannot use this command.");
                return;
            }

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
            await channel.DeleteMessagesAsync(await Context.Channel.GetMessagesAsync(num).FlattenAsync());

            await Context.Channel.SendMessageAsync($"We have successfully shredded, burned, and disposed of {num} messages. Encrypt them better next time.");
        }

        [Command("purge")]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task PurgeAsync(SocketGuildUser user, string count = "10")
        {
            SocketGuildUser u = Context.Guild.GetUser(Context.User.Id);
            if (!await VerifyUser.IsMod(u))
            {
                await Context.Channel.SendMessageAsync("You are not an assistant of the FBI and cannot use this command.");
                return;
            }

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
            foreach (IMessage msg in msgs)
            {
                if (msg.Author == (user as IUser))
                {
                    await msg.DeleteAsync();
                    i++;
                }

                if (i >= num)
                {
                    i = num;
                    break;
                }
            }

            await Context.Channel.SendMessageAsync($"We have successfully shredded, burned, and disposed of {num} messages sent by {user.Mention}. There goes all of the socialist propaganda.");
        }
    }
}
