using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Modules.AutoMod
{
    public class Pedophile
    {
        readonly SocketCommandContext _context;

        public Pedophile(SocketCommandContext context) => _context = context;

        public async Task AntiPedophileAsync()
        {
            await _context.Message.DeleteAsync();
            await _context.User.SendMessageAsync("You want to explain yourself?");
            await _context.Channel.SendMessageAsync($"\\arrest {_context.User.Mention} 5");
        }

        public static async Task<bool> IsPedophileAsync(SocketUserMessage msg)
        {
            bool isPedophile = false;

            List<string> bad = new List<string>()
            {
                "i like",
                "i love"
            };
            List<string> stillBad = new List<string>()
            {
                "kids",
                "children",
                "little kids",
                "little children"
            };
            foreach (string b in bad)
            {
                foreach (string s in stillBad)
                {
                    if (msg.Content.ToLower().Contains($"{b} {s}"))
                    {
                        isPedophile = true;
                        break;
                    }
                }
                if (isPedophile)
                {
                    break;
                }
            }

            return await Task.Run(() => isPedophile);
        }
    }
}
