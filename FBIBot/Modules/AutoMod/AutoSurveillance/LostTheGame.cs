using Discord.Commands;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;

namespace FBIBot.Modules.AutoMod.AutoSurveillance
{
    public static class LostTheGame
    {
        public static async Task<bool> HasLostTheGameAsync(SocketCommandContext Context)
        {
            await Task.Yield();

            string message = Context.Message.Content;
            var regexes = new Regex[]
            {
                new(@"l(?:o|0)s(?:s|t|e|ing)\s+(?:in\s+)?the(?:\w*\s+)+?game", RegexOptions.IgnoreCase | RegexOptions.Compiled),
                new(@"the(?:\w*\s+)+?game(?:\w*\s+)+?(?:is|has been|was)(?:\w*\s+)+?l(?:o|0)s(?:t|e)", RegexOptions.IgnoreCase | RegexOptions.Compiled)
            };
            return regexes.Any(r => r.IsMatch(message));
        }
    }
}