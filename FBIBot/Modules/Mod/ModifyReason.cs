using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class ModifyReason : ModuleBase<SocketCommandContext>
    {
        [Command("modifyreason")]
        public async Task ModifyReasonAsync(string id, [Remainder] string reason = null)
        {
            SocketGuildUser u = Context.Guild.GetUser(Context.User.Id);
            if (!await VerifyUser.IsMod(u))
            {
                await Context.Channel.SendMessageAsync("You are not a local director of the FBI and cannot use this command.");
                return;
            }

            if (!ulong.TryParse(id, out ulong ID))
            {
                await Context.Channel.SendMessageAsync($"Unfortunately, {id} is not a valid ID.");
                return;
            }

            if (!await SendToModLog.SetReasonAsync(Context.Guild, ID, reason))
            {
                await Context.Channel.SendMessageAsync("Unfortunately, that mod log is not permitted to have a valid reason. Don't ask.");
                return;
            }

            await Context.Channel.SendMessageAsync("The mod log's reason has been updated. Probably.");
        }
    }
}
