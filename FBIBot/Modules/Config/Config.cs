using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Modules.Config
{
    public class Config : ModuleBase<SocketCommandContext>
    {
        [Command("config")]
        [RequireAdmin]
        public async Task ConfigAsync()
        {
            Task<string> prefix = SetPrefix.GetPrefixAsync(Context.Guild);
            Task<SocketRole> verify = SetVerify.GetVerificationRoleAsync(Context.Guild);
            Task<SocketRole> mute = SetMute.GetMuteRole(Context.Guild);
            Task<bool> modifyMuted = ModifyMutedRoles.GetModifyMutedAsync(Context.Guild);
            Task<List<SocketRole>> modRoles = AddMod.GetModRolesAsync(Context.Guild);
            Task<List<SocketRole>> adminRoles = AddAdmin.GetAdminRolesAsync(Context.Guild);
            Task<SocketTextChannel> modlog = SetModLog.GetModLogChannelAsync(Context.Guild);
            Task<SocketTextChannel> captchalog = SetCaptchaLog.GetCaptchaLogChannelAsync(Context.Guild);
            Task<VerificationLevel?> raidMode = RaidMode.GetVerificationLevelAsync(Context.Guild);
            Task<bool> autoSurveillance = AutoSurveillance.GetAutoSurveillanceAsync(Context.Guild);
            Task<bool> antiZalgo = AntiZalgo.GetAntiZalgoAsync(Context.Guild);
            Task<bool> antiSpam = AntiSpam.GetAntiSpamAsync(Context.Guild);
            Task<bool> antiSingleSpam = AntiSingleSpam.GetAntiSingleSpamAsync(Context.Guild);
            Task<bool> antiMassMention = AntiMassMention.GetAntiMassMentionAsync(Context.Guild);
            Task<bool> antiCaps = AntiCaps.GetAntiCapsAsync(Context.Guild);
            Task<bool> antiInvite = AntiInvite.GetAntiInviteAsync(Context.Guild);
            Task<bool> antiLink = AntiLink.GetAntiLinkAsync(Context.Guild);

            string config = $"Prefix: **{(await prefix == @"\" ? @"\\" : await prefix)}**\n" +
                $"Verification Role: **{(await verify != null ? (await verify).Name : "(none)")}**\n" +
                $"Mute Role: **{(await mute != null ? (await mute).Name : "(none)")}**\n" +
                $"Modify Muted Member's Roles: **{(await modifyMuted ? "Enabled" : "Disabled")}**\n" +
                $"Mod Roles: **{string.Join(", ", await modRoles)}**\n" +
                $"Admin Roles: **{string.Join(", ", await adminRoles)}**\n" +
                $"Mod Log: **{(await modlog != null ? (await modlog).Mention : "(none)")}**\n" +
                $"CAPTCHA Log: **{(await captchalog != null ? (await captchalog).Mention : "(none)")}**\n" +
                $"FBI RAID MODE: **{(await raidMode != null ? "ENABLED" : "Disabled")}**\n" +
                $"\n" +
                $"__AutoMod:__\n" +
                $"Auto Surveillance: **{(await autoSurveillance ? "Enabled" : "Disabled")}**\n" +
                $"Anti-Zalgo: **{(await antiZalgo ? "Enabled" : "Disabled")}**\n" +
                $"Anti-Spam: **{(await antiSpam ? "Enabled" : "Disabled")}**\n" +
                $"Anti-Single-Spam: **{(await antiSingleSpam ? "Enabled" : "Disabled")}**\n" +
                $"Anti-Mass-Mention: **{(await antiMassMention ? "Enabled" : "Disabled")}**\n" +
                $"Anti-CAPS: **{(await antiCaps ? "Enabled" : "Disabled")}**\n" +
                $"Anti-Invite: **{(await antiInvite ? "Enabled" : "Disabled")}**\n" +
                $"Anti-Link: **{(await antiLink ? "Enabled" : "Disabled")}**\n";

            string @default = $"Prefix: **{(CommandHandler.prefix == @"\" ? @"\\" : CommandHandler.prefix)}**\n" +
                $"Mute Role: Muted **(created on mute command)**\n" +
                $"Modify Muted Member's Roles: **Disabled**\n" +
                $"__AutoMod:__ **Disabled**";

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
