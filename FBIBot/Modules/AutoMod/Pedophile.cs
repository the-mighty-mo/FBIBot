using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
