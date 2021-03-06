﻿using Discord;
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
        private readonly SocketCommandContext Context;

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
            List<IMessage> userMsgs = msgs.Where(x => x.Author.Id == Context.Message.Author.Id && x.Content.Length > 0).Take(20).ToList();

            string message = Context.Message.Content;
            Dictionary<string, int> messages = new Dictionary<string, int>
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

        public static async Task<bool> IsSingleSpamAsync(SocketCommandContext context)
        {
            static string ReplaceEmoji(string str)
            {
                Regex emojiRegex = new Regex(@"<a?:\S+:\d+>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                IEnumerable<Match> emojis = emojiRegex.Matches(str).Cast<Match>();
                foreach (string emoji in emojis.Select(e => e.Value))
                {
                    str = str.Replace(emoji, "emj");
                }
                return str;
            }

            bool isSpam = false;

            string message = context.Message.Content;
            if (ReplaceEmoji(message).Length > 60)
            {
                await Task.Yield();

                Regex regex = new Regex(@"(.+?\S+)(\s*\1){3,}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                IEnumerable<Match> matches = regex.Matches(message).Cast<Match>();

                int duplicate = matches.Select(x => ReplaceEmoji(x.Value).Length).Sum();

                int firstIndex = matches.Select(x => x.Index).OrderBy(x => x).DefaultIfEmpty(0).First();
                int lastIndex = matches.Select(x => x.Index + x.Length).OrderByDescending(x => x).DefaultIfEmpty(0).First();

                if (firstIndex != lastIndex)
                {
                    isSpam = (double)duplicate / (lastIndex - firstIndex) >= 0.80 // length of the duplicates is at least 80% of the segment of the message containing them
                        && (double)duplicate / message.Replace(" ", string.Empty).Length >= 0.60; // length of the duplicates is at least 60% of the message, excluding spaces
                }
            }

            return isSpam;
        }
    }
}