using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;

namespace FBIBot.Modules.AutoMod
{
    public class CAPS
    {
        private readonly SocketCommandContext CONTEXT;

        public CAPS(SocketCommandContext context) => CONTEXT = context;

        public async Task WARNASYNC() =>
            await Task.WhenAll
            (
                CONTEXT.Message.DeleteAsync(),
                CONTEXT.Channel.SendMessageAsync($"\\tempwarn {CONTEXT.User.Mention} 0.5 BIG CAPS\n" +
                    $"Message: {CONTEXT.Message.Content}")
            );

        public static async Task<bool> ISCAPSASYNC(SocketCommandContext Context)
        {
            bool isCaps = false;

            string message = Context.Message.Content.Replace(" ", string.Empty);
            if (message.Length >= 40)
            {
                await Task.Yield();

                int caps = message.Where(x => char.IsUpper(x)).Count();
                isCaps = (double)caps / message.Length >= 0.80;
            }

            return isCaps;
        }
    }
}