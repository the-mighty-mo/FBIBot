using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;

namespace FBIBot.Modules.AutoMod.AutoSurveillance
{
    public static partial class Pedophile
    {
        private static readonly Regex regex = PedoRegex();

        public static Task<bool> IsPedophileAsync(SocketCommandContext Context)
        {
            var message = Context.Message.Content;
            return Task.Run(() => regex.IsMatch(message));
        }

        [GeneratedRegex(@"I\s+(?:like|love)\s+(?:children|kids)", RegexOptions.IgnoreCase)]
        private static partial Regex PedoRegex();
    }
}
