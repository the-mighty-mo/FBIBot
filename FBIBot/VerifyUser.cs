using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot
{
    public static class VerifyUser
    {
        public static async Task<bool> IsAdmin(SocketGuildUser user)
        {
            bool isValidAdmin = user == user.Guild.Owner || user == user.Guild.CurrentUser ||
                (await modRolesDatabase.Admins.GetAdminRolesAsync(user.Guild).ConfigureAwait(false)).Select(r => user.Roles.Contains(r)).Contains(true);
            return isValidAdmin;
        }

        public static async Task<bool> IsMod(SocketGuildUser user)
        {
            bool isValidMod = await IsAdmin(user).ConfigureAwait(false) ||
                (await modRolesDatabase.Mods.GetModRolesAsync(user.Guild).ConfigureAwait(false)).Select(r => user.Roles.Contains(r)).Contains(true);
            return isValidMod;
        }

        public static async Task<bool> InvokerIsHigher(SocketGuildUser invoker, SocketGuildUser target)
        {
            bool isHigher = invoker == invoker.Guild.Owner || invoker == invoker.Guild.CurrentUser || invoker.Hierarchy > target.Hierarchy
                || (!await IsAdmin(target).ConfigureAwait(false) && await IsAdmin(invoker).ConfigureAwait(false))
                || (!await IsMod(target).ConfigureAwait(false) && await IsMod(invoker).ConfigureAwait(false));
            return isHigher;
        }

        public static Task<bool> BotIsHigher(SocketGuildUser bot, SocketGuildUser target) =>
            Task.FromResult(bot.Hierarchy > target.Hierarchy);
    }
}