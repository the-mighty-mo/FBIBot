using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Config
{
    public class SetMute : ModuleBase<SocketCommandContext>
    {
        [Command("setmute")]
        [Alias("set-mute")]
        [RequireAdmin]
        public async Task SetMuteAsync()
        {
            if (modRolesDatabase.Muted.GetMuteRole(Context.Guild) == null)
            {
                await Context.Channel.SendMessageAsync("Our intelligence team has informed us that you already lack a muted role.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription("You no longer have a muted role.\n" +
                    "~~Guess we'll take things into our own hands~~");

            await Task.WhenAll
            (
                modRolesDatabase.Muted.RemoveMuteRoleAsync(Context.Guild),
                Context.Channel.SendMessageAsync(embed: embed.Build())
            );
        }

        [Command("setmute")]
        [Alias("set-mute")]
        [RequireAdmin]
        public async Task SetMuteAsync(SocketRole role)
        {
            if (await modRolesDatabase.Muted.GetMuteRole(Context.Guild) == role)
            {
                await Context.Channel.SendMessageAsync($"All who commit treason already receive the {role.Name} role.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("Federal Bureau of Investigation")
                .WithDescription($"All who commit treason will now receive the {role.Name} role.");

            await Task.WhenAll
            (
                modRolesDatabase.Muted.SetMuteRoleAsync(role, Context.Guild),
                Context.Channel.SendMessageAsync(embed: embed.Build())
            );
        }

        [Command("setmute")]
        [Alias("set-mute")]
        [RequireAdmin]
        public async Task SetMuteAsync(string role)
        {
            SocketRole r;
            if (ulong.TryParse(role, out ulong roleID) && (r = Context.Guild.GetRole(roleID)) != null)
            {
                await SetMuteAsync(r);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given role does not exist.");
        }
    }
}
