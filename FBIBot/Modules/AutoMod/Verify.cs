using CaptchaGen.NetCore;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FBIBot.Modules.Mod.ModLog;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;
using Image = System.Drawing.Image;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace FBIBot.Modules.AutoMod
{
    public class Verify : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("verify", "Verify that you are not a spy from the CCP")]
        public async Task VerifyAsync(string? response = null)
        {
            if (await verificationDatabase.Verified.GetVerifiedAsync(Context.User).ConfigureAwait(false))
            {
                await Task.WhenAll
                (
                    GiveVerificationAsync(),
                    Context.Interaction.RespondAsync("We already decided you *probably* aren't a communist spy. We suggest you don't try your luck.", ephemeral: true)
                ).ConfigureAwait(false);
                return;
            }

            string? captcha = await verificationDatabase.Captcha.GetCaptchaAsync(Context.User).ConfigureAwait(false);
            if (response == null || captcha == null)
            {
                await Task.WhenAll(
                    SendCaptchaAsync(),
                    Context.Interaction.RespondAsync("Check your DMs", ephemeral: true)
                ).ConfigureAwait(false);
                return;
            }
            else if (response != captcha)
            {
                await BadAttemptAsync(captcha, response).ConfigureAwait(false);
                return;
            }

            await Task.WhenAll
            (
                GiveVerificationAsync(),
                verificationDatabase.Verified.SetVerifiedAsync(Context.User),
                Context.Interaction.RespondAsync("We have confirmed you are *probably* not a communist spy. You may proceed.", ephemeral: true)
            ).ConfigureAwait(false);

            List<Task> cmds = new();
            foreach (SocketGuild g in Context.User.MutualGuilds)
            {
                if (await verificationDatabase.Roles.GetVerificationRoleAsync(g).ConfigureAwait(false) == null)
                {
                    continue;
                }
                SocketGuildUser user = g.GetUser(Context.User.Id);
                cmds.Add(SendToCaptchaLog.SendToCaptchaLogAsync(SendToCaptchaLog.CaptchaType.Completed, user, captcha, response));
                cmds.Add(VerifyModLog.SendToModLogAsync(g.CurrentUser, user));
            }
            await Task.WhenAll(cmds).ConfigureAwait(false);
        }

        private async Task BadAttemptAsync(string captcha, string response)
        {
            const int maxAttempts = 5;
            int attempts = await verificationDatabase.Attempts.GetAttemptsAsync(Context.User).ConfigureAwait(false);
            ++attempts;

            if (attempts >= maxAttempts)
            {
                List<Task> commands = new()
                {
                    verificationDatabase.Captcha.RemoveCaptchaAsync(Context.User),
                    verificationDatabase.Attempts.RemoveAttemptsAsync(Context.User),
                    Context.Interaction.RespondAsync("You have run out of attempts, communist spy.\n" +
                        "If you would like to try again, please get a new captcha by typing `/verify`.", ephemeral: true)
                };

                foreach (SocketGuild g in Context.User.MutualGuilds)
                {
                    if (await verificationDatabase.Roles.GetVerificationRoleAsync(g).ConfigureAwait(false) == null)
                    {
                        continue;
                    }
                    SocketGuildUser user = g.GetUser(Context.User.Id);
                    commands.Add(SendToCaptchaLog.SendToCaptchaLogAsync(SendToCaptchaLog.CaptchaType.OutOfAttempts, user, captcha, response, maxAttempts));
                }
                await Task.WhenAll(commands).ConfigureAwait(false);
            }
            else
            {
                List<Task> commands = new()
                {
                    verificationDatabase.Attempts.SetAttemptsAsync(Context.User, attempts),
                    Context.Interaction.RespondAsync($"Incorrect. You have {maxAttempts - attempts} {(attempts == 1 ? "attempt" : "attempts")} remaining.", ephemeral: true)
                };

                foreach (SocketGuild g in Context.User.MutualGuilds)
                {
                    if (await verificationDatabase.Roles.GetVerificationRoleAsync(g).ConfigureAwait(false) == null)
                    {
                        continue;
                    }
                    SocketGuildUser user = g.GetUser(Context.User.Id);
                    commands.Add(SendToCaptchaLog.SendToCaptchaLogAsync(SendToCaptchaLog.CaptchaType.Failed, user, captcha, response, attempts));
                }
                await Task.WhenAll(commands).ConfigureAwait(false);
            }
        }

        private Task SendCaptchaAsync() => SendCaptchaAsync(Context.User, Context.Interaction);

        public static async Task SendCaptchaAsync(SocketUser u, SocketInteraction? interaction = null)
        {
            string captchaCode = ImageFactory.CreateCode(6);

            Task save = verificationDatabase.Captcha.SetCaptchaAsync(captchaCode, u);
#pragma warning disable CA1416 // Validate platform compatibility
            using (Image image = await Task.Run(() =>
                {
                    MemoryStream imageStream = ImageFactory.BuildImage(captchaCode, 60, 160, 24, 14);
                    imageStream.Position = 0;

                    var image = Image.FromStream(imageStream);
                    image.Save($"{u.Id}.png", ImageFormat.Png);

                    return image;
                }).ConfigureAwait(false))
#pragma warning restore CA1416 // Validate platform compatibility
            {
                if (interaction != null)
                {
                    await Task.WhenAll
                    (
                        save,
                        interaction.RespondWithFileAsync($"{u.Id}.png", text: $"Please type `/verify` with this captcha code to continue{((u as SocketGuildUser) != null ? $" to {(u as SocketGuildUser)!.Guild.Name}" : "")}.\n", ephemeral: true)
                    ).ConfigureAwait(false);
                }
                else
                {
                    await Task.WhenAll
                    (
                        save,
                        u.SendFileAsync($"{u.Id}.png", $"Please type `/verify` with this captcha code to continue{((u as SocketGuildUser) != null ? $" to {(u as SocketGuildUser)!.Guild.Name}" : "")}.\n")
                    ).ConfigureAwait(false);
                }
            }

            File.Delete($"{u.Id}.png");

            List<Task> commands = new();
            foreach (SocketGuild g in u.MutualGuilds)
            {
                if (await verificationDatabase.Roles.GetVerificationRoleAsync(g).ConfigureAwait(false) == null)
                {
                    continue;
                }
                SocketGuildUser user = g.GetUser(u.Id);
                commands.Add(SendToCaptchaLog.SendToCaptchaLogAsync(SendToCaptchaLog.CaptchaType.Requested, user, captchaCode));
            }
            await Task.WhenAll(commands).ConfigureAwait(false);
        }

        private async Task GiveVerificationAsync()
        {
            List<Task> cmds = new();
            foreach (SocketGuild g in Context.User.MutualGuilds)
            {
                SocketRole? role = await verificationDatabase.Roles.GetVerificationRoleAsync(g).ConfigureAwait(false);
                if (role != null && g.CurrentUser.GetPermissions(g.DefaultChannel).ManageRoles)
                {
                    cmds.Add(g.GetUser(Context.User.Id).AddRoleAsync(role));
                }
            }

            await Task.WhenAll(cmds).ConfigureAwait(false);
        }
    }
}