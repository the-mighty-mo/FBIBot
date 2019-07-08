using Discord.WebSocket;
using FBIBot.Modules.Config;
using System.Linq;
using System.Threading.Tasks;

namespace FBIBot
{
    public static class VerifyUser
    {
        public static async Task<bool> IsAdmin(SocketGuildUser user)
        {
            bool isValidAdmin = user == user.Guild.Owner || user == user.Guild.CurrentUser;
            foreach (SocketRole r in await AddAdmin.GetAdminRolesAsync(user.Guild))
            {
                if (isValidAdmin)
                {
                    break;
                }
                isValidAdmin = user.Roles.Contains(r);
            }
            return isValidAdmin;
        }

        public static async Task<bool> IsMod(SocketGuildUser user)
        {
            bool isValidMod = await IsAdmin(user);
            foreach (SocketRole r in await AddMod.GetModRolesAsync(user.Guild))
            {
                if (isValidMod)
                {
                    break;
                }
                isValidMod = user.Roles.Contains(r);
            }
            return isValidMod;
        }

        public static async Task<bool> InvokerIsHigher(SocketGuildUser invoker, SocketGuildUser target)
        {
            bool isHigher = target.Hierarchy < invoker.Hierarchy
                && (invoker == invoker.Guild.Owner || invoker == invoker.Guild.CurrentUser || (!await IsMod(target) && !await IsAdmin(target)));
            return isHigher;
        }

        public static async Task<bool> BotIsHigher(SocketGuildUser bot, SocketGuildUser target)
        {
            bool isHigher = await Task.Run(() => target.Hierarchy < bot.Hierarchy);
            return isHigher;
        }
    }
}
