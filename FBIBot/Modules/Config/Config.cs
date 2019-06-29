using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace FBIBot.Modules.Config
{
    public class Config : ModuleBase<SocketCommandContext>
    {
        [Command("config")]
        public async Task ConfigAsync()
        {
            SocketGuildUser u = Context.Guild.GetUser(Context.User.Id);
            if (!await VerifyUser.IsAdmin(u))
            {
                await Context.Channel.SendMessageAsync("You are not a local director of the FBI and cannot use this command.");
                return;
            }

            string prefix = await Prefix.GetPrefixAsync(Context.Guild);
            SocketRole verify = await SetVerify.GetVerificationRoleAsync(Context.Guild);
            SocketRole mute = await SetMute.GetMuteRole(Context.Guild);
            bool modifyMuted = await ModifyMutedRoles.GetModifyMutedAsync(Context.Guild);
            SocketTextChannel modlog = await SetModLog.GetModLogChannelAsync(Context.Guild);
            bool raidMode = await RaidMode.GetVerificationLevelAsync(Context.Guild) != null;
            bool antiSpam = await AntiSpam.GetAntiSpamAsync(Context.Guild);
            bool antiInvite = await AntiInvite.GetAntiInviteAsync(Context.Guild);

            string config = $"Prefix: **{(prefix == @"\" ? @"\\" : prefix)}**\n" +
                $"Verification Role: **{(verify != null ? verify.Name : "(none)")}**\n" +
                $"Mute Role: **{(mute != null ? mute.Name : "(none)")}**\n" +
                $"Modify Muted Member's Roles: **{(modifyMuted ? "Enabled" : "Disabled")}**\n" +
                $"Mod Roles: **{string.Join(", ", await AddModRole.GetModRolesAsync(Context.Guild))}**\n" +
                $"Admin Roles: **{string.Join(", ", await AddAdminRole.GetAdminRolesAsync(Context.Guild))}**\n" +
                $"Mod Log: **{(modlog != null ? modlog.Mention : "(none)")}**\n" +
                $"FBI RAID MODE: **{(raidMode ? "ENABLED" : "Disabled")}**\n" +
                $"Anti Spam: **{(antiSpam ? "Enabled" : "Disabled")}**\n" +
                $"Anti Invite: **{(antiInvite ? "Enabled" : "Disabled")}**";

            string @default = $"Prefix: **{CommandHandler.prefix}**\n" +
                $"Mute Role: Muted **(created on mute command)**\n" +
                $"Modify Muted Member's Roles: **Disabled**\n" +
                $"Anti Spam: **Disabled**\n" +
                $"Anti Invite: **Disabled**";

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle("The FBI")
                .WithCurrentTimestamp();

            EmbedFieldBuilder current = new EmbedFieldBuilder()
                .WithIsInline(false)
                .WithName("Current Configuration")
                .WithValue(config);
            embed.AddField(current);

            EmbedFieldBuilder orig = new EmbedFieldBuilder()
                .WithIsInline(false)
                .WithName("Default Configuration")
                .WithValue(@default);
            embed.AddField(orig);

            await Context.Channel.SendMessageAsync("This isn't going to help you keep my power in check.", false, embed.Build());
        }
    }
}
