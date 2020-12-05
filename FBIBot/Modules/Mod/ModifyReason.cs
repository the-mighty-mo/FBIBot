using Discord;
using Discord.Commands;
using FBIBot.Modules.Mod.ModLog;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Mod
{
    public class ModifyReason : ModuleBase<SocketCommandContext>
    {
        [Command("modifyreason")]
        [Alias("modify-reason")]
        [RequireMod]
        [RequireModLog]
        public async Task ModifyReasonAsync(string id, [Remainder] string reason = null)
        {
            if (!ulong.TryParse(id, out ulong ID) || ID >= await modLogsDatabase.ModLogs.GetNextModLogID(Context.Guild))
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that {id} is not a valid Mod Log ID.");
                return;
            }

            if (!await ModLogManager.SetReasonAsync(new ModLogManager.ReasonInfo(Context.Guild, ID, reason)))
            {
                await Context.Channel.SendMessageAsync("Our security team has informed us that the given Mod Log is not permitted to have a valid reason. Don't ask.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithDescription("The mod log's reason has been updated. Probably.");

            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }
    }
}