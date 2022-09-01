using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Config
{
    public class Config : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("config", "Displays the current bot configuration")]
        [RequireAdmin]
        public async Task ConfigAsync()
        {
            Task<SocketRole> verify = verificationDatabase.Roles.GetVerificationRoleAsync(Context.Guild);
            Task<SocketRole> mute = modRolesDatabase.Muted.GetMuteRole(Context.Guild);
            Task<bool> modifyMuted = configDatabase.ModifyMuted.GetModifyMutedAsync(Context.Guild);
            Task<List<SocketRole>> modRoles = modRolesDatabase.Mods.GetModRolesAsync(Context.Guild);
            Task<List<SocketRole>> adminRoles = modRolesDatabase.Admins.GetAdminRolesAsync(Context.Guild);
            Task<SocketTextChannel> modlog = modLogsDatabase.ModLogChannel.GetModLogChannelAsync(Context.Guild);
            Task<SocketTextChannel> captchalog = modLogsDatabase.CaptchaLogChannel.GetCaptchaLogChannelAsync(Context.Guild);
            Task<SocketTextChannel> welcome = modLogsDatabase.WelcomeChannel.GetWelcomeChannelAsync(Context.Guild);
            Task<VerificationLevel?> raidMode = raidModeDatabase.RaidMode.GetVerificationLevelAsync(Context.Guild);
            Task<bool> autoSurveillance = configDatabase.AutoSurveillance.GetAutoSurveillanceAsync(Context.Guild);
            Task<bool> antiZalgo = configDatabase.AntiZalgo.GetAntiZalgoAsync(Context.Guild);
            Task<bool> antiSpam = configDatabase.AntiSpam.GetAntiSpamAsync(Context.Guild);
            Task<bool> antiSingleSpam = configDatabase.AntiSingleSpam.GetAntiSingleSpamAsync(Context.Guild);
            Task<bool> antiMassMention = configDatabase.AntiMassMention.GetAntiMassMentionAsync(Context.Guild);
            Task<bool> antiCaps = configDatabase.AntiCaps.GetAntiCapsAsync(Context.Guild);
            Task<bool> antiInvite = configDatabase.AntiInvite.GetAntiInviteAsync(Context.Guild);
            Task<bool> antiLink = configDatabase.AntiLink.GetAntiLinkAsync(Context.Guild);

            string config =
                $"Verification Role: **{(await verify != null ? (await verify).Mention : "(none)")}**\n" +
                $"Mute Role: **{(await mute != null ? (await mute).Mention : "(none)")}**\n" +
                $"Modify Muted Member's Roles: **{(await modifyMuted ? "Enabled" : "Disabled")}**\n" +
                $"Mod Roles: **{string.Join(", ", (await modRoles).Select(x => x.Mention))}**\n" +
                $"Admin Roles: **{string.Join(", ", (await adminRoles).Select(x => x.Mention))}**\n" +
                $"Mod Log: **{(await modlog != null ? (await modlog).Mention : "(none)")}**\n" +
                $"CAPTCHA Log: **{(await captchalog != null ? (await captchalog).Mention : "(none)")}**\n" +
                $"Welcome Channel: **{(await welcome != null ? (await welcome).Mention : "(none)")}**\n" +
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

            string @default =
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

            await Context.Interaction.RespondAsync("This isn't going to help you keep my power in check.", embed: embed.Build());
        }
    }
}