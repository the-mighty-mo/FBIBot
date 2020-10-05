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
                Context.Channel.SendMessageAsync($"\\tempwarn {Context.User.Mention} 0.5 Big spam\n" +
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
            string secondary = null;
            foreach (IMessage msg in userMsgs)
            {
                if (Context.Message.Timestamp - msg.Timestamp > TimeSpan.FromMinutes(2))
                {
                    break;
                }
                else
                {
                    if (msg.Content == message)
                    {
                        i++;
                    }
                    else
                    {
                        secondary ??= msg.Content;
                        if (msg.Content == secondary)
                        {
                            j++;
                        }
                    }

                    if (i + j >= 5 && i != 1 && j != 1) // Spamming one message or alternating between two messages for five or more messages
                    {
                        isSpam = true;
                        break;
                    }
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

                Regex regex = new Regex(@"(\W|^)(.+\S+)(\s*\2){2,}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                MatchCollection matches = regex.Matches(message);
                IEnumerable<Match> m = matches.Cast<Match>();

                int duplicate = m.Select(x => x.Length).Sum();
                int firstIndex = m.Select(x => x.Index).OrderBy(x => x).DefaultIfEmpty(0).First();
                int lastIndex = m.Select(x => x.Index + x.Length).OrderByDescending(x => x).DefaultIfEmpty(0).First();

                if (firstIndex != lastIndex)
                {
                    isSpam = (double)duplicate / (lastIndex - firstIndex) >= 0.80 // length of the duplicates is at least 80% of the segment of the message containing them
                        && (double)duplicate / message.Replace(" ", string.Empty).Length >= 0.67; // length of the duplicates is at least 67% of the message, excluding spaces
                }
            }

            return isSpam;
        }
    }
}
