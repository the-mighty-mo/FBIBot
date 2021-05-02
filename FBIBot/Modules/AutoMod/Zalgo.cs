using Discord.Commands;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace FBIBot.Modules.AutoMod
{
    public class Zalgo
    {
        private readonly SocketCommandContext Context;

        public Zalgo(SocketCommandContext context) => Context = context;

        public async Task WarnAsync() => await Task.WhenAll
            (
                Context.Message.DeleteAsync(),
                Context.Channel.SendMessageAsync($"\\tempwarn {Context.User.Mention} 0.5 HE COMES (Zalgo)\n")
            );

        public static async Task<bool> IsZalgoAsync(SocketCommandContext Context) =>
            await IsZalgoAsync(Context.Message.Content);

        public static async Task<bool> IsZalgoAsync(string msg)
        {
            int i = 0;

            IEnumerable<bool> zalgoList = await Task.Run(() => msg.ToCharArray().Select(c => char.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark));
            foreach (bool isZalgo in zalgoList)
            {
                if (!isZalgo)
                {
                    if (i >= 3)
                    {
                        return true;
                    }
                    i = 0;
                }
                else
                {
                    i++;
                }
            }

            return false;
        }

        public static async Task<string> RemoveZalgoAsync(string msg)
        {
            IEnumerable<string> msgList = await Task.Run(() => msg.Where(c => char.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).Select(c => c.ToString()));
            return string.Join("", msgList);
        }
    }
}