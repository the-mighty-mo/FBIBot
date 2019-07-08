using Discord.Commands;
using System.Threading.Tasks;

namespace FBIBot.Modules.AutoMod
{
    public class CAPS
    {
        readonly SocketCommandContext CONTEXT;

        public CAPS(SocketCommandContext context) => CONTEXT = context;

        public async Task WARNASYNC()
        {
            await CONTEXT.Channel.SendMessageAsync($"\\warn {CONTEXT.User.Mention} 0.5 BIG CAPS\n" +
                $"Message: {CONTEXT.Message.Content}");
            await CONTEXT.Message.DeleteAsync();
        }

        public static async Task<bool> ISCAPSASYNC(SocketCommandContext Context)
        {
            bool isCaps = false;

            string message = Context.Message.Content.Replace(" ", string.Empty);
            if (message.Length >= 40)
            {
                await Task.Yield();

                int caps = 0;
                foreach (char c in message)
                {
                    if (char.IsUpper(c))
                    {
                        caps++;
                    }
                }

                isCaps = (double)caps / message.Length >= 0.80;
            }

            return isCaps;
        }
    }
}
