using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace FBIBot.Modules.AutoMod
{
    public class Pedophile
    {
        public async Task AntiPedophileAsync(SocketUser u)
        {
            await u.SendMessageAsync("You want to explain yourself?");
        }
    }
}
