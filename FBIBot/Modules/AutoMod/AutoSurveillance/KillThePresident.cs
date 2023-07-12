using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;

namespace FBIBot.Modules.AutoMod.AutoSurveillance
{
    public static partial class KillThePresident
    {
        private static readonly Regex regex = PresidentRegex();

        public static Task<bool> IsGoingToKillThePresidentAsync(SocketCommandContext Context)
        {
            var message = Context.Message.Content;
            return Task.Run(() => regex.IsMatch(message));
        }

        [GeneratedRegex(@"I(?:(?:'m|\s+am)\s+going\s+to|\s+want\s+to)(?:\w*\s+)+?(?:kill|kil(?!\w+)|assassinate|murder)\s+the(?:\w*\s+)+?president", RegexOptions.IgnoreCase)]
        private static partial Regex PresidentRegex();
    }
}