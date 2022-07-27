using Discord.Commands;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FBIBot.Modules.AutoMod.AutoSurveillance
{
    public static class LostTheGame
    {
        private static readonly Regex[] regexes = new Regex[]
        {
            new(@"l(?:o|0)s(?:s|t|e|ing)\s+(?:in\s+)?the(?:\w*\s+)+?game", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new(@"the(?:\w*\s+)+?game(?:\w*\s+)+?(?:is|has been|was)(?:\w*\s+)+?l(?:o|0)s(?:t|e)", RegexOptions.IgnoreCase | RegexOptions.Compiled)
        };

        public static Task<bool> HasLostTheGameAsync(SocketCommandContext Context)
        {
            var message = Context.Message.Content;
            return Task.Run(() => regexes.Any(r => r.IsMatch(message)));
        }
    }
}