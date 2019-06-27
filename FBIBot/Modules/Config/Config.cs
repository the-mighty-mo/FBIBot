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

            string config = $"Prefix: **{(prefix == @"\" ? @"\\" : prefix)}**\n" +
                $"Verification Role: **{(verify == null ? "(none)" : verify.Name)}**\n" +
                $"Mute Role: **{(mute == null ? "(none)" : mute.Name)}**\n" +
                $"Modify Muted Member's Roles: **{(await ModifyMutedRoles.GetModifyMutedAsync(Context.Guild) ? "Enabled" : "Disabled")}**\n" +
                $"Mod Roles: **{string.Join(", ", await AddModRole.GetModRolesAsync(Context.Guild))}**\n" +
                $"Admin Roles: **{string.Join(", ", await AddAdminRole.GetAdminRolesAsync(Context.Guild))}**";

            string @default = $"Prefix: **{CommandHandler.prefix}**\n" +
                $"Mute Role: **(created on mute command)**\n" +
                $"Modify Muted Member's Roles: **Disabled**";

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
