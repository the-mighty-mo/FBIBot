using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;

namespace FBIBot.Modules.AutoMod.AutoSurveillance
{
    public class NoRedSam
    {
        private readonly SocketCommandContext Context;

        public NoRedSam(SocketCommandContext context) => Context = context;

        public async Task CleanseSamAsync()
        {
            if (Context.User is SocketGuildUser user && user.Id == 494512479783878666)
            {
                await Task.WhenAll(
                    Context.Message.DeleteAsync(),
                    user.SendMessageAsync("n o"),
                    user.RemoveRoleAsync(Context.Guild.Roles.FirstOrDefault(x => x.Name == "red"))
                ).ConfigureAwait(false);
            }
        }

        public static Task<bool> IsRedSamAsync(SocketCommandContext Context)
        {
            var message = Context.Message.Content;
            return Task.Run(() => message.Trim().ToLower() == "?rank red");
        }
    }
}