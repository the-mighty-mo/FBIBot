using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;

namespace FBIBot.Modules.AutoMod.AutoSurveillance
{
    public static partial class LostTheGame
    {
        private static readonly Regex[] regexes = new Regex[]
        {
            LostGameRegex(),
            GameHasBeenLostRegex()
        };

        public static Task<bool> HasLostTheGameAsync(SocketCommandContext Context)
        {
            var message = Context.Message.Content;
            return Task.Run(() => regexes.Any(r => r.IsMatch(message)));
        }

        [GeneratedRegex(@"l(?:o|0)s(?:s|t|e|ing)\s+(?:in\s+)?the(?:\w*\s+)+?game", RegexOptions.IgnoreCase)]
        private static partial Regex LostGameRegex();

        [GeneratedRegex(@"the(?:\w*\s+)+?game(?:\w*\s+)+?(?:is|has been|was)(?:\w*\s+)+?l(?:o|0)s(?:t|e)", RegexOptions.IgnoreCase)]
        private static partial Regex GameHasBeenLostRegex();
    }
}