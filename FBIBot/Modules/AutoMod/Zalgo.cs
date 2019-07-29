using Discord.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Modules.AutoMod
{
    public class Zalgo
    {
        readonly SocketCommandContext Context;

        public Zalgo(SocketCommandContext context) => Context = context;

        public async Task WarnAsync()
        {
            await Task.WhenAll
            (
                Context.Message.DeleteAsync(),
                Context.Channel.SendMessageAsync($"\\warn {Context.User.Mention} 0.5 HE COMES (Zalgo)\n")
            );
        }

        public static async Task<bool> IsZalgoAsync(SocketCommandContext Context)
        {
            IEnumerable<bool> GetZalgoList()
            {
                foreach (char c in Context.Message.Content)
                {
                    yield return char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.NonSpacingMark;
                }
            }

            int i = 0;

            IEnumerable<bool> zalgoList = await Task.Run(() => GetZalgoList());
            foreach (bool isZalgo in zalgoList)
            {
                if (!isZalgo)
                {
                    if (i == 3)
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
    }
}
