using Discord.Commands;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FBIBot.Modules.AutoMod.AutoSurveillance
{
    public static class Pedophile
    {
        private static readonly Regex regex = new(@"I\s+(?:like|love)\s+(?:children|kids)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static Task<bool> IsPedophileAsync(SocketCommandContext Context)
        {
            var message = Context.Message.Content;
            return Task.Run(() => regex.IsMatch(message));
        }
    }
}
