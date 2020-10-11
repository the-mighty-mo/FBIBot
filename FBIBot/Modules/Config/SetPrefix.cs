using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Config
{
    public class SetPrefix : ModuleBase<SocketCommandContext>
    {
        [Command("setprefix")]
        [Alias("set-prefix")]
        [RequireAdmin]
        public async Task PrefixAsync(string prefix = CommandHandler.prefix)
        {
            if (await configDatabase.Prefixes.GetPrefixAsync(Context.Guild) == prefix)
            {
                await Context.Channel.SendMessageAsync($"The FBI's prefix is already `{prefix}`.");
                return;
            }

            if (prefix == "/")
            {
                await Context.Channel.SendMessageAsync($"The prefix `/` is not permitted due to Discord's commands using the prefix.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"The FBI's prefix has been set to `{prefix}`.");

            await Task.WhenAll
            (
                configDatabase.Prefixes.SetPrefixAsync(Context.Guild, prefix),
                Context.Channel.SendMessageAsync("", false, embed.Build())
            );
        }
    }
}
