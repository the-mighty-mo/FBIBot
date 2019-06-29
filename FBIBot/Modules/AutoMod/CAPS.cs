using Discord.Commands;
using System.Threading.Tasks;

namespace FBIBot.Modules.AutoMod
{
    public class CAPS
    {
        SocketCommandContext CONTEXT;

        public CAPS(SocketCommandContext context) => CONTEXT = context;

        public async Task WARNASYNC()
        {
            await CONTEXT.Channel.SendMessageAsync($"\\warn {CONTEXT.User.Mention} 0.5 BIG CAPS");
            await CONTEXT.Message.DeleteAsync();
        }

        public static async Task<bool> ISCAPSASYNC(SocketCommandContext context)
        {
            bool isCaps = false;

            string message = context.Message.Content.Replace(" ", string.Empty);
            if (message.Length >= 40)
            {
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

            return await Task.Run(() => isCaps);
        }
    }
}
