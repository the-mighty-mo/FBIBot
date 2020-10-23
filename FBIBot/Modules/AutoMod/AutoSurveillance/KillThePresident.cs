using Discord.Commands;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FBIBot.Modules.AutoMod.AutoSurveillance
{
    public static class KillThePresident
    {
        public static async Task<bool> IsGoingToKillThePresidentAsync(SocketCommandContext Context)
        {
            await Task.Yield();

            string message = Context.Message.Content;
            Regex regex = new Regex(@"\bI(('m|\s+am)\s+going\s+to|\s+want\s+to)(\s+\w*)*(kill|kil(?!\w+)|assassinate|murder)(\w*\s)+the\s+president\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            return regex.IsMatch(message);
        }
    }
}
