using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class Mute : ModuleBase<SocketCommandContext>
    {
        [Command("mute")]
        [RequireBotPermission(Discord.GuildPermission.ManageRoles)]
        public async Task MuteAsync()
        {
            
        }
    }
}
