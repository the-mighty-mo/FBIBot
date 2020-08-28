using Discord;
using Discord.Commands;
using System;
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
            List<IMessage> userMsgs = msgs.Where(x => x.Author.Id == Context.Message.Author.Id).Take(10).ToList();

            int i = 0;
            int j = 0;
            string message = Context.Message.Content;
            foreach (IMessage msg in userMsgs)
            {
                if (userMsgs[0].Timestamp - msg.Timestamp > TimeSpan.FromMinutes(1))
                {
                    break;
                }
                else
                {
                    if (i >= 4 || (i >= 3 && j == 1) || (i >= 2 && j >= 3)) // Five duplicates OR two groups of three duplicates OR 3+ groups of two duplicates
                    {
                        isSpam = true;
                        break;
                    }
                    else if (msg.Content != message)
                    {
                        if (i < 2)
                        {
                            break;
                        }
                        else if (i == 2)
                        {
                            j++; // Checking for three groups of two duplicates, else two groups of three duplicates
                        }
                        message = msg.Content;
                        j++;
                        i = 0;
                    }
                    i++;
                }
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
                IEnumerable<Match> m = matches.Cast<Match>();

                int duplicate = m.Select(x => x.Length).Sum();
                int firstIndex = m.Select(x => x.Index).OrderBy(x => x).DefaultIfEmpty(0).First();
                int lastIndex = m.Select(x => x.Index + x.Length).OrderByDescending(x => x).DefaultIfEmpty(0).First();

                if (firstIndex != lastIndex)
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
