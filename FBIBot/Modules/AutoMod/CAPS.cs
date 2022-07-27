using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;

namespace FBIBot.Modules.AutoMod
{
    public class CAPS
    {
        private readonly SocketCommandContext CONTEXT;

        public CAPS(SocketCommandContext context) => CONTEXT = context;

        public Task WARNASYNC() =>
            Task.WhenAll
            (
                CONTEXT.Message.DeleteAsync(),
                CONTEXT.Channel.SendMessageAsync($"\\tempwarn {CONTEXT.User.Mention} 0.5 BIG CAPS\n" +
                    $"Message: {CONTEXT.Message.Content}")
            );

        public static Task<bool> ISCAPSASYNC(SocketCommandContext Context)
        {
            var message = Context.Message.Content.Replace(" ", string.Empty);
            return Task.Run(() =>
            {
                bool isCaps = false;

                if (message.Length >= 40)
                {
                    int caps = message.Where(x => char.IsUpper(x)).Count();
                    isCaps = (double)caps / message.Length >= 0.80;
                }

                return isCaps;
            });
        }
    }
}