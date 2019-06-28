using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FBIBot.Modules.AutoMod
{
    public class Spam
    {
        readonly SocketCommandContext _context;

        public Spam(SocketCommandContext context) => _context = context;

        public async Task WarnAsync()
        {
            await _context.Channel.SendMessageAsync($"\\warn {_context.User.Mention} 0.5 Big spam");
            await _context.Message.DeleteAsync();
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
                if (userMsgs.Count >= 9)
                {
                    break;
                }
            }

            int i = 0;
            int j = 0;
            string message = context.Message.Content;
            foreach (string msg in userMsgs)
            {
                if (i >= 5 || (i >= 3 && j > 0))
                {
                    isSpam = true;
                    break;
                }
                if (msg != message)
                {
                    if (i < 3)
                    {
                        break;
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
