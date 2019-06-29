using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FBIBot.Modules.AutoMod
{
    public class Invite
    {
        readonly SocketCommandContext _context;
        public Invite(SocketCommandContext context) => _context = context;

        public async Task RemoveAsync()
        {
            await _context.Message.DeleteAsync();
            await _context.User.SendMessageAsync($"You cannot send invite links in the server {_context.Guild.Name}.");
        }

        public static async Task<bool> HasInviteAsync(SocketCommandContext context)
        {
            string message = context.Message.Content;

            Regex regex = new Regex(@"\b(discord\.gg\/|discordapp\.com\/invite\/)\S+\b", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
            Match match = regex.Match(message);

            bool hasInvite = match.Success;
            return await Task.Run(() => hasInvite);
        }
    }
}
