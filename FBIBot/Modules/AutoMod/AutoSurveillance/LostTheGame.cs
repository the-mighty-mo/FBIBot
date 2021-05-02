using Discord.Commands;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FBIBot.Modules.AutoMod.AutoSurveillance
{
    public static class LostTheGame
    {
        public static async Task<bool> HasLostTheGameAsync(SocketCommandContext Context)
        {
            await Task.Yield();

            string message = Context.Message.Content;
            Regex regex = new(@"l(o|0)s(t|e)\s+the(\w*\s+)+?game", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            return regex.IsMatch(message);
        }
    }
}