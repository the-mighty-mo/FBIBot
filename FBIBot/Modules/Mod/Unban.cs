using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FBIBot.Modules.Mod.ModLog;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class Unban : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("unban", "Permits the now-ex-KGB spy to reenter the server")]
        [RequireMod]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public Task UnbanAsync(SocketUser user) =>
            UnbanAsync(user as SocketGuildUser);

        private async Task UnbanAsync(SocketGuildUser user)
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(12, 156, 24))
                .WithDescription($"{user.Mention}, the now-ex-KGB spy, may reenter the nation.\n" +
                    $"They better not let their guard down.");

            await Task.WhenAll
            (
                Context.Guild.RemoveBanAsync(user),
                Context.Interaction.RespondAsync(embed: embed.Build()),
                UnbanModLog.SendToModLogAsync(Context.User as SocketGuildUser, user)
            );
        }
    }
}