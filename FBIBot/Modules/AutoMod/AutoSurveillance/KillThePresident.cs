using Discord.Commands;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FBIBot.Modules.AutoMod.AutoSurveillance
{
    public static class KillThePresident
    {
        private static readonly Regex regex = new(@"I(?:(?:'m|\s+am)\s+going\s+to|\s+want\s+to)(?:\w*\s+)+?(?:kill|kil(?!\w+)|assassinate|murder)\s+the(?:\w*\s+)+?president", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static Task<bool> IsGoingToKillThePresidentAsync(SocketCommandContext Context)
        {
            var message = Context.Message.Content;
            return Task.Run(() => regex.IsMatch(message));
        }
    }
}