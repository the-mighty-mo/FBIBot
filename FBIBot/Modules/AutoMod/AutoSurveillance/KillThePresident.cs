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
            Regex regex = new Regex(@"I(('m|\s+am)\s+going\s+to|\s+want\s+to)(\w*\s+)+?(kill|kil(?!\w+)|assassinate|murder)\s+the(\w*\s+)+?president", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            return regex.IsMatch(message);
        }
    }
}