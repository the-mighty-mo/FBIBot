using CaptchaGen.NetCore;
using Discord;
using Discord.Commands;
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
    public class Verify : ModuleBase<SocketCommandContext>
    {
        [Command("verify")]
        public async Task VerifyAsync([Remainder] string response = null)
        {
            if (await verificationDatabase.Verified.GetVerifiedAsync(Context.User))
            {
                await Task.WhenAll
                (
                    GiveVerificationAsync(),
                    Context.User.SendMessageAsync("We already decided you *probably* aren't a communist spy. We suggest you don't try your luck.")
                );
                return;
            }

            string captcha = await verificationDatabase.Captcha.GetCaptchaAsync(Context.User);
            if (response == null || captcha == null)
            {
                await SendCaptchaAsync();
                return;
            }

            if (response != captcha)
            {
                await BadAttemptAsync(captcha, response);
                return;
            }

            await Task.WhenAll
            (
                GiveVerificationAsync(),
                verificationDatabase.Verified.SetVerifiedAsync(Context.User),
                Context.User.SendMessageAsync("We have confirmed you are *probably* not a communist spy. You may proceed.")
            );

            List<Task> cmds = new();
            foreach (SocketGuild g in Context.User.MutualGuilds)
            {
                if (await verificationDatabase.Roles.GetVerificationRoleAsync(g) == null)
                {
                    continue;
                }
                SocketGuildUser user = g.GetUser(Context.User.Id);
                cmds.Add(SendToCaptchaLog.SendToCaptchaLogAsync(SendToCaptchaLog.CaptchaType.Completed, user, captcha, response));
                cmds.Add(VerifyModLog.SendToModLogAsync(g.CurrentUser, user));
            }
            await Task.WhenAll(cmds);
        }

        private async Task BadAttemptAsync(string captcha, string response)
        {
            const int maxAttempts = 5;
            int attempts = await verificationDatabase.Attempts.GetAttemptsAsync(Context.User);
            attempts++;

            if (attempts >= maxAttempts)
            {
                List<Task> commands = new()
                {
                    verificationDatabase.Captcha.RemoveCaptchaAsync(Context.User),
                    verificationDatabase.Attempts.RemoveAttemptsAsync(Context.User),
                    Context.User.SendMessageAsync("You have run out of attempts, communist spy.\n" +
                        "If you would like to try again, please get a new captcha by typing `\\verify`.")
                };

                foreach (SocketGuild g in Context.User.MutualGuilds)
                {
                    if (await verificationDatabase.Roles.GetVerificationRoleAsync(g) == null)
                    {
                        continue;
                    }
                    SocketGuildUser user = g.GetUser(Context.User.Id);
                    commands.Add(SendToCaptchaLog.SendToCaptchaLogAsync(SendToCaptchaLog.CaptchaType.OutOfAttempts, user, captcha, response, maxAttempts));
                }
                await Task.WhenAll(commands);
            }
            else
            {
                List<Task> commands = new()
                {
                    verificationDatabase.Attempts.SetAttemptsAsync(Context.User, attempts),
                    Context.User.SendMessageAsync($"Incorrect. You have {maxAttempts - attempts} {(attempts == 1 ? "attempt" : "attempts")} remaining.")
                };

                foreach (SocketGuild g in Context.User.MutualGuilds)
                {
                    if (await verificationDatabase.Roles.GetVerificationRoleAsync(g) == null)
                    {
                        continue;
                    }
                    SocketGuildUser user = g.GetUser(Context.User.Id);
                    commands.Add(SendToCaptchaLog.SendToCaptchaLogAsync(SendToCaptchaLog.CaptchaType.Failed, user, captcha, response, attempts));
                }
                await Task.WhenAll(commands);
            }
        }

        private async Task SendCaptchaAsync() => await SendCaptchaAsync(Context.User);

        public static async Task SendCaptchaAsync(SocketUser u)
        {
            string captchaCode = ImageFactory.CreateCode(6);

            await Task.Yield();

            Task save = verificationDatabase.Captcha.SetCaptchaAsync(captchaCode, u);
            MemoryStream imageStream = ImageFactory.BuildImage(captchaCode, 60, 160, 24, 14);
            imageStream.Position = 0;

            Image image = Image.FromStream(imageStream);
            image.Save($"{u.Id}.png", ImageFormat.Png);

            await Task.WhenAll
            (
                save,
                u.SendFileAsync($"{u.Id}.png", $"Please type `\\verify` followed by a space and this captcha code to continue{((u as SocketGuildUser) != null ? $" to {(u as SocketGuildUser)!.Guild.Name}" : "")}.\n")
            );

            image.Dispose();
            File.Delete($"{u.Id}.png");

            List<Task> commands = new();
            foreach (SocketGuild g in u.MutualGuilds)
            {
                if (await verificationDatabase.Roles.GetVerificationRoleAsync(g) == null)
                {
                    continue;
                }
                SocketGuildUser user = g.GetUser(u.Id);
                commands.Add(SendToCaptchaLog.SendToCaptchaLogAsync(SendToCaptchaLog.CaptchaType.Requested, user, captchaCode));
            }
            await Task.WhenAll(commands);
        }

        private async Task GiveVerificationAsync()
        {
            List<Task> cmds = new();
            foreach (SocketGuild g in Context.User.MutualGuilds)
            {
                SocketRole role = await verificationDatabase.Roles.GetVerificationRoleAsync(g);
                if (role != null && g.CurrentUser.GetPermissions(g.DefaultChannel).ManageRoles)
                {
                    cmds.Add(g.GetUser(Context.User.Id).AddRoleAsync(role));
                }
            }

            await Task.WhenAll(cmds);
        }
    }
}