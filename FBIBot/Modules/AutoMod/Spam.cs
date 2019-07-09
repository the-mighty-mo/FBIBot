using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FBIBot.Modules.AutoMod
{
    public class Spam
    {
        readonly SocketCommandContext Context;

        public Spam(SocketCommandContext context) => Context = context;

        public async Task WarnAsync()
        {
            await Task.WhenAll
            (
                Context.Message.DeleteAsync(),
                Context.Channel.SendMessageAsync($"\\warn {Context.User.Mention} 0.5 Big spam\n" +
                    $"Message: {Context.Message.Content}")
            );
        }

        public static async Task<bool> IsSpamAsync(SocketCommandContext Context)
        {
            bool isSpam = false;

            var msgs = await Context.Channel.GetMessagesAsync().FlattenAsync();
            List<string> userMsgs = new List<string>();
            foreach (IMessage msg in msgs.Where(x => x.Author.Id == Context.Message.Author.Id))
            {
                userMsgs.Add(msg.Content);
                if (userMsgs.Count >= 6)
                {
                    break;
                }
            }

            int i = 0;
            int j = 0;
            string message = Context.Message.Content;
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

            return isSpam;
        }

        public static async Task<bool> IsSingleSpamAsync(SocketCommandContext context)
        {
            bool isSpam = false;

            string message = context.Message.Content;
            if (message.Length > 40)
            {
                await Task.Yield();

                Regex regex = new Regex(@"(\W|^)(.+)\s*\2", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                MatchCollection matches = regex.Matches(message);

                int duplicate = 0;
                int firstIndex = message.Length;
                int lastIndex = 0;
                foreach (Match m in matches)
                {
                    duplicate += m.Length;
                    int last = m.Index + m.Length;

                    lastIndex = last > lastIndex ? last : lastIndex;
                    firstIndex = m.Index < firstIndex ? m.Index : firstIndex;
                }

                if (firstIndex != message.Length)
                {
                    string msg = message.Substring(firstIndex, lastIndex - firstIndex);
                    isSpam = (double)duplicate / msg.Length >= 0.80
                        && (double)msg.Length / message.Replace(" ", string.Empty).Length > 0.4;
                }
            }

            return isSpam;
        }
    }
}
