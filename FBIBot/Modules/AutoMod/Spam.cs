using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FBIBot.Modules.AutoMod
{
    public class Spam
    {
        readonly SocketCommandContext Context;

        public Spam(SocketCommandContext context) => Context = context;

        public async Task WarnAsync()
        {
            await Context.Channel.SendMessageAsync($"\\warn {Context.User.Mention} 0.5 Big spam");
            await Context.Message.DeleteAsync();
        }

        public static async Task<bool> IsSpamAsync(SocketCommandContext context)
        {
            bool isSpam = false;

            List<IMessage> msgs = (await context.Channel.GetMessagesAsync().FlattenAsync()).ToList();
            List<string> userMsgs = new List<string>();
            foreach (IMessage msg in msgs)
            {
                if (msg.Author.Id == context.Message.Author.Id)
                {
                    userMsgs.Add(msg.Content);
                }
                if (userMsgs.Count >= 6)
                {
                    break;
                }
            }

            int i = 0;
            int j = 0;
            string message = context.Message.Content;
            foreach (string msg in userMsgs)
            {
                if (i >= 5 || (i >= 3 && j == 1) || (i >= 2 && j >= 3)) // Five duplicates OR two groups of three duplicates OR 3+ groups of two duplicates
                {
                    isSpam = true;
                    break;
                }
                if (msg != message)
                {
                    if (i < 2)
                    {
                        break;
                    }
                    if (i == 2)
                    {
                        j++; // Checking for three groups of two duplicates, else two groups of three duplicates
                    }
                    message = msg;
                    j++;
                    i = 0;
                }
                i++;
            }

            return await Task.Run(() => isSpam);
        }
    }
}
