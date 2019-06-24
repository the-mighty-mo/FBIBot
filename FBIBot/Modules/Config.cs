using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FBIBot.Modules
{
    public class Config : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        public async Task Help(params string[] args)
        {
            if (args.Length == 0 || args[0] != SecurityInfo.botID)
            {
                return;
            }


        }
    }
}
