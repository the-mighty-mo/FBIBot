using Discord;
using Discord.Commands;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FBIBot.Modules.AutoMod
{
    public class Invite
    {
        readonly SocketCommandContext Context;
        public Invite(SocketCommandContext context) => Context = context;

        public async Task RemoveAsync()
        {
            await Context.Message.DeleteAsync();
            await Context.User.SendMessageAsync($"You cannot send invite links in the server {Context.Guild.Name}.\n" +
                $"Message removed: {Context.Message.Content}");
        }

        public static async Task<bool> HasInviteAsync(SocketCommandContext Context)
        {
            string message = Context.Message.Content;

            Regex regex = new Regex(@"\b(discord\.gg\/|discordapp\.com\/invite\/)\S+\b", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
            Match match = regex.Match(message);

            bool hasInvite = match.Success;
            return await Task.Run(() => hasInvite);
        }
    }
}
