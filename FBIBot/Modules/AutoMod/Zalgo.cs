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

        public Task WarnAsync() =>
            Task.WhenAll
            (
                Context.Message.DeleteAsync(),
                Context.Channel.SendMessageAsync($"\\tempwarn {Context.User.Mention} 0.5 HE COMES (Zalgo)\n")
            );

        public static Task<bool> IsZalgoAsync(SocketCommandContext Context) =>
            IsZalgoAsync(Context.Message.Content);

        public static Task<bool> IsZalgoAsync(string msg) =>
            Task.Run(() =>
            {
                int i = 0;

                var zalgoList = msg.ToCharArray().Select(c => char.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark);
                foreach (var isZalgo in zalgoList)
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
            });

        public static Task<string> RemoveZalgoAsync(string msg) =>
            Task.Run(() =>
            {
                var msgList = msg.Where(c => char.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).Select(c => c.ToString());
                return string.Join("", msgList);
            });
    }
}