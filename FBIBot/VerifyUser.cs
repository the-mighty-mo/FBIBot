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
            bool isHigher = ((invoker == invoker.Guild.Owner || invoker == invoker.Guild.CurrentUser) && invoker != target)
                || (!await IsAdmin(target) && await IsAdmin(invoker)) || (!await IsMod(target) && await IsMod(invoker)) || invoker.Hierarchy > target.Hierarchy;
            return isHigher;
        }

        public static async Task<bool> BotIsHigher(SocketGuildUser bot, SocketGuildUser target)
        {
            bool isHigher = await Task.Run(() => bot.Hierarchy > target.Hierarchy);
            return isHigher;
        }
    }
}
