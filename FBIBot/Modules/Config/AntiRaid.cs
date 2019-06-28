using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FBIBot.Modules.Config
{
    public class AntiRaid : ModuleBase<SocketCommandContext>
    {
        [Command("antiraid")]
        [RequireBotPermission(GuildPermission.ManageGuild)]
        public async Task AntiRaidAsync(string enabled = "false")
        {
            SocketGuildUser u = Context.Guild.GetUser(Context.User.Id);
            if (!await VerifyUser.IsAdmin(u))
            {
                await Context.Channel.SendMessageAsync("You are not a local director of the FBI and cannot use this command.");
                return;
            }

            bool isEnabled = enabled.ToLower() == "true" || enabled.ToLower() == "enable";

            if (isEnabled)
            {
                await Context.Guild.ModifyAsync(x => x.VerificationLevel = VerificationLevel.High);
                await Context.Channel.SendMessageAsync(":rotating_light: :rotating_light: RAID MODE ACTIVE! GET DOWN! :rotating_light: :rotating_light:");
            }
        }

        public static async Task GetVerificationLevelAsync(SocketGuild g)
        {

        }

        public static async Task<VerificationLevel> SaveVerificationLevelAsync(SocketGuild g)
        {
            VerificationLevel level = (VerificationLevel)3;
            return await Task.Run(() => level);
        }
    }
}
