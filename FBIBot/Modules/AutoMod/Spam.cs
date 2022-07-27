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
        private static readonly Regex emojiRegex = new(@"<a?:\S+?:\d+>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex regex = new(@"(.+?\S+)(\s*\1){3,}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly SocketCommandContext Context;

        public Spam(SocketCommandContext context) => Context = context;

        public Task WarnAsync() =>
            Task.WhenAll
            (
                Context.Message.DeleteAsync(),
                Context.Channel.SendMessageAsync($"\\tempwarn {Context.User.Mention} 0.5 Big spam\n" +
                    $"Message: {Context.Message.Content}")
            );

        public static async Task<bool> IsSpamAsync(SocketCommandContext Context)
        {
            bool isSpam = false;

            var msgs = await Context.Channel.GetMessagesAsync().FlattenAsync();
            var userMsgs = msgs.Where(x => x.Author.Id == Context.Message.Author.Id && x.Content.Length > 0).Take(20);

            var message = Context.Message.Content;
            Dictionary<string, int> messages = new()
            {
                [message] = 0
            };
            foreach (IMessage msg in userMsgs)
            {
                if (Context.Message.Timestamp - msg.Timestamp > TimeSpan.FromMinutes(2))
                {
                    break;
                }

                if (!messages.TryAdd(msg.Content, 1))
                {
                    messages[msg.Content]++;
                }

                int totalDuplicates = messages.Where(x => x.Value > 2).Sum(x => x.Value); // total number of duplicate messages
                if (messages[message] >= 4 || (messages[message] > 1 && totalDuplicates >= 5))
                {
                    isSpam = true;
                    break;
                }
            }

            return isSpam;
        }

        public static Task<bool> IsSingleSpamAsync(SocketCommandContext context)
        {
            static string ReplaceEmoji(string str)
            {
                var emojis = emojiRegex.Matches(str).Cast<Match>().Select(e => e.Value);
                foreach (string emoji in emojis)
                {
                    str = str.Replace(emoji, "emj");
                }
                return str;
            }

            var message = context.Message.Content;
            return Task.Run(() =>
            {
                bool isSpam = false;

                if ((message = ReplaceEmoji(message)).Length > 60)
                {
                    var matches = regex.Matches(message).Cast<Match>();

                    int duplicate = matches.Select(x => x.Length).Sum();

                    int firstIndex = matches.Select(x => x.Index).OrderBy(x => x).DefaultIfEmpty(0).First();
                    int lastIndex = matches.Select(x => x.Index + x.Length).OrderByDescending(x => x).DefaultIfEmpty(0).First();

                    if (firstIndex != lastIndex)
                    {
                        isSpam = (double)duplicate / (lastIndex - firstIndex) >= 0.80 // length of the duplicates is at least 80% of the segment of the message containing them
                            && (double)duplicate / message.Replace(" ", string.Empty).Length >= 0.60; // length of the duplicates is at least 60% of the message, excluding spaces
                    }
                }

                return isSpam;
            });
        }
    }
}